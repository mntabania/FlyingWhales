namespace Characters.Villager_Wants {
    public class ArmorWant : VillagerWant {
        public override int priority => 6;
        public override string name => "Armor";
        public override bool CanVillagerObtainWant(Character p_character) {
            if (!CharacterHasFaction(p_character)) return false;
            if (!CharacterLivesInAVillage(p_character)) return false;
            
            if (!HasWorkshopInSameVillageOwnedByValidCharacter(p_character, out bool needsToPay)) {
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
            //character has no equipped armor yet.
            return p_character.equipmentComponent.currentArmor == null;
        }
    }
}