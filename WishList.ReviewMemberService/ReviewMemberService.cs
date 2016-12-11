using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using ServiceFabric.PubSubActors.Interfaces;
using ServiceFabric.PubSubActors.SubscriberServices;
using ServiceFabric.PubSubActors.Helpers;
using WishList.Core.Extensions;

namespace WishList.ReviewMemberService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ReviewMemberService : StatelessService, ISubscriberService
    {
        private SubscriberServiceHelper _helper;

        public ReviewMemberService(StatelessServiceContext context)
            : base(context)
        { }

        public async Task ReceiveMessageAsync(MessageWrapper message)
        {
            var wishList = this.Deserialize<Core.Models.WishList>(message);
            var reviewerName = Context.ServiceName.Segments.Last().Split(':').Last();
            ServiceEventSource.Current.ServiceMessage(Context, $"Review Member {reviewerName} - Received Wish List for {wishList.EmailAddress}");
        }

        public async Task RegisterAsync()
        {
            ////register with BrokerActor:    
            //await this.RegisterMessageTypeAsync(typeof(Core.Models.WishList));

            //register with BrokerService:
            _helper = new SubscriberServiceHelper();
            await _helper.RegisterMessageTypeAsync(this, typeof(Core.Models.WishList));
        }

        public async Task UnregisterAsync()
        {
            ////unregister with BrokerActor:    
            //await this.UnregisterMessageTypeAsync(typeof(Core.Models.WishList), true);

            //unregister with BrokerService:
            await _helper.UnregisterMessageTypeAsync(this, typeof(Core.Models.WishList), true);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(context => new SubscriberCommunicationListener(this, context), "ReviewMemberServiceSubscriberListener");
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await RegisterAsync();
            await base.OnOpenAsync(cancellationToken);
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            await UnregisterAsync();
            await base.OnCloseAsync(cancellationToken);
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            // TODO: Replace the following sample code with your own logic 
            //       or remove this RunAsync override if it's not needed in your service.

            long iterations = 0;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ServiceEventSource.Current.ServiceMessage(this.Context, "Working-{0}", ++iterations);

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
