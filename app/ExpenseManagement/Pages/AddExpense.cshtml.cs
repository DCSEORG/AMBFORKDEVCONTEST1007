using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseManagement.Models;
using ExpenseManagement.Services;

namespace ExpenseManagement.Pages;

public class AddExpenseModel : PageModel
{
    private readonly ILogger<AddExpenseModel> _logger;
    private readonly DatabaseService _dbService;

    [BindProperty]
    public decimal Amount { get; set; }
    
    [BindProperty]
    public DateTime ExpenseDate { get; set; } = DateTime.Now;
    
    [BindProperty]
    public int CategoryId { get; set; }
    
    [BindProperty]
    public string? Description { get; set; }

    public List<ExpenseCategory> Categories { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public AddExpenseModel(ILogger<AddExpenseModel> logger, DatabaseService dbService)
    {
        _logger = logger;
        _dbService = dbService;
    }

    public async Task OnGetAsync()
    {
        try
        {
            Categories = await _dbService.GetCategoriesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
            ErrorMessage = $"Error loading categories: {ex.Message}";
            Categories = _dbService.GetDummyCategories();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Default to first user (Alice Example) for demo
            var amountMinor = (int)(Amount * 100);
            
            var request = new CreateExpenseRequest
            {
                UserId = 1, // Default user
                CategoryId = CategoryId,
                AmountMinor = amountMinor,
                ExpenseDate = ExpenseDate,
                Description = Description
            };

            var expenseId = await _dbService.CreateExpenseAsync(request);
            
            // Auto-submit the expense
            await _dbService.SubmitExpenseAsync(expenseId);
            
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating expense");
            ErrorMessage = $"Error creating expense: {ex.Message}";
            Categories = await _dbService.GetCategoriesAsync();
            return Page();
        }
    }
}
