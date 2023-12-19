using Rabit.Info;
using System;

namespace Rabit.Models
{
    public class Item
    {
        public string Code { get; set; }        // 종목코드
        public string Name { get; set; }        // 종목명
        public string Gubun { get; set; }       // 1:코스피, 2:코스닥   
        public long JnilClose { get; set; }     // 전일가
        public int Unit { get; set; }           // 호가단위
        public long UnitPrc { get; set; }       // 호가단위 변하는 가격 (매수진입 안함)

        public int TargetTick { get; set; }     // 목표 틱
        public int TargetQty { get; set; }      // 매매수량
          
        public DateTime RcvTime { get; set; }   // 체결시간

        public Hoga HogaAsk = new Hoga();       // 매도 호가

        public Hoga HogaBid = new Hoga();       // 매수 호가


        public EnumPositions CrntPos { get; set; }  // 포지션
        public long PosQty { get; set; }            // 보유 수량
        public int LossCnt { get; set; }            // 손실횟수
        public long TargetSellPrc { get; set; }     // 매도 가격
        public long TargetBuyPrc { get; set; }      // 매수 가격
        public TimeSpan TradeTime { get; set; }     // 거래시간
        

        public Exec Buying = new Exec();
        public Exec Selling = new Exec();
        public Market Info = new Market();      // 장마감후 분석용
    }
}