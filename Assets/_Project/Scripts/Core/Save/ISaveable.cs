namespace RelicAndCoin.Core.Save
{
    /// <summary>
    /// Implement interface này trên mọi hệ thống cần lưu/khôi phục trạng thái.
    /// T là kiểu data class tương ứng (nested trong SaveData hoặc class riêng).
    /// </summary>
    public interface ISaveable<T>
    {
        /// <summary>Trả về snapshot trạng thái hiện tại để ghi vào file.</summary>
        T CaptureState();

        /// <summary>Nhận snapshot từ file và khôi phục trạng thái.</summary>
        void RestoreState(T state);
    }
}
