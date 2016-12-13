using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using WishList.Core.Extensions;
using WishList.Core.Services;
using Microsoft.ServiceFabric.Services.Remoting.Runtime;
using WishList.Core.Actors;

namespace WishList.ReviewMemberService
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    internal sealed class ReviewMemberService : StatelessService, IWishListReviewService
    {
        private List<string> _reviewerActorNames;
        private readonly IReviewCommitteeActorFactory _actorFactory;
        private readonly IGiftProcessingServiceFactory _processingFactory;

        public ReviewMemberService(StatelessServiceContext context)
            : base(context)
        {
            _actorFactory = new ReviewCommitteeActorFactory();
            _processingFactory = new GiftProcessingServiceFactory();
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            yield return new ServiceInstanceListener(context => this.CreateServiceRemotingListener(context), "ReviewMemberServiceListener");
        }

        public Task<bool> ReviewWishListAsync(Core.Models.WishList wishList)
        {
            // Check if wish list has been reviewed by everyone
            var nextReviewer = _reviewerActorNames.Where(reviewer => !(wishList.Approvals?.Any(wla => wla.Approver == reviewer) ?? false)).FirstOrDefault();

            if (String.IsNullOrWhiteSpace(nextReviewer))
            {
                // List is fully reviewed, send it on to fulfillment
                var processor = _processingFactory.Create();
                return processor.ProcessWishListAsync(wishList);
            }

            try
            {
                // Fire this off, don't wait (if we can get the actor, we'll assume it all works)
                var actor = _actorFactory.Create(nextReviewer);

                Task.Run(() =>
                {
                    // Send to selected reviewer actor
                    actor.ReviewWishListAsync(wishList);
                });

                return Task.FromResult(true);
            }
            catch(Exception ex)
            {
                ServiceEventSource.Current.ServiceMessage(Context, "Exception Occurred Reviewing Wish List: {0}", ex.Message);
                return Task.FromResult(false);
            }
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            var configuredReviewers = Context.GetConfigurationValue("ReviewerNames");
            _reviewerActorNames = configuredReviewers.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return base.RunAsync(cancellationToken);
        }
    }
}
