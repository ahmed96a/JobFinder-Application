using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DataAccessLayer.Models
{
    // Part 41
    //------------------------------------

    public class ContactViewModel
    {
        [Required]
        public string Subject { get; set; }

        [Required]
        public string Message { get; set; }
    }

    //------------------------------------
}