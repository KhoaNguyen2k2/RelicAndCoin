namespace RelicAndCoin.AI.Shop
{
    /// <summary>
    /// Trạng thái của khách hàng trong cửa hàng trong suốt một ngày bán hàng.
    /// </summary>
    public enum CustomerState
    {
        Entering,
        Browsing,
        InspectingItem,
        QueuingToPay,
        LeavingHappy,
        LeavingAngry,
        Fleeing
    }
}
