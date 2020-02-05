using LibraryData.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryData
{
    public interface ILibraryAsset
    {
        IEnumerable<LibraryAsset> GetAll();
        LibraryAsset GetById(int id);
        void Add(LibraryAsset newAsset);
        void AddBook(Book newBook);

        void Delete(int id);

        string GetAuthorOrDirector(int id);
        string GetDeweyIndex(int id);
        string GetType(int id);
        string GetTitle(int id);
        string GetIsbn(int id);
        IEnumerable<Status> GetStatuses();
        IEnumerable<LibraryBranch> GetBranches();

        LibraryBranch GetCurrentLocation(int id);


    }
}
