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
#if DEBUG_LOG
        log += $"\n-{character.name} is a vampire";
#endif
        if ((character.moodComponent.moodState == MOOD_STATE.Bad || character.moodComponent.moodState == MOOD_STATE.Critical) && character.traitContainer.HasTrait("Hemophobic")) {
            return character.jobComponent.TriggerSuicideJob(out producedJob, "Hemophobic Vampire");
        }

        if (character.needsComponent.isHungry || character.needsComponent.isStarving) {
            TIME_IN_WORDS currentTime = GameManager.Instance.GetCurrentTimeInWordsOfTick();
            if (currentTime == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                character.needsComponent.PlanFullnessRecoveryActionsVampire();
            }
        }

        if (character.characterClass.className == "Vampire Lord") {
#if DEBUG_LOG
            log += $"\n-{character.name} is a Vampire Lord";
#endif
            if (character.homeStructure == null || character.homeStructure.structureType != STRUCTURE_TYPE.VAMPIRE_CASTLE) {
#if DEBUG_LOG
                log += $"\n-{character.name} does not have a home structure or does not live at a vampire castle";
#endif
                var structureSetting = new StructureSetting(STRUCTURE_TYPE.VAMPIRE_CASTLE, RESOURCE.STONE); //character.faction.factionType.mainResource
                if (character.homeSettlement != null) {
#if DEBUG_LOG
                    log += $"\n-{character.name} has a home settlement {character.homeSettlement.name}";
#endif
                    LocationStructure unoccupiedCastle = character.homeSettlement.GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE.VAMPIRE_CASTLE);
                    if (unoccupiedCastle != null) {
#if DEBUG_LOG
                        log += $"\n-{character.homeSettlement.name} has an unoccupied vampire castle {unoccupiedCastle.name}. Setting home to that.";
#endif
                        //Transfer home
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, unoccupiedCastle.tiles.First().tileObjectComponent.genericTileObject);
                        producedJob = null;
                        return true;
                    } else if (GameUtilities.RollChance(15, ref log) && character.faction?.factionType.type != FACTION_TYPE.Vagrants) { //15
#if DEBUG_LOG
                        log += $"\n-{character.homeSettlement.name} does not have an unoccupied vampire castle, and successfully rolled to build a new one";
#endif
                        //Build vampire castle
                        if (LandmarkManager.Instance.CanPlaceStructureBlueprint(character.homeSettlement, structureSetting, out var targetTile, out var structurePrefabName, out var connectorToUse, out var connectorTile)) {
#if DEBUG_LOG
                            log += $"\n-Will place dwelling blueprint {structurePrefabName} at {targetTile}.";
#endif
                            return character.jobComponent.TriggerBuildVampireCastle(targetTile, out producedJob, structurePrefabName);    
                        }    
                    }
                } 
