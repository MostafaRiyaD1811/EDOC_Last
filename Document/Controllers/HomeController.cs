using Document.Helpers;
using Document.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Dynamic;

namespace Document.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;

        public HomeController(ILogger<HomeController> logger, RequestContext context, IUtilities utilities,IWebHostEnvironment hostingEnvironment)
        {
            _logger = logger;
            _context = context;
            _utilities = utilities;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ActionResult>MgrIndex()
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            dynamic mymodel = new ExpandoObject();
            mymodel.CarRequests = await _context.CarRequests.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).OrderByDescending(e => e.CreationDate).ToListAsync();
            mymodel.DomainAccounts = await _context.DomainAccounts.Where(e => e.CurrentReviewerId == currentUser.EmployeeCode).OrderByDescending(e => e.CreatedAt).ToListAsync();
            mymodel.TravelDesks =  await _context.TravelDesks.Where(e => e.CurrentReviewerId == currentUser.EmployeeCode).OrderByDescending(e => e.CreatedAt).ToListAsync();
            mymodel.JobPlans =  await _context.JobPlanUpdates.Where(e => e.CurrentReviewerId == currentUser.EmployeeCode).OrderByDescending(e => e.CreatedAt).ToListAsync();

            return View(mymodel);
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Landing()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public async Task<IActionResult> Download(string fileName, int id)
        {
            try
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, $"{id}_Car Request");
                string FPath = Path.Combine(uploadsFolder, fileName);

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(FPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\domainAccount\Log_domainAccountCreate_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,domainAccount_error,{guid}");
                _utilities.WriteLog(file, ex?.StackTrace!);
                return Content("File Doesn't Exist");
            }


        }
    }
}