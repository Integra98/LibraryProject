using Library.Models.Branch;
using LibraryData;
using LibraryData.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Controllers
{
    public class BranchController : Controller
    {

        private readonly ILibraryBranch _branchService;

        public BranchController(ILibraryBranch branchService)
        {
            _branchService = branchService;
        }

        [HttpPost]
        public IActionResult Add(BranchCreateModel newBranch)
        {

            LibraryBranch asset = new LibraryBranch();
            asset.Name = newBranch.BranchName;
            asset.Address = newBranch.Address;
            asset.Telephone = newBranch.Telephone;
            asset.Description = newBranch.Description;
            asset.ImageUrl = newBranch.ImageUrl;

            _branchService.Add(asset);

            return View("./SuccessAdd");
        }
        public IActionResult Create(int id)
        {
            return View();
        }

        public IActionResult Delete(int id)
        {
            _branchService.Delete(id);

            return View("./SuccessDelete");

        }

        public IActionResult Index()
        {
            var branchModels = _branchService.GetAll()
                .Select(br => new BranchDetailModel
                {
                    Id = br.Id,
                    BranchName = br.Name,
                    NumberOfAssets = _branchService.GetAssetCount(br.Id),
                    NumberOfPatrons = _branchService.GetPatronCount(br.Id),
                    IsOpen = _branchService.IsBranchOpen(br.Id)
                }).ToList();



            var model = new BranchIndexModel
            {
                Branches = branchModels
            };

            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var branch = _branchService.Get(id);
            var model = new BranchDetailModel
            {
                Id = branch.Id,
                BranchName = branch.Name,
                Description = branch.Description,
                Address = branch.Address,
                Telephone = branch.Telephone,
                BranchOpenedDate = branch.OpenDate.ToString("yyyy-MM-dd"),
                NumberOfPatrons = _branchService.GetPatronCount(id),
                NumberOfAssets = _branchService.GetAssetCount(id),
                TotalAssetValue = _branchService.GetAssetsValue(id),
                ImageUrl = branch.ImageUrl,
                HoursOpen = _branchService.GetBranchHours(id)
            };

            return View(model);
        }

    }
}
