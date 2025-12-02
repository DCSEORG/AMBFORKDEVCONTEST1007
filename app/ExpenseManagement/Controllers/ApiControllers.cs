using Microsoft.AspNetCore.Mvc;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<ExpensesController> _logger;

        public ExpensesController(DatabaseService dbService, ILogger<ExpensesController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<Expense>>> GetExpenses([FromQuery] int? userId = null, [FromQuery] int? statusId = null, [FromQuery] int? categoryId = null, [FromQuery] string? searchTerm = null)
        {
            try
            {
                var expenses = await _dbService.GetExpensesAsync(userId, statusId, categoryId, searchTerm);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetExpenses");
                return Ok(_dbService.GetDummyExpenses());
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Expense>> GetExpense(int id)
        {
            try
            {
                var expense = await _dbService.GetExpenseByIdAsync(id);
                if (expense == null)
                    return NotFound();
                return Ok(expense);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetExpense");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<int>> CreateExpense([FromBody] CreateExpenseRequest request)
        {
            try
            {
                var expenseId = await _dbService.CreateExpenseAsync(request);
                return Ok(new { expenseId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateExpense");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseRequest request)
        {
            try
            {
                request.ExpenseId = id;
                await _dbService.UpdateExpenseAsync(request);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateExpense");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/submit")]
        public async Task<ActionResult> SubmitExpense(int id)
        {
            try
            {
                await _dbService.SubmitExpenseAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SubmitExpense");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<ActionResult> ApproveExpense(int id, [FromBody] int reviewerId)
        {
            try
            {
                await _dbService.ApproveExpenseAsync(id, reviewerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ApproveExpense");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<ActionResult> RejectExpense(int id, [FromBody] int reviewerId)
        {
            try
            {
                await _dbService.RejectExpenseAsync(id, reviewerId);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RejectExpense");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteExpense(int id)
        {
            try
            {
                await _dbService.DeleteExpenseAsync(id);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeleteExpense");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("pending")]
        public async Task<ActionResult<List<Expense>>> GetPendingExpenses([FromQuery] int? managerId = null)
        {
            try
            {
                var expenses = await _dbService.GetPendingExpensesAsync(managerId);
                return Ok(expenses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPendingExpenses");
                return Ok(_dbService.GetDummyExpenses().Where(e => e.StatusName == "Submitted").ToList());
            }
        }

        [HttpGet("summary")]
        public async Task<ActionResult<ExpenseSummary>> GetExpenseSummary([FromQuery] int? userId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var summary = await _dbService.GetExpenseSummaryAsync(userId, startDate, endDate);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetExpenseSummary");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(DatabaseService dbService, ILogger<CategoriesController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ExpenseCategory>>> GetCategories()
        {
            try
            {
                var categories = await _dbService.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetCategories");
                return Ok(_dbService.GetDummyCategories());
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class StatusesController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<StatusesController> _logger;

        public StatusesController(DatabaseService dbService, ILogger<StatusesController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ExpenseStatus>>> GetStatuses()
        {
            try
            {
                var statuses = await _dbService.GetStatusesAsync();
                return Ok(statuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStatuses");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(DatabaseService dbService, ILogger<UsersController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            try
            {
                var users = await _dbService.GetUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUsers");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
