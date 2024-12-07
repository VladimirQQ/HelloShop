using System.ComponentModel.DataAnnotations.Schema;

namespace MyShop1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }

        // Поле для связи с категорией, может быть null
        public int? CategoryId { get; set; }
        public Category? Category { get; set; }
        [NotMapped]
        public string? NewCategoryName { get; set; }
        // Связь с заказами
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<CartItem>? CartItems { get; set; }  // Навигационное свойство для связи с корзинами

    }
}
