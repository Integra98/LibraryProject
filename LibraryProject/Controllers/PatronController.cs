using Library.Models.Patron;
using LibraryData;
using LibraryData.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Controllers
{
    public class PatronController : Controller
    {

        private readonly IPatron _patronService;

        public PatronController(IPatron patronService)
        {
            _patronService = patronService;
        }

        [HttpPost]
        public IActionResult Add(PatronCreateModel newPatron)
        {

            Patron asset = new Patron();
            asset.FirstName = newPatron.FirstName;
            asset.LastName = newPatron.LastName;
            asset.Address = newPatron.Address;
            asset.HomeLibraryBranch= _patronService.GetBranches().FirstOrDefault(b => b.Id == newPatron.HomeLibraryId);
            asset.TelephoneNumber = newPatron.Telephone;
            asset.LibraryCard= new LibraryCard();

            _patronService.Add(asset);

            return View("./SuccessAdd");
        }
        public IActionResult Create(int id)
        {
            return View();
        }

        public IActionResult Delete(int id)
        {
            _patronService.Delete(id);

            return View("./SuccessDelete");

        }

        public IActionResult Index(string searchString)
        {
            var allPatrons = _patronService.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                allPatrons = allPatrons.Where(s => s.FirstName.Contains(searchString) || s.LastName.Contains(searchString));
            }

            var patronModels = allPatrons
                .Select(p => new PatronDetailModel
                {
                    Id = p.Id,
                    LastName = p.LastName ?? "No First Name Provided",
                    FirstName = p.FirstName ?? "No Last Name Provided",
                    LibraryCardId = p.LibraryCard.Id,
                    OverdueFees = p.LibraryCard?.Fees,
                    HomeLibrary = p.HomeLibraryBranch?.Name
                }).ToList();

            var model = new PatronIndexModel
            {
                Patrons = patronModels
            };

            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var patron = _patronService.Get(id);

            var model = new PatronDetailModel
            {
                Id = patron.Id,
                LastName = patron.LastName ?? "No Last Name Provided",
                FirstName = patron.FirstName ?? "No First Name Provided",
                Address = patron.Address ?? "No Address Provided",
                HomeLibrary = patron.HomeLibraryBranch?.Name ?? "No Home Library",
                MemberSince = patron.LibraryCard?.Created,
                OverdueFees = patron.LibraryCard?.Fees,
                LibraryCardId = patron.LibraryCard.Id,
                Telephone = string.IsNullOrEmpty(patron.TelephoneNumber) ? "No Telephone Number Provided" : patron.TelephoneNumber,
                AssetsCheckedOut = _patronService.GetCheckouts(id).ToList(),
                CheckoutHistory = _patronService.GetCheckoutHistory(id),
                Holds = _patronService.GetHolds(id)
            };

            return View(model);
        }

    }
}
