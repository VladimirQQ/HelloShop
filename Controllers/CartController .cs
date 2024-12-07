using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop1.Data;
using MyShop1.Models;

namespace MyShop1.Controllers
{

    public class CartController(AppDbContext context) : Controller
    {
        private readonly AppDbContext _context = context;

        // Метод для отображения содержимого корзины
        public async Task<IActionResult> Index()
        {
            var cartItems = await _context.CartItems.Include(ci => ci.Product).ToListAsync();
            return View(cartItems); // Отображаем товары в корзине
        }
        // Метод для отображения страницы подтверждения удаления товара
        public async Task<IActionResult> Delete(int id)
        {
            var cartItem = await _context.CartItems.Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == id);
            if (cartItem == null)
            {
                return RedirectToAction("Index");
            }

            return View(cartItem);  // Отправляем товар в представление для подтверждения удаления
        }

        // Метод для фактического удаления товара из корзины
        [HttpPost]
        public async Task<IActionResult> RemoveFromCartConfirmed(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");  // Перенаправляем на страницу корзины
        }

        // Метод для добавления товара в корзину
        public async Task<IActionResult> AddToCart(int productId)
        {
            var product = await _context.Products.FindAsync(productId);

            if (product != null)
            {
                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.ProductId == productId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                    _context.CartItems.Update(existingCartItem);
                }
                else
                {
                    var cartItem = new CartItem { Product = product, Quantity = 1 };
                    _context.CartItems.Add(cartItem);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index"); // Перенаправляем на страницу корзины
        }

        // Метод для отображения формы оформления заказа (GET)
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cartItems = await _context.CartItems.Include(ci => ci.Product).ToListAsync();

            var order = new Order
            {
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Product = ci.Product
                }).ToList()
            };

            return View(order); // Отображаем форму оформления заказа
        }

        // Метод для обработки оформления заказа (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            if (ModelState.IsValid)  // Проверяем, что модель валидна
            {
                try
                {
                    // Сохраняем заказ в базе данных
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();

                    // Очищаем корзину только после оформления заказа
                    var cartItems = await _context.CartItems.ToListAsync();
                    _context.CartItems.RemoveRange(cartItems);
                    await _context.SaveChangesAsync();

                    // После успешного оформления заказа перенаправляем на страницу успешного оформления
                    return RedirectToAction("OrderSuccess");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Ошибка при оформлении заказа: {ex.Message}");
                }
            }

            return View(order);  // Если модель невалидна, возвращаем обратно на страницу оформления
        }


        // Метод для отображения страницы успешного оформления заказа
        public IActionResult OrderSuccess()
        {
            return View(); // Страница с сообщением об успешном оформлении заказа
        }
    }
}
