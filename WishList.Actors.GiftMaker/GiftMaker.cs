using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using WishList.Core.Actors;
using WishList.Core.Models;

namespace WishList.Actors.GiftMaker
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class GiftMaker : Actor, IGiftMakerElfActor
    {
        /// <summary>
        /// Initializes a new instance of GiftMaker
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public GiftMaker(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }

        public Task<Gift> MakeWishListItemAsync(Core.Models.WishList wishList, WishListItem itemToMake)
        {
            // TODO: Make the actual gift
            var giftMade = new Gift
            {
                Id = Guid.NewGuid(),
                PersonId = wishList.PersonId,
                WishListItem = itemToMake,
                MadeBy = Id.GetStringId()
            };

            // Fire off the gift completed event
            var completedEvent = GetEvent<IGiftMakerElfActorEvents>();
            completedEvent.GiftCompleted(giftMade);

            return Task.FromResult(giftMade);
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");

            // The StateManager is this actor's private state store.

            // Data stored in the StateManager will be replicated for high-availability for actors that use volatile or persisted state storage.
            // Any serializable object can be saved in the StateManager.
            // For more information, see https://aka.ms/servicefabricactorsstateserialization
            return Task.FromResult(true);
        }

    }
}
