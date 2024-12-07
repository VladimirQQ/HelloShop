using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop1.Data;
using MyShop1.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop1.Controllers
{
    public class CategoriesController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        // Список категорий
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.ToListAsync();
            return View(categories);
        }

        // Страница добавления категории (GET)
        public IActionResult Create()
        {
            return View();
        }

        // Добавление категории в базу данных (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Проверка существования категории
                    var existingCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == category.Name);

                    if (existingCategory != null)
                    {
                        ModelState.AddModelError("", "Такая категория уже существует.");
                        return View(category);
                    }

                    // Логирование перед добавлением
                    Console.WriteLine($"Adding new category: {category.Name}");

                    _context.Categories.Add(category); // Добавляем категорию в базу данных
                    await _context.SaveChangesAsync(); // Сохраняем изменения
                    return RedirectToAction(nameof(Index)); // Перенаправление на список категорий
                }
                catch (Exception ex)
                {
                    // Логирование ошибки
                    ModelState.AddModelError("", $"Ошибка при добавлении категории: {ex.Message}");
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return View(category);
        }

        // Страница удаления категории (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // Подтверждение удаления категории (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            try
            {
                // Проверяем, есть ли связанные товары
                var associatedProducts = await _context.Products
                    .Where(p => p.CategoryId == id)
                    .ToListAsync();

                if (associatedProducts.Any())
                {
                    // Переносим товары в категорию "Неизвестная"
                    var unknownCategory = await _context.Categories
                        .FirstOrDefaultAsync(c => c.Name == "Неизвестная");

                    if (unknownCategory == null)
                    {
                        unknownCategory = new Category { Name = "Неизвестная" };
                        _context.Categories.Add(unknownCategory);
                        await _context.SaveChangesAsync();
                    }

                    // Переносим все связанные товары в категорию "Неизвестная"
                    foreach (var product in associatedProducts)
                    {
                        product.CategoryId = unknownCategory.Id;
                    }

                    await _context.SaveChangesAsync();
                }

                // Удаляем категорию
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index)); // Перенаправляем на страницу со списком категорий
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                ModelState.AddModelError("", $"Ошибка при удалении категории: {ex.Message}");
                return View(category);
            }
        }
    }
}
