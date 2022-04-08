using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Assignment2.Data;
using Assignment2.Models;
using Azure.Storage.Blobs;
using Assignment2.Models.ViewModels;
using Azure;

namespace Assignment2.Views
{
    public class AdvertisementsController : Controller
    {
        private readonly MarketDbContext _context;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string containerName = "advertisements";

        public AdvertisementsController(MarketDbContext context, BlobServiceClient blobServiceClient)
        {
            _context = context;
            _blobServiceClient = blobServiceClient;
        }

        // GET: Advertisements
        public async Task<IActionResult> Index(string Id)
        {
            var viewModel = new AdsViewModel
            {
                Advertisements = await _context.Advertisements
                                    .Include(a => a.Brokerage)
                                    .AsNoTracking()
                                    .OrderByDescending(a => a.Id)
                                    .ToListAsync()
            };

            ViewData["BrokerageId"] = Id;

            if (Id != null)
            {
                viewModel.Brokerage = _context.Brokerages.Where(a => a.Id.Equals(Id)).Single();
                viewModel.Advertisements = viewModel.Advertisements.Where(ads => Id.Equals(ads.BrokerageId)).ToList();
            }

            return View(viewModel);
        }




        // GET: Advertisements/Create
        public IActionResult Create(string Id, string BrokerageTitle)
        {
            ViewData["BrokerageId"] = Id;
            ViewData["brokerageTitle"] = BrokerageTitle;
            return View();
        }

        // POST: Advertisements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, string Id, [Bind("Id,FileName,Url,BrokerageId")] Advertisement advertisement)
        {
            if (file == null){
                return View();
            }
            if(advertisement != null && Id != null)
            {
                BlobContainerClient containerClient;
                // Create the container and return a container client object
                try
                {
                    containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName);
                    // Give access to public
                    containerClient.SetAccessPolicy(Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
                }
                catch (RequestFailedException)
                {
                    containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                }

                try
                {
                    string randomFileName = Path.GetRandomFileName();
                    // create the blob to hold the data
                    var blockBlob = containerClient.GetBlobClient(randomFileName);

                    //get url and file name
                    advertisement.Url = containerClient.GetBlobClient(blockBlob.Name).Uri.AbsoluteUri;
                    advertisement.FileName = randomFileName;
                    advertisement.BrokerageId = Id;
                    Brokerage brokerage = await _context.Brokerages.Where(b => b.Id.Equals(Id)).FirstOrDefaultAsync();
                    advertisement.Brokerage = brokerage;

                    //Validate model once again after everything in place
                    ModelState.Clear();
                    TryValidateModel(advertisement);

                    if (!ModelState.IsValid)
                    {
                        return View("Error");
                    }
                    _context.Advertisements.Add(advertisement);
                    _context.Update(brokerage);
                    await _context.SaveChangesAsync();

                    //If exist -> delete
                    if (await blockBlob.ExistsAsync())
                    {
                        await blockBlob.DeleteAsync();
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        // copy the file data into memory
                        await file.CopyToAsync(memoryStream);

                        // navigate back to the beginning of the memory stream
                        memoryStream.Position = 0;

                        // send the file to the cloud
                        await blockBlob.UploadAsync(memoryStream);
                        memoryStream.Close();
                    }
                }
                catch (RequestFailedException)
                {
                    return View("Error");
                }
            }
            return RedirectToAction("Index", new { Id = Id });
        }

        // GET: Advertisements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisements
                .Include(a => a.Brokerage)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (advertisement == null)
            {
                return NotFound();
            }
            ViewData["BrokerageId"] = advertisement.BrokerageId;

            return View(advertisement);
        }

        // POST: Advertisements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            string brokerageId = advertisement.BrokerageId;
            BlobContainerClient containerClient;
            // Get the container and return a container client object
            try
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }
            catch (RequestFailedException)
            {
                return View("Error");
            }

            //Finding a right blob and remove it
            foreach (var blob in containerClient.GetBlobs())
            {
                try
                {
                    // Get the blob that holds the data
                    if (blob.Name == advertisement.FileName)
                    {
                        var blockBlob = containerClient.GetBlobClient(blob.Name);
                        if (await blockBlob.ExistsAsync())
                        {
                            await blockBlob.DeleteAsync();
                        }
                    }
                }
                catch (RequestFailedException)
                {
                    return View("Error");
                }
            }


            //Remove from database
            if (advertisement != null)
            {
                _context.Advertisements.Remove(advertisement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", new { Id = brokerageId });
        }

        private bool AdvertisementExists(int id)
        {
            return _context.Advertisements.Any(e => e.Id == id);
        }
    }
}
