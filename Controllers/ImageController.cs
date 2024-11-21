using ImageUploaderApp.Data;
using ImageUploaderApp.Models;
using ImageUploaderApp.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ImageController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly BlobStorageService _blobService;

    public ImageController(AppDbContext dbContext, BlobStorageService blobService)
    {
        _dbContext = dbContext;
        _blobService = blobService;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Index(IFormFile file)
    {
        if (file != null)
        {
            string blobUri;
            using (var stream = file.OpenReadStream())
            {
                blobUri = await _blobService.UploadFileAsync(stream, file.FileName, file.ContentType);
            }

            var image = new ImageData
            {
                FileName = file.FileName,
                ContentType = file.ContentType,
                UploadDate = DateTime.UtcNow
            };

            _dbContext.ImageDatas.Add(image);
            await _dbContext.SaveChangesAsync();
            ViewData["Message"] = "Image uploaded successfully.";
            ViewData["ImageUri"] = blobUri;
        }

        return View();
    }
}

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
                    UploadTime = DateTime.UtcNow
                };

                // Add to the database
                _dbContext.ImageDatas.Add(imageData);
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
        var images = await _dbContext.ImageDatas.ToListAsync();
        return View(images);
    }

}