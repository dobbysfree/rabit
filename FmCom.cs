using Akka.Actor;
using Rabit.Assist;
using Rabit.Comm;
using Rabit.Helpers;
using Rabit.Info;
using Rabit.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rabit
{
    public partial class FmCom : Form
    {
        #region Instance
        public static TaskCompletionSource<bool> wait;

        static Session ESess;
     
        public static FmCom it;
        #endregion

        #region Main
        public FmCom()
        {
            InitializeComponent();
            it = this;

            ESess = new Session();
        }
        #endregion

        async void DayTrading()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    DayTrading();
                });
                return;
            }

            // [CSPAQ12200] 예수금 조회
            wait = new TaskCompletionSource<bool>();
            TR.ReqDeposit();
            await Task.WhenAny(wait.Task, Task.Delay(1000 * 60));

            // [t0424] (장중) 보유 주식잔고 조회
            wait = new TaskCompletionSource<bool>();
            TR.ReqBalanceStocks();
            await Task.WhenAny(wait.Task, Task.Delay(1000 * 60));

            new TROrd();
        }

        #region 거래시작
        public async void Tradiing()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    Tradiing();
                });
                return;
            }

            // 종목 조회
            wait = new TaskCompletionSource<bool>();
            TR.ReqStock(0);
            await Task.WhenAny(wait.Task, Task.Delay(1000 * 60));

            DB.GetLastdayInfo();
            Conf.ILog.Information("종목수 > " + Data.Items.Count);

            // [CSPAQ12200] 예수금 조회
            wait = new TaskCompletionSource<bool>();
            TR.ReqDeposit();
            await Task.WhenAny(wait.Task, Task.Delay(1000 * 60));

            // [T0425] 미체결 내역 조회
            wait = new TaskCompletionSource<bool>();
            TR.ReqPending();
            await Task.WhenAny(wait.Task, Task.Delay(1000 * 100));

            // [t0424] (장중) 보유 주식잔고 조회
            wait = new TaskCompletionSource<bool>();
            TR.ReqBalanceStocks();
            await Task.WhenAny(wait.Task, Task.Delay(1000 * 60));

            new TROrd();

            Program.ActSys.ActorOf(Props.Create(() => new RTSiseMng()), "SiseMng");
        }
        #endregion

        #region 장마감전 청산
        public void ClearContract()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    ClearContract();
                });
                return;
            }

            var li = Data.Items.Values.Where(x => x.CrntPos == EnumPositions.RcptBuy || x.CrntPos == EnumPositions.OngoingSell || x.CrntPos == EnumPositions.CollectLoss);
            foreach (var im in li)
            {
                if (im.CrntPos == EnumPositions.RcptBuy)
                {
                    Conf.ILog.Information(string.Format("type:청산취소, code:{0}, name:{1}, qty:{2}, askprc:{3}, askrem:{4}, bidprc:{5}, bidrem:{6}, pos:{7}",
                        im.Code, im.Name, im.Buying.Qty, im.HogaAsk.price, im.HogaAsk.rem, im.HogaBid.price, im.HogaBid.rem, im.CrntPos));

                    TROrd.ReqCancelOrder(im, im.Buying.OrdNo, im.Buying.Qty);
                    //Program.ActSys.ActorSelection("user/SiseMng/" + im.Code).Tell(new Mail { Type = "cancel", OrdNo = im.Buying.OrdNo, OrdQty = im.Buying.Qty });
                }
                else if (im.CrntPos >= EnumPositions.OngoingSell)
                {
                    Conf.ILog.Information(string.Format("type:청산정정, code:{0}, name:{1}, qty:{2}, askprc:{3}, askrem:{4}, bidprc:{5}, bidrem:{6}, pos:{7}, side:SELL, prc:{5}",
                            im.Code, im.Name, im.Selling.Qty, im.HogaAsk.price, im.HogaAsk.rem, im.HogaBid.price, im.HogaBid.rem, im.CrntPos));

                    TROrd.ReqModifyOrder(im, im.Selling.OrdNo, im.HogaBid.price, im.Selling.Qty);
                    //Program.ActSys.ActorSelection("user/SiseMng/" + im.Code).Tell(new Mail { Type = "modify", OrdNo = con.Key, OrdPrc = im.HogaBid.price, OrdQty = con.Value });
                }
            }
        }
        #endregion

        #region 장마감후 업데이터
        public async void UpdateMarketData()
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    UpdateMarketData();
                });
                return;
            }

            try
            {
                TR.GetTRUpdate();

                // [t8436] 종목정보
                wait = new TaskCompletionSource<bool>();
                TR.ReqStock(1);
                await Task.WhenAny(wait.Task, Task.Delay(1000 * 10));

                // [t1533] 특이테마
                // 장마감일자 획득
                wait = new TaskCompletionSource<bool>();
                TR.ReqIssueThema();
                await Task.WhenAny(wait.Task, Task.Delay(1000 * 10));

                // [t8425] 전체테마
                wait = new TaskCompletionSource<bool>();
                TR.ReqMetaThema();
                await Task.WhenAny(wait.Task, Task.Delay(1000 * 10));

                // [T8424] 전체업종
                wait = new TaskCompletionSource<bool>();
                TR.ReqMetaUpjong();
                await Task.WhenAny(wait.Task, Task.Delay(1000 * 10));

                // [t1516] 업종 시세
                wait = new TaskCompletionSource<bool>();
                TR.ReqUpjongSise();
                await Task.WhenAny(wait.Task, Task.Delay(1000 * 60 * 20));

                // [t1405] 투자주의 조회
                wait = new TaskCompletionSource<bool>();
                TR.ReqStockWarning();
                await Task.WhenAny(wait.Task, Task.Delay(1000 * 60 * 10));

                // [t1404] 관리 조회
                wait = new TaskCompletionSource<bool>();
                TR.ReqStockManaging();
                await Task.WhenAny(wait.Task, Task.Delay(1000 * 60 * 10));

                Conf.ILog.Warning("It is done today");
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region After
        public async static void GetTrading()
        {
            wait = new TaskCompletionSource<bool>();
            TR.ReqStock(1);
            await Task.WhenAny(wait.Task, Task.Delay(1000 * 10));
        }
        #endregion

        #region Form Closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ESess != null)
            {
                Conf.ILog.Warning("It is closing");
                ClearContract();
                ESess.Logout();
            }
        }

        #endregion

        #region Open Folder
        private void btnFolder_Click(object sender, EventArgs e)
        {
            string DirPath = Application.StartupPath + "/logs";
            if (new DirectoryInfo(DirPath).Exists == true) Process.Start(DirPath);
        }
        #endregion
    }
}