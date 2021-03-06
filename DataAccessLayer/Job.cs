//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccessLayer
{
    using System;
    using System.Collections.Generic;
    
    public partial class Job
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Job()
        {
            this.Applies = new HashSet<ApplyForJob>();
        }
    
        public int Id { get; set; }
        public string JobTitle { get; set; }
        public string JobContent { get; set; }
        public string JobImage { get; set; }
        public int CategoryId { get; set; }
        public string PublisherId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public bool IsSuspended { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ApplyForJob> Applies { get; set; }
        public virtual AspNetUser Publisher { get; set; }
        public virtual Category Category { get; set; }
    }
}
