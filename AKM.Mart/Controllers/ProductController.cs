using DataBaseAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModelClasses;
using ModelClasses.ViewModel;
using System.Linq;

namespace AKM.Mart.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _HostEnvironment;

        public ProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _HostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            var products = _context.Products.Include(u => u.Category).ToList();

            return View(products);
        }

        public IActionResult Create()
        {
            ProductVM productsVM = new ProductVM()
            {
                Inventories = new Inventory(),
                PImages = new PImage(),
                CategoriesList = _context.categories.ToList().Select(u => new SelectListItem
                {

                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
            };
            return View(productsVM);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductVM productVM)
        {
            string homeImageUrl = "";
            if (productVM.Images != null)
            {
                foreach (var image in productVM.Images)
                {
                    homeImageUrl = image.FileName;
                    if (homeImageUrl.Contains("Home"))
                    {

                        homeImageUrl = UploadFiles(image);
                        break;
                    }
                }

            }
            productVM.Products.HomeImgUrl = homeImageUrl;
            await _context.AddAsync(productVM.Products);
            await _context.SaveChangesAsync();
            var NewProduct = await _context.Products.Include(u => u.Category).FirstOrDefaultAsync(u => u.Name == productVM.Products.Name);
            productVM.Inventories.Name = NewProduct.Name;
            productVM.Inventories.Category = NewProduct.Category.Name;
            await _context.Inventories.AddAsync(productVM.Inventories);
            await _context.SaveChangesAsync();

            if (productVM.Images != null)
            {
                foreach (var image in productVM.Images)
                {
                    string tempFileName = image.FileName;
                    if (!tempFileName.Contains("Home"))
                    {
                        string stringFileName = UploadFiles(image);
                        var addressImage = new PImage
                        {
                            ImageUrl = stringFileName,
                            ProductId = NewProduct.Id,
                            ProductName = NewProduct.Name,
                        };
                        await _context.PImages.AddAsync(addressImage);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Product");
        }


        [HttpGet]
        public IActionResult Edit(int Id)
        {
            ProductVM productsVM = new ProductVM()
            {
                Products = _context.Products.FirstOrDefault(p => p.Id == Id),

                CategoriesList = _context.categories.ToList().Select(u => new SelectListItem
                {

                    Text = u.Name,
                    Value = u.Id.ToString(),
                }),
            };
            productsVM.Products.ImgUrls = _context.PImages.Where(u => u.ProductId == Id).ToList();

            return View(productsVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductVM productVM)
        {
            var ProductToEdit = _context.Products.FirstOrDefault(u => u.Id == productVM.Products.Id);

            if (ProductToEdit != null)
            {
                // Update basic product details
                ProductToEdit.Name = productVM.Products.Name;
                ProductToEdit.Price = productVM.Products.Price;
                ProductToEdit.Description = productVM.Products.Description;
                ProductToEdit.CategoryId = productVM.Products.CategoryId;

                if (productVM.Images != null && productVM.Images.Any())
                {
                    foreach (var item in productVM.Images)
                    {
                        string tempFileName = item.FileName;

                        if (tempFileName.Contains("Home", StringComparison.OrdinalIgnoreCase)) // Ensure case-insensitive check
                        {
                            // Set Home Image if it hasn't been set before
                            if (string.IsNullOrWhiteSpace(ProductToEdit.HomeImgUrl))
                            {
                                ProductToEdit.HomeImgUrl = UploadFiles(item);
                            }
                        }
                        else
                        {
                            // Add as additional product image
                            string stringFileName = UploadFiles(item);
                            var addressImage = new PImage
                            {
                                ImageUrl = stringFileName,
                                ProductId = productVM.Products.Id,
                                ProductName = productVM.Products.Name
                            };

                            _context.PImages.Add(addressImage);
                        }
                    }
                }

                _context.Products.Update(ProductToEdit);
                _context.SaveChanges(); // Ensuring SaveChanges is only called once
            }

            return RedirectToAction("Index", "Product");
        }



        [HttpDelete]
        public IActionResult Delete(int Id)
        {
            if (Id != 0)
            {
                var ProductDelete = _context.Products.FirstOrDefault(x => x.Id == Id);
                var ImagesToDelete = _context.PImages.Where(u => u.ProductId == Id).Select(u => u.ImageUrl);
                foreach (var image in ImagesToDelete)
                {
                    string imageUrl = "Images\\" + image;
                    var toDeleteImageFromFolder = Path.Combine(_HostEnvironment.WebRootPath, imageUrl.TrimStart('\\'));
                    DeleteAImage(toDeleteImageFromFolder);
                }
                if (ProductDelete.HomeImgUrl != " ")
                {
                    string imageUrl = "Images\\" + ProductDelete.HomeImgUrl;
                    var toDeleteImageFromFolder = Path.Combine(_HostEnvironment.WebRootPath, imageUrl.TrimStart('\\'));
                    DeleteAImage(toDeleteImageFromFolder);

                }
                _context.Products.Remove(ProductDelete);
                _context.SaveChanges();
            }
            else
            {
                return Json(new { success = false, message = "Failed To Delete The Item" });
            }

            return Json(new { success = true, message = "Failed To Delete The Item" });
        }

        private void DeleteAImage(string toDeleteImageFromFolder)
        {
            if (System.IO.File.Exists(toDeleteImageFromFolder))
            {
                System.IO.File.Delete(toDeleteImageFromFolder);
            }
        }

        public IActionResult DeleteImg(string Id)
        {
            int routeId = 0;
            if (Id != null)
            {
                if (!Id.Contains("Home"))
                {
                    var ImagetoDeleteFromPImage = _context.PImages.FirstOrDefault(u => u.ImageUrl == Id);
                    if (ImagetoDeleteFromPImage != null)
                    {
                        routeId = ImagetoDeleteFromPImage.ProductId;
                        _context.PImages.Remove(ImagetoDeleteFromPImage);
                    }
                }
                else
                {
                    var ImageToDeletefromProduct = _context.Products.FirstOrDefault(u => u.HomeImgUrl == Id);
                    if (ImageToDeletefromProduct != null)
                    {
                        ImageToDeletefromProduct.HomeImgUrl = "";
                        routeId = ImageToDeletefromProduct.Id;
                        _context.Products.Update(ImageToDeletefromProduct);
                    }
                }
                string ImageUrl = "Images\\" + Id;
                var toDeleteImageFromFolder = Path.Combine(_HostEnvironment.WebRootPath, ImageUrl);
                DeleteAImage(toDeleteImageFromFolder);
                _context.SaveChanges();
                return Json(new { success = true, message = "Picture was deleted successfully", id = routeId });
            }
            return Json(new { success = false, message = "failed to delete the item. " });
        }

        private string UploadFiles(IFormFile image)
        {
            string fileName = null;
            if (image != null)
            {
                string uploadDirLocation = Path.Combine(_HostEnvironment.WebRootPath, "Images");
                fileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                string filePath = Path.Combine(uploadDirLocation, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }
            }

            return fileName;
        }

   


    }
}
