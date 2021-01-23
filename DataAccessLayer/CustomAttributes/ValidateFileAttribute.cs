using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DataAccessLayer.CustomAttributes
{
    // reference { https://sensibledev.com/mvc-multiple-file-upload/ }
    public class ValidateFileAttribute : ValidationAttribute
    {
        private string[] AllowedExtensions { get; set; }
        private int ContentLength { get; set; }

        public ValidateFileAttribute(string[] allowedExtensions, int contentLength)
        {
            AllowedExtensions = allowedExtensions;
            ContentLength = contentLength;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(value is HttpPostedFileBase)
            {
                HttpPostedFileBase file = (HttpPostedFileBase)value;
                string fileName = file.FileName;
                string fileExtension = fileName.Substring(fileName.LastIndexOf(".")).ToLower();
                int fileSize = file.ContentLength;

                if (AllowedExtensions.Contains(fileExtension))
                {
                    if(fileSize <= ContentLength)
                    {
                        return ValidationResult.Success;
                    }
                    else
                    {
                        return new ValidationResult("The Uploaded file must be less than or equal " + ContentLength / 1024 / 1024 + "MB.");
                    }
                }
                else
                {
                    //var extensions = AllowedExtensions.ToString();
                    return new ValidationResult("The Uploaded file must be of one these extensions (" + string.Join(",", AllowedExtensions) + ").");
                }
            }
            else
            {
                return new ValidationResult("You Should Upload File.");
            }
        }
    }
}