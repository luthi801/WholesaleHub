using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WholesaleHub.Models;

namespace WholesaleHub.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();
            EnsureUserArchiveColumn(context);
            EnsurePaymentColumns(context);
            EnsureProductColumns(context);
            NormalizeInventoryStatuses(context);

            var passwordHasher = new PasswordHasher<User>();
            var existingUsers = context.Users.ToList();
            var upgradedUsers = existingUsers.Where(user => !user.Password.StartsWith("AQAAAA", StringComparison.Ordinal)).ToList();
            foreach (var user in upgradedUsers)
            {
                user.Password = passwordHasher.HashPassword(user, user.Password);
            }
            if (upgradedUsers.Count > 0) context.SaveChanges();
            if (existingUsers.Count > 0) return;

            var users = new User[]
            {
                new User { Name = "System Admin", UserName = "admin", Role = "Admin", Password = "password123" },
                new User { Name = "John Warehouse", UserName = "warehouse", Role = "Warehouse", Password = "password123" },
                new User { Name = "Alice Accountant", UserName = "accountant", Role = "Accountant", Password = "password123" },
                new User { Name = "Dela Cruz Store Owner", UserName = "delacruz", Role = "Customer", Password = "password123" }
            };
            foreach (var user in users)
            {
                user.Password = passwordHasher.HashPassword(user, user.Password);
            }
            context.Users.AddRange(users);
            context.SaveChanges();

            var suppliers = new Supplier[]
            {
                new Supplier { SupplierName = "Luzon Beverage Distributors Inc.", ContactPerson = "Ramon Cruz", Phone = "0917-555-0142", Email = "ramoncruz@luzonbev.ph", Address = "Km 18 East Service Rd, Muntinlupa" },
                new Supplier { SupplierName = "GreenField Grocery Supply Co.", ContactPerson = "Ana Manalac", Phone = "0918-354-9110", Email = "ana@greenfieldsupply.ph", Address = "Brgy. San Roque, Sta. Rosa, Laguna" },
                new Supplier { SupplierName = "Homecare Essentials Trading", ContactPerson = "Miguel Torres", Phone = "0919-255-8201", Email = "miguel@homecaretrading.ph", Address = "Manila Industrial Park, Bulacan" }
            };
            context.Suppliers.AddRange(suppliers);
            context.SaveChanges();

            var products = new Product[]
            {
                new Product { ProductName = "Mineral Water 500ml (24pk)", SKU = "BEV-0001", Category = "Beverages", UnitPrice = 120.00m, UnitOfMeasure = "box" },
                new Product { ProductName = "Instant Coffee 3-in-1 (30pk)", SKU = "BEV-0002", Category = "Beverages", UnitPrice = 185.50m, UnitOfMeasure = "box" },
                new Product { ProductName = "Refined Sugar 1kg", SKU = "GRO-0001", Category = "Grocery", UnitPrice = 75.00m, UnitOfMeasure = "pack" },
                new Product { ProductName = "Cooking Oil 1L", SKU = "GRO-0002", Category = "Grocery", UnitPrice = 90.00m, UnitOfMeasure = "bottle" },
                new Product { ProductName = "Dish Soap 500ml", SKU = "HOU-0001", Category = "Household", UnitPrice = 65.00m, UnitOfMeasure = "bottle" },
                new Product { ProductName = "Laundry Powder 1kg", SKU = "HOU-0002", Category = "Household", UnitPrice = 110.00m, UnitOfMeasure = "pack" },
                new Product { ProductName = "Assorted Biscuits (12pk)", SKU = "SNK-0001", Category = "Snacks", UnitPrice = 145.00m, UnitOfMeasure = "box" },
                new Product { ProductName = "Canned Corned Beef 150g", SKU = "CAN-0001", Category = "Grocery", UnitPrice = 58.00m, UnitOfMeasure = "can" }
            };
            context.Products.AddRange(products);
            context.SaveChanges();

            var inventories = new Inventory[]
            {
                new Inventory { ProductID = products[0].ProductID, QuantityOnHand = 120, ReorderLevel = 50 },
                new Inventory { ProductID = products[1].ProductID, QuantityOnHand = 15, ReorderLevel = 40 },
                new Inventory { ProductID = products[2].ProductID, QuantityOnHand = 200, ReorderLevel = 50 },
                new Inventory { ProductID = products[3].ProductID, QuantityOnHand = 10, ReorderLevel = 30 },
                new Inventory { ProductID = products[4].ProductID, QuantityOnHand = 0, ReorderLevel = 20 },
                new Inventory { ProductID = products[5].ProductID, QuantityOnHand = 75, ReorderLevel = 25 },
                new Inventory { ProductID = products[6].ProductID, QuantityOnHand = 50, ReorderLevel = 20 },
                new Inventory { ProductID = products[7].ProductID, QuantityOnHand = 180, ReorderLevel = 60 }
            };
            context.Inventories.AddRange(inventories);
            context.SaveChanges();

            var customers = new Customer[]
            {
                new Customer { UserID = users[3].UserID, CompanyName = "Dela Cruz Sari-Sari Store", ContactPerson = "Fe Dela Cruz", Email = "fedelacruz@gmail.com", Phone = "0917-555-1123", Address = "Poblacion, San Pedro, Laguna" },
                new Customer { CompanyName = "Manalo Mini Mart", ContactPerson = "Joel Manalo", Email = "joelmanalo@yahoo.com", Phone = "0922-333-2241", Address = "National Hwy, Calamba City" },
                new Customer { CompanyName = "Rivera Grocery & General Merchandise", ContactPerson = "Liza Rivera", Email = "lizarivera@outlook.com", Phone = "0918-444-7782", Address = "Brgy. Halang, Calamba, Laguna" }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();

            var po = new PurchaseOrder
            {
                SupplierID = suppliers[2].SupplierID,
                UserID = users[1].UserID,
                OrderDate = DateTime.Now.AddDays(-2),
                TotalAmount = 7760.00m,
                Status = "Pending"
            };
            context.PurchaseOrders.Add(po);
            context.SaveChanges();

            context.PurchaseOrderDetails.Add(new PurchaseOrderDetail
            {
                PurchaseOrderID = po.PurchaseOrderID,
                ProductID = products[4].ProductID,
                QuantityOrdered = 100,
                UnitCost = 50.00m,
                Subtotal = 5000.00m
            });
            context.PurchaseOrderDetails.Add(new PurchaseOrderDetail
            {
                PurchaseOrderID = po.PurchaseOrderID,
                ProductID = products[5].ProductID,
                QuantityOrdered = 30,
                UnitCost = 92.00m,
                Subtotal = 2760.00m
            });
            context.SaveChanges();

            var so = new SalesOrder
            {
                CustomerID = customers[0].CustomerID,
                OrderDate = DateTime.Now.AddDays(-1),
                TotalAmount = 3500.00m,
                OrderStatus = "Delivered"
            };
            context.SalesOrders.Add(so);
            context.SaveChanges();

            context.SalesOrderDetails.Add(new SalesOrderDetail
            {
                SalesOrderID = so.SalesOrderID,
                ProductID = products[0].ProductID,
                QuantityOrdered = 20,
                UnitPrice = 120.00m,
                Subtotal = 2400.00m
            });
            context.SalesOrderDetails.Add(new SalesOrderDetail
            {
                SalesOrderID = so.SalesOrderID,
                ProductID = products[5].ProductID,
                QuantityOrdered = 10,
                UnitPrice = 110.00m,
                Subtotal = 1100.00m
            });
            context.SaveChanges();

            var del = new Delivery
            {
                SalesOrderID = so.SalesOrderID,
                UserID = users[1].UserID,
                DeliveryDate = DateTime.Now,
                DeliveryStatus = "Delivered",
                TrackingNumber = "TRK-1002931"
            };
            context.Deliveries.Add(del);

            var ar = new AccountsReceivable
            {
                SalesOrderID = so.SalesOrderID,
                CustomerID = customers[0].CustomerID,
                TotalAmount = 3500.00m,
                AmountPaid = 1500.00m,
                PaymentStatus = "Partial"
            };
            context.AccountsReceivables.Add(ar);
            context.SaveChanges();

            context.CustomerPayments.Add(new CustomerPayment
            {
                AR_ID = ar.AR_ID,
                UserID = users[2].UserID,
                AmountPaid = 1500.00m,
                PaymentMethod = "GCash",
                PaymentDate = DateTime.Now
            });

            context.CustomerCrmInteractions.Add(new CustomerCrmInteraction
            {
                CustomerID = customers[0].CustomerID,
                UserID = users[0].UserID,
                InteractionType = "Phone Call",
                Notes = "Confirmed delivery schedule for weekly restock order.",
                InteractionDate = DateTime.Now.AddDays(-3)
            });

            context.AuditLogs.Add(new AuditLog
            {
                UserID = users[0].UserID,
                ActionPerformed = "System initialized and demo data seeded.",
                Timestamp = DateTime.Now
            });

            context.SaveChanges();
        }

        private static void EnsureUserArchiveColumn(ApplicationDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var openedHere = connection.State != System.Data.ConnectionState.Open;
            if (openedHere) connection.Open();

            try
            {
                using var command = connection.CreateCommand();
                if (context.Database.IsSqlite())
                {
                    command.CommandText = "PRAGMA table_info('Users')";
                    using var reader = command.ExecuteReader();
                    var hasColumn = false;
                    while (reader.Read())
                    {
                        if (string.Equals(reader.GetString(1), "IsArchived", StringComparison.OrdinalIgnoreCase))
                        {
                            hasColumn = true;
                            break;
                        }
                    }

                    if (!hasColumn)
                    {
                        reader.Close();
                        command.CommandText = "ALTER TABLE Users ADD COLUMN IsArchived INTEGER NOT NULL DEFAULT 0";
                        command.ExecuteNonQuery();
                    }
                }
                else
                {
                    command.CommandText = "IF COL_LENGTH('Users', 'IsArchived') IS NULL ALTER TABLE [Users] ADD [IsArchived] bit NOT NULL CONSTRAINT [DF_Users_IsArchived] DEFAULT 0";
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                if (openedHere) connection.Close();
            }
        }

        private static void EnsurePaymentColumns(ApplicationDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var openedHere = connection.State != System.Data.ConnectionState.Open;
            if (openedHere) connection.Open();

            try
            {
                using var command = connection.CreateCommand();

                if (context.Database.IsSqlite())
                {
                    AddSqliteColumnIfMissing(context, connection, "SalesOrders", "PaymentStatus", "TEXT NOT NULL DEFAULT 'Pending Payment'");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "SalesOrderID", "INTEGER NULL");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "CustomerID", "INTEGER NULL");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "TransactionReference", "TEXT NOT NULL DEFAULT ''");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "PaymentStatus", "TEXT NOT NULL DEFAULT 'Pending Payment'");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "CreatedAt", "TEXT NOT NULL DEFAULT '2000-01-01T00:00:00'");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "PaymentProofUrl", "TEXT NULL");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "Notes", "TEXT NULL");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "ProcessedBy", "TEXT NOT NULL DEFAULT ''");
                    AddSqliteColumnIfMissing(context, connection, "CustomerPayments", "AR_ID", "INTEGER NOT NULL DEFAULT 0");
                }
                else
                {
                    AddSqlServerColumnIfMissing(context, connection, "SalesOrders", "PaymentStatus", "NVARCHAR(50) NOT NULL CONSTRAINT DF_SalesOrders_PaymentStatus DEFAULT 'Pending Payment'");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "SalesOrderID", "INT NULL");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "CustomerID", "INT NULL");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "TransactionReference", "NVARCHAR(50) NOT NULL CONSTRAINT DF_CustomerPayments_TransactionReference DEFAULT ''");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "PaymentStatus", "NVARCHAR(30) NOT NULL CONSTRAINT DF_CustomerPayments_PaymentStatus DEFAULT 'Pending Payment'");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "CreatedAt", "DATETIME2 NOT NULL CONSTRAINT DF_CustomerPayments_CreatedAt DEFAULT GETUTCDATE()");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "PaymentProofUrl", "NVARCHAR(255) NULL");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "Notes", "NVARCHAR(500) NULL");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "ProcessedBy", "NVARCHAR(255) NOT NULL CONSTRAINT DF_CustomerPayments_ProcessedBy DEFAULT ''");
                    AddSqlServerColumnIfMissing(context, connection, "CustomerPayments", "AR_ID", "INT NOT NULL CONSTRAINT DF_CustomerPayments_AR_ID DEFAULT 0");
                }
            }
            finally
            {
                if (openedHere) connection.Close();
            }
        }

        private static void EnsureProductColumns(ApplicationDbContext context)
        {
            var connection = context.Database.GetDbConnection();
            var openedHere = connection.State != System.Data.ConnectionState.Open;
            if (openedHere) connection.Open();

            try
            {
                if (context.Database.IsSqlite())
                {
                    AddSqliteColumnIfMissing(context, connection, "Products", "Brand", "TEXT NOT NULL DEFAULT ''");
                    AddSqliteColumnIfMissing(context, connection, "Products", "ImageUrl", "TEXT NOT NULL DEFAULT ''");
                    AddSqliteColumnIfMissing(context, connection, "Products", "Description", "TEXT NOT NULL DEFAULT ''");
                    AddSqliteColumnIfMissing(context, connection, "Products", "MinimumOrderQuantity", "INTEGER NOT NULL DEFAULT 1");
                    AddSqliteColumnIfMissing(context, connection, "Products", "IsArchived", "INTEGER NOT NULL DEFAULT 0");
                    AddSqliteColumnIfMissing(context, connection, "Inventories", "WarehouseLocation", "TEXT NOT NULL DEFAULT ''");
                }
                else
                {
                    AddSqlServerColumnIfMissing(context, connection, "Products", "Brand", "NVARCHAR(80) NOT NULL CONSTRAINT DF_Products_Brand DEFAULT ''");
                    AddSqlServerColumnIfMissing(context, connection, "Products", "ImageUrl", "NVARCHAR(120) NOT NULL CONSTRAINT DF_Products_ImageUrl DEFAULT ''");
                    AddSqlServerColumnIfMissing(context, connection, "Products", "Description", "NVARCHAR(1000) NOT NULL CONSTRAINT DF_Products_Description DEFAULT ''");
                    AddSqlServerColumnIfMissing(context, connection, "Products", "MinimumOrderQuantity", "INT NOT NULL CONSTRAINT DF_Products_MinimumOrderQuantity DEFAULT 1");
                    AddSqlServerColumnIfMissing(context, connection, "Products", "IsArchived", "bit NOT NULL CONSTRAINT DF_Products_IsArchived DEFAULT 0");
                    AddSqlServerColumnIfMissing(context, connection, "Inventories", "WarehouseLocation", "NVARCHAR(100) NOT NULL CONSTRAINT DF_Inventories_WarehouseLocation DEFAULT ''");
                }
            }
            finally
            {
                if (openedHere) connection.Close();
            }
        }

        private static void NormalizeInventoryStatuses(ApplicationDbContext context)
        {
            var inventories = context.Inventories.ToList();
            foreach (var inventory in inventories)
            {
                inventory.Status = inventory.QuantityOnHand <= 0
                    ? "Out of Stock"
                    : inventory.QuantityOnHand <= inventory.ReorderLevel ? "Low Stock" : "In Stock";
            }

            if (inventories.Count > 0) context.SaveChanges();
        }

        private static void AddSqliteColumnIfMissing(ApplicationDbContext context, System.Data.Common.DbConnection connection, string tableName, string columnName, string columnDefinition)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info('{tableName}')";
            using var reader = command.ExecuteReader();
            var hasColumn = false;
            while (reader.Read())
            {
                if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                {
                    hasColumn = true;
                    break;
                }
            }

            if (!hasColumn)
            {
                using var alter = connection.CreateCommand();
                alter.CommandText = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}";
                alter.ExecuteNonQuery();
            }
        }

        private static void AddSqlServerColumnIfMissing(ApplicationDbContext context, System.Data.Common.DbConnection connection, string tableName, string columnName, string columnDefinition)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"IF COL_LENGTH('{tableName}', '{columnName}') IS NULL ALTER TABLE [dbo].[{tableName}] ADD [{columnName}] {columnDefinition}";
            command.ExecuteNonQuery();
        }
    }
}
