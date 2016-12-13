using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using WishList.Core.Models;
using WishList.Core.Services;
using WishList.Core.Actors;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using Microsoft.ServiceFabric.Actors.Client;

namespace WishList.GiftProcessingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class GiftProcessingService : StatefulService, IGiftProcessingService, IGiftMakerElfActorEvents
    {
        private readonly IGiftMakerElfActorFactory _elfFactory;
        private static readonly string QueuesToMonitorKey = "GiftQueuesToMonitor";
        private static readonly string GiftQueueStatePrefix = "GiftMakerQueue";
        private static readonly string GiftQueueStateFormat = $"{GiftQueueStatePrefix}-{{0}}";
        
        public GiftProcessingService(StatefulServiceContext context)
            : base(context)
        {
            _elfFactory = new GiftMakerElfActorFactory();
        }

        public async Task<bool> ProcessWishListAsync(Core.Models.WishList wishListToProcess)
        {
            if (wishListToProcess == null) { return false; }

            // Gather items that are approved by at least 2 committee members
            var approvedItemCounts = wishListToProcess.Approvals.SelectMany(x => x.ApprovedItems)
                                                                .GroupBy(x => x)
                                                                .Select(x => new { Key = x.Key, Approvals = x.Count() })
                                                                .Where(x => x.Approvals >= 2)
                                                                .ToDictionary(x => x.Key, x => x.Approvals);

            foreach(var item in wishListToProcess.Items)
            {
                if (approvedItemCounts.ContainsKey(item.Id))
                {
                    using (var tx = StateManager.CreateTransaction())
                    {
                        var itemType = item.ItemType.ToString();
                        var stateKey = String.Format(GiftQueueStateFormat, itemType);
                        var queue = await StateManager.GetOrAddAsync<IReliableQueue<GiftMakerEntry>>(tx, stateKey);

                        await queue.EnqueueAsync(tx, new GiftMakerEntry
                        {
                            WishList = wishListToProcess,
                            WishListItem = item
                        });

                        await tx.CommitAsync();
                    }
                }
            }

            return true;
        }

        public void GiftCompleted(Gift gift)
        {
            ServiceEventSource.Current.Message($"Gift completed by {gift.MadeBy} for {gift.PersonId} - {gift.WishListItem.Name}");
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            yield return new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context), "GiftProcessingService");
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var queueNames = Enum.GetNames(typeof(ItemType));

            // Setup the gift maker queues
            using (var tx = this.StateManager.CreateTransaction())
            {
                foreach(var name in queueNames)
                {
                    var stateKey = String.Format(GiftQueueStateFormat, name);
                    await StateManager.GetOrAddAsync<IReliableQueue<GiftMakerEntry>>(tx, stateKey);
                }

                await tx.CommitAsync();
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var giftsMade = false;

                using (var tx = this.StateManager.CreateTransaction())
                {
                    var giftTasks = new List<Task>();
                    var actors = new List<IGiftMakerElfActor>();

                    var keyEnumerator = queueNames.Select(name => String.Format(GiftQueueStateFormat, name)).GetEnumerator();
                    while (keyEnumerator.MoveNext())
                    {
                        var giftQueue = await StateManager.TryGetAsync<IReliableQueue<GiftMakerEntry>>(keyEnumerator.Current);
                        if (giftQueue.HasValue)
                        {
                            var giftToMake = await giftQueue.Value.TryDequeueAsync(tx);
                            if (giftToMake.HasValue)
                            {
                                var actor = _elfFactory.Create(giftToMake.Value.WishListItem.ItemType.ToString());
                                actors.Add(actor);

                                giftTasks.Add(actor.MakeWishListItemAsync(giftToMake.Value.WishList, giftToMake.Value.WishListItem));

                                await actor.SubscribeAsync<IGiftMakerElfActorEvents>(this);
                            }
                        }
                    }

                    if (giftTasks.Any())
                    {
                        giftsMade = true;
                        await Task.WhenAll(giftTasks);
                    }

                    // If an exception is thrown before calling CommitAsync, the transaction aborts, all changes are 
                    // discarded, and nothing is saved to the secondary replicas.
                    await tx.CommitAsync();

                    // Untie any event handlers
                    if (actors.Any())
                    {
                        actors.ForEach(async actor => await actor.UnsubscribeAsync<IGiftMakerElfActorEvents>(this));
                    }
                }

                // Sleep longer if we didn't make anything this round
                await Task.Delay(TimeSpan.FromSeconds(giftsMade ? 1 : 5), cancellationToken);
            }
        }

        [Serializable]
        private class GiftMakerEntry
        {
            public Core.Models.WishList WishList { get; set; }
            public WishListItem WishListItem { get; set; }
        }
    }
}
