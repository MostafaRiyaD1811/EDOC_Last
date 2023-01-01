using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Document.Models;
using Microsoft.IdentityModel.Tokens;
using Document.Helpers;
using Document.ViewModels;
using Azure.Core;
using RestSharp;

namespace Document.Controllers
{
    public class DomainAccountsController : Controller
    {
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;

        public DomainAccountsController(RequestContext context , IUtilities utilities)
        {
            _context = context;
            _utilities = utilities;
        }

        public async Task<IActionResult> RedirectTo()
        {

            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var mgrUser = _context.Requesters.FirstOrDefault(m => m.LDap == currentUser.LDap);
                var mgrOrITAdmin = _context.DomainAccounts.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).FirstOrDefault();
                var mgrOrITMgr = _context.DomainAccounts.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode && w.MgrStatus != MgrStatuses.Approve).FirstOrDefault();

                if (mgrUser != null)
                {
                    var isMgr = _context.Requesters.Where(m => m.MgrCode == mgrUser.EmployeeCode);
                    var isITManager = _context.Requesters.Where(m => m.ITMgr == mgrUser.ITMgr && m.ITMgr == 1);
                    var isITAdmin = _context.Requesters.Where(m => m.ITAdmin == mgrUser.ITAdmin && m.ITAdmin == 1);
                    
                    
                    if (!isMgr.IsNullOrEmpty() && mgrOrITAdmin != null && mgrOrITMgr!= null)
                    {
                        return RedirectToAction(nameof(MgrIndex));
                    }
                    else if (!isITManager.IsNullOrEmpty())
                    {
                        return RedirectToAction(nameof(ITMgrIndex));
                    }
                    else if (!isITAdmin.IsNullOrEmpty())
                    {
                        return RedirectToAction(nameof(ITAdminIndex));
                    }
                    else
                    {
                        return RedirectToAction(nameof(Index));
                    }
                }
                else
                {
                    return RedirectToAction(nameof(Index));
                }





            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Login");
            }

        }
        // GET: DomainAccounts
        public async Task<IActionResult> Index()
        
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            return View(await _context.DomainAccounts.Where(e => e.RequesterId == currentUser.EmployeeCode ).OrderByDescending(e=>e.CreatedAt).ToListAsync());
        }

        public async Task<IActionResult> MgrIndex()
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var mgrUser = _context.Requesters.FirstOrDefault(m => m.LDap == currentUser.LDap);
                



                if (mgrUser != null)
                {
                    var isMgr = _context.Requesters.Where(m => m.MgrCode == mgrUser.EmployeeCode);
                    if (!isMgr.IsNullOrEmpty())
                    {
                    return View(await _context.DomainAccounts.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode && w.MgrStatus!=MgrStatuses.Approve).ToListAsync());

                    }
                    return RedirectToAction("Index", "Home");
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
        public async Task<IActionResult> ITMgrIndex()
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.ITMgr == 1 && currentUser.Position== "Head of IT")
                {
                    return View(await _context.DomainAccounts.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).ToListAsync());

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

        public async Task<IActionResult> ITAdminIndex(int? searching)
        {
            try
            {

                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.ITAdmin == 1)
                {
                    return View(await _context.DomainAccounts.Where(w => w.CurrentReviewerId == "IT Admin" || w.OU ==null||w.LoginName == null || w.DisplayName==null).ToListAsync());
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

        // GET: DomainAccounts/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.DomainAccounts == null)
            {
                return NotFound();
            }

            var domainAccount = await _context.DomainAccounts
                .Include(d => d.CurrentReview)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (domainAccount == null)
            {
                return NotFound();
            }

            return View(domainAccount);
        }

        // GET: DomainAccounts/Create
        public IActionResult Create()
        {
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode");
            return View();
        }

        // POST: DomainAccounts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DomainAccount domainAccount)
        {
            try
            {

                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                var currentUserMgr = await _context.Requesters.Where(e => e.EmployeeCode == currentUser.MgrCode).FirstOrDefaultAsync(); 
                var requesterName = _context.Requesters.Where(c => c.EmployeeCode == domainAccount.RequesterCode).FirstOrDefault();
                if (requesterName != null)
                {
                    domainAccount.RequesterName = requesterName.Name;
                    domainAccount.RequesterJobTitle = requesterName.Position;
                    domainAccount.RequesterCompany = requesterName.Company;
                    domainAccount.RequesterCode = requesterName.EmployeeCode;
                }

                else
                {
                    ModelState.AddModelError(string.Empty, $"There's no data for the code: {domainAccount.RequesterCode}");
                    return View();
                }
                
                domainAccount.Id = Guid.NewGuid();
                domainAccount.CreatedAt = DateTime.Now;
                domainAccount.RequesterId = currentUser.EmployeeCode;
                domainAccount.MgrStatus = MgrStatuses.Pending;
                domainAccount.Status = MgrStatuses.Pending.ToString();
                domainAccount.RequesterDept = currentUser.Dept;
                
                var ldap = string.Empty;
                switch (domainAccount.RequesterDept)
                {
                        case "Adminstration":
                            var adminMgr = await _context.Requesters.Where(e => e.Position == "Administration Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = adminMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = adminMgr.Name;
                        ldap= adminMgr.LDap;
                            break;

                        case "People Department":

                            var peopleMgr = await _context.Requesters.Where(e => e.Position == "Head Of People").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = peopleMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = peopleMgr.Name;
                        ldap = peopleMgr.LDap;
                        break;
                        case "IT":

                            var ITMgr = await _context.Requesters.Where(e => e.Position == "Head of IT").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = ITMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = ITMgr.Name;
                            ldap = ITMgr.LDap;
                        break;

                        case "Finance":

                            var FinMgr = await _context.Requesters.Where(e => e.Position == "Finance Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = FinMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = FinMgr.Name; ldap = FinMgr.LDap;
                        break;
                        case "Procurement":

                            var procMgr = await _context.Requesters.Where(e => e.Position == "Manager ( Procurement)").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = procMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = procMgr.Name; ldap = procMgr.LDap;
                        break;
                        case "Security":

                            var secMgr = await _context.Requesters.Where(e => e.Position == "Security Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = secMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = secMgr.Name; ldap = secMgr.LDap;
                        break;
                        case "QHSE":

                            var safteyMgr = await _context.Requesters.Where(e => e.Position == "Acting Head of HSE").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = safteyMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = safteyMgr.Name;     ldap = safteyMgr.LDap;
                        break;
                        case "Customer Service":

                            var cusMgr = await _context.Requesters.Where(e => e.Position == "Customer Service Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = cusMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = cusMgr.Name; ldap = cusMgr.LDap;
                        break;
                        case "Commercial":

                            var comMgr = await _context.Requesters.Where(e => e.Position == "Commercial Manager - Container").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = comMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = comMgr.Name; ldap = comMgr.LDap;
                        break;
                        case "Operations":

                            var opMgr = await _context.Requesters.Where(e => e.Position == "Execution Manager ( Container)").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = opMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = opMgr.Name; ldap = opMgr.LDap;
                        break;
                        case "Technical":

                            var TechMgr = await _context.Requesters.Where(e => e.Position == "Head of Engineering").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = TechMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = TechMgr.Name; ldap = TechMgr.LDap;
                        break;

                        default:
                         break;
                }
                domainAccount.ChangedBy = currentUser.EmployeeCode;

                _context.Add(domainAccount);
                    await _context.SaveChangesAsync();
                 _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUser.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account", $"{currentUser.Name} Create a domain account.");
                  _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(ldap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name} Create a domain account.");

                return RedirectToAction(nameof(Index));
                
            }
            catch (Exception ex)
            {

                if (ex.Message == "An error occurred while saving the entity changes. See the inner exception for details.")
                {
                    ModelState.AddModelError(string.Empty, $"There's no data for the code: {domainAccount.RequesterCode}");
                    return View();
                }
                else
                {
                    var guid = Guid.NewGuid().ToString();
                    string fileName = $@"C:\domainAccount\Log_domainAccountCreate_{DateTime.Today:yyyyMMdd}";
                    _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,domainAccount_error,{guid}");
                    _utilities.WriteLog(fileName, ex?.StackTrace!);
                    return RedirectToAction("Index", "Error");
                }
            }
        }

        // GET: DomainAccounts/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.DomainAccounts == null)
            {
                return NotFound();
            }

            var domainAccount = await _context.DomainAccounts.FindAsync(id);

            if (domainAccount == null)
            {
                return NotFound();
            }
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", domainAccount.CurrentReviewerId);
            return View(domainAccount);
        }

        // POST: DomainAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, DomainAccount domainAccount)
        {
            if (id != domainAccount.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                        var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                    var currentUserMgr = _context.Requesters.Where(e => e.EmployeeCode == currentUser.MgrCode).FirstOrDefault();
                    var ldap = string.Empty;
                    switch (domainAccount.RequesterDept)
                    {
                        case "Adminstration":
                            var adminMgr = await _context.Requesters.Where(e => e.Position == "Administration Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = adminMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = adminMgr.Name;
                            ldap = adminMgr.LDap;
                            break;

                        case "People Department":

                            var peopleMgr = await _context.Requesters.Where(e => e.Position == "Head Of People").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = peopleMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = peopleMgr.Name;
                            ldap = peopleMgr.LDap;
                            break;
                        case "IT":

                            var ITMgr = await _context.Requesters.Where(e => e.Position == "Head of IT").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = ITMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = ITMgr.Name;
                            ldap = ITMgr.LDap;
                            break;

                        case "Finance":

                            var FinMgr = await _context.Requesters.Where(e => e.Position == "Finance Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = FinMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = FinMgr.Name; ldap = FinMgr.LDap;
                            break;
                        case "Procurement":

                            var procMgr = await _context.Requesters.Where(e => e.Position == "Manager ( Procurement)").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = procMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = procMgr.Name; ldap = procMgr.LDap;
                            break;
                        case "Security":

                            var secMgr = await _context.Requesters.Where(e => e.Position == "Security Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = secMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = secMgr.Name; ldap = secMgr.LDap;
                            break;
                        case "QHSE":

                            var safteyMgr = await _context.Requesters.Where(e => e.Position == "Acting Head of HSE").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = safteyMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = safteyMgr.Name; ldap = safteyMgr.LDap;
                            break;
                        case "Customer Service":

                            var cusMgr = await _context.Requesters.Where(e => e.Position == "Customer Service Manager").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = cusMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = cusMgr.Name; ldap = cusMgr.LDap;
                            break;
                        case "Commercial":

                            var comMgr = await _context.Requesters.Where(e => e.Position == "Commercial Manager - Container").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = comMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = comMgr.Name; ldap = comMgr.LDap;
                            break;
                        case "Operations":

                            var opMgr = await _context.Requesters.Where(e => e.Position == "Execution Manager ( Container)").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = opMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = opMgr.Name; ldap = opMgr.LDap;
                            break;
                        case "Technical":

                            var TechMgr = await _context.Requesters.Where(e => e.Position == "Head of Engineering").FirstOrDefaultAsync();
                            domainAccount.CurrentReviewerId = TechMgr.EmployeeCode;
                            domainAccount.CurrentReviewer = TechMgr.Name; ldap = TechMgr.LDap;
                            break;

                        default:
                            break;
                    }
                    domainAccount.ITMgrStatus = ITMgrStatuses.None;
                    domainAccount.MgrStatus = MgrStatuses.Pending;
                    domainAccount.ITAdminStatus = ITAdminStatuses.None;
                    domainAccount.Status = "Pending";
                    domainAccount.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(domainAccount);
                    await _context.SaveChangesAsync();
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(ldap), "Youssef.AboZaid@dpworld.com", "", "Domain Account", $"{currentUser.Name} update the domain account No. {domainAccount.ReqNumber} Plese check Your Inbox.");

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DomainAccountExists(domainAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        var guid = Guid.NewGuid().ToString();
                        string fileName = $@"C:\domainAccount\Log_domainAccountEdit_{DateTime.Today:yyyyMMdd}";
                        _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,domainAccount_error,{guid}");
                        _utilities.WriteLog(fileName, ex?.StackTrace!);
                        return RedirectToAction("Index", "Error");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", domainAccount.CurrentReviewerId);
            return View(domainAccount);
        }
        // GET: DomainAccounts/Edit/5
        public async Task<IActionResult> MgrEdit(Guid? id)
        {
            if (id == null || _context.DomainAccounts == null)
            {
                return NotFound();
            }

            var domainAccount = await _context.DomainAccounts.FindAsync(id);
            domainAccount.ApprovalNotes = string.Empty;
            if (domainAccount == null)
            {
                return NotFound();
            }
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", domainAccount.CurrentReviewerId);
            return View(domainAccount);
        }

        // POST: DomainAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MgrEdit(Guid id, DomainAccount domainAccount)
        {
            if (id != domainAccount.Id)
            {
                return NotFound();
            }
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            //if (ModelState.IsValid)
            //{
            try
                {
                var requester = await _context.Requesters.Where(e => e.EmployeeCode == domainAccount.RequesterId).FirstOrDefaultAsync();

                if (domainAccount.MgrStatus == MgrStatuses.Approve)
                {
                      var iTHead=  await _context.Requesters.Where(e => e.Position == "Head of IT").FirstOrDefaultAsync();
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"{currentUser.Name} Approved The request");
                     _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(iTHead.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"A domain account request in your inbox");
                        domainAccount.Status = ITAdminStatuses.Pending.ToString();
                        domainAccount.ITMgrStatus = ITMgrStatuses.Pending;
                        domainAccount.CurrentReviewer = iTHead.Name;
                        domainAccount.CurrentReviewerId = iTHead.EmployeeCode;
                }

                    else if (domainAccount.MgrStatus == MgrStatuses.Decline)
                    {
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"{currentUser.Name} Declined The request");
                        domainAccount.Status = "Declined";
                        domainAccount.ITMgrStatus = ITMgrStatuses.None;
                        domainAccount.CurrentReviewer = string.Empty;
                        domainAccount.CurrentReviewerId = string.Empty;                       
                    }
                domainAccount.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(domainAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DomainAccountExists(domainAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                    var guid = Guid.NewGuid().ToString();
                    string fileName = $@"C:\domainAccount\Log_domainAccountMgrEdit_{DateTime.Today:yyyyMMdd}";
                    _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,domainAccount_error,{guid}");
                    _utilities.WriteLog(fileName, ex?.StackTrace!);
                    return RedirectToAction("Index", "Error");
                    }
            }
                return RedirectToAction(nameof(MgrIndex));
            //}
           
        }
        // GET: DomainAccounts/Edit/5
        public async Task<IActionResult> ITMgrEdit(Guid? id)
        {
            if (id == null || _context.DomainAccounts == null)
            {
                return NotFound();
            }

            var domainAccount = await _context.DomainAccounts.FindAsync(id);
            domainAccount.ApprovalNotes = string.Empty;
            if (domainAccount == null)
            {
                return NotFound();
            }
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", domainAccount.CurrentReviewerId);
            return View(domainAccount);
        }

        // POST: DomainAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ITMgrEdit(Guid id, DomainAccount domainAccount)
        {
            if (id != domainAccount.Id)
            {
                return NotFound();
            }

           
                try
                {
                var requester = await _context.Requesters.Where(e => e.EmployeeCode == domainAccount.RequesterId).FirstOrDefaultAsync();
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                var iTAdmin = await _context.Requesters.Where(e => e.ITAdmin == 1).FirstOrDefaultAsync();
                if (domainAccount.ITMgrStatus == ITMgrStatuses.Approve)
                    {                   
                          _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"{currentUser.Name} Approved The request");
                         _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(iTAdmin.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"A domain account request in your inbox");
                        domainAccount.Status = ITAdminStatuses.Pending.ToString();
                        domainAccount.ITAdminStatus = ITAdminStatuses.Pending;
                        domainAccount.CurrentReviewer = "IT Admin";
                        domainAccount.CurrentReviewerId = "IT Admin";
                    }

                    else if(domainAccount.MgrStatus == MgrStatuses.Decline)
                    {
                        _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"{currentUser.Name} Declined The request");
                        domainAccount.Status = "Declined";
                        domainAccount.ITMgrStatus = ITMgrStatuses.None;
                        domainAccount.CurrentReviewer = string.Empty;
                        domainAccount.CurrentReviewerId = string.Empty;
                    }
                domainAccount.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(domainAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!DomainAccountExists(domainAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                    var guid = Guid.NewGuid().ToString();
                    string fileName = $@"C:\domainAccount\Log_domainAccountITMgrEdit_{DateTime.Today:yyyyMMdd}";
                    _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,domainAccount_error,{guid}");
                    _utilities.WriteLog(fileName, ex?.StackTrace!);
                    return RedirectToAction("Index", "Error");
                }
                }
                return RedirectToAction(nameof(ITMgrIndex));
            
         
            
        }
        // GET: DomainAccounts/Edit/5
        public async Task<IActionResult> ITAdminEdit(Guid? id)
        {
            if (id == null || _context.DomainAccounts == null)
            {
                return NotFound();
            }

            var domainAccount = await _context.DomainAccounts.FindAsync(id);
            domainAccount.ApprovalNotes = string.Empty;
            if (domainAccount == null)
            {
                return NotFound();
            }
            ViewData["CurrentReviewerId"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", domainAccount.CurrentReviewerId);
            return View(domainAccount);
        }

        // POST: DomainAccounts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ITAdminEdit(Guid id, DomainAccount domainAccount)
        {
            if (id != domainAccount.Id)
            {
                return NotFound();
            }

           
            try
            {
                var requester = await _context.Requesters.Where(e => e.EmployeeCode == domainAccount.RequesterId).FirstOrDefaultAsync();
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (domainAccount.ITAdminStatus == ITAdminStatuses.Approve)
                    {
                    HttpClientHandler clientHandler = new()
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                    };
                    var client = new RestClient(clientHandler);
                    var request = new RestRequest($"https://appsportali.dpworldsokhna.com:500/sms_ws/api/sms/SendSMS", Method.Post);
                    var body = $"{{\r\n    \"MsgText\": \"{currentUser.Name} Approved The request and  OU :{domainAccount.OU}\n, Login name :{domainAccount.LoginName}\n ,Display Name:{domainAccount.DisplayName}\",\r\n " +
                  $"   \"MobileNo\": \"+2{domainAccount.RequesterPhone}\",\r\n " +
                  $"   \"pol\":\"E-Document\",\r\n   ";
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    RestResponse response = await client.ExecuteAsync(request);
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"{currentUser.Name} Approved The request and  OU :{domainAccount.OU}\n, Login name :{domainAccount.LoginName}\n ,Display Name:{domainAccount.DisplayName}");
                    domainAccount.Status = "Approved";
                    domainAccount.CurrentReviewer = "Completed";
                        domainAccount.CurrentReviewerId = "Completed";
                    }

                    else if (domainAccount.ITAdminStatus == ITAdminStatuses.Decline)
                    {
                     _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Domain Account Request", $"{currentUser.Name} Declined The request");

                    domainAccount.Status = "Declined";
                        domainAccount.ITAdminStatus = ITAdminStatuses.None;
                        domainAccount.CurrentReviewer = string.Empty;
                        domainAccount.CurrentReviewerId = string.Empty;
                    }
                domainAccount.ChangedBy = currentUser.EmployeeCode;
                    _context.Update(domainAccount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DomainAccountExists(domainAccount.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(ITAdminIndex));
            
        }
        // GET: DomainAccounts/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.DomainAccounts == null)
            {
                return NotFound();
            }

            var domainAccount = await _context.DomainAccounts
                .Include(d => d.CurrentReview)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (domainAccount == null)
            {
                return NotFound();
            }

            return View(domainAccount);
        }

        // POST: DomainAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (_context.DomainAccounts == null)
            {
                return Problem("Entity set 'RequestContext.DomainAccounts'  is null.");
            }
            var domainAccount = await _context.DomainAccounts.FindAsync(id);
            if (domainAccount != null)
            {
                domainAccount.ChangedBy = currentUser.EmployeeCode.ToString();
                _context.DomainAccounts.Update(domainAccount);
                await _context.SaveChangesAsync();
                _context.DomainAccounts.Remove(domainAccount);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DomainAccountExists(Guid id)
        {
          return _context.DomainAccounts.Any(e => e.Id == id);
        }
    }
}
