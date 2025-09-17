using ModelClasses.Orders;

namespace ModelClasses.ViewModel
{

    public class OrderDetailVM
    {
        public UserOrder? Orders { get; set; }
        public IEnumerable<OrderDetails> UserProductList { get; set; }
    }

}
