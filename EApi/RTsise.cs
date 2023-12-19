using Akka.Actor;
using Rabit.Helpers;
using Rabit.Info;
using Rabit.Models;
using System;
using System.Text;
using XA_DATASETLib;

namespace Rabit.Comm
{
    public class RTsise : UntypedActor
    {
        #region Instance
        Item im;
        XARealClass xh;
        RTOrd RcvOrd;
        #endregion

        #region Actor Receiver
        protected override void OnReceive(object obj)
        {
            //try
            //{
            //    Mail mail = (Mail)obj;
            //    if (mail.Type == "sell") TROrd.ReqNewOrder(im, Dic.Sell, mail.OrdPrc, mail.OrdQty);
            //    else if (mail.Type == "cancel") TROrd.ReqCancelOrder(im, mail.OrdNo, mail.OrdQty);
            //    else if (mail.Type == "modify") TROrd.ReqModifyOrder(im, mail.OrdNo, mail.OrdPrc, mail.OrdQty);                              
            //}
            //catch (Exception ex)
            //{                
            //    Conf.ILog.Error(string.Format("rcv_act > code:{0}, name:{1}, error:{2}", im.Code, im.Name, ex.ToString()));
            //}
        }
        #endregion

        #region Request
        public RTsise(Item item)
        {
            im = item;

            RcvOrd = new RTOrd(im); // 주문수신

            xh = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + (item.Gubun == Dic.KOSPI ? "H1_" : "HA_") + ".res"
            };
            xh.ReceiveRealData += Hoga;
            xh.SetFieldData("InBlock", "shcode", im.Code);
            xh.AdviseRealData();
        }
        #endregion

        #region 5호가 수신(실시간)
        void Hoga(string tr)
        {
            try
            {
                // 1:장중, 2:시간외, 3:장전/장중/장마감            
                if (xh.GetFieldData("OutBlock", "donsigubun") != "1") return;
         
                im.RcvTime       = DateTime.ParseExact(xh.GetFieldData("OutBlock", "hotime"), "HHmmss", null);     // 호가시간            
                im.HogaAsk.price = long.Parse(xh.GetFieldData("OutBlock", "offerho1"));  // 매도1호가
                im.HogaAsk.rem   = long.Parse(xh.GetFieldData("OutBlock", "offerrem1")); // 매도1잔량
                im.HogaBid.price = long.Parse(xh.GetFieldData("OutBlock", "bidho1"));    // 매수1호가
                im.HogaBid.rem   = long.Parse(xh.GetFieldData("OutBlock", "bidrem1"));   // 매수1잔량
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        void ModifySell(string type, long prc, StringBuilder add)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("type:").Append(type).Append(", ");
            sb.Append("code:").Append(im.Code).Append(", ");
            sb.Append("name:").Append(im.Name).Append(", ");
            sb.Append("time:").Append(im.RcvTime.ToString("HH:mm:ss")).Append(", ");
            sb.Append("qty:").Append(im.PosQty).Append(", ");
            sb.Append("prc:").Append(prc).Append(", ");
            sb.Append("side:").Append("SELL").Append(", ");
            if (add != null) sb.Append(add);
            sb.Append("askprc:").Append(im.HogaAsk.price).Append(", ");
            sb.Append("askrem:").Append(im.HogaAsk.rem).Append(", ");
            sb.Append("bidprc:").Append(im.HogaBid.price).Append(", ");
            sb.Append("bidrem:").Append(im.HogaBid.rem).Append(", ");
            sb.Append("pos:").Append(im.CrntPos);
            Conf.ILog.Information(sb.ToString());

            TROrd.ReqModifyOrder(im, im.Selling.OrdNo, prc, im.PosQty);
        }
    }
}