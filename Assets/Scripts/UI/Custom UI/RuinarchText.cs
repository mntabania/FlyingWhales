using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RuinarchText : TextMeshProUGUI {
    
    public new void SetText(string text) {
        if (text.Length <= 300) {
            //only replace text if text length is less than X amount. This is for performance reasons
            //villager
            if (text.Contains("Villager")) {
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
            if (text.Contains("mana")) {
                text = text.Replace("mana", $"{UtilityScripts.Utilities.ManaIcon()}mana");
            }
            //charges
            if (text.Contains("Charges")) {
                text = text.Replace("Charges", $"{UtilityScripts.Utilities.ChargesIcon()}Charges");
            }
            if (text.Contains("charges")) {
                text = text.Replace("charges", $"{UtilityScripts.Utilities.ChargesIcon()}charges");
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
        }
        
        base.SetText(text);
    }    
}
