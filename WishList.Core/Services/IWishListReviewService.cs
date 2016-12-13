using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Threading.Tasks;

namespace WishList.Core.Services
{
    public interface IWishListReviewService : IService
    {
        Task<bool> ReviewWishListAsync(Models.WishList wishList);
    }

    public interface IWishListReviewServiceFactory
    {
        IWishListReviewService Create();
    }

    public class WishListReviewServiceFactory : IWishListReviewServiceFactory
    {
        public static readonly Uri FabricUri = new Uri("fabric:/WishList.ServiceFabric/ReviewMemberService");

        public IWishListReviewService Create()
        {
            return ServiceProxy.Create<IWishListReviewService>(FabricUri, ServicePartitionKey.Singleton);
        }
    }
}
