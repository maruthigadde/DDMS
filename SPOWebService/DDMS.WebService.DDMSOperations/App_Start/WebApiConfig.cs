using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using DDMS.WebService.SPOActions;
using DDMS.WebService.ExternalServices.Interfaces;
using Unity;
using Unity.Lifetime;

namespace DDMS.WebService.DDMSOperations
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            var container = new UnityContainer();
            container.RegisterType<IDDMSSearchDocument, DDMSSearchDocument>(new HierarchicalLifetimeManager());
            container.RegisterType<IDDMSUploadDocument, DDMSUploadDocument>(new HierarchicalLifetimeManager());
            container.RegisterType<IDDMSDeleteDocument, DDMSDeleteDocument>(new HierarchicalLifetimeManager());
            config.DependencyResolver = new UnityResolver(container);

        }
    }
}
