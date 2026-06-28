using Azure.Core;
using ManagementAppMVC.Models;
using ManagementAppMVC.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ManagementAppMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAuthService _authService;
        private readonly IPowerBiService _powerBiService;

        public HomeController(ILogger<HomeController> logger, IAuthService authService, IPowerBiService powerBiService)
        {
            _logger = logger;
            _authService = authService;
            _powerBiService = powerBiService;
        }

        public IActionResult Index()
        {
            return View();
        }

        
        public async Task<IActionResult> ViewReport()
        {
            var authResult = await _authService.GetAccessTokenAsync();

            var embededInfo = await _powerBiService.GetEmbedInfo(
                authResult,
                username: "veaceslav.chirilov@amdaris.com",
                "CompanyRLS",
                "C003"
            );

            var country = Request.Query.ContainsKey("country") ? Request.Query["country"].ToString() : string.Empty;

            var model = new EmbedInfoViewModel
            {
                EmbedToken = embededInfo.EmbedToken,
                EmbedUrl = embededInfo.EmbedUrl,
                ReportId = embededInfo.reportId,
                Country = country
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}


