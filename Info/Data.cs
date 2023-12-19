using Rabit.Models;
using System;
using System.Collections.Generic;

namespace Rabit.Info
{
    public class Data
    {
        public static DateTime Today;


        // key : 종목코드, value : 종목정보
        public static Dictionary<string, Item> Items { get; set; } = new Dictionary<string, Item>();

        public static long Deposit { get; set; }

        // KEY : 주문TR요청의 RequestId | VALUE : MAX의 ORDER_ID(REQ_ID + ACTION_ID)
        public static Dictionary<int, string> ReqId_OrdId = new Dictionary<int, string>();


        // KEY : 주문확인응답의 OrdNo | VALUE : MAX의 ORDER_ID(REQ_ID + ACTION_ID)
        public static Dictionary<long, string> OrdNo_OrdId = new Dictionary<long, string>();

        public static Dictionary<string, Daily> DailyTradeItems { get; set; } = new Dictionary<string, Daily>(); 
    }
}