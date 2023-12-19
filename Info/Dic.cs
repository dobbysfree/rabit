using System;
using System.Collections.Generic;

namespace Rabit.Info
{
    public class Dic
    {
        #region 요일
        public static Dictionary<DayOfWeek, int> Days = new Dictionary<DayOfWeek, int>()
        {
            { DayOfWeek.Monday, 1 },
            { DayOfWeek.Tuesday, 2 },
            { DayOfWeek.Wednesday, 3 },
            { DayOfWeek.Thursday, 4 },
            { DayOfWeek.Friday, 5 },
            { DayOfWeek.Saturday, 6 },
            { DayOfWeek.Sunday, 7 },
        };
        #endregion

        #region Market Type
        public static string KOSPI { get { return "1"; } }
        public static string KOSDAQ { get { return "2"; } }        

        public static Dictionary<string, string> Market = new Dictionary<string, string>()
        {
            { "1", "KOSPI" },
            { "2", "KOSDAQ" }
        };
        #endregion

        #region Order Type
        public static Dictionary<string, string> Chegyul = new Dictionary<string, string>()
        {
            { "01", "주문" },
            { "02", "정정" },
            { "03", "취소" },
            { "11", "체결" },
            { "12", "정정확인" },
            { "13", "취소확인" },
            { "14", "거부" },
            { "A1", "접수중" },
            { "AC", "접수완료" },
        };
        #endregion

        #region 주문절차
        // 1:포착매수요청, 1:매수접수, 2:매수체결, 3:매도요청, 4:매도접수, 5:매도체결
        public static Dictionary<string, int> OrderType = new Dictionary<string, int>()
        {
            { "01", 2 },
            { "11", 3 },
        };
        #endregion

        #region Buy Sell
        public static string Buy { get { return "2"; } }
        public static string Sell { get { return "1"; } }

        public static Dictionary<string, string> BuySell = new Dictionary<string, string>()
        {
            { "1", "SELL" },
            { "2", "BUY" }
        };
        #endregion
                
        #region Ebest Errors
        public static Dictionary<int, string> Error = new Dictionary<int, string>()
        {
            { -1, "소켓생성 실패"},
            { -2, "서버연결 실패" },
            { -3, "서버주소가 맞지 않습니다." },
            { -4, "서버 연결시간 초과" },
            { -5, "이미 서버에 연결중입니다." },
            { -6, "해당 TR은 사용할 수 없습니다." },
            { -7, "로그인이 필요합니다." },
            { -8, "시세전용에서는 사용이 불가능합니다." },
            { -9, "해당 계좌번호를 가지고 있지 않습니다." },
            { -10, "Packet의 크기가 잘못되었습니다." },
            { -11, "Data 크기가 다릅니다." },
            { -12, "계좌가 존재하지 않습니다." },
            { -13, "Request ID 부족" },
            { -14, "소켓이 생성되지 않았습니다." },
            { -15, "암호화 생성에 실패했습니다." },
            { -16, "데이터 전송에 실패했습니다." },
            { -17, "암호화(RTN) 처리에 실패했습니다." },
            { -18, "공인인증 파일이 없습니다." },
            { -19, "공인인증 Function이 없습니다." },
            { -20, "메모리가 충분하지 않습니다." },
            { -21, "TR의 초당 사용횟수 초과로 사용이 불가능합니다." },
            { -22, "해당 TR은 해당함수를 이용할 수 없습니다." },
            { -23, "TR에 대한 정보를 찾을 수 없습니다." },
            { -24, "계좌위치가 지정되지 않았습니다." },
            { -25, "계좌를 가지고 있지 않습니다." },
            { -26, "파일읽기에 실패했습니다. (종목검색조회시, 파일이없는경우)" },
            { -27, "실시간 종목검색 조건 등록이 10건을 초과하였습니다." },
            { -28, "등록 키에 대한 정보를 찾을 수 없습니다.(API->HTS 종목 연동키 오류" },
            { -34, "초당건수제한" }
        };
        #endregion
        
        #region 투자정보
        public static Dictionary<string, string> WarningType = new Dictionary<string, string>()
        {
            { "1", "투자경고" },
            { "2", "매매정지" },
            { "3", "정리매매" },
            { "4", "투자주의" },
            { "5", "투자위험" },
            { "6", "위험예고" },
            { "7", "단기과열지정" },
            { "8", "단기과열지정예고" },
        };
        #endregion

        #region 주문 실패 코드
        public static Dictionary<string, string> OrderRejectCode = new Dictionary<string, string>()
        {
            { "00000", "정상" },
            { "00156", "취소주문완료" },
            { "00039", "매도주문완료" },
            { "00040", "매수주문완료" },
            { "00132", "정정주문완료" },
            { "02661", "취소가능수량초과" },
            { "02257", "원주문가와 정정주문가가 동일하여 정정주문 불가능" },
            { "02705", "주문가격 잘못입력" },
            { "02714", "주문수량이 매매가능수량을 초과" },
            { "08677", "(가능수량:0)증거금 부족으로 주문이 불가" }
        };
        #endregion

        #region eapi 거부 trcode
        public static Dictionary<string, string> RejectTrCode = new Dictionary<string, string>()
        {
            { "SONAT000", "신규주문" },
            { "SONAT001", "정정주문" },
            { "SONAT002", "취소주문" },
            { "SONAS100", "체결확인" },
        };
        #endregion
    }
}