using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplicationMVC.DataAccess.Repository.IRepository;
using WebApplicationMVC.Models.Models;
using WebApplicationMVC.Models.ViewModel;

namespace WebApplicationMVC.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;

		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}

		public IActionResult Index()
		{
			List<Product> productList = _unitOfWork.product.GetAll(includeProperties:"Category").ToList();
			return View(productList);
		}

		public IActionResult Upsert(int? id)
		{
			IEnumerable<SelectListItem> categoryList = _unitOfWork.category.GetAll().Select(u => new SelectListItem()
			{
				Text = u.Name,
				Value = u.Id.ToString()
			});

			//ViewBag.CategoryList = categoryList;

			ProductVM productVM = new ProductVM()
			{
				CategoryList = categoryList,
				Product = new Product()
			};

			if (id == null || id == 0)
			{
				//create
				return View(productVM);
			}
			else
			{
				//update
				productVM.Product = _unitOfWork.product.Get(u => u.Id == id);
				return View(productVM);
			}
		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVM, IFormFile? formFile)
		{
			if (ModelState.IsValid)
			{
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if(formFile != null)
				{
					string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
					string productPath = Path.Combine(wwwRootPath, @"images/product");

					if (!string.IsNullOrEmpty(productVM.Product.ImageURL))
					{
						var oldImagePath = Path.Combine(wwwRootPath,
							productVM.Product.ImageURL.TrimStart('\\'));

						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					using(var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
					{
						formFile.CopyTo(fileStream);
					}

					productVM.Product.ImageURL = @"images/product/" + fileName;
				}

				if (productVM.Product.Id == 0)	
				{
					_unitOfWork.product.Add(productVM.Product);
				}
				else
				{
					_unitOfWork.product.Update(productVM.Product);
				}
				_unitOfWork.Save();
				TempData["success"] = "product created successfully";
				return RedirectToAction("Index");
			}
			else
			{
				productVM.CategoryList = _unitOfWork.category.GetAll().Select(u => new SelectListItem()
				{
					Text = u.Name,
					Value = u.Id.ToString()
				});
				return View(productVM);
			}
		}

		#region API's
		[HttpGet]
		public IActionResult GetAll()
		{
            List<Product> productList = _unitOfWork.product.GetAll(includeProperties: "Category").ToList();
			return Json(new { data = productList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            string productPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

            if (Directory.Exists(finalPath))
            {
                string[] filePaths = Directory.GetFiles(finalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }

                Directory.Delete(finalPath);
            }


            _unitOfWork.product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
