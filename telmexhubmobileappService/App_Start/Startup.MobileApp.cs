using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.Azure.Mobile.Server.Config;
using telmexhubmobileappService.DataObjects;
using telmexhubmobileappService.Models;
using Owin;

namespace telmexhubmobileappService
{
    public partial class Startup
    {
        public static void ConfigureMobileApp(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer(new telmexhubmobileappInitializer());

            // To prevent Entity Framework from modifying your database schema, use a null database initializer
            // Database.SetInitializer(null);

            app.UseMobileAppAuthentication(config);
            app.UseWebApi(config);
        }
    }

    public class telmexhubmobileappInitializer : CreateDatabaseIfNotExists<telmexhubmobileappContext>
    {
        
    }
}

