namespace MyShop1.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }  // Связь с заказом
        public Order? Order { get; set; }  // Навигационное свойство для связи с заказом

        public int ProductId { get; set; }  // Связь с продуктом
        public Product? Product { get; set; }  // Навигационное свойство для связи с продуктом

        public int Quantity { get; set; }  // Количество товара в заказе
    }

}