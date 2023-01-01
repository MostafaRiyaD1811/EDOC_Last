using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Security.Cryptography;
using Document.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Document.ViewModels
{
    public class JobPlanUpdateViewModel
    {
        public Guid Id { get; set; }
        [DisplayName("Job Plan Code")]
        public string? JobPlanCode { get; set; }
        [Required]
        [DisplayName("Required Action")]
        public string? RequiredAction { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }
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
        [DisplayName("Request Number")]
        public int ReqNumber { get; set; } = RandomNumberGenerator.GetInt32(0, int.MaxValue);

        [Display(Name = "Requester Name")]
        public string? RequesterName { get; set; }
        [Display(Name = "Current Reviewer")]
        public string? CurrentReviewer { get; set; }
        public string? Attachment { get; set; }
        [Display(Name = "Asset Code")]

        public string? AssetCode { get; set; }
        [Display(Name = "Asset Type")]

        public string? AssetType { get; set; }
        [Display(Name = "Attachments (Allowed Extensions(.doc, .pdf, .docx))")]
        //[RegularExpression("([a-zA-Z0-9\\s_\\\\.\\-:])+(.doc|.docx|.pdf)$", ErrorMessage = "The file type is not valid ")]
        [AllowedFilesExtensions(new string[] { ".doc", ".pdf", ".docx" })]

        public List<IFormFile>? Attachments { get; set; }
        public string? Status { get; set; }
        [Display(Name = "Created At")]

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Approval Notes")]
        public String? ApprovalNotes { get; set; }
        [Display(Name = "Technical Planning Satus")]
        public TechnicalPlanningSatuses TechnicalPlanningSatus { get; set; }

        [Display(Name = "Requester manager Satus")]
        public MgrStatuses MgrStatuts { get; set; }

        [Display(Name = "Requester Manager")]
        public string? RequesterMgrName { get; set; }
        [ForeignKey("CurrentReview")]
        public string? CurrentReviewerId { get; set; }
        public virtual Requester? CurrentReview { get; set; }

        [Display(Name = "Created by")]
        [ForeignKey("Requester")]
        public string? RequesterId { get; set; }
    }

    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;
        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }
        protected override ValidationResult IsValid(
        object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"The file type is not valid";
        }
    }
    public enum TechnicalPlanningSatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }
    public enum MgrStatuses
    {
        None,
        Pending,
        Approve,
        Decline
    }

}
