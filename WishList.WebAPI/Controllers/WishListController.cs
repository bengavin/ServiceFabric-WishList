using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using WishList.WebAPI.Models.WishList;
using CoreModels = WishList.Core.Models;
using WishList.Core.Services;

namespace WishList.WebAPI.Controllers
{
    [ServiceRequestActionFilter]
    public class WishListController : ApiController
    {
        private readonly IMapper _mapper;
        private readonly IPersonServiceFactory _personServiceFactory;

        public WishListController()
        {
            _mapper = AutoMapper.Mapper.Instance;
            _personServiceFactory = new PersonServiceFactory();
        }

        public string Get(string id)
        {
            return "TBD";
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
                }

                // Update wish list with person data
                incomingWishList.ActualBehaviorRating = person.BehaviorRating;
                incomingWishList.EmailAddress = person.EmailAddress;
                incomingWishList.FamilyName = person.FamilyName;
                incomingWishList.GivenName = person.GivenName;
                incomingWishList.PersonId = person.Id;
                incomingWishList.TwitterHandle = person.TwitterHandle;
            }

            // TODO: Figure out how to easily publish messages to services/actors from here...
            //await ServiceFabric.PubSubActors.PublisherServices.PublisherServiceExtensions.PublishMessageAsync()
            //await this.PublishMessageToBrokerServiceAsync(incomingWishList);

            var routeDictionary = new Dictionary<string, object>(ControllerContext.RouteData.Values) { { "id", incomingWishList.PersonId } };

            return CreatedAtRoute("DefaultApi", routeDictionary, new { message = "Wish List Accepted" });
        }
    }
}
