using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Document.Models;
using Document.Helpers;
using RestSharp;

namespace Document.Controllers
{
    public class DeptAdminController : Controller
    {
        private readonly RequestContext _context;
        private readonly IUtilities _utilities;

        public DeptAdminController(RequestContext context, IUtilities utilities)
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
                string file = $@"C:\EDoc\Log_CarRequest_DeptAdmin_Index_{DateTime.Today:yyyyMMdd}";
                _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,DeptAdmin_Index_error,{guid}");
                _utilities.WriteLog(file, ex.StackTrace!);
     
                return RedirectToAction("Index", "Error");
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

            var requester = _context.Requesters.Where(e => e.EmployeeCode == carRequest.CreatorId).FirstOrDefault();
            if (id != carRequest.Id)
            {
                return NotFound();
            }

            try
            {

                if (carRequest.DeptAdminStatuts == DeptAdminStatutses.Approve)
                {
                    
                    carRequest.Status = "Approved";
                    carRequest.CurrentReview = string.Empty;
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name}  approved your car request.");
                   // _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(Trans), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"Car requset No. {carRequest.ReqNumber} has been approved Kindly check your E-document Inbox.");
                    HttpClientHandler clientHandler = new()
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                    };
                    var client = new RestClient(clientHandler);
                    var request = new RestRequest($"https://appsportali.dpworldsokhna.com:500/sms_ws/api/sms/SendSMS", Method.Post);
                    var body = $"{{\r\n    \"MsgText\": \"{currentUser.Name} Approved your car request \",\r\n " +
                  $"   \"MobileNo\": \"+2{carRequest.MobileNo}\",\r\n " +
                  $"   \"pol\":\"E-Document\",\r\n   ";
                    request.AddParameter("application/json", body, ParameterType.RequestBody);
                    RestResponse response = await client.ExecuteAsync(request);
                }
                else
                {
                    carRequest.Status = "Declined";
                    carRequest.CurrentReview = string.Empty;
                    carRequest.MgrStatuts = MgrStatutses.None;
                    _utilities.SendMail("DPWS_eservice@dpworld.com", _utilities.GetMail(requester.LDap), "Youssef.AboZaid@dpworld.com", "", "Car Request", $"{currentUser.Name}  declined your car request.");

                }
                carRequest.ChangedBy = currentUser.EmployeeCode;
                _context.Update(carRequest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!CarRequestExists(carRequest.Id))
                {
                    return NotFound();
                }
                else
                {
                    var guid = Guid.NewGuid().ToString();
                    string file = $@"C:\CarRequsets\Log_CarRequsetsEdit_{DateTime.Today:yyyyMMdd}";
                    _utilities.WriteLog(file, $">> {DateTime.Now:yyyy-MM-dd HH:mm:ss},INFO,CarRequsets_error,{guid}");
                    _utilities.WriteLog(file, ex?.StackTrace!);

                    return RedirectToAction("Index", "Error");
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
