using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartPOS.Data;
using SmartPOS.Models;
using SmartPOS.Shared.Enums;

namespace SmartPOS.Web.Data;

public static class DatabaseSeeder
{
     public static async Task SeedAsync(IServiceProvider serviceProvider)
     {
          using var scope = serviceProvider.CreateScope();
          using var db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext();

          var now = DateTime.UtcNow;

          // ── Roles ──────────────────────────────────────────────────────────
          if (!await db.Roles.AnyAsync())
          {
               db.Roles.AddRange(
                   new Role { Name = "Admin" },
                   new Role { Name = "Manager" },
                   new Role { Name = "Cashier" },
                   new Role { Name = "Customer" }
               );
               await db.SaveChangesAsync();
          }

          var adminRole    = await db.Roles.FirstAsync(r => r.Name == "Admin");
          var managerRole  = await db.Roles.FirstAsync(r => r.Name == "Manager");
          var cashierRole  = await db.Roles.FirstAsync(r => r.Name == "Cashier");
          var customerRole = await db.Roles.FirstAsync(r => r.Name == "Customer");

          // ── Staff Users (Admin / Manager / Cashier) ────────────────────────
          if (!await db.Users.AnyAsync(u => u.Email == "admin@smartpos.com"))
          {
               db.Users.Add(new User
               {
                    Name = "Admin",
                    Email = "admin@smartpos.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    RoleId = adminRole.Id,
                    IsActive = true,
                    CreatedAt = now
               });
               await db.SaveChangesAsync();
          }

          if (!await db.Users.AnyAsync(u => u.Email == "manager@smartpos.com"))
          {
               db.Users.AddRange(
                   new User { Name = "Maria Manager", Email = "manager@smartpos.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager@123"), RoleId = managerRole.Id,  IsActive = true, CreatedAt = now },
                   new User { Name = "Casey Cashier", Email = "cashier@smartpos.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Cashier@123"), RoleId = cashierRole.Id, IsActive = true, CreatedAt = now }
               );
               await db.SaveChangesAsync();
          }

          var sellerUser = await db.Users.FirstAsync(u => u.Email == "admin@smartpos.com");

          // ── Categories ─────────────────────────────────────────────────────
          if (!await db.Categories.AnyAsync())
          {
               db.Categories.AddRange(
                   new Category { Name = "Bakery",    Description = "Freshly baked breads and pastries" },
                   new Category { Name = "Beverages", Description = "Hot and cold drinks" },
                   new Category { Name = "Dairy",     Description = "Milk, cheese and yogurt" },
                   new Category { Name = "Snacks",    Description = "Chips, chocolate and treats" },
                   new Category { Name = "Produce",   Description = "Fresh fruits and vegetables" }
               );
               await db.SaveChangesAsync();
          }

          // ── Suppliers ──────────────────────────────────────────────────────
          if (!await db.Suppliers.AnyAsync())
          {
               db.Suppliers.AddRange(
                   new Supplier { Name = "Fresh Foods Co.",     ContactPerson = "Imran Khan",  ContactNo = "0300-1112233", Email = "sales@freshfoods.com",    Address = "12 Market Rd, Lahore",   IsActive = true },
                   new Supplier { Name = "Daily Beverages Ltd.", ContactPerson = "Sara Ahmed",  ContactNo = "0301-4445566", Email = "orders@dailybev.com",     Address = "5 Canal Bank, Lahore",   IsActive = true },
                   new Supplier { Name = "Sunrise Dairy",        ContactPerson = "Bilal Raza",  ContactNo = "0302-7778899", Email = "contact@sunrisedairy.com", Address = "88 Farm Lane, Kasur",   IsActive = true }
               );
               await db.SaveChangesAsync();
          }

          // ── Products + Inventory ───────────────────────────────────────────
          if (!await db.Products.AnyAsync())
          {
               var bakery    = await db.Categories.FirstAsync(c => c.Name == "Bakery");
               var beverages = await db.Categories.FirstAsync(c => c.Name == "Beverages");
               var dairy     = await db.Categories.FirstAsync(c => c.Name == "Dairy");
               var snacks    = await db.Categories.FirstAsync(c => c.Name == "Snacks");
               var produce   = await db.Categories.FirstAsync(c => c.Name == "Produce");

               var fresh = await db.Suppliers.FirstAsync(s => s.Name == "Fresh Foods Co.");
               var bev   = await db.Suppliers.FirstAsync(s => s.Name == "Daily Beverages Ltd.");
               var sun   = await db.Suppliers.FirstAsync(s => s.Name == "Sunrise Dairy");

               var products = new List<Product>
               {
                    new() { Name = "Butter Croissant",   SKU = "BKRY-001", Price = 150m, CostPrice = 80m,  CategoryId = bakery.Id,    SupplierId = fresh.Id, IsActive = true, CreatedAt = now, Description = "Flaky all-butter croissant" },
                    new() { Name = "Sourdough Loaf",      SKU = "BKRY-002", Price = 320m, CostPrice = 180m, CategoryId = bakery.Id,    SupplierId = fresh.Id, IsActive = true, CreatedAt = now, Description = "Naturally leavened sourdough" },
                    new() { Name = "Blueberry Muffin",    SKU = "BKRY-003", Price = 180m, CostPrice = 95m,  CategoryId = bakery.Id,    SupplierId = fresh.Id, IsActive = true, CreatedAt = now, Description = "Muffin loaded with blueberries" },
                    new() { Name = "Cappuccino (cup)",    SKU = "BEV-001",  Price = 250m, CostPrice = 110m, CategoryId = beverages.Id, SupplierId = bev.Id,   IsActive = true, CreatedAt = now, Description = "Espresso with steamed milk" },
                    new() { Name = "Green Tea",           SKU = "BEV-002",  Price = 120m, CostPrice = 45m,  CategoryId = beverages.Id, SupplierId = bev.Id,   IsActive = true, CreatedAt = now, Description = "Refreshing green tea" },
                    new() { Name = "Orange Juice 1L",     SKU = "BEV-003",  Price = 280m, CostPrice = 160m, CategoryId = beverages.Id, SupplierId = bev.Id,   IsActive = true, CreatedAt = now, Description = "Freshly squeezed orange juice" },
                    new() { Name = "Whole Milk 1L",       SKU = "DRY-001",  Price = 210m, CostPrice = 150m, CategoryId = dairy.Id,     SupplierId = sun.Id,   IsActive = true, CreatedAt = now, Description = "Full-cream milk" },
                    new() { Name = "Cheddar Cheese 250g", SKU = "DRY-002",  Price = 540m, CostPrice = 360m, CategoryId = dairy.Id,     SupplierId = sun.Id,   IsActive = true, CreatedAt = now, Description = "Aged cheddar" },
                    new() { Name = "Greek Yogurt 500g",   SKU = "DRY-003",  Price = 390m, CostPrice = 240m, CategoryId = dairy.Id,     SupplierId = sun.Id,   IsActive = true, CreatedAt = now, Description = "Thick strained yogurt" },
                    new() { Name = "Potato Chips",        SKU = "SNK-001",  Price = 130m, CostPrice = 70m,  CategoryId = snacks.Id,    SupplierId = fresh.Id, IsActive = true, CreatedAt = now, Description = "Salted potato chips" },
                    new() { Name = "Chocolate Bar",       SKU = "SNK-002",  Price = 160m, CostPrice = 90m,  CategoryId = snacks.Id,    SupplierId = fresh.Id, IsActive = true, CreatedAt = now, Description = "Milk chocolate bar" },
                    new() { Name = "Bananas (dozen)",     SKU = "PRD-001",  Price = 220m, CostPrice = 140m, CategoryId = produce.Id,   SupplierId = fresh.Id, IsActive = true, CreatedAt = now, Description = "Fresh bananas" },
                    new() { Name = "Apples (kg)",         SKU = "PRD-002",  Price = 300m, CostPrice = 190m, CategoryId = produce.Id,   SupplierId = fresh.Id, IsActive = true, CreatedAt = now, Description = "Crisp red apples" },
               };
               db.Products.AddRange(products);
               await db.SaveChangesAsync();

               var stock = new[] { 120, 60, 80, 200, 150, 90, 75, 40, 55, 110, 130, 70, 65 };
               var reorder = new[] { 20, 15, 20, 30, 25, 20, 20, 10, 15, 25, 25, 15, 15 };
               for (int i = 0; i < products.Count; i++)
               {
                    db.Inventories.Add(new Inventory
                    {
                         ProductId = products[i].Id,
                         Quantity = stock[i],
                         ReorderLevel = reorder[i],
                         LastUpdated = now
                    });
               }
               await db.SaveChangesAsync();
          }

          // ── Customers (each backed by a Customer-role User account) ─────────
          if (!await db.Customers.AnyAsync())
          {
               var seed = new[]
               {
                    new { Name = "Alice Johnson", Email = "alice@example.com", Phone = "0311-2223344", Addr = "House 4, Gulberg, Lahore",  Points = 120, Spent = 0m },
                    new { Name = "Bob Smith",     Email = "bob@example.com",   Phone = "0312-5556677", Addr = "Flat 9, DHA Phase 5, Lahore", Points = 45,  Spent = 0m },
                    new { Name = "Carol White",   Email = "carol@example.com", Phone = "0313-8889900", Addr = "21 Model Town, Lahore",      Points = 230, Spent = 0m },
                    new { Name = "David Lee",      Email = "david@example.com", Phone = "0314-1212121", Addr = "7 Johar Town, Lahore",       Points = 0,   Spent = 0m },
               };

               foreach (var s in seed)
               {
                    var user = new User
                    {
                         Name = s.Name,
                         Email = s.Email,
                         PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                         RoleId = customerRole.Id,
                         IsActive = true,
                         CreatedAt = now
                    };
                    db.Users.Add(user);
                    await db.SaveChangesAsync();

                    db.Customers.Add(new Customer
                    {
                         UserId = user.Id,
                         Name = s.Name,
                         Email = s.Email,
                         Phone = s.Phone,
                         Address = s.Addr,
                         IsActive = true,
                         LoyaltyPoints = s.Points,
                         TotalSpent = s.Spent,
                         CreatedAt = now
                    });
               }
               await db.SaveChangesAsync();
          }

          // ── Promotions ─────────────────────────────────────────────────────
          if (!await db.Promotions.AnyAsync())
          {
               var today = DateOnly.FromDateTime(now);
               db.Promotions.AddRange(
                   new Promotion { Code = "WELCOME10", Description = "10% off your order",        DiscountType = DiscountType.Percentage, Value = 10m, MinOrderValue = 0m,    MaxUsageLimit = null, UsageCount = 0, ValidFrom = today.AddMonths(-1), ValidTo = today.AddMonths(2),  IsActive = true,  CreatedAt = now },
                   new Promotion { Code = "FLAT50",    Description = "Rs.50 off orders over 500", DiscountType = DiscountType.Flat,       Value = 50m, MinOrderValue = 500m,  MaxUsageLimit = 100,  UsageCount = 12, ValidFrom = today.AddMonths(-1), ValidTo = today.AddMonths(1),  IsActive = true,  CreatedAt = now },
                   new Promotion { Code = "SUMMER20",  Description = "20% summer special",        DiscountType = DiscountType.Percentage, Value = 20m, MinOrderValue = 1000m, MaxUsageLimit = 50,   UsageCount = 50, ValidFrom = today.AddMonths(-3), ValidTo = today.AddDays(-5),   IsActive = false, CreatedAt = now }
               );
               await db.SaveChangesAsync();
          }

          // ── Sales + SaleItems + Payments ───────────────────────────────────
          if (!await db.Sales.AnyAsync())
          {
               var products = await db.Products.OrderBy(p => p.Id).ToListAsync();
               var customers = await db.Customers.OrderBy(c => c.Id).ToListAsync();
               const decimal taxRate = 0.15m;

               // (productIndex, qty) baskets, optional customer index, sale type, days ago
               var baskets = new[]
               {
                    new { Items = new[] { (0, 2), (3, 1) },        Cust = 0,  Type = SaleType.Onsite, DaysAgo = 1, Method = PaymentMethod.Cash },
                    new { Items = new[] { (1, 1), (6, 2), (9, 3) }, Cust = 1,  Type = SaleType.Onsite, DaysAgo = 2, Method = PaymentMethod.Card },
                    new { Items = new[] { (4, 4) },                Cust = -1, Type = SaleType.Onsite, DaysAgo = 3, Method = PaymentMethod.Cash },
                    new { Items = new[] { (7, 1), (8, 2) },        Cust = 2,  Type = SaleType.Online, DaysAgo = 4, Method = PaymentMethod.Online },
                    new { Items = new[] { (10, 5), (11, 1) },      Cust = 3,  Type = SaleType.Onsite, DaysAgo = 5, Method = PaymentMethod.Card },
                    new { Items = new[] { (2, 3), (5, 2), (12, 1) }, Cust = 0, Type = SaleType.Online, DaysAgo = 7, Method = PaymentMethod.Online },
               };

               foreach (var b in baskets)
               {
                    var sale = new Sale
                    {
                         UserId = sellerUser.Id,
                         CustomerId = b.Cust >= 0 ? customers[b.Cust].Id : null,
                         SaleType = b.Type,
                         SaleDate = now.AddDays(-b.DaysAgo),
                         Status = SaleStatus.Completed,
                         DiscountAmount = 0m
                    };
                    db.Sales.Add(sale);
                    await db.SaveChangesAsync();

                    decimal subtotal = 0m;
                    foreach (var (pi, qty) in b.Items)
                    {
                         var product = products[pi];
                         var line = product.Price * qty;
                         subtotal += line;
                         db.SaleItems.Add(new SaleItem
                         {
                              SaleId = sale.Id,
                              ProductId = product.Id,
                              Quantity = qty,
                              UnitPrice = product.Price,
                              LineTotal = line
                         });
                    }

                    sale.TaxAmount = Math.Round(subtotal * taxRate, 2);
                    sale.TotalAmount = subtotal + sale.TaxAmount - sale.DiscountAmount;

                    db.Payments.Add(new Payment
                    {
                         SaleId = sale.Id,
                         Method = b.Method,
                         Amount = sale.TotalAmount,
                         Status = PaymentStatus.Completed,
                         TransactionRef = $"TXN-{sale.SaleDate:yyyyMMdd}-{sale.Id:D4}",
                         PaidAt = sale.SaleDate
                    });

                    if (b.Cust >= 0)
                    {
                         var cust = customers[b.Cust];
                         cust.TotalSpent += sale.TotalAmount;
                         cust.LoyaltyPoints += (int)(sale.TotalAmount / 100m);
                    }

                    await db.SaveChangesAsync();
               }
          }

          // ── Reviews ────────────────────────────────────────────────────────
          if (!await db.Reviews.AnyAsync())
          {
               var products = await db.Products.OrderBy(p => p.Id).ToListAsync();
               var customers = await db.Customers.OrderBy(c => c.Id).ToListAsync();

               if (products.Count > 0 && customers.Count > 0)
               {
                    db.Reviews.AddRange(
                        new Review { CustomerId = customers[0].Id, ProductId = products[0].Id,  Rating = 5, Comment = "Best croissant in town, perfectly flaky!",      Sentiment = "Positive", SentimentScore = 0.96, CreatedAt = now.AddDays(-1) },
                        new Review { CustomerId = customers[1].Id, ProductId = products[3].Id,  Rating = 4, Comment = "Good cappuccino, could be a touch hotter.",       Sentiment = "Positive", SentimentScore = 0.74, CreatedAt = now.AddDays(-2) },
                        new Review { CustomerId = customers[2].Id, ProductId = products[7].Id,  Rating = 5, Comment = "The cheddar is rich and well aged. Love it.",      Sentiment = "Positive", SentimentScore = 0.91, CreatedAt = now.AddDays(-3) },
                        new Review { CustomerId = customers[3].Id, ProductId = products[9].Id,  Rating = 2, Comment = "Chips were a bit stale this time.",                Sentiment = "Negative", SentimentScore = 0.22, CreatedAt = now.AddDays(-4) },
                        new Review { CustomerId = customers[0].Id, ProductId = products[12].Id, Rating = 3, Comment = "Apples were okay, not very crisp.",                 Sentiment = "Neutral",  SentimentScore = 0.50, CreatedAt = now.AddDays(-5) }
                    );
                    await db.SaveChangesAsync();
               }
          }
     }
}
