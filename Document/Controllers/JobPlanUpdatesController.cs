using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Document.Models;
using Document.ViewModels;
using Document.Helpers;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using Microsoft.AspNetCore.Hosting;


namespace Document.Controllers
{
    public class JobPlanUpdatesController : Controller
    {
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public JobPlanUpdatesController(RequestContext context, IUtilities utilities, IWebHostEnvironment hostingEnvironment)
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
                    var isMgr = _context.Requesters.Where(m => m.MgrCode == mgrUser.EmployeeCode);
                    var isTechPlan = _context.Requesters.Where(m => m.TechnicalPlanning == mgrUser.TechnicalPlanning && m.TechnicalPlanning==1);

                    if (!isTechPlan.IsNullOrEmpty() )
                    {
                        return RedirectToAction(nameof(TechPlanIndex));
                    }
                    else if (!isMgr.IsNullOrEmpty())
                    {
                        return RedirectToAction(nameof(MgrIndex));
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

        // GET: JobPlanUpdates
        public async Task<IActionResult> Index()
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            return View(await _context.JobPlanUpdates.Where(w => w.RequesterId == currentUser.EmployeeCode).ToListAsync());
        }


        public async Task<IActionResult> MgrIndex()
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                return View(await _context.JobPlanUpdates.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).ToListAsync());
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public async Task<IActionResult> TechPlanIndex(int? searching)
        {
            try
            {

                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (searching.HasValue)
                    return View(await _context.JobPlanUpdates.Where(w => w.CurrentReviewerId == "Technical Planning").Where(P => P.ReqNumber.ToString().Contains(searching.ToString())).ToListAsync());
                else
                    return View(await _context.JobPlanUpdates.Where(w => w.CurrentReviewerId == "Technical Planning").ToListAsync());
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }
        }

        // GET: JobPlanUpdates/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.JobPlanUpdates == null)
            {
                return NotFound();
            }

