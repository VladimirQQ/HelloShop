using Microsoft.EntityFrameworkCore;
using MyShop1.Models;

namespace MyShop1.Data
{
    public class AppDbContext : DbContext
    {
        // DbSet для различных сущностей
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }  // Добавили DbSet для Order
        public DbSet<OrderItem> OrderItems { get; set; }  // Добавили DbSet для OrderItem

        // Конструктор, принимающий параметры конфигурации
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Конфигурация сущностей через Fluent API —
        // это набор методов для определения сопоставления между
        // классами и их свойствами, таблицами и их столбцами в Entity Framework.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Связь между OrderItem и Order (множество OrderItems к одному Order)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)  // Связь с заказом
                .WithMany(o => o.OrderItems)  // Один заказ может иметь несколько товаров
                .HasForeignKey(oi => oi.OrderId);  // Ключ связи

            // Связь между OrderItem и Product (множество OrderItems к одному Product)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)  // Связь с продуктом
                .WithMany(p => p.OrderItems)  // Один продукт может быть в нескольких заказах
                .HasForeignKey(oi => oi.ProductId);  // Ключ связи

            // Связь между Product и Category (каждому продукту принадлежит категория)
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)  // Каждый продукт имеет категорию (один к одному)
                .WithMany(c => c.Products)  // Одна категория может иметь несколько продуктов (один-ко-многим)
                .HasForeignKey(p => p.CategoryId);  // Связь по ключу CategoryId (метод в API сборки)

            base.OnModelCreating(modelBuilder);
        }
    }
}
