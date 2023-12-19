using Akka.Actor;
using Microsoft.Extensions.Configuration;
using Rabit.Assist;
using Rabit.Helpers;
using Rabit.Info;
using Rabit.Models;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Rabit
{
    static class Program
    {
        public static ActorSystem ActSys;

        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday) return;

            Conf.IConfig        = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            Conf.OpenTime       = DateTime.ParseExact(Conf.IConfig["time:open"], "HH:mm", null);
            Conf.CloseTime      = DateTime.ParseExact(Conf.IConfig["time:close"], "HH:mm", null);
            Conf.ScaleupTime    = Conf.OpenTime.AddMinutes(15);
            Conf.SellTime       = Conf.CloseTime.AddMinutes(-10);

            Conf.ILog = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fffff}] {Message:lj}{NewLine}")
                .WriteTo.File(
                    Application.StartupPath + "/logs/log_.txt",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "[{Timestamp:HH:mm:ss.fffff}] {Message:lj}{NewLine}",
                    fileSizeLimitBytes: 50_000_000,
                    rollOnFileSizeLimit: true,
                    shared: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1),
                    retainedFileCountLimit: 500)
                .CreateLogger();

            // Actor System
            ActSys = ActorSystem.Create("ActSys");

            TimerWorks.Set();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FmCom());
        }

        public static List<Sise> Daily { get; set; } = new List<Sise>();
    }
}