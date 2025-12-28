using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ONT_3rdyear_Project.Data;
using ONT_3rdyear_Project.Models;
using ONT_3rdyear_Project.StaticHelper;
using ONT_3rdyear_Project.ViewModels;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ONT_3rdyear_Project.Controllers
{
    [Authorize(Roles = "ConsumableManager,Admin")]
    public class ConsumableController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        public ConsumableController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        //Dashboard for Consumables Management
        public async Task<IActionResult> Dashboard()
        {
           var orders = await _context.Orders.Include(o => o.Ward)
           .Include(o => o.ConsumableOrders)
           .ThenInclude(o => o.Consumable).ToListAsync();
            ViewBag.FinalOrders = orders.Where(s => s.Status == "Delivered" || s.Status == "Rejected"  && s.OrderDate >= DateTime.Now.AddDays(-7)).ToList();
            ViewBag.ProcessOrders = orders.Where(s => s.Status == "Pending" || s.Status == "pending"|| s.Status == "Approved").ToList();
            ViewBag.LowStockItems = _context.WardConsumables.Include(c => c.Ward).Include(c => c.Consumable).Where(c => c.Quantity <= 50 ).Select(c => new WardConsumableViewModel{ 
                WardName = c.Ward.Name,
                WardID = c.WardID,
                ConsumableID = c != null ? c.ConsumableID : 0,
                ConsumableName = c != null ? c.Consumable.Name : "No Consumable",
                Category = c != null ? c.Consumable.Category : "-",
                 SystemQuantity = c.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == c.ConsumableID && sti.StockTake.WardID == c.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.SystemQuantity)
                      .FirstOrDefault(),
                    QuantityCounted = c.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == c.ConsumableID && sti.StockTake.WardID == c.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.QuantityCounted)
                      .FirstOrDefault()

                }).ToList();
            
            //return to the Dashboardy
            ViewBag.TotalConsumables = await _context.Consumables
                .Where(c => c.IsDeleted == false)
                .CountAsync();
             ViewBag.PendingOrders = await _context.Orders.Where(s => s.Status == "Pending" || s.Status == "pending")
                .CountAsync();
            ViewBag.CriticalStock = await _context.WardConsumables
                .Where(c => c.Quantity <= 10 )
                .CountAsync();
            ViewBag.LowStock = await _context.WardConsumables
                .Where(c => c.Quantity == 11  || c.Quantity <= 50)
                .CountAsync();
            return View();
        }
        public async Task<IActionResult> ApprovedOrders()
        {
            var orders = await _context.Orders.Include(o => o.Ward)
            .Where(s => s.Status == "Pending" || s.Status == "pending" || s.Status == "Approved")
            .Include(o => o.User)
            .Include(o => o.ConsumableOrders)
            .ThenInclude(o => o.Consumable).ToListAsync();
            
            return View(orders);
        }
        public async Task<IActionResult> OrderHistory()
        {
            var orders = await _context.Orders.Include(o => o.Ward)
            .Where(s => s.Status == "Delivered" || s.Status == "Rejected")
            .Include(o => o.Supplier)
            .Include(o => o.User)
            .Include(o => o.ConsumableOrders)
            .ThenInclude(o => o.Consumable).ToListAsync();

            return View(orders);
        }
        public async Task<IActionResult> FullInventory()
        {
            var orders = await _context.Orders.Include(o => o.Ward)           
            .Include(o => o.Supplier)
            .Include(o => o.User)
            .Include(o => o.ConsumableOrders)
            .ThenInclude(o => o.Consumable)
            .OrderByDescending(o => o.Status == "Pending")
            .ToListAsync();

            return View(orders);
        }
         // Add new Ward on the System
        [HttpGet]

        public IActionResult AddWard(bool partial = false)
        {
            if (partial)
            {
                return PartialView("AddWard", new Ward());
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWard(Ward wardView)
        { 
            wardView.IsActive = true;
         
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, See your system administrator.");
                return View(wardView);
            }
            _context.Wards.Add(wardView);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
            
        }
        public async Task<IActionResult> Wards(string searchString)
        {
            var wards = _context.Wards.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                wards = wards.Where(w => w.Name.Contains(searchString));
            }
             ViewData["CurrentFilter"] = searchString;
            return View(await wards.ToListAsync());
        }
        public async Task<IActionResult> Index(string searchString)
        {
            var consumables = _context.Consumables.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                consumables = consumables.Where(c => c.Name.Contains(searchString) || c.Category.Contains(searchString));
            }
             ViewData["CurrentFilter"] = searchString;
            return View(await consumables.ToListAsync());
        }
        //Add New Consumable controller
        [HttpGet]
        [Route("Consumable/NewConsumable")]
        public async Task<IActionResult> NewConsumable(string? name, bool partial = false)
        {
            var consumable = await _context.Consumables.FindAsync(name);           
            ViewBag.CategoryList = ConsumableCategoryHelper.GetCategories();              

            return PartialView("NewConsumable");         

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewConsumable(string name, Consumable model)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, See your system administrator.");
                return View(model);
            }
            bool nameExists = await _context.Consumables
          .AnyAsync(c => c.Name.ToLower() == model.Name.ToLower());

            if (nameExists)
            {
                ModelState.AddModelError("isAvailable", "Consumable already exists.");
            }         

           
            _context.Consumables.Add(model);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Consumable");                 
               
           
        }
        //Edit Consumable controller
        [HttpGet]
        [Route("Consumable/EditConsumable/{id}")]
        public async Task<IActionResult> EditConsumable(int id)
        {
            var consumable = await _context.Consumables.FirstOrDefaultAsync(c => c.ConsumableId == id);
            if (consumable == null)
            {
                return NotFound();
            }
            ViewBag.CategoryList = ConsumableCategoryHelper.GetCategories();
            return PartialView(consumable);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConsumable(int id , Consumable model)
        {
            if (id != model.ConsumableId)
                return NotFound();

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Consumables.Any(p => p.ConsumableId == id))
                    return NotFound();
                else
                    throw;
            }
            ViewBag.CategoryList = ConsumableCategoryHelper.GetCategories();
            
            return RedirectToAction("Index", "Consumable");
        }
        //Soft Delete Consumable controller
        public async Task<IActionResult> DeleteConsumable(int id)
        {
            var item = await _context.Consumables.FindAsync(id);
            if (item != null)
            {
                item.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Consumables");
        }
        //View Consumables inside a Ward
        public async Task<IActionResult> WardConsumables()
        {
            var data = await _context.WardConsumables.Where(wc => wc.Consumable.IsDeleted == false )
                  .OrderBy(wc => wc.Ward.Name)
                  .Include(wc => wc.Ward)
                  .Include(wc => wc.Consumable)
                  .Select(wc => new WardConsumableViewModel
                  {
                      WardName = wc.Ward.Name,
                      WardID = wc.WardID,
                      ConsumableID = wc != null ? wc.ConsumableID : 0,
                      ConsumableName = wc != null ? wc.Consumable.Name : "No Consumable",
                      Category = wc != null ? wc.Consumable.Category : "-",
                      SystemQuantity = wc != null ? wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.SystemQuantity)
                      .FirstOrDefault ():0,
                      QuantityCounted = wc != null ? wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.QuantityCounted)
                      .FirstOrDefault ():0,
                       Discrepancy = wc != null ? wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.Discrepancy)
                      .FirstOrDefault ():0,
                      LastUpdated = wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.StockTake.StockTakeDate)
                      .FirstOrDefault()

                  }).ToListAsync();

            return View(data);
        }
        // Search Consumables in a Ward
        [HttpGet]
        public async Task<IActionResult> SearchConsumablesInWard(int wardId, string searchString)
        {   
            var consumables = _context.WardConsumables
                .Where(wc => wc.Consumable.IsDeleted == false);

            if (!string.IsNullOrEmpty(searchString))
            {
                consumables = consumables.Where(wc => wc.Consumable.Name.Contains(searchString) || wc.Consumable.Category.Contains(searchString));
            }

            var result = await consumables
                .Include(wc => wc.Ward)
                .Include(wc => wc.Consumable)
                .Select(wc => new WardConsumableViewModel
                {
                    WardID = wardId,
                    WardName = wc.Ward.Name,
                    ConsumableID = wc.ConsumableID,
                    ConsumableName = wc.Consumable.Name,
                    Category = wc.Consumable.Category,
                    SystemQuantity = wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.SystemQuantity)
                      .FirstOrDefault(),
                    QuantityCounted = wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.QuantityCounted)
                      .FirstOrDefault(),
                    Discrepancy = wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.Discrepancy)
                      .FirstOrDefault(),
                    LastUpdated = wc.Consumable.StockTakeItems
                      .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                      .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                      .Select(sti => sti.StockTake.StockTakeDate)
                      .FirstOrDefault()
                })
                .ToListAsync();

            return View("WardConsumables",result);
        }
        //Retrieve data for stock take as Json
        public async Task<IActionResult> GetConsumablesByWard(int wardId)
        {

            var consumables = await _context.WardConsumables
                .Where(wc => wc.WardID == wardId)
                 .Include(wc => wc.Ward)  // Include StockTakeItems if needed
                .Include(wc => wc.Consumable)
                 .ThenInclude(c => c.StockTakeItems)
                    .ThenInclude(sti => sti.StockTake)
                .Select(wc => new WardConsumableViewModel
                {
                    WardID = wardId,
                    WardName = wc.Ward.Name,
                    ConsumableID = wc.ConsumableID,
                    ConsumableName = wc.Consumable.Name,
                    Category = wc.Consumable.Category,                  
                    SystemQuantity = wc.Consumable.StockQuantity,
                    LastUpdated = wc.Consumable.StockTakeItems
                    .Where(sti => sti.ConsumableID == wc.ConsumableID && sti.StockTake.WardID == wc.WardID)
                    .OrderByDescending(sti => sti.StockTake.StockTakeDate)
                    .Select(sti => sti.StockTake.StockTakeDate)
                    .FirstOrDefault()

                })
                .ToListAsync();

            return Json(consumables);

        }
        //Add Consumables to a ward
        [HttpGet]
        public IActionResult AddConsumableToWard(int wardId)
        {
            // Set ViewBag lists
            ViewBag.ConsumableList = new SelectList(_context.Consumables.ToList(), "ConsumableId", "Name");

           var ward = _context.Wards.FirstOrDefault(w => w.WardID == wardId);



            return PartialView("AddConsumableToWard");
        }
        //This post method handles the addition of a consumable to a specific ward and also do Stock Take.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsumableToWard(int WardId, int ConsumableID, int Quantity)
        {
            if (WardId == 0 || ConsumableID == 0)
            {
                ViewBag.WardList = new SelectList(_context.Wards.ToList(), "WardID", "Name");
                ViewBag.ConsumableList = new SelectList(_context.Consumables.ToList(), "ConsumableId", "Name");
                return View();
            }
            var user = await _userManager.GetUserAsync(User);
            var consumable = await _context.WardConsumables
             .Include(wc => wc.Ward)
             .Include(wc => wc.Consumable)
             .FirstOrDefaultAsync(wc => wc.WardID == WardId && wc.ConsumableID == ConsumableID);
            if (consumable != null)
            {
                ModelState.AddModelError("", "Consumable already exists in this ward.");
                ViewBag.WardList = new SelectList(_context.Wards.ToList(), "WardID", "Name");
                ViewBag.ConsumableList = new SelectList(_context.Consumables.ToList(), "ConsumableId", "Name");
                return View();
            }
            else
            {
                var stocktake = new StockTake
                {
                    WardID = WardId,
                    StockTakeDate = DateTime.Now,
                    TakenBy = user.Id,
                    TakenByUser = user

                };
                _context.StockTakes.Add(stocktake);
                await _context.SaveChangesAsync();

                var stockTakeItem = new StockTakeItem
                {
                    StockTakeID = stocktake.StockTakeID,
                    ConsumableID = ConsumableID,
                    QuantityCounted = Quantity,
                    SystemQuantity = _context.Consumables.Where(c => c.ConsumableId == ConsumableID).Select(c => c.StockQuantity).FirstOrDefault()              
                };
               
                var wardConsumable = new WardConsumable
                {
                    ConsumableID = ConsumableID,
                    WardID = WardId,
                    Quantity = Quantity

                };

                _context.StockTakeItems.Add(stockTakeItem);
                _context.WardConsumables.Add(wardConsumable);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("WardConsumables", "Consumable");
        }
        //Add Consumable direct from the ward masterList
        public async Task<IActionResult> AddConsumableDirect(int wardId)
        {
           var ward = await _context.Wards.FindAsync(wardId);
           var consumablesInWard = await _context.WardConsumables
                .Where(wc => wc.WardID == wardId)
                .Select(wc => wc.ConsumableID)
                .ToListAsync();
             if (ward == null)
            {
                // Ward not found — return 404 or handle gracefully
                return NotFound($"Ward with ID {wardId} does not exist.");
            }
           var model = new WardConsumable
            {
                WardID = wardId               
            };
            ViewBag.WardName = ward.Name;
           var consumableList = _context.Consumables
                .Where(c => !consumablesInWard.Contains(c.ConsumableId))
                .ToList();

            ViewBag.ConsumableList = new SelectList(consumableList, "ConsumableId", "Name");
           
            return PartialView("AddConsumableDirect",model);
        }
       
        [HttpPost]
        public async Task<IActionResult> AddConsumableDirect(WardConsumable model)
        {
            if (ModelState.IsValid) 
            {
                var user = await _userManager.GetUserAsync(User);
                
                var existingConsumable = await _context.WardConsumables.Include(wc => wc.Consumable)
                    .FirstOrDefaultAsync(wc => wc.WardID == model.WardID && wc.ConsumableID == model.ConsumableID);
                
                if (existingConsumable != null)
                {
                    TempData["Error"] = "This consumable already exists in the selected ward.";
                     ModelState.AddModelError("", "This consumable already exists in the selected ward.");
                    ViewBag.WardName = (await _context.Wards.FindAsync(model.WardID))?.Name;
                    ViewBag.ConsumableList = new SelectList(_context.Consumables.ToList(), "ConsumableId", "Name");
                    return PartialView("AddConsumableDirect", model);
                }
                else
                {
                    var stocktake = new StockTake
                    {
                        WardID = model.WardID,
                        StockTakeDate = DateTime.Now,
                        TakenBy = user.Id,
                        TakenByUser = user
                    };
                    _context.StockTakes.Add(stocktake);
                    await _context.SaveChangesAsync();

                    var stockTakeItem = new StockTakeItem
                    {
                        StockTakeID = stocktake.StockTakeID,
                        ConsumableID = model.ConsumableID,
                        QuantityCounted = 0,
                        SystemQuantity = model.Quantity,
                        Discrepancy = 0 - model.Quantity
                    };
                     
                     _context.StockTakeItems.Add(stockTakeItem);
                    _context.WardConsumables.Add(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("WardConsumables");
                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill in all required fields.");
            }
            return RedirectToAction("WardConsumables");
        }
        //Creating a New Stock Take request for a Ward
        [HttpGet]
        public async Task<IActionResult> StockTake(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            ViewBag.Wards = new SelectList(_context.Wards, "WardID", "Name");

            var model = new StockTakeViewModel
            {
                TakenByID = user.Id,

                Items = new List<StockTakeItemEntry>() // empty list initially
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StockTake(StockTakeViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            ViewBag.Wards = new SelectList(_context.Wards, "WardID", "Name");
            var wardConsumables = await _context.WardConsumables
                .Where(wc => wc.WardID == model.WardID)
                .Include(wc => wc.Consumable)
                .ToListAsync();
            var stockTake = new StockTake
            {
                WardID = model.WardID,
                StockTakeDate = DateTime.Now,
                TakenBy = user.Id,
                TakenByUser = user,

                StockTakes = model.Items.Select(i => 
                {
                    var systemQty = wardConsumables
                    .Where(wc => wc.ConsumableID == i.ConsumableID)
                    .Select(wc => wc.Consumable.StockQuantity)
                    .FirstOrDefault();

                return new StockTakeItem
                {
                    ConsumableID = i.ConsumableID,
                    QuantityCounted = i.CountedQuantity,
                    SystemQuantity = systemQty,
                    Discrepancy = i.CountedQuantity - systemQty
                };
                }).ToList()
            };
          
            if (!ModelState.IsValid)
            {
                var Wards = await _context.Wards
                .Select(w => new SelectListItem { Value = w.WardID.ToString(), Text = w.Name })
                .ToListAsync();
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine(error.ErrorMessage); // or use logging
                }

                return View(model);
            }
            _context.Add(stockTake);
            //Update Available Quantity
            foreach (var item in model.Items)
            {
                var wardConsumable = await _context.WardConsumables.FirstOrDefaultAsync(wc => wc.WardID == model.WardID && wc.ConsumableID == item.ConsumableID);


                if (wardConsumable != null && item.CountedQuantity != 0)
                {
                    wardConsumable.Quantity = item.CountedQuantity;
                    _context.Update(wardConsumable);
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
        //Create Stock request controller
        [HttpGet]
        public IActionResult OrderConsumables(int id)
        {
            var viewModel = new Order
            {
                ConsumableOrders = new List<ConsumableOrder> { new()}
            }; 
            ViewBag.Wards = new SelectList(_context.Wards, "WardID", "Name");
            return View(viewModel);
        }
        //This controller allows users to request consumables for a specific ward using Ajax.
        [HttpGet]
        public IActionResult RequestConsumablesByWard(int WardId)
        {
            // Filter consumables by the wardID
            var WardConsumables = _context.Consumables
                .Where(c => c.WardConsumables.Any(ws => ws.WardID == WardId))
                .Select(c => new
                {
                    c.ConsumableId,
                    c.Name
                })
                .ToList();

            return Json(WardConsumables);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderConsumables(Order model, int i)
        {
            
            if (!ModelState.IsValid)
            {
                ViewBag.Consumables = new SelectList(_context.Consumables, "ConsumableId", "Name");
                return View(model);
            }
            var user = await _userManager.GetUserAsync(User);
            var s = await _context.Suppliers.FindAsync(1);
            if (user == null) return Unauthorized();
            model.OrderedBy = user.Id;
            //temp
            model.SupplierID = s.SupplierId;
            model.OrderDate = DateTime.Now;
            model.Status = "Pending";          
            model.ConsumableOrders
            .Where(i => i.ConsumableId > 0 && i.QuantityRequested > 0)
            .Select(i =>  new ConsumableOrder 
            {
                
                ConsumableId = i.ConsumableId,
                QuantityRequested = i.QuantityRequested,
                Reason = i.Reason 
            }).ToList();
            model.OrderQuantity = model.ConsumableOrders.Sum(i => i.QuantityRequested);
            ViewBag.Wards = new SelectList(_context.Wards, "WardID", "Name");
            ViewBag.Consumables = new SelectList(_context.Consumables.Include(c => c.WardConsumables).Where(c => c.WardConsumables.Any(ws => ws.WardID == i)), "ConsumableId", "Name");
            _context.Orders.Add(model);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Order of Consumables Successful";
            return RedirectToAction("Dashboard");
        }
        //Get a list of Order Requests
        [HttpGet]
        public async Task<IActionResult> ListOfOrders()
        {
            var requests = await _context.Orders.Include(r => r.ConsumableOrders).ThenInclude(r =>r.Consumable).Include(r => r.Ward).Where(r => r.Status == "Pending" || r.Status == "New" ).ToListAsync();
            return View(requests);
        }
        //Approve Order Request
         [HttpPost]
        public async Task<IActionResult> ApproveRequest(int requestId,int type, IEnumerable<ConsumableOrder> items)
        {
            // Load the request with tracked items
              var user = await _userManager.GetUserAsync(User);
            var order = await _context.Orders
                .Include(r => r.ConsumableOrders)
                .FirstOrDefaultAsync(r => r.OrderID == requestId);
        
            if (order == null) return NotFound();

            // Update approved quantities only for posted items
            foreach (var item in items)
            {
                var consumableOrder = order.ConsumableOrders.FirstOrDefault(ri => ri.ConsumableId == item.ConsumableId && ri.OrderId == item.OrderId);
                if (consumableOrder != null)
                {
                
                    // Update request status
                    if (type == 1)
                    {
                        order.Status = "Rejected";
                        order.OrderDate = DateTime.UtcNow;
                        consumableOrder.QuantityApproved = 0;
                    }
                    else if(type == 2)
                    {  
                       var D = _context.Deliveries
                        .Include(d => d.DeliveryItems)
                            .ThenInclude(di => di.Consumable)
                        .FirstOrDefault(d => d.OrderID == order.OrderID);

                    if (D != null)
                    {
                        D.RecievedBy = user.Id;
                        D.RecievedByUser = user;

                        _context.Deliveries.Update(D);
                        await _context.SaveChangesAsync();
                    }
                       
                        order.Status = "Delivered";
                        order.OrderDate = DateTime.UtcNow;
                        var sysQuantity = _context.StockTakeItems.FirstOrDefault(s => s.ConsumableID == item.ConsumableId);
                        if (sysQuantity != null)
                        {
                            sysQuantity.QuantityCounted = consumableOrder.QuantityApproved;
                            _context.StockTakeItems.Update(sysQuantity);
                        }
                        TempData["Message"] = "Consumables Delivered Successful";
                        await _context.SaveChangesAsync();
                        return RedirectToAction("Dashboard");

                    }
                    else
                    {
                        var Delivery = new Delivery
                        {
                            OrderID = order.OrderID,
                            DeliveryDate = DateTime.UtcNow,                           
                            DeliveredBy = user.Id,
                            DeliveredByUser = user,
                            DeliveryItems = new List<DeliveryItem>
                            {
                                new DeliveryItem
                                {
                                    ConsumableID = item.ConsumableId,
                                    QuantityDelivered = item.QuantityApproved
                                }
                            }
                        };
                        _context.Deliveries.Add(Delivery);                      
                        order.Status = "Approved";
                         TempData["Message"] = "Consumables Approved ";
                        order.OrderDate = DateTime.UtcNow;
                        consumableOrder.QuantityApproved = item.QuantityApproved;
                        
                    }

                }
            } 
            await _context.SaveChangesAsync();
            return RedirectToAction("ListOfOrders");
        }
        public async Task<IActionResult> GenerateReports()
    {
        var orders = await _context.Orders.Include(o => o.Ward)
            .Where(s => s.Status == "Delivered" || s.Status == "Rejected")
            .Include(o => o.Supplier)
            .Include(o => o.User)
            .Include(o => o.ConsumableOrders)
            .ThenInclude(o => o.Consumable).ToListAsync();
        
        QuestPDF.Settings.License = LicenseType.Community;
        
        var pdf = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                
                // Professional Header with Logo
                page.Header()
                    .BorderBottom(2)
                    .BorderColor("#2C3E50")
                    .PaddingBottom(10)
                    .Row(row =>
                    {
                        // Logo section (left side)
                        
                        
                        // Title section (right side)
                        row.RelativeItem()
                            .PaddingLeft(15)
                            .Column(column =>
                            {
                                column.Item().Text("Consumables Order Report")
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor("#2C3E50");
                                
                                column.Item().PaddingTop(5).Text($"Generated: {DateTime.Now:dd MMMM yyyy}")
                                    .FontSize(10)
                                    .FontColor("#7F8C8D");
                                
                                column.Item().Text($"Total Orders: {orders.Count}")
                                    .FontSize(10)
                                    .FontColor("#7F8C8D");
                            });
                    });
                
                // Content
                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                       
                        
                        // Table Section
                        column.Item().Table(table =>
                        {
                            // Define columns with better proportions
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);  // Order ID
                                 columns.RelativeColumn(2);   // order by
                                columns.RelativeColumn(1.6f);   // Ward
                                columns.RelativeColumn(1.3f); // Status
                                columns.RelativeColumn(1.5f); // Order Date
                                columns.RelativeColumn(1.1f);   // Total Quantity
                            });

                            // Professional Table Header
                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).Text("Order ID").Bold();
                                header.Cell().Element(HeaderStyle).Text("Order By").Bold();
                                header.Cell().Element(HeaderStyle).Text("Ward").Bold();
                                header.Cell().Element(HeaderStyle).Text("Status").Bold();
                                header.Cell().Element(HeaderStyle).Text("Order Date").Bold();
                                header.Cell().Element(HeaderStyle).Text("Quantity").Bold();
                            });

                            // Table Rows with alternating colors
                            var index = 0;
                            foreach (var order in orders)
                            {
                                var isEven = index % 2 == 0;
                                
                                table.Cell().Element(c => RowStyle(c, isEven))
                                    .Text(order.OrderID.ToString());
                                table.Cell().Element(c => RowStyle(c, isEven))
                                    .Text(order.User.FullName.ToString());
                                
                                table.Cell().Element(c => RowStyle(c, isEven))
                                    .Text(order.Ward.Name);
                                
                                table.Cell().Element(c => RowStyle(c, isEven))
                                    .Text(order.Status)
                                    .FontColor(order.Status == "Delivered" ? "#27AE60" : "#E74C3C")
                                    .Bold();
                                
                                table.Cell().Element(c => RowStyle(c, isEven))
                                    .Text(order.OrderDate.ToString("MMM dd, yyyy"));
                                
                                table.Cell().Element(c => RowStyle(c, isEven))
                                    .AlignCenter()
                                    .Text(order.OrderQuantity.ToString());
                                
                                index++;
                            }
                        });
                    });
                
                // Professional Footer
                page.Footer()
                    .BorderTop(1)
                    .BorderColor("#CCCCCC")
                    .PaddingTop(10)
                    .AlignCenter()
                    .DefaultTextStyle(x => x.FontSize(9).FontColor("#7F8C8D"))
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });
        
        // Helper function for header styling
        IContainer HeaderStyle(IContainer container)
        {
            return container
                .Background("#34495E")
                .Padding(8)
                .AlignMiddle()
                .DefaultTextStyle(x => x.FontColor("#FFFFFF").FontSize(11));
        }
        
        // Helper function for row styling
        IContainer RowStyle(IContainer container, bool isEven)
        {
            return container
                .Background(isEven ? "#FFFFFF" : "#F8F9FA")
                .Padding(8)
                .BorderBottom(0)
                .BorderColor("#E8E8E8")
                .AlignMiddle()
                .DefaultTextStyle(x => x.FontSize(10).FontColor("#2C3E50"));
        }
        
        var pdfBytes = pdf.GeneratePdf();
        
        if (pdfBytes == null || pdfBytes.Length == 0)
        {
            TempData["Error"] = "No reports generated.";
            return RedirectToAction("Dashboard");
        }
        
        TempData["Message"] = "Report generated successfully.";
        return File(pdfBytes, "application/pdf", "Order_History_Report.pdf");
    }
    }
}
