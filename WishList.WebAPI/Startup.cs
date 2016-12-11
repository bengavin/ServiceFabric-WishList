using System.Web.Http;
using Owin;
using System.Fabric;
using System;

namespace WishList.WebAPI
{
    public interface IServiceContextProvider
    {
        ServiceContext GetServiceContext();
    }
    public class ServiceContextProvider : IServiceContextProvider
    {
        private readonly ServiceContext _serviceContext;

        public ServiceContextProvider(ServiceContext serviceContext)
        {
            _serviceContext = serviceContext;
        }

        public ServiceContext GetServiceContext()
        {
            return _serviceContext;
        }
    }

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

            appBuilder.UseWebApi(config);
        }
    }
}
