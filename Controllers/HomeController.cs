using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using AppTran.Services;
using System;
using Microsoft.Extensions.Logging;

namespace AppTran.Controllers
{
    public class HomeController : Controller
    {
        private readonly IApplicantService _applicantService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IApplicantService applicantService, ILogger<HomeController> logger)
        {
            _applicantService = applicantService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> SearchApplicants(string searchText)
        {
            var results = await _applicantService.SearchApplicantsAsync(searchText);
            return Json(results);
        }

        [HttpGet]
        public async Task<JsonResult> CheckRegNumber(string number)
        {
            var exists = await _applicantService.CheckRegNumberExistsAsync(number);
            return Json(exists);
        }



    }
}