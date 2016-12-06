using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using WishList.WebAPI.Models.WishList;

namespace WishList.WebAPI.Controllers
{
    [ServiceRequestActionFilter]
    public class WishListController : ApiController
    {
        public string Get(string id)
        {
            return "TBD";
        }

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody]WishListApiModel wishList)
        {
            var routeDictionary = new Dictionary<string, object>(ControllerContext.RouteData.Values) { { "id", Guid.NewGuid().ToString("n") } };

            return CreatedAtRoute("DefaultApi", routeDictionary, new { message = "Wish List Accepted" });
        }
    }
}
