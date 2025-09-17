using DataBaseAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ModelClasses.ViewModel;
using ModelClasses;
using Microsoft.EntityFrameworkCore;
using AKM.Mart.Utility;
using System.Security.Claims;

namespace AKM.Mart.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;



        public CartController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public async Task<IActionResult> CartIndex()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in User ID
            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Redirect to login if not found
            }

            var cartItems = await _context.UserCarts
                .Include(c => c.product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            var viewModel = new CartIndexVM
            {
                CartList = cartItems
            };

            return View(viewModel);
        }


        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, string? returnUrl)
        {
            var productAddToCArt = await _context.Products.FirstOrDefaultAsync(u => u.Id == productId);
            var checkIfUserSignInOrNot = _signInManager.IsSignedIn(User);
            if (checkIfUserSignInOrNot)
            {
                var user = _userManager.GetUserId(User);
                if (user != null)
                {
                    var getTheCartIfAnyExistForTheUser = await _context.UserCarts.Where(u => u.UserId.Contains(user)).ToListAsync();
                    if (getTheCartIfAnyExistForTheUser.Count() > 0)
                    {
                        var getTheQuantity = getTheCartIfAnyExistForTheUser.FirstOrDefault(p => p.ProductId == productId);
                        if (getTheQuantity != null)
                        {
                            getTheQuantity.Quantity = getTheQuantity.Quantity + 1;
                            _context.UserCarts.Update(getTheQuantity);
                        }
                        else
                        {
                            UserCart newItemToCart = new UserCart
                            {
                                ProductId = productId,
                                UserId = user,
                                Quantity = 1,
                            };
                            await _context.UserCarts.AddAsync(newItemToCart);
                        }
                    }
                    else
                    {
                        UserCart newItemToCart = new UserCart
                        {
                            ProductId = productId,
                            UserId = user,
                            Quantity = 1,
                        };
                        await _context.UserCarts.AddAsync(newItemToCart);
                    }
                    await _context.SaveChangesAsync();
                }

            }
            if (returnUrl != null)
            {
                return RedirectToAction("CartIndex", "Cart");
            }
            return RedirectToAction("Index", "Home");
        }

        public IActionResult MinusItem(int productId)
        {
            //get the item which we want to minus form our qantity
            var itemToMinus = _context.UserCarts.FirstOrDefault(u => u.ProductId == productId);
            if (itemToMinus != null)
            {
                if (itemToMinus.Quantity - 1 == 0)
                {
                    _context.UserCarts.Remove(itemToMinus);
                }
                else
                {
                    itemToMinus.Quantity -= 1;
                    _context.UserCarts.Update(itemToMinus);
                }
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(CartIndex));
        }


        public IActionResult DeleteItem(int productId)
        {
            //get the item which we want to minus form our qantity
            var itemToRemove = _context.UserCarts.FirstOrDefault(u => u.ProductId == productId);
            if (itemToRemove != null)
            {
                _context.UserCarts.Remove(itemToRemove);

                _context.SaveChanges();

            }
            return RedirectToAction(nameof(CartIndex));
        }
    }
}
