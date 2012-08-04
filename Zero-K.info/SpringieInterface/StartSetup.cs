﻿using System;
using System.Collections.Generic;
using System.Linq;
using PlasmaShared;
using ZkData;

namespace ZeroKWeb.SpringieInterface
{
    public class SpringBattleStartSetup
    {
        public List<ScriptKeyValuePair> ModOptions = new List<ScriptKeyValuePair>();
        public List<UserCustomParameters> UserParameters = new List<UserCustomParameters>();

        #region Nested type: ScriptKeyValuePair

        public class ScriptKeyValuePair
        {
            public string Key;
            public string Value;
        }

        #endregion

        #region Nested type: UserCustomParameters

        public class UserCustomParameters
        {
            public int LobbyID;
            public List<ScriptKeyValuePair> Parameters = new List<ScriptKeyValuePair>();
        }

        #endregion
    }


    public class StartSetup
    {
        public static SpringBattleStartSetup GetSpringBattleStartSetup(BattleContext context) {
            try {
                AutohostMode mode = context.GetMode();
                var ret = new SpringBattleStartSetup();
                var commanderTypes = new LuaTable();
                var db = new ZkDataContext();

                var accountIDsWithExtraComms = new List<int>();
                // calculate to whom to send extra comms
                if (mode == AutohostMode.Planetwars || mode == AutohostMode.BigTeams || mode == AutohostMode.GameFFA ||
                    mode == AutohostMode.SmallTeams || mode == AutohostMode.Experienced) {
                    IOrderedEnumerable<IGrouping<int, PlayerTeam>> groupedByTeam =
                        context.Players.Where(x => !x.IsSpectator).GroupBy(x => x.AllyID).OrderByDescending(x => x.Count());
                    IGrouping<int, PlayerTeam> biggest = groupedByTeam.FirstOrDefault();
                    if (biggest != null) {
                        foreach (var other in groupedByTeam.Skip(1)) {
                            int cnt = biggest.Count() - other.Count();
                            if (cnt > 0) {
                                foreach (Account a in
                                    other.Select(x => db.Accounts.First(y => y.LobbyID == x.LobbyID)).OrderByDescending(x => x.Elo*x.EloWeight).Take(
                                        cnt)) accountIDsWithExtraComms.Add(a.AccountID);
                            }
                        }
                    }
                }

                Faction attacker = null;
                Faction defender = null;
                Planet planet = null;
                if (mode == AutohostMode.Planetwars) {
                    planet = db.Galaxies.Single(x => x.IsDefault).Planets.Single(x => x.Resource.InternalName == context.Map);
                    List<int> presentFactions =
                        context.Players.Where(x => !x.IsSpectator).Select(x => db.Accounts.First(y => y.LobbyID == x.LobbyID)).Where(
                            x => x.Faction != null).GroupBy(x => x.Faction).Select(x => x.Key.FactionID).ToList();
                    attacker = planet.GetAttacker(presentFactions);
                    defender = planet.Faction;

                    ret.ModOptions.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "attackingFaction", Value = attacker.Shortcut });
                    if (defender != null) ret.ModOptions.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "defendingFaction", Value = defender.Shortcut });
                    ret.ModOptions.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "planet", Value = planet.Name });
                }

                foreach (PlayerTeam p in context.Players) {
                    Account user = db.Accounts.FirstOrDefault(x => x.LobbyID == p.LobbyID);
                    if (user != null) {
                        var userParams = new List<SpringBattleStartSetup.ScriptKeyValuePair>();
                        ret.UserParameters.Add(new SpringBattleStartSetup.UserCustomParameters { LobbyID = p.LobbyID, Parameters = userParams });

                        bool userBanMuted = user.PunishmentsByAccountID.Any(x => x.BanExpires > DateTime.UtcNow && x.BanMute);
                        if (userBanMuted) userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "muted", Value = "1" });
                        userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair
                                       { Key = "faction", Value = user.Faction != null ? user.Faction.Shortcut : "" });
                        userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair
                                       { Key = "clan", Value = user.Clan != null ? user.Clan.Shortcut : "" });
                        userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "level", Value = user.Level.ToString() });
                        userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "elo", Value = user.EffectiveElo.ToString() });
                        userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "avatar", Value = user.Avatar });

                        if (!p.IsSpectator) {
                            if (mode == AutohostMode.Planetwars) {
                                bool allied = user.Faction != null && defender != null && user.Faction != defender &&
                                              defender.HasTreatyRight(user.Faction, x => x.EffectPreventIngamePwStructureDestruction == true, planet);

                                if (!allied && user.Faction != null && (user.Faction == attacker || user.Faction == defender)) {
                                    userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "canAttackPwStructures", Value = "1" });
                                    ;
                                }
                            }

                            var pu = new LuaTable();
                            bool userUnlocksBanned = user.PunishmentsByAccountID.Any(x => x.BanExpires > DateTime.UtcNow && x.BanUnlocks);
                            bool userCommandersBanned = user.PunishmentsByAccountID.Any(x => x.BanExpires > DateTime.UtcNow && x.BanCommanders);

                            if (!userUnlocksBanned) {
                                if (mode != AutohostMode.Planetwars || user.Faction == null) foreach (Unlock unlock in user.AccountUnlocks.Select(x => x.Unlock)) pu.Add(unlock.Code);
                                else {
                                    foreach (Unlock unlock in
                                        user.AccountUnlocks.Select(x => x.Unlock).Union(user.Faction.GetFactionUnlocks().Select(x => x.Unlock))) pu.Add(unlock.Code);
                                }
                            }

                            userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "unlocks", Value = pu.ToBase64String() });

                            if (accountIDsWithExtraComms.Contains(user.AccountID)) userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "extracomm", Value = "1" });

                            var pc = new LuaTable();

                            if (!userCommandersBanned) {
                                foreach (Commander c in user.Commanders.Where(x => x.Unlock != null && x.ProfileNumber <= GlobalConst.CommanderProfileCount)) {
                                    try {
                                        if (string.IsNullOrEmpty(c.Name) || c.Name.Any(x => !Char.IsLetterOrDigit(x) && x != ' ')) c.Name = c.CommanderID.ToString();
                                        var morphTable = new LuaTable();
                                        pc["[\"" + c.Name + "\"]"] = morphTable;

                                        string prevKey = null;
                                        for (int i = 0; i <= GlobalConst.NumCommanderLevels; i++) {
                                            string key = string.Format("c{0}_{1}_{2}", user.AccountID, c.ProfileNumber, i);
                                            morphTable.Add(key);

                                            var comdef = new LuaTable();
                                            commanderTypes[key] = comdef;

                                            comdef["chassis"] = c.Unlock.Code + Math.Max(i,1);

                                            comdef["name"] = c.Name.Substring(0, Math.Min(25, c.Name.Length)) + " level " + i;
                                            if (i > 0)
                                            {
                                                var modules = new LuaTable();
                                                comdef["modules"] = modules;

                                                comdef["cost"] = c.GetTotalMorphLevelCost(i);

                                                if (prevKey != null) comdef["prev"] = prevKey;

                                                prevKey = key;
                                                foreach (Unlock m in
                                                        c.CommanderModules.Where(x => x.CommanderSlot.MorphLevel == i && x.Unlock != null).OrderBy(
                                                            x => x.Unlock.UnlockType).ThenBy(x => x.SlotID).Select(x => x.Unlock)) modules.Add(m.Code);
                                            }
                                        }
                                    } catch (Exception ex) {
                                        throw new ApplicationException(
                                            string.Format("Error processing commander: {0} - {1} of player {2} - {3}",
                                                          c.CommanderID,
                                                          c.Name,
                                                          user.AccountID,
                                                          user.Name),
                                            ex);
                                    }
                                }
                            }
                            else userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "jokecomm", Value = "1" });

                            userParams.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "commanders", Value = pc.ToBase64String() });
                        }
                    }
                }

                ret.ModOptions.Add(new SpringBattleStartSetup.ScriptKeyValuePair { Key = "commanderTypes", Value = commanderTypes.ToBase64String() });
                if (mode == AutohostMode.Planetwars) {
                    string owner = planet.Faction != null ? planet.Faction.Shortcut : "";

                    var pwStructures = new LuaTable();
                    foreach (PlanetStructure s in planet.PlanetStructures.Where(x => x.StructureType!= null && !string.IsNullOrEmpty(x.StructureType.IngameUnitName))) {
                        pwStructures.Add("s" + s.StructureTypeID,
                                         new LuaTable
                                         {
                                             { "unitname", s.StructureType.IngameUnitName },
                                             //{ "isDestroyed", s.IsDestroyed ? true : false },
                                             { "name", string.Format("{0} {1} ({2})", owner, s.StructureType.Name, s.Account!= null ? s.Account.Name:"unowned") },
                                             { "description", s.StructureType.Description }
                                         });
                    }
                    ret.ModOptions.Add(new SpringBattleStartSetup.ScriptKeyValuePair
                                       { Key = "planetwarsStructures", Value = pwStructures.ToBase64String() });
                }

                return ret;
            } catch (Exception ex) {
                var db = new ZkDataContext();
                Account licho = db.Accounts.SingleOrDefault(x => x.AccountID == 5986);
                if (licho != null) foreach (string line in ex.ToString().Lines()) AuthServiceClient.SendLobbyMessage(licho, line);
                throw;
            }
        }
    }
}