﻿using Inner_Maps.Location_Structures;
namespace Characters.Villager_Wants {
    public class WeaponWant : EquipmentWant {
        public override int priority => 8;
        public override string name => "Weapon";
        public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure) {
            if (!CharacterHasFaction(p_character)) {
                p_preferredStructure = null;
                return false;
            }
            if (!CharacterLivesInAVillage(p_character)) {
                p_preferredStructure = null;
                return false;
            }
            
            if (!HasWorkshopInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay, out p_preferredStructure)) {
                //could not find workshop that is owned by a valid character
                return false;
            }
            if (needsToPay && !p_character.moneyComponent.CanAfford(30)) {
                //the character must pay for the goods and cannot afford it.
                return false;
            }
            return true;
        }
        
        public override bool IsWantValid(Character p_character) {
            //character has no equipped weapon yet.
            return p_character.equipmentComponent.currentWeapon == null;
        }
    }
}