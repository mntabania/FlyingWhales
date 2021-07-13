using Inner_Maps.Location_Structures;
namespace Characters.Villager_Wants {
    public class FoodWant_3 : FoodWant {
        public override int priority => 1;
        public override string name => "Food Want 3";
        public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure) {
            if (!CharacterHasFaction(p_character)) {
                p_preferredStructure = null;
                return false;
            }
            if (!CharacterLivesInAVillage(p_character)) {
                p_preferredStructure = null;
                return false;
            }
            if (!CharacterLivesInAValidHome(p_character)) {
                p_preferredStructure = null;
                return false;
            }

            if (!HasFoodProducingStructureInSameVillageOwnedByValidCharacter(p_character, out var needsToPay, out p_preferredStructure)) {
                //could not find food producing structure that is owned by a valid character
                return false;
            }
            if (needsToPay && !p_character.moneyComponent.CanAfford(10)) {
                //the character must pay for the goods and cannot afford it.
                return false;
            }
            
            return true;
        }
        
        public override bool IsWantValid(Character p_character) {
            if (p_character.traitContainer.HasTrait("Vampire")) return false;
            if (!CharacterLivesInAValidHome(p_character)) return false;
            return p_character.homeStructure is Dwelling dwelling && dwelling.differentFoodPileKindsInDwelling < 3;
        }
    }
}