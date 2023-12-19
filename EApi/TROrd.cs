using Rabit.Helpers;
using Rabit.Info;
using Rabit.Models;
using System;
using System.Text;
using XA_DATASETLib;

namespace Rabit.Comm
{
    public class TROrd
    {
        #region Instance
        static object lockey = new object();
        #endregion

        #region Initialize
        public TROrd()
        {
            xaord = new XAQueryClass();
            xaord.ResFileName = Conf.IConfig["ebest:res"] + "CSPAT00600.res";
            xaord.ReceiveMessage += Xaord_ReceiveMessage;

            Tool.Delay(70);

            xacncl = new XAQueryClass();
            xacncl.ResFileName = Conf.IConfig["ebest:res"] + "CSPAT00800.res";
            xacncl.ReceiveMessage += Xacncl_ReceiveMessage;

            Tool.Delay(70);

            xamdfy = new XAQueryClass();
            xamdfy.ResFileName = Conf.IConfig["ebest:res"] + "CSPAT00700.res";
            xamdfy.ReceiveMessage += Xamdfy_ReceiveMessage;
        }
        #endregion

        #region [CSPAT00600] 현물 정상주문
        static XAQueryClass xaord;        
        public static void ReqNewOrder(Item im, string bs, long prc, long qty)
        {
            try
            {
                lock (lockey)
                {
                    string blockNm = "CSPAT00600InBlock1";
                    xaord.SetFieldData(blockNm, "AcntNo", 0, Conf.IConfig["ebest:acnt"]);       // 계좌번호
                    xaord.SetFieldData(blockNm, "InptPwd", 0, Conf.IConfig["ebest:acntpw"]);    // 비밀번호
                    xaord.SetFieldData(blockNm, "IsuNo", 0, im.Code);                           // 종목번호
                    xaord.SetFieldData(blockNm, "OrdQty", 0, qty.ToString());                   // 주문수량
                    xaord.SetFieldData(blockNm, "OrdPrc", 0, prc.ToString());                   // 주문가
                    xaord.SetFieldData(blockNm, "BnsTpCode", 0, bs);                            // 1:매도, 2:매수
                    xaord.SetFieldData(blockNm, "OrdprcPtnCode", 0, "00");                      // 00:지정가, 03:시장가, 61:장개시전시간외종가, 82:시간외단일가
                    xaord.SetFieldData(blockNm, "MgntrnCode", 0, "000");                        // 신용거래코드
                    xaord.SetFieldData(blockNm, "LoanDt", 0, "");                               // 대출일                                
                    xaord.SetFieldData(blockNm, "OrdCndiTpCode", 0, "0");                       // 0:없음, 1:IOC(주문 즉시 체결 후 남은 수량 취소), 2:FOK(주문 즉시 전량 체결되지 않으면 주문 자체를 취소)

                    var result = xaord.Request(false);
                    if (result < 0) Conf.ILog.Warning(Dic.BuySell[bs] + " 주문요청실패 > " + im.Code + ", " + result);
                    else
                    {
                        Conf.ILog.Information(string.Format("type:{2}요청, code:{0}, name:{1}, side:{2}, qty:{3}, prc:{4}, pos:{5}, askprc:{6}, askrem:{7}, bidprc:{8}, bidrem:{9}, reqid:{10}",
                            im.Code, im.Name, Dic.BuySell[bs], qty, prc, im.CrntPos, im.HogaAsk.price, im.HogaAsk.rem, im.HogaBid.price, im.HogaBid.rem, result));
                    }
                }                
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region [CSPAT00800] 현물 취소주문
        static XAQueryClass xacncl;
        public static void ReqCancelOrder(Item im, string ordno, long qty)
        {
            try
            {
                lock (lockey)
                {
                    string blockNm = "CSPAT00800InBlock1";
                    xacncl.SetFieldData(blockNm, "OrgOrdNo", 0, ordno);                         // 원주문번호
                    xacncl.SetFieldData(blockNm, "AcntNo", 0, Conf.IConfig["ebest:acnt"]);      // 계좌번호
                    xacncl.SetFieldData(blockNm, "InptPwd", 0, Conf.IConfig["ebest:acntpw"]);   // 비밀번호
                    xacncl.SetFieldData(blockNm, "IsuNo", 0, im.Code);                          // 종목번호
                    xacncl.SetFieldData(blockNm, "OrdQty", 0, qty.ToString());                  // 주문수량

                    var result = xacncl.Request(false);
                    if (result < 0) Conf.ILog.Warning("취소요청실패 > " + im.Code + ", " + result);
                    else Conf.ILog.Information(string.Format("type:취소요청, code:{0}, name:{1}, ordno:{2}, qty:{3}, pos:{4}, reqid:{5}",
                        im.Code, im.Name, ordno, qty, im.CrntPos, result));
                }
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region [CSPAT00700] 현물 정정주문
        static XAQueryClass xamdfy;
        public static void ReqModifyOrder(Item im, string ordno, long price, long qty)
        {
            try
            {
                lock (lockey)
                {
                    string blockNm = "CSPAT00700InBlock1";
                    xamdfy.SetFieldData(blockNm, "OrgOrdNo", 0, ordno);                         // 원주문번호
                    xamdfy.SetFieldData(blockNm, "AcntNo", 0, Conf.IConfig["ebest:acnt"]);      // 계좌번호
                    xamdfy.SetFieldData(blockNm, "InptPwd", 0, Conf.IConfig["ebest:acntpw"]);   // 비밀번호
                    xamdfy.SetFieldData(blockNm, "IsuNo", 0, im.Code);                          // 종목번호
                    xamdfy.SetFieldData(blockNm, "OrdQty", 0, qty.ToString());                  // 주문수량
                    xamdfy.SetFieldData(blockNm, "OrdprcPtnCode", 0, "00");                     // 호가유형코드
                    xamdfy.SetFieldData(blockNm, "OrdCndiTpCode", 0, "0");                      // 주문조건구분
                    xamdfy.SetFieldData(blockNm, "OrdPrc", 0, price.ToString());                // 주문가

                    var result = xamdfy.Request(false);
                    if (result < 0) Conf.ILog.Warning("정정요청실패 > " + im.Code + ", " + result);
                    else Conf.ILog.Information(string.Format("type:정정요청, code:{0}, name:{1}, ordno:{2}, qty:{3}, prc:{4}, pos:{5}, reqid:{6}",
                        im.Code, im.Name, ordno, qty, price, im.CrntPos, result));
                }
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion


        #region receive message 주문
        private void Xaord_ReceiveMessage(bool IsError, string MsgCode, string Msg)
        {
            if (MsgCode == "00039" || MsgCode == "00040") return;

            StringBuilder sb = new StringBuilder();
            sb.Append("주문실패수신 > err:" + MsgCode + ", msg:" + Msg);

            //try
            //{
            //    string code = xaord.GetFieldData("CSPAT00600InBlock1", "IsuNo", 0).Trim();
            //    if (!string.IsNullOrEmpty(code))
            //    {
            //        var im = Data.Items[code];

            //        sb.Append(", ");
            //        sb.Append("code:").Append(im.Code).Append(", ");
            //        sb.Append("name:").Append(im.Name).Append(", ");
            //        sb.Append("ord_qty:").Append(xaord.GetFieldData("CSPAT00800InBlock1", "OrdQty", 0).Trim()).Append(", ");
            //        sb.Append("pos_qty:").Append(im.PosQty).Append(", ");
            //        sb.Append("pos:").Append(im.CrntPos);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Conf.ILog.Error(ex.ToString());
            //}
            
            Conf.ILog.Warning(sb.ToString());
        }
        #endregion

        #region receive message 정정
        private void Xamdfy_ReceiveMessage(bool IsError, string MsgCode, string Msg)
        {
            if (MsgCode == "00132") return;

            StringBuilder sb = new StringBuilder();
            sb.Append("정정실패수신 > err:" + MsgCode + ", msg:" + Msg);

            string code = xamdfy.GetFieldData("CSPAT00700InBlock1", "IsuNo", 0).Trim();
            if (!string.IsNullOrEmpty(code))
            {
                var im = Data.Items[code];

                sb.Append(", ");
                sb.Append("code:").Append(im.Code).Append(", ");
                sb.Append("name:").Append(im.Name).Append(", ");
                sb.Append("ord_qty:").Append(xamdfy.GetFieldData("CSPAT00700InBlock1", "OrdQty", 0).Trim()).Append(", ");
                sb.Append("pos_qty:").Append(im.PosQty).Append(", ");
                sb.Append("pos:").Append(im.CrntPos);
            }
            Conf.ILog.Warning(sb.ToString());
        }
        #endregion

        #region receive message 취소
        private void Xacncl_ReceiveMessage(bool IsError, string MsgCode, string Msg)
        {
            if (MsgCode == "00156") return;

            StringBuilder sb = new StringBuilder();
            sb.Append("취소실패수신 > err:" + MsgCode + ", msg:" + Msg);

            string code = xacncl.GetFieldData("CSPAT00800InBlock1", "IsuNo", 0).Trim();
            if (!string.IsNullOrEmpty(code))
            {
                var im = Data.Items[code];

                sb.Append(", ");
                sb.Append("code:").Append(im.Code).Append(", ");
                sb.Append("name:").Append(im.Name).Append(", ");
                sb.Append("ord_qty:").Append(xacncl.GetFieldData("CSPAT00800InBlock1", "OrdQty", 0).Trim()).Append(", ");
                sb.Append("pos_qty:").Append(im.PosQty).Append(", ");
                sb.Append("pos:").Append(im.CrntPos);
            }
            Conf.ILog.Warning(sb.ToString());
        }
        #endregion
    }
}