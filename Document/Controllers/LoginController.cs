using Document.Models;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Document.Helpers;
using Google.Apis.Util;

namespace Document.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IUtilities _utilities;
        private readonly RequestContext _context;

        public LoginController(RequestContext context, ILogger<LoginController> logger, IUtilities utilities)
        {
            _context = context;
            _logger = logger;
            _utilities = utilities;
           
        }

        public static string SessionKeyName = "_Name";




        public IActionResult Index()
        {


            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel user)
        {


            try
            {


                if (ModelState.IsValid)
                {



                    string adPath = "LDAP://spdc.com"; //Path to your LDAP directory server
                    Authentication adAuth = new Authentication(adPath);
                    if (adAuth.IsAuthenticated("spdc.com", user.Username, user.Password) == true)
                    {
                        HttpContext.Session.SetString("Session", "");
                        HttpContext.Session.Clear();

                        HttpContext.Session.SetString("Session", user.Username);

                    try
                    {
                        var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                        var mgrUser = _context.Requesters.FirstOrDefault(m => m.LDap == currentUser.LDap);


                        if (mgrUser != null)
                        {
                            var isMgr = _context.Requesters.Where(m => m.MgrCode == mgrUser.EmployeeCode);

                            if (!isMgr.IsNullOrEmpty())

                            {
                                return RedirectToAction("MgrIndex", "Home");
                            }
                            else
                                return RedirectToAction("Landing", "Home");
                        }
                    }
                    catch (Exception ex)
                    {
                            var guid = Guid.NewGuid().ToString();
                            string file = $@"C:\EDoc\Log_LOGIN_{DateTime.Today:yyyyMMdd}";
                            _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Login_error,{guid}");
                            _utilities.WriteLog(file, ex?.Message!);
                            SessionKeyName = string.Empty;

                        HttpContext.Session.Clear();
                        return RedirectToAction("Index", "Error");
                       
                    }

                    }
                    else
                    {
                        return Content("The user name or password is incorrect");
                    }

                }
                return View();
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_LOGIN_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Login_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                if (ex.Message == "The user name or password is incorrect.")
                {
                    
                    ModelState.AddModelError(string.Empty, "The user name or password is incorrect.");
                    SessionKeyName = string.Empty;
                    HttpContext.Session.SetString("Session", "");
                    HttpContext.Session.Clear();
                    return View();
                }
                else
                {
                    SessionKeyName = string.Empty;
                    HttpContext.Session.SetString("Session", "");
                    HttpContext.Session.Clear();
                    return View();
                }





            }
           

        }
        public IActionResult Logout()
        {

            try
            {

                HttpContext.Session.SetString("Session", "");

                HttpContext.Session.Clear();


                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_LOGOUT_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Logout_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("Index", "Error");
            }



        }
}   }
