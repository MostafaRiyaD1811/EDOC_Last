using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Document.Models;
using Document.Helpers;
using Document.ViewModels;
using Microsoft.IdentityModel.Tokens;

namespace Document.Controllers
{
    public class TravelDesksController : Controller
    {
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public TravelDesksController(RequestContext context, IUtilities utilities, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _utilities = utilities;
            _hostingEnvironment = hostingEnvironment;
        }


        public async Task<IActionResult> RedirectTo()
        {

            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var mgrUser = _context.Requesters.FirstOrDefault(m => m.LDap == currentUser.LDap);
                

                if (mgrUser != null)
                {
                    var mgrRequests = _context.TravelDesks.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode && w.MgrStatus != MgrStatuses.Approve).FirstOrDefault();
                    var isMgr = _context.Requesters.Where(m => m.MgrCode == mgrUser.EmployeeCode);
                    var isAdmin = _context.Requesters.Where(e => e.Position == "Administration Manager"&&e.Position==currentUser.Position);
                    if (!(isMgr.IsNullOrEmpty())&& mgrRequests!=null )
                    {
                        
                        return RedirectToAction("MgrIndex");
                    }

                    else if (!isAdmin.IsNullOrEmpty())
                    {
                        return RedirectToAction("AdminMgrIndex");
                    }
                   
                    else
                    {
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    return RedirectToAction("Index");
                }


            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_RedirectTo_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_RedirectTo_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("Index", "Login");
            }

        }
        // GET: TravelDesks
        public async Task<IActionResult> Index()
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            var requestContext =  _context.TravelDesks.Where(w => w.RequesterId == currentUser.EmployeeCode || w.RequesterCode==currentUser.EmployeeCode).OrderByDescending(e=>e.CreatedAt);
            return View(await requestContext.ToListAsync());
        }
        [HttpGet]
        public async Task<IActionResult> Download(string fileName, int id)
        {
            try
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, $"{id}_Travel Desk");
                string FPath = Path.Combine(uploadsFolder, fileName);

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(FPath);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_Index_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_Index_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return Content("File Doesn't Exist");
            }


        }
        // GET: TravelDesks/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.TravelDesks == null)
            {
                return NotFound();
            }

            var travelDesk = await _context.TravelDesks
                .Include(t => t.CurrentReview)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelDesk == null)
            {
                return NotFound();
            }

            return View(travelDesk);
        }

        // GET: TravelDesks/Create
        public IActionResult Create()       
        {
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode");
            return View();
        }

        // POST: TravelDesks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TravelDeskViewModel travelDesk)
        {
            try
            {
                string uniqueFileName = null;
                var model = new TravelDesk
                {
                    Id = Guid.NewGuid()
                };
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var requesterName = _context.Requesters.Where(c => c.EmployeeCode == travelDesk.RequesterCode).FirstOrDefault();
                var uploadsFolder = Directory.CreateDirectory($@"{_hostingEnvironment.WebRootPath}\{travelDesk.ReqNumber}_Travel Desk");
                if (travelDesk.Attach!= null)
                {
                    string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{travelDesk.ReqNumber}_Travel Desk");
                    uniqueFileName = travelDesk.Attach.FileName;
                    string filePath = Path.Combine(uploadsFolder1, uniqueFileName);
                    FileStream file = new FileStream(filePath, FileMode.Create);

                    await travelDesk.Attach.CopyToAsync(file);
                    model.Attach = uniqueFileName;

                    file.Close();
                    file.Dispose();
                }
                if (travelDesk.RequesterCode == null)
                {
                    travelDesk.RequesterCode = currentUser.EmployeeCode;
                    travelDesk.RequesterName = currentUser.Name;
                    travelDesk.Title = currentUser.Position;
                    travelDesk.RequesterDept = currentUser.Dept;
                    travelDesk.CurrentReviewer = currentUser.MgrName;
                    travelDesk.CurrentReviewerId = currentUser.MgrCode;
                    travelDesk.RequesterId = currentUser.EmployeeCode;
                    travelDesk.RequestedFor = currentUser.Name;
                }
                else
                {
                    travelDesk.RequesterCode = requesterName.EmployeeCode;
                    travelDesk.RequesterName = requesterName.Name;
                    travelDesk.RequesterDept = requesterName.Dept;
                    travelDesk.Title = requesterName.Position;
                    travelDesk.CurrentReviewer = requesterName.MgrName;
                    travelDesk.CurrentReviewerId = requesterName.MgrCode;
                    travelDesk.RequesterId = currentUser.EmployeeCode;
                    travelDesk.RequestedFor = requesterName.Name;

                }
                model.RequesterCode = travelDesk.RequesterCode;
                model.RequestedFor = travelDesk.RequestedFor;
                model.RequesterId = travelDesk.RequesterId;
                model.RequesterName = travelDesk.RequesterName;
                model.Title = travelDesk.Title;
                model.RequesterDept = travelDesk.RequesterDept;
                model.CurrentReviewer = travelDesk.CurrentReviewer;
                model.CurrentReviewerId = travelDesk.CurrentReviewerId;
                model.ReqNumber = travelDesk.ReqNumber;
                model.Nationality = travelDesk.Nationality;
                model.RequestPurpose = travelDesk.RequestPurpose;
                model.CheckIn = travelDesk.CheckIn;
                model.CheckOut = travelDesk.CheckOut;
                model.MissionAddress = travelDesk.MissionAddress;
                model.MethodOfPayment = travelDesk.MethodOfPayment;
                model.TripDirection = travelDesk.TripDirection;
                model.DestinationCountry = travelDesk.DestinationCountry;
                model.ExpectedTravelTime = travelDesk.ExpectedTravelTime;
                model.CostAllocation = travelDesk.CostAllocation;
                model.Remarks = travelDesk.Remarks;
                model.RequestedFor = travelDesk.RequestedFor;
                model.Departure = travelDesk.Departure;
                model.ReturnBack = travelDesk.ReturnBack;
                model.CreatedAt = travelDesk.CreatedAt;
                model.CurrentReviewer = travelDesk.CurrentReviewer;
                model.RequesterCode= travelDesk.RequesterCode;
                model.Remarks = travelDesk.Remarks;

                model.Request = String.Join('&', travelDesk.Requests);
                
                
                model.MgrStatus = MgrStatuses.Pending;
                model.Status = Models.DeptAdminStatutses.Pending.ToString();
                model.ApprovalNotes = string.Empty;
               

                switch (model.RequesterDept)
                {
                    case "Adminstration":
                        var adminMgr = await _context.Requesters.Where(e => e.Position == "Administration Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = adminMgr.EmployeeCode;
                        model.CurrentReviewer = adminMgr.Name;
                        break;

                    case "People Department":

                        var peopleMgr = await _context.Requesters.Where(e => e.Position == "Head Of People").FirstOrDefaultAsync();
                        model.CurrentReviewerId = peopleMgr.EmployeeCode;
                        model.CurrentReviewer = peopleMgr.Name;
                        break;
                    case "IT":

                        var ITMgr = await _context.Requesters.Where(e => e.Position == "Head of IT").FirstOrDefaultAsync();
                        model.CurrentReviewerId = ITMgr.EmployeeCode;
                        model.CurrentReviewer = ITMgr.Name;
                        break;

                    case "Finance":

                        var FinMgr = await _context.Requesters.Where(e => e.Position == "Finance Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = FinMgr.EmployeeCode;
                        model.CurrentReviewer = FinMgr.Name; break;
                    case "Procurement":

                        var procMgr = await _context.Requesters.Where(e => e.Position == "Manager ( Procurement)").FirstOrDefaultAsync();
                        model.CurrentReviewerId = procMgr.EmployeeCode;
                        model.CurrentReviewer = procMgr.Name; break;
                    case "Security":

                        var secMgr = await _context.Requesters.Where(e => e.Position == "Security Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = secMgr.EmployeeCode;
                        model.CurrentReviewer = secMgr.Name; break;
                    case "QHSE":

                        var safteyMgr = await _context.Requesters.Where(e => e.Position == "Acting Head of HSE").FirstOrDefaultAsync();
                        model.CurrentReviewerId = safteyMgr.EmployeeCode;
                        model.CurrentReviewer = safteyMgr.Name; break;
                    case "Customer Service":

                        var cusMgr = await _context.Requesters.Where(e => e.Position == "Customer Service Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = cusMgr.EmployeeCode;
                        model.CurrentReviewer = cusMgr.Name; break;
                    case "COMMERCIAL":

                        var comMgr = await _context.Requesters.Where(e => e.Position == "Commercial Manager - Container").FirstOrDefaultAsync();
                        model.CurrentReviewerId = comMgr.EmployeeCode;
                        model.CurrentReviewer = comMgr.Name; break;
                    case "Operations":

                        var opMgr = await _context.Requesters.Where(e => e.Position == "Execution Manager ( Container)").FirstOrDefaultAsync();
                        model.CurrentReviewerId = opMgr.EmployeeCode;
                        model.CurrentReviewer = opMgr.Name; break;
                    case "Technical":

                        var TechMgr = await _context.Requesters.Where(e => e.Position == "Head of Engineering").FirstOrDefaultAsync();
                        model.CurrentReviewerId = TechMgr.EmployeeCode;
                        model.CurrentReviewer = TechMgr.Name; break;

                    default:
                        break;
                }

                model.ChangedBy = currentUser.EmployeeCode;

                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_Create_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_Create_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                if (ex.Message == "An error occurred while saving the entity changes. See the inner exception for details.")
                {
                    ModelState.AddModelError(string.Empty, $"There's no data for the code: {travelDesk.RequesterCode}");
                    return View();
                }
                else
                {
                    return View();
                }
            }
           
        }

        // GET: TravelDesks/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.TravelDesks == null)
            {
                return NotFound();
            }

            var travelDesk = await _context.TravelDesks.FindAsync(id);
            var model = new TravelDeskViewModel();
            if (travelDesk == null)
            {
                return NotFound();
            }
            model.RequesterCode = travelDesk.RequesterCode;
            model.RequesterName = travelDesk.RequesterName;
            model.Title = travelDesk.Title;
            model.RequesterDept = travelDesk.RequesterDept;
            model.CurrentReviewer = travelDesk.CurrentReviewer;
            model.CurrentReviewerId = travelDesk.CurrentReviewerId;
            model.ReqNumber = travelDesk.ReqNumber;
            model.Nationality = travelDesk.Nationality;
            model.RequestPurpose = travelDesk.RequestPurpose;
            model.CheckIn = travelDesk.CheckIn;
            model.CheckOut = travelDesk.CheckOut;
            model.MissionAddress = travelDesk.MissionAddress;
            model.MethodOfPayment = travelDesk.MethodOfPayment;
            model.TripDirection = travelDesk.TripDirection;
            model.DestinationCountry = travelDesk.DestinationCountry;
            model.ExpectedTravelTime = travelDesk.ExpectedTravelTime;
            model.CostAllocation = travelDesk.CostAllocation;
            model.Remarks = travelDesk.Remarks;
            model.RequestedFor = travelDesk.RequestedFor;
            model.Departure = travelDesk.Departure;
            model.ReturnBack = travelDesk.ReturnBack;
            model.CreatedAt = travelDesk.CreatedAt;
            model.CurrentReviewer = travelDesk.CurrentReviewer;
            model.RequesterCode = travelDesk.RequesterCode;
            model.RequesterId = travelDesk.RequesterId;
            model.Remarks = travelDesk.Remarks;

            model.Request = travelDesk.Request;


            model.MgrStatus = MgrStatuses.Pending;
            model.Status = Models.DeptAdminStatutses.Pending.ToString();
            model.ApprovalNotes = string.Empty;
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", travelDesk.CurrentReviewerId);
            return View(model);
        }

        // POST: TravelDesks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,  TravelDeskViewModel travelDesk)
        {
            if (id != travelDesk.Id)
            {
                return NotFound();
            }
            

            try
            {
                string uniqueFileName = null;
                var model = await _context.TravelDesks.Where(v => v.Id == id).FirstOrDefaultAsync();

                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var requesterName = _context.Requesters.Where(c => c.EmployeeCode == travelDesk.RequesterCode).FirstOrDefault();
                var uploadsFolder = Directory.CreateDirectory($@"{_hostingEnvironment.WebRootPath}\{travelDesk.ReqNumber}_Travel Desk");
                if (travelDesk.Attach != null)
                {
                    string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{travelDesk.ReqNumber}_Travel Desk");
                    uniqueFileName = travelDesk.Attach.FileName;
                    string filePath = Path.Combine(uploadsFolder1, uniqueFileName);
                    FileStream file = new FileStream(filePath, FileMode.Create);

                    await travelDesk.Attach.CopyToAsync(file);
                    model.Attach = uniqueFileName;

                    file.Close();
                    file.Dispose();
                }
                if (travelDesk.RequesterCode == null)
                {
                    travelDesk.RequesterCode = currentUser.EmployeeCode;
                    travelDesk.RequesterName = currentUser.Name;
                    travelDesk.Title = currentUser.Position;
                    travelDesk.RequesterDept = currentUser.Dept;
                    travelDesk.CurrentReviewer = currentUser.MgrName;
                    travelDesk.CurrentReviewerId = currentUser.MgrCode;
                    travelDesk.RequesterId = currentUser.EmployeeCode;
                }
                else
                {
                    travelDesk.RequesterCode = requesterName.EmployeeCode;
                    travelDesk.RequesterName = requesterName.Name;
                    travelDesk.RequesterDept = requesterName.Dept;
                    travelDesk.Title = requesterName.Position;
                    travelDesk.CurrentReviewer = requesterName.MgrName;
                    travelDesk.CurrentReviewerId = requesterName.MgrCode;
                    travelDesk.RequesterId = currentUser.EmployeeCode;

                }
                model.RequesterCode = travelDesk.RequesterCode;
                model.RequesterName = travelDesk.RequesterName;
                model.Title = travelDesk.Title;
                model.RequesterDept = travelDesk.RequesterDept;
                model.CurrentReviewer = travelDesk.CurrentReviewer;
                model.CurrentReviewerId = travelDesk.CurrentReviewerId;
                model.ReqNumber = travelDesk.ReqNumber;
                model.Nationality = travelDesk.Nationality;
                model.RequestPurpose = travelDesk.RequestPurpose;
                model.CheckIn = travelDesk.CheckIn;
                model.CheckOut = travelDesk.CheckOut;
                model.MissionAddress = travelDesk.MissionAddress;
                model.MethodOfPayment = travelDesk.MethodOfPayment;
                model.TripDirection = travelDesk.TripDirection;
                model.DestinationCountry = travelDesk.DestinationCountry;
                model.ExpectedTravelTime = travelDesk.ExpectedTravelTime;
                model.CostAllocation = travelDesk.CostAllocation;
                model.Remarks = travelDesk.Remarks;
                model.RequestedFor = travelDesk.RequestedFor;
                model.Departure = travelDesk.Departure;
                model.ReturnBack = travelDesk.ReturnBack;
                model.CreatedAt = travelDesk.CreatedAt;
                model.CurrentReviewer = travelDesk.CurrentReviewer;
                model.RequesterCode = travelDesk.RequesterCode;
                model.Remarks = travelDesk.Remarks;

                model.Request = travelDesk.Request;


                model.MgrStatus = MgrStatuses.Pending;
                model.Status = Models.DeptAdminStatutses.Pending.ToString();
                model.ApprovalNotes = string.Empty;


                switch (model.RequesterDept)
                {
                    case "Adminstration":
                        var adminMgr = await _context.Requesters.Where(e => e.Position == "Administration Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = adminMgr.EmployeeCode;
                        model.CurrentReviewer = adminMgr.Name;
                        break;

                    case "People Department":

                        var peopleMgr = await _context.Requesters.Where(e => e.Position == "Head Of People").FirstOrDefaultAsync();
                        model.CurrentReviewerId = peopleMgr.EmployeeCode;
                        model.CurrentReviewer = peopleMgr.Name;
                        break;
                    case "IT":

                        var ITMgr = await _context.Requesters.Where(e => e.Position == "Head of IT").FirstOrDefaultAsync();
                        model.CurrentReviewerId = ITMgr.EmployeeCode;
                        model.CurrentReviewer = ITMgr.Name;
                        break;

                    case "Finance":

                        var FinMgr = await _context.Requesters.Where(e => e.Position == "Finance Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = FinMgr.EmployeeCode;
                        model.CurrentReviewer = FinMgr.Name; break;
                    case "Procurement":

                        var procMgr = await _context.Requesters.Where(e => e.Position == "Manager ( Procurement)").FirstOrDefaultAsync();
                        model.CurrentReviewerId = procMgr.EmployeeCode;
                        model.CurrentReviewer = procMgr.Name; break;
                    case "Security":

                        var secMgr = await _context.Requesters.Where(e => e.Position == "Security Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = secMgr.EmployeeCode;
                        model.CurrentReviewer = secMgr.Name; break;
                    case "QHSE":

                        var safteyMgr = await _context.Requesters.Where(e => e.Position == "Acting Head of HSE").FirstOrDefaultAsync();
                        model.CurrentReviewerId = safteyMgr.EmployeeCode;
                        model.CurrentReviewer = safteyMgr.Name; break;
                    case "Customer Service":

                        var cusMgr = await _context.Requesters.Where(e => e.Position == "Customer Service Manager").FirstOrDefaultAsync();
                        model.CurrentReviewerId = cusMgr.EmployeeCode;
                        model.CurrentReviewer = cusMgr.Name; break;
                    case "COMMERCIAL":

                        var comMgr = await _context.Requesters.Where(e => e.Position == "Commercial Manager - Container").FirstOrDefaultAsync();
                        model.CurrentReviewerId = comMgr.EmployeeCode;
                        model.CurrentReviewer = comMgr.Name; break;
                    case "Operations":

                        var opMgr = await _context.Requesters.Where(e => e.Position == "Execution Manager ( Container)").FirstOrDefaultAsync();
                        model.CurrentReviewerId = opMgr.EmployeeCode;
                        model.CurrentReviewer = opMgr.Name; break;
                    case "Technical":

                        var TechMgr = await _context.Requesters.Where(e => e.Position == "Head of Engineering").FirstOrDefaultAsync();
                        model.CurrentReviewerId = TechMgr.EmployeeCode;
                        model.CurrentReviewer = TechMgr.Name; break;

                    default:
                        break;
                }
                if (ModelState.IsValid)
                {

                model.ChangedBy = currentUser.EmployeeCode;
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_Edit_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_Edit_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                if (ex.Message == "An error occurred while saving the entity changes. See the inner exception for details.")
                {
                    ModelState.AddModelError(string.Empty, $"There's no data for the code: {travelDesk.RequesterCode}");
                    return View();
                }
                else
                {
                    return View();
                }
            }
           
        }

        // GET: TravelDesks/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.TravelDesks == null)
            {
                return NotFound();
            }

            var travelDesk = await _context.TravelDesks
                .Include(t => t.CurrentReview)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (travelDesk == null)
            {
                return NotFound();
            }

            return View(travelDesk);
        }

        // POST: TravelDesks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.TravelDesks == null)
            {
                return Problem("Entity set 'RequestContext.TravelDesks'  is null.");
            }
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            var travelDesk = await _context.TravelDesks.FindAsync(id);
            if (travelDesk != null)
            {
                travelDesk.ChangedBy = currentUser.EmployeeCode.ToString();
                _context.TravelDesks.Update(travelDesk);
                await _context.SaveChangesAsync();
                _context.TravelDesks.Remove(travelDesk);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> MgrIndex()
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
             var mgrRequests =   _context.TravelDesks.Where(w => w.RequesterId == currentUser.EmployeeCode || w.RequesterCode == currentUser.EmployeeCode);

                if (mgrRequests.IsNullOrEmpty())
                {
                    return View(await _context.TravelDesks.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).ToListAsync());
                }
                else
                {
                    return View(await mgrRequests.ToListAsync()); 
                }
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_MgrIndex_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_MgrIndex_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("Index", "Error");
            }



        }
        public async Task<IActionResult> MgrEdit(Guid? id)
        {


            var traveldesk = await _context.TravelDesks.FindAsync(id);

            return View(traveldesk);
        }

        // POST: CarRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MgrEdit(Guid id, TravelDesk travelDesk)
        {
            var requester = await _context.Requesters.Where(e => e.EmployeeCode == travelDesk.RequesterId).FirstOrDefaultAsync();
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            if (id != travelDesk.Id)
            {
                return NotFound();
            }

            try
            {
                var adminMgr = await _context.Requesters.Where(e => e.Position == "Administration Manager").FirstOrDefaultAsync();

                if (travelDesk.MgrStatus == MgrStatuses.Approve)
                {
                    travelDesk.Status = DeptAdminStatuses.Pending.ToString();
                    travelDesk.DeptAdminStatus = DeptAdminStatuses.Pending;
                    travelDesk.CurrentReviewerId =adminMgr.EmployeeCode;
                    travelDesk.CurrentReviewer = adminMgr.Name;
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "","Travel Desk Requisition", $"{currentUser.Name}  Approved your car request.");

                }

                if (travelDesk.MgrStatus == MgrStatuses.Decline)

                {
                    travelDesk.Status = "Declined";
                    travelDesk.DeptAdminStatus = DeptAdminStatuses.None;
                    travelDesk.CurrentReviewer = string.Empty;
                     _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Travel Desk Requisition", $"{currentUser.Name}  Declined your car request.");

                }
                travelDesk.ChangedBy = currentUser.EmployeeCode;


                _context.Update(travelDesk);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_MgrEdit_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_MgrEdit_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!); 

                if (!TravelDeskExists(travelDesk.Id))
                {
                    return NotFound();
                }
                else
                {
                  return RedirectToAction("Index", "Error");
                }
            }
            return RedirectToAction(nameof(MgrIndex));
            //   return View(carRequest);
        }


        public async Task<IActionResult> AdminMgrIndex( )
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
               
                    return View(await _context.TravelDesks.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).ToListAsync());
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_AdminMgrIndex_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_AdminMgrIndex_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("Index", "Error");
            }



        }

        public async Task<IActionResult> AdminMgrEdit(Guid? id)
        {


            var traveldesk = await _context.TravelDesks.FindAsync(id);

            return View(traveldesk);
        }

        // POST: CarRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdminMgrEdit(Guid id, TravelDesk travelDesk)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            if (id != travelDesk.Id)
            {
                return NotFound();
            }

            try
            {

                if (travelDesk.DeptAdminStatus == DeptAdminStatuses.Approve)
                {

                    travelDesk.Status = "Approved";
                    travelDesk.CurrentReviewer = string.Empty;
                    // _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name}  approved your car request.");
                }
                else
                {
                    travelDesk.Status = "Declined";
                    travelDesk.CurrentReviewer = string.Empty;
                    travelDesk.MgrStatus = MgrStatuses.None;
                   //  _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name}  declined your car request.");

                }
                travelDesk.ChangedBy = currentUser.EmployeeCode;

                _context.Update(travelDesk);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_TravelDesks_AdminMgrEdit_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,TravelDesks_AdminMgrEdit_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                if (!TravelDeskExists(travelDesk.Id))
                {
                    return NotFound();
                }
                else
                {
                 
                    return RedirectToAction("Index", "Error");
                }
               

            }
            return RedirectToAction(nameof(AdminMgrIndex));
            
        }
        private bool TravelDeskExists(Guid id)
        {
          return _context.TravelDesks.Any(e => e.Id == id);
        }
    }
}
