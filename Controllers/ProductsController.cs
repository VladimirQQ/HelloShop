using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyShop1.Data;
using MyShop1.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop1.Controllers
{
    public class ProductsController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        // Список товаров
        public async Task<IActionResult> Index()
        {
            // Получаем список всех товаров с категорией
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        // Форма добавления товара (GET-запрос)
        public IActionResult Create()
        {
            // Загружаем список категорий для выпадающего списка
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // Добавление товара в базу данных (POST-запрос)
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита от CSRF-атак
        public async Task<IActionResult> Create([Bind("Id,Name,Price,Description,CategoryId,NewCategoryName")] Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Обрабатываем категорию товара (если это новая категория, она будет добавлена)
                    await HandleCategoryAssignment(product);

                    _context.Add(product); // Добавляем товар в контекст базы данных
                    await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных
                    return RedirectToAction(nameof(Index)); // Перенаправляем на список товаров
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ошибка при добавлении товара: {ex.Message}");
                    // Логируем ошибку, если это необходимо
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            // В случае ошибки или неверной модели повторно загружаем список категорий и возвращаем форму
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product); // Отправляем обратно в форму, с ошибками
        }

        // Форма редактирования товара (GET-запрос)
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync/*метод возвращает первый элемент последовательности*/(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            // Загружаем список категорий для выпадающего списка
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.ExistingCategoryName = product.Category?.Name;
            return View(product);
        }

        // Редактирование товара в базе данных (POST-запрос)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Name,Price,Description,CategoryId,NewCategoryName")] Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await HandleCategoryAssignment(product); // Обрабатываем категорию товара

                    _context.Update(product); // Обновляем товар в контексте базы данных
                    await _context.SaveChangesAsync(); // Сохраняем изменения
                    return RedirectToAction(nameof(Index)); // Перенаправляем на список товаров
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", $"Ошибка при редактировании товара: {ex.Message}");
                }
            }

            // В случае ошибки или неверной модели повторно загружаем список категорий и возвращаем форму
            ViewBag.Categories = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            ViewBag.ExistingCategoryName = product.Category?.Name;
            return View(product); // Отправляем товар обратно в форму
        }

        // Удаление товара (страница подтверждения)
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // Подтверждение удаления товара
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Проверка существования товара с указанным id
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // Метод для обработки категории товара
        private async Task HandleCategoryAssignment(Product product)
        {
            if (!string.IsNullOrWhiteSpace(product.NewCategoryName)) // Если указано название новой категории
            {
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == product.NewCategoryName.Trim());

                if (existingCategory == null)
                {
                    var newCategory = new Category { Name = product.NewCategoryName.Trim() };
                    _context.Categories.Add(newCategory); // Добавляем категорию в контекст
                    await _context.SaveChangesAsync(); // Сохраняем изменения
                    product.CategoryId = newCategory.Id; // Присваиваем товару новый id категории
                }
                else
                {
                    product.CategoryId = existingCategory.Id; // Присваиваем товару id существующей категории
                }
            }
            else if (!product.CategoryId.HasValue) // Если категория не указана
            {
                // Проверяем, существует ли категория "Неизвестная"
                var unknownCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == "Неизвестная");

                if (unknownCategory == null)
                {
                    unknownCategory = new Category { Name = "Неизвестная" };
                    _context.Categories.Add(unknownCategory);
                    await _context.SaveChangesAsync();
                }

                product.CategoryId = unknownCategory.Id; // Присваиваем товару категорию "Неизвестная"
            }
        }
    }
}
