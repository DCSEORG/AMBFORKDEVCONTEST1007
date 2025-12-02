using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly DatabaseService _dbService;

    public List<Expense> Expenses { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public IndexModel(ILogger<IndexModel> logger, DatabaseService dbService)
    {
        _logger = logger;
        _dbService = dbService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Expenses = await _dbService.GetExpensesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading expenses");
            ErrorMessage = $"Error at {ex.TargetSite?.DeclaringType?.Name}.{ex.TargetSite?.Name}: {ex.Message}";
            Expenses = _dbService.GetDummyExpenses();
        }
    }
}
