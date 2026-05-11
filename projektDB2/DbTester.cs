using System;
using System.Linq;
using projektDB2.Repositories;

namespace projektDB2
{
    public static class DbTester
    {
        public static void RunTests(string connectionString)
        {
            var repo = new InventoryRepository(connectionString);

            Console.WriteLine("\n==========================================");
            Console.WriteLine("=== SPUŠTĚNÍ AUTOMATICKÝCH DB TESTŮ ===");
            Console.WriteLine("==========================================\n");

            try
            {
                // 1. Čtení dat
                Console.Write("Test 1: Načtení uživatele z DB... ");
                var user = repo.GetCurrentUser(1);
                if (user != null) PrintOk(); else PrintFail();

                Console.Write("Test 2: Vyhledání skinů (kurzor)... ");
                var skins = repo.GetSkins("Head"); // Hledá "Head Shot"
                if (skins.Count > 0) PrintOk(); else PrintFail();

                // 2. Transakce - Přidání itemu s validací floatu (AK-47 má min 0.00, max 1.00)
                Console.Write("Test 3: Zápis itemu do DB (Správná data)... ");
                bool added = repo.AddItemToInventory(userId: 1, skinId: 1, floatValue: 0.15m, patternSeed: 420, isStatTrak: false);
                if (added) PrintOk(); else PrintFail();

                Console.Write("Test 4: Zápis itemu do DB (Špatný Seed > 1000, má být zamítnuto)... ");
                bool badSeedAdded = repo.AddItemToInventory(userId: 1, skinId: 1, floatValue: 0.15m, patternSeed: 9999, isStatTrak: false);
                if (!badSeedAdded) PrintOk("ZAMÍTNUTO (Správně)"); else PrintFail("Prošlo to a nemělo!");

                // Najdeme ID nově přidaného itemu pro další testy
                var inventory = repo.GetInventory(1);
                var testItem = inventory.OrderByDescending(i => i.Id).FirstOrDefault();
                long testItemId = testItem?.Id ?? 0;

                // 3. Transakce - Přidání samolepky
                Console.Write("Test 5: Přidání samolepky na zbraň... ");
                bool stickerAdded = false;
                if (testItemId > 0) stickerAdded = repo.AddSticker(testItemId, "Titan Holo Test", 1, 0.05m);
                if (stickerAdded) PrintOk(); else PrintFail();

                // 4. Transakce - Trade Itemu
                Console.Write("Test 6: Trade Itemu (z ID 1 na ID 2 za 500 balance)... ");
                bool traded = false;
                if (testItemId > 0) traded = repo.TradeItem(testItemId, userFrom: 1, userTo: 2, price: 500m);
                if (traded) PrintOk(); else PrintFail();

                // 5. Transakce - Smazání itemu (Musí smazat i samolepku kvůli vazbě)
                Console.Write("Test 7: Kaskádové smazání itemu a jeho samolepek... ");
                bool removed = false;
                if (testItemId > 0) removed = repo.RemoveItem(testItemId);
                if (removed) PrintOk(); else PrintFail();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[!] KRITICKÁ CHYBA: {ex.Message}");
            }

            Console.WriteLine("\n==========================================");
            Console.WriteLine("=== TESTY DOKONČENY ===");
            Console.WriteLine("==========================================\n");
        }

        private static void PrintOk(string message = "OK")
        {
            // V konzoli VS to sice nepřebarví text (kdyby to bylo čisté CMD, tak ano), 
            // ale pro přehlednost formátujeme výpis s plusem.
            Console.WriteLine($"[+] {message}");
        }

        private static void PrintFail(string message = "CHYBA")
        {
            Console.WriteLine($"[-] {message}");
        }
    }
}