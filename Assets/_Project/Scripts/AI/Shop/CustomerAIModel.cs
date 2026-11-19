using System;
using RelicAndCoin.Inventory;
using RelicAndCoin.Shop;
using UnityEngine;

namespace RelicAndCoin.AI.Shop
{
    /// <summary>
    /// Pure C# model xử lý quyết định mua hàng của khách hàng:
    /// đánh giá giá niêm yết qua DynamicEconomyService, kiểm tra ngân sách tối đa
    /// và đếm ngược thời gian suy nghĩ trước khi mua.
    /// Không phụ thuộc Unity lifecycle.
    /// </summary>
    public sealed class CustomerAIModel
    {
        private readonly DynamicEconomyService _economyService;
        private readonly int _maxBudget;

        public CustomerState State { get; private set; } = CustomerState.Entering;
        public float ThinkTimeRemaining { get; private set; }
        public bool IsThinking => ThinkTimeRemaining > 0f;

        public CustomerAIModel(DynamicEconomyService economyService, int maxBudget)
        {
            _economyService = economyService ?? throw new ArgumentNullException(nameof(economyService));
            _maxBudget = maxBudget;
        }

        /// <summary>Bắt đầu đếm ngược thời gian suy nghĩ trước khi quyết định mua.</summary>
        public void StartThinking(float thinkDurationSeconds)
        {
            ThinkTimeRemaining = Mathf.Max(0f, thinkDurationSeconds);
        }

        /// <summary>Giảm thời gian suy nghĩ mỗi frame. Trả true khi hết thời gian suy nghĩ.</summary>
        public bool TickThinkTime(float deltaTime)
        {
            if (ThinkTimeRemaining <= 0f) return true;
            ThinkTimeRemaining -= deltaTime;
            return ThinkTimeRemaining <= 0f;
        }

        /// <summary>
        /// Quyết định có mua món hàng hay không dựa trên phản ứng giá và ngân sách.
        /// Trả về (có mua hay không, phản ứng giá của khách hàng).
        /// </summary>
        public (bool WillBuy, PriceReaction Reaction) DecidePurchase(
            ItemDataSO item,
            float listedPrice,
            ItemPopularity popularity)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var reaction = _economyService.EvaluatePrice(item, listedPrice, popularity);
            bool canAfford = listedPrice <= _maxBudget;

            // Khách hàng chỉ mua nếu đủ tiền và mức giá không quá đắt (Overpriced).
            bool willBuy = canAfford && reaction != PriceReaction.Overpriced;

            State = willBuy ? CustomerState.QueuingToPay : CustomerState.LeavingAngry;
            return (willBuy, reaction);
        }

        public void SetHappy() => State = CustomerState.LeavingHappy;
        public void SetAngry() => State = CustomerState.LeavingAngry;
        public void SetFleeing() => State = CustomerState.Fleeing;

        public bool IsFleeing => State == CustomerState.Fleeing;

        internal void SetState(CustomerState state) => State = state;
    }
}
