namespace MyShop1.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingAddress { get; set; }

        public List<OrderItem>? OrderItems { get; set; }  // Связь с товарами из корзины
    }
}