using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Numerics;
using Document.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Document.Models
{
    public class JobPlanUpdate
    {
        public Guid Id { get; set; }= Guid.NewGuid();
        [DisplayName("Job Plan Code")]
        [Required]
        public string? JobPlanCode { get; set; }

        [DisplayName("Old Job Plan Interval")]
        [Required]
        public string? OldJobInterval { get; set; }

        [DisplayName("New Job Plan Interval")]
        [Required]
        public string? NewJobInterval { get; set; }
        [DisplayName("Materials need to be removed")]
        [Required]
        public string? MaterialsRemoved { get; set; }
        [DisplayName("Materials need to be Added")]
        [Required]
        public string? MaterialsAdded { get; set; }
        
        [Required]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
        [Required]
        [DisplayName("Required Action")]
        public string? RequiredAction { get; set; }

        [Required]
        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);

        [Display(Name = "Requester Name")]
        public string? RequesterName { get; set; }
        [Display(Name = "Requester Manager")]
        public string? RequesterMgrName { get; set; }
        [Display(Name = "Asset Code")]

        public string? AssetCode { get; set; }
        [Display(Name = "Asset Type")]

        public string? AssetType { get; set; }



        [Display(Name = "Created At")]

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Assignee")]
        public string? CurrentReviewer { get; set; }
        public string? Status { get; set; }

        [Display(Name = "Approval Notes")]
        public String? ApprovalNotes { get; set; }
        public TechnicalPlanningSatuses TechnicalPlanningSatus { get; set; }

        public MgrStatuses MgrStatuts { get; set; }
        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
        [ForeignKey("CurrentReview")]
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReview { get; set; }
        public string? ChangedBy { get; set; }

    }
}
