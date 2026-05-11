using Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CsgoInventory.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryRepository _repository;

        public InventoryController(InventoryRepository repository)
        {
            _repository = repository;
        }

        // Hlavní stránka inventáře (kombinuje F1 a F6)
        public IActionResult Index(int userId = 1) // default test user
        {
            ViewBag.CurrentUser = _repository.GetCurrentUser(userId);
            var inventory = _repository.GetInventory(userId);
            return View(inventory);
        }

        // F4 API - Hledání skinu
        [HttpGet]
        public IActionResult SearchSkins(string query)
        {
            return Json(_repository.GetSkins(query));
        }

        // F5 Akce
        [HttpPost]
        public IActionResult AddSkin(int userId, int skinId, decimal floatValue, int patternSeed, bool isStatTrak)
        {
            _repository.AddItemToInventory(userId, skinId, floatValue, patternSeed, isStatTrak);
            return RedirectToAction("Index", new { userId = userId });
        }

        // F7 Akce
        [HttpPost]
        public IActionResult DeleteItem(long itemId, int currentUserId)
        {
            _repository.RemoveItem(itemId);
            return RedirectToAction("Index", new { userId = currentUserId });
        }


        // F9 API - Získání samolepek
        [HttpGet]
        public IActionResult GetStickers(long inventoryItemId)
        {
            return Json(_repository.GetStickers(inventoryItemId));
        }

        // Zobrazí stránku s detailem zbraně a formulářem pro samolepky
        [HttpGet]
        public IActionResult Sticker(long itemId, int userId = 1)
        {
            ViewBag.CurrentUser = _repository.GetCurrentUser(userId);

            // Najdeme ten konkrétní kliknutý item
            var item = _repository.GetInventory(userId).FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound("Předmět nebyl nalezen.");

            // Rovnou z databáze načteme existující samolepky na této zbrani (F9 z PDF)
            ViewBag.Stickers = _repository.GetStickers(itemId);

            return View(item);
        }

        // Zobrazí stránku s formulářem pro Trade
        [HttpGet]
        public IActionResult Trade(long itemId, int userId = 1)
        {
            ViewBag.CurrentUser = _repository.GetCurrentUser(userId);

            // Najdeme ten konkrétní kliknutý item
            var item = _repository.GetInventory(userId).FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound("Předmět nebyl nalezen.");

            return View(item);
        }

        // F8 Akce
        [HttpPost]
        public IActionResult AddSticker(long inventoryItemId, string stickerName, int slotIndex, decimal wear, int currentUserId)
        {
            _repository.AddSticker(inventoryItemId, stickerName, slotIndex, wear);
            return RedirectToAction("Index", new { userId = currentUserId });
        }

        // F10 Akce
        [HttpPost]
        public IActionResult DeleteSticker(long stickerId, int currentUserId)
        {
            _repository.RemoveSticker(stickerId);
            return RedirectToAction("Index", new { userId = currentUserId });
        }

        // F11 Akce
        [HttpPost]
        public IActionResult Trade(long itemId, int userFrom, int userTo, decimal price)
        {
            _repository.TradeItem(itemId, userFrom, userTo, price);
            return RedirectToAction("Index", new { userId = userFrom });
        }
    }
}