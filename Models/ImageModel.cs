using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using ImageUploaderApp.Models;

namespace ImageUploaderApp.Models
{
    public class ImageData
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? FileName { get; set; }
        [Required]
        public string? ContentType { get; set; }
        [Required]
        public DateTime UploadDate { get; set; }
          // Add these properties
        public string Url { get; set; }  // This will store the SAS URL
        // Optional: Time the image was uploaded
    }
}