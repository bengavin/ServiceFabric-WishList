using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using WishList.WebAPI.Models.WishList;
using CoreModels = WishList.Core.Models;
using WishList.Core.Services;
using Microsoft.ServiceFabric.Actors;
using ServiceFabric.PubSubActors.Interfaces;
using Microsoft.ServiceFabric.Actors.Client;
using Newtonsoft.Json;
using System.Fabric;
using System.Threading;

namespace WishList.WebAPI.Controllers
{
    [ServiceRequestActionFilter]
    public class WishListController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IPersonServiceFactory _personServiceFactory;
        private readonly Random _random;

        public WishListController()
        {
            _mapper = AutoMapper.Mapper.Instance;
            _personServiceFactory = new PersonServiceFactory();
            _random = new Random();
        }

        public IHttpActionResult Get(Guid id)
        {
            return Content(System.Net.HttpStatusCode.NotFound, new { message = "Did you really think you could see what you're getting?" });
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody]WishListApiModel wishList)
        {
            var incomingWishList = _mapper.Map<CoreModels.WishList>(wishList);
            var personService = _personServiceFactory.Create(wishList.FamilyName);

            if (personService != null)
            {
                var person = await personService.GetByEmailOrTwitterAsync(incomingWishList.EmailAddress, incomingWishList.TwitterHandle);
                if (person == null)
                {
                    // TODO: Need to create a new person entry
                    person = new Core.Models.Person
                    {
                        BehaviorRating = (Core.Models.BehaviorRating)_random.Next((int)Core.Models.BehaviorRating.Unaware, (int)Core.Models.BehaviorRating.Angelic),
                        EmailAddress = incomingWishList.EmailAddress,
                        FamilyName = incomingWishList.FamilyName,
                        GivenName = incomingWishList.GivenName,
                        TwitterHandle = incomingWishList.TwitterHandle
                    };

                    if (!await personService.AddOrUpdateAsync(person))
                    {
                        return Content(System.Net.HttpStatusCode.PreconditionFailed, new { message = "Unable to create person" });
                    }
                }

                // Update wish list with person data
                incomingWishList.ActualBehaviorRating = person.BehaviorRating;
                incomingWishList.EmailAddress = person.EmailAddress;
                incomingWishList.FamilyName = person.FamilyName;
                incomingWishList.GivenName = person.GivenName;
                incomingWishList.PersonId = person.Id;
                incomingWishList.TwitterHandle = person.TwitterHandle;
            }

            await SendBrokeredMessageAsync(incomingWishList);

            var routeDictionary = new Dictionary<string, object>(ControllerContext.RouteData.Values) { { "id", incomingWishList.PersonId } };

            return CreatedAtRoute("DefaultApi", routeDictionary, new { message = "Wish List Accepted" });
        }

        #region Ported from PubSubActors codebase to support integration with ApiController
        private async Task SendBrokeredMessageAsync(object message)
        {
            var context = await FabricRuntime.GetActivationContextAsync(TimeSpan.FromSeconds(1), CancellationToken.None);

            var applicationName = context.ApplicationName;

            var brokerActor = GetBrokerActorForMessage(applicationName, message);
            var wrapper = CreateMessageWrapper(message);
            await brokerActor.PublishMessageAsync(wrapper);
        }

        private MessageWrapper CreateMessageWrapper(object message)
        {
            var wrapper = new MessageWrapper
            {
                MessageType = message.GetType().FullName,
                Payload = JsonConvert.SerializeObject(message),
            };

            return wrapper;
        }

        private static IBrokerActor GetBrokerActorForMessage(string applicationName, object message)
        {
            ActorId actorId = new ActorId(message.GetType().FullName);
            IBrokerActor brokerActor = ActorProxy.Create<IBrokerActor>(actorId, applicationName, nameof(IBrokerActor));
            return brokerActor;
        }
        #endregion
    }
}
