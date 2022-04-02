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
        public async Task<IActionResult> Index(string Id)
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

            ViewData["BrokerageId"] = Id;

            if (Id != null)
            {
                viewModel.Brokerage = _context.Brokerages.Where(a => a.Id.Equals(Id)).Single();
                viewModel.Advertisements = viewModel.Advertisements.Where(ads => Id.Equals(ads.BrokerageId)).ToList();
            }

            return View(viewModel);
        }




        // GET: Advertisements/Create
        public IActionResult Create()
        {
            ViewData["BrokerageId"] = new SelectList(_context.Brokerages, "Id", "Id");
            return View();
        }

        // POST: Advertisements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile file, string Id, [Bind("Id,FileName,Url,BrokerageId")] Advertisement advertisement)
        {
            //

            /*var viewModel = new FileInputViewModel
            {
                BrokerageId = BrokerageId,
                File = file,
            };

            if(BrokerageId != null)
            {
                Brokerage brokerage = await _context.Brokerages.Where(b => b.Equals(BrokerageId)).FirstOrDefaultAsync();
                viewModel.BrokerageTitle = brokerage.Title;
            }*/

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
                        View("Error");
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
                    View("Error");
                }
                /*if (ModelState.IsValid)
                    {
                    _context.Add(advertisement);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                    }
                    ViewData["BrokerageId"] = new SelectList(_context.Brokerages, "Id", "Id", advertisement.BrokerageId);*/
                //return View(advertisement);
            }
            return RedirectToAction("Index");
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
