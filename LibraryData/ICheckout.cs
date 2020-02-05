using LibraryData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData
{
    public interface ICheckout
    {
        void Add(Checkout newCheckout);   
        IEnumerable<Checkout> GetAll();
        IEnumerable<CheckoutHistory> GetCheckoutHistory(int id);
        IEnumerable<Hold> GetCurrentHolds(int id);
        bool IsCheckedOut(int id);

        Checkout GetById(int id);
        Checkout GetLatestCheckout(int id);
        string GetCurrentHoldPatronName(int id);
        string GetCurrentCheckoutPatron(int id);
        DateTime GetCurrentHoldPlaced(int id);

        void CheckoutItem(int id, int libraryCardId);
        void CheckInItem(int id);
        void PlaceHold(int assetId, int libraryCardId);
        void MarkLost(int id);
        void MarkFound(int id);
    }
}
