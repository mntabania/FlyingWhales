namespace Characters.Villager_Wants {
    public class AccessoryWant : VillagerWant {
        public override int priority => 3;
        public override string name => "Accessory";
        public override bool CanVillagerObtainWant(Character p_character) {
            if (!CharacterHasFaction(p_character)) return false;
            if (!CharacterLivesInAVillage(p_character)) return false;
            
            if (!HasWorkshopInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay)) {
                //could not find workshop that is owned by a valid character
                return false;
            }
            if (needsToPay && !p_character.moneyComponent.CanAfford(50)) {
                //the character must pay for the goods and cannot afford it.
                return false;
            }
            return true;
        }
        public override bool IsWantValid(Character p_character) {
            //character has no equipped accessory yet.
            return p_character.equipmentComponent.currentAccessory == null;
        }
    }
}