            var jobPlanUpdate = await _context.JobPlanUpdates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobPlanUpdate == null)
            {
                return NotFound();
            }

            return View(jobPlanUpdate);
        }

        // GET: JobPlanUpdates/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: JobPlanUpdates/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(JobPlanUpdateViewModel jobPlanUpdate)
        {
            try
            {
                if (ModelState.IsValid)

                {
                        var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                    var currentUserMgr = _context.Requesters.Where(e => e.EmployeeCode == currentUser.MgrCode).FirstOrDefault();
                    var model = new JobPlanUpdate
                    {
                        Id = Guid.NewGuid()
                    };


                    if (jobPlanUpdate.Attachments != null)
                    {
                        string wwwPath = _hostingEnvironment.WebRootPath;
                        string contentPath = _hostingEnvironment.ContentRootPath;
                        string path = Path.Combine(_hostingEnvironment.WebRootPath, $"{jobPlanUpdate.ReqNumber}_Job Plan Update");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        List<string> uploadedFiles = new List<string>();
                        foreach (IFormFile postedFile in jobPlanUpdate.Attachments)
                        {
                            string fileName = Path.GetFileName(postedFile.FileName);
                            using FileStream stream = new(Path.Combine(path, fileName), FileMode.Create);
                            postedFile.CopyTo(stream);
                            uploadedFiles.Add(fileName);
                            ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
                        }

                    }


                    model.JobPlanCode = jobPlanUpdate.JobPlanCode;
                    model.RequiredAction = jobPlanUpdate.RequiredAction;
                    model.OldJobInterval = jobPlanUpdate.OldJobInterval;
                    model.NewJobInterval = jobPlanUpdate.NewJobInterval;
                    model.MaterialsAdded = jobPlanUpdate.MaterialsAdded;
                    model.MaterialsRemoved = jobPlanUpdate.MaterialsRemoved;
                    model.ReqNumber = jobPlanUpdate.ReqNumber;
                    model.Description = jobPlanUpdate.Description;
                    model.RequesterName = currentUser.Name;
                    model.CreatedAt = DateTime.Now;
                    model.RequesterMgrName = currentUser.MgrName;
                    model.CurrentReviewer = currentUser.MgrName;
                    model.Status = MgrStatuses.Pending.ToString();
                    model.MgrStatuts = MgrStatuses.Pending;
                    model.ApprovalNotes = string.Empty;
                    model.CurrentReviewerId = currentUser.MgrCode;
                    model.RequesterId = currentUser.EmployeeCode;
                    model.ChangedBy = currentUser.EmployeeCode;

                    _context.Add(model);
                    await _context.SaveChangesAsync();
                     _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUser.LDap), "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", $"{currentUser.Name} Create a job plan update.");
                      _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUserMgr.LDap), "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", $"{currentUser.Name} Create a job plan update.");

                    return RedirectToAction(nameof(Index));
                }
                return View();
            }
            catch (Exception ex)
            {

                var guid = Guid.NewGuid().ToString();
                string fileName = $@"C:\JobPlan\Log_JobPlanCreate_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,JobPlan_error,{guid}");
                _utilities.WriteLog(fileName, ex?.StackTrace!);
                return RedirectToAction("Index", "Error");
            }
            
        }

        // GET: JobPlanUpdates/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            try
            {
                if (id == null || _context.JobPlanUpdates == null)
                {
                    return NotFound();
                }

                var jobPlanUpdate = await _context.JobPlanUpdates.FindAsync(id);
                var model = new JobPlanUpdateViewModel();
                if (jobPlanUpdate == null)
                {
                    return NotFound();
                }
                model.RequesterId = jobPlanUpdate.RequesterId;
                model.JobPlanCode = jobPlanUpdate.JobPlanCode;
                model.RequiredAction = jobPlanUpdate.RequiredAction;
                model.OldJobInterval = jobPlanUpdate.OldJobInterval;
                model.NewJobInterval = jobPlanUpdate.NewJobInterval;
                model.MaterialsAdded = jobPlanUpdate.MaterialsAdded;
                model.MaterialsRemoved = jobPlanUpdate.MaterialsRemoved;
                model.ReqNumber = jobPlanUpdate.ReqNumber;
                model.Description = jobPlanUpdate.Description;
                model.RequesterName = jobPlanUpdate.RequesterName;
                model.CreatedAt = jobPlanUpdate.CreatedAt;
                model.RequesterMgrName = jobPlanUpdate.RequesterMgrName;
                model.CurrentReviewer = jobPlanUpdate.RequesterMgrName;
                model.Status = jobPlanUpdate.Status;
                model.MgrStatuts = jobPlanUpdate.MgrStatuts;
                model.ApprovalNotes = jobPlanUpdate.ApprovalNotes;
                model.CurrentReviewerId = jobPlanUpdate.CurrentReviewerId;


                return View(model);
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string fileName = $@"C:\JobPlan\Log_JobPlanEdit_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,JobPlan_error,{guid}");
                _utilities.WriteLog(fileName, ex?.StackTrace!);
                return RedirectToAction("Index", "Error");
            }
           
           
        }

        // POST: JobPlanUpdates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, JobPlanUpdateViewModel jobPlanUpdate)
        {
            if (id != jobPlanUpdate.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                        var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                    var currentUserMgr = _context.Requesters.Where(e => e.EmployeeCode == currentUser.MgrCode).FirstOrDefault();


                    var model = await _context.JobPlanUpdates.Where(v => v.Id == id).FirstOrDefaultAsync();
                    string path = Path.Combine(_hostingEnvironment.WebRootPath, $"{model.ReqNumber}_Job Plan Update");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    List<string> uploadedFiles = new List<string>();
                    if (jobPlanUpdate.Attachments!=null)
                    {
                        foreach (IFormFile postedFile in jobPlanUpdate.Attachments)
                        {
                            string fileName = Path.GetFileName(postedFile.FileName);
                            using FileStream stream = new(Path.Combine(path, fileName), FileMode.Create);
                            postedFile.CopyTo(stream);
                            uploadedFiles.Add(fileName);
                            ViewBag.Message += string.Format("<b>{0}</b> uploaded.<br />", fileName);
                        }
                    }
                    

                    model.JobPlanCode = jobPlanUpdate.JobPlanCode;
                    model.RequiredAction = jobPlanUpdate.RequiredAction;
                    model.OldJobInterval= jobPlanUpdate.OldJobInterval;
                    model.NewJobInterval = jobPlanUpdate.NewJobInterval;
                    model.MaterialsAdded= jobPlanUpdate.MaterialsAdded;
                    model.MaterialsRemoved= jobPlanUpdate.MaterialsRemoved;
                    model.ReqNumber = jobPlanUpdate.ReqNumber;
                    model.Description = jobPlanUpdate.Description;
                    model.RequesterName = jobPlanUpdate.RequesterName;
                    model.CreatedAt = jobPlanUpdate.CreatedAt;
                    model.RequesterMgrName = jobPlanUpdate.RequesterMgrName;
                    model.CurrentReviewer = jobPlanUpdate.RequesterMgrName;
                    model.Status = jobPlanUpdate.Status;
                    model.MgrStatuts = jobPlanUpdate.MgrStatuts;
                    model.ApprovalNotes = jobPlanUpdate.ApprovalNotes;
                    model.CurrentReviewerId = jobPlanUpdate.CurrentReviewerId;
                    model.RequesterId = jobPlanUpdate.RequesterId;
                    model.ChangedBy = currentUser.EmployeeCode;


                    _context.Update(model);
                    await _context.SaveChangesAsync();
                     _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUserMgr.LDap), "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", $"{currentUser.Name} update Job Plan Request  No. {model.ReqNumber}.");

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!JobPlanUpdateExists(jobPlanUpdate.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        var guid = Guid.NewGuid().ToString();
                        string fileName = $@"C:\JobPlan\Log_JobPlanEdit_{DateTime.Today:yyyyMMdd}";
                        _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,JobPlan_error,{guid}");
                        _utilities.WriteLog(fileName, ex?.StackTrace!);
                        return RedirectToAction("Index", "Error");
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(jobPlanUpdate);
        }



        // GET: JobPlanUpdates/Edit/5
        public async Task<IActionResult> MgrEdit(Guid? id)
        {
            if (id == null || _context.JobPlanUpdates == null)
            {
                return NotFound();
            }

            var jobPlanUpdate = await _context.JobPlanUpdates.FindAsync(id);
            jobPlanUpdate.ApprovalNotes = string.Empty;
            if (jobPlanUpdate == null)
            {
                return NotFound();
            }
            return View(jobPlanUpdate);
        }

        // POST: JobPlanUpdates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MgrEdit(Guid id,  JobPlanUpdate jobPlanUpdate)
        {
            var requester = _context.Requesters.Where(e => e.EmployeeCode == jobPlanUpdate.RequesterId).FirstOrDefault();
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));


            if (id != jobPlanUpdate.Id)
            {
                return NotFound();
            }

            try
            {
                if (jobPlanUpdate.MgrStatuts == MgrStatuses.Approve)
                {
                     //_utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.DeptAdminLDAP), "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", $"{currentUser.Name} Approved the request");
                     _utilities.SendMail("DPWS_eservice@dpworld.com", "DPWSokhna.TechnicalPlanning@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", "Kindly check your E-Document Inbox");

                    jobPlanUpdate.Status = TechnicalPlanningSatuses.Pending.ToString();

                    jobPlanUpdate.TechnicalPlanningSatus = TechnicalPlanningSatuses.Pending;

                    jobPlanUpdate.CurrentReviewer ="Technical Planning";

                    jobPlanUpdate.CurrentReviewerId= "Technical Planning";

                }
                
               else if (jobPlanUpdate.MgrStatuts == MgrStatuses.Decline)
               {
                    // _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.DeptAdminLDAP), "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", $"{currentUser.Name} Declined the request");

                    jobPlanUpdate.Status = "Declined";

                    jobPlanUpdate.TechnicalPlanningSatus = TechnicalPlanningSatuses.None;

                    jobPlanUpdate.CurrentReviewer = string.Empty;

                    jobPlanUpdate.CurrentReviewerId = string.Empty;
                jobPlanUpdate.ChangedBy = currentUser.EmployeeCode;

               }


                _context.Update(jobPlanUpdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!JobPlanUpdateExists(jobPlanUpdate.Id))
                {
                    return NotFound();
                }
                else
                {
                    var guid = Guid.NewGuid().ToString();
                    string fileName = $@"C:\JobPlan\Log_JobPlanMgrEdit_{DateTime.Today:yyyyMMdd}";
                    _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,JobPlan_error,{guid}");
                    _utilities.WriteLog(fileName, ex?.StackTrace!);
                    return RedirectToAction("Index", "Error");

                }
            }
            return RedirectToAction(nameof(MgrIndex));
        }



        // GET: JobPlanUpdates/Edit/5
        public async Task<IActionResult> TechPlanEdit(Guid? id)
        {
            if (id == null || _context.JobPlanUpdates == null)
            {
                return NotFound();
            }

            var jobPlanUpdate = await _context.JobPlanUpdates.FindAsync(id);
            jobPlanUpdate.ApprovalNotes = string.Empty;

            if (jobPlanUpdate == null)
            {
                return NotFound();
            }
            return View(jobPlanUpdate);
        }

        // POST: JobPlanUpdates/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TechPlanEdit(Guid id, JobPlanUpdate jobPlanUpdate)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id != jobPlanUpdate.Id)
            {
                return NotFound();
            }

            try
            {

                var requester = _context.Requesters.Where(e => e.EmployeeCode == jobPlanUpdate.RequesterId).FirstOrDefault();

                if (jobPlanUpdate.TechnicalPlanningSatus == TechnicalPlanningSatuses.Approve)
                {
                    // _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDAP), "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", $"{currentUser.Name} Approved the request");

                    jobPlanUpdate.Status = "Approved";

                    jobPlanUpdate.CurrentReviewer = string.Empty;

                    jobPlanUpdate.CurrentReviewerId =string.Empty;

                }

               else if (jobPlanUpdate.TechnicalPlanningSatus == TechnicalPlanningSatuses.Decline)
               {
                  //   _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDAP), "Youssef.AboZaid@dpworld.com", "", "Job Plan Request", $"{currentUser.Name} Declined The request");

                    jobPlanUpdate.Status = "Declined";

                    jobPlanUpdate.MgrStatuts = MgrStatuses.None;

                    jobPlanUpdate.CurrentReviewer = string.Empty;

                    jobPlanUpdate.CurrentReviewerId = string.Empty;

               }


                jobPlanUpdate.ChangedBy = currentUser.EmployeeCode;
                _context.Update(jobPlanUpdate);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!JobPlanUpdateExists(jobPlanUpdate.Id))
                {
                    return NotFound();
                }
                else
                {
                    var guid = Guid.NewGuid().ToString();
                    string fileName = $@"C:\JobPlan\Log_JobPlanMgrEdit_{DateTime.Today:yyyyMMdd}";
                    _utilities.WriteLog(fileName, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,JobPlan_error,{guid}");
                    _utilities.WriteLog(fileName, ex?.StackTrace!);
                    return RedirectToAction("Index", "Error");
                }
            }
            return RedirectToAction(nameof(TechPlanIndex));
        }

     

        // GET: JobPlanUpdates/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.JobPlanUpdates == null)
            {
                return NotFound();
            }

            var jobPlanUpdate = await _context.JobPlanUpdates
                .FirstOrDefaultAsync(m => m.Id == id);
            if (jobPlanUpdate == null)
            {
                return NotFound();
            }

            return View(jobPlanUpdate);
        }

        // POST: JobPlanUpdates/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (_context.JobPlanUpdates == null)
            {
                return Problem("Entity set 'RequestContext.JobPlanUpdates'  is null.");
            }
            var jobPlanUpdate = await _context.JobPlanUpdates.FindAsync(id);
            if (jobPlanUpdate != null)
            {
                jobPlanUpdate.ChangedBy = currentUser.EmployeeCode.ToString();
                _context.JobPlanUpdates.Update(jobPlanUpdate);
                await _context.SaveChangesAsync();
                _context.JobPlanUpdates.Remove(jobPlanUpdate);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Files(int? id)
        {
            try
            {
                string[] filePaths = Directory.GetFiles(Path.Combine(_hostingEnvironment.WebRootPath, $"{id}_Job Plan Update"));

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
        public IActionResult DownloadFile(string fileName , int id)
        {
            try
            {
                string path = Path.Combine(_hostingEnvironment.WebRootPath, $"{id}_Job Plan Update/") + fileName;

                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception)
            {

                return Content("File Doesn't Exist");


            }


        }


        private bool JobPlanUpdateExists(Guid id)
        {
          return _context.JobPlanUpdates.Any(e => e.Id == id);
        }



    }
}
