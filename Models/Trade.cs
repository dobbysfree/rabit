using System;

namespace Rabit.Models
{
    public class Trade
    {
        public string code { get; set; }
        public string name { get; set; }
        public long buy_prc { get; set; }
        public long sell_prc { get; set; }
        public long qty { get; set; }
        public double per { get; set; }
        public int tick { get; set; }
        public int mdfy_cnt { get; set; }
        public int cncl_cnt { get; set; }
        public long catch_ask { get; set; }
        public long catch_bid { get; set; }
        public DateTime start_time { get; set; }
        public DateTime end_time { get; set; }

        public long 익절정정가 { get; set; }
        public long 손절정정가 { get; set; }
        public long 청산정정가 { get; set; }
    }
}