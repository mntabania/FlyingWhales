namespace Characters.Villager_Wants {
    public class BedWant : VillagerWant {
        public override int priority => 9;
        public override string name => "Bed";
        public override bool CanVillagerObtainWant(Character p_character) {
            if (!CharacterHasFaction(p_character)) return false;
            if (!CharacterLivesInAVillage(p_character)) return false;
            if (!CharacterLivesInADwelling(p_character)) return false;
            
            if (!HasBasicResourceProducingStructureInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay)) {
                //could not find basic resource producing structure that is owned by a valid character
                return false;
            }
            if (needsToPay && !p_character.moneyComponent.CanAfford(20)) {
                //the character must pay for the goods and cannot afford it.
                return false;
            }
            return true;
        }
        public override bool IsWantValid(Character p_character) {
            if (!CharacterLivesInADwelling(p_character)) return false;
            
            //character has no bed yet.
            return !p_character.homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.BED);
        }
    }
}