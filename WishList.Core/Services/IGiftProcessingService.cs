using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Threading.Tasks;

namespace WishList.Core.Services
{
    public interface IGiftProcessingService : IService
    {
        Task<bool> ProcessWishListAsync(Models.WishList withListToProcess);
    }

    public interface IGiftProcessingServiceFactory
    {
        IGiftProcessingService Create();
    }

    public class GiftProcessingServiceFactory : IGiftProcessingServiceFactory
    {
        private static readonly Uri FabricUri = new Uri("fabric:/WishList.ServiceFabric/GiftProcessingService");

        public IGiftProcessingService Create()
        {
            return ServiceProxy.Create<IGiftProcessingService>(FabricUri, new ServicePartitionKey(1));
        }
    }
}
