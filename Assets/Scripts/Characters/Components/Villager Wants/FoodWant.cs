using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Characters.Villager_Wants {
    public abstract class FoodWant : VillagerWant {
        // public override bool CanVillagerObtainWant(Character p_character) {
        //     if (!CharacterHasFaction(p_character)) return false;
        //     if (!CharacterLivesInAVillage(p_character)) return false;
        //     if (!CharacterLivesInADwelling(p_character)) return false;
        //     
        //     if (p_character.homeStructure.HasTileObjectThatIsBuiltFoodPile()) {
        //         //the character's dwelling should not have any food pile.
        //         return false;
        //     }
        //
        //     if (!HasFoodProducingStructureInSameVillageOwnedByValidCharacter(p_character, out var needsToPay)) {
        //         //could not find food producing structure that is owned by a valid character
        //         return false;
        //     }
        //     if (needsToPay && !p_character.moneyComponent.CanAfford(10)) {
        //         //the character must pay for the goods and cannot afford it.
        //         return false;
        //     }
        //     
        //     return true;
        // }
        /// <summary>
        /// Is the provided character part of a village that has a food producing structure that it can purchase from?
        /// </summary>
        /// <param name="p_character">The character to check.</param>
        /// <param name="needsToPay">Does this character need to pay for the goods.</param>
        /// /// <param name="foundStructure">The found food producing structure</param>
        protected bool HasFoodProducingStructureInSameVillageOwnedByValidCharacter(Character p_character, out bool needsToPay, out LocationStructure foundStructure) {
            Assert.IsNotNull(p_character.homeSettlement);
            if (!p_character.homeSettlement.HasFoodProducingStructure()) {
                needsToPay = true;
                foundStructure = null;
                return false;
            }
            if (p_character.structureComponent.HasWorkPlaceStructure() && p_character.structureComponent.workPlaceStructure.structureType.IsFoodProducingStructure() &&
                p_character.structureComponent.workPlaceStructure.HasTileObjectThatIsBuiltFoodPileThatCharacterDoesntHaveAtHome(p_character)) {
                //character works at a food producing structure
                needsToPay = false;
                foundStructure = p_character.structureComponent.workPlaceStructure;
                return true;
            }
            
            List<LocationStructure> foodProducingStructures = RuinarchListPool<LocationStructure>.Claim();
            List<LocationStructure> huntersLodge = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.HUNTER_LODGE);
            if (huntersLodge != null) { foodProducingStructures.AddRange(huntersLodge); }
            List<LocationStructure> farm = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.FARM);
            if (farm != null) { foodProducingStructures.AddRange(farm); }
            List<LocationStructure> fishery = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.FISHERY);
            if (fishery != null) { foodProducingStructures.AddRange(fishery); }

            foundStructure = null;
            needsToPay = true;
            for (int i = 0; i < foodProducingStructures.Count; i++) {
                LocationStructure structure = foodProducingStructures[i];
                ManMadeStructure manMadeStructure = structure as ManMadeStructure;
                Assert.IsNotNull(manMadeStructure, $"Food producing structure is not Man made! {structure?.name}");
                if (manMadeStructure.HasTileObjectThatIsBuiltFoodPileThatCharacterDoesntHaveAtHome(p_character) && manMadeStructure.CanPurchaseFromHereBasedOnAssignedWorker(p_character, out needsToPay)) {
                    foundStructure = manMadeStructure;
                    if (!needsToPay) {
                        //if character found a structure that he/she doesn't need to pay at, break this loop,
                        //otherwise continue loop in case this character can find a structure where it doesn't have to pay
                        break;
                    }
                }
            }
            RuinarchListPool<LocationStructure>.Release(foodProducingStructures);
            return foundStructure != null;
        }
    }
}