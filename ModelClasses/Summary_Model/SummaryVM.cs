using Microsoft.AspNetCore.Mvc.Rendering;
using ModelClasses.Orders;

namespace ModelClasses.Summary_Model
{
    public class SummaryVM
    {
        public IEnumerable<UserCart>? UserCartList { get; set; }

        public UserOrder? OrderSummary { get; set; }

        public string? CartUserId { get; set; }

        public IEnumerable<SelectListItem>? PaymentOptions { get; set; }

        public double? PaymentPaidbyCard { get; set; } = 0.0;

    }
}
