using Rabit.Assist;
using Rabit.Info;
using System;
using System.Net;
using System.Net.Sockets;

namespace Rabit.Helpers
{
    public class Tool
    {
        #region Delay
        public static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                ThisMoment = DateTime.Now;
            }
            return DateTime.Now;
        }
        #endregion

        #region 호가단위
        public static int HogaUnit(long prc, string gb)
        {
            int unit = 1;
            if (prc >= 500000) unit = 1000;
            else if (prc >= 200000) unit = 500;
            else if (prc >= 100000) unit = 100;
            else if (prc >= 50000) unit = 100;
            else if (prc >= 20000) unit = 50;
            else if (prc >= 10000) unit = 10;
            else if (prc >= 5000) unit = 10;
            else if (prc >= 2000) unit = 5;
            else if (prc >= 1000) unit = 1;
            return unit;
        }
        #endregion

        #region 호가기준가격
        public static long HogaUnitPrice(long prc, string gb)
        {
            long unit = 0;
            if (gb == Dic.KOSPI && (70000 <= prc && prc <= 130000)) unit = 100000;
            else if (35000 <= prc && prc < 65000) unit = 50000;

            return unit;
        }
        #endregion

        #region 타겟 틱
        public static int TargetTickByUnit(int unit)
        {
            int tick = 8;
            if (unit == 500) tick = 6;
            else if (unit == 100) tick = 7;
            return tick;
        }
        #endregion

        #region 매매 타겟 수량
        public static int TargetQtyByPrice(long prc)
        {
            int qty = 1;

            if (prc < 50000) qty = 3;
            else if (prc < 100000) qty = 2;

            return qty;
        }
        #endregion


        #region 휴일체크
        public static long IsHoliday()
        {
            var dt = DB.SelectSingle("SELECT EXISTS (SELECT date FROM tb_holiday WHERE date=CURDATE() AND NATION_CD='KR') AS success;");
            return (long)dt.Rows[0].ItemArray[0];
        }
        #endregion

        #region current IP
        public static string GetCrntIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string ClientIP = string.Empty;
            for (int i = 0; i < host.AddressList.Length; i++)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ClientIP = host.AddressList[i].ToString();
                }
            }

            return ClientIP;
        }
        #endregion
    }
}