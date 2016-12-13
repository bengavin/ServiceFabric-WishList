using Microsoft.ServiceFabric.Actors;
using System.Threading.Tasks;
using WishList.Core.Models;
using System;
using Microsoft.ServiceFabric.Actors.Client;

namespace WishList.Core.Actors
{
    public interface IGiftMakerElfActor : IActor, IActorEventPublisher<IGiftMakerElfActorEvents>
    {
        Task<Gift> MakeWishListItemAsync(Models.WishList wishList, WishListItem itemToMake);
    }

    public interface IGiftMakerElfActorFactory
    {
        IGiftMakerElfActor Create(string actorId);
    }

    public class GiftMakerElfActorFactory : IGiftMakerElfActorFactory
    {
        private static readonly Uri FabricUri = new Uri("fabric:/WishList.ServiceFabric/GiftMakerElfActorService");

        public IGiftMakerElfActor Create(string actorId)
        {
            return ActorProxy.Create<IGiftMakerElfActor>(new ActorId(actorId), FabricUri);
        }
    }
}
