using NLog;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using NorthwindConsole.Model;
using System.ComponentModel.DataAnnotations;
string path = Directory.GetCurrentDirectory() + "//nlog.config";

// create instance of Logger
var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();

logger.Info("Program started");

do
{
  Console.WriteLine("Northwind Console Application");
  Console.WriteLine("1) Product Management");
  Console.WriteLine("2) Category Management");
  Console.WriteLine("q) Quit");
  Console.Write("Enter your choice: ");
  string? choice = Console.ReadLine();
  Console.Clear();
  logger.Info($"Main menu option {choice} selected");

    if (choice == "1")
    {
        ProductMenu();
    }
    else if (choice == "2")
    {
        CategoryMenu();
    }
    else if (choice?.ToLower() == "q")
    {
        break;
    }
    else
    {
        logger.Warn("Invalid menu option selected");
        Console.WriteLine("Invalid choice. Please try again.\n");
    }
} while (true);

logger.Info("Program ended");

void ProductMenu()
{
    string? choice;
    do
    {
        Console.WriteLine("Product Management");
        Console.WriteLine("1) Add Product");
        Console.WriteLine("2) Edit Product");
        Console.WriteLine("3) Display All Products");
        Console.WriteLine("4) Display Specific Product");
        Console.WriteLine("5) Delete Product");
        Console.WriteLine("b) Back to Main Menu");
        Console.Write("Enter your choice: ");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Product menu option {choice} selected");

        if (choice == "1")
        {
            AddProduct();
        }
        else if (choice == "2")
        {
            EditProduct();
        }
        else if (choice == "3")
        {
            DisplayAllProducts();
        }
        else if (choice == "4")
        {
            DisplaySpecificProduct();
        }
        else if (choice == "5")
        {
            DeleteProduct();
        }
        else if (choice?.ToLower() == "b")
        {
            Console.Clear();
            break;
        }
        else
        {
            logger.Warn("Invalid product menu option selected");
            Console.WriteLine("Invalid choice. Please try again.\n");
        }
    } while (true);
}

