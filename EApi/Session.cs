using Rabit.Info;
using System;
using XA_SESSIONLib;

namespace Rabit.Comm
{
    public class Session
    {
        public static XASessionClass eapi;

        public Session()
        {
            eapi = new XASessionClass();
            eapi._IXASessionEvents_Event_Login += XASession_Login;

            Connect();
        }

        #region connect
        void Connect()
        {
            bool conn = eapi.ConnectServer("hts.ebestsec.co.kr", 20001);
            if (!conn)
            {
                var errCode = eapi.GetLastError();
                var errMsg = eapi.GetErrorMessage(errCode);
                Conf.ILog.Warning("Failed session > " + errCode + " , " + errMsg);
                return;
            }

            bool login = eapi.Login(Conf.IConfig["ebest:id"], Conf.IConfig["ebest:pw"], Conf.IConfig["ebest:cert"], 0, false);
            if (!login)
            {
                Conf.ILog.Warning("Failed login");
                return;
            }
        }
        #endregion

        #region 로그인 수신
        private void XASession_Login(string code, string msg)
        {
            Conf.ILog.Information(code + " > " + msg);
            if (code != "0000") return;

            if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday) return;

            if (DateTime.Now > Conf.CloseTime) FmCom.it.UpdateMarketData();
            else FmCom.it.Tradiing();
        }
        #endregion

        #region logout
        public void Logout()
        {
            Conf.ILog.Warning("logout > " + eapi.Logout());
        }
        #endregion
    }
}