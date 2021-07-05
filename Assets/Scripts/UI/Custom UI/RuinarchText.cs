using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RuinarchText : TextMeshProUGUI {
    
    public void SetTextAndReplaceWithIcons(string text) {
        if (text.Length <= 300) {
            //only replace text if text length is less than X amount. This is for performance reasons
            //villager
            if (text.Contains("Villager") && !text.Contains("Villager_")) {
                text = text.Replace("Villager", $"{UtilityScripts.Utilities.VillagerIcon()}Villager");
            }
            if (text.Contains("villager")) {
                text = text.Replace("villager", $"{UtilityScripts.Utilities.VillagerIcon()}villager");
            }
            //monster
            if (text.Contains("Monster")) {
                text = text.Replace("Monster", $"{UtilityScripts.Utilities.MonsterIcon()}Monster");
            }
            if (text.Contains("monster")) {
                text = text.Replace("monster", $"{UtilityScripts.Utilities.MonsterIcon()}monster");
            }
            //mana
            if (text.Contains("Mana")) {
                text = text.Replace("Mana", $"{UtilityScripts.Utilities.ManaIcon()}Mana");
            }
            if (text.Contains("mana ")) {
                text = text.Replace("mana", $"{UtilityScripts.Utilities.ManaIcon()}mana ");
            }
            //charges
            if (text.Contains("Bonus Charges")) {
                text = text.Replace("Bonus Charges", $"{UtilityScripts.Utilities.BonusChargesIcon()}Bonus Charges");
            } else if (text.Contains("bonus charges")) {
                text = text.Replace("bonus charges", $"{UtilityScripts.Utilities.BonusChargesIcon()}bonus charges");
            } else {
                if (text.Contains("Charges")) {
                    text = text.Replace("Charges", $"{UtilityScripts.Utilities.ChargesIcon()}Charges");
                }
                if (text.Contains("charges")) {
                    text = text.Replace("charges", $"{UtilityScripts.Utilities.ChargesIcon()}charges");
                }    
            }

            //threat
            if (text.Contains("Threat")) {
                text = text.Replace("Threat", $"{UtilityScripts.Utilities.ThreatIcon()}Threat");
            }
            if (text.Contains("threat")) {
                text = text.Replace("threat", $"{UtilityScripts.Utilities.ThreatIcon()}threat");
            }
            //cooldown
            if (text.Contains("Cooldown")) {
                text = text.Replace("Cooldown", $"{UtilityScripts.Utilities.CooldownIcon()}Cooldown");
            }
            if (text.Contains("cooldown")) {
                text = text.Replace("cooldown", $"{UtilityScripts.Utilities.CooldownIcon()}cooldown");
            }    
            //undead
            if (text.Contains("Undead")) {
                text = text.Replace("Undead", $"{UtilityScripts.Utilities.UndeadIcon()}Undead");
            }
            if (text.Contains("undead")) {
                text = text.Replace("undead", $"{UtilityScripts.Utilities.UndeadIcon()}undead");
            }
            ////village
            //if (text.Contains("Village")) {
            //    text = text.Replace("Village", $"{UtilityScripts.Utilities.VillageIcon()}Village");
            //}
            //if (text.Contains("village")) {
            //    text = text.Replace("village", $"{UtilityScripts.Utilities.VillageIcon()}village");
            //}
            //object
            //if (text.Contains("Object")) {
            //    text = text.Replace("Object", $"{UtilityScripts.Utilities.TileObjectIcon()}Object");
            //}
            //if (text.Contains("object")) {
            //    text = text.Replace("object", $"{UtilityScripts.Utilities.TileObjectIcon()}object");
            //}
            //structure
            //if (text.Contains("Structure")) {
            //    text = text.Replace("Structure", $"{UtilityScripts.Utilities.StructureIcon()}Structure");
            //}
            //if (text.Contains("structure")) {
            //    text = text.Replace("structure", $"{UtilityScripts.Utilities.StructureIcon()}structure");
            //}
            //Elements
            if (text.Contains("Water")) {
                text = text.Replace("Water", $"{UtilityScripts.Utilities.WaterIcon()}Water");
            }
            if (text.Contains("Fire")) {
                text = text.Replace("Fire", $"{UtilityScripts.Utilities.FireIcon()}Fire");
            }
            if (text.Contains("Earth")) {
                text = text.Replace("Earth", $"{UtilityScripts.Utilities.EarthIcon()}Earth");
            }
            if (text.Contains("Poison")) {
                text = text.Replace("Poison", $"{UtilityScripts.Utilities.PoisonIcon()}Poison");
            }
            if (text.Contains("Ice")) {
                text = text.Replace("Ice", $"{UtilityScripts.Utilities.IceIcon()}Ice");
            }
            if (text.Contains("Wind")) {
                text = text.Replace("Wind", $"{UtilityScripts.Utilities.WindIcon()}Wind");
            }
            if (text.Contains("Electric")) {
                text = text.Replace("Electric", $"{UtilityScripts.Utilities.ElectricIcon()}Electric");
            }
        }
        
        base.SetText(text);
    }    
}
