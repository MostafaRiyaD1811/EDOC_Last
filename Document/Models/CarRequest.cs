using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Document.Models
{
    public class CarRequest
    {
        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        public Guid Id { get; set; } = new Guid();

        
        [Required]
        [Display(Name = "Requested For")]
        [DataType(DataType.MultilineText)]
        public string? Name { get; set; }
        [Display(Name = "Mobile No.")]
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(\+\s ?)?((?<!\+.*)\(\+?\d+([\s\-\.]?\d+)?\)|\d+)([\s\-\.]?(\(\d+([\s\-\.]?\d+)?\)|\d+))*(\s? (x|ext\.?)\s?\d+)?$", ErrorMessage = "The PhoneNumber field is not a valid phone number")]
        public string? MobileNo { get; set; }
        [Display(Name = "Departure")]
        [Required]
        public string? DepartureAddress { get; set; }
        [Display(Name = "Destination")]
        [Required]
        public string? DestinationAddress { get; set; }
        
        [Remote("IsValidDate", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        [Display(Name = "Departure date")]
        [Required]
        [DataType(DataType.DateTime)]
        [Column(TypeName = "datetime")]

        public DateTime Departure { get; set; }
        [Display(Name = "Return back")]
        [Required]
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]
        [Remote("IsValidDateRet", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        public DateTime ReturnBack { get; set; }
        [Display(Name = "Luggage description (Number/Size)")]
        [Required]
        public string? LuggageDescription { get; set; }

        [Display(Name = "Car type")]
        [Required]
        public string? CarType { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mission")]
        public string? Justification { get; set; }

        [Display(Name = "Code")]
        public string? CreatorId { get; set; }
     
        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
        [Display(Name="Created")]
        public DateTime CreationDate { get; set; } = DateTime.Now;
        [Display(Name = "Requester")]
        [DataType(DataType.Text)]
        public string? RequesterName { get; set; }
        public string? Status { get; set; }

        public string? Notes { get; set; }
        [Display(Name = "Attachment")]
        public string? AttachmentPath { get; set; }

        [Display(Name = "Approval")]
        public DeptAdminStatutses DeptAdminStatuts { get; set; }
      
        public MgrStatutses MgrStatuts { get; set; }
        public TransTeamLeaderStatuses TransTeamLeaderStatus { get; set; }

        [ForeignKey("CurrentReviewer")]
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReviewer { get; set; }
        public virtual Requester? Requester { get; set; }
        [Display(Name = "Assignee")]
        public string? CurrentReview { get; set; }

        public string? ChangedBy { get; set; }


    }

    public enum DeptAdminStatutses
    {
        None,
        Pending,
        Approve,
        Decline
    }
    public enum MgrStatutses
    {
        None,
        Pending,
        Approve,
        Decline
    }
    public enum TransTeamLeaderStatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }
}
