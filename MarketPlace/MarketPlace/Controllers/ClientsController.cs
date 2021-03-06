using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketPlace.Data;
using MarketPlace.Models;
using MarketPlace.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Controllers
{
    public class ClientsController : Controller
    {
        private readonly MarketDbContext _context;

        public ClientsController(MarketDbContext context)
        {
            _context = context;
        }

        // GET: Clients
        public async Task<IActionResult> Index(int? Id)
        {
            //return View(await _context.Clients.ToListAsync());

            var viewModel = new BrokerageViewModel
            {
                Clients = await _context.Clients
                  .Include(i => i.Subscriptions)
                  .AsNoTracking()
                  .OrderBy(i => i.Id)
                  .ToListAsync()
            };

            IList<Brokerage> BrokerageList = await _context.Brokerages.ToListAsync();

            //viewModel.Clients = viewModel.Subscriptions.Single(x => x.BrokerageId == Id).Client;
            if (Id.HasValue) {
                if (!viewModel.Clients.Where(i => Id == i.Id).Any()) { throw new Exception("Invalid Client Id"); }
                
                IList<Subscription> Subscriptions = viewModel.Clients.Where(i => Id == i.Id).FirstOrDefault().Subscriptions;
                Subscriptions.ToList().ForEach(subscription =>
                {
                    subscription.Brokerage = BrokerageList.ToList().Where((brokerage) => subscription.BrokerageId == brokerage.Id).Single();
                });

                viewModel.Subscriptions = Subscriptions;
            }

            return View(viewModel);

        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FirstName");

            return View();
        }

        // POST: Clients/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LastName,FirstName,BirthDate")] Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FirstName");

            return View(client);
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients.FindAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FirstName");

            return View(client);
        }

        // POST: Clients/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,LastName,FirstName,BirthDate")] Client client)
        {
            if (id != client.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id))
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
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "FirstName");

            return View(client);
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/EditSubscriptions/5
        public async Task<IActionResult> EditSubscriptions(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = new ClientSubscriptionsViewModel
            {
                Client = await _context.Clients.FindAsync(id),
            };

            if (viewModel.Client == null)
            {
                return NotFound();
            }

            //list of brokerages 
            IList<Brokerage> BrokerageList = await _context.Brokerages.ToListAsync(); //list of brokerages 
            //model view list of all brokerages (check IsMember)
            IList<BrokerageSubscriptionsViewModel> SubscriptionList = new List<BrokerageSubscriptionsViewModel>();
            BrokerageList.ToList().ForEach(brokerage =>
            {
               bool isSubbed = _context.Subscriptions.ToList().Any(subscription => subscription.BrokerageId.Equals(brokerage.Id) && subscription.ClientId.Equals(id));
                
                BrokerageSubscriptionsViewModel subscriptionModel = new BrokerageSubscriptionsViewModel
                {
                    BrokerageId = brokerage.Id, 
                    Title = brokerage.Title,
                    IsMember = isSubbed
                };
                
                //add to subscriptionList
                SubscriptionList.Add(subscriptionModel);
            });

            viewModel.Subscriptions = SubscriptionList.OrderBy(s => !s.IsMember).ThenBy(s => s.Title);

            return View(viewModel);
        }

        // GET: Clients/EditSubscriptions/5 (Add)
        public async Task<IActionResult> AddSubscriptions(int clientId, string brokerageId)
        {

            Client client = _context.Clients.Where(c => c.Id == clientId).Single();
            Brokerage brokerage = _context.Brokerages.Where(c => c.Id == brokerageId).Single();
            if(client == null || brokerage == null)
            {
                return View("Error");   
            }

            Subscription newSub = new()
            {
                ClientId = clientId,
                BrokerageId = brokerageId,
                Client = client,
                Brokerage = brokerage,
            };
         
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(newSub);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return View("Error");
                }
            }
            return RedirectToAction("EditSubscriptions", new { Id = clientId });
        }

        // GET: Clients/EditSubscriptions/5 (Delete)
        public async Task<IActionResult> RemoveSubscriptions(int clientId, string brokerageId)
        {
            var removedSub = await _context.Subscriptions.Where(sub => brokerageId.Equals(sub.BrokerageId) && sub.ClientId == clientId).SingleOrDefaultAsync();

            if (removedSub == null)
            {
                return View("Error");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Subscriptions.Remove(removedSub);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    return View("Error");
                }
            }
            return RedirectToAction("EditSubscriptions", new { Id = clientId });
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}
