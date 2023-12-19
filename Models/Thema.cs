namespace Rabit.Models
{
    public class Thema
    {
        public int idx { get; set; }        // 순서
        public string code { get; set; }    // 테마코드
        public string name { get; set; }    // 테마명    
        public long totcnt { get; set; }    // 전체
        public long upcnt { get; set; }     // 상승
        public long dncnt { get; set; }     // 하락
        public float uprate { get; set; }   // 상승비율
        public float voldiff { get; set; }  // 거래증가율
        public float avgdiff { get; set; }  // 평균등락율
        public float chgdiff { get; set; }  // 대비등락율
    }
}