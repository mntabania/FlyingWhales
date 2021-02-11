using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UtilityScripts;

public class VampireBehaviour : CharacterBehaviourComponent {

    public VampireBehaviour() {
        priority = 40;
    }
    
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        log += $"\n-{character.name} is a vampire";
        if ((character.moodComponent.moodState == MOOD_STATE.Bad || character.moodComponent.moodState == MOOD_STATE.Critical) && character.traitContainer.HasTrait("Hemophobic")) {
            return character.jobComponent.TriggerSuicideJob(out producedJob, "Hemophobic Vampire");
        }

        if (character.needsComponent.isHungry || character.needsComponent.isStarving) {
            TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
            if (currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                character.needsComponent.PlanFullnessRecoveryActionsVampire();
            }
        }

        if (character.characterClass.className == "Vampire Lord") {
            log += $"\n-{character.name} is a Vampire Lord";
            if (character.homeStructure == null || character.homeStructure.structureType != STRUCTURE_TYPE.VAMPIRE_CASTLE) {
                log += $"\n-{character.name} does not have a home structure or does not live at a vampire castle";
                var structureSetting = new StructureSetting(STRUCTURE_TYPE.VAMPIRE_CASTLE, RESOURCE.STONE); //character.faction.factionType.mainResource
                if (character.homeSettlement != null) {
                    log += $"\n-{character.name} has a home settlement {character.homeSettlement.name}";
                    LocationStructure unoccupiedCastle = character.homeSettlement.GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE.VAMPIRE_CASTLE);
                    if (unoccupiedCastle != null) {
                        log += $"\n-{character.homeSettlement.name} has an unoccupied vampire castle {unoccupiedCastle.name}. Setting home to that.";
                        //Transfer home
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, unoccupiedCastle.tiles.First().genericTileObject);
                        producedJob = null;
                        return true;
                    } else if (GameUtilities.RollChance(15, ref log) && character.faction?.factionType.type != FACTION_TYPE.Vagrants){ //15
                        log += $"\n-{character.homeSettlement.name} does not have an unoccupied vampire castle, and successfully rolled to build a new one";
                        //Build vampire castle
                        if (LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, structureSetting, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
                            log += $"\n-Will place dwelling blueprint {structurePrefabName} at {targetTile}.";
                            return character.jobComponent.TriggerBuildVampireCastle(targetTile, out producedJob, structurePrefabName);    
                        }    
                    }
                } else {
                    log += $"\n-{character.name} does not have a home settlement. Will try to find unoccupied vampire castles in the wild.";
                    LocationStructure unoccupiedCastle = GetFirstNonSettlementVampireCastles(character);
                    if (unoccupiedCastle != null) {
                        log += $"\n-Found unoccupied castle {unoccupiedCastle.name}";
                        //Transfer home
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, unoccupiedCastle.tiles.First().genericTileObject);
                        producedJob = null;
                        return true;
                    } else if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && GameUtilities.RollChance(15, ref log) && character.faction?.factionType.type != FACTION_TYPE.Vagrants){ //15
                        Area targetArea = GetNoStructurePlainAreaInAllRegions();
                        if (targetArea != null) {
                            log += $"\n-Could not find valid castle in wild, and successfully rolled to build a new castle at {targetArea}";
                            //Build vampire castle
                            List<GameObject> choices = InnerMapManager.Instance.GetIndividualStructurePrefabsForStructure(structureSetting);
                            GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
                            return character.jobComponent.TriggerBuildVampireCastle(targetArea.gridTileComponent.centerGridTile, out producedJob, chosenStructurePrefab.name);    
                        } else {
                            log += $"\n-Could not find valid Area in wild to build a vampire castle.";
                        }
                    }
                }
            }

            if (character.homeStructure != null) {
                log += $"\n-{character.name} has a home. Will check if it has a prisoner there.";
                bool hasPrisonerAtHome = false;
                for (int i = 0; i < character.homeStructure.charactersHere.Count; i++) {
                    Character otherCharacter = character.homeStructure.charactersHere[i];
                    Prisoner prisoner = otherCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                    if (otherCharacter != character && prisoner != null && !otherCharacter.isDead) {
                        log += $"\n-{character.name} found a prisoner at home: {otherCharacter.name}. Will not create Imprison blood source.";
                        hasPrisonerAtHome = true;
                        break;
                    }
                }
                if (!hasPrisonerAtHome) {
                    Vampire vampire = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                    if (!vampire.dislikedBeingVampire || character.traitContainer.GetTraitsOrStatuses<Trait>("Evil", "Treacherous", "Glutton").Count > 0) {
                        if (GameUtilities.RollChance(15)) { //15
                            return character.jobComponent.TriggerImprisonBloodSource(out producedJob, ref log);
                        }
                    }
                }
            }
        } else {
            Vampire vampire = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if (!vampire.hasAlreadyBecomeVampireLord) {
                if (character.characterClass.className == "Necromancer") {
                    log += $"\n-{character.name} is not yet a vampire lord. But is a necromancer, not becoming a Vampire Lord.";
                } else if (character.characterClass.className == "Werewolf") {
                    log += $"\n-{character.name} is not yet a vampire lord. But is currently a Werewolf, not becoming a Vampire Lord.";
                } else if (character.traitContainer.HasTrait("Enslaved")) {
                    log += $"\n-{character.name} is Enslaved, not becoming a Vampire Lord.";
                } else {
                    log += $"\n-{character.name} is not yet a vampire lord. Rolling for chance to check converted villagers.";
                    if (GameUtilities.RollChance(10, ref log)) { //10
                        log += $"\n-{character.name} converted villagers are {vampire.numOfConvertedVillagers.ToString()}.";
                        if (vampire.numOfConvertedVillagers >= 3) {
                            //Become vampire lord
                            log += $"\n-{character.name} will become a vampire lord.";
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Vampire_Lord, character);
                            producedJob = null;
                            return true;    
                        }
                    }    
                }    
            }
            
        }

        if (character.needsComponent.isSulking) {
            log += $"\n-{character.name} is sulking. Rolling for chance to vampiric embrace.";
            if (GameUtilities.RollChance(3, ref log)) { //3
                WeightedDictionary<Character> embraceWeights = GetVampiricEmbraceTargetWeights(character);
                log += $"\n-{embraceWeights.GetWeightsSummary("Vampiric Embrace weights:")}.";
                if (embraceWeights.GetTotalOfWeights() > 0) {
                    Character chosenEmbraceTarget = embraceWeights.PickRandomElementGivenWeights();
                    log += $"\n-Chosen target is {chosenEmbraceTarget.name}";
                    return character.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.VAMPIRIC_EMBRACE, chosenEmbraceTarget, out producedJob);
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
                    if (structure.settlementLocation == null || structure.settlementLocation.owner == null) {
                        return structure;
                    }
                }
            }
        }
        return null;
    }
    private Area GetNoStructurePlainAreaInAllRegions() {
        Area area = null;
        for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            Region region = GridMap.Instance.allRegions[i];
            area = GetNoStructurePlainAreaInRegion(region);
            if (area != null) {
                return area;
            }
        }
        return area;
    }
    private Area GetNoStructurePlainAreaInRegion(Region region) {
        return region.GetRandomHexThatMeetCriteria(a => a.elevationType != ELEVATION.WATER && a.elevationType != ELEVATION.MOUNTAIN && !a.structureComponent.HasStructureInArea() && !a.IsNextToOrPartOfVillage() && !a.gridTileComponent.HasCorruption());
    }

    public static WeightedDictionary<Character> GetVampiricEmbraceTargetWeights(Character character) {
        WeightedDictionary<Character> embraceWeights = new WeightedDictionary<Character>();
        foreach (var relationships in character.relationshipContainer.relationships) {
            Character otherCharacter = DatabaseManager.Instance.characterDatabase.GetCharacterByID(relationships.Key);
            if (otherCharacter != null) {
                if (!otherCharacter.traitContainer.HasTrait("Vampire") && relationships.Value.awareness.state != AWARENESS_STATE.Presumed_Dead && 
                    relationships.Value.awareness.state != AWARENESS_STATE.Missing && !otherCharacter.partyComponent.isActiveMember && !otherCharacter.isDead
                    && otherCharacter.marker && otherCharacter.gridTileLocation != null && otherCharacter.grave == null) {
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
        return embraceWeights;
    }
}
