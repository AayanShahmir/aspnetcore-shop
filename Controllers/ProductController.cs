using BIsm2.Models;
using BIsm2.Services;
using BIsm2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web;


namespace Ism.Controllers
{
    public class ProductController : Controller
    {

         private readonly ICompositeViewEngine _viewEngine;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailsend;


        public ProductController(AppDbContext context, IWebHostEnvironment env,
            UserManager<Users> userManager, SignInManager<Users> signInManager,
            RoleManager<IdentityRole> roleManager, IEmailSender emailsend, ICompositeViewEngine viewEngine
            )
        {
            _context = context;
            _env = env;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailsend = emailsend;
            _viewEngine = viewEngine;


        }
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Media) // eager load media
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> SellersPg()
        {
            var products = await _context.Products
                .Include(p => p.Media)
                .ToListAsync();

            return View(products);
        }

        public IActionResult Signup()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Signup(SignupViewModel svm)
        {
            if (!ModelState.IsValid) return View(svm);

            if (await _userManager.FindByNameAsync(svm.UserName) != null)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
                return View(svm);
            }

            if (await _userManager.FindByEmailAsync(svm.Email) != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(svm);
            }

            var user = new Users
            {
                UserName = svm.UserName,
                Email = svm.Email
            };

            var result = await _userManager.CreateAsync(user, svm.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(svm);
            }
            else
            {
                var Token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("Confirm", "Product", new
                {
                    email = user.Email,
                    token = HttpUtility.UrlEncode(Token)
                }, Request.Scheme);
                await _emailsend.SendEmailAsync(user.Email, confirmationLink);
                await _userManager.AddToRoleAsync(user, "User");

                return RedirectToAction("Index", "Product");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Confirm(string email, string token)
        {
            if (email == null)
            {
                ModelState.AddModelError("", "something went wrong");
                return View();
            }
            var user = await _userManager.FindByEmailAsync(email);

            var decodetoken = HttpUtility.UrlDecode(token);
            var IsEmailVerified = await _userManager.ConfirmEmailAsync(user, decodetoken);
            var cat = await _userManager.FindByEmailAsync(email);
            if (IsEmailVerified.Succeeded && user.EmailConfirmed)
            {
                await _userManager.AddToRoleAsync(cat, "User");
                return RedirectToAction("EmailConfirmed");
            }
            return BadRequest("Email confirmation failed.");

        }
        public IActionResult EmailConfirmed()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel logvm, string returnUrl = null)
        {
            if (!ModelState.IsValid) return View(logvm);
            //var User = await _userManager.FindByEmailAsync(logvm.UserName);
            var user = await _userManager.FindByEmailAsync(logvm.UserIdentifier) ?? await _userManager.FindByNameAsync(logvm.UserIdentifier);

            if (user == null)
            {
                ModelState.AddModelError("", "user not found");
                return View(logvm);
            }
            if (!user.EmailConfirmed)
            {
                ModelState.AddModelError("", "User not found");
                await _userManager.DeleteAsync(user);
                return View(logvm);
            }
            var result = await _signInManager.PasswordSignInAsync(user, logvm.Password, logvm.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        if (returnUrl.Contains("/Product/AddToCart"))
                        {
                            // Instead, go to Index
                            return RedirectToAction("Cart", "Product");
                        }
                        // If ReturnUrl points to Review, rewrite it to Details
                        if (returnUrl.Contains("/Product/Review"))
                        {
                            // extract pid from query string
                            var uri = new Uri($"{Request.Scheme}://{Request.Host}{returnUrl}");
                            var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                            var pid = query["pid"].FirstOrDefault();
                            return RedirectToAction("Details", "Product", new { id = pid });
                        }
                        return Redirect(returnUrl);
                    }

                }
                return RedirectToAction("Index", "Product");
            }
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(logvm);
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Product");
        }

        // CREATE
        // GET: Product/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product product, List<IFormFile> files)
        {
            if (files.Count > 10)
            {
                ModelState.AddModelError("", "You can upload a maximum of 10 files.");
                return View(product);
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
           
            foreach (var file in files)
            {
                using (var ms = new MemoryStream())
                {
                    await file.CopyToAsync(ms);
                   
                    var media = new ProductMedia
                    {
                        ProductId = product.Id,
                        FileData = ms.ToArray(),
                        ContentType = file.ContentType,
                        Type = file.ContentType.StartsWith("video") ? MediaType.Video : MediaType.Image
                    };
                    
                    _context.ProductMedias.Add(media);
                    await _context.SaveChangesAsync();
                }
            }
           
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int id)
        {
            DetailsViewModel detailsvm = new DetailsViewModel();
            if (id == null)
            {
                return NotFound();
            }
            else
            {

                detailsvm.Product = await _context.Products.Include(p => p.Media)
                                    .FirstOrDefaultAsync(p => p.Id == id);
                detailsvm.Comment = new Comment();
                detailsvm.Comments = await _context.Comments.Where(c => c.ProductId == id).ToListAsync();
                return View(detailsvm);

            }
        }
        // GET: Product/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View("~/Views/Product/Edit.cshtml", product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, [FromForm] List<IFormFile> files)
        {
            if (id != product.Id) return NotFound();

            // Server-side file validation
            var allowed = new[] { "image/png", "image/jpeg", "image/jpg", "image/gif", "video/mp4", "video/webm" };
            const long maxBytes = 25 * 1024 * 1024; // 25 MB

            if (files != null)
            {
                foreach (var f in files)
                {
                    if (f == null) continue;
                    if (f.Length == 0) { ModelState.AddModelError("files", $"File {f.FileName} is empty."); break; }
                    if (f.Length > maxBytes) { ModelState.AddModelError("files", $"File {f.FileName} exceeds {maxBytes} bytes."); break; }
                    if (!allowed.Contains(f.ContentType)) { ModelState.AddModelError("files", $"File {f.FileName} has unsupported type {f.ContentType}."); break; }
                }
            }

            if (!ModelState.IsValid)
            {
                product.Media = await _context.ProductMedias.Where(m => m.ProductId == product.Id).ToListAsync();
                return View(product);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tracked = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
                if (tracked == null) return NotFound();

                tracked.Name = product.Name;
                tracked.Price = product.Price;
                tracked.Description = product.Description;

                _context.Products.Update(tracked);

                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        if (file == null || file.Length == 0) continue;

                        await using var ms = new MemoryStream();
                        await file.CopyToAsync(ms);

                        var media = new ProductMedia
                        {
                            ProductId = tracked.Id,
                            FileData = ms.ToArray(),
                            ContentType = file.ContentType,
                            Type = file.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase) ? MediaType.Image : MediaType.Video,
                            //CreatedAt = DateTime.UtcNow
                        };

                        await _context.ProductMedias.AddAsync(media);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the exception (replace with your logger)
                Console.Error.WriteLine(ex);
                ModelState.AddModelError("", "An error occurred while saving. " + ex.Message);
                product.Media = await _context.ProductMedias.Where(m => m.ProductId == product.Id).ToListAsync();
                return View(product);
            }
        }
        // Optional helper endpoint used by your JS to delete media
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            var media = await _context.ProductMedias.FindAsync(id);
            if (media == null)
                return Json(new { success = false });

            _context.ProductMedias.Remove(media);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Media)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product != null)
            {
                // Delete associated media
                _context.ProductMedias.RemoveRange(product.Media);

                // Delete product
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Review(int pid, int? cid, string content)
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var commentToEdit = await _context.Comments.FirstOrDefaultAsync(c => c.Id == cid);
            if (commentToEdit != null)
            {
                if (commentToEdit.UserId != userId)
                    return Forbid();
                // prevent editing someone else’s comment
                commentToEdit.Content = content;
                commentToEdit.CreatedAt = DateTime.UtcNow;
                _context.Comments.Update(commentToEdit);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newComment = new Comment
                {
                    ProductId = pid,
                    UserId = user?.Id,
                    UName = user?.UserName,
                    Content = content,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Comments.Add(newComment);
                await _context.SaveChangesAsync();
            }


            return RedirectToAction("Details", new { id = pid });
            //return RedirectToAction($"Details/{id}");
        }
        public async Task<IActionResult> DeleteComment(int id, int PId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (id == null)
            {
                return NotFound();
            }
            else
            {
                var commentToDelete = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
                if (commentToDelete != null)
                {
                    if (commentToDelete.UserId != userId)
                    {
                        return Forbid();
                    }
                    _context.Comments.Remove(commentToDelete);
                    await _context.SaveChangesAsync();
                }

            }
            return RedirectToAction("Details", new { id = PId });
            //return RedirectToAction($"Details/{PId}");

        }
        [HttpGet]
        public async Task<IActionResult> Autocomplete(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(new string[] { });

            // Fuzzy search using ILIKE (case-insensitive, partial match)
            var suggestions = await _context.Products
                .Where(p => EF.Functions.ILike(p.Name, $"%{term}%"))
                .Select(p => p.Name)
                .Distinct()
                .Take(5)
                .ToListAsync();

            return Json(suggestions);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return View(new List<Product>());

            var products = await _context.Products
                .Where(p => EF.Functions.ILike(p.Name, $"%{query}%") ||
                            EF.Functions.ILike(p.Description, $"%{query}%"))
                .Include(p => p.Media)
                .ToListAsync();

            return View(products);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id, Items = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                TempData["Message"] = "Product already in cart!";
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = productId, Quantity = 1 });
                TempData["Message"] = "Product added to cart!";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Product");
        }

        // View cart
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
        {
            var item = await _context.CartItems
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item != null)
            {
                item.Quantity = quantity;
                await _context.SaveChangesAsync();

                var cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.Id == item.CartId);

                var grandTotal = cart.Items.Sum(ci => ci.Quantity * ci.Product.Price);

                return Json(new
                {
                    success = true,
                    itemTotal = item.Quantity * item.Product.Price,
                    grandTotal = grandTotal,
                    cartCount = cart.Items.Sum(ci => ci.Quantity)
                });
            }

            return Json(new { success = false });
        }
        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var user = await _userManager.GetUserAsync(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            ViewBag.CartCount = cart?.Items.Sum(ci => ci.Quantity) ?? 0;

            if (!User.Identity.IsAuthenticated)
            { ViewBag.CartCount = 0; }
            if (cart == null)
            {
                cart = new Cart { Items = new List<CartItem>() }; // return empty cart instead of null
            }


            return View(cart);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int itemId)
        {
            var item = await _context.CartItems.FirstOrDefaultAsync(i => i.Id == itemId);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }

            // Redirect back to Cart page so the view reloads with updated data
            return RedirectToAction("Cart", "Product");
        }
        [Authorize]
        public async Task<IActionResult> BuyNow(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var model = new Order
            {
                ProductId = product.Id,
                Product = product,
                Quantity = 1,
                TotalPrice = product.Price,
                Charges = product.Price * 0.015m,
                GrandTotal = product.Price + (product.Price * 0.015m)
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyNow(Order order)
        {
            var product = await _context.Products.FindAsync(order.ProductId);
            if (product == null) return NotFound();

            // Force EF to treat this as a new entity
            order.Id = 0;

            // Recalculate totals
            order.TotalPrice = product.Price * order.Quantity;
            order.Charges = order.TotalPrice * 0.015m;
            order.GrandTotal = order.TotalPrice + order.Charges;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Redirect based on payment method
            return order.PaymentMethod switch
            {
                "Cash on Delivery" => RedirectToAction("Success", new { orderId = order.Id }),
                "JazzCash" => RedirectToAction("JazzCashPayment", new { orderId = order.Id }),
                "EasyPaisa" => RedirectToAction("EasyPaisaPayment", new { orderId = order.Id }),
                _ => RedirectToAction("Cancel", new { orderId = order.Id })
            };
        }
        public async Task<IActionResult> JazzCashPayment(int orderId)
        {
            var order = await _context.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            // Here you call JazzCash API with order.GrandTotal
            // Redirect user to JazzCash hosted payment page

            return View(order);
        }
        public async Task<IActionResult> EasyPaisaPayment(int orderId)
        {
            var order = await _context.Orders.Include(o => o.Product).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null) return NotFound();

            // Here you call EasyPaisa API with order.GrandTotal
            // Redirect user to EasyPaisa hosted payment page

            return View(order);
        }

        public async Task<IActionResult> Success(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.IsPaid = order.PaymentMethod == "Cash on Delivery" ? false : true;
                await _context.SaveChangesAsync();
            }
            return View(order);
        }

        public async Task<IActionResult> Cancel(int orderId)
        {
            return View();
        }
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cartItems = await _context.CartItems.Include(c => c.Product).ToListAsync();

            var totalPrice = cartItems.Sum(c => c.Product.Price * c.Quantity);
            var charges = totalPrice * 0.015m;
            var grandTotal = totalPrice + charges;

            var checkout = new Checkout
            {
                TotalPrice = totalPrice,
                Charges = charges,
                GrandTotal = grandTotal
            };

            foreach (var ci in cartItems)
            {
                checkout.Items.Add(new CheckoutItem
                {
                    ProductId = ci.ProductId,
                    Product = ci.Product,
                    Quantity = ci.Quantity,
                    Subtotal = ci.Product.Price * ci.Quantity
                });
            }

            return View(checkout);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(Checkout checkout)
        {
            if (!ModelState.IsValid)
                return View(checkout);

            // Recalculate totals
            var cartItems = await _context.CartItems.Include(c => c.Product).ToListAsync();
            checkout.TotalPrice = cartItems.Sum(c => c.Product.Price * c.Quantity);
            checkout.Charges = checkout.TotalPrice * 0.015m;
            checkout.GrandTotal = checkout.TotalPrice + checkout.Charges;

            _context.Checkouts.Add(checkout);
            await _context.SaveChangesAsync();

            foreach (var ci in cartItems)
            {
                var item = new CheckoutItem
                {
                    CheckoutId = checkout.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Subtotal = ci.Product.Price * ci.Quantity
                };
                _context.CheckoutItems.Add(item);
            }
            await _context.SaveChangesAsync();

            return checkout.PaymentMethod switch
            {
                "Cash on Delivery" => RedirectToAction("Success", new { checkoutId = checkout.Id }),
                "JazzCash" => RedirectToAction("JazzCashPayment", new { checkoutId = checkout.Id }),
                "EasyPaisa" => RedirectToAction("EasyPaisaPayment", new { checkoutId = checkout.Id }),
                _ => RedirectToAction("Cancel", new { checkoutId = checkout.Id })
            };
        }

    }

}


