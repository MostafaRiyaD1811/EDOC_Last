using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Document.Models
{
    public class Voucher_Audit
    {
        [Key]

        public Guid Id { get; set; } = new Guid();

        public DateTime ChangedAt { get; set; }
        public string? ChangedBy { get; set; }
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        public Guid RequestId { get; set; } = new Guid();
        public string? VendorName { get; set; }
        public string? VendorNum { get; set; }
        public decimal Amount  { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string? Details { get; set; }
        public string? BeneficiaryName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Status { get; set; }
        public string? ApprovalNotes { get; set; }
        public string? Approved { get; set; }
        public string? OtherNotes { get; set; }
        public string? Type { get; set; }

        public string? Description { get; set; }
        public string? VoucherCurrency { get; set; }
        public HRReviewerStatuses HRReviewerStatuts { get; set; }
        public HRMgrStatutses HRMgrStatuts { get; set; }
        public FinanceInitiatorStatuses FinanceInitiatorStatus { get; set; }
        public FinanceReviewerStatuses FinanceReviewerStatus { get; set; }
        public FinanceMgrStatuses FinanceMgrStatus { get; set; }
        public virtual Requester? Requester { get; set; }
        public virtual string? RequesterId { get; set; }

        public string? CurrentReviewerId { get; set; }


    }
}
