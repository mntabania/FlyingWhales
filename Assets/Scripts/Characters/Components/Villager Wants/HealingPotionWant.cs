namespace Characters.Villager_Wants {
    public class HealingPotionWant : VillagerWant {
        public override int priority => 7;
        public override string name => "Healing Potion";
        public override bool CanVillagerObtainWant(Character p_character) {
            if (!CharacterHasFaction(p_character)) return false;
            if (!CharacterLivesInAVillage(p_character)) return false;
            
            if (!HasHospiceInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay)) {
                //could not find hospice structure that is owned by a valid character
                return false;
            }
            
            if (needsToPay && !p_character.moneyComponent.CanAfford(5)) {
                //the character must pay for the goods and cannot afford it.
                return false;
            }
            return true;
        }
        public override bool IsWantValid(Character p_character) {
            return !p_character.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
        }
    }
}