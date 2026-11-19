using System;
using RelicAndCoin.Core.Difficulty;
using RelicAndCoin.Inventory;

namespace RelicAndCoin.Shop
{
    /// <summary>
    /// Dịch vụ kinh tế động: tính toán mức giá chuẩn hiện tại của một vật phẩm
    /// dựa trên giá gốc, mức độ phổ biến và hệ số dificultad, đồng thời đánh giá
    /// mức giá người chơi đặt để trả về phản ứng của khách hàng.
    /// Pure C# class — không chứa business logic Unity lifecycle.
    /// </summary>
    public sealed class DynamicEconomyService
    {
        private readonly DifficultyConfigSO _difficultyConfig;

        // Hệ số điều chỉnh giá theo mức độ nhu cầu thị trường.
        private static readonly float[] PopularityMultipliers =
        {
            0.8f, // LowDemand  - giảm giá để khơng hồi hàng
            1.0f, // Normal
            1.2f  // HighDemand - có thể bán cao hơn
        };

        // Ngưỡng đánh giá phản ứng giá, tính theo tỷ lệ so với mức giá chuẩn.
        private const float CheapThresholdRatio   = 0.80f;
        private const float FairThresholdRatio    = 1.00f;
        private const float ExpensiveThresholdRatio = 1.25f;

        public DynamicEconomyService(DifficultyConfigSO difficultyConfig)
        {
            _difficultyConfig = difficultyConfig ?? throw new ArgumentNullException(nameof(difficultyConfig));
        }

        /// <summary>
        /// Tính mức giá chuẩn hiện tại của một món hàng.
        /// </summary>
        public float GetStandardPrice(ItemDataSO item, ItemPopularity popularity)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            float popularityMultiplier = GetPopularityMultiplier(popularity);
            return item.BaseValue * popularityMultiplier * _difficultyConfig.ShopPriceMultiplier;
        }

        /// <summary>
        /// Đánh giá mức giá người chơi đặt (ListedPrice) và trả về phản ứng dự kiến của khách hàng.
        /// </summary>
        public PriceReaction EvaluatePrice(ItemDataSO item, float listedPrice, ItemPopularity popularity)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (listedPrice < 0f) throw new ArgumentOutOfRangeException(nameof(listedPrice), "Giá niêm yết không được âm.");

            float standardPrice = GetStandardPrice(item, popularity);

            if (listedPrice <= standardPrice * CheapThresholdRatio)
                return PriceReaction.Cheap;
            if (listedPrice <= standardPrice * FairThresholdRatio)
                return PriceReaction.Fair;
            if (listedPrice <= standardPrice * ExpensiveThresholdRatio)
                return PriceReaction.Expensive;
            return PriceReaction.Overpriced;
        }

        private static float GetPopularityMultiplier(ItemPopularity popularity) =>
            popularity switch
            {
                ItemPopularity.LowDemand  => PopularityMultipliers[0],
                ItemPopularity.Normal     => PopularityMultipliers[1],
                ItemPopularity.HighDemand => PopularityMultipliers[2],
                _                         => PopularityMultipliers[1]
            };
    }
}
