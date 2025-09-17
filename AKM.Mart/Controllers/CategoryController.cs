using DataBaseAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModelClasses;

namespace AKM.Mart.Controllers
{
    public class CategoryController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var items = _context.categories.ToList();
            return View(items);
        }

        [Authorize]
        public IActionResult Upsert(int? id)
        {

            if (id == 0)
            {
                Category category = new Category();
                return View(category);
            }
            else
            {
                var items = _context.categories.FirstOrDefault(u => u.Id == id);
                return View(items);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Upsert(int? id, Category category)
        {
            #region error
            //    if (id == null)
            //    {
            //        var foundItem = await _context.Categories.FirstOrDefaultAsync(u => u.Name == category.Name);
            //        if (foundItem != null)
            //        {
            //            category = foundItem;
            //            await _context.Categories.AddAsync(category);
            //            await _context.SaveChangesAsync();
            //            return RedirectToAction("Index");
            //        }
            //        else
            //        {

            //            await _context.Categories.AddAsync(category);
            //            await _context.SaveChangesAsync();
            //        }
            //        // await _context.Categories.AddAsync(category);

            //       // await _context.SaveChangesAsync();
            //        //return View(category);  

            //        return RedirectToAction("Index");
            //    }
            //    else
            //    {
            //        var items = await _context.Categories.FirstOrDefaultAsync(u => u.Id == id);

            //        items.Name = category.Name; 

            //    }
            //    await _context.SaveChangesAsync();
            //    return RedirectToAction("Index");
            //}
            #endregion error

            if (id == null || id == 0) // Create operation
            {
                var foundItem = await _context.categories.FirstOrDefaultAsync(u => u.Name == category.Name);
                if (foundItem != null)
                {
                    TempData["AlertMessage"] = category.Name + " this item already exist in the Category list";
                    // If the category already exists, redirect to Edit mode
                    return RedirectToAction("Upsert", new { id = foundItem.Id });
                }



                // If the category is new, add it
                await _context.categories.AddAsync(category);
                await _context.SaveChangesAsync();
                TempData["AlertMessage"] = category.Name + " has added into the Category list";
                return RedirectToAction("Index");
            }
            else
            {
                // Edit operation
                var existingItem = await _context.categories.FirstOrDefaultAsync(u => u.Id == id);
                if (existingItem == null)
                {
                    return NotFound(); // If item doesn't exist, return 404
                }

                existingItem.Name = category.Name;
                _context.categories.Update(existingItem); // Ensure EF Core tracks the changes
                await _context.SaveChangesAsync();

                TempData["AlertMessage"] = category.Name + " has Edited into the Category list";
                return RedirectToAction("Index");
            }



        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            var items = _context.categories.FirstOrDefault(u => u.Id == id);
            return View(items);


        }

        [HttpPost]

        public async Task<IActionResult> Delete(Category category)
        {
            var items = _context.categories.FirstOrDefault(u => u.Id == category.Id);
            _context.categories.Remove(items);

            TempData["AlertMessage"] = items.Name + " has been deleted from the Category list";

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");


        }
    }
}
