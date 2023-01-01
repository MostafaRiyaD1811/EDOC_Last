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
    public class CarRequest_Audit
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        
        public Guid RequestId { get; set; } = new Guid();

        
        public string? Name { get; set; }
        public string? MobileNo { get; set; }
        public string? DepartureAddress { get; set; }
        public string? DestinationAddress { get; set; }
        
        public DateTime Departure { get; set; }
        public DateTime ReturnBack { get; set; }
        public string? LuggageDescription { get; set; }

        public string? CarType { get; set; }
        public string? Justification { get; set; }

        public string? CreatorId { get; set; }
     
        public string? RequesterId { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public string? RequesterName { get; set; }
        public string? Status { get; set; }

        public string? Notes { get; set; }
        public string? AttachmentPath { get; set; }

        public DeptAdminStatutses DeptAdminStatuts { get; set; }
      
        public MgrStatutses MgrStatuts { get; set; }
        public TransTeamLeaderStatuses TransTeamLeaderStatus { get; set; }

        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReviewer { get; set; }
        public virtual Requester? Requester { get; set; }
        public string? CurrentReview { get; set; }



    }
}
