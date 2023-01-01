using Document.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.ComponentModel;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace Document.Models
{
    public class TravelDesk_Audit
    {
        [Key]

        public Guid Id { get; set; } = new Guid();

        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        public Guid RequestId { get; set; } = new Guid();
        
        public string?  Request { get; set; }
        public string? RequesterName { get; set; }
        public string? RequesterDept { get; set; }
        public string? Title { get; set; }
        public string? Nationality { get; set; }
        public string? RequestPurpose { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string? MissionAddress { get; set; }
        public string? MethodOfPayment { get; set; }
        public string? TripDirection { get; set; }
        public string? DestinationCountry { get; set; }
        public DateTime? ExpectedTravelTime { get; set; }
        public string? CostAllocation { get; set; }
        public string? Remarks { get; set; }
        public string? RequestedFor { get; set; }
        public DateTime? Departure { get; set; }
        public DateTime? ReturnBack { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CurrentReviewer { get; set; }
        public string? Status { get; set; }
        public string?Attach { get; set; }
        public string? RequesterCode { get; set; }
        public String? ApprovalNotes { get; set; }
        public DeptAdminStatuses DeptAdminStatus { get; set; }
        public MgrStatuses MgrStatus { get; set; }
        public string? RequesterId { get; set; }
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReview { get; set; }
     
    }
}
