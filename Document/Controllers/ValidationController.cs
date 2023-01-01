using Document.Models;
using Microsoft.AspNetCore.Mvc;

namespace Document.Controllers
{
    public class ValidationController : Controller
    {
       
        
            [HttpPost]
            public JsonResult IsValidDate(CarRequest car)
            {
                var min = DateTime.Now.AddDays(0);
                var max = DateTime.Now.AddYears(1);
            var msg = string.Format($"Please enter a valid date");

                  DateTime date = Convert.ToDateTime(car.Departure);
                    if (date < min || date > max)
                        return Json(msg);
                    else
                        return Json(true);
                
               
            }
        [HttpPost]
        public JsonResult IsValidDateRet(CarRequest car)
        {
            var min = DateTime.Now.AddDays(0);
            var max = DateTime.Now.AddYears(1);
            var msg = string.Format($"Please enter a valid date");

            DateTime date = Convert.ToDateTime(car.ReturnBack);
            if (date < min || date > max)
                return Json(msg);
            else
                return Json(true);


        }
        [HttpPost]
        public JsonResult IsValidVDate(Voucher req)
        {
            var min = DateTime.Now.AddDays(0);
            var max = DateTime.Now.AddYears(1);
            var msg = string.Format($"Please enter a valid date");

            DateTime date = Convert.ToDateTime(req.InvoiceDate);
            if (date < min || date > max)
                return Json(msg);
            else
                return Json(true);


        }

        [HttpPost]
        public JsonResult IsValidVDateCheckIn(TravelDesk req)
        {
            var min = DateTime.Now.AddDays(0);
            var max = DateTime.Now.AddYears(1);
            var msg = string.Format($"Please enter a valid date");

            DateTime date = Convert.ToDateTime(req.CheckIn);

            if (date < min || date > max)
                return Json(msg);
            else
                return Json(true);


        }
        [HttpPost]
        public JsonResult IsValidVDateCheckOut(TravelDesk req)
        {
            var min = DateTime.Now.AddDays(0);
            var max = DateTime.Now.AddYears(1);
            var msg = string.Format($"Please enter a valid date");

            DateTime date = Convert.ToDateTime(req.CheckOut);

            if (date < min || date > max)
                return Json(msg);
            else
                return Json(true);
        }
        [HttpPost]
        public JsonResult IsValidDateDeparture(TravelDesk req)
        {
            var min = DateTime.Now.AddDays(0);
            var max = DateTime.Now.AddYears(1);
            var msg = string.Format($"Please enter a valid date");

            DateTime date = Convert.ToDateTime(req.Departure);
            if (date < min || date > max)
                return Json(msg);
            else
                return Json(true);


        }
        [HttpPost]
        public JsonResult IsValidDateReturn(TravelDesk req)
        {
            var min = DateTime.Now.AddDays(0);
            var max = DateTime.Now.AddYears(1);
            var msg = string.Format($"Please enter a valid date");

            DateTime date = Convert.ToDateTime(req.ReturnBack);
            if (date < min || date > max)
                return Json(msg);
            else
                return Json(true);


        }

    }
}
