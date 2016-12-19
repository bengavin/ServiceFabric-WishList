using System.Collections.Generic;
using System.Fabric;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using WishList.Core.Services;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using WishList.Core.Models;

namespace WishList.ShippingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class ShippingService : StatefulService, IShippingService
    {
        public ShippingService(StatefulServiceContext context)
            : base(context)
        { }

        public Task<bool> ShipGiftAsync(Gift gift, Core.Models.WishList wishList)
        {
            // TODO: Put this into our dictionary, look at the person service, see if family gifts are ready, etc

            return Task.FromResult(true);
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
            return new ServiceReplicaListener[]
                {
                    new ServiceReplicaListener(context => this.CreateServiceRemotingListener(context))
                };
        }
    }
}
