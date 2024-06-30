using Microsoft.AspNetCore.Mvc;
using SimpleWebApplication.Models;
using System.Diagnostics;
using SimpleWebApplication.Helpers;
using SimpleWebApplication.WebFirewall;
using Npgsql;
using System.Data.Common;
using System.Text;
using System.Security.Cryptography;
using SimpleWebApplication.Engines;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace SimpleWebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailEngine _emailEngine;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IEmailEngine emailEngine)
        {
            _logger = logger;
            _configuration = configuration;
            _emailEngine = emailEngine;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        public IActionResult Documents()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || password != confirmPassword)
            {
                ViewBag.ErrorMessage = "invalid username email or password or they do not match.";
                return View();
            }

            // Username criteria is at least 8 characters long and contains only letters and numbers
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]{8,}$"))
            {
                ViewBag.ErrorMessage = "Username must be at least 8 characters long and contain only letters and numbers.";
                return View();
            }

            // Email criteria is valid email format
            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.ErrorMessage = "Please provide valid email address.";
                return View();
            }

            // Password criteria is at least 8 characters long, should contain at least one uppercase letter, one lowercase letter, and one number
            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
            {
                ViewBag.ErrorMessage = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number.";
                return View();
            }

            var connectionString = _configuration.GetConnectionString("MyAppDbConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand("INSERT INTO users (username, email, password) VALUES (@username, @email, @password)", connection);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("email", email);
            cmd.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(password));

            try
            {
                await cmd.ExecuteNonQueryAsync();
                ViewBag.SuccessMessage = "Your application was sent. Thank you for choosing us :) To finish the process we need your resident permit file. Please upload this file using the 'Send Document' menu.";
            }
            catch (Exception ex)
            {
                HttpContext.Items["Exception"] = ex.Message;
                return RedirectToAction("CustomErrorPage", "Error");
            }

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var connectionString = _configuration.GetConnectionString("MyAppDbConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT password, is_approved FROM users WHERE username = @username", connection);
            cmd.Parameters.AddWithValue("username", username);

            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var storedPassword = reader.GetString(0);
                var isApproved = reader.GetBoolean(1);
                if (VerifyPassword(password, storedPassword))
                {
                    // Authentication successful
                    HttpContext.Session.SetString("Username", username);

                    // additional security with custom header for my process page.
                    // if the user not approved then user can show my process page.
                    if (isApproved)
                    {
                        CustomHeaderSecurity.AddCustomHeaderAsync(HttpContext, "MoneyTransfer", "Approved");
                    }
                    else
                    {
                        CustomHeaderSecurity.AddCustomHeaderAsync(HttpContext, "MyProcess", "Pending");
                    }

                    // to simulate money transfer process. 
                    HttpContext.Session.SetString("TotalAmount", "5000");


                    return RedirectToAction("MyAccount");
                }
            }

            // Authentication failed
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var connectionString = _configuration.GetConnectionString("MyAppDbConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT username FROM users WHERE email = @Email", connection);
            cmd.Parameters.AddWithValue("Email", email);

            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var username = reader.GetString(0);
                var resetToken = Guid.NewGuid().ToString();
                var expiration = DateTime.UtcNow.AddHours(1);

                reader.Close();

                var updateCmd = new NpgsqlCommand("UPDATE users SET reset_token = @token, reset_token_expiration = @expiration WHERE email = @Email", connection);
                updateCmd.Parameters.AddWithValue("token", resetToken);
                updateCmd.Parameters.AddWithValue("expiration", expiration);
                updateCmd.Parameters.AddWithValue("Email", email);
                await updateCmd.ExecuteNonQueryAsync();

                await SendResetPasswordEmail(email, resetToken);
            }

            ViewBag.Message = "If the username exists then a password reset link will be sent.";
            return View();
        }

        public IActionResult ResetPassword(string token)
        {
            // Verify token. not implemented to keep simplfy
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var connectionString = _configuration.GetConnectionString("MyAppDbConnection");
            using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            var cmd = new NpgsqlCommand("SELECT email FROM users WHERE reset_token = @token AND reset_token_expiration > @now", connection);
            cmd.Parameters.AddWithValue("token", token);
            cmd.Parameters.AddWithValue("now", DateTime.UtcNow);

            var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var email = reader.GetString(0);
                reader.Close();

                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var updateCmd = new NpgsqlCommand("UPDATE users SET password = @password, reset_token = NULL, reset_token_expiration = NULL WHERE email = @Email", connection);
                updateCmd.Parameters.AddWithValue("password", hashedPassword);
                updateCmd.Parameters.AddWithValue("Email", email);
                await updateCmd.ExecuteNonQueryAsync();

                ViewBag.Message = "Password has been reset successfully.";
            }
            else
            {
                ViewBag.Message = "Invalid or expired reset token.";
            }

            return View();
        }

        private async Task SendResetPasswordEmail(string email, string resetToken)
        {
            var resetLink = Url.Action("ResetPassword", "Home", new { token = resetToken }, Request.Scheme);
            var emailInfo = new EmailInfo
            {
                To = email,
                Subject = "Reset your password",
                Message = $"Click the link to reset your password: {resetLink}"
            };

            await _emailEngine.SendEmailAsync(emailInfo);
        }

        [HttpPost]
        [Route("GenerateToken")]
        public IActionResult GenerateToken([FromBody] LoginRequest loginRequest)
        {
            var textPassword = _configuration["JwtSettings:Password"];
        
            if (loginRequest.Username != _configuration["JwtSettings:Username"] || !VerifyPassword(textPassword!, loginRequest.Password))
                return Unauthorized("Invalid credentials.");
        
            var authHelper = new AuthorizationHelper(_configuration);
            var token = authHelper.GenerateJwtToken(loginRequest.Username);
        
            return Ok(token);
        }
        
        private bool VerifyPassword(string inputPassword, string storedHashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHashedPassword);
        }

        public IActionResult MyAccount()
        {
            return View();
        }

        // [ServiceFilter(typeof(CustomHeaderFilter))]
        public IActionResult MoneyTransfer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoneyTransfer(string iban, string receiverName, decimal amount)
        {
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

        // [ServiceFilter(typeof(CustomHeaderFilter))]
        public IActionResult MyProcess()
        {
            return View();
        }

        public IActionResult SendDocument()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SendDocument(IFormFile file)
        {
            Console.Write("true");
            return Ok();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

    }
}