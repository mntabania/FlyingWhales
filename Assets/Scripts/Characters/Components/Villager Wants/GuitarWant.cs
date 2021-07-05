using Inner_Maps.Location_Structures;
namespace Characters.Villager_Wants {
    public class GuitarWant : FurnitureWant {
        public override int priority => 2;
        public override string name => "Guitar";
        public override TILE_OBJECT_TYPE furnitureWanted => TILE_OBJECT_TYPE.GUITAR;
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
            TileObjectData tileObjectData = TileObjectDB.GetTileObjectData(furnitureWanted);
            if (!HasBasicResourceProducingStructureInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay, out p_preferredStructure, tileObjectData.craftResourceCost)) {
                //could not find basic resource producing structure that is owned by a valid character
                return false;
            }
            if (needsToPay && !p_character.moneyComponent.CanAfford(30)) {
                //the character must pay for the goods and cannot afford it.
                return false;
            }
            return true;
        }
        public override bool IsWantValid(Character p_character) {
            if (!CharacterLivesInADwelling(p_character)) return false;
            if (p_character.traitContainer.HasTrait("Music Hater")) return false;
            return !p_character.homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.GUITAR);
        }
    }
}