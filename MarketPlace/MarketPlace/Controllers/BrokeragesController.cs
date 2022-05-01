using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MarketPlace.Data;
using MarketPlace.Models;
using MarketPlace.Models.ViewModels;

namespace MarketPlace.Controllers
{
    public class BrokeragesController : Controller
    {
        private readonly MarketDbContext _context;

        public BrokeragesController(MarketDbContext context)
        {
            _context = context;
        }

        // GET: Brokerages
        public async Task<IActionResult> Index(string Id)
        {
            var viewModel = new BrokerageViewModel
            {
                Brokerages = await _context.Brokerages
                  .Include(i => i.Subscriptions)
                  .AsNoTracking()
                  .OrderBy(i => i.Id)
                  .ToListAsync()
            };

            IList<Client> ClientList = await _context.Clients.ToListAsync();

            if (Id != null)
            {
                ViewData["BrokerageId"] = Id;
                //viewModel.Clients = viewModel.Subscriptions.Single(x => x.BrokerageId == Id).Client;

                IList<Subscription> Subscriptions = viewModel.Brokerages.Where(i => Id.Equals(i.Id)).Single().Subscriptions;
                Subscriptions.ToList().ForEach(subscription =>
                {
                    subscription.Client = ClientList.ToList().Where((client) => subscription.ClientId == client.Id).Single();
                });

                viewModel.Subscriptions = Subscriptions;
            }

            return View(viewModel);
        }

        // GET: Brokerages/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brokerage = await _context.Brokerages
                .FirstOrDefaultAsync(m => m.Id == id);
            if (brokerage == null)
            {
                return NotFound();
            }

            return View(brokerage);
        }

        // GET: Brokerages/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Brokerages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Fee")] Brokerage brokerage)
        {
            if (ModelState.IsValid)
            {
                _context.Add(brokerage);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(brokerage);
        }

        // GET: Brokerages/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var brokerage = await _context.Brokerages.FindAsync(id);
            if (brokerage == null)
            {
                return NotFound();
            }
            return View(brokerage);
        }

        // POST: Brokerages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Title,Fee")] Brokerage brokerage)
        {
            if (id != brokerage.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(brokerage);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BrokerageExists(brokerage.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(brokerage);
        }

        // GET: Brokerages/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            AdsViewModel vModel = new AdsViewModel
            {
                Brokerage = await _context.Brokerages.Where(b => b.Id == id).FirstOrDefaultAsync(),
                Advertisements = await _context.Advertisements.Where(a => a.BrokerageId == id).ToListAsync()
            };

            if (vModel.Brokerage == null)
            {
                return NotFound();
            }

            return View(vModel);
        }

        // POST: Brokerages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var brokerage = await _context.Brokerages.FindAsync(id);

            if (id != null)
            {
                IList<Advertisement> ads = await _context.Advertisements.ToListAsync();
                ads = ads.Where(ads => id.Equals(ads.BrokerageId)).ToList();
                if (!ads.Any())
                {
                    _context.Brokerages.Remove(brokerage);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

            }

            return View("Error");
        }

        private bool BrokerageExists(string id)
        {
            return _context.Brokerages.Any(e => e.Id == id);
        }
    }
}
