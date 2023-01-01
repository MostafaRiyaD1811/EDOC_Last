using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Document.Models;
using Document.Helpers;
using Azure.Core;

namespace Document.Controllers
{
    public class RequestersController : Controller
    {
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;

        public RequestersController(RequestContext context, IUtilities utilities)
        {
            _context = context;
            _utilities = utilities;
        }

        // GET: Requesters
        public async Task<IActionResult> Index(string ? searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.SystemAdmin==1)
                {
                    var requestContext = _context.Requesters.Include(r => r.Manager).Where(P => P.EmployeeCode.Contains(searching) || searching == null);
                    return View(await requestContext.ToListAsync());
                }
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_Index_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Index_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("NoFound", "Error");
            }

        }

        // GET: Requesters/Details/5
        public async Task<IActionResult> Details(string id)
        {

            if (id == null || _context.Requesters == null)
            {
                return NotFound();
            }

            var requester = await _context.Requesters
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(m => m.EmployeeCode == id);
            if (requester == null)
            {
                return NotFound();
            }
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.SystemAdmin == 1)
                {
                   
                    return View(requester);
                }else
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_Details_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Details_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("NoFound", "Error");
            }

        }

        // GET: Requesters/Create
        public async Task<IActionResult> Create()
        {

            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.SystemAdmin == 1)
                {

                    ViewData["MgrCode"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode");
                    return View();
                }
                else
                    return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_Create_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Create_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("NoFound", "Error");
            }

        }

        // POST: Requesters/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create( Requester requester)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.SystemAdmin == 1)
                {
                    if (ModelState.IsValid)
                    {
                        requester.ChangedBy = currentUser.EmployeeCode;
                        _context.Add(requester);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    ViewData["MgrCode"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", requester.MgrCode);
                    return View(requester);
                }
                else
                    return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_Create_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Create_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("Index", "Error");
            }


        }

        // GET: Requesters/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

            if (id == null || _context.Requesters == null)
            {
                return NotFound();
            }
            try
            {
                if (currentUser.SystemAdmin==1)
                {
                    var requester = await _context.Requesters.FindAsync(id);
                    if (requester == null)
                    {
                        return NotFound();
                    }
                    ViewData["MgrCode"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", requester.MgrCode);
                    return View(requester);
                }
                else
                {
                    return RedirectToAction("Index", "Login");

                }

            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_Edit_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Edit_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("NoFound", "Error");
            }

        }

        // POST: Requesters/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Requester requester)
        {
            if (id != requester.EmployeeCode)
            {
                return NotFound();
            }
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.SystemAdmin == 1)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            requester.ChangedBy = currentUser.EmployeeCode;
                            _context.Update(requester);
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException ex)
                        {
                            var guid = Guid.NewGuid().ToString();
                            string file = $@"C:\EDoc\Log_Requester_Edit_{DateTime.Today:yyyyMMdd}";
                            _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Edit_error,{guid}");
                            _utilities.WriteLog(file, ex.StackTrace!);

                            if (!RequesterExists(requester.EmployeeCode))
                            {
                                return RedirectToAction("NoFound", "Error");
                            }
                            else
                            {
                                throw;
                            }
                        }
                        return RedirectToAction(nameof(Index));
                    }
                    ViewData["MgrCode"] = new SelectList(_context.Requesters, "EmployeeCode", "EmployeeCode", requester.MgrCode);
                    return View(requester);
                }
                else
                    return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_Edit_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Edit_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("NoFound", "Error");
            }

        }

        // GET: Requesters/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.Requesters == null)
            {
                return NotFound();
            }
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (currentUser.SystemAdmin == 1)
                {
                    var requester = await _context.Requesters
                .Include(r => r.Manager)
                .FirstOrDefaultAsync(m => m.EmployeeCode == id);
                    if (requester == null)
                    {
                        return NotFound();
                    }
                    return View(requester);
                } 
                else
                    return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_Delete_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_Delete_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                return RedirectToAction("NoFound", "Error");
                
            }         
        }

        // POST: Requesters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));

                if (_context.Requesters == null)
                {
                    return Problem("Entity set 'RequestContext.Requesters'  is null.");
                }
                var requester = await _context.Requesters.FindAsync(id);
                if (requester != null)
                {
                    requester.ChangedBy = currentUser.EmployeeCode.ToString();
                    _context.Requesters.Remove(requester);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_Requester_DeleteConfirmed_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,Requester_DeleteConfirmed_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);

                throw;
            }
          
        }

        private bool RequesterExists(string id)
        {
            return _context.Requesters.Any(e => e.EmployeeCode == id);
        }
    }
}
