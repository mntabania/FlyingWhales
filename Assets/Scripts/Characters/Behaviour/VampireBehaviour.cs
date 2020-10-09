using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class VampireBehaviour : CharacterBehaviourComponent {

    public VampireBehaviour() {
        priority = 40;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        if (character.characterClass.className == "Vampire Lord") {
            if (character.homeStructure == null || character.homeStructure.structureType != STRUCTURE_TYPE.VAMPIRE_CASTLE) {
                if (character.homeSettlement != null) {
                    LocationStructure unoccupiedCastle = character.homeSettlement.GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE.VAMPIRE_CASTLE);
                    if (unoccupiedCastle != null) {
                        //TODO: Transfer home    
                    } else if (GameUtilities.RollChance(15)){
                        //TODO: Build vampire castle
                    }
                } else {
                    LocationStructure unoccupiedCastle = GetFirstNonSettlementVampireCastles(character);
                    if (unoccupiedCastle != null) {
                        //TODO: Transfer home
                    } else if (GameUtilities.RollChance(15)){
                        //TODO: Build vampire castle
                    }
                }
            }

            if (character.homeStructure != null) {
                //TODO: Prisoner
            }
        } else {
            //TODO: Add checking for number of embraced characters
            // //Become vampire lord
            // character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Vampire_Lord, character);
            // producedJob = null;
            // return true;
        }

        if (character.needsComponent.isSulking) {
            if (GameUtilities.RollChance(3)) {
                WeightedDictionary<Character> embraceWeights = new WeightedDictionary<Character>();
                foreach (var relationships in character.relationshipContainer.relationships) {
                    Character otherCharacter = DatabaseManager.Instance.characterDatabase.GetCharacterByID(relationships.Key);
                    if (otherCharacter != null) {
                        if (!otherCharacter.traitContainer.HasTrait("Vampire") && relationships.Value.awareness.state != AWARENESS_STATE.Presumed_Dead && 
                            relationships.Value.awareness.state != AWARENESS_STATE.Missing && !otherCharacter.partyComponent.isActiveMember) {
                            var opinionLabel = relationships.Value.opinions.GetOpinionLabel();
                            if (relationships.Value.IsLover() && (opinionLabel == RelationshipManager.Close_Friend || opinionLabel == RelationshipManager.Friend)) {
                                embraceWeights.AddElement(otherCharacter, 100);
                            } else if (relationships.Value.HasRelationship(RELATIONSHIP_TYPE.AFFAIR) && (opinionLabel == RelationshipManager.Close_Friend || opinionLabel == RelationshipManager.Friend)) {
                                embraceWeights.AddElement(otherCharacter, 50);
                            } else if (opinionLabel == RelationshipManager.Close_Friend) {
                                embraceWeights.AddElement(otherCharacter, 50);
                            } else if (opinionLabel == RelationshipManager.Friend) {
                                embraceWeights.AddElement(otherCharacter, 10);
                            } else if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                                embraceWeights.AddElement(otherCharacter, 5);
                            }
                        }
                    }
                }

                if (embraceWeights.GetTotalOfWeights() > 0) {
                    Character chosenEmbraceTarget = embraceWeights.PickRandomElementGivenWeights();
                    //TODO: Embrace target
                }
            }
        }
        
        producedJob = null;
        return false;
    }

    private LocationStructure GetFirstNonSettlementVampireCastles(Character character) {
        List<Region> regionsToCheck = new List<Region> {character.currentRegion};
        regionsToCheck.AddRange(character.currentRegion.neighbours);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region region = regionsToCheck[i];
            if (region.HasStructure(STRUCTURE_TYPE.VAMPIRE_CASTLE)) {
                List<LocationStructure> vampireCastles = region.GetStructuresAtLocation<LocationStructure>(STRUCTURE_TYPE.VAMPIRE_CASTLE);
                for (int j = 0; j < vampireCastles.Count; j++) {
                    LocationStructure structure = vampireCastles[j];
                    if (structure.settlementLocation == null || structure.settlementLocation.locationType != LOCATION_TYPE.VILLAGE) {
                        return structure;
                    }
                }
            }
        }
        return null;
    }
}
