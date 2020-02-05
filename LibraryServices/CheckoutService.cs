
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryData;
using LibraryData.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryServices
{
    public class CheckoutService : ICheckout
    {
        private readonly LibraryContext _context;

        public CheckoutService(LibraryContext context)
        {
            _context = context;
        }

        public void Add(Checkout newCheckout)
        {
            _context.Add(newCheckout);
            _context.SaveChanges();
        }

        public void CheckInItem(int id)
        {
            var now = DateTime.Now;

            var item = _context.LibraryAssets
                .First(a => a.Id == id);


            RemoveExistingCheckouts(id);

            CloseExistingCheckoutHistory(id, now);

            var currentHolds = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(a => a.LibraryAsset.Id == id);


            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(id, currentHolds);
                return;
            }


            UpdateAssetStatus(id, "Available");
            _context.SaveChanges();
        }

        private void CheckoutToEarliestHold(int id, IQueryable<Hold> currentHolds)
        {
            var earliestHold = currentHolds
                .OrderBy(holds => holds.HoldPlaced)
                .FirstOrDefault();

            var card = earliestHold.LibraryCard;

            _context.Remove(earliestHold);
            _context.SaveChanges();
            CheckoutItem(id, card.Id);
        }

        public void CheckoutItem(int id, int libraryCardId)
        {
            if (IsCheckedOut(id)) { return; }

            var item = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == id);

            UpdateAssetStatus(id, "Checked Out");

            var now = DateTime.Now;

            var libraryCard = _context.LibraryCards
                .Include(c => c.Checkouts)
                .FirstOrDefault(a => a.Id == libraryCardId);

            var checkout = new Checkout
            {
                LibraryAsset = item,
                LibraryCard = libraryCard,
                Since = now,
                Until = GetDefaultCheckoutTime(now)
            };

            _context.Add(checkout);

            var checkoutHistory = new CheckoutHistory
            {
                CheckedOut = now,
                LibraryAsset = item,
                LibraryCard = libraryCard
            };

            _context.Add(checkoutHistory);
            _context.SaveChanges();
        }

        public bool IsCheckedOut(int id)
        {
            var isCheckedOut = _context.Checkouts
                .Where(a => a.LibraryAsset.Id == id)
                .Any();

            return isCheckedOut;
        }

        private DateTime GetDefaultCheckoutTime(DateTime now)
        {
            return now.AddDays(30);
        }

        public IEnumerable<Checkout> GetAll()
        {
            return _context.Checkouts;
        }

        public Checkout GetById(int id)
        {
            return _context.Checkouts.FirstOrDefault(c => c.Id == id);
        }

        public IEnumerable<CheckoutHistory> GetCheckoutHistory(int id)
        {
            return _context.CheckoutHistories
                .Include(h => h.LibraryAsset)
                .Include(h => h.LibraryCard)
                .Where(h => h.LibraryAsset.Id == id);

        }

        public string GetCurrentHoldPatronName(int holdId)
        {
            var hold = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(v => v.Id == holdId);

            var cardId = hold
                .Include(a => a.LibraryCard)
                .Select(a => a.LibraryCard.Id)
                .FirstOrDefault();

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .First(p => p.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        public DateTime GetCurrentHoldPlaced(int id)
        {
            return _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .FirstOrDefault(v => v.Id == id)
                .HoldPlaced;


        }

        public IEnumerable<Hold> GetCurrentHolds(int id)
        {
            return _context.Holds
                 .Include(h => h.LibraryAsset)
                 .Where(h => h.LibraryAsset.Id == id);
        }

        public void MarkFound(int id)
        {
            var now = DateTime.Now;

            var checkout = _context.Checkouts
               .Include(a => a.LibraryCard)
               .Include(a => a.LibraryAsset)
               .FirstOrDefault(a => a.LibraryAsset.Id == id);

            var asset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == checkout.LibraryAsset.Id);

            checkout.LibraryCard.Fees = checkout.LibraryCard.Fees - asset.Cost;
            
            RemoveExistingCheckouts(id);
            CloseExistingCheckoutHistory(id, now);

            var currentHolds = _context.Holds
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .Where(a => a.LibraryAsset.Id == id);

            if (currentHolds.Any())
            {
                CheckoutToEarliestHold(id, currentHolds);
                return;
            }
            else {
                UpdateAssetStatus(id, "Available");
            }


            _context.SaveChanges();
        }

        private void UpdateAssetStatus(int id, string newStatus)
        {
            var item = _context.LibraryAssets
               .FirstOrDefault(a => a.Id == id);

            _context.Update(item);

            item.Status = _context.Statuses
                .FirstOrDefault(a => a.Name == newStatus);
        }

        private void CloseExistingCheckoutHistory(int id, DateTime now)
        {
            var history = _context.CheckoutHistories
                .FirstOrDefault(h =>
                    h.LibraryAsset.Id == id
                    && h.CheckedIn == null);

            if (history != null)
            {
                _context.Update(history);
                history.CheckedIn = now;
            }
        }

        private void RemoveExistingCheckouts(int id)
        {
            var checkout = _context.Checkouts
                .FirstOrDefault(a => a.LibraryAsset.Id == id);

            if (checkout != null)
            {
                _context.Remove(checkout);
            }
        }

        public void MarkLost(int id)
        {
            var checkout = _context.Checkouts
                .Include(a => a.LibraryCard)
                .Include(a => a.LibraryAsset)
                .FirstOrDefault(a => a.LibraryAsset.Id == id);

            var asset = _context.LibraryAssets
                .FirstOrDefault(a => a.Id == checkout.LibraryAsset.Id);

            checkout.LibraryCard.Fees = checkout.LibraryCard.Fees + asset.Cost;

            UpdateAssetStatus(id, "Lost");

            _context.SaveChanges();
        }

        public void PlaceHold(int assetId, int libraryCardId)
        {
            var now = DateTime.Now;

            var asset = _context.LibraryAssets
                .Include(a => a.Status)
                .First(a => a.Id == assetId);

            var card = _context.LibraryCards
                .First(a => a.Id == libraryCardId);


            if (asset.Status.Name == "Available")
            {
                UpdateAssetStatus(assetId, "On Hold");
            }


            var hold = new Hold
            {
                HoldPlaced = now,
                LibraryAsset = asset,
                LibraryCard = card
            };

            _context.Add(hold);
            _context.SaveChanges();
        }

        public Checkout GetLatestCheckout(int id)
        {

            return _context.Checkouts
                .Where(c => c.LibraryAsset.Id == id)
                .OrderByDescending(c => c.Since)
                .FirstOrDefault();
        }

        public string GetCurrentCheckoutPatron(int id)
        {
            var checkout = GetCheckoutByAssetId(id);
            if (checkout == null) {
                return "Not checked out";
            }
            
            var cardId = checkout.LibraryCard.Id;

            var patron = _context.Patrons
                .Include(p => p.LibraryCard)
                .First(c => c.LibraryCard.Id == cardId);

            return patron.FirstName + " " + patron.LastName;
        }

        private Checkout GetCheckoutByAssetId(int id)
        {
            return _context.Checkouts
                .Include(a => a.LibraryAsset)
                .Include(a => a.LibraryCard)
                .FirstOrDefault(a => a.LibraryAsset.Id == id);

        }


    }
}