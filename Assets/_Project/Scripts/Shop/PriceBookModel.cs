using System;
using System.Collections.Generic;

namespace RelicAndCoin.Shop
{
    /// <summary>
    /// Sổ tay Thương nhân: lưu lịch sử mức giá người chơi từng thử cho từng ItemId,
    /// ghi nhận mức giá Fair cao nhất đã bán thành công và cập nhật mức độ phổ biến
    /// của từng món hàng.
    /// Pure C# class — không phụ thuộc Unity lifecycle.
    /// </summary>
    public sealed class PriceBookModel
    {
        // ItemId -> danh sách mức giá từng lần thử (để phân tích xu hướng sau này).
        private readonly Dictionary<string, List<float>> _priceHistory = new();

        // ItemId -> mức giá Fair cao nhất đã bán thành công.
        private readonly Dictionary<string, float> _highestFairSoldPrice = new();

        // ItemId -> mức độ phổ biến hiện tại.
        private readonly Dictionary<string, ItemPopularity> _popularity = new();

        /// <summary>Ghi nhận một lần người chơi thử niêm yết giá cho một món hàng.</summary>
        public void RecordPriceAttempt(string itemId, float listedPrice, PriceReaction reaction)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("ItemId không được rỗng.", nameof(itemId));
            if (listedPrice < 0f) throw new ArgumentOutOfRangeException(nameof(listedPrice), "Giá không được âm.");

            if (!_priceHistory.TryGetValue(itemId, out var history))
            {
                history = new List<float>();
                _priceHistory[itemId] = history;
            }
            history.Add(listedPrice);

            // Chỉ ghi nhận mức giá Fair cao nhất khi khách hàng thực sự mua (reaction Fair hoặc Cheap).
            if (reaction is PriceReaction.Fair or PriceReaction.Cheap)
            {
                if (!_highestFairSoldPrice.TryGetValue(itemId, out var highest) || listedPrice > highest)
                    _highestFairSoldPrice[itemId] = listedPrice;
            }
        }

        /// <summary>Trả về mức giá Fair cao nhất đã bán thành công của món hàng (0 nếu chưa có).</summary>
        public float GetHighestFairSoldPrice(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("ItemId không được rỗng.", nameof(itemId));
            return _highestFairSoldPrice.TryGetValue(itemId, out var price) ? price : 0f;
        }

        /// <summary>Cập nhật mức độ phổ biến của một món hàng.</summary>
        public void SetPopularity(string itemId, ItemPopularity popularity)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("ItemId không được rỗng.", nameof(itemId));
            _popularity[itemId] = popularity;
        }

        /// <summary>Trả về mức độ phổ biến hiện tại của một món hàng (mặc định Normal).</summary>
        public ItemPopularity GetPopularity(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("ItemId không được rỗng.", nameof(itemId));
            return _popularity.TryGetValue(itemId, out var popularity) ? popularity : ItemPopularity.Normal;
        }

        /// <summary>Trả về số lần đã thử niêm yết giá của một món hàng.</summary>
        public int GetAttemptCount(string itemId)
        {
            if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("ItemId không được rỗng.", nameof(itemId));
            return _priceHistory.TryGetValue(itemId, out var history) ? history.Count : 0;
        }
    }
}