//                 else {
// #if DEBUG_LOG
//                     log += $"\n-{character.name} does not have a home settlement. Will try to find unoccupied vampire castles in the wild.";
// #endif
//                     LocationStructure unoccupiedCastle = GetFirstNonSettlementVampireCastles(character);
//                     if (unoccupiedCastle != null) {
// #if DEBUG_LOG
//                         log += $"\n-Found unoccupied castle {unoccupiedCastle.name}";
// #endif
//                         //Transfer home
//                         character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, unoccupiedCastle.tiles.First().tileObjectComponent.genericTileObject);
//                         producedJob = null;
//                         return true;
//                     } else if (!WorldSettings.Instance.worldSettingsData.villageSettings.disableNewVillages && GameUtilities.RollChance(15, ref log) && character.faction != null && 
//                                character.faction.factionType.type != FACTION_TYPE.Vagrants){ //15
//                         // Area targetArea = GetNoStructurePlainAreaInAllRegions();
//                         VillageSpot villageSpot = character.currentRegion.GetFirstUnoccupiedVillageSpotThatCanAccomodateFaction(character.faction.factionType.type);
//                         if (villageSpot != null) {
//                             Area targetArea = villageSpot.mainSpot;
// #if DEBUG_LOG
//                             log += $"\n-Could not find valid castle in wild, and successfully rolled to build a new castle at {targetArea}";
// #endif
//                             //Build vampire castle
//                             List<GameObject> choices = InnerMapManager.Instance.GetStructurePrefabsForStructure(structureSetting);
//                             GameObject chosenStructurePrefab = CollectionUtilities.GetRandomElement(choices);
//                             if (LandmarkManager.Instance.HasEnoughSpaceForStructure(chosenStructurePrefab.name, targetArea.gridTileComponent.centerGridTile)) {
//                                 return character.jobComponent.TriggerBuildVampireCastle(targetArea.gridTileComponent.centerGridTile, out producedJob, chosenStructurePrefab.name);    
//                             }
//                         } else {
// #if DEBUG_LOG
//                             log += $"\n-Could not find valid Area in wild to build a vampire castle.";
// #endif
//                         }
//                     }
//                 }
            }

            if (character.homeStructure != null) {
#if DEBUG_LOG
                log += $"\n-{character.name} has a home. Will check if it has a prisoner there.";
#endif
                bool hasPrisonerAtHome = false;
                for (int i = 0; i < character.homeStructure.charactersHere.Count; i++) {
                    Character otherCharacter = character.homeStructure.charactersHere[i];
                    Prisoner prisoner = otherCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                    if (otherCharacter != character && prisoner != null && !otherCharacter.isDead) {
#if DEBUG_LOG
                        log += $"\n-{character.name} found a prisoner at home: {otherCharacter.name}. Will not create Imprison blood source.";
#endif
                        hasPrisonerAtHome = true;
                        break;
                    }
                }
                if (!hasPrisonerAtHome) {
                    Vampire vampire = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                    if (!vampire.dislikedBeingVampire || character.traitContainer.HasTrait("Evil", "Treacherous", "Glutton")) { //character.traitContainer.GetTraitsOrStatuses<Trait>("Evil", "Treacherous", "Glutton").Count > 0
                        if (GameUtilities.RollChance(15)) { //15
                            return character.jobComponent.TriggerImprisonBloodSource(out producedJob, ref log);
                        }
                    }
                }
            }
        } else {
            Vampire vampire = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if (!vampire.hasAlreadyBecomeVampireLord) {
                if (PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.VAMPIRISM).currentLevel >= 2) {
                    if (character.characterClass.className == "Necromancer") {
#if DEBUG_LOG
                        log += $"\n-{character.name} is not yet a vampire lord. But is a necromancer, not becoming a Vampire Lord.";
#endif
                    } else if (character.characterClass.className == "Werewolf") {
#if DEBUG_LOG
                        log += $"\n-{character.name} is not yet a vampire lord. But is currently a Werewolf, not becoming a Vampire Lord.";
#endif
                    } else if (character.traitContainer.HasTrait("Enslaved")) {
#if DEBUG_LOG
                        log += $"\n-{character.name} is Enslaved, not becoming a Vampire Lord.";
#endif
                    } else {
#if DEBUG_LOG
                        log += $"\n-{character.name} is not yet a vampire lord. Rolling for chance to check converted villagers.";
#endif
                        if (ChanceData.RollChance(CHANCE_TYPE.Vampire_Lord_Chance, ref log)) { //10
#if DEBUG_LOG
                            log += $"\n-{character.name} converted villagers are {vampire.numOfConvertedVillagers.ToString()}.";
#endif
                            if (vampire.numOfConvertedVillagers >= 3) {
                                //Become vampire lord
#if DEBUG_LOG
                                log += $"\n-{character.name} will become a vampire lord.";
#endif
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Vampire_Lord, character);
                                producedJob = null;
                                return true;
                            }
                        }
                    }
                }
            }
        }

        if (character.needsComponent.isSulking) {
#if DEBUG_LOG
            log += $"\n-{character.name} is sulking. Rolling for chance to vampiric embrace.";
#endif
            if (GameUtilities.RollChance(3, ref log)) { //3
                WeightedDictionary<Character> embraceWeights = GetVampiricEmbraceTargetWeights(character);
#if DEBUG_LOG
                log += $"\n-{embraceWeights.GetWeightsSummary("Vampiric Embrace weights:")}.";
#endif
                if (embraceWeights.GetTotalOfWeights() > 0) {
                    Character chosenEmbraceTarget = embraceWeights.PickRandomElementGivenWeights();
#if DEBUG_LOG
                    log += $"\n-Chosen target is {chosenEmbraceTarget.name}";
#endif
                    return character.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.VAMPIRIC_EMBRACE, chosenEmbraceTarget, out producedJob);
                }
            }
        }
        
        producedJob = null;
        return false;
    }

    private LocationStructure GetFirstNonSettlementVampireCastles(Character character) {
        Region region = character.currentRegion;
        if (region != null && region.HasStructure(STRUCTURE_TYPE.VAMPIRE_CASTLE)) {
            List<LocationStructure> vampireCastles = region.GetStructuresAtLocation(STRUCTURE_TYPE.VAMPIRE_CASTLE);
            if (vampireCastles != null) {
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
        return region.GetRandomAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage();
    }

    public static WeightedDictionary<Character> GetVampiricEmbraceTargetWeights(Character character) {
        WeightedDictionary<Character> embraceWeights = new WeightedDictionary<Character>();
        List<Character> embraceChoices = RuinarchListPool<Character>.Claim();
        if (character.homeSettlement != null) {
            embraceChoices.AddRange(character.homeSettlement.residents);
            embraceChoices.Remove(character);
        }
        foreach (var relationships in character.relationshipContainer.relationships) {
            Character otherCharacter = DatabaseManager.Instance.characterDatabase.GetCharacterByID(relationships.Key);
            if (otherCharacter != null && !embraceChoices.Contains(otherCharacter)) {
                embraceChoices.Add(otherCharacter);
            }
        }

        for (int i = 0; i < embraceChoices.Count; i++) {
            Character otherCharacter = embraceChoices[i];
            AWARENESS_STATE awarenessState = character.relationshipContainer.GetAwarenessState(otherCharacter);
            if (!otherCharacter.traitContainer.HasTrait("Vampire") && awarenessState != AWARENESS_STATE.Presumed_Dead && 
                awarenessState != AWARENESS_STATE.Missing && !otherCharacter.partyComponent.isActiveMember && !otherCharacter.isDead && 
                otherCharacter.marker && otherCharacter.gridTileLocation != null && otherCharacter.grave == null) {
                var opinionLabel = character.relationshipContainer.GetOpinionLabel(otherCharacter);
                IRelationshipData relationshipData = character.relationshipContainer.GetRelationshipDataWith(otherCharacter);
                if (relationshipData != null && relationshipData.IsLover() && (opinionLabel == RelationshipManager.Close_Friend || opinionLabel == RelationshipManager.Friend)) {
                    embraceWeights.AddElement(otherCharacter, 100);
                } else if (relationshipData != null && relationshipData.HasRelationship(RELATIONSHIP_TYPE.AFFAIR) && (opinionLabel == RelationshipManager.Close_Friend || opinionLabel == RelationshipManager.Friend)) {
                    embraceWeights.AddElement(otherCharacter, 50);
                } else if (opinionLabel == RelationshipManager.Close_Friend) {
                    embraceWeights.AddElement(otherCharacter, 50);
                } else if (opinionLabel == RelationshipManager.Friend) {
                    embraceWeights.AddElement(otherCharacter, 10);
                } else if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
                    embraceWeights.AddElement(otherCharacter, 5);
                } else {
                    embraceWeights.AddElement(otherCharacter, 5);
                }
            }
        }
        
        return embraceWeights;
    }
}
