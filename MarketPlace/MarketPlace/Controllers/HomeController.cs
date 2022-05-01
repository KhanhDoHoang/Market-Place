using MarketPlace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MarketPlace.Controllers
{
    public class HomeController : Controller
    {
        private readonly MarketDbContext _context;

        public HomeController(MarketDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Error()
        {
            return View();
        }
    }
}
