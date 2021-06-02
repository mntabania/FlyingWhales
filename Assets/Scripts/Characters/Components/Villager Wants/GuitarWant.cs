namespace Characters.Villager_Wants {
    public class GuitarWant : VillagerWant {
        public override int priority => 2;
        public override string name => "Guitar";
        public override bool CanVillagerObtainWant(Character p_character) {
            if (!CharacterHasFaction(p_character)) return false;
            if (!CharacterLivesInAVillage(p_character)) return false;
            if (!CharacterLivesInADwelling(p_character)) return false;
            
            if (!HasBasicResourceProducingStructureInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay)) {
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
            return !p_character.homeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.GUITAR);
        }
    }
}