using Rabit.Assist;
using Rabit.Helpers;
using Rabit.Info;
using Rabit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XA_DATASETLib;

namespace Rabit.Comm
{
    public class TR
    {
        #region 시간(T0167) 조회 - 연결 여부 확인용
        static XAQueryClass t0167;
        public static int ReqT0167()
        {
            t0167 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t0167.res"
            };

            t0167.SetFieldData("t0167InBlock", "id", 0, Conf.IConfig["ebest:id"]);
            var result = t0167.Request(false);

            if (result < 0) Conf.ILog.Warning("시간(T0167) 조회 실패 : " + Dic.Error[result]);

            return result;
        }
        #endregion
               
        #region [t8436] 종목 조회
        static XAQueryClass t8436;
        public static void ReqStock(int state)
        {
            t8436 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t8436.res"
            };

            if (state == 1) t8436.ReceiveData += AftT8436;
            else t8436.ReceiveData += RcvT8436;

            t8436.SetFieldData("t8436InBlock", "gubun", 0, "0");
            var result = t8436.Request(false);

            if (result < 0) Conf.ILog.Warning("[t8436] 종목 조회 실패 : " + Dic.Error[result]);
        }

        static void RcvT8436(string tr)
        {
            Data.Items = new Dictionary<string, Item>();

            string trout = tr + "OutBlock";

            for (int i = 0; i < t8436.GetBlockCount(trout); i++)
            {
                // 증권그룹 주식 외 제외
                if (t8436.GetFieldData(trout, "bu12gubun", i) != "01") continue;

                if (t8436.GetFieldData(trout, "spac_gubun", i) == "Y") continue;

                long jnilClose = long.Parse(t8436.GetFieldData(trout, "jnilclose", i)); // 전일가

                // 가능 거래 가격 종목
                if (DateTime.Now < Conf.CloseTime && (jnilClose < 10000 || jnilClose > 200000)) continue;

                Item im = new Item
                {
                    Code        = t8436.GetFieldData(trout, "shcode", i),   // 단축코드
                    Name        = t8436.GetFieldData(trout, "hname", i),    // 종목명
                    Gubun       = t8436.GetFieldData(trout, "gubun", i),    // 1:kospi, 2:kosdaq
                    JnilClose   = jnilClose
                };

                im.Unit         = Tool.HogaUnit(im.JnilClose, im.Gubun);
                im.UnitPrc      = Tool.HogaUnitPrice(im.JnilClose, im.Gubun);
                im.TargetTick   = Tool.TargetTickByUnit(im.Unit); // 타겟 틱
                im.TargetQty    = 1;
                
                Data.Items[im.Code] = im;
            }

            Conf.ILog.Information("[t8436] 종목 조회 완료");
            FmCom.wait?.TrySetResult(true);
        }

        static void AftT8436(string tr)
        {
            Data.Items = new Dictionary<string, Item>();

            string trout = tr + "OutBlock";
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < t8436.GetBlockCount(trout); i++)
            {
                // 증권그룹 주식 외 제외
                if (t8436.GetFieldData(trout, "bu12gubun", i) != "01") continue;

                if (t8436.GetFieldData(trout, "spac_gubun", i) == "Y") continue;

                Item im         = new Item();
                im.Code         = t8436.GetFieldData(trout, "shcode", i);   // 단축코드
                im.Name         = t8436.GetFieldData(trout, "hname", i);    // 종목명
                im.Gubun        = t8436.GetFieldData(trout, "gubun", i);    // 1:kospi, 2:kosdaq
                im.JnilClose    = long.Parse(t8436.GetFieldData(trout, "jnilclose", i)); // 전일가
                
                Data.Items[im.Code] = im;

                sb.Append(string.Format("INSERT INTO tb_stock_items (code, name, market) VALUES ('{0}', '{1}', '{2}') ON DUPLICATE KEY UPDATE name='{1}', market='{2}';\n", im.Code, im.Name, im.Gubun));
            }

            // DB 저장
            DB.Execute(sb.ToString());

            Conf.ILog.Information("[t8436] 종목 조회 완료");
            FmCom.wait?.TrySetResult(true);
        }
        #endregion
        

        //---------------------- 계좌관리 ----------------------       
        #region [t0424] (장중 or 시간외 단일가) 보유 주식잔고 조회
        static XAQueryClass t0424;
        public static void ReqBalanceStocks()
        {            
            t0424 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t0424.res"
            };
            t0424.ReceiveData += RcvT0424;

            t0424.SetFieldData("t0424InBlock", "accno", 0, Conf.IConfig["ebest:acnt"]);
            t0424.SetFieldData("t0424InBlock", "passwd", 0, Conf.IConfig["ebest:acntpw"]);
            t0424.SetFieldData("t0424InBlock", "prcgb", 0, "1");        // 1:평균단가
            t0424.SetFieldData("t0424InBlock", "chegb", 0, "2");        // 2:체결기준
            t0424.SetFieldData("t0424InBlock", "dangb", 0, "0");        // 0:정규장, 1:시간외단일가
            t0424.SetFieldData("t0424InBlock", "charge", 0, "1");       // 1:제비용포함
            t0424.SetFieldData("t0424InBlock", "cts_expcode", 0, "");
            var result = t0424.Request(false);

            if (result < 0) Conf.ILog.Warning("[t0424] 보유 주식잔고 조회 실패 : " + Dic.Error[result]);

            Tool.Delay(1000);
        }

        static void RcvT0424(string trCode)
        {
            try
            {
                string trout = trCode + "OutBlock1";

                for (int i = 0; i < t0424.GetBlockCount(trout); i++)
                {
                    string code     = t0424.GetFieldData(trout, "expcode", i);
                    string name     = t0424.GetFieldData(trout, "hname", i);
                    long qty        = long.Parse(t0424.GetFieldData(trout, "mdposqt", i));      // 매도가능수량
                    long pos_prc    = long.Parse(t0424.GetFieldData(trout, "pamt", i));         // 평균단가
                    double pos_per  = double.Parse(t0424.GetFieldData(trout, "sunikrt", i));    // 수익율
                    long price      = long.Parse(t0424.GetFieldData(trout, "price", i));        // 현재가
                    long janqty     = long.Parse(t0424.GetFieldData(trout, "janqty", i));       // 잔고수량
                    long appamt     = long.Parse(t0424.GetFieldData(trout, "appamt", i));       // 평가금액
                    long dtsunik    = long.Parse(t0424.GetFieldData(trout, "dtsunik", i));      // 평가손익

                    Conf.ILog.Information(string.Format("보유잔고 > code:{0}, name:{1}, 잔고수량:{2}, 매도가능수량:{3}, 평균단가:{4}, 현재가:{5}, 수익율:{6}, 평가금액:{7}, 평가손익:{8}",
                        code, name, janqty, qty, pos_prc, price, pos_per, appamt, dtsunik));

                    Data.Items.TryGetValue(code, out Item im);

                    im.PosQty       = janqty;   // 보유수량
                    im.TargetBuyPrc = pos_prc;  // 매수가격

                    if (im.CrntPos != EnumPositions.OngoingSell)
                    {
                        im.CrntPos = EnumPositions.OrdSell;

                        // 매도 요청
                        TROrd.ReqNewOrder(im, Dic.Sell, price, janqty);
                    }
                }
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
            finally
            {
                FmCom.wait?.TrySetResult(true);
            }
        }
        #endregion

        #region [CSPAQ12200] 예수금 조회        
        static XAQueryClass cspaq12200;
        public static void ReqDeposit()
        {
            cspaq12200 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "CSPAQ12200.res"
            };
            cspaq12200.ReceiveData += RcvCSPAQ12200;

            cspaq12200.SetFieldData("CSPAQ12200InBlock1", "RecCnt", 0, "1");
            cspaq12200.SetFieldData("CSPAQ12200InBlock1", "MgmtBrnNo", 0, "");
            cspaq12200.SetFieldData("CSPAQ12200InBlock1", "AcntNo", 0, Conf.IConfig["ebest:acnt"]);
            cspaq12200.SetFieldData("CSPAQ12200InBlock1", "Pwd", 0, Conf.IConfig["ebest:acntpw"]);
            cspaq12200.SetFieldData("CSPAQ12200InBlock1", "BalCreTp", 0, "0");

            var result = cspaq12200.Request(false);
            if (result < 0) Conf.ILog.Warning("[CSPAQ12200] 예수금 조회 실패 : " + Dic.Error[result]);

            Tool.Delay(1000);
        }

        static void RcvCSPAQ12200(string trCode)
        {
            try
            {
                string trout        = trCode + "OutBlock2";

                Data.Deposit        = long.Parse(cspaq12200.GetFieldData(trout, "Dps", 0));            // 예수금
                long MnyOrdAbleAmt  = long.Parse(cspaq12200.GetFieldData(trout, "MnyOrdAbleAmt", 0));  // 현금주문가능               
                long DpsastTotamt   = long.Parse(cspaq12200.GetFieldData(trout, "DpsastTotamt", 0));   // 예탁자산총액
               
                Conf.ILog.Information(string.Format("예수금 조회 완료 > 예수금:{0} | 현금주문가능:{1} | 예탁자산총액:{2}", Data.Deposit.ToString("N0"), MnyOrdAbleAmt.ToString("N0"), DpsastTotamt.ToString("N0")));
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
            finally
            {
                FmCom.wait?.TrySetResult(true);
            }
        }
        #endregion

        #region [T0425] 주식 체결/미체결 내역 조회
        static XAQueryClass t0425;
        static string cts_ordno = "";
        static bool isnext = true;
        static TaskCompletionSource<bool> tcs;

        public static async void ReqPending()
        {
            t0425 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t0425.res"
            };
            t0425.ReceiveData += RcvT0425;

            while (isnext)
            {
                t0425.SetFieldData("t0425InBlock", "accno", 0, Conf.IConfig["ebest:acnt"]);
                t0425.SetFieldData("t0425InBlock", "passwd", 0, Conf.IConfig["ebest:acntpw"]);
                t0425.SetFieldData("t0425InBlock", "expcode", 0, "");
                t0425.SetFieldData("t0425InBlock", "chegb", 0, "2");        // 0:전체, 1:체결, 2:미체결
                t0425.SetFieldData("t0425InBlock", "medosu", 0, "0");       // 0:전체, 1:매도, 2:매수
                t0425.SetFieldData("t0425InBlock", "sortgb", 0, "1");       // 1:주문번호 역순, 2:주문번호 순
                t0425.SetFieldData("t0425InBlock", "cts_ordno", 0, cts_ordno);

                tcs = new TaskCompletionSource<bool>();

                var result = t0425.Request(false);
                if (result < 0) Conf.ILog.Warning("[t0425] 주식 체결/미체결 내역 조회 실패 : " + Dic.Error[result]);

                await Task.WhenAny(tcs.Task, Task.Delay(10000));
                Tool.Delay(1000);
            }
        }

        static void RcvT0425(string tr)
        {
            cts_ordno = t0425.GetFieldData(tr + "OutBlock", "cts_ordno", 0);
            if (string.IsNullOrEmpty(cts_ordno)) isnext = false;

            tr += "OutBlock1";
            
            try
            {                
                for (int i = 0; i < t0425.GetBlockCount(tr); i++)
                {
                    string ordno    = t0425.GetFieldData(tr, "ordno", i);                   // 주문번호
                    string code     = t0425.GetFieldData(tr, "expcode", i);                 // 종목번호
                    string medosu   = t0425.GetFieldData(tr, "medosu", i);                  // 구분(매도, 매수)
                    long ord_qty    = long.Parse(t0425.GetFieldData(tr, "qty", i));         // 주문수량
                    long ord_prc    = long.Parse(t0425.GetFieldData(tr, "price", i));       // 주문가격
                    long ord_rem    = long.Parse(t0425.GetFieldData(tr, "ordrem", i));      // 미체결잔량
                    long cfmqty     = long.Parse(t0425.GetFieldData(tr, "cfmqty", i));      // 확인수량
                    string status   = t0425.GetFieldData(tr, "status", i);                  // 상태(체결)
                    long orgordno   = long.Parse(t0425.GetFieldData(tr, "orgordno", i));    // 원주문번호
                    string ordgb    = t0425.GetFieldData(tr, "ordgb", i);                   // 유형
                    var ordtime     = DateTime.ParseExact(t0425.GetFieldData(tr, "ordtime", i), "HHmmssff", null); // 주문시간
                    string hogagb   = t0425.GetFieldData(tr, "hogagb", i);                  // 호가유형(00:보통, 03:시장가)
                    long price      = long.Parse(t0425.GetFieldData(tr, "price1", i));      // 현재가

                    Data.Items.TryGetValue(code, out Item im);

                    Conf.ILog.Information(string.Format("미체결 > code:{0}, name:{1}, 주문번호:{2}, 구분:{3}, 주문수량:{4}, 주문가격:{5}, 미체결잔량:{6}, 확인수량:{7}, 상태:{8}, 원주문번호:{9}, 유형:{10}, 현재가:{11}",
                        code, im.Name, ordno, medosu, ord_qty, ord_prc, ord_rem, cfmqty, status, orgordno, ordgb, price));

                    im.TradeTime = ordtime.TimeOfDay;

                    if (medosu == "매수")
                    {
                        im.CrntPos      = EnumPositions.RcptBuy;

                        im.Buying.OrdNo = ordno;
                        im.TargetBuyPrc = ord_prc;
                        im.Buying.Qty   = ord_qty;

                        TROrd.ReqCancelOrder(im, ordno, ord_qty);
                    }
                    else if (medosu == "매도")
                    {
                        im.CrntPos          = EnumPositions.OngoingSell;
                        im.TargetBuyPrc     = ord_prc - (im.Unit * 2);
                        im.TargetSellPrc    = ord_prc;

                        im.Selling.OrdNo    = ordno;
                        im.Selling.Qty      = ord_qty;                   
                    }
                }
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
            finally
            {
                tcs?.TrySetResult(true);

                if (isnext == false)
                {
                    Conf.ILog.Information("[t0425] 주식 미체결 내역 조회 완료");
                    FmCom.wait?.TrySetResult(true);
                }
            }
        }
        #endregion


        //--------------------- 업데이트 --------------------
        #region TR Last Update
        public static void TRUpdate(string tr)
        {
            DB.Execute("UPDATE tb_tr_update SET date='" + DateTime.Today.ToString("yyyy-MM-dd") + "' WHERE tr='" + tr + "';");
        }

        static Dictionary<string, DateTime> TrLastUpdate;
        public static void GetTRUpdate()
        {
            TrLastUpdate = new Dictionary<string, DateTime>();

            var dt = DB.SelectSingle("SELECT * FROM tb_tr_update;");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i].ItemArray;
                TrLastUpdate[(string)row[0]] = (DateTime)row[1];
            }
        }
        #endregion

        #region [t1533] 특이테마
        static XAQueryClass t1533;
        public static void ReqIssueThema()
        {
            t1533 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t1533.res"
            };
            t1533.ReceiveData += RcvT1533;

            t1533.SetFieldData("t1533InBlock", "gubun", 0, "1");
            t1533.SetFieldData("t1533InBlock", "chgdate", 0, "");
                        
            var result = t1533.Request(false);
            if (result < 0) Conf.ILog.Warning("[T1533] 특이테마 조회 실패 : " + Dic.Error[result]);

            Tool.Delay(1000);
        }

        static void RcvT1533(string trCode)
        {
            try
            {
                string trout = trCode + "OutBlock";
                        
                Data.Today = DateTime.ParseExact(t1533.GetFieldData(trout, "bdate", 0), "yyyyMMdd", null);


                if (TrLastUpdate["t1533"] < Data.Today)
                {
                    Dictionary<string, Thema> Thema = new Dictionary<string, Thema>();

                    trout += "1";
                    StringBuilder sb = new StringBuilder();

                    for (int i = 0; i < t1533.GetBlockCount(trout); i++)
                    {
                        Thema thema = new Thema
                        {
                            idx     = i,
                            code    = t1533.GetFieldData(trout, "tmcode", i),                   // 테마코드
                            name    = t1533.GetFieldData(trout, "tmname", i),                   // 테마명
                            totcnt  = long.Parse(t1533.GetFieldData(trout, "totcnt", i)),       // 전체
                            upcnt   = long.Parse(t1533.GetFieldData(trout, "upcnt", i)),        // 상승
                            dncnt   = long.Parse(t1533.GetFieldData(trout, "dncnt", i)),        // 하락
                            uprate  = float.Parse(t1533.GetFieldData(trout, "uprate", i)),      // 상승비율
                            voldiff = float.Parse(t1533.GetFieldData(trout, "diff_vol", i)),    // 거래증가율
                            avgdiff = float.Parse(t1533.GetFieldData(trout, "avgdiff", i)),     // 평균등락율
                            chgdiff = float.Parse(t1533.GetFieldData(trout, "chgdiff", i))     // 대비등락율
                        };

                        Thema[thema.code] = thema;

                        sb.Append(string.Format("INSERT IGNORE INTO tb_daily_thema (date, code, totcnt, upcnt, dncnt, uprate, vol_diff, avg_diff, chg_diff) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');\n",
                            Data.Today.ToString("yyyy-MM-dd"), thema.code, thema.totcnt, thema.upcnt, thema.dncnt, thema.uprate, thema.voldiff, thema.avgdiff, thema.chgdiff));
                    }

                    TRUpdate("t1533");                
                    DB.Execute(sb.ToString());
                    Conf.ILog.Warning("[t1533] 특이테마 조회 완료 > " + Thema.Count);
                }

                FmCom.wait?.TrySetResult(true);
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion
                
        #region [t8425] 테마전체조회
        static XAQueryClass t8425;
        public static void ReqMetaThema()
        {
            if (Data.Today == TrLastUpdate["t8425"])
            {
                FmCom.wait?.TrySetResult(true);
                return;
            }

            t8425 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t8425.res"
            };
            t8425.ReceiveData += RcvT8425;

            t8425.SetFieldData("t8425InBlock", "dummy", 0, "");
            var result = t8425.Request(false);

            if (result < 0) Conf.ILog.Warning("[t8425] 테마전체조회 조회 실패 : " + Dic.Error[result]);
            
            Tool.Delay(1000);
        }

        static void RcvT8425(string trCode)
        {
            try
            {
                string trout = trCode + "OutBlock";
                int cnt = t8425.GetBlockCount(trout);

                StringBuilder sb = new StringBuilder(); 

                for (int i = 0; i < cnt; i++)
                {
                    string code = t8425.GetFieldData(trout, "tmcode", i);
                    string name = t8425.GetFieldData(trout, "tmname", i);

                    sb.Append(string.Format("INSERT INTO tb_meta_thema (code, name) VALUES ('{0}', '{1}') ON DUPLICATE KEY UPDATE name='{1}';\n", code, name));
                }

                // DB 저장
                DB.Execute(sb.ToString());

                TRUpdate("t8425");

                Conf.ILog.Information("[t8425] 전체 테마 조회 완료");
                FmCom.wait?.TrySetResult(true);
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region [T8424] 업종전체조회
        static XAQueryClass t8424;
        public static void ReqMetaUpjong()
        {
            if (Data.Today == TrLastUpdate["t8424"])
            {
                FmCom.wait?.TrySetResult(true);
                return;
            }

            t8424 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t8424.res"
            };
            t8424.ReceiveData += RcvT8424;

            t8424.SetFieldData("t8424InBlock", "gubun1", 0, "");
            var result = t8424.Request(false);

            if (result < 0)
            {
                Conf.ILog.Warning("[T8424] 업종전체조회 조회 실패 : " + Dic.Error[result]);
                return;
            }

            Tool.Delay(1000);
        }

        static void RcvT8424(string trCode)
        {
            try
            {
                string trout = trCode + "OutBlock";

                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < t8424.GetBlockCount(trout); i++)
                {
                    string code = t8424.GetFieldData(trout, "upcode", i);
                    string name = t8424.GetFieldData(trout, "hname", i);

                    sb.Append(string.Format("INSERT INTO tb_meta_upjong (code, name) VALUES ('{0}', '{1}') ON DUPLICATE KEY UPDATE name='{1}';\n", code, name));
                }

                DB.Execute(sb.ToString());

                TRUpdate("t8424");
                Conf.ILog.Information("[t8424] 전체 업종 조회 완료");
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
            finally
            {
                FmCom.wait?.TrySetResult(true);
            }
        }
        #endregion

        #region [t1516] 업종별 종목시세 조회        
        static XAQueryClass t1516;

        static bool next;
        static string key;

        public static async void ReqUpjongSise()
        {
            if (Data.Today == TrLastUpdate["t1516"])
            {
                FmCom.wait?.TrySetResult(true);
                return;
            }

            t1516 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t1516.res"
            };
            t1516.ReceiveData += RcvT1516;

            int idx = 0;
            for (int i = 1; i <= 2; i++)
            {
                next = true;
                key = "";

                while (next)
                {
                    t1516.SetFieldData("t1516InBlock", "upcode", 0, i == 1 ? "001" : "301");
                    t1516.SetFieldData("t1516InBlock", "gubun", 0, "");
                    t1516.SetFieldData("t1516InBlock", "shcode", 0, key);

                    tcs = new TaskCompletionSource<bool>();

                    var rst = t1516.Request(next);
                    Conf.ILog.Warning("[t1516] [" + rst + "] > [" + ++idx + "][" + key + "]");
                    if (rst < 0)
                    {
                        Conf.ILog.Warning("[t1516] 업종별 종목시세 조회 실패 > " + Dic.Error[rst]);
                        return;
                    }

                    await Task.WhenAny(tcs.Task, Task.Delay(10000));
                    Tool.Delay(4000);
                }
            }

            TRUpdate("t1516");

            Conf.ILog.Information("[t1516] 업종별 종목시세 조회 완료");
            FmCom.wait?.TrySetResult(true);
        }

        static void RcvT1516(string trCode)
        {
            try
            {
                string trout = trCode + "OutBlock";

                key = t1516.GetFieldData(trout, "shcode", 0);

                StringBuilder sb = new StringBuilder();

                // OutBlock1 - 업종별 종목시세 
                trout += "1";
                for (int i = 0; i < t1516.GetBlockCount(trout); i++)
                {
                    string code = t1516.GetFieldData(trout, "shcode", i);

                    Data.Items.TryGetValue(code, out Item im);

                    if (im == null) continue;

                    long price      = long.Parse(t1516.GetFieldData(trout, "price", i));    // 현재가
                    long open       = long.Parse(t1516.GetFieldData(trout, "open", i));     // 시가
                    long high       = long.Parse(t1516.GetFieldData(trout, "high", i));     // 고가
                    long low        = long.Parse(t1516.GetFieldData(trout, "low", i));      // 저가
                    float diff      = float.Parse(t1516.GetFieldData(trout, "diff", i));    // 전일대비
                    int sign        = int.Parse(t1516.GetFieldData(trout, "sign", i));      // 전일대비구분(1:상한,2:상승,3:보합,4:하한,5:하락)                
                    long volume     = long.Parse(t1516.GetFieldData(trout, "volume", i));   // 누적거래량
                    long value      = long.Parse(t1516.GetFieldData(trout, "value", i));    // 거래대금                                
                    long capital    = long.Parse(t1516.GetFieldData(trout, "total", i));    // 시가총액(억)                
                    float perx      = float.Parse(t1516.GetFieldData(trout, "perx", i));    // PER 

                    float sojinrate = float.Parse(t1516.GetFieldData(trout, "sojinrate", i));   // 소진율
                    long frgsvolume = long.Parse(t1516.GetFieldData(trout, "frgsvolume", i));   // 외인순매수
                    long orgsvolume = long.Parse(t1516.GetFieldData(trout, "orgsvolume", i));   // 기관순매수
                    float diffvol   = float.Parse(t1516.GetFieldData(trout, "diff_vol", i));    // 거래증가율
                    
                    sb.Append(string.Format("INSERT IGNORE INTO tb_stock_sise (date, code, close, open, high, low, diff, sign, volume, value, capital, perx, sojin_rate, frgs_volume, orgs_volume, diff_vol) " +
                        "VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}');\n",
                        Data.Today.ToString("yyyy-MM-dd"), code, price, open, high, low, diff, sign, volume, value, capital, perx, sojinrate, frgsvolume, orgsvolume, diffvol));
                }

                // DB 저장
                if (sb.Length > 0)
                {
                    DB.Execute(sb.ToString());                    
                }

                next = !string.IsNullOrEmpty(key);
                Conf.ILog.Information("[t1516] > [" + t1516.IsNext.ToString() + "] [" + key + "]");
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
            finally
            {
                tcs?.TrySetResult(true);
            }
        }
        #endregion

        #region [t1405] 투자주의
        static XAQueryClass t1405;
        static bool is_last = false;
        static string t1405_key = "";
        public static async void ReqStockWarning()
        {
            if (Data.Today == TrLastUpdate["t1405"])
            {
                FmCom.wait?.TrySetResult(true);
                return;
            }

            t1405 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t1405.res"
            };
            t1405.ReceiveData += RcvT1405;

            foreach (var kv in Dic.WarningType)
            {
                if (kv.Key == "8") is_last = true;

                t1405.SetFieldData("t1405InBlock", "gubun", 0, "0");
                t1405.SetFieldData("t1405InBlock", "jongchk", 0, kv.Key);
                t1405.SetFieldData("t1405InBlock", "cts_shcode", 0, "");

                t1405_key = kv.Key;
                Conf.ILog.Warning("[t1405] > [" + kv.Key + "][" + kv.Value + "]");

                tcs = new TaskCompletionSource<bool>();

                var rst = t1405.Request(false);
                if (rst < 0)
                {
                    Conf.ILog.Warning("[t1405] 투자주의 조회 실패 > " + Dic.Error[rst]);
                    return;
                }

                await Task.WhenAny(tcs.Task, Task.Delay(10000));
                Tool.Delay(2000);
            }
        }

        static void RcvT1405(string trCode)
        {
            string trout = trCode + "OutBlock1";
            StringBuilder sb = new StringBuilder();

            try
            {
                for (int i = 0; i < t1405.GetBlockCount(trout); i++)
                {
                    string code = t1405.GetFieldData(trout, "shcode", i);   // 종목코드     
                    sb.Append("UPDATE tb_stock_sise SET isalert='" + t1405_key + "' WHERE date='" + Data.Today.ToString("yyyy-MM-dd") + "' AND code='" + code + "';");
                }
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
            finally
            {
                if (sb.Length > 0) DB.Execute(sb.ToString());

                tcs?.TrySetResult(true);

                if (is_last && t1404 == null)
                {
                    Conf.ILog.Warning("[t1405] 투자주의 조회 완료");
                    TRUpdate("t1405");
                    FmCom.wait?.TrySetResult(true);
                }
            }
        }
        #endregion

        #region [t1404] 관리 종목
        static XAQueryClass t1404;
        public static void ReqStockManaging()
        {
            if (Data.Today == TrLastUpdate["t1404"])
            {
                FmCom.wait?.TrySetResult(true);
                return;
            }

            t1404 = new XAQueryClass
            {
                ResFileName = Conf.IConfig["ebest:res"] + "t1404.res"
            };
            t1404.ReceiveData += RcvT1404;

            t1404.SetFieldData("t1404InBlock", "gubun", 0, "0");
            t1404.SetFieldData("t1404InBlock", "jongchk", 0, "1");
            t1404.SetFieldData("t1404InBlock", "cts_shcode", 0, "");

            tcs = new TaskCompletionSource<bool>();

            var result = t1404.Request(false);
            if (result < 0) Conf.ILog.Warning("[t1404] 관리종목 조회 실패 : " + Dic.Error[result]);

            Tool.Delay(1000);
        }

        static void RcvT1404(string trCode)
        {
            string trout = trCode + "OutBlock1";

            StringBuilder sb = new StringBuilder();

            try
            {                
                for (int i = 0; i < t1404.GetBlockCount(trout); i++)
                {
                    string code     = t1404.GetFieldData(trout, "shcode", i);   // 종목코드                
                    string reason   = t1404.GetFieldData(trout, "reason", i);   // 사유

                    sb.Append("UPDATE tb_stock_sise SET ismanage='" + reason + "' WHERE date='" + Data.Today.ToString("yyyy-MM-dd") + "' AND code='" + code + "';");
                }
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
            finally
            {
                if (sb.Length > 0) DB.Execute(sb.ToString());

                Conf.ILog.Warning("[t1404] 관리종목 조회 완료");
                TRUpdate("t1404");
                FmCom.wait?.TrySetResult(true);
            }
        }
        #endregion
    }
}