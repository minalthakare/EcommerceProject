using AKM.Mart.Utility;
using AKM.Mart.Models;
using DataBaseAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelClasses.ViewModel;
using System.Diagnostics;
using ModelClasses;


public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
        }

    public IActionResult Index(string? searchByName, string? searchByCategory)
    {
        if (_signInManager.IsSignedIn(User))
        {
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                var count = _context.UserCarts.Count(u => u.UserId == userId);
                HttpContext.Session.SetInt32(CartCount.sessionCount, count);
            }
        }

        // Initialize ViewModel
        HomePageVM vm = new HomePageVM();

        if (!string.IsNullOrEmpty(searchByName))
        {
            vm.ProductList = _context.Products
                .Where(product => EF.Functions.Like(product.Name, $"%{searchByName}%"))
                .ToList();
        }
        else if (!string.IsNullOrEmpty(searchByCategory))
        {
            var category = _context.categories.FirstOrDefault(u => u.Name == searchByCategory);
            if (category != null)
            {
                vm.ProductList = _context.Products.Where(u => u.CategoryId == category.Id).ToList();
            }
            else
            {
                vm.ProductList = new List<Product>(); // If category not found, return empty list
            }
        }
        else
        {
            vm.ProductList = _context.Products.ToList();
        }

        // Always load categories
        vm.CategoryList = _context.categories.ToList();

        return View(vm);
    }



    public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

