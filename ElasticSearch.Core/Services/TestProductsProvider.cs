namespace ElasticSearch.Core.Services;

/// <summary>
/// Provider for test product data (approx. 250 products)
/// </summary>
public static class TestProductsProvider
{
    public static List<object> GetTestProducts()
    {
        var now = DateTime.Now;
        var products = new List<object>();

        // --- 10 Georgian Products (Exactly 10 as requested) ---
        products.Add(new { Name = "MacBook Pro 16\" M3", Description = "ძლიერი ლეპტოპი პროფესიონალებისთვის Apple M3 ჩიპით, 16GB RAM და 512GB SSD", Price = 4999.99m, Stock = 15, Category = "Laptops", Tags = "apple,laptop,macbook,premium,m3", Brand = "Apple", Model = "MacBook Pro 16\" 2024", CreatedDate = now.AddDays(-30), UpdatedAt = now.AddDays(-30), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "iPhone 15 Pro Max", Description = "უახლესი iPhone A17 Pro ჩიპით, 256GB, Titanium design", Price = 2799.99m, Stock = 50, Category = "Smartphones", Tags = "apple,iphone,smartphone,5g,premium", Brand = "Apple", Model = "iPhone 15 Pro Max", CreatedDate = now.AddDays(-20), UpdatedAt = now.AddDays(-20), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "Samsung Galaxy S24 Ultra", Description = "ფლაგმანი სმარტფონი Samsung-დან, Snapdragon 8 Gen 3, 512GB", Price = 2399.99m, Stock = 35, Category = "Smartphones", Tags = "samsung,galaxy,smartphone,android,premium", Brand = "Samsung", Model = "Galaxy S24 Ultra", CreatedDate = now.AddDays(-18), UpdatedAt = now.AddDays(-18), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "Sony WH-1000XM5", Description = "პრემიუმ უკაბელო ყურსასმენი შესანიშნავი ხმის დაბლოკვით", Price = 399.99m, Stock = 100, Category = "Audio", Tags = "sony,headphones,wireless,noise-cancelling,premium", Brand = "Sony", Model = "WH-1000XM5", CreatedDate = now.AddDays(-15), UpdatedAt = now.AddDays(-15), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "iPad Pro 12.9\" M2", Description = "მძლავრი ტაბლეტი M2 ჩიპით, 256GB, Liquid Retina XDR დისპლეი", Price = 1899.99m, Stock = 20, Category = "Tablets", Tags = "apple,ipad,tablet,m2,premium", Brand = "Apple", Model = "iPad Pro 12.9\" 2023", CreatedDate = now.AddDays(-12), UpdatedAt = now.AddDays(-12), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "LG 55\" OLED C3", Description = "პრემიუმ OLED ტელევიზორი 4K, 120Hz, WebOS Smart TV", Price = 1799.99m, Stock = 12, Category = "TVs", Tags = "lg,tv,oled,4k,smart-tv", Brand = "LG", Model = "OLED55C3", CreatedDate = now.AddDays(-10), UpdatedAt = now.AddDays(-10), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "Logitech MX Master 3S", Description = "ერგონომიული უკაბელო მაუსი პროფესიონალებისთვის", Price = 99.99m, Stock = 150, Category = "Accessories", Tags = "logitech,mouse,wireless,ergonomic,productivity", Brand = "Logitech", Model = "MX Master 3S", CreatedDate = now.AddDays(-8), UpdatedAt = now.AddDays(-8), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "HP LaserJet Pro M404dn", Description = "სამუშაო პრინტერი ოფისისთვის, ბეჭდვის სიჩქარე 40 გვერდი/წუთში", Price = 299.99m, Stock = 30, Category = "Printers", Tags = "hp,printer,laser,office,business", Brand = "HP", Model = "LaserJet Pro M404dn", CreatedDate = now.AddDays(-7), UpdatedAt = now.AddDays(-7), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "Kingston Fury 32GB DDR5", Description = "მაღალსიჩქარიანი RAM 5600MHz, 2x16GB Kit", Price = 149.99m, Stock = 80, Category = "Components", Tags = "kingston,ram,memory,ddr5,gaming", Brand = "Kingston", Model = "Fury Beast DDR5", CreatedDate = now.AddDays(-6), UpdatedAt = now.AddDays(-6), IsActive = true, IsDeleted = false });
        products.Add(new { Name = "ASUS ROG Strix RTX 4080", Description = "გეიმერული ვიდეოკარტა NVIDIA RTX 4080, 16GB GDDR6X", Price = 1599.99m, Stock = 8, Category = "Components", Tags = "asus,gpu,graphics-card,nvidia,gaming", Brand = "ASUS", Model = "ROG Strix RTX 4080", CreatedDate = now.AddDays(-4), UpdatedAt = now.AddDays(-4), IsActive = true, IsDeleted = false });

        // --- ~240 English Products ---
        var categories = new[] { "Laptops", "Smartphones", "Audio", "Tablets", "TVs", "Accessories", "Printers", "Components", "Storage", "Gaming" };
        var brands = new[] { "Apple", "Samsung", "Sony", "Dell", "HP", "Lenovo", "ASUS", "Logitech", "Microsoft", "Intel" };
        
        for (var i = 1; i <= 240; i++)
        {
            var category = categories[i % categories.Length];
            var brand = brands[i % brands.Length];
            var price = 50 + i * 15.5m % 2000;
            
            products.Add(new
            {
                Name = $"{brand} {category} Model {i}",
                Description = $"High-quality {category} from {brand}. This model {i} provides excellent performance and reliability for daily tasks.",
                Price = price,
                Stock = (i * 7) % 200,
                Category = category,
                Tags = $"{brand.ToLower()},{category.ToLower()},tech,model-{i}",
                Brand = brand,
                Model = $"M-{i:D3}",
                CreatedDate = now.AddDays(-(i % 60)),
                UpdatedAt = now.AddDays(-(i % 10)),
                IsActive = true,
                IsDeleted = false
            });
        }

        return products;
    }
}
