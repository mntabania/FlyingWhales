﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using UtilityScripts;
using Inner_Maps;
using Object_Pools;

public class DrinkBlood : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public DrinkBlood() : base(INTERACTION_TYPE.DRINK_BLOOD) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Drink_Blood_Icon;
        doesNotStopTargetCharacter = true;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes, LOG_TAG.Needs};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET }, HasUnconsciousOrRestingTarget);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", target = GOAP_EFFECT_TARGET.TARGET });
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
        if (actor.traitContainer.HasTrait("Vampire")) {
            List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
            List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
            if (baseEE != null && baseEE.Count > 0) {
                ee.AddRange(baseEE);
            }
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
            isOverridden = true;
            return ee;
        }
        return base.GetExpectedEffects(actor, target, otherData, out isOverridden);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drink Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (job.jobType == JOB_TYPE.TRIGGER_FLAW) {
            if (actor.traitContainer.HasTrait("Cannibal")) {
                cost = UtilityScripts.Utilities.Rng.Next(450, 551);
                costLog += $" {cost}(Actor is cannibal and job is trigger flaw)";
            } else {
                if (!target.traitContainer.HasTrait("Vampire")) {
                    cost = UtilityScripts.Utilities.Rng.Next(450, 551);
                    costLog += $" {cost}(Actor not cannibal, target not vampire, and job is trigger flaw)";
                } else {
                    cost = 2000;
                    costLog += $" {cost}(Actor not cannibal, target vampire, and job is trigger flaw)";
                }
            }
            actor.logComponent.AppendCostLog(costLog);
            return cost;
        }
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
                return 2000;
            }
        }
        if (actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive) {
            if (actor.partyComponent.isActiveMember) {
                if (!(target is Animal)) {
                    CRIME_SEVERITY severity = actor.partyComponent.currentParty.partyFaction.GetCrimeSeverity(actor, actor, CRIME_TYPE.Vampire);
                    if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                        //Should not target non-animals if party faction considers Vampire a crime
                        costLog += $" +2000(Active Party, target is not animal and party faction considers crime)";
                        actor.logComponent.AppendCostLog(costLog);
                        return 2000;
                    }
                } else {
                    if (!actor.needsComponent.isStarving) {
                        //Should not target animals if actor is not starvinf
                        costLog += $" +2000(Active Party, target is animal and actor is not starving)";
                        actor.logComponent.AppendCostLog(costLog);
                        return 2000;
                    }
                }
                if (target.gridTileLocation != null && actor.gridTileLocation != null) {
                    LocationGridTile centerGridTileOfTarget = target.gridTileLocation.parentArea.GetCenterLocationGridTile();
                    LocationGridTile centerGridTileOfActor = actor.gridTileLocation.parentArea.GetCenterLocationGridTile();
                    float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                    int distanceToCheck = InnerMapManager.AreaLocationGridTileSize.x * 3;

                    if (distance > distanceToCheck) {
                        //target is at structure that character is avoiding
                        costLog += $" +2000(Active Party, Location of target too far from actor)";
                        actor.logComponent.AppendCostLog(costLog);
                        return 2000;
                    }
                }
            }
        }
        if (target is Character targetCharacter) {
            if (targetCharacter.traitContainer.HasTrait("Vampire") && !actor.traitContainer.HasTrait("Cannibal")) {
                cost += 2000;
                costLog += " +2000(Vampire target, not Cannibal actor)";
                actor.logComponent.AppendCostLog(costLog);
                //Skip further cost processing
                return cost;
            }
            if (!targetCharacter.traitContainer.HasTrait("Vampire") && actor.traitContainer.HasTrait("Cannibal")) {
                cost += 2000;
                costLog += " +2000(not Vampire target, Cannibal actor)";
                actor.logComponent.AppendCostLog(costLog);
                //Skip further cost processing
                return cost;
            }
            if (!actor.isVagrant) {
                AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(targetCharacter);
                if(actor.currentRegion != targetCharacter.currentRegion || awarenessState == AWARENESS_STATE.Missing || awarenessState == AWARENESS_STATE.Presumed_Dead
                    || targetCharacter.partyComponent.isMemberThatJoinedQuest) {
                    cost += 2000;
                    costLog += " +2000(not Vagrant and not Same Region/Missing/Presumed Dead/Joined a Party Quest)";
                    actor.logComponent.AppendCostLog(costLog);
                    //Skip further cost processing
                    return cost;
                }
            }
            if (targetCharacter.limiterComponent.canPerform && targetCharacter.limiterComponent.canMove) {
                cost += 80;
                costLog += " +80(Can Perform and Move)";
            }
            if (!targetCharacter.race.IsSapient()) {
                cost += 200;
                costLog += " +200(Not Sapient)";
            }
            if (actor.needsComponent.isHungry || (!actor.needsComponent.isHungry && !actor.needsComponent.isStarving)) {
                //if (actor.currentRegion != targetCharacter.currentRegion) {
                //    cost += 2000;
                //    costLog += " +2000(Starving, Diff Region)";
                //    actor.logComponent.AppendCostLog(costLog);
                //    //Skip further cost processing
                //    return cost;
                //}
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 2000;
                    costLog += " +2000(Hungry, Friend/Close)";
                    actor.logComponent.AppendCostLog(costLog);
                    //Skip further cost processing
                    return cost;
                } else if (actor.homeStructure != null && targetCharacter.currentStructure == actor.homeStructure && targetCharacter.traitContainer.HasTrait("Prisoner")) {
                    cost += 0;
                    costLog += " +0(Hungry, Prisoner and inside actor home)";
                } else if (opinionLabel == RelationshipManager.Rival) {
                    cost += 20;
                    costLog += " +20(Hungry, Rival)";
                } else if (opinionLabel == RelationshipManager.Enemy) {
                    cost += 40;
                    costLog += " +40(Hungry, Enemy)";
                } else if (opinionLabel == RelationshipManager.Acquaintance) {
                    cost += 75;
                    costLog += " +75(Hungry, Acquaintance)";
                } else {
                    cost += 40;
                    costLog += " +40(Hungry, Other)";
                }
            } else if (actor.needsComponent.isStarving) {
                //if (actor.currentRegion != targetCharacter.currentRegion) {
                //    cost += 2000;
                //    costLog += " +2000(Starving, Diff Region)";
                //    actor.logComponent.AppendCostLog(costLog);
                //    //Skip further cost processing
                //    return cost;
                //}
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (actor.homeStructure != null && targetCharacter.currentStructure == actor.homeStructure && targetCharacter.traitContainer.HasTrait("Prisoner")) {
                    cost += 0;
                    costLog += " +0(Starving, Prisoner and inside actor home)";
                } else if (opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 400;
                    costLog += " +400(Starving, Close Friend)";
                } else if (opinionLabel == RelationshipManager.Friend) {
                    cost += 300;
                    costLog += " +300(Starving, Friend)";
                } else if (opinionLabel == RelationshipManager.Rival) {
                    cost += 20;
                    costLog += " +20(Starving, Rival)";
                } else if (opinionLabel == RelationshipManager.Enemy) {
                    cost += 40;
                    costLog += " +40(Starving, Enemy)";
                } else if (opinionLabel == RelationshipManager.Acquaintance) {
                    cost += 75;
                    costLog += " +75(Starving, Acquaintance)";
                } else {
                    cost += 40;
                    costLog += " +40(Starving, Other)";
                }
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    //public override void OnStopWhilePerforming(ActualGoapNode node) {
    //    base.OnStopWhilePerforming(node);
    //    Character actor = node.actor;
    //    actor.needsComponent.AdjustDoNotGetHungry(-1);
    //    actor.needsComponent.AdjustDoNotGetBored(-1);
    //}
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity actionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (actionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.limiterComponent.canMove && targetCharacter.limiterComponent.canPerform/*|| targetCharacter.limiterComponent.canWitness || targetCharacter.IsAvailable() == false*/) {
                actionInvalidity.isInvalid = true;
                actionInvalidity.stateName = "Drink Fail";
            }
        }
        return actionInvalidity;
    }
    public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateReactionsToActor(reactions, actor, target, witness, node, status);
        CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire);
        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
            reactions.Add(EMOTION.Shock);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                reactions.Add(EMOTION.Despair);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                reactions.Add(EMOTION.Threatened);
            }
            if (target is Character targetCharacter) {
                string opinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionToTarget == RelationshipManager.Friend || opinionToTarget == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Disapproval);
                    reactions.Add(EMOTION.Anger);
                } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                    reactions.Add(EMOTION.Disapproval);
                    reactions.Add(EMOTION.Anger);
                } else if (opinionToTarget == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Anger);
                    }
                }
            }
        } else {
            if (witness.traitContainer.HasTrait("Hemophiliac")) {
                if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                    reactions.Add(EMOTION.Arousal);
                } else {
                    reactions.Add(EMOTION.Approval);
                }
            } else if (witness.traitContainer.HasTrait("Hemophobic")) {
                reactions.Add(EMOTION.Threatened);
            }
        }
    }
    public override void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(targetCharacter, actor, target, CRIME_TYPE.Vampire);
            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                reactions.Add(EMOTION.Shock);
                string opinionLabel = targetCharacter.relationshipContainer.GetOpinionLabel(actor);
                if (targetCharacter.traitContainer.HasTrait("Coward", "Hemophobic")) {
                    reactions.Add(EMOTION.Fear);
                } else {
                    reactions.Add(EMOTION.Threatened);
                }
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Betrayal);
                }
            } else {
                if (targetCharacter.traitContainer.HasTrait("Hemophiliac")) {
                    if (RelationshipManager.IsSexuallyCompatible(actor, targetCharacter)) {
                        reactions.Add(EMOTION.Arousal);
                    } else {
                        reactions.Add(EMOTION.Approval);
                    }
                } else if (targetCharacter.traitContainer.HasTrait("Hemophobic")) {
                    reactions.Add(EMOTION.Threatened);
                }
            }
        }
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        Vampire vampire = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        if(vampire != null) {
            vampire.AddAwareCharacter(witness);
        }
        return response;
    }
    
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        if (target is Character targetCharacter) {
            Vampire vampire = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if (vampire != null) {
                vampire.AddAwareCharacter(targetCharacter);
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Vampire;
    }
    public override bool IsHappinessRecoveryAction() {
        return true;
    }
    public override bool IsFullnessRecoveryAction() {
        return true;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            //if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            //    return false;
            //}
            if(poiTarget is Character targetCharacter) {
                return actor != targetCharacter && actor.traitContainer.HasTrait("Vampire") && !targetCharacter.isDead && targetCharacter.carryComponent.IsNotBeingCarried();
            }
            return actor != poiTarget && actor.traitContainer.HasTrait("Vampire");
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasUnconsciousOrRestingTarget(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        Character target = poiTarget as Character;
        return target.traitContainer.HasTrait("Unconscious", "Resting");
    }
    #endregion

    #region Effects
    //public void PreDrinkSuccess(ActualGoapNode goapNode) {
    //    goapNode.actor.needsComponent.AdjustDoNotGetHungry(1);
    //    goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
    //}
    public void PerTickDrinkSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;

        actor.needsComponent.AdjustFullness(34f);
        actor.needsComponent.AdjustHappiness(13.3f);
    }
    public void AfterDrinkSuccess(ActualGoapNode goapNode) {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        Character actor = goapNode.actor;
        //actor.needsComponent.AdjustDoNotGetHungry(-1);
        //goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);

        if (goapNode.poiTarget is Character targetCharacter) {
            if (targetCharacter.HasItem("Phylactery")) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "activate_phylactery", goapNode, LOG_TAG.Social);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();

                targetCharacter.UnobtainItem("Phylactery");
                actor.AdjustHP(-500, ELEMENTAL_TYPE.Normal);
                if(actor.currentHP <= 0) {
                    actor.Death(deathFromAction: goapNode, responsibleCharacter: targetCharacter, _deathLog: log);
                } else {
                    actor.traitContainer.AddTrait(actor, "Unconscious", targetCharacter, goapNode);
                }
                LogPool.Release(log);
            } else {
                if (actor.currentSettlement is NPCSettlement currentSettlement && currentSettlement.eventManager.CanHaveEvents()) {
                    if (currentSettlement.owner != null && GameUtilities.RollChance(15)) { //15
                        CRIME_SEVERITY crimeSeverity = currentSettlement.owner.GetCrimeSeverity(actor, goapNode.poiTarget, CRIME_TYPE.Vampire);
                        if (crimeSeverity != CRIME_SEVERITY.None && crimeSeverity != CRIME_SEVERITY.Unapplicable && !currentSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt)) {
                            currentSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt);
                        }
                    }
                }
                //If a vampire drinks the blood of another vampire and he is not a cannibal, add Poor Meal status
                if (!targetCharacter.race.IsSapient() || (targetCharacter.traitContainer.HasTrait("Vampire") && !actor.traitContainer.HasTrait("Cannibal"))) {
                    actor.traitContainer.AddTrait(actor, "Poor Meal", targetCharacter);
                }
                if (GameUtilities.RollChance(98)) {
                    //Lethargic lethargic = TraitManager.Instance.CreateNewInstancedTraitClass<Lethargic>("Lethargic");
                    targetCharacter.traitContainer.AddTrait(targetCharacter, "Lethargic", actor, goapNode);
                } else {
                    //Vampire vampire = TraitManager.Instance.CreateNewInstancedTraitClass<Vampire>("Vampire");
                    if(targetCharacter.traitContainer.AddTrait(targetCharacter, "Vampire", actor)) {
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "contracted", goapNode, LOG_TAG.Life_Changes);
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddLogToDatabase();
                        PlayerManager.Instance.player.ShowNotificationFrom(actor, log, true);
                    }

                    if (targetCharacter.isNormalCharacter) {
                        Vampire vampireTrait = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                        if(vampireTrait != null) {
                            vampireTrait.AdjustNumOfConvertedVillagers(1);
                        }
                    }
                }
            }
        }
    }
    #endregion
}