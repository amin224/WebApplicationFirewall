using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;

namespace SimpleWebApplication.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        public IActionResult MoneyTransfer()
        {
            var isApproved = User.Claims.FirstOrDefault(c => c.Type == "IsApproved")?.Value;
            if (!string.IsNullOrEmpty(isApproved) && isApproved.Equals("False", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("MyProcess", "User");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoneyTransfer(string iban, string receiverName, decimal amount)
        {
            var isApprovedClaim = User.Claims.FirstOrDefault(c => c.Type == "isApproved")?.Value;
            if (!string.IsNullOrEmpty(isApprovedClaim) && isApprovedClaim.Equals("False", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("MyProcess", "User");
            }

            // to simulate transaction, get or initialize total amount
            var totalAmount = HttpContext.Session.GetString("TotalAmount");
            if (string.IsNullOrEmpty(totalAmount))
            {
                totalAmount = "5000";
                HttpContext.Session.SetString("TotalAmount", totalAmount);
            }

            if (decimal.TryParse(totalAmount, out var currentAmount))
            {
                if (currentAmount >= amount)
                {
                    var initialAmount = currentAmount;
                    currentAmount -= amount;
                    HttpContext.Session.SetString("TotalAmount", currentAmount.ToString());
                    ViewBag.InitialAmount = initialAmount;
                    ViewBag.TransferredAmount = amount;
                    ViewBag.CurrentAmount = currentAmount;
                    ViewBag.SuccessMessage = "Money transferred successfully.";
                }
                else
                {
                    ViewBag.ErrorMessage = "Insufficient funds.";
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Transaction error.";
            }

            return View();
        }
    }
}
