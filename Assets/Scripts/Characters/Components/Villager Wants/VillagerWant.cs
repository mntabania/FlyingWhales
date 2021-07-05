using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Characters.Villager_Wants {
    public abstract class VillagerWant {
        /// <summary>
        /// The priority of this want. This determines which want is processed first in getting a characters want.
        /// Higher priorities get processed first.
        /// <see cref="VillagerWantsComponent.GetTopPriorityWant"/>
        /// </summary>
        public abstract int priority { get; }
        public abstract string name { get; }
        public abstract bool CanVillagerObtainWant(Character p_character, out LocationStructure p_preferredStructure);

        public virtual void OnWantToggledOn(Character p_character) { }
        public virtual void OnWantToggledOff(Character p_character) { }
        
        #region Validity
        /// <summary>
        /// This function is used to determine if the character should still process this want or not.
        /// This will usually return false if the character has already fulfilled this want.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsWantValid(Character p_character);
        #endregion

        #region Utilities
        protected bool CharacterLivesInADwelling(Character p_character) {
            if (p_character.homeStructure == null || p_character.homeStructure.structureType != STRUCTURE_TYPE.DWELLING) {
                //the character must live in a dwelling
                return false;
            }
            return true;
        }
        protected bool CharacterLivesInAVillage(Character p_character) {
            if (p_character.homeSettlement == null || p_character.homeSettlement.locationType != LOCATION_TYPE.VILLAGE) {
                //the character must live in a village
                return false;
            }
            return true;
        }
        protected bool CharacterHasFaction(Character p_character) {
            if (p_character.faction == null || !p_character.faction.isMajorFaction) {
                //the must character has a faction
                return false;
            }
            return true;
        }
        /// <summary>
        /// Is the provided character part of a village that has a basic resource producing structure that it can purchase from?
        /// </summary>
        /// <param name="p_character">The character to check.</param>
        /// <param name="needsToPay">Does this character need to pay for the goods.</param>
        /// <param name="foundStructure">The found basic resource structure</param>
        protected bool HasBasicResourceProducingStructureInSameVillageOwnedByValidCharacter(Character p_character, out bool needsToPay, out LocationStructure foundStructure) {
            Assert.IsNotNull(p_character.homeSettlement);
            Assert.IsTrue(p_character.faction.isMajorFaction);
            if (!p_character.homeSettlement.HasBasicResourceProducingStructure()) {
                needsToPay = true;
                foundStructure = null;
                return false;
            }
            if (p_character.structureComponent.HasWorkPlaceStructure() && 
                p_character.structureComponent.workPlaceStructure.structureType.IsBasicResourceProducingStructureForFaction(p_character.faction.factionType.type)) {
                bool hasNeededResource = false;
                if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
                    hasNeededResource = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE);
                } else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                    hasNeededResource = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
                } else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
                           p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                    hasNeededResource = p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE) || p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
                }
                if (hasNeededResource) {
                    //character works at a basic resource producing structure
                    needsToPay = true;
                    foundStructure = p_character.structureComponent.workPlaceStructure;
                    return true;    
                }
            }
            
            List<LocationStructure> basicResourceProducingStructures = RuinarchListPool<LocationStructure>.Claim();
            if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
                p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                List<LocationStructure> mines = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.MINE);
                if (mines != null) { basicResourceProducingStructures.AddRange(mines); }    
            }
            
            if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom || p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
                p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                List<LocationStructure> lumberyards = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.LUMBERYARD);
                if (lumberyards != null) { basicResourceProducingStructures.AddRange(lumberyards); }    
            }

            foundStructure = null;
            needsToPay = true;
            // int highestOpinion = Int32.MinValue;
            // for (int i = 0; i < basicResourceProducingStructures.Count; i++) {
            //     LocationStructure structure = basicResourceProducingStructures[i];
            //     ManMadeStructure manMadeStructure = structure as ManMadeStructure;
            //     Assert.IsNotNull(manMadeStructure, $"Basic Resource producing structure is not Man made! {structure?.name}");
            //     bool hasNeededResource = false;
            //     if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
            //         hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE);
            //     } else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
            //         hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
            //     } else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
            //                p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
            //         hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE) || manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
            //     }
            //     if (hasNeededResource && manMadeStructure.CanPurchaseFromHere(p_character, out bool needsToPayAtCurrentStructure, out int buyerOpinionOfWorker)) {
            //         if (buyerOpinionOfWorker > highestOpinion) {
            //             preferredStructure = manMadeStructure;    
            //             needsToPay = needsToPayAtCurrentStructure;
            //         }
            //         // foundStructure = manMadeStructure;
            //         // if (!needsToPay) {
            //         //     //if character found a structure that he/she doesn't need to pay at, break this loop,
            //         //     //otherwise continue loop in case this character can find a structure where it doesn't have to pay
            //         //     break;
            //         // }
            //     }
            // }
            if (basicResourceProducingStructures.Count > 0) {
                //villagers can now buy from any basic resource producing structure and are required to pay regardless of situation.
                basicResourceProducingStructures.Shuffle();
                for (int i = 0; i < basicResourceProducingStructures.Count; i++) {
                    LocationStructure structure = basicResourceProducingStructures[i];
                    ManMadeStructure manMadeStructure = structure as ManMadeStructure;
                    Assert.IsNotNull(manMadeStructure, $"Food producing structure is not Man made! {structure?.name}");
                    bool hasNeededResource = false;
                    if (p_character.faction.factionType.type == FACTION_TYPE.Human_Empire) {
                        hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE);
                    } else if (p_character.faction.factionType.type == FACTION_TYPE.Elven_Kingdom) {
                        hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
                    } else if (p_character.faction.factionType.type == FACTION_TYPE.Demon_Cult ||
                               p_character.faction.factionType.type == FACTION_TYPE.Lycan_Clan || p_character.faction.factionType.type == FACTION_TYPE.Vampire_Clan) {
                        hasNeededResource = manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.STONE_PILE) || manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.WOOD_PILE);
                    }
                    if (hasNeededResource) {
                        foundStructure = manMadeStructure;
                        break;
                    }
                }
            }
            
            RuinarchListPool<LocationStructure>.Release(basicResourceProducingStructures);
            return foundStructure != null;
        }
        /// <summary>
        /// Is the provided character part of a village that has a workshop that it can purchase from?
        /// </summary>
        /// <param name="p_character">The character to check.</param>
        /// <param name="needsToPay">Does this character need to pay for the goods.</param>
        /// <param name="preferredStructure">The Found Workshop</param>
        protected bool HasWorkshopInSameVillageOwnedByValidCharacter(Character p_character, out bool needsToPay, out LocationStructure preferredStructure) {
            Assert.IsNotNull(p_character.homeSettlement);
            Assert.IsTrue(p_character.faction.isMajorFaction);
            preferredStructure = null;
            if (!p_character.homeSettlement.HasStructure(STRUCTURE_TYPE.WORKSHOP)) {
                needsToPay = true;
                return false;
            }
            if (p_character.structureComponent.HasWorkPlaceStructure() && 
                p_character.structureComponent.workPlaceStructure.structureType == STRUCTURE_TYPE.WORKSHOP) {
                //character works at a workshop
                needsToPay = false;
                preferredStructure = p_character.structureComponent.workPlaceStructure;
                return true;
            }
            
            List<LocationStructure> workshops = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.WORKSHOP);
            
            needsToPay = true;
            int highestOpinion = Int32.MinValue;
            for (int i = 0; i < workshops.Count; i++) {
                LocationStructure structure = workshops[i];
                ManMadeStructure manMadeStructure = structure as ManMadeStructure;
                Assert.IsNotNull(manMadeStructure, $"Workshop is not Man made! {structure?.name}");
                if (manMadeStructure.CanPurchaseFromHere(p_character, out bool needsToPayAtCurrentStructure, out int buyerOpinionOfWorker)) {
                    if (buyerOpinionOfWorker > highestOpinion) {
                        highestOpinion = buyerOpinionOfWorker;
                        preferredStructure = manMadeStructure;
                        needsToPay = needsToPayAtCurrentStructure;
                    }
                    // foundStructure = manMadeStructure;
                    // if (!needsToPay) {
                    //     //if character found a structure that he/she doesn't need to pay at, break this loop,
                    //     //otherwise continue loop in case this character can find a structure where it doesn't have to pay
                    //     break;
                    // }
                }
            }
            return preferredStructure != null;
        }
        /// <summary>
        /// Is the provided character part of a village that has a workshop that it can purchase from?
        /// </summary>
        /// <param name="p_character">The character to check.</param>
        /// <param name="needsToPay">Does this character need to pay for the goods.</param>
        /// <param name="preferredStructure">The Found Hospice</param>
        protected bool HasHospiceInSameVillageOwnedByValidCharacter(Character p_character, out bool needsToPay, out LocationStructure preferredStructure) {
            Assert.IsNotNull(p_character.homeSettlement);
            Assert.IsTrue(p_character.faction.isMajorFaction);
            if (!p_character.homeSettlement.HasStructure(STRUCTURE_TYPE.HOSPICE)) {
                needsToPay = true;
                preferredStructure = null;
                return false;
            }
            if (p_character.structureComponent.HasWorkPlaceStructure() && 
                p_character.structureComponent.workPlaceStructure.structureType == STRUCTURE_TYPE.HOSPICE &&
                p_character.structureComponent.workPlaceStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.HEALING_POTION)) {
                //character works at a hospice
                needsToPay = false;
                preferredStructure = p_character.structureComponent.workPlaceStructure;
                return true;
            }
            
            List<LocationStructure> hospice = p_character.homeSettlement.GetStructuresOfType(STRUCTURE_TYPE.HOSPICE);

            preferredStructure = null;
            needsToPay = true;
            int highestOpinion = Int32.MinValue;
            for (int i = 0; i < hospice.Count; i++) {
                LocationStructure structure = hospice[i];
                ManMadeStructure manMadeStructure = structure as ManMadeStructure;
                Assert.IsNotNull(manMadeStructure, $"Workshop is not Man made! {structure?.name}");
                if (manMadeStructure.HasBuiltTileObjectOfType(TILE_OBJECT_TYPE.HEALING_POTION) && 
                    manMadeStructure.CanPurchaseFromHere(p_character, out bool needsToPayAtCurrentStructure, out int buyerOpinionOfWorker)) {
                    if (buyerOpinionOfWorker > highestOpinion) {
                        highestOpinion = buyerOpinionOfWorker;
                        preferredStructure = manMadeStructure;
                        needsToPay = needsToPayAtCurrentStructure;
                    }
                    // foundStructure = manMadeStructure;
                    // if (!needsToPay) {
                    //     //if character found a structure that he/she doesn't need to pay at, break this loop,
                    //     //otherwise continue loop in case this character can find a structure where it doesn't have to pay
                    //     break;
                    // }
                }
            }
            return preferredStructure != null;
        }
        #endregion

        #region For Testing
        public override string ToString() {
            return name;
        }
        #endregion
    }
}