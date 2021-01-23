using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DataAccessLayer
{
    // There is no need to add Display property since we only need the validation properties for the web api, but we add them so if we will make the mvc project use that DataAccessLayer instead of the entity classes that exist in the model folder of the Mvc project.

    public class CategoryMetaData
    {
        [Required]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Required]
        [Display(Name = "Category Description")]
        public string CategoryDescription { get; set; }
    }

    [MetadataType(typeof(CategoryMetaData))]
    public partial class Category
    {

    }

    //-------------------------------------------------

    public partial class JobMetaData
    {
        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; }


        [Required]
        [Display(Name = "Job Content")]
        public string JobContent { get; set; }

        
        [DisplayFormat(ApplyFormatInEditMode = true)]
        [Display(Name = "Job Image")]
        public string JobImage { get; set; }


        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }


        [Display(Name = "Job Status")]
        public bool IsSuspended { get; set; }


        [Display(Name = "Job Category")]
        public int CategoryId { get; set; }

    }

    [MetadataType(typeof(JobMetaData))]
    public partial class Job
    {        
        [NotMapped]
        [Display(Name = "Job Image")]
        [Required]
        [CustomAttributes.ValidateFile(new string[] { ".jpg", ".png", ".jpeg" }, 2*1024*1024)]
        public HttpPostedFileBase JobImageFile { get; set; }
    }

    //-------------------------------------------------

    public class NotificationMetaData
    {
        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }
    }

    [MetadataType(typeof(NotificationMetaData))]
    public partial class Notification
    {

    }

    //------------------------------------------------

    public class ApplyForJobMetaData
    {
        //[DataType(DataType.Upload)] // that attribue has no use, so there is no need to it
        [Required]
        public string CV { get; set; }
    }

    [MetadataType(typeof(ApplyForJobMetaData))]
    public partial class ApplyForJob
    {
        
    }

    //------------------------------------------------

    public class AspNetUserMetaData
    {
        [Required]
        public string UserType { get; set; }
    }

    [MetadataType(typeof(AspNetUserMetaData))]
    public partial class AspNetUser
    {

    }

    //------------------------------------------------
}
