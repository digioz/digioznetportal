using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using digioz.Portal.Bll.Interfaces;
using digioz.Portal.Bo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace digioz.Portal.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class MenuManagerController : Controller
    {
        private readonly ILogger<LinkManagerController> _logger;
        private readonly ILogic<Menu> _menuLogic;
        private readonly ILogic<AspNetUser> _userLogic;
        private readonly ILogic<Zone> _zoneLogic;

        public MenuManagerController(
            ILogger<LinkManagerController> logger,
            ILogic<Menu> menuLogic,
            ILogic<AspNetUser> userLogic,
            ILogic<Zone> zoneLogic
        )
        {
            _logger = logger;
            _menuLogic = menuLogic;
            _userLogic = userLogic;
            _zoneLogic = zoneLogic;
        }

        // GET: /Admin/MenuManager/
        public ActionResult Index()
        {
            var menus = _menuLogic.GetAll().OrderBy(m=>m.SortOrder);

            return View(menus.ToList());
        }

        // GET: /Admin/MenuManager/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Menu menu = _menuLogic.Get(id);

            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // GET: /Admin/MenuManager/Create
        public ActionResult Create()
        {
            // Get list of Users
            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName");

            // Get Menu Locations
            var locations = _zoneLogic.GetGeneric(x => x.ZoneType == "Menu").ToList();
            List<SelectListItem> locationListItems = new List<SelectListItem>();

            foreach (var location in locations)
            {
                locationListItems.Add(new SelectListItem() { Text = location.Name, Value = location.Name });
            }

            ViewBag.Location = locationListItems;
            
            return View();
        }

        // POST: /Admin/MenuManager/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind("Id,UserId,Name,Location,Controller,Action,Url,Target,Visible,Timestamp")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                if (menu.UserId == null)
                {
                    menu.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                }

                menu.Timestamp = DateTime.Now;
                int maxSortOrder = _menuLogic.GetGeneric(m => m.Location == menu.Location).Max(m => m.SortOrder); //max sort order for this location
                menu.SortOrder = maxSortOrder + 1;

                //bump all menus above
                var menusHigherSort = _menuLogic.GetGeneric(m => m.SortOrder >= menu.SortOrder);

                foreach (Menu menuHigherSort in menusHigherSort)
                {
                    menuHigherSort.SortOrder++;
                }

                _menuLogic.Add(menu);

                return RedirectToAction("Index");
            }

            // Get list of users
            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName", menu.UserId);

            // Get Menu Locations
            var locations = _zoneLogic.GetGeneric(x => x.ZoneType == "Menu").ToList();
            List<SelectListItem> locationListItems = new List<SelectListItem>();

            foreach (var location in locations)
            {
                locationListItems.Add(new SelectListItem() { Text = location.Name, Value = location.Name });
            }

            ViewBag.Location = locationListItems;

            return View(menu);
        }

        // GET: /Admin/MenuManager/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Menu menu = _menuLogic.Get(id);

            if (menu == null)
            {
                return NotFound();
            }

            // Get list of users
            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName", menu.UserId);

            // Get Menu Locations
            var locations = _zoneLogic.GetGeneric(x => x.ZoneType == "Menu").ToList();
            List<SelectListItem> locationListItems = new List<SelectListItem>();

            foreach (var location in locations)
            {
                locationListItems.Add(new SelectListItem() { Text = location.Name, Value = location.Name });
            }

            ViewBag.Location = locationListItems;

            return View(menu);
        }

        // POST: /Admin/MenuManager/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind("Id,UserId,Name,Location,Controller,Action,Url,Target,Visible,Timestamp,SortOrder")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                menu.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                menu.Timestamp = DateTime.Now;

                _menuLogic.Edit(menu);

                return RedirectToAction("Index");
            }

            // Get list of users
            ViewBag.UserID = new SelectList(_userLogic.GetAll(), "Id", "UserName", menu.UserId);

            // Get Menu Locations
            var locations = _zoneLogic.GetGeneric(x => x.ZoneType == "Menu").ToList();
            List<SelectListItem> locationListItems = new List<SelectListItem>();

            foreach (var location in locations)
            {
                locationListItems.Add(new SelectListItem() { Text = location.Name, Value = location.Name });
            }

            ViewBag.Location = locationListItems;

            return View(menu);
        }

        public ActionResult MoveUp(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Menu menu1 = _menuLogic.Get(id);

            if (menu1 == null)
            {
                return NotFound();
            }

            menu1.Timestamp = DateTime.Now;            
            menu1.SortOrder--;
            int newSortOrder = menu1.SortOrder;

            Menu menu2 = _menuLogic.GetGeneric(item => item.SortOrder == newSortOrder).SingleOrDefault();

            if (menu2 != null)
            {
                if (menu2.Location == menu1.Location)
                {
                    menu2.Timestamp = DateTime.Now;
                    menu2.SortOrder++;
                }else
                {
                    //no need to move or change sort order. this menu is already the first sort order for it's location (Top/Left)
                    menu1.SortOrder++; //revert to same before save changes
                }
            }

            _menuLogic.Edit(menu1);
            _menuLogic.Edit(menu2);

            return RedirectToAction("Index");
        }

        public ActionResult MoveDown(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }

            Menu menu1 =_menuLogic.Get(id);

            if (menu1 == null)
            {
                return NotFound();
            }

            menu1.Timestamp = DateTime.Now;            
            menu1.SortOrder++;
            int newSortOrder = menu1.SortOrder;
            Menu menu2 = _menuLogic.GetGeneric(item => item.SortOrder == newSortOrder).SingleOrDefault();

            if (menu2 != null)
            {
                if (menu2.Location == menu1.Location)
                {
                    menu2.Timestamp = DateTime.Now;
                    menu2.SortOrder--;
                }else
                {
                    //no need to move or change sort order. this menu is already the first sort order for it's location (Top/Left)
                    menu1.SortOrder--; //revert to same before save changes
                }
            }

            _menuLogic.Edit(menu1);
            _menuLogic.Edit(menu2);

            return RedirectToAction("Index");
        }

        // GET: /Admin/MenuManager/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return BadRequest();
            }
            Menu menu = _menuLogic.Get(id);

            if (menu == null)
            {
                return NotFound();
            }

            return View(menu);
        }

        // POST: /Admin/MenuManager/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Menu menu = _menuLogic.Get(id);
            int deletedSortOrder = menu.SortOrder;
            var menus = _menuLogic.GetGeneric(s => s.SortOrder > deletedSortOrder);

            foreach (Menu otherMenu in menus)
            {
                otherMenu.SortOrder--;
            }

            _menuLogic.Delete(menu);

            return RedirectToAction("Index");
        }
    }
}
