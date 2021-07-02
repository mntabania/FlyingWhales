using Inner_Maps.Location_Structures;
namespace Characters.Villager_Wants {
    public class FoodWant_2 : FoodWant {
        public override int priority => 4;
        public override string name => "Food Want 2";
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
            if (!CharacterLivesInADwelling(p_character)) return false;
            return p_character.homeStructure is Dwelling dwelling && dwelling.differentFoodPileKindsInDwelling < 2;
        }

        public override void OnWantToggledOn(Character p_character) {
            //character no longer has at least 2 types of food at home
            p_character.traitContainer.RemoveTrait(p_character, "Stocked Up");
        }
        public override void OnWantToggledOff(Character p_character) {
            //character has at least 2 types of food at home
            if (!CharacterLivesInADwelling(p_character)) {
                p_character.traitContainer.RemoveTrait(p_character, "Stocked Up");
                return;
            }
            if (!CharacterHasFaction(p_character)) {
                p_character.traitContainer.RemoveTrait(p_character, "Stocked Up");
                return;
            }
            if (!CharacterLivesInAVillage(p_character)) {
                p_character.traitContainer.RemoveTrait(p_character, "Stocked Up");
                return;
            }
            p_character.traitContainer.AddTrait(p_character, "Stocked Up");
        }
    }
}