using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class Nap : GoapAction {

    public Nap() : base(INTERACTION_TYPE.NAP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        // validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.LUNCH_TIME };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STAMINA_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Nap Success", goapNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "no_space_bed";
            } else if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_unavailable";
            }
        }
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
#endif
        int cost = 0;
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
#if DEBUG_LOG
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return 2000;
            }
        }
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                    LocationGridTile centerGridTileOfTarget = target.gridTileLocation.area.gridTileComponent.centerGridTile;
                    LocationGridTile centerGridTileOfActor = actor.gridTileLocation.area.gridTileComponent.centerGridTile;
                    float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                    int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                    if (distance > distanceToCheck) {
                        //target is at structure that character is avoiding
#if DEBUG_LOG
                        costLog += $" +2000(Active Party, Location of target too far from actor)";
                        actor.logComponent.AppendCostLog(costLog);
#endif
                        return 2000;
                    }
                }
            }
        }
        if (target is BaseBed targetBed) {
            if (!targetBed.IsSlotAvailable()) {
                cost += 2000;
#if DEBUG_LOG
                costLog += " +2000(Fully Occupied)";
#endif
            } else if (actor.traitContainer.HasTrait("Travelling")) {
                cost += 100;
#if DEBUG_LOG
                costLog += " +100(Travelling)";
#endif
            } else {
                if (targetBed.IsOwnedBy(actor) || targetBed.structureLocation == actor.homeStructure) {
                    cost += UtilityScripts.Utilities.Rng.Next(30, 51);
#if DEBUG_LOG
                    costLog += $" +{cost}(Owned/Location is in home structure)";
#endif
                } else {
                    if (actor.needsComponent.isExhausted) {
                        if (targetBed.IsInHomeStructureOfCharacterWithOpinion(actor, RelationshipManager.Close_Friend, RelationshipManager.Friend)) {
                            cost += UtilityScripts.Utilities.Rng.Next(130, 151);
#if DEBUG_LOG
                            costLog += $" +{cost}(Exhausted, Is in Friend home structure)";
#endif
                        } else if (targetBed.IsInHomeStructureOfCharacterWithOpinion(actor, RelationshipManager.Rival, RelationshipManager.Enemy)) {
                            cost += 2000;
#if DEBUG_LOG
                            costLog += " +2000(Exhausted, Is in Enemy home structure)";
#endif
                        } else {
                            cost = UtilityScripts.Utilities.Rng.Next(80, 101);
#if DEBUG_LOG
                            costLog += $" +{cost}(Else)";
#endif
                        }
                    } else {
                        cost += 2000;
#if DEBUG_LOG
                        costLog += $" +{cost}(Not Exhausted)";
#endif
                    }
                }

                Character alreadySleepingCharacter = null;
                for (int i = 0; i < targetBed.users.Length; i++) {
                    if(targetBed.users[i] != null) {
                        alreadySleepingCharacter = targetBed.users[i];
                        break;
                    }
                }

                if(alreadySleepingCharacter != null) {
                    string opinionLabel = actor.relationshipContainer.GetOpinionLabel(alreadySleepingCharacter);
                    if(opinionLabel == RelationshipManager.Friend) {
                        cost += 20;
#if DEBUG_LOG
                        costLog += " +20(Friend Occupies)";
#endif
                    } else if (opinionLabel == RelationshipManager.Acquaintance) {
                        cost += 25;
#if DEBUG_LOG
                        costLog += " +25(Acquaintance Occupies)";
#endif
                    } else if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival || opinionLabel == string.Empty) {
                        cost += 100;
#if DEBUG_LOG
                        costLog += " +100(Enemy/Rival/None Occupies)";
#endif
                    }
                }
            }
        }
#if DEBUG_LOG
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapAreaIsNot(poiTarget.gridTileLocation.area)) {
                return false;
            }
            return poiTarget.IsAvailable() && CanSleepInBed(actor, poiTarget as TileObject) && poiTarget.gridTileLocation != null;
        }
        return false;
    }
#endregion

#region State Effects
    public void PreNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
    }
    public void PerTickNapSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        CharacterNeedsComponent needsComponent = actor.needsComponent;

        needsComponent.AdjustTiredness(1f);
        needsComponent.AdjustHappiness(3f);

        //float staminaAdjustment = 0f;
        //if (actor.currentStructure == actor.homeStructure) {
        //    staminaAdjustment = 1f;
        //} else if (actor.currentStructure is Dwelling && actor.currentStructure != actor.homeStructure) {
        //    staminaAdjustment = 0.5f;
        //} else if (actor.currentStructure.structureType == STRUCTURE_TYPE.INN) {
        //    staminaAdjustment = 0.8f;
        //} else if (actor.currentStructure.structureType == STRUCTURE_TYPE.PRISON) {
        //    staminaAdjustment = 0.4f;
        //} else if (actor.currentStructure.structureType.IsOpenSpace()) {
        //    staminaAdjustment = 0.3f;
        //}
        //needsComponent.AdjustStamina(staminaAdjustment);
    }
    public void AfterNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    //public void PreNapFail() {
    //    goapNode.descriptionLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void PreNapMissing() {
    //    goapNode.descriptionLog.AddToFillers(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
#endregion

    private bool CanSleepInBed(Character character, TileObject tileObject) {
        return (tileObject as BaseBed).CanUseBed(character);
    }
}