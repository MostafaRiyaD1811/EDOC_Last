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
    public class MgrController : Controller
    {
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;

        public MgrController(RequestContext context, IUtilities utilities)
        {
            _context = context;
            _utilities = utilities;
        }

        // GET: Mgr
        public async Task<IActionResult> Index(int? searching)
        {
            try
            {
                    var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
                if (searching.HasValue)
                    return View(await _context.CarRequests.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).Where(P => P.ReqNumber.ToString().Contains(searching.ToString())).OrderByDescending(e => e.CreationDate).ToListAsync());
                else
                    return View(await _context.CarRequests.Where(w => w.CurrentReviewerId == currentUser.EmployeeCode).OrderByDescending(e => e.CreationDate).ToListAsync());
            }
            catch (Exception ex)
            {
                var guid = Guid.NewGuid().ToString();
                string file = $@"C:\EDoc\Log_CarRequest_MGR_Index_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,MGR_Index_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);
                return RedirectToAction("Index", "Login");
            }



        }
        public async Task<IActionResult> Edit(Guid? id)
        {
            

            var carRequest = await _context.CarRequests.FindAsync(id);
           
            return View(carRequest);
        }

        // POST: CarRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, CarRequest carRequest)
        {

                var currentUser = _context.Requesters.First(r => r.LDap == HttpContext.Session.GetString("Session"));
            var requester = await _context.Requesters.Where(e => e.EmployeeCode == carRequest.CreatorId).FirstOrDefaultAsync();
            var Adminstration = await _context.Requesters.Where(w => w.Adminstration == 1).FirstOrDefaultAsync();

            if (id != carRequest.Id)
            {
                return NotFound();
            }

            try
            {
                if (carRequest.MgrStatuts == MgrStatutses.Approve)
                {
                    carRequest.Status = DeptAdminStatuses.Pending.ToString();
                    carRequest.DeptAdminStatuts = DeptAdminStatutses.Pending;
                    carRequest.CurrentReviewerId = Adminstration.EmployeeCode;
                    carRequest.CurrentReview = "Adminstration Department";
                     _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name}  Approved your car request.");
                   _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(Adminstration.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name}  Approved the car request of {requester.Name} Kindly review this request No.{carRequest.ReqNumber}.");
                }

                if (carRequest.MgrStatuts == MgrStatutses.Decline)               
                {
                    carRequest.Status = "Declined";
                    carRequest.DeptAdminStatuts = DeptAdminStatutses.None;
                    carRequest.CurrentReview = string.Empty;
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name}  Declined your car request.");

                }

                carRequest.ChangedBy = currentUser.EmployeeCode;

                _context.Update(carRequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CarRequestExists(carRequest.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
         //   return View(carRequest);
        }
        private bool CarRequestExists(Guid id)
        {
            return _context.CarRequests.Any(e => e.Id == id);
        }
    }
}
