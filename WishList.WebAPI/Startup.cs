using System.Web.Http;
using Owin;
using System.Fabric;

namespace WishList.WebAPI
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder, ServiceContext serviceContext)
        {
            // Setup AutoMapper
            AutoMapper.Mapper.Initialize(mapConfig =>
            {
                mapConfig.AllowNullCollections = true;
                mapConfig.AllowNullDestinationValues = true;
                mapConfig.CreateMissingTypeMaps = true;

                mapConfig.CreateMap<Models.WishList.WishListApiModel, Core.Models.WishList>()
                         .ForMember(dest => dest.SelfReportedBehaviorRating, dest => dest.MapFrom(src => src.BehaviorRating));
            });

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            config.Services.Add(typeof(ServiceContext), serviceContext);

            appBuilder.UseWebApi(config);
        }
    }
}
