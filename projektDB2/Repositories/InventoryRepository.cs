using System;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using projektDB2.Models; // Uprav případně podle své složky s modely

namespace projektDB2.Repositories
{
    public class InventoryRepository
    {
        private readonly string _connectionString;

        public InventoryRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // F1. GetCurrentUser - čtení přes funkci a kurzor
        public User? GetCurrentUser(int userId)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetCurrentUser_Func";

                    // Založení parametru pro návratovou hodnotu (kurzor)
                    var returnParam = new OracleParameter("ReturnValue", OracleDbType.RefCursor);
                    returnParam.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(returnParam);

                    cmd.Parameters.Add("p_id_user", OracleDbType.Int32).Value = userId;

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

        // F4. GetSkins - čtení přes funkci a kurzor
        public List<Skin> GetSkins(string name)
        {
            var skins = new List<Skin>();
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetSkins_Func";

                    var returnParam = new OracleParameter("ReturnValue", OracleDbType.RefCursor);
                    returnParam.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(returnParam);

                    cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;

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

        // F6. GetInventory - čtení přes funkci a kurzor
        public List<InventoryItem> GetInventory(int userId)
        {
            var inventory = new List<InventoryItem>();
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetInventory_Func";

                    var returnParam = new OracleParameter("ReturnValue", OracleDbType.RefCursor);
                    returnParam.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(returnParam);

                    cmd.Parameters.Add("p_id_user", OracleDbType.Int32).Value = userId;

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

        // F9. GetStickers - čtení přes funkci a kurzor
        public List<ItemSticker> GetStickers(long inventoryItemId)
        {
            var stickers = new List<ItemSticker>();
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "GetStickers_Func";

                    var returnParam = new OracleParameter("ReturnValue", OracleDbType.RefCursor);
                    returnParam.Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.Add(returnParam);

                    cmd.Parameters.Add("p_id_item", OracleDbType.Int64).Value = inventoryItemId;

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

        // F5. AddItemToInventory - Zápis přes proceduru
        public bool AddItemToInventory(int userId, int skinId, decimal floatValue, int patternSeed, bool isStatTrak)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "AddItem_Proc";

                    cmd.Parameters.Add("p_id_user", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_id_skin", OracleDbType.Int32).Value = skinId;
                    cmd.Parameters.Add("p_float_value", OracleDbType.Decimal).Value = floatValue;
                    cmd.Parameters.Add("p_pattern_seed", OracleDbType.Int32).Value = patternSeed;
                    cmd.Parameters.Add("p_is_stattrak", OracleDbType.Int32).Value = isStatTrak ? 1 : 0;

                    var outSuccess = new OracleParameter("p_success", OracleDbType.Int32);
                    outSuccess.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outSuccess);

                    cmd.ExecuteNonQuery();
                    return outSuccess.Value.ToString() == "1";
                }
            }
        }

        // F11. TradeItem - Zápis přes proceduru
        public bool TradeItem(long itemId, int userFrom, int userTo, decimal price)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "TradeItem_Proc";

                    cmd.Parameters.Add("p_id_item", OracleDbType.Int64).Value = itemId;
                    cmd.Parameters.Add("p_id_user_from", OracleDbType.Int32).Value = userFrom;
                    cmd.Parameters.Add("p_id_user_to", OracleDbType.Int32).Value = userTo;
                    cmd.Parameters.Add("p_price", OracleDbType.Decimal).Value = price;

                    var outSuccess = new OracleParameter("p_success", OracleDbType.Int32);
                    outSuccess.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outSuccess);

                    cmd.ExecuteNonQuery();
                    return outSuccess.Value.ToString() == "1";
                }
            }
        }

        // F8. AddSticker - Zápis přes proceduru
        public bool AddSticker(long inventoryItemId, string stickerName, int slotIndex, decimal wear)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "AddSticker_Proc";

                    cmd.Parameters.Add("p_item_id", OracleDbType.Int64).Value = inventoryItemId;
                    cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = stickerName;
                    cmd.Parameters.Add("p_slot", OracleDbType.Int32).Value = slotIndex;
                    cmd.Parameters.Add("p_wear", OracleDbType.Decimal).Value = wear;

                    var outSuccess = new OracleParameter("p_success", OracleDbType.Int32);
                    outSuccess.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outSuccess);

                    cmd.ExecuteNonQuery();
                    return outSuccess.Value.ToString() == "1";
                }
            }
        }

        // F7. RemoveItem - Smazání přes proceduru
        public bool RemoveItem(long itemId)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "RemoveItem_Proc";

                    cmd.Parameters.Add("p_id_item", OracleDbType.Int64).Value = itemId;

                    var outSuccess = new OracleParameter("p_success", OracleDbType.Int32);
                    outSuccess.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outSuccess);

                    cmd.ExecuteNonQuery();
                    return outSuccess.Value.ToString() == "1";
                }
            }
        }

        // F10. RemoveSticker - Smazání přes proceduru
        public bool RemoveSticker(long stickerId)
        {
            using (OracleConnection con = new OracleConnection(_connectionString))
            {
                con.Open();
                using (OracleCommand cmd = con.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "RemoveSticker_Proc";

                    cmd.Parameters.Add("p_id_sticker", OracleDbType.Int64).Value = stickerId;

                    var outSuccess = new OracleParameter("p_success", OracleDbType.Int32);
                    outSuccess.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(outSuccess);

                    cmd.ExecuteNonQuery();
                    return outSuccess.Value.ToString() == "1";
                }
            }
        }
    }
}