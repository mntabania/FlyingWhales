using Inner_Maps.Location_Structures;
namespace Characters.Villager_Wants {
    public class DwellingWant : VillagerWant {
        public override int priority => 12;
        public override string name => "Dwelling";
        
        public override bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure) {
            p_preferredStructure = null;
            if (!p_character.moneyComponent.CanAfford(50)) {
                //requires 50 coins
                return false;
            }
            if (p_character.faction == null || !p_character.faction.isMajorFaction) {
                //the character has a faction
                return false;
            }
            if (p_character.homeSettlement == null || p_character.homeSettlement.locationType != LOCATION_TYPE.VILLAGE) {
                //the character lives in a village
                return false;
            }
            if (!p_character.homeSettlement.HasUnclaimedDwellingThatIsNotPreviousHome(p_character, out p_preferredStructure)) {
                //there is an unclaimed dwelling in the village
                return false;
            }
            return true;
        }
        
        public override bool IsWantValid(Character p_character) {
            return !CharacterLivesInAValidHome(p_character);
        }
    }
}