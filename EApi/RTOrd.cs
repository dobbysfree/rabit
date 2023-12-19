using Rabit.Helpers;
using Rabit.Info;
using Rabit.Models;
using System;
using System.Text;
using XA_DATASETLib;

namespace Rabit.Comm
{
    public class RTOrd
    {
        #region Instance
        XARealClass sc0;    // 접수
        XARealClass sc1;    // 체결FF
        XARealClass sc2;    // 정정
        XARealClass sc3;    // 취소
        XARealClass sc4;    // 거부

        Item im;
        #endregion

        #region Request
        public RTOrd(Item item)
        {
            im = item;

            sc0 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC0.res"
            };
            sc0.ReceiveRealData += ReceiveSC0;
            sc0.AdviseRealData();

            Tool.Delay(70);

            sc1 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC1.res"
            };
            sc1.ReceiveRealData += ReceiveSC1;
            sc1.AdviseRealData();
            
            Tool.Delay(70);

            sc2 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC2.res"
            };
            sc2.ReceiveRealData += ReceiveSC2;
            sc2.AdviseRealData();

            Tool.Delay(70);

            sc3 = new XARealClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "SC3.res"
            };
            sc3.ReceiveRealData += ReceiveSC3;
            sc3.AdviseRealData();

            Tool.Delay(70);

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
                                
                string code = sc0.GetFieldData("OutBlock", "shtcode").Substring(1);     // 종목코드
                if (code != im.Code) return;

                // 주문체결구분(01:주문, 02:정정, 03:취소, 11:체결, 12:정정확인, 13:취소확인, 14:거부)
                string gubun    = sc0.GetFieldData("OutBlock", "ordchegb");
                string ordno    = sc0.GetFieldData("OutBlock", "ordno");                    // 주문번호                 
                long ord_qty    = long.Parse(sc0.GetFieldData("OutBlock", "ordqty"));       // 주문수량
                string bs       = sc0.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수
                string orgordno = sc0.GetFieldData("OutBlock", "orgordno");                 // 원주문번호

                if (gubun == "01") // 주문
                {
                    if (bs == Dic.Buy)
                    {
                        im.Buying.OrdNo = ordno;
                        im.CrntPos      = EnumPositions.RcptBuy;
                    }
                    else if (bs == Dic.Sell)
                    {
                        im.Selling.OrdNo    = ordno;
                        im.Selling.Qty      = ord_qty;
                        im.CrntPos          = EnumPositions.OngoingSell;
                    }
                }
                else if (gubun == "02" && im.Selling.OrdNo == orgordno) // 정정
                {
                    im.Selling.OrdNo    = ordno;
                    im.Selling.Qty      = ord_qty;
                }

                string name     = sc0.GetFieldData("OutBlock", "hname");                    // 종목명
                long ord_prc    = long.Parse(sc0.GetFieldData("OutBlock", "ordprice"));     // 주문가격

                StringBuilder sb = new StringBuilder();
                sb.Append("type:").Append(Dic.Chegyul[gubun]).Append("접수").Append(", ");
                sb.Append("code:").Append(code).Append(", ");
                sb.Append("name:").Append(name).Append(", ");
                sb.Append("qty:").Append(ord_qty).Append(", ");
                sb.Append("prc:").Append(ord_prc).Append(", ");
                sb.Append("side:").Append(Dic.BuySell[bs]).Append(", ");             
                sb.Append("ordno:").Append(ordno).Append(", ");
                sb.Append("orgordno:").Append(orgordno).Append(", ");
                sb.Append("askprc:").Append(im.HogaAsk.price).Append(", ");
                sb.Append("askrem:").Append(im.HogaAsk.rem).Append(", ");
                sb.Append("bidprc:").Append(im.HogaBid.price).Append(", ");
                sb.Append("bidrem:").Append(im.HogaBid.rem).Append(", ");
                sb.Append("pos:").Append(im.CrntPos);
                Conf.ILog.Information(sb.ToString());
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

                string code = sc1.GetFieldData("OutBlock", "shtnIsuno").Substring(1);   // 종목코드
                if (code != im.Code) return;

                long exec_qty   = long.Parse(sc1.GetFieldData("OutBlock", "execqty"));      // 체결수량
                string bs       = sc1.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수
                string name     = sc1.GetFieldData("OutBlock", "Isunm");                    // 종목명
                long unercqty   = long.Parse(sc1.GetFieldData("OutBlock", "unercqty"));     // 미체결수량
                long deposit    = long.Parse(sc1.GetFieldData("OutBlock", "deposit"));      // 예수금
                long ordablemny = long.Parse(sc1.GetFieldData("OutBlock", "ordablemny"));   // 주문가능현금
                string ordno    = sc1.GetFieldData("OutBlock", "ordno");                    // 주문번호
                string exectime = sc1.GetFieldData("OutBlock", "exectime");                 // 체결시각
                long exec_prc   = long.Parse(sc1.GetFieldData("OutBlock", "execprc"));      // 체결가격

                StringBuilder sb = new StringBuilder();
                sb.Append("type:체결").Append(", ");
                sb.Append("code:").Append(code).Append(", ");
                sb.Append("name:").Append(name).Append(", ");
                sb.Append("execqty:").Append(exec_qty).Append(", ");
                sb.Append("execprc:").Append(exec_prc).Append(", ");
                sb.Append("side:").Append(Dic.BuySell[bs]).Append(", ");
                sb.Append("unercqty:").Append(unercqty).Append(", ");
                sb.Append("exectime:").Append(exectime).Append(", ");
                sb.Append("ordno:").Append(ordno).Append(", ");
                sb.Append("posqty:").Append(im.PosQty).Append(", ");
                sb.Append("askprc:").Append(im.HogaAsk.price).Append(", ");
                sb.Append("askrem:").Append(im.HogaAsk.rem).Append(", ");
                sb.Append("bidprc:").Append(im.HogaBid.price).Append(", ");
                sb.Append("bidrem:").Append(im.HogaBid.rem).Append(", ");
                sb.Append("pos:").Append(im.CrntPos);
                Conf.ILog.Information(sb.ToString());
                
                // 매수
                if (bs == Dic.Buy)
                {
                    im.PosQty += exec_qty;
                    im.CrntPos = EnumPositions.OrdSell;
                    im.TargetSellPrc = im.HogaAsk.price - im.Unit;

                    // 매도 요청
                    TROrd.ReqNewOrder(im, Dic.Sell, im.TargetSellPrc, exec_qty);
                    //Program.ActSys.ActorSelection("user/SiseMng/" + im.Code).Tell(new Mail { Type = "sell", OrdPrc = im.TargetSellPrc, OrdQty = exec_qty });

                    // 부분 체결시, 잔량 취소주문 요청
                    if (unercqty > 0) TROrd.ReqCancelOrder(im, ordno, unercqty);
                    //Program.ActSys.ActorSelection("user/SiseMng/" + im.Code).Tell(new Mail { Type = "cancel", OrdNo = ordno, OrdQty = unercqty });
                }
                // 매도
                else if (bs == Dic.Sell)
                {
                    im.PosQty -= exec_qty;

                    // 미체결이 없을때
                    if (im.PosQty == 0)
                    {
                        im.TradeTime = DateTime.Now.AddSeconds(10).TimeOfDay; // Next action interval time

                        double per = (exec_qty / (double)im.TargetBuyPrc - 1) * 100;
                        if (per < 0) im.LossCnt -= 1;

                        // 2회 손실인 경우 종목 제외
                        if (im.LossCnt <= -2 || per < -1.8 || im.CrntPos == EnumPositions.CollectLoss) im.CrntPos = EnumPositions.DropOut;
                        else im.CrntPos = EnumPositions.None;
                    }
                }
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

                string code = sc2.GetFieldData("OutBlock", "shtnIsuno").Substring(1);   // 종목코드
                if (code != im.Code) return;

                if (im.CrntPos == EnumPositions.Waiting) im.CrntPos = EnumPositions.OngoingSell;
                else im.CrntPos = EnumPositions.CollectLoss;

                long mdfy_qty   = long.Parse(sc2.GetFieldData("OutBlock", "mdfycnfqty"));   // 정정확인수량
                string name     = sc2.GetFieldData("OutBlock", "Isunm");                    // 종목명
                string bs       = sc2.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수
                string ordno    = sc2.GetFieldData("OutBlock", "ordno");                    // 주문번호
                string orgordno = sc2.GetFieldData("OutBlock", "orgordno");                 // 원주문번호
                long ord_qty    = long.Parse(sc2.GetFieldData("OutBlock", "ordqty"));       // 주문수량                
                long ord_prc    = long.Parse(sc2.GetFieldData("OutBlock", "ordprc"));       // 주문가격
                long mdfy_prc   = long.Parse(sc2.GetFieldData("OutBlock", "mdfycnfprc"));   // 정정확인가격
                long unercqty   = long.Parse(sc2.GetFieldData("OutBlock", "unercqty"));     // 미체결수량

                StringBuilder sb = new StringBuilder();
                sb.Append("type:정정").Append(", ");
                sb.Append("code:").Append(code).Append(", ");
                sb.Append("name:").Append(name).Append(", ");
                sb.Append("side:").Append(Dic.BuySell[bs]).Append(", ");
                sb.Append("ordno:").Append(ordno).Append(", ");
                sb.Append("orgordno:").Append(orgordno).Append(", ");
                sb.Append("ordqty:").Append(ord_qty).Append(", ");
                sb.Append("mdfyqty:").Append(mdfy_qty).Append(", ");
                sb.Append("ordprc:").Append(ord_prc).Append(", ");
                sb.Append("mdfyprc:").Append(mdfy_prc).Append(", ");
                sb.Append("unercqty:").Append(unercqty).Append(", ");
                sb.Append("askprc:").Append(im.HogaAsk.price).Append(", ");
                sb.Append("askrem:").Append(im.HogaAsk.rem).Append(", ");
                sb.Append("bidprc:").Append(im.HogaBid.price).Append(", ");
                sb.Append("bidrem:").Append(im.HogaBid.rem).Append(", ");
                sb.Append("pos:").Append(im.CrntPos);
                Conf.ILog.Information(sb.ToString());
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

                string code = sc3.GetFieldData("OutBlock", "shtnIsuno").Substring(1);   // 종목코드
                if (code != im.Code) return;

                long cncl_qty           = long.Parse(sc3.GetFieldData("OutBlock", "canccnfqty"));   // 취소확인수량
                string bs               = sc3.GetFieldData("OutBlock", "bnstp");                    // 1:매도, 2:매수
                string name             = sc3.GetFieldData("OutBlock", "Isunm");                    // 종목명
                string ordno            = sc3.GetFieldData("OutBlock", "ordno");                    // 주문번호                
                string orgordno         = sc3.GetFieldData("OutBlock", "orgordno");                 // 원주문번호                
                string orgordcancqty    = sc3.GetFieldData("OutBlock", "orgordcancqty");            // 원주문취소수량

                // 취소의 경우는 매수밖에 없고, 매도는 청산밖에 없음.
                string type = im.Buying.Qty == cncl_qty ? "전체" : "부분";

                StringBuilder sb = new StringBuilder();
                sb.Append("type:").Append(im.Buying.Qty == cncl_qty ? "전체" : "부분").Append("취소").Append(", ");
                sb.Append("code:").Append(code).Append(", ");
                sb.Append("name:").Append(name).Append(", ");
                sb.Append("side:").Append(Dic.BuySell[bs]).Append(", ");
                sb.Append("ordno:").Append(ordno).Append(", ");
                sb.Append("orgordno:").Append(orgordno).Append(", ");
                sb.Append("orgqty:").Append(im.Buying.Qty).Append(", ");
                sb.Append("cnclqty:").Append(cncl_qty).Append(", ");
                sb.Append("orgcnclqty:").Append(orgordcancqty).Append(", ");
                sb.Append("askprc:").Append(im.HogaAsk.price).Append(", ");
                sb.Append("askrem:").Append(im.HogaAsk.rem).Append(", ");
                sb.Append("bidprc:").Append(im.HogaBid.price).Append(", ");
                sb.Append("bidrem:").Append(im.HogaBid.rem).Append(", ");
                sb.Append("pos:").Append(im.CrntPos);
                Conf.ILog.Information(sb.ToString());

                if (im.Buying.Qty == cncl_qty)
                {
                    im.CrntPos = (int)EnumPositions.None;
                    im.TradeTime = DateTime.Now.AddSeconds(10).TimeOfDay;
                }

                im.Buying.Qty -= cncl_qty;
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
                if (code != im.Code) return;

                long rjt_qty    = long.Parse(sc4.GetFieldData("OutBlock", "rjtqty"));               // 거부수량
                string bs    = sc4.GetFieldData("OutBlock", "bnstp");                            // 1:매도, 2:매수 
                long ord_qty    = long.Parse(sc4.GetFieldData("OutBlock", "ordqty"));               // 주문수량
                long ord_prc    = long.Parse(sc4.GetFieldData("OutBlock", "ordprc"));               // 주문수량
                string ordno    = sc4.GetFieldData("OutBlock", "ordno");                            // 주문번호                
                string orgordno = sc4.GetFieldData("OutBlock", "orgordno");                         // 원주문번호
                string trcode   = sc4.GetFieldData("OutBlock", "trcode");                           // TRCODE

                StringBuilder sb = new StringBuilder();
                sb.Append(Dic.RejectTrCode[trcode]).Append("거부").Append(", ");
                sb.Append("code:").Append(code).Append(", ");
                sb.Append("name:").Append(im.Name).Append(", ");
                sb.Append("side:").Append(Dic.BuySell[bs]).Append(", ");
                sb.Append("ordno:").Append(ordno).Append(", ");
                sb.Append("orgordno:").Append(orgordno).Append(", ");
                sb.Append("ordqty:").Append(ord_qty).Append(", ");
                sb.Append("ordprc:").Append(ord_prc).Append(", ");
                sb.Append("rjtqty:").Append(rjt_qty).Append(", ");
                sb.Append("askprc:").Append(im.HogaAsk.price).Append(", ");
                sb.Append("askrem:").Append(im.HogaAsk.rem).Append(", ");
                sb.Append("bidprc:").Append(im.HogaBid.price).Append(", ");
                sb.Append("bidrem:").Append(im.HogaBid.rem).Append(", ");
                sb.Append("pos:").Append(im.CrntPos);
                Conf.ILog.Information(sb.ToString());
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion
    }
}