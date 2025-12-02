using Microsoft.Data.SqlClient;
using ExpenseManagement.Models;
using Azure.Identity;
using Azure.Core;

namespace ExpenseManagement.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DatabaseService> _logger;
        private readonly string? _managedIdentityClientId;

        public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _managedIdentityClientId = configuration["ManagedIdentityClientId"];
            
            var sqlServer = configuration["SqlServer"];
            var database = configuration["Database"];
            
            if (!string.IsNullOrEmpty(sqlServer) && !string.IsNullOrEmpty(database))
            {
                _connectionString = $"Server=tcp:{sqlServer};Database={database};Authentication=Active Directory Managed Identity;User Id={_managedIdentityClientId};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            }
            else
            {
                _connectionString = string.Empty;
            }
        }

        private async Task<SqlConnection> GetConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<List<Expense>> GetExpensesAsync(int? userId = null, int? statusId = null, int? categoryId = null, string? searchTerm = null)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.GetExpenses", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                command.Parameters.AddWithValue("@StatusId", (object?)statusId ?? DBNull.Value);
                command.Parameters.AddWithValue("@CategoryId", (object?)categoryId ?? DBNull.Value);
                command.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);

                var expenses = new List<Expense>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    expenses.Add(new Expense
                    {
                        ExpenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
                        StatusName = reader.GetString(reader.GetOrdinal("StatusName")),
                        AmountMinor = reader.GetInt32(reader.GetOrdinal("AmountMinor")),
                        AmountGBP = reader.GetDecimal(reader.GetOrdinal("AmountGBP")),
                        Currency = reader.GetString(reader.GetOrdinal("Currency")),
                        ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        ReceiptFile = reader.IsDBNull(reader.GetOrdinal("ReceiptFile")) ? null : reader.GetString(reader.GetOrdinal("ReceiptFile")),
                        SubmittedAt = reader.IsDBNull(reader.GetOrdinal("SubmittedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                        ReviewedBy = reader.IsDBNull(reader.GetOrdinal("ReviewedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ReviewedBy")),
                        ReviewerName = reader.IsDBNull(reader.GetOrdinal("ReviewerName")) ? null : reader.GetString(reader.GetOrdinal("ReviewerName")),
                        ReviewedAt = reader.IsDBNull(reader.GetOrdinal("ReviewedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ReviewedAt")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
                return expenses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expenses. ConnectionString configured: {HasConnectionString}, Error: {Message}", 
                    !string.IsNullOrEmpty(_connectionString), ex.Message);
                throw new Exception($"Database connection error at DatabaseService.GetExpensesAsync: {ex.Message}. Please ensure managed identity '{_managedIdentityClientId}' has proper permissions on the database.", ex);
            }
        }

        public async Task<Expense?> GetExpenseByIdAsync(int expenseId)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.GetExpenseById", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ExpenseId", expenseId);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new Expense
                    {
                        ExpenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
                        StatusName = reader.GetString(reader.GetOrdinal("StatusName")),
                        AmountMinor = reader.GetInt32(reader.GetOrdinal("AmountMinor")),
                        AmountGBP = reader.GetDecimal(reader.GetOrdinal("AmountGBP")),
                        Currency = reader.GetString(reader.GetOrdinal("Currency")),
                        ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        ReceiptFile = reader.IsDBNull(reader.GetOrdinal("ReceiptFile")) ? null : reader.GetString(reader.GetOrdinal("ReceiptFile")),
                        SubmittedAt = reader.IsDBNull(reader.GetOrdinal("SubmittedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                        ReviewedBy = reader.IsDBNull(reader.GetOrdinal("ReviewedBy")) ? null : reader.GetInt32(reader.GetOrdinal("ReviewedBy")),
                        ReviewerName = reader.IsDBNull(reader.GetOrdinal("ReviewerName")) ? null : reader.GetString(reader.GetOrdinal("ReviewerName")),
                        ReviewedAt = reader.IsDBNull(reader.GetOrdinal("ReviewedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("ReviewedAt")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expense by ID");
                throw new Exception($"Database error at DatabaseService.GetExpenseByIdAsync: {ex.Message}", ex);
            }
        }

        public async Task<int> CreateExpenseAsync(CreateExpenseRequest request)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.CreateExpense", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@UserId", request.UserId);
                command.Parameters.AddWithValue("@CategoryId", request.CategoryId);
                command.Parameters.AddWithValue("@AmountMinor", request.AmountMinor);
                command.Parameters.AddWithValue("@ExpenseDate", request.ExpenseDate);
                command.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@ReceiptFile", (object?)request.ReceiptFile ?? DBNull.Value);
                
                var outputParam = new SqlParameter("@ExpenseId", System.Data.SqlDbType.Int)
                {
                    Direction = System.Data.ParameterDirection.Output
                };
                command.Parameters.Add(outputParam);

                await command.ExecuteNonQueryAsync();
                return (int)outputParam.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating expense");
                throw new Exception($"Database error at DatabaseService.CreateExpenseAsync: {ex.Message}", ex);
            }
        }

        public async Task UpdateExpenseAsync(UpdateExpenseRequest request)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.UpdateExpense", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ExpenseId", request.ExpenseId);
                command.Parameters.AddWithValue("@CategoryId", request.CategoryId);
                command.Parameters.AddWithValue("@AmountMinor", request.AmountMinor);
                command.Parameters.AddWithValue("@ExpenseDate", request.ExpenseDate);
                command.Parameters.AddWithValue("@Description", (object?)request.Description ?? DBNull.Value);
                command.Parameters.AddWithValue("@ReceiptFile", (object?)request.ReceiptFile ?? DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating expense");
                throw new Exception($"Database error at DatabaseService.UpdateExpenseAsync: {ex.Message}", ex);
            }
        }

        public async Task SubmitExpenseAsync(int expenseId)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.SubmitExpense", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ExpenseId", expenseId);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting expense");
                throw new Exception($"Database error at DatabaseService.SubmitExpenseAsync: {ex.Message}", ex);
            }
        }

        public async Task ApproveExpenseAsync(int expenseId, int reviewerId)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.ApproveExpense", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ExpenseId", expenseId);
                command.Parameters.AddWithValue("@ReviewerId", reviewerId);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving expense");
                throw new Exception($"Database error at DatabaseService.ApproveExpenseAsync: {ex.Message}", ex);
            }
        }

        public async Task RejectExpenseAsync(int expenseId, int reviewerId)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.RejectExpense", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ExpenseId", expenseId);
                command.Parameters.AddWithValue("@ReviewerId", reviewerId);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting expense");
                throw new Exception($"Database error at DatabaseService.RejectExpenseAsync: {ex.Message}", ex);
            }
        }

        public async Task DeleteExpenseAsync(int expenseId)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.DeleteExpense", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ExpenseId", expenseId);
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting expense");
                throw new Exception($"Database error at DatabaseService.DeleteExpenseAsync: {ex.Message}", ex);
            }
        }

        public async Task<List<ExpenseCategory>> GetCategoriesAsync()
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.GetCategories", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                var categories = new List<ExpenseCategory>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    categories.Add(new ExpenseCategory
                    {
                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive"))
                    });
                }
                return categories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting categories");
                throw new Exception($"Database error at DatabaseService.GetCategoriesAsync: {ex.Message}", ex);
            }
        }

        public async Task<List<ExpenseStatus>> GetStatusesAsync()
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.GetStatuses", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                var statuses = new List<ExpenseStatus>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    statuses.Add(new ExpenseStatus
                    {
                        StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
                        StatusName = reader.GetString(reader.GetOrdinal("StatusName"))
                    });
                }
                return statuses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statuses");
                throw new Exception($"Database error at DatabaseService.GetStatusesAsync: {ex.Message}", ex);
            }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.GetUsers", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                var users = new List<User>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                        RoleName = reader.GetString(reader.GetOrdinal("RoleName")),
                        ManagerId = reader.IsDBNull(reader.GetOrdinal("ManagerId")) ? null : reader.GetInt32(reader.GetOrdinal("ManagerId")),
                        ManagerName = reader.IsDBNull(reader.GetOrdinal("ManagerName")) ? null : reader.GetString(reader.GetOrdinal("ManagerName")),
                        IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                throw new Exception($"Database error at DatabaseService.GetUsersAsync: {ex.Message}", ex);
            }
        }

        public async Task<List<Expense>> GetPendingExpensesAsync(int? managerId = null)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.GetPendingExpenses", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@ManagerId", (object?)managerId ?? DBNull.Value);

                var expenses = new List<Expense>();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    expenses.Add(new Expense
                    {
                        ExpenseId = reader.GetInt32(reader.GetOrdinal("ExpenseId")),
                        UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                        UserName = reader.GetString(reader.GetOrdinal("UserName")),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        StatusId = reader.GetInt32(reader.GetOrdinal("StatusId")),
                        StatusName = reader.GetString(reader.GetOrdinal("StatusName")),
                        AmountMinor = reader.GetInt32(reader.GetOrdinal("AmountMinor")),
                        AmountGBP = reader.GetDecimal(reader.GetOrdinal("AmountGBP")),
                        Currency = reader.GetString(reader.GetOrdinal("Currency")),
                        ExpenseDate = reader.GetDateTime(reader.GetOrdinal("ExpenseDate")),
                        Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                        ReceiptFile = reader.IsDBNull(reader.GetOrdinal("ReceiptFile")) ? null : reader.GetString(reader.GetOrdinal("ReceiptFile")),
                        SubmittedAt = reader.IsDBNull(reader.GetOrdinal("SubmittedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("SubmittedAt")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                    });
                }
                return expenses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending expenses");
                throw new Exception($"Database error at DatabaseService.GetPendingExpensesAsync: {ex.Message}", ex);
            }
        }

        public async Task<ExpenseSummary> GetExpenseSummaryAsync(int? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                using var connection = await GetConnectionAsync();
                using var command = new SqlCommand("dbo.GetExpenseSummary", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                
                command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                command.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                command.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ExpenseSummary
                    {
                        TotalExpenses = reader.GetInt32(reader.GetOrdinal("TotalExpenses")),
                        DraftCount = reader.GetInt32(reader.GetOrdinal("DraftCount")),
                        SubmittedCount = reader.GetInt32(reader.GetOrdinal("SubmittedCount")),
                        ApprovedCount = reader.GetInt32(reader.GetOrdinal("ApprovedCount")),
                        RejectedCount = reader.GetInt32(reader.GetOrdinal("RejectedCount")),
                        TotalAmountMinor = reader.IsDBNull(reader.GetOrdinal("TotalAmountMinor")) ? 0 : reader.GetInt32(reader.GetOrdinal("TotalAmountMinor")),
                        TotalAmountGBP = reader.IsDBNull(reader.GetOrdinal("TotalAmountGBP")) ? 0 : reader.GetDecimal(reader.GetOrdinal("TotalAmountGBP")),
                        ApprovedAmountMinor = reader.IsDBNull(reader.GetOrdinal("ApprovedAmountMinor")) ? 0 : reader.GetInt32(reader.GetOrdinal("ApprovedAmountMinor")),
                        ApprovedAmountGBP = reader.IsDBNull(reader.GetOrdinal("ApprovedAmountGBP")) ? 0 : reader.GetDecimal(reader.GetOrdinal("ApprovedAmountGBP"))
                    };
                }
                return new ExpenseSummary();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting expense summary");
                throw new Exception($"Database error at DatabaseService.GetExpenseSummaryAsync: {ex.Message}", ex);
            }
        }
        
        public List<Expense> GetDummyExpenses()
        {
            return new List<Expense>
            {
                new Expense
                {
                    ExpenseId = 1,
                    UserId = 1,
                    UserName = "Alice Example",
                    Email = "alice@example.co.uk",
                    CategoryId = 1,
                    CategoryName = "Travel",
                    StatusId = 2,
                    StatusName = "Submitted",
                    AmountMinor = 2540,
                    AmountGBP = 25.40m,
                    Currency = "GBP",
                    ExpenseDate = DateTime.Now.AddDays(-10),
                    Description = "Taxi from airport to client site",
                    SubmittedAt = DateTime.Now.AddDays(-9),
                    CreatedAt = DateTime.Now.AddDays(-10)
                },
                new Expense
                {
                    ExpenseId = 2,
                    UserId = 1,
                    UserName = "Alice Example",
                    Email = "alice@example.co.uk",
                    CategoryId = 2,
                    CategoryName = "Meals",
                    StatusId = 3,
                    StatusName = "Approved",
                    AmountMinor = 1425,
                    AmountGBP = 14.25m,
                    Currency = "GBP",
                    ExpenseDate = DateTime.Now.AddDays(-30),
                    Description = "Client lunch meeting",
                    SubmittedAt = DateTime.Now.AddDays(-29),
                    ReviewedBy = 2,
                    ReviewerName = "Bob Manager",
                    ReviewedAt = DateTime.Now.AddDays(-28),
                    CreatedAt = DateTime.Now.AddDays(-30)
                }
            };
        }

        public List<ExpenseCategory> GetDummyCategories()
        {
            return new List<ExpenseCategory>
            {
                new ExpenseCategory { CategoryId = 1, CategoryName = "Travel", IsActive = true },
                new ExpenseCategory { CategoryId = 2, CategoryName = "Meals", IsActive = true },
                new ExpenseCategory { CategoryId = 3, CategoryName = "Supplies", IsActive = true },
                new ExpenseCategory { CategoryId = 4, CategoryName = "Accommodation", IsActive = true },
                new ExpenseCategory { CategoryId = 5, CategoryName = "Other", IsActive = true }
            };
        }
    }
}
