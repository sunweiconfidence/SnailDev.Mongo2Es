﻿using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Mongo2Es
{
    class Program
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var arguments = new ConfigurationBuilder().AddCommandLine(args).Build();
            var url = arguments["bindip"] ?? "http://localhost:5000";
            var mongoUrl = arguments["mongo"] ?? "mongodb://localhost:27017";

            //Log
            string assemblyFolder = Directory.GetCurrentDirectory();
            NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(assemblyFolder, "NLog.config"), true);
            logger.Info("Log Setting Completed.");

            // Service
            Thread syncSerivce = new Thread(() =>
            {
                var client = new Middleware.SyncClient(mongoUrl);
                client.Run();
            });
            syncSerivce.Start();
            logger.Info("Service Start Completed.");

            // Web
            Thread syncWeb = new Thread(() =>
             {
                 var host =
                     WebHost.CreateDefaultBuilder(args)
                     .UseContentRoot(assemblyFolder)
                     .UseUrls(url)
                     .UseStartup<Startup>()
                     .Build();

                 host.Run();
             });
            syncWeb.Start();
            logger.Info("Web Start Completed.");
        }
    }
}
