// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using ProductInventoryDashboard.Models;
using System.Data.Odbc;
using System.Diagnostics;

namespace ProductInventoryDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        // Constructor to get the configuration from the dependency injection system
        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
            // Get the connection string once and store it
            _connectionString = _configuration.GetConnectionString("4D_DB")
                                ?? throw new InvalidOperationException("Connection string '4D_DB' not found.");
        }

        // Action for the main dashboard page (handles searching)
        public IActionResult Index(string searchQuery)
        {
            var products = new List<Product>();

            string sqlQuery = "SELECT ID, SKU, Name, Description, Price, StockLevel, Supplier FROM Products";

            if (!string.IsNullOrEmpty(searchQuery))
            {
                sqlQuery += " WHERE Name LIKE ? OR SKU LIKE ?";
            }

            try
            {
                using (var connection = new OdbcConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new OdbcCommand(sqlQuery, connection))
                    {
                        // Add the search query as a parameter to prevent SQL injection
                        if (!string.IsNullOrEmpty(searchQuery))
                        {
                            command.Parameters.AddWithValue("search", $"%{searchQuery}%");
                            command.Parameters.AddWithValue("searchSku", $"%{searchQuery}%");
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    Id = reader.GetInt32(0),
                                    SKU = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                                    Name = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
                                    Description = reader.IsDBNull(3) ? "No Description" : reader.GetString(3),
                                    Price = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                                    StockLevel = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                    Supplier = reader.IsDBNull(6) ? "N/A" : reader.GetString(6)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Database operation failed: {ex.Message}.";
            }

            ViewBag.CurrentFilter = searchQuery;
            return View(products);
        }

        // Action for the product details page
        public IActionResult Details(int id)
        {
            Product? product = null;
            string sqlQuery = "SELECT ID, SKU, Name, Description, Price, StockLevel, Supplier FROM Products WHERE ID = ?";

            try
            {
                using (var connection = new OdbcConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new OdbcCommand(sqlQuery, connection))
                    {
                        command.Parameters.AddWithValue("id", id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                product = new Product
                                {
                                    Id = reader.GetInt32(0),
                                    SKU = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                                    Name = reader.IsDBNull(2) ? "N/A" : reader.GetString(2),
                                    Description = reader.IsDBNull(3) ? "No Description" : reader.GetString(3),
                                    Price = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                                    StockLevel = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                                    Supplier = reader.IsDBNull(6) ? "N/A" : reader.GetString(6)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Database operation failed: {ex.Message}";
            }

            if (product == null)
            {
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View(product);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
