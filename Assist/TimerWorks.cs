using Rabit.Info;
using System;
using System.Timers;

namespace Rabit.Helpers
{
    public class TimerWorks
    {
        #region Set
        public static void Set()
        {
            var close = DateTime.ParseExact(Conf.IConfig["time:close"], "HH:mm", null);
            TimeSpan[] times = new TimeSpan[2] { Conf.SellTime.TimeOfDay, close.AddMinutes(40).TimeOfDay };

            for (int i = 0; i < times.Length; i++)
            {
                if (times[i] < DateTime.Now.TimeOfDay) continue;

                double rem_tm = (times[i] - DateTime.Now.TimeOfDay).TotalMilliseconds;
                Timer tm = new Timer(rem_tm)
                {
                    AutoReset = false,
                    Enabled = true
                };

                if (i == 0) tm.Elapsed += OnSell;
                else if (i == 1) tm.Elapsed += OnUpdate;
            }
        }
        #endregion

        #region 청산 절차
        static void OnSell(object sender, ElapsedEventArgs e)
        {
            Timer tm = (Timer)sender;
            tm.Stop();

            FmCom.it.ClearContract();
        }
        #endregion

        #region 업데이트
        private static void OnUpdate(object sender, ElapsedEventArgs e)
        {
            Timer tm = (Timer)sender;
            tm.Stop();
            Conf.ILog.Warning("On Market Update");

            FmCom.it.UpdateMarketData();
        }
        #endregion
    }
}