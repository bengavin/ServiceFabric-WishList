using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WishList.Core.Models;

namespace WishList.Core.Services
{
    public interface IShippingService : IService
    {
        Task<bool> ShipGiftAsync(Gift gift, Models.WishList wishList);
    }

    public interface IShippingServiceFactory
    {
        IShippingService Create();
    }

    public class ShippingServiceFactory : IShippingServiceFactory
    {
        private static readonly Uri FabricUri = new Uri("fabric:/WishList.ServiceFabric/ShippingService");

        public IShippingService Create()
        {
            return ServiceProxy.Create<IShippingService>(FabricUri, new ServicePartitionKey(1));
        }
    }
}