void AddProduct()
{
    try
    {
        var db = new DataContext();
        Product product = new();
        
        Console.WriteLine("Add New Product");
        
        Console.Write("Enter Product Name: ");
        product.ProductName = Console.ReadLine()!;
        
        var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
        Console.WriteLine("\nAvailable Categories:");
        foreach (var cat in categories)
        {
            Console.WriteLine($"{cat.CategoryId}) {cat.CategoryName}");
        }
        Console.Write("Enter Category ID (or press Enter to skip): ");
        string? catInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(catInput) && int.TryParse(catInput, out int catId))
        {
            product.CategoryId = catId;
        }
        
        var suppliers = db.Suppliers.OrderBy(s => s.CompanyName).ToList();
        Console.WriteLine("\nAvailable Suppliers:");
        foreach (var sup in suppliers)
        {
            Console.WriteLine($"{sup.SupplierId}) {sup.CompanyName}");
        }
        Console.Write("Enter Supplier ID (or press Enter to skip): ");
        string? supInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(supInput) && int.TryParse(supInput, out int supId))
        {
            product.SupplierId = supId;
        }
        
        Console.Write("Enter Quantity Per Unit (or press Enter to skip): ");
        product.QuantityPerUnit = Console.ReadLine();
        
        Console.Write("Enter Unit Price (or press Enter to skip): ");
        string? priceInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal price))
        {
            product.UnitPrice = price;
        }
        
        Console.Write("Enter Units In Stock (or press Enter to skip): ");
        string? stockInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(stockInput) && short.TryParse(stockInput, out short stock))
        {
            product.UnitsInStock = stock;
        }
        
        Console.Write("Enter Units On Order (or press Enter to skip): ");
        string? orderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(orderInput) && short.TryParse(orderInput, out short order))
        {
            product.UnitsOnOrder = order;
        }
        
        Console.Write("Enter Reorder Level (or press Enter to skip): ");
        string? reorderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(reorderInput) && short.TryParse(reorderInput, out short reorder))
        {
            product.ReorderLevel = reorder;
        }
        
        Console.Write("Is this product discontinued? (y/n): ");
        string? discInput = Console.ReadLine();
        product.Discontinued = discInput?.ToLower() == "y";

        ValidationContext context = new ValidationContext(product, null, null);
        List<ValidationResult> results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(product, context, results, true);
        
        if (isValid)
        {
            if (db.Products.Any(p => p.ProductName.ToLower() == product.ProductName.ToLower()))
            {
                isValid = false;
                logger.Error($"Product name '{product.ProductName}' already exists");
                Console.WriteLine("\nError: A product with this name already exists!");
            }
            else
            {
                db.Products.Add(product);
                db.SaveChanges();
                logger.Info($"Product added: {product.ProductName}");
                Console.WriteLine("\nProduct added successfully!");
            }
        }
        
        if (!isValid && results.Count > 0)
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error adding product");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void EditProduct()
{
    try
    {
        var db = new DataContext();
        
        Console.WriteLine("Edit Product");
        Console.Write("Enter Product ID to edit: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            logger.Warn("Invalid Product ID entered for editing");
            Console.WriteLine("Invalid Product ID.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        var product = db.Products.FirstOrDefault(p => p.ProductId == productId);
        if (product == null)
        {
            logger.Warn($"Product ID {productId} not found for editing");
            Console.WriteLine("Product not found.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        Console.WriteLine($"\nEditing Product: {product.ProductName}");
        Console.WriteLine("(Press Enter to keep current value)\n");
        
        Console.Write($"Product Name [{product.ProductName}]: ");
        string? nameInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(nameInput))
        {
            product.ProductName = nameInput;
        }
        
        var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
        Console.WriteLine("\nAvailable Categories:");
        foreach (var cat in categories)
        {
            Console.WriteLine($"{cat.CategoryId}) {cat.CategoryName}");
        }
        Console.Write($"Category ID [{product.CategoryId}]: ");
        string? catInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(catInput) && int.TryParse(catInput, out int catId))
        {
            product.CategoryId = catId;
        }
        
        var suppliers = db.Suppliers.OrderBy(s => s.CompanyName).ToList();
        Console.WriteLine("\nAvailable Suppliers:");
        foreach (var sup in suppliers)
        {
            Console.WriteLine($"{sup.SupplierId}) {sup.CompanyName}");
        }
        Console.Write($"Supplier ID [{product.SupplierId}]: ");
        string? supInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(supInput) && int.TryParse(supInput, out int supId))
        {
            product.SupplierId = supId;
        }
        
        Console.Write($"Quantity Per Unit [{product.QuantityPerUnit}]: ");
        string? qtyInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(qtyInput))
        {
            product.QuantityPerUnit = qtyInput;
        }
        
        Console.Write($"Unit Price [{product.UnitPrice}]: ");
        string? priceInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(priceInput) && decimal.TryParse(priceInput, out decimal price))
        {
            product.UnitPrice = price;
        }
        
        Console.Write($"Units In Stock [{product.UnitsInStock}]: ");
        string? stockInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(stockInput) && short.TryParse(stockInput, out short stock))
        {
            product.UnitsInStock = stock;
        }
        
        Console.Write($"Units On Order [{product.UnitsOnOrder}]: ");
        string? orderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(orderInput) && short.TryParse(orderInput, out short order))
        {
            product.UnitsOnOrder = order;
        }
        
        Console.Write($"Reorder Level [{product.ReorderLevel}]: ");
        string? reorderInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(reorderInput) && short.TryParse(reorderInput, out short reorder))
        {
            product.ReorderLevel = reorder;
        }
        
        Console.Write($"Discontinued [{(product.Discontinued ? "Yes" : "No")}] (y/n): ");
        string? discInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(discInput))
        {
            product.Discontinued = discInput.ToLower() == "y";
        }

        ValidationContext context = new ValidationContext(product, null, null);
        List<ValidationResult> results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(product, context, results, true);
        
        if (isValid)
        {
            db.SaveChanges();
            logger.Info($"Product updated: {product.ProductName} (ID: {product.ProductId})");
            Console.WriteLine("\nProduct updated successfully!");
        }
        else
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error editing product");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void DisplayAllProducts()
{
    try
    {
        var db = new DataContext();
        
        Console.WriteLine("Display Products");
        Console.WriteLine("1) All Products");
        Console.WriteLine("2) Active Products Only");
        Console.WriteLine("3) Discontinued Products Only");
        Console.Write("Enter your choice: ");
        string? choice = Console.ReadLine();
        
        IQueryable<Product> query = db.Products.OrderBy(p => p.ProductName);
        
        string filterType = "All";
        if (choice == "2")
        {
            query = query.Where(p => !p.Discontinued);
            filterType = "Active";
        }
        else if (choice == "3")
        {
            query = query.Where(p => p.Discontinued);
            filterType = "Discontinued";
        }
        
        var products = query.ToList();
        
        logger.Info($"Displaying {filterType} products - {products.Count} records");
        
        Console.WriteLine($"\n{filterType} Products ({products.Count} found):");
        
        foreach (var product in products)
        {
            if (product.Discontinued)
            {
                Console.WriteLine($"{product.ProductName} [DISCONTINUED]");
            }
            else
            {
                Console.WriteLine($"{product.ProductName}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error displaying products");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void DisplaySpecificProduct()
{
    try
    {
        var db = new DataContext();
        
        Console.WriteLine("Display Specific Product");
        Console.Write("Enter Product ID: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            logger.Warn("Invalid Product ID entered for display");
            Console.WriteLine("Invalid Product ID.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        var product = db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefault(p => p.ProductId == productId);
            
        if (product == null)
        {
            logger.Warn($"Product ID {productId} not found");
            Console.WriteLine("Product not found.");
        }
        else
        {
            logger.Info($"Displaying product: {product.ProductName} (ID: {product.ProductId})");
            Console.WriteLine("");
            Console.WriteLine($"Product ID: {product.ProductId}");
            Console.WriteLine($"Product Name: {product.ProductName}");
            Console.WriteLine($"Category: {product.Category?.CategoryName ?? "N/A"}");
            Console.WriteLine($"Supplier: {product.Supplier?.CompanyName ?? "N/A"}");
            Console.WriteLine($"Quantity Per Unit: {product.QuantityPerUnit ?? "N/A"}");
            Console.WriteLine($"Unit Price: {(product.UnitPrice.HasValue ? $"${product.UnitPrice.Value:F2}" : "N/A")}");
            Console.WriteLine($"Units In Stock: {product.UnitsInStock?.ToString() ?? "N/A"}");
            Console.WriteLine($"Units On Order: {product.UnitsOnOrder?.ToString() ?? "N/A"}");
            Console.WriteLine($"Reorder Level: {product.ReorderLevel?.ToString() ?? "N/A"}");
            Console.WriteLine($"Discontinued: {(product.Discontinued ? "Yes" : "No")}");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error displaying specific product");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void DeleteProduct()
{
    try
    {
        var db = new DataContext();
        
        Console.WriteLine("Delete Product");
        Console.Write("Enter Product ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int productId))
        {
            logger.Warn("Invalid Product ID entered for deletion");
            Console.WriteLine("Invalid Product ID.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        var product = db.Products
            .Include(p => p.OrderDetails)
            .FirstOrDefault(p => p.ProductId == productId);
            
        if (product == null)
        {
            logger.Warn($"Product ID {productId} not found for deletion");
            Console.WriteLine("Product not found.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        Console.WriteLine($"\nProduct: {product.ProductName}");
        
        if (product.OrderDetails.Any())
        {
            Console.WriteLine($"\nWarning: This product has {product.OrderDetails.Count} related order detail(s).");
            Console.WriteLine("Deleting this product will also delete all related order details.");
        }
        
        Console.Write("\nAre you sure you want to delete this product? (y/n): ");
        string? confirm = Console.ReadLine();
        
        if (confirm?.ToLower() == "y")
        {
            if (product.OrderDetails.Any())
            {
                db.OrderDetails.RemoveRange(product.OrderDetails);
                logger.Info($"Removed {product.OrderDetails.Count} order detail(s) related to product {product.ProductName}");
            }
            
            db.Products.Remove(product);
            db.SaveChanges();
            logger.Info($"Product deleted: {product.ProductName} (ID: {product.ProductId})");
            Console.WriteLine("\nProduct deleted successfully!");
        }
        else
        {
            logger.Info($"Product deletion cancelled for: {product.ProductName}");
            Console.WriteLine("\nDeletion cancelled.");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error deleting product");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void CategoryMenu()
{
    string? choice;
    do
    {
        Console.WriteLine("Category Management");
        Console.WriteLine("1) Add Category");
        Console.WriteLine("2) Edit Category");
        Console.WriteLine("3) Display All Categories");
        Console.WriteLine("4) Display All Categories and their Products");
        Console.WriteLine("5) Display Specific Category and its Products");
        Console.WriteLine("6) Delete Category");
        Console.WriteLine("b) Back to Main Menu");
        Console.Write("Enter your choice: ");
        choice = Console.ReadLine();
        Console.Clear();
        logger.Info($"Category menu option {choice} selected");

        if (choice == "1")
        {
            AddCategory();
        }
        else if (choice == "2")
        {
            EditCategory();
        }
        else if (choice == "3")
        {
            DisplayAllCategories();
        }
        else if (choice == "4")
        {
            DisplayAllCategoriesWithProducts();
        }
        else if (choice == "5")
        {
            DisplaySpecificCategoryWithProducts();
        }
        else if (choice == "6")
        {
            DeleteCategory();
        }
        else if (choice?.ToLower() == "b")
        {
            Console.Clear();
            break;
        }
        else
        {
            logger.Warn("Invalid category menu option selected");
            Console.WriteLine("Invalid choice. Please try again.\n");
        }
    } while (true);
}

void AddCategory()
{
    try
    {
        var db = new DataContext();
        Category category = new();
        
        Console.WriteLine("Add New Category");
        
        Console.Write("Enter Category Name: ");
        category.CategoryName = Console.ReadLine()!;
        
        Console.Write("Enter Category Description (or press Enter to skip): ");
        category.Description = Console.ReadLine();

        ValidationContext context = new ValidationContext(category, null, null);
        List<ValidationResult> results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(category, context, results, true);
        
        if (isValid)
        {
            if (db.Categories.Any(c => c.CategoryName.ToLower() == category.CategoryName.ToLower()))
            {
                isValid = false;
                logger.Error($"Category name '{category.CategoryName}' already exists");
                Console.WriteLine("\nError: A category with this name already exists!");
            }
            else
            {
                db.Categories.Add(category);
                db.SaveChanges();
                logger.Info($"Category added: {category.CategoryName}");
                Console.WriteLine("\nCategory added successfully!");
            }
        }
        
        if (!isValid && results.Count > 0)
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error adding category");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void EditCategory()
{
    try
    {
        var db = new DataContext();
        
        Console.WriteLine("Edit Category");
        Console.Write("Enter Category ID to edit: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            logger.Warn("Invalid Category ID entered for editing");
            Console.WriteLine("Invalid Category ID.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        var category = db.Categories.FirstOrDefault(c => c.CategoryId == categoryId);
        if (category == null)
        {
            logger.Warn($"Category ID {categoryId} not found for editing");
            Console.WriteLine("Category not found.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        Console.WriteLine($"\nEditing Category: {category.CategoryName}");
        Console.WriteLine("(Press Enter to keep current value)\n");
        
        Console.Write($"Category Name [{category.CategoryName}]: ");
        string? nameInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(nameInput))
        {
            category.CategoryName = nameInput;
        }
        
        Console.Write($"Description [{category.Description}]: ");
        string? descInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(descInput))
        {
            category.Description = descInput;
        }

        ValidationContext context = new ValidationContext(category, null, null);
        List<ValidationResult> results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(category, context, results, true);
        
        if (isValid)
        {
            db.SaveChanges();
            logger.Info($"Category updated: {category.CategoryName} (ID: {category.CategoryId})");
            Console.WriteLine("\nCategory updated successfully!");
        }
        else
        {
            foreach (var result in results)
            {
                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                Console.WriteLine($"Error: {result.ErrorMessage}");
            }
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error editing category");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void DisplayAllCategories()
{
    try
    {
        var db = new DataContext();
        var categories = db.Categories.OrderBy(c => c.CategoryName).ToList();
        
        logger.Info($"Displaying all categories - {categories.Count} records");
        
        Console.WriteLine($"\nAll Categories ({categories.Count} found):\n");
        
        foreach (var category in categories)
        {
            Console.WriteLine($"{category.CategoryName} - {category.Description}");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error displaying categories");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void DisplayAllCategoriesWithProducts()
{
    try
    {
        var db = new DataContext();
        var categories = db.Categories
            .Include(c => c.Products.Where(p => !p.Discontinued))
            .OrderBy(c => c.CategoryName)
            .ToList();
        
        logger.Info($"Displaying all categories with active products");
        
        Console.WriteLine("\nAll Categories and their Active Products:\n");
        
        foreach (var category in categories)
        {
            Console.WriteLine($"{category.CategoryName}");
            if (category.Products.Any())
            {
                foreach (var product in category.Products.OrderBy(p => p.ProductName))
                {
                    Console.WriteLine($"  - {product.ProductName}");
                }
            }
            else
            {
                Console.WriteLine("  (No active products)");
            }
            Console.WriteLine();
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error displaying categories with products");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void DisplaySpecificCategoryWithProducts()
{
    try
    {
        var db = new DataContext();
        
        Console.WriteLine("Display Specific Category and its Products");
        Console.Write("Enter Category ID: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            logger.Warn("Invalid Category ID entered for display");
            Console.WriteLine("Invalid Category ID.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        var category = db.Categories
            .Include(c => c.Products.Where(p => !p.Discontinued))
            .FirstOrDefault(c => c.CategoryId == categoryId);
            
        if (category == null)
        {
            logger.Warn($"Category ID {categoryId} not found");
            Console.WriteLine("Category not found.");
        }
        else
        {
            logger.Info($"Displaying category: {category.CategoryName} (ID: {category.CategoryId})");
            Console.WriteLine($"\n{category.CategoryName}");
            Console.WriteLine($"Description: {category.Description}\n");
            Console.WriteLine("Active Products:");
            
            if (category.Products.Any())
            {
                foreach (var product in category.Products.OrderBy(p => p.ProductName))
                {
                    Console.WriteLine($"  - {product.ProductName}");
                }
            }
            else
            {
                Console.WriteLine("  (No active products)");
            }
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error displaying specific category with products");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}

void DeleteCategory()
{
    try
    {
        var db = new DataContext();
        
        Console.WriteLine("Delete Category");
        Console.Write("Enter Category ID to delete: ");
        if (!int.TryParse(Console.ReadLine(), out int categoryId))
        {
            logger.Warn("Invalid Category ID entered for deletion");
            Console.WriteLine("Invalid Category ID.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        var category = db.Categories
            .Include(c => c.Products)
            .ThenInclude(p => p.OrderDetails)
            .FirstOrDefault(c => c.CategoryId == categoryId);
            
        if (category == null)
        {
            logger.Warn($"Category ID {categoryId} not found for deletion");
            Console.WriteLine("Category not found.");
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
            return;
        }
        
        Console.WriteLine($"\nCategory: {category.CategoryName}");
        
        if (category.Products.Any())
        {
            Console.WriteLine($"\nWarning: This category has {category.Products.Count} related product(s).");
            int totalOrderDetails = category.Products.Sum(p => p.OrderDetails.Count);
            if (totalOrderDetails > 0)
            {
                Console.WriteLine($"These products have {totalOrderDetails} related order detail(s).");
            }
            Console.WriteLine("Deleting this category will:");
            Console.WriteLine("  1. Set CategoryId to NULL for all related products");
            Console.WriteLine("  2. Products will remain in the database");
        }
        
        Console.Write("\nAre you sure you want to delete this category? (y/n): ");
        string? confirm = Console.ReadLine();
        
        if (confirm?.ToLower() == "y")
        {
            foreach (var product in category.Products)
            {
                product.CategoryId = null;
            }
            
            db.Categories.Remove(category);
            db.SaveChanges();
            logger.Info($"Category deleted: {category.CategoryName} (ID: {category.CategoryId})");
            Console.WriteLine("\nCategory deleted successfully!");
        }
        else
        {
            logger.Info($"Category deletion cancelled for: {category.CategoryName}");
            Console.WriteLine("\nDeletion cancelled.");
        }
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Error deleting category");
        Console.WriteLine($"\nError: {ex.Message}");
    }
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}