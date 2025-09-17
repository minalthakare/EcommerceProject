using AKM.Mart.Utility;
using DataBaseAccess;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelClasses.Orders;
using ModelClasses.Summary_Model;
using System.Threading.Tasks;

namespace AKM.Mart.Controllers
{
    public class OrderController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;




        public OrderController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult OrderDetail()
        {
            if (_signInManager.IsSignedIn(User))
            {
                var UserId = _userManager.GetUserId(User);
                var CurrentUser = _context.applicationUser.FirstOrDefault(x => x.Id == UserId);

                // Fetch User Cart List with filtering
                var userCartList = _context.UserCarts
                    .Where(u => u.UserId == UserId)
                    .Include(u => u.product)// Correct filtering
                    .ToList();

                SummaryVM summaryVM = new SummaryVM()
                {
                    UserCartList = userCartList,
                    OrderSummary = new UserOrder(),

                };

                if (CurrentUser != null)
                {
                    summaryVM.OrderSummary.DeliveryAddress = CurrentUser.Address;
                    summaryVM.OrderSummary.City = CurrentUser.City;
                    summaryVM.OrderSummary.State = CurrentUser.State;
                    summaryVM.OrderSummary.PostalCode = CurrentUser.PostalCode;
                    summaryVM.OrderSummary.PhoneNumber = CurrentUser.PhoneNumber;
                    summaryVM.OrderSummary.Name = $"{CurrentUser.FirstName} {CurrentUser.LastName}";
                }

                // Set session count for cart items
                var count = userCartList.Count;
                HttpContext.Session.SetInt32(CartCount.sessionCount, count);

                return View(summaryVM);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Summary(SummaryVM summaryVMFromview)
        {
            if (_signInManager.IsSignedIn(User))
            {
                var UserId = _userManager.GetUserId(User);
                var CurrentUser = _context.applicationUser.FirstOrDefault(x => x.Id == UserId);

                // Fetch User Cart List
                var userCartList = _context.UserCarts
                    .Where(u => u.UserId == UserId)
                    .Include(u => u.product)
                    .ToList();

                SummaryVM summaryVM = new SummaryVM()
                {
                    UserCartList = userCartList,
                    OrderSummary = new UserOrder(),
                };

                if (CurrentUser != null)
                {
                    summaryVM.OrderSummary.Name = summaryVMFromview.OrderSummary.Name;
                    summaryVM.OrderSummary.DeliveryAddress = summaryVMFromview.OrderSummary.DeliveryAddress;
                    summaryVM.OrderSummary.City = summaryVMFromview.OrderSummary.City;
                    summaryVM.OrderSummary.State = summaryVMFromview.OrderSummary.State;
                    summaryVM.OrderSummary.PostalCode = summaryVMFromview.OrderSummary.PostalCode;
                    summaryVM.OrderSummary.PhoneNumber = summaryVMFromview.OrderSummary.PhoneNumber;
                    summaryVM.OrderSummary.DateOfOrder = DateTime.Now;
                    summaryVM.OrderSummary.OrderStatus = "Pending";
                    summaryVM.OrderSummary.PaymentStatus = "Not Paid";

                    await _context.AddAsync(summaryVM.OrderSummary);
                    await _context.SaveChangesAsync();
                }

                if (summaryVMFromview.OrderSummary.TotalOrderAmount != null && summaryVMFromview.OrderSummary.TotalOrderAmount > 0)
                {
                    var CardChargeFee = (summaryVMFromview.OrderSummary.TotalOrderAmount / 100) * 2.90 + 0.30;
                    double creditCardBalance = 5000; // Replace with real payment API integration

                    if (creditCardBalance > summaryVMFromview.OrderSummary.TotalOrderAmount + CardChargeFee)
                    {
                        return RedirectToAction("OrderSuccess", new { id = summaryVM.OrderSummary.Id });
                    }
                    else
                    {
                        return RedirectToAction("OrderCancel");
                    }
                }
            }

            return View();
        }


        public IActionResult OrderSuccess(int id)
        {
            var UserId = _userManager.GetUserId(User);
            var UserCartRemove = _context.UserCarts.Where(u => u.UserId.Contains(UserId)).ToList();
            var OrderProcesed = _context.Orders.FirstOrDefault(h => h.Id == id);

            if (OrderProcesed != null)
            {
                if (OrderProcesed.PaymentStatus == "Not Paid")
                {
                    OrderProcesed.PaymentStatus = "Paid";
                    OrderProcesed.PaymentProccessDate = DateTime.Now;
                }

            }

            foreach (var order in UserCartRemove)
            {
                OrderDetails details = new OrderDetails()
                {
                    OrderId = OrderProcesed.Id,
                    ProductId = (int)order.ProductId,
                    Count = order.Quantity,
                };
                _context.Details.Add(details);
            }

            ViewBag.OrderId = id;
            _context.UserCarts.RemoveRange(UserCartRemove);
            _context.SaveChanges();
            var count = _context.UserCarts.Where(u => u.UserId.Contains(UserId)).ToList().Count;
            HttpContext.Session.SetInt32(CartCount.sessionCount, count);

            return View();
        }

        public IActionResult OrderCancel(int id)
        {
            var orderProcessCanceled = _context.Orders.FirstOrDefault(u => u.Id == id);
            _context.Orders.Remove(orderProcessCanceled);
            _context.SaveChanges();
            return RedirectToAction("CartIndex", "Cart");
        }

        //public IActionResult OrderHistory(string? status)
        //{
        //    var userId = _userManager.GetUserId(User);
        //    List<UserOrder> orderlist = new List<UserOrder>();

        //    if (status != null && status != "All")
        //    {
        //        if (User.IsInRole("Admin"))
        //        {
        //            orderlist = _context.Orders.Where(u => u.OrderStatus == status).ToList();

        //        }
        //        else
        //        {
        //            orderlist = _context.Orders.Where(u => u.OrderStatus == status && u.UserId == userId).ToList();

        //        }
        //    }


        //    else
        //    {
        //        if (User.IsInRole("Admin"))
        //        {
        //            orderlist = _context.Orders.ToList();

        //        }
        //        else
        //        {
        //            orderlist = _context.Orders.Where(u => u.UserId == userId).ToList();

        //        }
        //    }
        //    return View(orderlist);
        //}



        //    [HttpPost]
        //    [ValidateAntiForgeryToken]
        //    public async Task<IActionResult> Summary(SummaryVM summaryVMFromView)
        //    {
        //       string stripeKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
        //        if (_signInManager.IsSignedIn(User))
        //        {
        //            var UserId = _userManager.GetUserId(User);
        //            var CurrentUser = _context.applicationUser.FirstOrDefault(x => x.Id == UserId);

        //            // Fetch User Cart List
        //            var userCartList = _context.UserCarts
        //                .Where(u => u.UserId == UserId)
        //                .Include(u => u.product)
        //                .ToList();

        //            if (!userCartList.Any())
        //            {
        //                return RedirectToAction("CartIndex", "Cart"); // Redirect if cart is empty
        //            }

        //            // Create new order summary
        //            SummaryVM summaryVM = new SummaryVM()
        //            {
        //                UserCartList = userCartList,
        //                OrderSummary = new UserOrder(),
        //                CartUserId = UserId,

        //            };

        //            if (CurrentUser != null)
        //            {
        //                summaryVM.OrderSummary.Name = summaryVMFromView.OrderSummary.Name;
        //                summaryVM.OrderSummary.DeliveryAddress = summaryVMFromView.OrderSummary.DeliveryAddress;
        //                summaryVM.OrderSummary.City = summaryVMFromView.OrderSummary.City;
        //                summaryVM.OrderSummary.State = summaryVMFromView.OrderSummary.State;
        //                summaryVM.OrderSummary.PostalCode = summaryVMFromView.OrderSummary.PostalCode;
        //                summaryVM.OrderSummary.PhoneNumber = summaryVMFromView.OrderSummary.PhoneNumber;
        //                summaryVM.OrderSummary.DateOfOrder = DateTime.Now;
        //                summaryVM.OrderSummary.OrderStatus = "";
        //                summaryVM.OrderSummary.PaymentStatus = "";
        //                summaryVM.OrderSummary.UserId = summaryVMFromView.CartUserId;
        //                summaryVM.OrderSummary.TotalOrderAmount = summaryVMFromView.OrderSummary.TotalOrderAmount;
        //                summaryVM.OrderSummary.UserId = UserId; // Link order to user
        //            }

        //            // Save order to DB
        //            await _context.Orders.AddAsync(summaryVM.OrderSummary);
        //            await _context.SaveChangesAsync();

        //            Console.WriteLine($"Total Order Amount: {summaryVMFromView.OrderSummary.TotalOrderAmount}");

        //            // Payment Processing (Dummy Example)
        //            if (summaryVMFromView.OrderSummary.TotalOrderAmount > 0)
        //            {
        //                var options = new SessionCreateOptions
        //                {
        //                    SuccessUrl = "https://localhost:7271/Order/OrderSuccess/" + summaryVM.OrderSummary.Id,
        //                    CancelUrl = "https://localhost:7271/Order/OrderCancel/" + summaryVM.OrderSummary.Id,
        //                    LineItems = new List<SessionLineItemOptions>(),
        //                    Mode = "payment",
        //                };
        //                foreach (var item in summaryVM.UserCartList)
        //                {
        //                    var sessionLineItem = new SessionLineItemOptions
        //                    {
        //                        PriceData = new SessionLineItemPriceDataOptions
        //                        {
        //                            UnitAmount = (long)(item.product.Price * 100), // Only multiply if Price is decimal

        //                            Currency = "usd",
        //                            ProductData = new SessionLineItemPriceDataProductDataOptions
        //                            {
        //                                Name = item.product.Name,
        //                                Description = item.product.Description,

        //                            }
        //                        },
        //                        Quantity = item.Quantity,
        //                    };
        //                    options.LineItems.Add(sessionLineItem);
        //                }

        //                try
        //                {
        //                    var services = new SessionService();
        //                    Session session = services.Create(options);
        //                    Response.Headers.Add("Location", session.Url);
        //                    return new StatusCodeResult(303);
        //                }
        //                catch (StripeException ex)
        //                {
        //                    Console.WriteLine($"Stripe Error: {ex.Message}");
        //                    return RedirectToAction("PaymentError", "Order");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"Unexpected Error: {ex.Message}");
        //                    return RedirectToAction("PaymentError", "Order");
        //                }


        //            }
        //        }

        //        return RedirectToAction("Index", "Home");
        //    }



        //  




    }

}





