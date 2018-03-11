﻿using System.Collections.Generic;
using System.Linq;
using static PKHeX.Core.Legal;
using static PKHeX.Core.EncounterEvent;

namespace PKHeX.Core
{
    public static class MysteryGiftGenerator
    {
        // MysteryGift
        public static IEnumerable<MysteryGift> GetValidGifts(PKM pkm)
        {
            switch (pkm.GenNumber)
            {
                case 3: return GetMatchingWC3(pkm, MGDB_G3);
                case 4: return GetMatchingPCD(pkm, MGDB_G4);
                case 5: return GetMatchingPGF(pkm, MGDB_G5);
                case 6: return GetMatchingWC6(pkm, MGDB_G6);
                case 7: return GetMatchingWC7(pkm, MGDB_G7);
                default: return Enumerable.Empty<MysteryGift>();
            }
        }
        private static IEnumerable<MysteryGift> GetMatchingWC3(PKM pkm, IEnumerable<MysteryGift> DB)
        {
            if (DB == null)
                yield break;

            var validWC3 = new List<MysteryGift>();
            var vs = GetValidPreEvolutions(pkm, MaxSpeciesID_3).ToArray();
            var enumerable = DB.OfType<WC3>().Where(wc => vs.Any(dl => dl.Species == wc.Species));
            foreach (WC3 wc in enumerable)
            {
                if (!GetIsMatchWC3(pkm, wc))
                    continue;

                if (wc.Species == pkm.Species) // best match
                    yield return wc;
                else
                    validWC3.Add(wc);
            }
            foreach (var z in validWC3)
                yield return z;
        }
        private static IEnumerable<MysteryGift> GetMatchingPCD(PKM pkm, IEnumerable<MysteryGift> DB)
        {
            if (DB == null || pkm.IsEgg && pkm.Format != 4) // transferred
                yield break;

            if (IsRangerManaphy(pkm))
            {
                if (pkm.Language != (int)LanguageID.Korean) // never korean
                    yield return new PGT { Data = { [0] = 7, [8] = 1 } };
                yield break;
            }

            var deferred = new List<MysteryGift>();
            var vs = GetValidPreEvolutions(pkm).ToArray();
            var enumerable = DB.OfType<PCD>().Where(wc => vs.Any(dl => dl.Species == wc.Species));
            foreach (PCD mg in enumerable)
            {
                var wc = mg.Gift.PK;
                if (!GetIsMatchPCD(pkm, wc, vs))
                    continue;

                bool receivable = mg.CanBeReceivedBy(pkm.Version);
                if (wc.Species == pkm.Species && receivable) // best match
                    yield return mg;
                else
                    deferred.Add(mg);
            }
            foreach (var z in deferred)
                yield return z;
        }
        private static IEnumerable<MysteryGift> GetMatchingPGF(PKM pkm, IEnumerable<MysteryGift> DB)
        {
            if (DB == null)
                yield break;

            var deferred = new List<MysteryGift>();
            var vs = GetValidPreEvolutions(pkm).ToArray();
            var enumerable = DB.OfType<PGF>().Where(wc => vs.Any(dl => dl.Species == wc.Species));
            foreach (PGF wc in enumerable)
            {
                if (!GetIsMatchPGF(pkm, wc, vs))
                    continue;

                if (wc.Species == pkm.Species) // best match
                    yield return wc;
                else
                    deferred.Add(wc);
            }
            foreach (var z in deferred)
                yield return z;
        }
        private static IEnumerable<MysteryGift> GetMatchingWC6(PKM pkm, IEnumerable<MysteryGift> DB)
        {
            if (DB == null)
                yield break;
            var deferred = new List<MysteryGift>();
            var vs = GetValidPreEvolutions(pkm).ToArray();
            var enumerable = DB.OfType<WC6>().Where(wc => vs.Any(dl => dl.Species == wc.Species));
            foreach (WC6 wc in enumerable)
            {
                if (!GetIsMatchWC6(pkm, wc, vs))
                    continue;

                switch (wc.CardID)
                {
                    case 0525 when wc.IV_HP == 0xFE: // Diancie was distributed with no IV enforcement & 3IVs
                    case 0504 when wc.RibbonClassic != ((IRibbonSetEvent4)pkm).RibbonClassic: // magmar with/without classic
                        deferred.Add(wc);
                        continue;
                }
                if (wc.Species == pkm.Species) // best match
                    yield return wc;
                else
                    deferred.Add(wc);
            }
            foreach (var z in deferred)
                yield return z;
        }
        private static IEnumerable<MysteryGift> GetMatchingWC7(PKM pkm, IEnumerable<MysteryGift> DB)
        {
            if (DB == null)
                yield break;
            var deferred = new List<MysteryGift>();
            var vs = GetValidPreEvolutions(pkm).ToArray();
            var enumerable = DB.OfType<WC7>().Where(wc => vs.Any(dl => dl.Species == wc.Species));
            foreach (WC7 wc in enumerable)
            {
                if (!GetIsMatchWC7(pkm, wc, vs))
                    continue;

                if ((pkm.SID << 16 | pkm.TID) == 0x79F57B49) // Greninja WC has variant PID and can arrive @ 36 or 37
                {
                    if (!pkm.IsShiny)
                        deferred.Add(wc);
                    continue;
                }
                if (wc.PIDType == 0 && pkm.PID != wc.PID)
                    continue;

                if (wc.Species == pkm.Species) // best match
                    yield return wc;
                else
                    deferred.Add(wc);
            }
            foreach (var z in deferred)
                yield return z;
        }
        private static bool GetIsMatchWC3(PKM pkm, WC3 wc)
        {
            // Gen3 Version MUST match.
            if (wc.Version != 0 && !((GameVersion)wc.Version).Contains((GameVersion)pkm.Version))
                return false;

            bool hatchedEgg = wc.IsEgg && !pkm.IsEgg;
            if (!hatchedEgg)
            {
                if (wc.SID != -1 && wc.SID != pkm.SID) return false;
                if (wc.TID != -1 && wc.TID != pkm.TID) return false;
                if (wc.OT_Name != null && wc.OT_Name != pkm.OT_Name) return false;
                if (wc.OT_Gender < 3 && wc.OT_Gender != pkm.OT_Gender) return false;
            }

            if (wc.Language != -1 && wc.Language != pkm.Language) return false;
            if (wc.Ball != pkm.Ball) return false;
            if (wc.Fateful != pkm.FatefulEncounter)
            {
                // XD Gifts only at level 20 get flagged after transfer
                bool valid = wc.Level == 20 && pkm is XK3;
                if (!valid)
                    return false;
            }

            if (pkm.IsNative)
            {
                if (wc.Met_Level != pkm.Met_Level)
                    return false;
                if (wc.Location != pkm.Met_Location && (!wc.IsEgg || pkm.IsEgg))
                    return false;
            }
            else
            {
                if (pkm.IsEgg)
                    return false;
                if (wc.Level > pkm.Met_Level)
                    return false;
            }
            return true;
        }
        private static bool GetIsMatchPCD(PKM pkm, PKM wc, IEnumerable<DexLevel> vs)
        {
            if (!wc.IsEgg)
            {
                if (wc.TID != pkm.TID) return false;
                if (wc.SID != pkm.SID) return false;
                if (wc.OT_Name != pkm.OT_Name) return false;
                if (wc.OT_Gender != pkm.OT_Gender) return false;
                if (wc.Language != 0 && wc.Language != pkm.Language) return false;

                if (pkm.Format != 4) // transferred
                {
                    // met location: deferred to general transfer check
                    if (wc.CurrentLevel > pkm.Met_Level) return false;
                }
                else
                {
                    if (wc.Egg_Location + 3000 != pkm.Met_Location) return false;
                    if (wc.CurrentLevel != pkm.Met_Level) return false;
                }
            }
            else // Egg
            {
                if (wc.Egg_Location + 3000 != pkm.Egg_Location && pkm.Egg_Location != 2002) // traded
                    return false;
                if (wc.CurrentLevel != pkm.Met_Level)
                    return false;
                if (pkm.IsEgg && !pkm.IsNative)
                    return false;
            }

            if (wc.AltForm != pkm.AltForm && vs.All(dl => !IsFormChangeable(pkm, dl.Species)))
                return false;

            if (wc.Ball != pkm.Ball) return false;
            if (wc.OT_Gender < 3 && wc.OT_Gender != pkm.OT_Gender) return false;
            if (wc.PID == 1 && pkm.IsShiny) return false;
            if (wc.Gender != 3 && wc.Gender != pkm.Gender) return false;

            if (wc.CNT_Cool > pkm.CNT_Cool) return false;
            if (wc.CNT_Beauty > pkm.CNT_Beauty) return false;
            if (wc.CNT_Cute > pkm.CNT_Cute) return false;
            if (wc.CNT_Smart > pkm.CNT_Smart) return false;
            if (wc.CNT_Tough > pkm.CNT_Tough) return false;
            if (wc.CNT_Sheen > pkm.CNT_Sheen) return false;

            return true;
        }
        private static bool GetIsMatchPGF(PKM pkm, PGF wc, IEnumerable<DexLevel> vs)
        {
            if (!wc.IsEgg)
            {
                if (wc.SID != pkm.SID) return false;
                if (wc.TID != pkm.TID) return false;
                if (wc.OT_Name != pkm.OT_Name) return false;
                if (wc.OTGender < 3 && wc.OTGender != pkm.OT_Gender) return false;
                if (wc.PID != 0 && pkm.PID != wc.PID) return false;
                if (wc.PIDType == 0 && pkm.IsShiny) return false;
                if (wc.PIDType == 2 && !pkm.IsShiny) return false;
                if (wc.OriginGame != 0 && wc.OriginGame != pkm.Version) return false;
                if (wc.Language != 0 && wc.Language != pkm.Language) return false;

                if (wc.EggLocation != pkm.Egg_Location) return false;
                if (wc.MetLocation != pkm.Met_Location) return false;
            }
            else
            {
                if (wc.EggLocation != pkm.Egg_Location) // traded
                {
                    if (pkm.Egg_Location != 30003)
                        return false;
                }
                else if (wc.PIDType == 0 && pkm.IsShiny)
                    return false; // can't be traded away for unshiny
                if (pkm.IsEgg && !pkm.IsNative)
                    return false;
            }

            if (wc.Form != pkm.AltForm && vs.All(dl => !IsFormChangeable(pkm, dl.Species))) return false;

            if (wc.Level != pkm.Met_Level) return false;
            if (wc.Ball != pkm.Ball) return false;
            if (wc.Nature != 0xFF && wc.Nature != pkm.Nature) return false;
            if (wc.Gender != 2 && wc.Gender != pkm.Gender) return false;

            if (wc.CNT_Cool > pkm.CNT_Cool) return false;
            if (wc.CNT_Beauty > pkm.CNT_Beauty) return false;
            if (wc.CNT_Cute > pkm.CNT_Cute) return false;
            if (wc.CNT_Smart > pkm.CNT_Smart) return false;
            if (wc.CNT_Tough > pkm.CNT_Tough) return false;
            if (wc.CNT_Sheen > pkm.CNT_Sheen) return false;

            return true;
        }
        private static bool GetIsMatchWC6(PKM pkm, WC6 wc, IEnumerable<DexLevel> vs)
        {
            if (pkm.Egg_Location == 0) // Not Egg
            {
                if (wc.CardID != pkm.SID) return false;
                if (wc.TID != pkm.TID) return false;
                if (wc.OT_Name != pkm.OT_Name) return false;
                if (wc.OTGender != pkm.OT_Gender) return false;
                if (wc.PIDType == 0 && pkm.PID != wc.PID) return false;
                if (wc.PIDType == 2 && !pkm.IsShiny) return false;
                if (wc.PIDType == 3 && pkm.IsShiny) return false;
                if (wc.OriginGame != 0 && wc.OriginGame != pkm.Version) return false;
                if (wc.EncryptionConstant != 0 && wc.EncryptionConstant != pkm.EncryptionConstant) return false;
                if (wc.Language != 0 && wc.Language != pkm.Language) return false;
            }
            if (wc.Form != pkm.AltForm && vs.All(dl => !IsFormChangeable(pkm, dl.Species))) return false;

            if (wc.IsEgg)
            {
                if (wc.EggLocation != pkm.Egg_Location) // traded
                {
                    if (pkm.Egg_Location != 30002)
                        return false;
                }
                else if (wc.PIDType == 0 && pkm.IsShiny)
                    return false; // can't be traded away for unshiny
                if (pkm.IsEgg && !pkm.IsNative)
                    return false;
            }
            else
            {
                if (wc.EggLocation != pkm.Egg_Location) return false;
                if (wc.MetLocation != pkm.Met_Location) return false;
            }

            if (wc.Level != pkm.Met_Level) return false;
            if (wc.Ball != pkm.Ball) return false;
            if (wc.OTGender < 3 && wc.OTGender != pkm.OT_Gender) return false;
            if (wc.Nature != 0xFF && wc.Nature != pkm.Nature) return false;
            if (wc.Gender != 3 && wc.Gender != pkm.Gender) return false;

            if (wc.CNT_Cool > pkm.CNT_Cool) return false;
            if (wc.CNT_Beauty > pkm.CNT_Beauty) return false;
            if (wc.CNT_Cute > pkm.CNT_Cute) return false;
            if (wc.CNT_Smart > pkm.CNT_Smart) return false;
            if (wc.CNT_Tough > pkm.CNT_Tough) return false;
            if (wc.CNT_Sheen > pkm.CNT_Sheen) return false;

            return true;
        }
        private static bool GetIsMatchWC7(PKM pkm, WC7 wc, IEnumerable<DexLevel> vs)
        {
            if (pkm.Egg_Location == 0) // Not Egg
            {
                if (wc.OTGender != 3)
                {
                    if (wc.SID != pkm.SID) return false;
                    if (wc.TID != pkm.TID) return false;
                    if (wc.OTGender != pkm.OT_Gender) return false;
                }
                if (!string.IsNullOrEmpty(wc.OT_Name) && wc.OT_Name != pkm.OT_Name) return false;
                if (wc.OriginGame != 0 && wc.OriginGame != pkm.Version) return false;
                if (wc.EncryptionConstant != 0 && wc.EncryptionConstant != pkm.EncryptionConstant) return false;
                if (wc.Language != 0 && wc.Language != pkm.Language) return false;
            }
            if (wc.Form != pkm.AltForm && vs.All(dl => !IsFormChangeable(pkm, dl.Species)))
            {
                if (wc.Species == 744 && wc.Form == 1 && pkm.Species == 745 && pkm.AltForm == 2)
                {
                    // Rockruff gift edge case; has altform 1 then evolves to altform 2
                }
                else
                    return false;
            }

            if (wc.IsEgg)
            {
                if (wc.EggLocation != pkm.Egg_Location) // traded
                {
                    if (pkm.Egg_Location != 30002)
                        return false;
                }
                else if (wc.PIDType == 0 && pkm.IsShiny)
                    return false; // can't be traded away for unshiny
                if (pkm.IsEgg && !pkm.IsNative)
                    return false;
            }
            else
            {
                if (wc.EggLocation != pkm.Egg_Location) return false;
                if (wc.MetLocation != pkm.Met_Location) return false;
            }

            if (wc.MetLevel != pkm.Met_Level) return false;
            if (wc.Ball != pkm.Ball) return false;
            if (wc.OTGender < 3 && wc.OTGender != pkm.OT_Gender) return false;
            if (wc.Nature != 0xFF && wc.Nature != pkm.Nature) return false;
            if (wc.Gender != 3 && wc.Gender != pkm.Gender) return false;

            if (wc.CNT_Cool > pkm.CNT_Cool) return false;
            if (wc.CNT_Beauty > pkm.CNT_Beauty) return false;
            if (wc.CNT_Cute > pkm.CNT_Cute) return false;
            if (wc.CNT_Smart > pkm.CNT_Smart) return false;
            if (wc.CNT_Tough > pkm.CNT_Tough) return false;
            if (wc.CNT_Sheen > pkm.CNT_Sheen) return false;

            if (wc.PIDType == 2 && !pkm.IsShiny) return false;
            if (wc.PIDType == 3 && pkm.IsShiny) return false;

            switch (wc.CardID)
            {
                case 1624: // Rockruff
                    if (pkm.Species == 745 && pkm.AltForm != 2)
                        return false;
                    if (pkm.Version == (int)GameVersion.US)
                        return wc.Move3 == 424; // Fire Fang
                    if (pkm.Version == (int)GameVersion.UM)
                        return wc.Move3 == 422; // Thunder Fang
                    return false;
                case 2046: // Ash Greninja
                    return pkm.SM; // not USUM
            }
            return true;
        }

        // Utility
        private static bool IsRangerManaphy(PKM pkm)
        {
            var egg = pkm.Egg_Location;
            const int ranger = 3001;
            const int linkegg = 2002;
            if (!pkm.IsEgg) // Link Trade Egg or Ranger
                return egg == linkegg || egg == ranger;
            if (egg != ranger)
                return false;
            var met = pkm.Met_Location;
            return met == linkegg || met == 0;
        }
    }
}
