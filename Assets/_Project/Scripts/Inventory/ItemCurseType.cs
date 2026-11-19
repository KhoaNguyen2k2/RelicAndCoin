namespace RelicAndCoin.Inventory
{
    /// <summary>
    /// Loại lời nguyền gắn liền với vật phẩm, kích hoạt khi rời hầm ngục
    /// (hoặc ràng buộc đặt vị trí đối với EdgeOnly).
    /// </summary>
    public enum ItemCurseType
    {
        None,
        DestroyAdjacent,
        SendHome,
        EdgeOnly
    }
}
