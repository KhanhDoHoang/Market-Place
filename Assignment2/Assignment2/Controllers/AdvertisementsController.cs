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
using Azure;
using Assignment2.Models.ViewModels;

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
        public async Task<IActionResult> Index(string BrokerageId)
        {
            // var marketDbContext = _context.Advertisements.Include(a => a.BrokerageId.Equals(Id));

            var viewModel = new AdsViewModel
            {
                Advertisements = await _context.Advertisements
                                    .Include(a => a.Brokerage)
                                    .AsNoTracking()
                                    .OrderByDescending(a => a.Id)
                                    .ToListAsync()
            };



            ViewData["BrokerageId"] = BrokerageId;

            if (BrokerageId != null)
            {
                IList<Advertisement> Ads = viewModel.Advertisements.Where(ads => ads.Brokerage.Id.Equals(BrokerageId)).ToList();
                Brokerage brokerage = _context.Brokerages.Where(a => a.Id.Equals(BrokerageId)).ToList().FirstOrDefault();
                viewModel.Brokerage = brokerage;
                viewModel.Advertisements = Ads;
            }

            return View(viewModel);


            // Create a container for organizing blobs within the storage account.
/*            BlobContainerClient containerClient;
            try
            {
                containerClient = await _blobServiceClient.CreateBlobContainerAsync(containerName, Azure.Storage.Blobs.Models.PublicAccessType.BlobContainer);
            }
            catch (RequestFailedException e)
            {
                containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            }

            List<Advertisement> advertisements = new();

            foreach (var blob in containerClient.GetBlobs())
            {
                // Blob type will be BlobClient, CloudPageBlob or BlobClientDirectory
                // Use blob.GetType() and cast to appropriate type to gain access to properties specific to each type
                advertisements.Add(new Advertisement { FileName = blob.Name, Url = containerClient.GetBlobClient(blob.Name).Uri.AbsoluteUri });
            }
            return View(advertisements);*/
        }




        // GET: Advertisements/Create
        public IActionResult Create()
        {
            ViewData["BrokerageId"] = new SelectList(_context.Brokerages, "Id", "Id");
            return View();
        }

        // POST: Advertisements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, string BrokerageId)
        {
            //[Bind("Id,FileName,Url,BrokerageId")] Advertisement advertisement

            var viewModel = new AdsViewModel
            {
                Advertisements = await _context.Advertisements
                                    .Include(a => a.Brokerage)
                                    .AsNoTracking()
                                    .OrderByDescending(a => a.Id)
                                    .ToListAsync()
            };

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
                Advertisement.Url = containerClient.GetBlobClient(blockBlob.Name).Uri.AbsoluteUri;
                AnswerImage.FileName = randomFileName;

                //Validate model once again after everything in place
                ModelState.Clear();
                TryValidateModel(AnswerImage);

                if (!ModelState.IsValid)
                {
                    /*return Page();*/
                    return RedirectToPage("/Error");
                }

                _context.AnswerImages.Add(AnswerImage);
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
                View("Error");
            }

            return RedirectToAction("Index");


            /*if (ModelState.IsValid)
            {
                _context.Add(advertisement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrokerageId"] = new SelectList(_context.Brokerages, "Id", "Id", advertisement.BrokerageId);
            return View(advertisement);*/
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

            return View(advertisement);
        }

        // POST: Advertisements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisements.FindAsync(id);
            _context.Advertisements.Remove(advertisement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdvertisementExists(int id)
        {
            return _context.Advertisements.Any(e => e.Id == id);
        }
    }
}
