namespace Rabit.Models
{
    public class Report
    {
        public int 실매매종목수 { get; set; }
        public int 이익종목수 { get; set; }
        public int 손실종목수 { get; set; }
        public int 초당주문횟수 { get; set; }
        public int 매수요청수 { get; set; }
        public int 매도요청수 { get; set; }
        public int 취소요청수 { get; set; }
        public int 정정요청수 { get; set; }
        public int 미진입 { get; set; }
    }
}