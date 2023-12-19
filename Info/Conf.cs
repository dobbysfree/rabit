using Microsoft.Extensions.Configuration;
using Serilog;
using System;

namespace Rabit.Info
{
    public class Conf
    {
        public static IConfiguration IConfig { get; set; }

        public static ILogger ILog;

        public static DateTime OpenTime { get; set; }
        public static DateTime CloseTime { get; set; }
        public static DateTime SellTime { get; set; }
        public static DateTime ScaleupTime { get; set; }
    }
}