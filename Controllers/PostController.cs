using BlogApp.Data;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{

    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string[] _allowedExtension = { ".jpg", ".jpeg", ".png" };

        public PostController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;

        }

        [HttpGet]
        [AllowAnonymous]

        public IActionResult Index(int? categoryID)
        {
           var postQuery = _context.Posts.Include(p=> p.Category).AsQueryable();
            if (categoryID.HasValue)
            {
                postQuery = postQuery.Where(p => p.CategoryId == categoryID);
            }
            var posts = postQuery.ToList();
           
            ViewBag.Categories = _context.Categories.ToList();

            return View(posts);
               
        }

        

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts
                .Include(p => p.Category).Include(c => c.Comments)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);

        }

        [HttpGet]
        public IActionResult Create()
        {
            var postviewmodel = new PostViewModel();
            postviewmodel.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(postviewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10485760)]
        public async Task<IActionResult> Create(PostViewModel postviewmodel)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                var inputFile = Path.GetExtension(postviewmodel.FeatureImage.FileName).ToLower();
                if (!_allowedExtension.Contains(inputFile))
                {
                    ModelState.AddModelError("", "Only .jpg, .jpeg, .png files are allowed.");
                    return View(postviewmodel); // Return the view with the model to show errors
                }

                postviewmodel.Post.FeatureImagePath = await UploadFiletoFolder(postviewmodel.FeatureImage);
                await _context.Posts.AddAsync(postviewmodel.Post);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            postviewmodel.Categories = _context.Categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            return View(postviewmodel); // Return the view if the model state is invalid
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }


            EditPostViewModel editPostViewModel = new EditPostViewModel
            {
                Post = post,
                Categories = _context.Categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList()
            };
            return View(editPostViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult>Edit(EditPostViewModel editPostViewModel)
        {
            if(!ModelState.IsValid)
            {
                
                return View(editPostViewModel);
            }

            var post = await _context.Posts.AsNoTracking().FirstOrDefaultAsync(p=>p.Id==editPostViewModel.Post.Id);

            if (post == null)
            {
                return NotFound();
            }
            if (editPostViewModel.FeatureImage != null)
            {
                var inputFile = Path.GetExtension(editPostViewModel.FeatureImage.FileName).ToLower();
                if (!_allowedExtension.Contains(inputFile))
                {
                    ModelState.AddModelError("", "Only .jpg, .jpeg, .png files are allowed.");
                    return View(editPostViewModel); // Return the view with the model to show errors
                }
                var oldImagePath = 
                    Path.Combine(_webHostEnvironment.WebRootPath, "Images",Path.GetFileName(post.FeatureImagePath));

                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }

                editPostViewModel.Post.FeatureImagePath = await UploadFiletoFolder(editPostViewModel.FeatureImage);


            }
            else
            {
                editPostViewModel.Post.FeatureImagePath = post.FeatureImagePath;
            }
            _context.Posts.Update(editPostViewModel.Post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");

        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirm(int id)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);
            if(string.IsNullOrEmpty(post.FeatureImagePath))
            {
                var oldImagePath =
                    Path.Combine(_webHostEnvironment.WebRootPath, "Images", Path.GetFileName(post.FeatureImagePath));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }


        public JsonResult AddComment([FromBody] Comment comment)
        {
            comment.CommentDate = DateTime.Now;
            _context.Comments.Add(comment);
            _context.SaveChanges();
            return Json(new
            {
                userName = comment.UserName,
                commentDate = comment.CommentDate.ToString("MMMM dd,yyyy"),
                content = comment.Content
            });
        }

        private async Task<string> UploadFiletoFolder(IFormFile file)
        {
            var inputFile = Path.GetExtension(file.FileName);
            var fileName = Guid.NewGuid().ToString() + inputFile;
            var wwwRootPath = _webHostEnvironment.WebRootPath;
            var imagesFolderPath = Path.Combine(wwwRootPath, "images");

            if (!Directory.Exists(imagesFolderPath))
            {
                Directory.CreateDirectory(imagesFolderPath);
            }

            var filePath = Path.Combine(imagesFolderPath, fileName);
            try
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it)
                throw new Exception("File upload failed", ex);
            }
            return "images/" + fileName;
        }
    }
}