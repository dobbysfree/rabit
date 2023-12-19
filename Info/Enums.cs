namespace Rabit.Info
{
    public enum EnumPositions
    {
        None,           // None, 포착진입 가능 상태
        Waiting,        // 대기 상태
        OrdBuy,         // 매수요청, 포착진입한 상태
        OrdSell,        // 매도요청
        RcptBuy,        // 주문접수(매수)
        OngoingSell,    // 매도 포지션 상태 => 주문접수(매도) or 정정확인
        CollectLoss,    // 손절 정정
        DropOut         // 탈락(2회 손실)
    }

    public enum BuySell
    {
        Buy,
        Sell
    }
}