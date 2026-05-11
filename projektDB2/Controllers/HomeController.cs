using Microsoft.AspNetCore.Mvc;
using System.Linq;
using projektDB2.Repositories;
using projektDB2.Models;

namespace projektDB2.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryRepository _repository;

        public InventoryController(InventoryRepository repository)
        {
            _repository = repository;
        }

        // 1. Zobrazení hlavní stránky
        [HttpGet]
        public IActionResult Index(int userId = 1)
        {
            ViewBag.CurrentUser = _repository.GetCurrentUser(userId);
            var inventory = _repository.GetInventory(userId);
            return View(inventory);
        }

        // 2. Zobrazení formuláře pro Sticker
        [HttpGet]
        public IActionResult Sticker(long itemId, int userId = 1)
        {
            ViewBag.CurrentUser = _repository.GetCurrentUser(userId);

            var item = _repository.GetInventory(userId).FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound("Předmět nebyl nalezen.");

            ViewBag.Stickers = _repository.GetStickers(itemId);
            return View(item);
        }

        // 3. Zobrazení formuláře pro Trade
        [HttpGet]
        public IActionResult Trade(long itemId, int userId = 1)
        {
            ViewBag.CurrentUser = _repository.GetCurrentUser(userId);

            var item = _repository.GetInventory(userId).FirstOrDefault(i => i.Id == itemId);
            if (item == null) return NotFound("Předmět nebyl nalezen.");

            return View(item);
        }

        // AKCE: Přidání skinu (F5)
        [HttpPost]
        public IActionResult AddSkin(int userId, int skinId, decimal floatValue, int patternSeed, bool isStatTrak)
        {
            bool success = _repository.AddItemToInventory(userId, skinId, floatValue, patternSeed, isStatTrak);

            if (success)
                TempData["SuccessMessage"] = "Předmět byl úspěšně přidán do inventáře!";
            else
                TempData["ErrorMessage"] = "Chyba: Zkontrolujte, zda ID skinu existuje, Float je v povoleném rozsahu a Seed je 0-1000.";

            return RedirectToAction("Index", new { userId = userId });
        }

        // AKCE: Smazání itemu (F7)
        [HttpPost]
        public IActionResult DeleteItem(long itemId, int currentUserId)
        {
            bool success = _repository.RemoveItem(itemId);

            if (success)
                TempData["SuccessMessage"] = "Předmět (i s případnými samolepkami) byl smazán.";
            else
                TempData["ErrorMessage"] = "Chyba při mazání předmětu.";

            return RedirectToAction("Index", new { userId = currentUserId });
        }

        // AKCE: Přidání samolepky (F8)
        [HttpPost]
        public IActionResult AddSticker(long inventoryItemId, string stickerName, int slotIndex, decimal wear, int currentUserId)
        {
            bool success = _repository.AddSticker(inventoryItemId, stickerName, slotIndex, wear);

            if (success)
                TempData["SuccessMessage"] = "Sticker byl úspěšně nalepen!";
            else
                TempData["ErrorMessage"] = "Chyba: Slot je již obsazený, nebo je Wear mimo rozsah 0-1.";

            // Vracíme se zpět na detail Stickeru, ne na Index
            return RedirectToAction("Sticker", new { itemId = inventoryItemId, userId = currentUserId });
        }

        // AKCE: Smazání samolepky (F10)
        [HttpPost]
        public IActionResult DeleteSticker(long stickerId, long inventoryItemId, int currentUserId)
        {
            bool success = _repository.RemoveSticker(stickerId);

            if (success)
                TempData["SuccessMessage"] = "Sticker byl seškrábán.";
            else
                TempData["ErrorMessage"] = "Chyba při odstraňování stickeru.";

            return RedirectToAction("Sticker", new { itemId = inventoryItemId, userId = currentUserId });
        }

        // AKCE: Odeslání Trade (F11)
        [HttpPost]
        public IActionResult TradeItem(long itemId, int userFrom, int userTo, decimal price)
        {
            bool success = _repository.TradeItem(itemId, userFrom, userTo, price);

            if (success)
                TempData["SuccessMessage"] = $"Předmět byl úspěšně prodán uživateli ID {userTo} za {price}!";
            else
                TempData["ErrorMessage"] = "Trade selhal! Zkontrolujte, zda má příjemce dostatek peněz a zda předmět stále vlastníte.";

            return RedirectToAction("Index", new { userId = userFrom });
        }
    }
}