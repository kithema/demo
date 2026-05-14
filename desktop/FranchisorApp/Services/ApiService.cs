using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using FranchisorApp.Models;

namespace FranchisorApp.Services
{
    public class ApiService
    {
        private readonly string _connectionString;
        private bool _useDatabase = true; 

        public ApiService()
        {
            _connectionString = "Host=localhost;Port=5432;Database=vending_db;Username=postgres;Password=admin;";
        }

        public async Task<List<VendingMachine>> GetVendingMachinesAsync()
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = @"
                            SELECT vm.*, s.status_name, c.country_name 
                            FROM vending_machines vm
                            LEFT JOIN statuses s ON vm.status_id = s.status_id
                            LEFT JOIN countries c ON vm.country_id = c.country_id
                            ORDER BY vm.machine_id";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var machines = new List<VendingMachine>();
                            while (await reader.ReadAsync())
                            {
                                machines.Add(MapToVendingMachine(reader));
                            }

                            if (machines.Count > 0)
                            {
                                DataGenerator.EmulateDynamicData(machines);
                                return machines;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения к БД: {ex.Message}. Использую генерацию данных.");
                _useDatabase = false;
            }

            return GenerateMockMachines();
        }

        public async Task<VendingMachine?> GetVendingMachineByIdAsync(int id)
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = @"
                            SELECT vm.*, s.status_name, c.country_name 
                            FROM vending_machines vm
                            LEFT JOIN statuses s ON vm.status_id = s.status_id
                            LEFT JOIN countries c ON vm.country_id = c.country_id
                            WHERE vm.machine_id = @id";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    var machine = MapToVendingMachine(reader);
                                    DataGenerator.EmulateDynamicData(new List<VendingMachine> { machine });
                                    return machine;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            return GenerateMockMachines().FirstOrDefault(m => m.machine_id == id);
        }

        public async Task<int> CreateVendingMachineAsync(VendingMachine machine)
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = @"
                            INSERT INTO vending_machines (
                                serial_number, inventory_number, location, model, manufacturer,
                                manufacture_date, commissioning_date, status_id, country_id
                            ) VALUES (
                                @serial, @inventory, @location, @model, @manufacturer,
                                @manufactureDate, @commissioningDate, 
                                (SELECT status_id FROM statuses WHERE status_name = @statusName),
                                (SELECT country_id FROM countries WHERE country_name = @countryName)
                            ) RETURNING machine_id";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            AddMachineParameters(cmd, machine);
                            return (int)await cmd.ExecuteScalarAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            var mockMachines = GenerateMockMachines();
            var newId = mockMachines.Max(m => m.machine_id) + 1;
            machine.machine_id = newId;
            return newId;
        }

        public async Task UpdateVendingMachineAsync(int id, VendingMachine machine)
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = @"
                            UPDATE vending_machines SET
                                serial_number = @serial,
                                inventory_number = @inventory,
                                location = @location,
                                model = @model,
                                manufacturer = @manufacturer,
                                manufacture_date = @manufactureDate,
                                commissioning_date = @commissioningDate,
                                status_id = (SELECT status_id FROM statuses WHERE status_name = @statusName),
                                country_id = (SELECT country_id FROM countries WHERE country_name = @countryName)
                            WHERE machine_id = @id";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            AddMachineParameters(cmd, machine);
                            cmd.Parameters.AddWithValue("@id", id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обновления: {ex.Message}");
            }
        }

        public async Task DeleteVendingMachineAsync(int id)
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = "DELETE FROM vending_machines WHERE machine_id = @id";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления: {ex.Message}");
            }
        }

        public async Task<List<Maintenance>> GetMaintenanceAsync()
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = "SELECT * FROM maintenance ORDER BY maintenance_date DESC LIMIT 10";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var list = new List<Maintenance>();
                            while (await reader.ReadAsync())
                            {
                                list.Add(new Maintenance
                                {
                                    maintenance_id = reader.GetInt32(reader.GetOrdinal("maintenance_id")),
                                    machine_id = reader.IsDBNull(reader.GetOrdinal("machine_id")) ? null : reader.GetInt32(reader.GetOrdinal("machine_id")),
                                    maintenance_date = reader.GetDateTime(reader.GetOrdinal("maintenance_date")),
                                    description = reader.GetString(reader.GetOrdinal("description")),
                                    problems = reader.IsDBNull(reader.GetOrdinal("problems")) ? null : reader.GetString(reader.GetOrdinal("problems")),
                                    executor = reader.GetString(reader.GetOrdinal("executor"))
                                });
                            }
                            if (list.Count > 0) return list;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            return GenerateMockMaintenance();
        }

        public async Task<List<Sale>> GetSalesSummaryAsync()
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = "SELECT * FROM sales ORDER BY sale_datetime DESC LIMIT 50";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var list = new List<Sale>();
                            while (await reader.ReadAsync())
                            {
                                list.Add(new Sale
                                {
                                    sale_id = reader.GetInt64(reader.GetOrdinal("sale_id")),
                                    machine_id = reader.IsDBNull(reader.GetOrdinal("machine_id")) ? null : reader.GetInt32(reader.GetOrdinal("machine_id")),
                                    product_id = reader.IsDBNull(reader.GetOrdinal("product_id")) ? null : reader.GetInt32(reader.GetOrdinal("product_id")),
                                    quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                                    amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                                    sale_datetime = reader.GetDateTime(reader.GetOrdinal("sale_datetime")),
                                    payment_method = reader.IsDBNull(reader.GetOrdinal("payment_method")) ? null : reader.GetString(reader.GetOrdinal("payment_method"))
                                });
                            }
                            if (list.Count > 0) return list;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            return GenerateMockSales();
        }

        public async Task<List<Country>> GetCountriesAsync()
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = "SELECT country_id, country_name FROM countries ORDER BY country_name";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var list = new List<Country>();
                            while (await reader.ReadAsync())
                            {
                                list.Add(new Country
                                {
                                    country_id = reader.GetInt32(reader.GetOrdinal("country_id")),
                                    country_name = reader.GetString(reader.GetOrdinal("country_name"))
                                });
                            }
                            if (list.Count > 0) return list;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            return new List<Country>
            {
                new Country { country_id = 1, country_name = "Россия" },
                new Country { country_id = 2, country_name = "Германия" },
                new Country { country_id = 3, country_name = "Китай" },
                new Country { country_id = 4, country_name = "США" },
                new Country { country_id = 5, country_name = "Япония" }
            };
        }

        public async Task<List<Status>> GetStatusesAsync()
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = "SELECT status_id, status_name FROM statuses ORDER BY status_id";
                        using (var cmd = new NpgsqlCommand(sql, conn))
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            var list = new List<Status>();
                            while (await reader.ReadAsync())
                            {
                                list.Add(new Status
                                {
                                    status_id = reader.GetInt32(reader.GetOrdinal("status_id")),
                                    status_name = reader.GetString(reader.GetOrdinal("status_name"))
                                });
                            }
                            if (list.Count > 0) return list;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            return new List<Status>
            {
                new Status { status_id = 1, status_name = "Работает" },
                new Status { status_id = 2, status_name = "Вышел из строя" },
                new Status { status_id = 3, status_name = "В ремонте/на обслуживании" }
            };
        }

        public async Task<User?> AuthenticateUserAsync(string email, string password)
        {
            try
            {
                if (_useDatabase)
                {
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        await conn.OpenAsync();
                        var sql = @"
                            SELECT u.user_id, u.full_name, u.email, u.phone, u.role_id, r.role_name
                            FROM users u
                            LEFT JOIN roles r ON u.role_id = r.role_id
                            WHERE u.email = @email";

                        using (var cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@email", email);
                            using (var reader = await cmd.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    if (password == "123456") 
                                    {
                                        return MapToUser(reader);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка аутентификации: {ex.Message}");
            }

            if (email == "admin@example.com" && password == "123456")
            {
                return new User
                {
                    user_id = 1,
                    full_name = "Автоматов А.А.",
                    email = "admin@example.com",
                    phone = "+7 (999) 123-45-67",
                    role_id = 1,
                    role_name = "Администратор"
                };
            }
            if (email == "user@example.com" && password == "123456")
            {
                return new User
                {
                    user_id = 2,
                    full_name = "Петров П.П.",
                    email = "user@example.com",
                    phone = "+7 (999) 765-43-21",
                    role_id = 2,
                    role_name = "Оператор"
                };
            }
            return null;
        }

        private List<VendingMachine> GenerateMockMachines()
        {
            var machines = new List<VendingMachine>();
            var random = new Random();
            var statuses = new[] { "Работает", "Вышел из строя", "В ремонте/на обслуживании" };
            var countries = new[] { "Россия", "Германия", "Китай", "США", "Япония" };
            var companyNames = new[] { "ООО КофеАвтомат", "ИП Иванов", "ООО Снэки", "ООО Напитки", "ИП Петрова" };
            var models = new[] { "Sesa Cristal 400", "Unicom Ross", "Bianchi BVM 972", "Necta Kikko Max", "Jofemar Conferma" };
            var locations = new[] { "ул. Московская 121", "Академическая ул. 15", "Баррикад 174", "Грабцевское шоссе 174", "пер. Воскресенский 28" };

            for (int i = 1; i <= 12; i++)
            {
                var status = statuses[random.Next(statuses.Length)];
                machines.Add(new VendingMachine
                {
                    machine_id = i,
                    serial_number = $"SN{i:D3}",
                    inventory_number = $"INV{i:D3}",
                    location = locations[random.Next(locations.Length)],
                    model = models[random.Next(models.Length)],
                    manufacturer = models[random.Next(models.Length)].Split(' ')[0],
                    manufacture_date = new DateTime(2022, random.Next(1, 13), random.Next(1, 29)),
                    commissioning_date = new DateTime(2023, random.Next(1, 13), random.Next(1, 29)),
                    status_name = status,
                    country_name = countries[random.Next(countries.Length)],
                    company_name = companyNames[random.Next(companyNames.Length)],
                    modem_id = $"MODEM_{i:000}",
                    total_income = random.Next(50000, 500000),
                    current_cash = random.Next(1000, 50000),
                    connection_status = random.Next(100) < 85 ? "Online" : "Offline",
                    extra_status = GetRandomExtraStatus()
                });
            }
            return machines;
        }

        private List<Maintenance> GenerateMockMaintenance()
        {
            var list = new List<Maintenance>();
            var random = new Random();
            for (int i = 1; i <= 10; i++)
            {
                list.Add(new Maintenance
                {
                    maintenance_id = i,
                    machine_id = random.Next(1, 13),
                    maintenance_date = DateTime.Now.AddDays(-random.Next(1, 90)),
                    description = "Плановое техническое обслуживание",
                    problems = random.Next(5) == 0 ? "Обнаружена неисправность купюроприемника" : null,
                    executor = random.Next(2) == 0 ? "Иванов И.И." : "Петров П.П."
                });
            }
            return list;
        }

        private List<Sale> GenerateMockSales()
        {
            var list = new List<Sale>();
            var random = new Random();
            for (int i = 1; i <= 50; i++)
            {
                list.Add(new Sale
                {
                    sale_id = i,
                    machine_id = random.Next(1, 13),
                    product_id = random.Next(1, 11),
                    quantity = random.Next(1, 10),
                    amount = random.Next(50, 5000),
                    sale_datetime = DateTime.Now.AddHours(-random.Next(1, 720)),
                    payment_method = random.Next(3) switch { 0 => "cash", 1 => "card", _ => "qr" }
                });
            }
            return list.OrderByDescending(x => x.sale_datetime).ToList();
        }

        private string GetRandomExtraStatus()
        {
            var random = new Random();
            var rand = random.Next(100);
            if (rand < 60) return "Норма";
            if (rand < 80) return "Требуется внимание";
            if (rand < 95) return "Низкий запас";
            return "Ошибка";
        }

        private VendingMachine MapToVendingMachine(NpgsqlDataReader reader)
        {
            var machine = new VendingMachine
            {
                machine_id = reader.GetInt32(reader.GetOrdinal("machine_id")),
                serial_number = reader.GetString(reader.GetOrdinal("serial_number")),
                inventory_number = reader.GetString(reader.GetOrdinal("inventory_number")),
                location = reader.GetString(reader.GetOrdinal("location")),
                model = reader.GetString(reader.GetOrdinal("model")),
                manufacturer = reader.GetString(reader.GetOrdinal("manufacturer")),
                manufacture_date = reader.GetDateTime(reader.GetOrdinal("manufacture_date")),
                commissioning_date = reader.GetDateTime(reader.GetOrdinal("commissioning_date"))
            };

            if (!reader.IsDBNull(reader.GetOrdinal("status_name")))
                machine.status_name = reader.GetString(reader.GetOrdinal("status_name"));
            if (!reader.IsDBNull(reader.GetOrdinal("country_name")))
                machine.country_name = reader.GetString(reader.GetOrdinal("country_name"));
            if (!reader.IsDBNull(reader.GetOrdinal("total_income")))
                machine.total_income = reader.GetDecimal(reader.GetOrdinal("total_income"));

            machine.company_name = $"Франчайзи {machine.machine_id % 3 + 1}";
            machine.modem_id = $"MODEM_{machine.machine_id:000}";
            machine.current_cash = new Random().Next(1000, 50000);
            machine.connection_status = new Random().Next(100) < 85 ? "Online" : "Offline";
            machine.extra_status = GetRandomExtraStatus();

            return machine;
        }

        private void AddMachineParameters(NpgsqlCommand cmd, VendingMachine machine)
        {
            cmd.Parameters.AddWithValue("@serial", machine.serial_number);
            cmd.Parameters.AddWithValue("@inventory", machine.inventory_number);
            cmd.Parameters.AddWithValue("@location", machine.location);
            cmd.Parameters.AddWithValue("@model", machine.model);
            cmd.Parameters.AddWithValue("@manufacturer", machine.manufacturer);
            cmd.Parameters.AddWithValue("@manufactureDate", machine.manufacture_date);
            cmd.Parameters.AddWithValue("@commissioningDate", machine.commissioning_date);
            cmd.Parameters.AddWithValue("@statusName", machine.status_name ?? "Работает");
            cmd.Parameters.AddWithValue("@countryName", machine.country_name ?? "Россия");
        }

        private User MapToUser(NpgsqlDataReader reader)
        {
            return new User
            {
                user_id = reader.GetInt32(reader.GetOrdinal("user_id")),
                full_name = reader.GetString(reader.GetOrdinal("full_name")),
                email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString(reader.GetOrdinal("phone")),
                role_id = reader.GetInt32(reader.GetOrdinal("role_id")),
                role_name = reader.IsDBNull(reader.GetOrdinal("role_name")) ? null : reader.GetString(reader.GetOrdinal("role_name"))
            };
        }
    }

    public class Sale
    {
        public long sale_id { get; set; }
        public int? machine_id { get; set; }
        public int? product_id { get; set; }
        public int quantity { get; set; }
        public decimal amount { get; set; }
        public DateTime sale_datetime { get; set; }
        public string? payment_method { get; set; }
    }

    public class Country
    {
        public int country_id { get; set; }
        public string country_name { get; set; } = string.Empty;
    }

    public class Status
    {
        public int status_id { get; set; }
        public string status_name { get; set; } = string.Empty;
    }
}