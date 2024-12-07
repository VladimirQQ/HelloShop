namespace MyShop1.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        // Связь с продуктом
        public int ProductId { get; set; }
        public Product? Product { get; set; }  // Навигационное свойство для связи с продуктом

        // Количество товара в корзине
        public int Quantity { get; set; }
    }

}
