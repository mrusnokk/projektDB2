using projektDB2.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;

namespace Repositories
{
    public class InventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // F1. GetCurrentUser
        public User? GetCurrentUser(int userId)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, username, steam_id, balance FROM users WHERE id = :id";
                    cmd.Parameters.Add(new OracleParameter("id", userId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Username = reader["username"]?.ToString(),
                                SteamId = reader["steam_id"]?.ToString(),
                                Balance = Convert.ToDecimal(reader["balance"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        // F4. GetSkins
        public List<Skin> GetSkins(string name)
        {
            var skins = new List<Skin>();
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT s.id, s.name, w.name AS weapon_name, r.name AS rarity_name, r.color_hex, c.name AS collection_name, s.min_float, s.max_float
                        FROM skins s
                        JOIN weapons w ON s.weapon_id = w.id
                        JOIN rarities r ON s.rarity_id = r.id
                        JOIN collections c ON s.collection_id = c.id
                        WHERE s.name LIKE '%' || :name || '%'";
                    cmd.Parameters.Add(new OracleParameter("name", name));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            skins.Add(new Skin
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"]?.ToString(),
                                WeaponName = reader["weapon_name"]?.ToString(),
                                Rarity = reader["rarity_name"]?.ToString(),
                                ColorHex = reader["color_hex"]?.ToString(),
                                CollectionName = reader["collection_name"]?.ToString(),
                                MinFloat = reader["min_float"] != DBNull.Value ? Convert.ToDecimal(reader["min_float"]) : 0,
                                MaxFloat = reader["max_float"] != DBNull.Value ? Convert.ToDecimal(reader["max_float"]) : 1
                            });
                        }
                    }
                }
            }
            return skins;
        }

        // F5. AddItemToInventory
        public bool AddItemToInventory(int userId, int skinId, decimal floatValue, int patternSeed, bool isStatTrak)
        {
            if (patternSeed < 0 || patternSeed > 1000) return false;

            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleTransaction transaction = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        using (OracleCommand cmd = con.CreateCommand())
                        {
                            cmd.Transaction = transaction;

                            // Kontrola rozsahu floatu
                            cmd.CommandText = "SELECT min_float, max_float FROM skins WHERE id = :skinId";
                            cmd.Parameters.Add(new OracleParameter("skinId", skinId));
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (!reader.Read()) throw new Exception("Skin nenalezen");
                                decimal minF = Convert.ToDecimal(reader["min_float"]);
                                decimal maxF = Convert.ToDecimal(reader["max_float"]);
                                if (floatValue < minF || floatValue > maxF) throw new Exception("Neplatný float");
                            }

                            // Insert
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO inventory_items (user_id, skin_id, float_value, pattern_seed, is_stattrak) VALUES (:userId, :skinId, :floatValue, :patternSeed, :isStatTrak)";
                            cmd.Parameters.Add(new OracleParameter("userId", userId));
                            cmd.Parameters.Add(new OracleParameter("skinId", skinId));
                            cmd.Parameters.Add(new OracleParameter("floatValue", floatValue));
                            cmd.Parameters.Add(new OracleParameter("patternSeed", patternSeed));
                            cmd.Parameters.Add(new OracleParameter("isStatTrak", isStatTrak ? 1 : 0));
                            cmd.ExecuteNonQuery();

                            transaction.Commit();
                            return true;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // F6. GetInventory
        public List<InventoryItem> GetInventory(int userId)
        {
            var inventory = new List<InventoryItem>();
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT ii.id, ii.user_id, ii.skin_id, ii.float_value, ii.pattern_seed, ii.is_stattrak,
                               s.name AS skin_name, w.name AS weapon_name, 
                               r.name AS rarity_name, r.color_hex,
                               (SELECT COUNT(*) FROM item_stickers WHERE inventory_item_id = ii.id) AS sticker_count
                        FROM inventory_items ii
                        JOIN skins s ON ii.skin_id = s.id
                        JOIN weapons w ON s.weapon_id = w.id
                        JOIN rarities r ON s.rarity_id = r.id
                        WHERE ii.user_id = :userId";
                    cmd.Parameters.Add(new OracleParameter("userId", userId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            inventory.Add(new InventoryItem
                            {
                                Id = Convert.ToInt64(reader["id"]),
                                UserId = Convert.ToInt32(reader["user_id"]),
                                SkinId = Convert.ToInt32(reader["skin_id"]),
                                FloatValue = Convert.ToDecimal(reader["float_value"]),
                                PatternSeed = Convert.ToInt32(reader["pattern_seed"]),
                                IsStatTrak = Convert.ToBoolean(reader["is_stattrak"]),
                                SkinName = reader["skin_name"]?.ToString(),
                                WeaponName = reader["weapon_name"]?.ToString(),
                                Rarity = reader["rarity_name"]?.ToString(),
                                ColorHex = reader["color_hex"]?.ToString(),
                                StickerCount = Convert.ToInt32(reader["sticker_count"])
                            });
                        }
                    }
                }
            }
            return inventory;
        }

        // F7. RemoveItem
        public bool RemoveItem(long itemId)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        using (OracleCommand cmd = con.CreateCommand())
                        {
                            cmd.Transaction = transaction;
                            // Nejdřív smazat cizí klíče (stickers)
                            cmd.CommandText = "DELETE FROM item_stickers WHERE inventory_item_id = :itemId";
                            cmd.Parameters.Add(new OracleParameter("itemId", itemId));
                            cmd.ExecuteNonQuery();

                            // Smazat samotný item
                            cmd.Parameters.Clear();
                            cmd.CommandText = "DELETE FROM inventory_items WHERE id = :itemId";
                            cmd.Parameters.Add(new OracleParameter("itemId", itemId));
                            cmd.ExecuteNonQuery();

                            transaction.Commit();
                            return true;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // F8. AddSticker
        public bool AddSticker(long inventoryItemId, string stickerName, int slotIndex, decimal wear)
        {
            if (slotIndex < 0 || slotIndex > 4 || wear < 0 || wear > 1) return false;

            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleTransaction transaction = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        using (OracleCommand cmd = con.CreateCommand())
                        {
                            cmd.Transaction = transaction;

                            // Kontrola obsazeného slotu
                            cmd.CommandText = "SELECT COUNT(*) FROM item_stickers WHERE inventory_item_id = :itemId AND slot_index = :slotIndex";
                            cmd.Parameters.Add(new OracleParameter("itemId", inventoryItemId));
                            cmd.Parameters.Add(new OracleParameter("slotIndex", slotIndex));

                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0) throw new Exception("Slot je obsazen");

                            // Insert stickeru
                            cmd.Parameters.Clear();
                            cmd.CommandText = "INSERT INTO item_stickers (inventory_item_id, sticker_name, slot_index, wear) VALUES (:itemId, :name, :slotIndex, :wear)";
                            cmd.Parameters.Add(new OracleParameter("itemId", inventoryItemId));
                            cmd.Parameters.Add(new OracleParameter("name", stickerName));
                            cmd.Parameters.Add(new OracleParameter("slotIndex", slotIndex));
                            cmd.Parameters.Add(new OracleParameter("wear", wear));
                            cmd.ExecuteNonQuery();

                            transaction.Commit();
                            return true;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        // F9. GetStickers
        public List<ItemSticker> GetStickers(long inventoryItemId)
        {
            var stickers = new List<ItemSticker>();
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "SELECT id, inventory_item_id, sticker_name, slot_index, wear FROM item_stickers WHERE inventory_item_id = :itemId";
                    cmd.Parameters.Add(new OracleParameter("itemId", inventoryItemId));

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            stickers.Add(new ItemSticker
                            {
                                Id = Convert.ToInt64(reader["id"]),
                                InventoryItemId = Convert.ToInt64(reader["inventory_item_id"]),
                                StickerName = reader["sticker_name"]?.ToString(),
                                SlotIndex = Convert.ToInt32(reader["slot_index"]),
                                Wear = Convert.ToDecimal(reader["wear"])
                            });
                        }
                    }
                }
            }
            return stickers;
        }

        // F10. RemoveSticker
        public bool RemoveSticker(long stickerId)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM item_stickers WHERE id = :stickerId";
                    cmd.Parameters.Add(new OracleParameter("stickerId", stickerId));
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // F11. TradeItem
        public bool TradeItem(long itemId, int userFrom, int userTo, decimal price)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleTransaction transaction = con.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        using (OracleCommand cmd = con.CreateCommand())
                        {
                            cmd.Transaction = transaction;

                            cmd.CommandText = "SELECT user_id FROM inventory_items WHERE id = :itemId FOR UPDATE";
                            cmd.Parameters.Add(new OracleParameter("itemId", itemId));
                            object? ownerIdObj = cmd.ExecuteScalar();
                            if (ownerIdObj == null || Convert.ToInt32(ownerIdObj) != userFrom) throw new Exception("Neplatný vlastník");

                            cmd.Parameters.Clear();
                            cmd.CommandText = "SELECT balance FROM users WHERE id = :userTo FOR UPDATE";
                            cmd.Parameters.Add(new OracleParameter("userTo", userTo));
                            object? balanceObj = cmd.ExecuteScalar();
                            if (balanceObj == null || Convert.ToDecimal(balanceObj) < price) throw new Exception("Nedostatek prostředků");

                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE inventory_items SET user_id = :userTo WHERE id = :itemId";
                            cmd.Parameters.Add(new OracleParameter("userTo", userTo));
                            cmd.Parameters.Add(new OracleParameter("itemId", itemId));
                            cmd.ExecuteNonQuery();

                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE users SET balance = balance - :price WHERE id = :userTo";
                            cmd.Parameters.Add(new OracleParameter("price", price));
                            cmd.Parameters.Add(new OracleParameter("userTo", userTo));
                            cmd.ExecuteNonQuery();

                            cmd.Parameters.Clear();
                            cmd.CommandText = "UPDATE users SET balance = balance + :price WHERE id = :userFrom";
                            cmd.Parameters.Add(new OracleParameter("price", price));
                            cmd.Parameters.Add(new OracleParameter("userFrom", userFrom));
                            cmd.ExecuteNonQuery();

                            transaction.Commit();
                            return true;
                        }
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}