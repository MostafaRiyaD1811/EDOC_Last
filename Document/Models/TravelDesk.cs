using Document.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace Document.Models
{
    public class TravelDesk
    {
        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        public Guid Id { get; set; } = new Guid();
        
        public string?  Request { get; set; }
       

        [Display(Name = "Requester Name")]
        [RegularExpression(@"[a-zA-Z\s]{1,50}", ErrorMessage = "Enter a Valid Name without numeric characters")]
        public string? RequesterName { get; set; }
        [Required]
        [Display(Name = "Requester Department")]
        public string? RequesterDept { get; set; }
        [Required]
        [RegularExpression(@"[a-zA-Z\s]{1,50}", ErrorMessage = "Enter a Valid Title without numeric characters")]

        public string? Title { get; set; }

        [Required]
        [RegularExpression(@"[a-zA-Z\s]{1,50}", ErrorMessage = "Enter a Valid Nationality without numeric characters")]

        public string? Nationality { get; set; }
        [Required]
        [Display(Name = "Request Purpose")]

        public string? RequestPurpose { get; set; }
        [DataType(DataType.DateTime)]
        [Column(TypeName = "datetime")]
        [Display(Name = "Check in")]
        [Remote("IsValidVDateCheckIn", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        public DateTime? CheckIn { get; set; }

        [DataType(DataType.DateTime)]
        [Column(TypeName = "datetime")]
        [Display(Name = "Check out")]
        [Remote("IsValidVDateCheckOut", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        public DateTime? CheckOut { get; set; }
        [Display(Name = "Mission Address")]
        public string? MissionAddress { get; set; }
        [Display(Name = "Method of Payment")]
        public string? MethodOfPayment { get; set; }
        [Display(Name = "Trip Direction")]
        public string? TripDirection { get; set; }
        [Display(Name = "Destination Country")]
        public string? DestinationCountry { get; set; }
        [Display(Name = "Expected Travel Time")]
        public DateTime? ExpectedTravelTime { get; set; }
        [Display(Name = "Cost Allocation")]
        public string? CostAllocation { get; set; }
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }
        [Display(Name = "Requested For")]
        [RegularExpression(@"[a-zA-Z\s]{1,50}", ErrorMessage = "Enter a Valid Name without numeric characters")]
        public string? RequestedFor { get; set; }


        [Remote("IsValidDateDeparture", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        [Display(Name = "Departure date")]
        [DataType(DataType.DateTime)]
        [Column(TypeName = "datetime")]

        public DateTime? Departure { get; set; }
        
        [Display(Name = "Return back ")]
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        [Remote("IsValidDateReturn", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        public DateTime? ReturnBack { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Assignee")]
        public string? CurrentReviewer { get; set; }
        public string? Status { get; set; }
        [DisplayName(" Attachment")]
        public string?Attach { get; set; }
        [Display(Name = "Requester Id")]
        public string? RequesterCode { get; set; }

        [Display(Name = " Notes")]
        public String? ApprovalNotes { get; set; }

        public DeptAdminStatuses DeptAdminStatus { get; set; }

        public MgrStatuses MgrStatus { get; set; }

        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
        [ForeignKey("CurrentReview")]
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReview { get; set; }
        public string? ChangedBy { get; set; }
     
    }
}
