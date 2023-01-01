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
using Google.Apis.Admin.Directory.directory_v1.Data;
using Rotativa.AspNetCore;

namespace Document.Controllers
{
    public class VouchersController : Controller
    {
        private readonly RequestContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IUtilities _utilities;

        public VouchersController(RequestContext context, IWebHostEnvironment hostingEnvironment, IUtilities utilities)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _utilities = utilities;
        }

        public async Task<IActionResult> RedirectTo()
        {

            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));


                if (currentUser != null)
                {

                    if (currentUser.HRUser == 1)
                    {
                        return RedirectToAction("Index");
                    }
                    else if (currentUser.HRReviewer == 1)
                    {
                        return RedirectToAction("HRReviewerIndex");
                    }

                    else if (currentUser.HRMgr == 1)
                    {
                        return RedirectToAction("HRMgrIndex");
                    }
                    else if (currentUser.FinanceInitiator == 1)
                    {
                        return RedirectToAction("FinanceInitiatorIndex");
                    }
                    else if (currentUser.FinanceReviewr == 1)
                    {
                        return RedirectToAction("FinanceReviewerIndex");
                    }
                    else if (currentUser.FinanceMgr == 1)
                    {
                        return RedirectToAction("FinanceMgrIndex");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            


            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Login");
            }

        }
        // GET: Vouchers
        public async Task<IActionResult> Index(string searching)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            if (currentUser.HRUser == 1 || currentUser.FinanceInitiator==1)
            {
                var requestContext = _context.Vouchers.Where(w => w.RequesterId == currentUser.EmployeeCode);
                return View(await requestContext.Where(P => P.VendorName.Contains(searching) || P.VendorNum.Contains(searching) || searching == null).ToListAsync());
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }

        }
        public async Task<IActionResult> HRReviewerIndex(string? searching)
        {

            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (currentUser.HRReviewer == 1 )
                {
                    return View(await _context.Vouchers.Where(w => w.CurrentReviewerId == "HR Reviewer").Where(P => P.VendorName.Contains(searching) || P.VendorNum.Contains(searching) || searching == null).ToListAsync());

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }



        }
        public async Task<IActionResult> HRMgrIndex(string? searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (currentUser.HRMgr == 1)
                {
                    return View(await _context.Vouchers.Where(w => w.CurrentReviewerId == "HR Manager").Where(P => P.VendorName.Contains(searching) || P.VendorNum.Contains(searching) || searching == null).ToListAsync());

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }

        }
        public async Task<IActionResult> FinanceInitiatorIndex(string searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (currentUser.FinanceInitiator == 1)
                {
                    return View(await _context.Vouchers.Where(w => w.CurrentReviewerId == "Finance Initiator").Where(P => P.VendorName.Contains(searching) || P.VendorNum.Contains(searching) || searching == null).ToListAsync());

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }

        }
        public IActionResult Files(int? id)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(Path.Combine(_hostingEnvironment.WebRootPath, $"{id}_Vouchers"));

                List<FileModel> files = new List<FileModel>();
                foreach (var filePath in filePaths)
                {
                    files.Add(new FileModel
                    {
                        Id = id,
                        FileName = Path.GetFileName(filePath)
                    });

                }
                return View(files);
            }
            catch (Exception)
            {

                return Content("Files Don't Exist");
            }

        }

        public IActionResult Download(string fileName, int id)
        {
            try
            {
                string path = Path.Combine(_hostingEnvironment.WebRootPath, $"{id}_Vouchers/") + fileName;

                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception)
            {

                return Content("File Doesn't Exist");


            }


        }
        // GET: Vouchers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        // GET: Vouchers/Create
        public IActionResult Create()
        {
            ViewData["RequesterId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode");
            return View();
        }

        // POST: Vouchers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VoucherViewModel voucher)
        {
            if (ModelState.IsValid)
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
               
             
                var model = new Voucher
                {
                    Id = Guid.NewGuid()
                };

                if (voucher.Attachments != null)
                {
                    string wwwPath = _hostingEnvironment.WebRootPath;
                    string contentPath = _hostingEnvironment.ContentRootPath;

                    string path = Path.Combine(_hostingEnvironment.WebRootPath, $"{voucher.ReqNumber}_Vouchers");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    List<string> uploadedFiles = new List<string>();
                    foreach (IFormFile postedFile in voucher.Attachments)
                    {
                        string fileName = Path.GetFileName(postedFile.FileName);
                        using FileStream stream = new(Path.Combine(path, fileName), FileMode.Create);
                        postedFile.CopyTo(stream);
                        uploadedFiles.Add(fileName);
                        ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
                    }

                }


                model.ReqNumber = voucher.ReqNumber;
                model.VendorNum = voucher.VendorNum;
                model.VendorName = voucher.VendorName;
                model.Amount = voucher.Amount;
                model.InvoiceDate = voucher.InvoiceDate;
                model.BeneficiaryName = voucher.BeneficiaryName;
                model.OtherNotes = voucher.OtherNotes;
                model.Type = voucher.Type;
                model.Description = voucher.Description;
                model.VoucherCurrency = voucher.VoucherCurrency;
                model.Approved = "Pending";
                if (currentUser.HRUser==1)
                {
                    model.HRReviewerStatuts = HRReviewerStatuses.Pending;
                    model.CurrentReviewerId = "HR Reviewer";
                }
                else if (currentUser.FinanceInitiator == 1){
                    model.HRReviewerStatuts = HRReviewerStatuses.Approve;
                    model.HRMgrStatuts = HRMgrStatutses.Approve;
                    model.FinanceInitiatorStatus = FinanceInitiatorStatuses.Approve;
                    model.FinanceReviewerStatus = FinanceReviewerStatuses.Pending;

                    model.CurrentReviewerId = "Finance Reviewer";
                }
               
              
                model.RequesterId = currentUser.EmployeeCode;
                model.ChangedBy = currentUser.EmployeeCode;
               

                _context.Add(model);
                await _context.SaveChangesAsync();
                // _utilities.SendMail("DPWS_eservice@dpworld.com", "HR.Sokhna@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name}Create an E-Voucher Kindly check your E-Document Inbox");

                return RedirectToAction(nameof(Index));
            }

            return View();

        }
        
        // GET: Vouchers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            ViewData["RequesterId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", voucher.RequesterId);

            var model = new VoucherViewModel();
            model.Id = voucher.Id;
            model.VendorName= voucher.VendorName;
            model.VendorNum = voucher.VendorNum;
            model.Amount = voucher.Amount;
            model.InvoiceDate= voucher.InvoiceDate;
            model.RequesterId = voucher.RequesterId;
            model.BeneficiaryName = voucher.BeneficiaryName;
            model.CreatedAt= voucher.CreatedAt; 
            model.Status= voucher.Status;   
            model.OtherNotes = voucher.OtherNotes;
            model.Description = voucher.Description;
            model.VoucherCurrency = voucher.VoucherCurrency;
            model.Details= voucher.Details;
            model.Type= voucher.Type;
            
            return View(model);
        }
      
        // POST: Vouchers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id,  VoucherViewModel voucher)
        {
            if (id != voucher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var model = await _context.Vouchers.Where(v => v.Id == id).FirstOrDefaultAsync();
                string path = Path.Combine(_hostingEnvironment.WebRootPath, $"{model.ReqNumber}_Vouchers");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                List<string> uploadedFiles = new List<string>();
                if (voucher.Attachments != null)
                {
                    foreach (IFormFile postedFile in voucher.Attachments)
                    {
                        string fileName = Path.GetFileName(postedFile.FileName);
                        using FileStream stream = new(Path.Combine(path, fileName), FileMode.Create);
                        postedFile.CopyTo(stream);
                        uploadedFiles.Add(fileName);
                        ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
                    }
                }


                model.VendorNum = voucher.VendorNum;

                model.CreatedAt= voucher.CreatedAt;
                model.VendorName= voucher.VendorName;
                model.Amount = voucher.Amount;
                model.InvoiceDate = voucher.InvoiceDate;
                model.BeneficiaryName = voucher.BeneficiaryName;
                model.OtherNotes = voucher.OtherNotes;
                model.Type = voucher.Type;
                model.Description = voucher.Description;
                model.VoucherCurrency = voucher.VoucherCurrency;
                model.RequesterId = voucher.RequesterId;
                model.Status = "Pending";

                if (currentUser.HRUser == 1)
                {
                    model.HRReviewerStatuts = HRReviewerStatuses.Pending;
                    model.HRMgrStatuts = HRMgrStatutses.None;
                    model.FinanceInitiatorStatus = FinanceInitiatorStatuses.None;
                    model.FinanceMgrStatus = FinanceMgrStatuses.None;
                    model.CurrentReviewerId = "HR Reviewer";
                    voucher.Status = "Pending";
                }
                else if (currentUser.FinanceInitiator == 1)
                {
                    model.HRReviewerStatuts = HRReviewerStatuses.Approve;
                    model.HRMgrStatuts = HRMgrStatutses.Approve;
                    model.FinanceInitiatorStatus = FinanceInitiatorStatuses.Approve;
                    model.FinanceReviewerStatus = FinanceReviewerStatuses.Pending;
                    model.CurrentReviewerId = "Finance Reviewer";
                    voucher.Status = "Pending";
                }
                
                
                   
                model.RequesterId = currentUser.EmployeeCode;
                model.ChangedBy = currentUser.EmployeeCode.ToString(); // to record modifier

                _context.Update(model);
                await _context.SaveChangesAsync();
                // _utilities.SendMail("DPWS_eservice@dpworld.com", "HR.Sokhna@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Update an E-Voucher Kindly check your E-Document Inbox");

                return RedirectToAction(nameof(Index));

            }
            return RedirectToAction(nameof(Index));
        }


        #region MyRegion
        // GET: Vouchers/Edit/5
        public async Task<IActionResult> HRReviewerEdit(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            voucher.ApprovalNotes = string.Empty;

            if (voucher == null)
            {
                return NotFound();
            }
            ViewData["RequesterId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", voucher.RequesterId);
            return View(voucher);
        }

        // POST: HRUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HRReviewerEdit(Guid id, Voucher voucher)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id != voucher.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    if (voucher.HRReviewerStatuts == HRReviewerStatuses.Approve)
                    {
                        

                        voucher.Status = string.Join("&", $"{currentUser.Name} Approved at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Approved at {DateTime.Now}";

                        voucher.HRMgrStatuts = HRMgrStatutses.Pending;
                        voucher.CurrentReviewerId = "HR Manager";

                    }

                    else if (voucher.HRReviewerStatuts == HRReviewerStatuses.Decline)

                    {

                        voucher.Status = string.Join("&", $"{currentUser.Name} Declined at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Declined at {DateTime.Now}";

                        voucher.HRMgrStatuts = HRMgrStatutses.None;

                    }
                    voucher.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!VoucherExists(voucher.Id))
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
                // _utilities.SendMail("DPWS_eservice@dpworld.com", "HR.Sokhna@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Approved an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox");

                return RedirectToAction(nameof(HRReviewerIndex));
            }
            return RedirectToAction(nameof(HRReviewerIndex));
        }

        // GET: Vouchers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        } 
        #endregion
        #region HRMGR

        // GET: Vouchers/Edit/5
        public async Task<IActionResult> HRMgrEdit(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            voucher.ApprovalNotes = string.Empty;

            if (voucher == null)
            {
                return NotFound();
            }
            ViewData["RequesterId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", voucher.RequesterId);
            return View(voucher);
        }

        // POST: HRUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HRMgrEdit(Guid id, Voucher voucher)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id != voucher.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    if (voucher.HRMgrStatuts == HRMgrStatutses.Approve)
                    {

                        voucher.Status += string.Join("&", $"{currentUser.Name} Approved at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Approved at {DateTime.Now}";
                        voucher.FinanceInitiatorStatus = FinanceInitiatorStatuses.Pending;
                        voucher.CurrentReviewerId = "Finance Initiator";
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com;HR.Sokhna@dpworld.com;", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Approved an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                    }

                    else if (voucher.HRMgrStatuts == HRMgrStatutses.Decline)

                    {
                        voucher.FinanceInitiatorStatus = FinanceInitiatorStatuses.None;
                        voucher.HRReviewerStatuts= HRReviewerStatuses.None;
                        voucher.Status = string.Join("&", $"{currentUser.Name} Declined at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Declined at {DateTime.Now}";
                        voucher.HRMgrStatuts = HRMgrStatutses.None;
                        voucher.CurrentReviewerId = "HR User";
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "HR.Sokhna@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Declined an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                    }
                    voucher.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(HRMgrIndex));
            }
            return RedirectToAction(nameof(HRMgrIndex));
        }

        // GET: Vouchers/Delete/5

        #endregion

        #region FIN_INIT

        // GET: Vouchers/Edit/5
        public async Task<IActionResult> FinanceInitiatorEdit(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            voucher.ApprovalNotes = string.Empty;

            if (voucher == null)
            {
                return NotFound();
            }
            ViewData["RequesterId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", voucher.RequesterId);
            return View(voucher);
        }

        // POST: HRUser/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinanceInitiatorEdit(Guid id, Voucher voucher)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id != voucher.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    if (voucher.FinanceInitiatorStatus == FinanceInitiatorStatuses.Approve)
                    {

                        voucher.Status += string.Join("&", $"{currentUser.Name} Approved at {DateTime.Now} ;");
                        voucher.Approved = $"{currentUser.Name} Approved at {DateTime.Now}"; 
                        voucher.FinanceReviewerStatus = FinanceReviewerStatuses.Pending;
                        voucher.CurrentReviewerId = "Finance Reviewer";
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Approved an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                    }

                    else if (voucher.FinanceInitiatorStatus == FinanceInitiatorStatuses.Decline)

                    {
                        voucher.HRMgrStatuts = HRMgrStatutses.None;
                        voucher.FinanceReviewerStatus = FinanceReviewerStatuses.None;
                        voucher.FinanceInitiatorStatus = FinanceInitiatorStatuses.None;
                        voucher.HRReviewerStatuts = HRReviewerStatuses.None;
                        voucher.Status = string.Join("&", $"{currentUser.Name} Declined at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Declined at {DateTime.Now}";

                        voucher.HRMgrStatuts = HRMgrStatutses.None;
                        voucher.CurrentReviewerId = "HR User";
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Declined an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                    }
                    voucher.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(FinanceInitiatorIndex));
            }
            return RedirectToAction(nameof(FinanceInitiatorIndex));
        }

        // GET: Vouchers/Delete/5

        #endregion

        #region Finance Reviewer & Finance MGR Actions

        #region Finace Reviewer
        [HttpGet]
        public async Task<IActionResult> FinanceReviewerIndex(string? searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (currentUser.FinanceReviewr == 1)
                {
                    var vouchers = await _context.Vouchers.Where(w => w.CurrentReviewerId == "Finance Reviewer").Where(P => P.VendorName.Contains(searching) || searching == null).ToListAsync();
                    return View(vouchers);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
               
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }



        }

        [HttpGet]
        public async Task<IActionResult> FinanceReviewerEdit(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            voucher.ApprovalNotes = string.Empty;

            if (voucher == null)
            {
                return NotFound();
            }
            ViewData["RequesterId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", voucher.RequesterId);
            return View(voucher);
        }

        [HttpPost]
        public async Task<IActionResult> FinanceReviewerEdit(Guid id, Voucher voucher)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id != voucher.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                var finInit = _context.Requesters.Where(f => f.FinanceInitiator == 1 && f.EmployeeCode==voucher.RequesterId).Any();
                try
                {
                    if (voucher.FinanceReviewerStatus == FinanceReviewerStatuses.Approve)
                    {
                        voucher.Status += string.Join("&", $"{currentUser.Name} Approved at {DateTime.Now} ;");
                        voucher.Approved = $"{currentUser.Name} Approved at {DateTime.Now}";
                        voucher.FinanceMgrStatus = FinanceMgrStatuses.Pending;
                        voucher.CurrentReviewerId = "Finance Manager";
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Approved an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                    }

                    else if (voucher.FinanceReviewerStatus == FinanceReviewerStatuses.Decline && finInit)

                    {
                         voucher.Status = string.Join("&", $"{currentUser.Name} Declined at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Declined at {DateTime.Now}";
                        voucher.HRReviewerStatuts = HRReviewerStatuses.Approve;
                        voucher.HRMgrStatuts = HRMgrStatutses.Approve;
                        voucher.FinanceInitiatorStatus = FinanceInitiatorStatuses.None;
                        voucher.CurrentReviewerId = "Finance Initiator";
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com;HR.Sokhna@dpworld.com;", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Declined an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                    }
                    else
                    {
                        voucher.Status = string.Join("&", $"{currentUser.Name} Declined at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Declined at {DateTime.Now}";
                        voucher.HRReviewerStatuts = HRReviewerStatuses.None;
                        voucher.HRMgrStatuts = HRMgrStatutses.None;
                        voucher.FinanceInitiatorStatus = FinanceInitiatorStatuses.Approve;
                        voucher.CurrentReviewerId = "HR User";
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com;HR.Sokhna@dpworld.com;", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Declined an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                    }
                    voucher.ChangedBy = currentUser.EmployeeCode;

                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.Id))
                    {
                        return NotFound();
                    }

                }
                return RedirectToAction("FinanceReviewerIndex");
            }
            return RedirectToAction("FinanceReviewerIndex");
        }
        #endregion


        #region Finace MGR
        [HttpGet]
        public async Task<IActionResult> FinanceMGRIndex(string? searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (currentUser.FinanceMgr == 1)
                {
                    return View(await _context.Vouchers.Where(w => w.CurrentReviewerId == "Finance Manager").Where(P => P.VendorName.Contains(searching) || searching == null).ToListAsync());

                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }



        }


        [HttpGet]
        public async Task<IActionResult> FinanceMGREdit(Guid? id)
        {
            if (id == null || _context.Vouchers == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            ViewData["RequesterId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", voucher.RequesterId);
            return View(voucher);
        }

        public async Task< IActionResult> DemoViewAsPDF(Guid id)
        {
            Voucher voucher = await _context.Vouchers.FirstOrDefaultAsync(c => c.Id == id);

            var report = new ViewAsPdf("Details", voucher)
            {
                PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 },
            };

            return report;
        }
        [HttpPost]
        public async Task<IActionResult> FinanceMGREdit(Guid id, Voucher voucher)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id != voucher.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {

                    if (voucher.FinanceMgrStatus == FinanceMgrStatuses.Approve)
                    {
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com;HR.Sokhna@dpworld.com;", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Approved an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");
                        voucher.Status += string.Join("&", $"{currentUser.Name} Approved at {DateTime.Now};");
                        voucher.Approved = $"{currentUser.Name} Approved at {DateTime.Now}";
                        voucher.CurrentReviewerId = "completed";                      
                    }

                    else if (voucher.FinanceMgrStatus == FinanceMgrStatuses.Decline)

                    {
                        // _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com;HR.Sokhna@dpworld.com;", "Youssef.AboZaid@dpworld.com", "", "E-Voucher", $"{currentUser.Name} Declined an E-Voucher No. {voucher.ReqNumber} Kindly check your E-Document Inbox.");

                        voucher.Status = $"{currentUser.Name} Declined at {DateTime.Now}"; ;
                        voucher.Approved = string.Join("&", $"{currentUser.Name} Declined at {DateTime.Now};");
                        voucher.HRReviewerStatuts = HRReviewerStatuses.None;
                        voucher.HRMgrStatuts = HRMgrStatutses.None;
                        voucher.FinanceInitiatorStatus = FinanceInitiatorStatuses.None;
                        voucher.FinanceReviewerStatus = FinanceReviewerStatuses.None;


                    }
                    voucher.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.Id))
                    {
                        return NotFound();
                    }

                }
                return RedirectToAction("FinanceMGRIndex");
            }
            return RedirectToAction("FinanceMGRIndex");
        }
        #endregion
        #endregion
        // POST: Vouchers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            if (_context.Vouchers == null)
            {
                return Problem("Entity set 'RequestContext.Voucher'  is null.");
            }
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher != null)
            {
                voucher.ChangedBy = currentUser.EmployeeCode.ToString();
                _context.Vouchers.Update(voucher);
                _context.Vouchers.Remove(voucher);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VoucherExists(Guid id)
        {
          return _context.Vouchers.Any(e => e.Id == id);
        }
    }
}
