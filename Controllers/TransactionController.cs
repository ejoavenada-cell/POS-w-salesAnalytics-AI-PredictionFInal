using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FoodOrderingSytemAIAnalytics.Services;
using System;
using System.Threading.Tasks;

namespace FoodOrderingSytemAIAnalytics.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<IActionResult> History(string? searchTerm, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue)
            {
                var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate.Value, endDate.Value);
                ViewBag.StartDate = startDate.Value.ToString("yyyy-MM-dd");
                ViewBag.EndDate = endDate.Value.ToString("yyyy-MM-dd");
                return View(transactions);
            }
            else
            {
                var transactions = await _transactionService.SearchTransactionsAsync(searchTerm ?? "");
                ViewBag.SearchTerm = searchTerm;
                return View(transactions);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null) return NotFound();
            return PartialView("_TransactionDetails", transaction);
        }
    }
}
