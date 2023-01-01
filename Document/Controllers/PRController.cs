using Document.Helpers;
using Document.Models;
using Document.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;

namespace Document.Controllers
{
    public class PRController : Controller
    {
        private readonly RequestContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;
        private readonly IUtilities _utilities;

        public PRController(RequestContext context, Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment , IUtilities utilities)
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

                var user = _context.Requesters.FirstOrDefault(m => m.LDap == currentUser.LDap);

                if (user != null)
                {

                    if (currentUser.Dept == "Procurement")
                    {
                        return RedirectToAction("Index");
                    }

                    else if (currentUser.FinanceReviewr == 1)
                    {
                        return RedirectToAction("FinanceReviewerIndex");
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

        // GET: PR
        public async Task<IActionResult> Index(string? searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (currentUser.Dept == "Procurement")
                {
                    return View(await _context.PRs.Where(P => P.PONo.Contains(searching)  || P.VendorName.Contains(searching) || searching == null).ToListAsync());
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {

                return RedirectToAction("Index", "Home");
            }
           
           
        }

        // GET: PR/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.PRs == null)
            {
                return NotFound();
            }

            var pR = await _context.PRs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pR == null)
            {
                return NotFound();
            }

            return View(pR);
        }

        // GET: PR/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PR/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PRViewModel pR)
        {
            if (ModelState.IsValid)
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                string pOuniqueFileName = null; 

                string InvoiceuniqueFileName = null;

                string otheruniqueFileName = null;

                var model = new PR();


                var uploadsFolder = Directory.CreateDirectory($@"{_hostingEnvironment.WebRootPath}\{pR.ReqNum}_PR Details");
                if (pR.POAttach != null)
                {
                    string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{pR.ReqNum}_PR Details");
                    pOuniqueFileName = pR.POAttach.FileName;
                    string filePath = Path.Combine(uploadsFolder1, pOuniqueFileName);
                    FileStream file = new FileStream(filePath, FileMode.Create);

                    await pR.POAttach.CopyToAsync(file);

                    model.POAttach = pOuniqueFileName;
                    file.Close();
                    file.Dispose();
                }
                if (pR.InvoiceAttach != null)
                {

                    string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{pR.ReqNum}_PR Details");
                    InvoiceuniqueFileName = pR.InvoiceAttach.FileName;
                    string filePath = Path.Combine(uploadsFolder1, InvoiceuniqueFileName);
                    FileStream file = new FileStream(filePath, FileMode.Create);

                    await pR.InvoiceAttach.CopyToAsync(file);
                    model.InvoiceAttach = InvoiceuniqueFileName;
                    file.Close();
                    file.Dispose();
                }
                if (pR.OtherAttach != null)
                {
                    string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{pR.ReqNum}_PR Details");
                    otheruniqueFileName = pR.OtherAttach.FileName;
                    string filePath = Path.Combine(uploadsFolder1, otheruniqueFileName);
                    FileStream file = new FileStream(filePath, FileMode.Create);
                    await pR.OtherAttach.CopyToAsync(file);
                    model.OtherAttach = otheruniqueFileName;
                    file.Close();
                    file.Dispose();


                }
                model.CreationNotes=pR.CreationNotes;
                model.VendorName = pR.VendorName;
                model.VendorNum = pR.VendorNum;
                model.ReqNumber = pR.ReqNum;
                model.CreatedAt = DateTime.Now;
                model.PONo = pR.PONo;
                model.InvoiceNo = pR.InvoiceNo;
                model.POAttach = pOuniqueFileName;
                model.InvoiceAttach = InvoiceuniqueFileName;
                model.OtherAttach = otheruniqueFileName;
                model.Status = "Pending";
                model.FinanceReviewerStatus = FinanceReviewerStatuses.Pending;
                model.RequesterId = currentUser.EmployeeCode;
                model.CurrentReviewerId = "Finance Reviewer";
                model.ChangedBy= currentUser.EmployeeCode;

                  _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(currentUser.LDap), "Youssef.AboZaid@dpworld.com", "", "Purchase Order", $"{currentUser.Name} Create a PO.");
                _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "Purchase Order", $"{currentUser.Name} Create a PO kindly check your inbox.");
                _context.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pR);
        }

        // GET: PR/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.PRs == null)
            {
                return NotFound();
            }

            var pR = await _context.PRs.FindAsync(id);
            var model = new PRViewModel();
            

            if (pR == null)
            {
                return NotFound();
            }
            model.VendorNum = pR.VendorNum;

            model.CreationNotes = pR.CreationNotes;
            model.VendorName=pR.VendorName;
            model.ReqNum = pR.ReqNumber;
            model.Id = pR.Id;
            model.PONo = pR.PONo;
            model.InvoiceNo = pR.InvoiceNo;
            model.Attach1 = pR.POAttach;
            model.Attach2 = pR.InvoiceAttach;
            model.Attach3 = pR.OtherAttach;
            model.RequesterId = pR.RequesterId;
            model.CreatedAt = pR.CreatedAt;
            model.CurrentReviewerId = pR.CurrentReviewerId;
            model.Status = pR.Status;
            model.ApprovalNotes= pR.ApprovalNotes;
            model.FinanceReviewerStatus = FinanceReviewerStatuses.Pending;
            return View(model);
        }

        // POST: PR/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PRViewModel pR)
        {
            if (id != pR.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                        var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                    string pOuniqueFileName = null;
                    string InvoiceuniqueFileName = null;
                    string otheruniqueFileName = null;


                    var model = await _context.PRs.Where(v => v.Id == id).FirstOrDefaultAsync();
                      var uploadsFolder =  Directory.CreateDirectory($@"{_hostingEnvironment.WebRootPath}\{model.ReqNumber}_PR Details");
                    if (pR.POAttach != null)
                    {
                        string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{model.ReqNumber}_PR Details");
                        pOuniqueFileName = pR.POAttach.FileName;
                        string filePath = Path.Combine(uploadsFolder1, pOuniqueFileName);
                        FileStream file = new FileStream(filePath, FileMode.Create);

                        await pR.POAttach.CopyToAsync(file);

                        model.POAttach = pOuniqueFileName;
                        file.Close();
                        file.Dispose();
                    }
                    if (pR.InvoiceAttach != null)
                    {
                        
                        string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{model.ReqNumber}_PR Details");
                        InvoiceuniqueFileName = pR.InvoiceAttach.FileName;
                        string filePath = Path.Combine(uploadsFolder1, InvoiceuniqueFileName);
                        FileStream file = new FileStream(filePath, FileMode.Create );

                        await pR.InvoiceAttach.CopyToAsync(file);
                        model.InvoiceAttach = InvoiceuniqueFileName;
                        file.Close();
                        file.Dispose();
                    }
                    if (pR.OtherAttach != null)
                    {
                        string uploadsFolder1 = Path.Combine(_hostingEnvironment.WebRootPath, $"{model.ReqNumber}_PR Details");
                        otheruniqueFileName = pR.OtherAttach.FileName;
                        string filePath = Path.Combine(uploadsFolder1, otheruniqueFileName);
                        FileStream file = new FileStream(filePath, FileMode.Create);
                        await pR.OtherAttach.CopyToAsync(file);
                        model.OtherAttach = otheruniqueFileName;
                        file.Close();
                        file.Dispose();


                    }

                    model.CreationNotes = pR.CreationNotes;                  
                model.VendorNum = pR.VendorNum;

                    model.Id = pR.Id;
                    model.PONo = pR.PONo;
                    model.InvoiceNo = pR.InvoiceNo;
                    model.RequesterId = pR.RequesterId;
                    model.VendorName= pR.VendorName;
                    model.CurrentReviewerId = "Finance Reviewer";
                    model.Status = pR.Status;
                    model.ApprovalNotes = pR.ApprovalNotes;



                    model.ChangedBy = currentUser.EmployeeCode;

                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    _utilities.SendMail("DPWS_eservice@dpworld.com", "dpwsokhna.finance@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "Purchase Order", $"{currentUser.Name} update PO  No. {model.PONo}.");

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PRExists(pR.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pR);
        }

        // GET: PR/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.PRs == null)
            {
                return NotFound();
            }

            var pR = await _context.PRs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pR == null)
            {
                return NotFound();
            }

            return View(pR);
        }




        [HttpGet]
        public async Task<IActionResult> FinanceReviewerIndex(string searching)
        {
            try
            {

                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (currentUser.FinanceReviewr ==1)
                {
                    var PRs = await _context.PRs.Where(w => w.CurrentReviewerId == "Finance Reviewer").Where(P => P.PONo.Contains(searching) || searching == null ||P.VendorName.Contains(searching)||P.InvoiceNo.Contains(searching)).ToListAsync();
                    return View(PRs);
                }
                else { 
                    return RedirectToAction("Index", "Login");
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
            if (id == null || _context.PRs == null)
            {
                return NotFound();
            }

            var pR = await _context.PRs.FindAsync(id);
            pR.ApprovalNotes = string.Empty;
            if (pR == null)
            {
                return NotFound();
            }
           
            return View(pR);
        }

        [HttpPost]
        public async Task<IActionResult> FinanceReviewerEdit(Guid id, PR pR)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (id != pR.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                
                try
                {
                    if (pR.FinanceReviewerStatus == FinanceReviewerStatuses.Approve)
                    {
                        _utilities.SendMail("DPWS_eservice@dpworld.com", "DPW.Procurement@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "Purchase Order", $"{currentUser.Name} Approved PO  No. {pR.PONo}.");

                        pR.Status =$"{currentUser.Name} Approved At {DateTime.Now}";
                        pR.ApprovedAt= DateTime.Now;
                        pR.CurrentReviewerId = "Completed";
                    }

                    else if (pR.FinanceReviewerStatus == FinanceReviewerStatuses.Decline)
                    {
                    pR.ChangedBy = currentUser.EmployeeCode;
                        _utilities.SendMail("DPWS_eservice@dpworld.com", "DPW.Procurement@dpworld.com", "Youssef.AboZaid@dpworld.com", "", "Purchase Order", $"{currentUser.Name} Declined PO  No. {pR.PONo}.");

                        pR.Status = $"{currentUser.Name} Declined At {DateTime.Now}";       
                    }
                    
                    _context.Update(pR);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PRExists(pR.Id))
                    {
                        return NotFound();
                    }

                }
                return RedirectToAction("FinanceReviewerIndex");
            }
            return RedirectToAction("FinanceReviewerIndex");
        }

        // POST: PR/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            if (_context.PRs == null)
            {
                return Problem("Entity set 'RequestContext.PRs'  is null.");
            }
            var pR = await _context.PRs.FindAsync(id);
            if (pR != null)
            {
                pR.ChangedBy = currentUser.EmployeeCode.ToString();
                _context.PRs.Update(pR);
                await _context.SaveChangesAsync();
                _context.PRs.Remove(pR);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task< IActionResult> Download(string fileName , int id)
        {
            try
            {
                string uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, $"{id}_PR Details");
                string FPath = Path.Combine(uploadsFolder, fileName);

                byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(FPath);
                return  File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
            }
            catch (Exception)
            {

                return  Content("File Doesn't Exist");
            }
        }

        public IActionResult DemoViewAsPDF(Guid id)
        {
            PR pR = _context.PRs.FirstOrDefault(c => c.Id == id);

            var report = new ViewAsPdf("Details",pR)
            {
                PageMargins = { Left = 20, Bottom = 20, Right = 20, Top = 20 },
            };
            return report;
        }


        private bool PRExists(Guid id)
        {
          return _context.PRs.Any(e => e.Id == id);
        }
    }
}
