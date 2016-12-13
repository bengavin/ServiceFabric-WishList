using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace WishList.Core.Actors
{
    public interface IReviewCommitteeActor : IActor
    {
        Task ReviewWishListAsync(Models.WishList wishListToReview);
    }

    public interface IReviewCommitteeActorFactory
    {
        IReviewCommitteeActor Create(string actorId);
    }

    public class ReviewCommitteeActorFactory : IReviewCommitteeActorFactory
    {
        public static readonly Uri FabricUri = new Uri("fabric:/WishList.ServiceFabric/ReviewCommitteeActorService");

        public IReviewCommitteeActor Create(string actorId)
        {
            return ActorProxy.Create<IReviewCommitteeActor>(new ActorId(actorId), FabricUri);
        }
    }
}
