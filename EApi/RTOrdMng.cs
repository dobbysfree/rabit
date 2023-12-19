using Akka.Actor;
using Rabit.Info;
using Rabit.Models;
using System;
using System.Linq;
using XA_DATASETLib;

namespace Rabit.Comm
{
    public class RTOrdMng : UntypedActor
    {
        #region Instance
        readonly XARealClass sc0;    // 접수
        readonly XARealClass sc1;    // 체결
        readonly XARealClass sc2;    // 정정
        readonly XARealClass sc3;    // 취소
        readonly XARealClass sc4;    // 거부
        #endregion

        #region Actor Receive
        protected override void OnReceive(object obj)
        {

        }
        #endregion

        #region Request
        public RTOrdMng()
        {
            sc0 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC0.res"
            };
            sc0.ReceiveRealData += ReceiveSC0;
            sc0.AdviseRealData();

            sc1 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC1.res"
            };
            sc1.ReceiveRealData += ReceiveSC1;
            sc1.AdviseRealData();

            sc2 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC2.res"
            };
            sc2.ReceiveRealData += ReceiveSC2;
            sc2.AdviseRealData();

            sc3 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC3.res"
            };
            sc3.ReceiveRealData += ReceiveSC3;
            sc3.AdviseRealData();

            sc4 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC4.res"
            };
            sc4.ReceiveRealData += ReceiveSC4;
            sc4.AdviseRealData();
        }
        #endregion

        #region 주문접수 수신
        public void ReceiveSC0(string tr)
        {
            try
            {
                if (sc0.GetFieldData("OutBlock", "accno") != Conf.IConfig["ebest:acnt"]) return;

                
                // 주문체결구분(01:주문, 02:정정, 03:취소, 11:체결, 12:정정확인, 13:취소확인, 14:거부)
                string gubun    = sc0.GetFieldData("OutBlock", "ordchegb");
                string code     = sc0.GetFieldData("OutBlock", "shtcode").Substring(1);     // 종목코드
                string ordNo    = sc0.GetFieldData("OutBlock", "ordno");                    // 주문번호                 
                long ordQty     = long.Parse(sc0.GetFieldData("OutBlock", "ordqty"));       // 주문수량
                string bs       = sc0.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수
                string orgordno = sc0.GetFieldData("OutBlock", "orgordno");                 // 원주문번호

                Data.Items.TryGetValue(code, out Item im);

                string name     = sc0.GetFieldData("OutBlock", "hname");                    // 종목명                
                long ordPrc     = long.Parse(sc0.GetFieldData("OutBlock", "ordprice"));     // 주문가격                
                long deposit    = long.Parse(sc0.GetFieldData("OutBlock", "deposit"));      // 예수금
                long ordablemny = long.Parse(sc0.GetFieldData("OutBlock", "ordablemny"));   // 주문가능현금

                Conf.ILog.Information(string.Format("type:{0}접수, code:{1}, name:{2}, qty:{3}, prc:{4}, side:{5}, ordno:{6}, orgordno:{7}, askprc:{8}, askrem:{9}, bidprc:{10}, bidrem:{11}",
                    Dic.Chegyul[gubun], code, name, ordQty, ordPrc, Dic.BuySell[bs], ordNo, orgordno, im.HogaAsk.price, im.HogaAsk.rem, im.HogaBid.price, im.HogaBid.rem));
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion
        
        #region 주문체결 수신
        public void ReceiveSC1(string tr)
        {
            try
            {
                if (sc1.GetFieldData("OutBlock", "accno") != Conf.IConfig["ebest:acnt"]) return;

                string code     = sc1.GetFieldData("OutBlock", "shtnIsuno").Substring(1);   // 종목코드                
                long execQty    = long.Parse(sc1.GetFieldData("OutBlock", "execqty"));      // 체결수량
                string bs       = sc1.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수                
                string name     = sc1.GetFieldData("OutBlock", "Isunm");                    // 종목명
                long unercqty   = long.Parse(sc1.GetFieldData("OutBlock", "unercqty"));     // 미체결수량
                long deposit    = long.Parse(sc1.GetFieldData("OutBlock", "deposit"));      // 예수금
                long ordablemny = long.Parse(sc1.GetFieldData("OutBlock", "ordablemny"));   // 주문가능현금
                string ordno    = sc1.GetFieldData("OutBlock", "ordno");                    // 주문번호
                string execTm   = sc1.GetFieldData("OutBlock", "exectime");                 // 체결시각
                long execPrc    = long.Parse(sc1.GetFieldData("OutBlock", "execprc"));      // 체결가격

                Data.Items.TryGetValue(code, out Item im);

                Conf.ILog.Information(string.Format("type:체결, code:{0}, name:{1}, execqty:{2}, execprc:{3}, side:{4}, unercqty:{5}, exectime:{6}, ordno:{7}, posqty:{8}, askprc:{9}, askrem:{10}, bidprc:{11}, bidrem:{12}, pos:{13}",
                    code, name, execQty, execPrc, Dic.BuySell[bs], unercqty, execTm, ordno, im.PosQty, im.HogaAsk.price, im.HogaAsk.rem, im.HogaBid.price, im.HogaBid.rem, im.CrntPos));                
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region 주문정정 수신
        public void ReceiveSC2(string tr)
        {
            try
            {
                if (sc2.GetFieldData("OutBlock", "accno") != Conf.IConfig["ebest:acnt"]) return;

                string code         = sc2.GetFieldData("OutBlock", "shtnIsuno").Substring(1);   // 종목코드
                long mdfy_qty       = long.Parse(sc2.GetFieldData("OutBlock", "mdfycnfqty"));   // 정정확인수량

                Data.Items.TryGetValue(code, out Item im);

                if (im.CrntPos == (int)PosStatus.Waiting) im.CrntPos = (int)PosStatus.OngoingSell;
                else im.CrntPos = (int)PosStatus.CollectLoss;

                string name         = sc2.GetFieldData("OutBlock", "Isunm");                    // 종목명
                string bnstp        = sc2.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수
                string ordno        = sc2.GetFieldData("OutBlock", "ordno");                    // 주문번호
                string orgordno     = sc2.GetFieldData("OutBlock", "orgordno");                 // 원주문번호
                long ord_qty        = long.Parse(sc2.GetFieldData("OutBlock", "ordqty"));       // 주문수량                
                long ord_prc        = long.Parse(sc2.GetFieldData("OutBlock", "ordprc"));       // 주문가격
                long mdfy_prc       = long.Parse(sc2.GetFieldData("OutBlock", "mdfycnfprc"));   // 정정확인가격
                long unercqty       = long.Parse(sc2.GetFieldData("OutBlock", "unercqty"));     // 미체결수량

                Conf.ILog.Information(string.Format("type:정정, code:{0}, name:{1}, side:{2}, ordno:{3}, orgordno:{4}, ordqty:{5}, mdfyqty:{6}, ordprc:{7}, mdfyprc:{8}, unercqty:{9}, askprc:{10}, askrem:{11}, bidprc:{12}, bidrem:{13}, pos:{14}",
                    code, name, Dic.BuySell[bnstp], ordno, orgordno, ord_qty, mdfy_qty, ord_prc, mdfy_prc, unercqty, im.HogaAsk.price, im.HogaAsk.rem, im.HogaBid.price, im.HogaBid.rem, im.CrntPos));
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region 주문취소 수신        
        public void ReceiveSC3(string tr)
        {
            try
            {
                if (sc3.GetFieldData("OutBlock", "accno") != Conf.IConfig["ebest:acnt"]) return;

                string code             = sc3.GetFieldData("OutBlock", "shtnIsuno").Substring(1);   // 종목코드
                long cncl_qty           = long.Parse(sc3.GetFieldData("OutBlock", "canccnfqty"));   // 취소확인수량
                string bs               = sc3.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수
                string name             = sc3.GetFieldData("OutBlock", "Isunm");                    // 종목명
                string ordno            = sc3.GetFieldData("OutBlock", "ordno");                    // 주문번호                
                string orgordno         = sc3.GetFieldData("OutBlock", "orgordno");                 // 원주문번호                
                string orgordcancqty    = sc3.GetFieldData("OutBlock", "orgordcancqty");            // 원주문취소수량

                Data.Items.TryGetValue(code, out Item im);
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region 거부 수신
        public void ReceiveSC4(string tr)
        {
            try
            {
                if (sc4.GetFieldData("OutBlock", "accno") != Conf.IConfig["ebest:acnt"]) return;

                string code     = sc4.GetFieldData("OutBlock", "shtnIsuno").Substring(1).Trim();    // 종목코드
                long rjt_qty    = long.Parse(sc4.GetFieldData("OutBlock", "rjtqty"));               // 거부수량
                string bnstp    = sc4.GetFieldData("OutBlock", "bnstp");                            // 1:매도, 2:매수 
                long ord_qty    = long.Parse(sc4.GetFieldData("OutBlock", "ordqty"));               // 주문수량
                long ord_prc    = long.Parse(sc4.GetFieldData("OutBlock", "ordprc"));               // 주문수량
                string ordno    = sc4.GetFieldData("OutBlock", "ordno");                            // 주문번호                
                string orgordno = sc4.GetFieldData("OutBlock", "orgordno");                         // 원주문번호      

                Item im         = Data.Items[code];

                Conf.ILog.Information(string.Format("거부, code:{0}, name:{1}, side:{2}, ordno:{3}, orgordno:{4}, ordqty:{5}, ordprc:{6}, rjtqty:{7}, askprc:{8}, askrem:{9}, bidprc:{10}, bidrem:{11}, pos:{12}",
                    code, im.Name, bnstp, ordno, orgordno, ord_qty, ord_prc, rjt_qty, im.HogaAsk.price, im.HogaAsk.rem, im.HogaBid.price, im.HogaBid.rem, im.CrntPos));
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion
    }
}