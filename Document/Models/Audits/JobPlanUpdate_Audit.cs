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
    public class JobPlanUpdate_Audit
    {
        [Key]
        public Guid Id { get; set; } = new Guid();

        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public Guid RequestId { get; set; }= Guid.NewGuid();
        public string? JobPlanCode { get; set; }
        public string? OldJobInterval { get; set; }

        public string? NewJobInterval { get; set; }
        public string? MaterialsRemoved { get; set; }
        public string? MaterialsAdded { get; set; }
        public string? Description { get; set; }
        public string? RequiredAction { get; set; }
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);

        public string? RequesterName { get; set; }
        public string? RequesterMgrName { get; set; }

        public string? AssetCode { get; set; }

        public string? AssetType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CurrentReviewer { get; set; }
        public string? Status { get; set; }

        public String? ApprovalNotes { get; set; }
        public TechnicalPlanningSatuses TechnicalPlanningSatus { get; set; }

        public MgrStatuses MgrStatuts { get; set; }
        public string? RequesterId { get; set; }
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReview { get; set; }
    }
}
