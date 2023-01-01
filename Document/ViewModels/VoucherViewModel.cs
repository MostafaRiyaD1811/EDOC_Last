using Document.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Document.ViewModels
{
    public class VoucherViewModel
    {
        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);
        public Guid? Id { get; set; }
        [Required]
        [Display(Name = "Vendor Name")]
        public string? VendorName { get; set; }
        [DisplayName("Vendor Number")]
        [Required]
        public string? VendorNum { get; set; }
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(1, 100000000)]
        public decimal Amount { get; set; }

        [Display(Name = "Invoice Date")]
        [Required]
        [Column(TypeName = "datetime")]
        [DataType(DataType.DateTime)]

        //[Remote("IsValidVDate", "Validation", HttpMethod = "POST", ErrorMessage = "Please provide a valid date.")]
        public DateTime InvoiceDate { get; set; }     

        [Display(Name = "Beneficiary Name")]
        [Required]
        public string? BeneficiaryName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? Status { get; set; }     

        [Required]
        public string? Type { get; set; }
        [Display(Name = "Other Notes")]
        public string? OtherNotes { get; set; }

        public string? Description { get; set; }
        [Required]
        public string? VoucherCurrency { get; set; }
        public virtual Requester? Requester { get; set; }

        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
        public string? Details { get; set; }
      
        // [RegularExpression("([a-zA-Z0-9\\s_\\\\.\\-:])+(.doc|.docx|.pdf)$", ErrorMessage = "The file type is not valid ")]

        [AllowedFilesExtensions(new string[] { ".doc", ".pdf", ".docx" })]
        [Display(Name = "Attachments (Allowed Extensions(.doc, .pdf, .docx))")]

        public List <IFormFile>? Attachments { get; set; }

        [Display(Name = "Current Reviewer")]
        public string? CurrentReviewerId { get; set; }
    }
}
