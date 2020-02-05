using Library.Models.Catalog;
using Library.Models.CheckoutModels;
using LibraryData;
using LibraryData.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _assets;
        private ICheckout _checkouts;

        public CatalogController(ILibraryAsset assets, ICheckout checkoutsService)
        {
            _assets = assets;
            _checkouts = checkoutsService;
        }
        [HttpPost]
        public IActionResult AddAsset(AssetCreateModel newAsset) {

            var statuses = _assets.GetStatuses();

            Book asset = new Book();
            asset.Title = newAsset.Title;
            asset.Status = _assets.GetStatuses().FirstOrDefault(s => s.Id == 6);
            asset.NumberOfCopies = newAsset.NumberOfCopies;
            asset.Location = _assets.GetBranches().FirstOrDefault(b => b.Id == newAsset.CurrentLocationId);
            asset.Year = newAsset.Year;
            asset.Author = newAsset.AuthorOrDirector;
            asset.Cost = newAsset.Cost;
            asset.ISBN = newAsset.ISBN;
            asset.DeweyIndex = newAsset.DeweyCallNumber;
            asset.ImageUrl = newAsset.ImageUrl;

            _assets.AddBook(asset);

            return View("./SuccessAdd");
        }
        public IActionResult Create(int id)
        {
            return View();
        }

        public IActionResult Delete(int id) {
            _assets.Delete(id);

            return View("./SuccessDelete");

        }

        public IActionResult Index(string searchString)
        {
            var assetModels = _assets.GetAll();

            if (!String.IsNullOrEmpty(searchString))
            {
                assetModels = assetModels.Where(s => s.Title.Contains(searchString));
            }

            var listingResult = assetModels
                .Select(result => new AssetIndexListingModel
                {
                    Id = result.Id,
                    ImageUrl = result.ImageUrl,
                    AuthorOrDirector = _assets.GetAuthorOrDirector(result.Id),
                    DeweyCallNumber = _assets.GetDeweyIndex(result.Id),
                    Title = result.Title,
                    Type = _assets.GetType(result.Id)


                });

            var model = new AssetIndexModel()
            {
                Assets = listingResult
            };

            return View(model);
        }

        public IActionResult Detail(int id) {

            var asset = _assets.GetById(id);

            var currentHolds = _checkouts.GetCurrentHolds(id)
                .Select(a => new AssetHoldModel
            {
                HoldPlaced = _checkouts.GetCurrentHoldPlaced(a.Id),
                PatronName = _checkouts.GetCurrentHoldPatronName(a.Id)
            });

            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Type = _assets.GetType(id),
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id)?.Name,
                DeweyCallNumber = _assets.GetDeweyIndex(id),
                CheckoutHistory = _checkouts.GetCheckoutHistory(id),
                ISBN = _assets.GetIsbn(id),
                LatestCheckout = _checkouts.GetLatestCheckout(id),
                CurrentHolds = currentHolds,
                PatronName = _checkouts.GetCurrentCheckoutPatron(id)
            };

            return View(model);
        }

        public IActionResult Checkout(int id)
        {
            var asset = _assets.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                IsCheckedOut = _checkouts.IsCheckedOut(id)
            };
            return View(model);
        }

        public IActionResult CheckIn(int id)
        {
            _checkouts.CheckInItem(id);
            return RedirectToAction("Detail", new { id });
        }

        public IActionResult Hold(int id)
        {
            var asset = _assets.GetById(id);

            var model = new CheckoutModel
            {
                AssetId = id,
                ImageUrl = asset.ImageUrl,
                Title = asset.Title,
                LibraryCardId = "",
                HoldCount = _checkouts.GetCurrentHolds(id).Count()
            };
            return View(model);
        }

        
        public IActionResult MarkLost(int id)
        {
            _checkouts.MarkLost(id);
            return RedirectToAction("Detail", new { id });
        }

        public IActionResult MarkFound(int id)
        {
            _checkouts.MarkFound(id);
            return RedirectToAction("Detail", new { id });
        }

        [HttpPost]
        public IActionResult PlaceCheckout(int assetId, int libraryCardId)
        {
            _checkouts.CheckoutItem(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        [HttpPost]
        public IActionResult PlaceHold(int assetId, int libraryCardId)
        {
            _checkouts.PlaceHold(assetId, libraryCardId);
            return RedirectToAction("Detail", new { id = assetId });
        }

        public IActionResult SuccessAdd() {
            return View();
        }

        public IActionResult SuccessDelete()
        {
            return View();
        }
    }
}
