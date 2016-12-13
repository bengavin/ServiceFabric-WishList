using Microsoft.ServiceFabric.Actors;

namespace WishList.Core.Actors
{
    public interface IGiftMakerElfActorEvents : IActorEvents
    {
        void GiftCompleted(Models.Gift gift);
    }
}
