using System.Collections.Generic;

namespace Rabit.Models
{
    public class Market
    {
        public int Close { get; set; }          // 종가
        public int Open { get; set; }           // 시가
        public int High { get; set; }           // 고가
        public int Low { get; set; }            // 저가
        public float Diff { get; set; }         // 등락율
        public int Sign { get; set; }           // 전일대비구분
        public long Volume { get; set; }        // 거래량
        public long Value { get; set; }         // 거래대금
        public long Capital { get; set; }       // 시가총액
        public float Perx { get; set; }
        public float SojinRate { get; set; } 
        public long FrgsVolume { get; set; }    // 외인순매수
        public long OrgsVolume { get; set; }    // 기관순매수
        public float DiffVol { get; set; }      // 거래증가율
        public string IsAlert { get; set; }     // 경고종목
        public string IsManage { get; set; }    // 관리종목

        public int IsTrade { get; set; }    // 특정종목 제외
        public int IsRunning { get; set; }  // 급등주&3일연속 상승 제외


        public List<Sise> Daily { get; set; } = new List<Sise>();
    }
}