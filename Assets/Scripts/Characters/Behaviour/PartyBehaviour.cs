using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class PartyBehaviour : CharacterBehaviourComponent {
    public PartyBehaviour() {
        priority = 29;
    }
    public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = true;
        Party party = character.partyComponent.currentParty;
        if (party.isActive) {
            log += $"\n-Party is active, will try to do party behaviour";
            if (party.partyState == PARTY_STATE.Waiting) {
                log += $"\n-Party is waiting";
                if(party.meetingPlace != null && !party.meetingPlace.hasBeenDestroyed && party.meetingPlace.passableTiles.Count > 0) {
                    if(character.currentStructure == party.meetingPlace) {
                        party.AddMemberThatJoinedQuest(character);
                        character.trapStructure.SetForcedStructure(party.meetingPlace);
                        character.needsComponent.CheckExtremeNeedsWhileInActiveParty();

                        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                    } else {
                        LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(party.meetingPlace.passableTiles);
                        hasJob = character.jobComponent.CreateGoToWaitingJob(targetTile, out producedJob);
                    }
                    hasJob = true;
                }
            } else {
                if (party.membersThatJoinedQuest.Contains(character)) {
                    if (party.IsMemberActive(character)) {
                        if (party.partyState == PARTY_STATE.Moving) {
                            log += $"\n-Party is moving";
                            if (party.targetDestination != null && !party.targetDestination.hasBeenDestroyed) {
                                if (party.targetDestination.IsAtTargetDestination(character)) {
                                    if(party.targetDestination == party.partySettlement) {
                                        party.currentQuest.EndQuest();
                                    } else {
                                        party.SetPartyState(PARTY_STATE.Working);
                                    }
                                    return hasJob;
                                    //hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
                                } else {
                                    LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
                                    if(tile != null) {
                                        hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                                    }
                                }
                            }
                        } else if (party.partyState == PARTY_STATE.Resting) {
                            log += $"\n-Party is resting";
                            if (party.targetRestingTavern != null && !party.targetRestingTavern.hasBeenDestroyed && party.targetRestingTavern.passableTiles.Count > 0) {
                                if (character.currentStructure == party.targetRestingTavern) {
                                    character.trapStructure.SetForcedStructure(party.targetRestingTavern);
                                    character.needsComponent.CheckExtremeNeedsWhileInActiveParty();

                                    hasJob = TavernBehaviour(character, party, out producedJob);
                                } else {
                                    LocationGridTile tile = UtilityScripts.CollectionUtilities.GetRandomElement(party.targetRestingTavern.passableTiles);
                                    if (tile != null) {
                                        hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                                    }
                                }
                            } else if (party.targetCamp != null) {
                                if (character.gridTileLocation != null && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap
                                    && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == party.targetCamp) {
                                    character.trapStructure.SetForcedHex(party.targetCamp);
                                    character.needsComponent.CheckExtremeNeedsWhileInActiveParty();

                                    hasJob = CampBehaviour(character, party, out producedJob);
                                } else {
                                    LocationGridTile targetTile = UtilityScripts.CollectionUtilities.GetRandomElement(party.targetCamp.locationGridTiles);
                                    hasJob = character.jobComponent.CreatePartyGoToJob(targetTile, out producedJob);
                                }
                            }
                        } else if (party.partyState == PARTY_STATE.Working) {
                            log += $"\n-Party is working";
                            if (!party.targetDestination.IsAtTargetDestination(character)) {
                                if (party.hasChangedTargetDestination) {
                                    party.SetHasChangedTargetDestination(false);
                                    party.SetPartyState(PARTY_STATE.Moving);
                                    return true;
                                } else {
                                    LocationGridTile tile = party.targetDestination.GetRandomPassableTile();
                                    hasJob = character.jobComponent.CreatePartyGoToJob(tile, out producedJob);
                                }
                            }
                        }
                        hasJob = true;
                    }
                }
            }
        }
        if (producedJob != null) {
            producedJob.SetIsThisAPartyJob(true);
        }
        //return hasJob;
        return true;
    }

    private bool CampBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;

        if (party.campSetter == null) {
            party.SetCampSetter(character);
        } else if (party.foodProducer == null) {
            party.SetFoodProducer(character);
        }

        if(party.campSetter == character) {
            hasJob = CampSetterBehaviour(character, party, out producedJob);
            if (hasJob) {
                return hasJob;
            }
        }
        if(party.foodProducer == character) {
            hasJob = FoodProducerBehaviour(character, party, out producedJob);
            if (hasJob) {
                return hasJob;
            }
        }

        //if(!character.needsComponent.isHungry && !character.needsComponent.isStarving 
        //    && !character.needsComponent.isTired && !character.needsComponent.isExhausted
        //    && !character.needsComponent.isBored && !character.needsComponent.isSulking) {

        //} else {
        //    character.needsComponent.CheckExtremeNeedsWhileInActiveParty();
        //}

        if (GameUtilities.RollChance(50)) {
            Campfire campfire = GetPartyCampfireInHex(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner, character, party);
            if(campfire != null) {
                hasJob = character.jobComponent.TriggerWarmUp(campfire, out producedJob);
                if (hasJob) {
                    return hasJob;
                }
            }
        }
        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        return hasJob;
    }

    private bool TavernBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;

        //if (!character.needsComponent.isHungry && !character.needsComponent.isStarving
        //    && !character.needsComponent.isTired && !character.needsComponent.isExhausted
        //    && !character.needsComponent.isBored && !character.needsComponent.isSulking) {

        //} else {
        //    character.needsComponent.CheckExtremeNeedsWhileInActiveParty();
        //}

        hasJob = character.jobComponent.TriggerRoamAroundStructure(out producedJob);
        return hasJob;
    }

    private bool CampSetterBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;

        Campfire campfire = GetPartyCampfireInHex(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner, character, party);
        if (campfire == null) {
            hasJob = character.jobComponent.TriggerBuildCampfireJob(JOB_TYPE.IDLE_CAMP, out producedJob);
        }
        return hasJob;
    }

    private bool FoodProducerBehaviour(Character character, Party party, out JobQueueItem producedJob) {
        producedJob = null;
        bool hasJob = false;

        if(HasMemberThatIsHungryOrStarvingAndThereIsNoFoodInCamp(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner, party)) {
            hasJob = character.jobComponent.CreateProduceFoodForCampJob(out producedJob);
        }
        return hasJob;
    }

    private Campfire GetPartyCampfireInHex(HexTile hex, Character character, Party party) {
        Campfire chosenCampfire = null;
        for (int i = 0; i < hex.locationGridTiles.Count; i++) {
            LocationGridTile tile = hex.locationGridTiles[i];
            if (tile.objHere != null && tile.objHere is Campfire campfire) {
                if (campfire.characterOwner == null 
                    || campfire.IsOwnedBy(character) 
                    || (!character.IsHostileWith(campfire.characterOwner) && !character.relationshipContainer.IsEnemiesWith(campfire.characterOwner))
                    || party.IsMember(campfire.characterOwner)) {
                    chosenCampfire = campfire;
                    break;
                }
            }
        }
        return chosenCampfire;
    }

    private bool HasMemberThatIsHungryOrStarvingAndThereIsNoFoodInCamp(HexTile hex, Party party) {
        bool hasHungryStarvingMember = false;
        for (int i = 0; i < party.membersThatJoinedQuest.Count; i++) {
            Character character = party.membersThatJoinedQuest[i];
            if (party.IsMemberActive(character)) {
                if(character.needsComponent.isHungry || character.needsComponent.isStarving) {
                    hasHungryStarvingMember = true;
                    break;
                }
            }
        }
        return hasHungryStarvingMember && GetFoodPileInCamp(hex, party) == null;
    }

    private FoodPile GetFoodPileInCamp(HexTile hex, Party party) {
        FoodPile chosenFoodPile = null;
        for (int i = 0; i < hex.locationGridTiles.Count; i++) {
            LocationGridTile tile = hex.locationGridTiles[i];
            if (tile.objHere != null && tile.objHere is FoodPile foodPile && foodPile.storedResources[RESOURCE.FOOD] >= 12) {
                chosenFoodPile = foodPile;
                break;
            }
        }
        return chosenFoodPile;
    }
}
