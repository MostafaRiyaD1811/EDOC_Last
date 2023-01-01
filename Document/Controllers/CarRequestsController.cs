using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Document.Models;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Document.Helpers;
using Google.Apis.Admin.Directory.directory_v1.Data;
using System.DirectoryServices.ActiveDirectory;
using Document.ViewModels;
using Microsoft.IdentityModel.Tokens;
using Rotativa.AspNetCore;
using MgrStatutses = Document.ViewModels.MgrStatutses;
using DeptAdminStatutses = Document.ViewModels.DeptAdminStatutses;

namespace Document.Controllers
{

    public class CarRequestsController : Controller
    {
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;
        private readonly IWebHostEnvironment _hostingEnvironment;

       
        public CarRequestsController(RequestContext context, IUtilities utilities, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _utilities = utilities;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: CarRequests
        public async Task<IActionResult> RedirectTo()
        {
           
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var mgrUser = _context.Requesters.FirstOrDefault(m => m.LDap == currentUser.LDap);


                if (mgrUser != null)
                {
                    var isMgr = _context.Requesters.Where(m => m.MgrCode == mgrUser.EmployeeCode);
                   
                    if (currentUser.Transportation == 1)
                    {
                        return RedirectToAction(nameof(TransIndex));
                    }

                    /*  else*/
                    if (currentUser.Adminstration ==1)
                    {
                        return RedirectToAction("Index", "DeptAdmin");
                    }
                    else if (!isMgr.IsNullOrEmpty())
                    {
                        return RedirectToAction("Index", "Mgr");
                    }
                    else
                    {
                        return RedirectToAction("Index", "CarRequests");
                    }
                } 
                else 
                {
                    return RedirectToAction("Index", "CarRequests");
                }

               
            }
            catch (Exception)
            {

                return RedirectToAction("Index","Login");
            }
            
        }
        public async Task<IActionResult> Index(int? searching)
        {
            

            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (searching.HasValue)
                    return View(await _context.CarRequests.Where(w => w.RequesterId == currentUser.EmployeeCode).Where(P => P.ReqNumber.ToString().Contains(searching.ToString())).OrderByDescending(e=>e.CreationDate).ToListAsync());
                else

                    return View(await _context.CarRequests.Where(w => w.RequesterId == currentUser.EmployeeCode).OrderByDescending(e => e.CreationDate).ToListAsync());

            }
            catch (Exception)
            {
                return RedirectToAction("NoFound", "Error");
            }

        }
        public async Task<IActionResult> TransIndex(int? searching)
        {
           

            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (searching.HasValue)
                    return View(await _context.CarRequests.Where(w => w.DeptAdminStatuts == Models.DeptAdminStatutses.Approve).Where(P => P.ReqNumber.ToString().Contains(searching.ToString())).OrderByDescending(e => e.CreationDate).ToListAsync());
                else

                    return View(await _context.CarRequests.Where(w => w.DeptAdminStatuts == Models.DeptAdminStatutses.Approve).OrderByDescending(e => e.CreationDate).ToListAsync());

            }
            catch (Exception)
            {

                return RedirectToAction("NoFound", "Error");
            }

        }

        // GET: CarRequests/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.CarRequests == null)
            {
                return RedirectToAction("NoFound", "Error");
            }

            var carRequest = await _context.CarRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carRequest == null)
            {
                return RedirectToAction("NoFound", "Error");
            }

            return View(carRequest);
        }

        // GET: CarRequests/Create
        public IActionResult Create()
        {
            return View();
        }

       
        // POST: CarRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CarRequestViewModel carRequest)
        {
           
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            var currentUserMgr = _context.Requesters.Where(e=> e.EmployeeCode== currentUser.MgrCode).FirstOrDefault();
            try
            {
                if (ModelState.IsValid)
                {

                    string uniqueFileName = null;

                    var model = new CarRequest
                    {
                        Id = Guid.NewGuid()
                    };
                    var uploadsFolder = Directory.CreateDirectory($@"{_hostingEnvironment.WebRootPath}\{carRequest.ReqNumber}_Car Request");
                    if (carRequest.Attach != null)
                    {
                        string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{carRequest.ReqNumber}_Car Request");
                        uniqueFileName = carRequest.Attach.FileName;
                        string filePath = Path.Combine(uploadsFolder1, uniqueFileName);
                        FileStream file = new FileStream(filePath, FileMode.Create);

                        await carRequest.Attach.CopyToAsync(file);

                        model.AttachmentPath = uniqueFileName;
                        file.Close();
                        file.Dispose();
                    }

                    model.CurrentReview = currentUser.MgrName;
                    model.RequesterName = currentUser.Name;
                    model.ReqNumber = carRequest.ReqNumber;
                    model.Name = carRequest.Name;
                    model.MobileNo = carRequest.MobileNo;
                    model.DepartureAddress = carRequest.DepartureAddress;
                    model.DestinationAddress = carRequest.DestinationAddress;
                    model.Departure = carRequest.Departure;
                    model.ReturnBack = carRequest.ReturnBack;
                    model.LuggageDescription = carRequest.LuggageDescription;
                    model.CarType = carRequest.CarType;
                    model.Justification = carRequest.Justification;
                    model.CreatorId = currentUser.EmployeeCode;
                    model.RequesterId = currentUser.EmployeeCode;
                    model.CurrentReviewerId = currentUser.MgrCode;
                    model.MgrStatuts = Models.MgrStatutses.Pending;
                    model.Status = Models.DeptAdminStatutses.Pending.ToString();
                    model.Notes = string.Empty;
                    model.AttachmentPath = uniqueFileName;
                    model.ChangedBy = currentUser.EmployeeCode.ToString();

                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUser.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name} Request a Car From: {carRequest.DepartureAddress} to {carRequest.DestinationAddress} \n At {carRequest.Departure} \n Kindly Check your E-Document Inbox");
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUserMgr.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name} Request a Car From: {carRequest.DepartureAddress} to {carRequest.DestinationAddress} \n At {carRequest.Departure} \n Kindly Check your E-Document Inbox");
                    _context.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index), new { id = model.Id });
                }



               
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string fileName = $@"C:\CarRequsets\Log_CarRequsetsCreate_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,CarRequsets_error,{guid}");
                _utilities.WriteLog(fileName, ex?.StackTrace!);
                return RedirectToAction("Index", "Error");
               
            }

            return View();


        }

        // GET: CarRequests/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id == null || _context.CarRequests == null)
            {
                return RedirectToAction("NoFound", "Error");
            }

            var carRequest = await _context.CarRequests.FindAsync(id);
            var model = new CarRequestViewModel();
            if (carRequest == null)
            {
                return RedirectToAction("NoFound", "Error");
            }
            model.RequesterName = carRequest.RequesterName;
            model.ReqNumber = carRequest.ReqNumber;
            model.Name = carRequest.Name;
            model.MobileNo = carRequest.MobileNo;
            model.DepartureAddress = carRequest.DepartureAddress;
            model.DestinationAddress = carRequest.DestinationAddress;
            model.Departure = carRequest.Departure;
            model.ReturnBack = carRequest.ReturnBack;
            model.LuggageDescription = carRequest.LuggageDescription;
            model.CarType = carRequest.CarType;
            model.Justification = carRequest.Justification;
            model.CreatorId = currentUser.EmployeeCode;
            model.RequesterId = currentUser.EmployeeCode;
            model.CurrentReviewerId = currentUser.MgrCode;
            model.MgrStatuts = MgrStatutses.Pending;
            model.Status =DeptAdminStatutses.Pending.ToString();
            model.Notes = string.Empty;
            model.Attach1 = carRequest.AttachmentPath;
            return View(model);
        }

        // POST: CarRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CarRequestViewModel carRequest)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            var currentUserMgr = _context.Requesters.Where(e => e.EmployeeCode == currentUser.MgrCode).FirstOrDefault();

            if (id != carRequest.Id)
            {
                return RedirectToAction("NoFound", "Error");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = null;
                  


                    var model = await _context.CarRequests.Where(v => v.Id == id).FirstOrDefaultAsync();
                    var uploadsFolder = Directory.CreateDirectory($@"{_hostingEnvironment.WebRootPath}\{model.ReqNumber}_Car Request");
                    if (carRequest.Attach != null)
                    {
                        string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{model.ReqNumber}_Car Request");
                        uniqueFileName = carRequest.Attach.FileName;
                        string filePath = Path.Combine(uploadsFolder1, uniqueFileName);
                        FileStream file = new FileStream(filePath, FileMode.Create);

                        await carRequest.Attach.CopyToAsync(file);

                        model.AttachmentPath = uniqueFileName;
                        file.Close();
                        file.Dispose();
                    }

                    model.RequesterName = carRequest.RequesterName;
                    model.ReqNumber = carRequest.ReqNumber;
                    model.Name = carRequest.Name;
                    model.MobileNo = carRequest.MobileNo;
                    model.DepartureAddress = carRequest.DepartureAddress;
                    model.DestinationAddress = carRequest.DestinationAddress;
                    model.Departure = carRequest.Departure;
                    model.ReturnBack = carRequest.ReturnBack;
                    model.LuggageDescription = carRequest.LuggageDescription;           
                    model.Justification = carRequest.Justification;
                    model.CarType = carRequest.CarType;
                    model.CurrentReview = currentUser.MgrName;
                    model.CurrentReviewerId = currentUser.MgrCode;
                    model.MgrStatuts = Models.MgrStatutses.Pending;
                    model.DeptAdminStatuts = Models.DeptAdminStatutses.None;
                    model.Status = DeptAdminStatutses.Pending.ToString();
                    model.RequesterId = currentUser.EmployeeCode;
                    model.ChangedBy= currentUser.EmployeeCode.ToString(); // to record modifier

                   _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUserMgr.LDap), "Youssef.AboZaid@dpworld.com","", "Car Request", $"{currentUser.Name} update the request.");

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!CarRequestExists(carRequest.Id))
                    {
                        return RedirectToAction("NoFound", "Error");

                    }
                    else
                    {
                        var guid = Guid.NewGuid().ToString();
                        string fileName = $@"C:\CarRequsets\Log_CarRequsetsEdit_{DateTime.Today:yyyyMMdd}";
                        _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,CarRequsets_error,{guid}");
                        _utilities.WriteLog(fileName, ex?.StackTrace!);
                        return RedirectToAction("Index", "Error");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(carRequest);
        }

        // GET: CarRequests/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.CarRequests == null)
            {
                return NotFound();
            }

            var carRequest = await _context.CarRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (carRequest == null)
            {
                return RedirectToAction("NoFound", "Error");
            }

            return View(carRequest);
        }

        // POST: CarRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            if (_context.CarRequests == null)
            {
                return Problem("Entity set 'RequestContext.CarRequests'  is null.");
            }
            var carRequest = await _context.CarRequests.FindAsync(id);
            if (carRequest != null)
            {
                carRequest.ChangedBy = currentUser.EmployeeCode.ToString();
                _context.CarRequests.Update(carRequest);
                await _context.SaveChangesAsync();
                _context.CarRequests.Remove(carRequest);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult DemoViewAsPDF(Guid id)
        {
            CarRequest Creq = _context.CarRequests.FirstOrDefault(c => c.Id == id);

            var report = new ViewAsPdf("Details", Creq)
            {
                PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 },
            };
            return report;
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
                string file = $@"C:\CarRequsets\Log_CarRequsetsAttachment_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,CarRequsets_error,{guid}");
                _utilities.WriteLog(file, ex?.StackTrace!);

                return Content("File Doesn't Exist");
            }


        }
        private bool CarRequestExists(Guid id)
        {
            return _context.CarRequests.Any(e => e.Id == id);
        }


        public async Task<IActionResult> TransTeamLeaderIndex(int? searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (searching.HasValue)
                    return View(await _context.CarRequests.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).Where(P => P.ReqNumber.ToString().Contains(searching.ToString())).ToListAsync());
                else
                    return View(await _context.CarRequests.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).ToListAsync());
            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Login");
            }



        }
        public async Task<IActionResult> TransTeamLeaderEdit(Guid? id)
        {


            var carRequest = await _context.CarRequests.FindAsync(id);
            carRequest.Notes = string.Empty;

            return View(carRequest);
        }

        
      
    }


}
