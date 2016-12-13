using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using WishList.Core.Actors;
using WishList.Core.Models;
using WishList.Core.Services;

namespace WishList.Actors.ReviewCommittee
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
    internal class ReviewCommittee : Actor, IReviewCommitteeActor
    {
        private const string ReviewedPersonListKey = "ReviewedPersonList";
        private const string ReviewerAlgorithmKey = "ReviewerAlgorithm";
        private static readonly Random _randomizer = new Random();

        private enum ReviewerAlgorithm
        {
            Happy,
            Grumpy,
            Honest
        }

        private readonly IWishListReviewServiceFactory _reviewServiceFactory;

        /// <summary>
        /// Initializes a new instance of ReviewCommittee
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        public ReviewCommittee(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _reviewServiceFactory = new WishListReviewServiceFactory();
        }

        public async Task ReviewWishListAsync(Core.Models.WishList wishListToReview)
        {
            // make sure we don't review the same person more than once
            var algorithm = await StateManager.GetStateAsync<ReviewerAlgorithm>(ReviewerAlgorithmKey);
            var personList = await StateManager.GetStateAsync<List<Guid>>(ReviewedPersonListKey);
            var reviewService = _reviewServiceFactory.Create();

            var actorId = Id.GetStringId();

            // If this actor reviewed this person already, then kick them back to the review service 
            // without approving anything (the review committee hates doing extra work)
            if (personList.Contains(wishListToReview.PersonId))
            {
                if (!(wishListToReview.Approvals?.Any(wla => wla.Approver == actorId) ?? false))
                {
                    wishListToReview.AddApproval(actorId);
                }
            }
            else
            {
                // Otherwise, let's approve some items
                // TODO: Make this fancier
                var randomItemMod = 100;
                switch (algorithm)
                {
                    case ReviewerAlgorithm.Grumpy: randomItemMod = 4; break;
                    case ReviewerAlgorithm.Happy: randomItemMod = 2; break;
                    case ReviewerAlgorithm.Honest: randomItemMod = ((int)BehaviorRating.Angelic + 1) - ((int)BehaviorRating.Angelic - (int)wishListToReview.SelfReportedBehaviorRating); break;
                }

                var approvedItems = wishListToReview.Items?.Where(i => _randomizer.Next(100) % randomItemMod == 0).Select(i => i.Id).ToArray() ?? new Guid[0];
                wishListToReview.AddApproval(actorId, approvedItems);

                personList.Add(wishListToReview.PersonId);
                await StateManager.AddOrUpdateStateAsync(ReviewedPersonListKey, personList, (name, value) => { value.Add(wishListToReview.PersonId); return value; });
            }

            if (!await reviewService.ReviewWishListAsync(wishListToReview))
            {
                ActorEventSource.Current.Message("Error Returning Wish List for Review: {0}, {1}", actorId, wishListToReview.PersonId);
            }
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

            return Task.WhenAll(
                        StateManager.TryAddStateAsync(ReviewedPersonListKey, new List<Guid>()),
                        StateManager.TryAddStateAsync(ReviewerAlgorithmKey, GetReviewerAlgorithmFromActorId()));
        }

        private ReviewerAlgorithm GetReviewerAlgorithmFromActorId()
        {
            switch(this.GetActorId().GetStringId().ToLower())
            {
                case "happy": return ReviewerAlgorithm.Happy;
                case "grumpy": return ReviewerAlgorithm.Grumpy;
                case "honest": return ReviewerAlgorithm.Honest;
                default: return ReviewerAlgorithm.Honest;
            }
        }
    }
}
