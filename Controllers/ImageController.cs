using ImageUploaderApp.Data;
using ImageUploaderApp.Models;
using ImageUploaderApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class HomeController : Controller
{
    private readonly BlobStorageService _blobStorageService;
    private readonly AppDbContext _dbContext;

    public HomeController(BlobStorageService blobStorageService, AppDbContext appDbContext)
    {
        _blobStorageService = blobStorageService;
        _dbContext = appDbContext;
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            // Getting the file stream and the other required parameters
            using (var stream = file.OpenReadStream())
            {
                var result = await _blobStorageService.UploadFileAsync(stream, file.FileName, file.ContentType);

                // Save to the database if upload is successful
                var imageData = new ImageData
                {
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    UploadDate = DateTime.UtcNow,
                    Url = result,  // Assuming 'result' contains the URL from BlobStorageService
                };

                // Add to the database
                _dbContext.FileData.Add(imageData);
                await _dbContext.SaveChangesAsync();

                ViewBag.Message = "Image uploaded successfully!";
            }
        }
        else
        {
            ViewBag.Message = "Please select a valid image file.";
        }

        return View("Upload");
    }

    public async Task<IActionResult> ViewImages()
    {
        var images = await _dbContext.FileData.ToListAsync();
        return View(images);
    }

}