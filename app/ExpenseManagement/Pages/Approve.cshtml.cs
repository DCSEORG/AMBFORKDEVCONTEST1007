using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Pages;

public class ApproveModel : PageModel
{
    private readonly ILogger<ApproveModel> _logger;
    private readonly DatabaseService _dbService;

    public List<Expense> PendingExpenses { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public ApproveModel(ILogger<ApproveModel> logger, DatabaseService dbService)
    {
        _logger = logger;
        _dbService = dbService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            PendingExpenses = await _dbService.GetPendingExpensesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading pending expenses");
            ErrorMessage = $"Error loading pending expenses: {ex.Message}";
            PendingExpenses = _dbService.GetDummyExpenses().Where(e => e.StatusName == "Submitted").ToList();
        }
    }

    public async Task<IActionResult> OnPostAsync(int expenseId, string action)
    {
        try
        {
            // Default to manager user (Bob Manager) for demo
            int reviewerId = 2;
            
            if (action == "approve")
            {
                await _dbService.ApproveExpenseAsync(expenseId, reviewerId);
            }
            else if (action == "reject")
            {
                await _dbService.RejectExpenseAsync(expenseId, reviewerId);
            }
            
            return RedirectToPage("/Approve");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expense");
            ErrorMessage = $"Error processing expense: {ex.Message}";
            PendingExpenses = await _dbService.GetPendingExpensesAsync();
            return Page();
        }
    }
}
