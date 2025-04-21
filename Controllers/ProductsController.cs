using BestStoreMVC.Models;
using BestStoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System;

namespace BestStoreMVC.Controllers
{
	[Authorize(Roles = "admin" )]
	[Route("/Admin/[controller]/{action=Index}/{id?}")]
	public class ProductsController : Controller
	{
		private readonly ApplicationDBContext context;
		private readonly IWebHostEnvironment environment;
		private readonly int pageSize = 5;

		public ProductsController(ApplicationDBContext context, IWebHostEnvironment enviornment)
		{
			this.context = context;
			this.environment = enviornment;
		}
		public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
		{
			IQueryable<Product> query = context.Products;

			//serach
			if (search != null)
			{
				query = query.Where(p => p.Name.Contains(search) || p.Brand.Contains(search));
			}
			//sort
			string[] validColumns = { "Id", "Name", "Brand", "Category", "Price", "CreateAt" };
			string[] validOrderBy = { "desc", "asc" };

			if (!validColumns.Contains(column))
			{
				column = "Id";
			}

			if (!validOrderBy.Contains(orderBy))
			{
				orderBy ="desc";
			}

			if (column == "Name")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Name);
				}
				else
				{
					query = query.OrderByDescending(p => p.Name);
				}
			}
			else if (column == "Brand")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Brand);
				}
				else
				{
					query = query.OrderByDescending(p => p.Brand);
				}
			}
			else if (column == "Category")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Category);
				}
				else
				{
					query = query.OrderByDescending(p => p.Category);
				}
			}
			else if (column == "Price")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Price);
				}
				else
				{
					query = query.OrderByDescending(p => p.Price);
				}
			}
			else if (column == "CreateAt")
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.CreateAt);
				}
				else
				{
					query = query.OrderByDescending(p => p.CreateAt);
				}
			}
			else
			{
				if (orderBy == "asc")
				{
					query = query.OrderBy(p => p.Id);
				}
				else
				{
					query = query.OrderByDescending(p => p.Id);
				}
			}

			//pagination

			if (pageIndex < 1)
			{
				pageIndex = 1;
			}

			decimal count = query.Count();
			int totalPages = (int)Math.Ceiling(count / pageSize);
			query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

			var products = query.ToList();

			ViewData["PageIndex"] = pageIndex;
			ViewData["TotalPages"] = totalPages;
			ViewData["Search"] = search ?? "";
			ViewData["Column"] = column;
			ViewData["OrderBy"] = orderBy;

			return View(products);
		}

		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public IActionResult Create(ProductDto productDto)
		{
			if (productDto.ImageFile == null)
			{
				ModelState.AddModelError("ImageFile", "The Image File is required");
			}
			if (!ModelState.IsValid)
			{
				return View(productDto);
			}

			string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssffff");
			newFileName += Path.GetExtension(productDto.ImageFile!.FileName);

			string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
			using (var stream = System.IO.File.Create(imageFullPath))
			{
				productDto.ImageFile.CopyTo(stream);
			}

			Product product = new Product()
			{
				Name = productDto.Name,
				Brand = productDto.Brand,
				Category = productDto.Category,
				Price = productDto.Price,
				Description = productDto.Description,
				ImageFileName = newFileName,
				CreateAt = DateTime.Now,
			};

			context.Products.Add(product);
			context.SaveChanges();

			return RedirectToAction("Index", "Products");
		}
		public IActionResult Edit(int id)
		{
			var product = context.Products.Find(id);

			if (product == null)
			{
				return RedirectToAction("Index", "Products");
			}

			var productDto = new ProductDto()
			{
				Name = product.Name,
				Brand = product.Brand,
				Category = product.Category,
				Price = product.Price,
				Description = product.Description,
			};

			ViewData["ProductId"] = product.Id;
			ViewData["ImageFileName"] = product.ImageFileName;
			ViewData["CreateAt"] = product.CreateAt.ToString("MM/dd/yyyy");

			return View(productDto);
		}
		[HttpPost]
		public IActionResult Edit(int id, ProductDto productDto)
		{
			var product = context.Products.Find(id);
			if (product == null)
			{
				return RedirectToAction("Index", "Products");
			}

			if (!ModelState.IsValid)
			{
				ViewData["ProductId"] = product.Id;
				ViewData["ImageFileName"] = product.ImageFileName;
				ViewData["CreateAt"] = product.CreateAt.ToString("MM/dd/yyyy");
				return View(productDto);
			}

			//update the image file if a new image file added
			string newFileName = product.ImageFileName;
			if (productDto.ImageFile != null)
			{
				newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
				newFileName += Path.GetExtension(productDto.ImageFile.FileName);

				string imageFullPath = environment.WebRootPath + "/products/" + newFileName;
				using (var stream = System.IO.File.Create(imageFullPath))
				{
					productDto.ImageFile.CopyTo(stream);
				}

				// delete the old image
				string oldImageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
				System.IO.File.Delete(oldImageFullPath);
			}

			product.Name = productDto.Name;
			product.Brand = productDto.Brand;
			product.Category = productDto.Category;
			product.Description = productDto.Description;
			product.Price = productDto.Price;
			product.ImageFileName = newFileName;

			context.SaveChanges();

			return RedirectToAction("Index", "Products");
		}

		public IActionResult Delete(int id)
		{
			var product = context.Products.Find(id);

			if (product == null)
			{
				return RedirectToAction("Index", "Products");
			}

			string imageFullPath = environment.WebRootPath + "/products/" + product.ImageFileName;
			System.IO.File.Delete(imageFullPath);

			context.Products.Remove(product);
			context.SaveChanges(true);

			return RedirectToAction("Index", "Products");
		}
	}
}
