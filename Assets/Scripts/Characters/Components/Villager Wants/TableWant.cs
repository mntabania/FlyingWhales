using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Characters.Villager_Wants {
    public class TableWant : FurnitureWant {
        public override int priority => 10;
        public override string name => "Table";
        public override TILE_OBJECT_TYPE furnitureWanted => TILE_OBJECT_TYPE.TABLE;
        public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure) {
            if (!CharacterHasFaction(p_character)) {
                p_preferredStructure = null;
                return false;
            }
            if (!CharacterLivesInAVillage(p_character)) {
                p_preferredStructure = null;
                return false;
            }
            if (!CharacterLivesInADwelling(p_character)) {
                p_preferredStructure = null;
                return false;
            }

            if (!HasBasicResourceProducingStructureInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay, out p_preferredStructure)) {
                //could not find basic resource producing structure that is owned by a valid character
                return false;
            }
            if (needsToPay && !p_character.moneyComponent.CanAfford(10)) {
                //the character must pay for the goods and cannot afford it.
                return false;
            }
            return true;
        }
        
        public override bool IsWantValid(Character p_character) {
            if (!CharacterLivesInADwelling(p_character)) return false;
            return !p_character.homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.TABLE);
        }
    }
}