using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;
using Inner_Maps;

public class RitualKilling : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    //TODO: This is one of the ways to optimize actions, situational preconditions can be cached at the beginning so that we will not call new Precondition every time
    //TODO: This also applies to expected effects
    //CREATE A SYSTEM FOR THIS
    private Precondition atHomePrecondition;
    private Precondition notAtHomePrecondition;
    
    public RitualKilling() : base(INTERACTION_TYPE.RITUAL_KILLING) {
        actionIconString = GoapActionStateDB.Death_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        atHomePrecondition = new Precondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", target = GOAP_EFFECT_TARGET.TARGET }, HasRestrained);
        notAtHomePrecondition = new Precondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetInWildernessOrHome);
        logTags = new[] {LOG_TAG.Crimes, LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Killing Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        if (target is Character) {
            //List<Precondition> baseP = base.GetPrecondition(actor, target, otherData, out isOverridden);
            //List<Precondition> p = ObjectPoolManager.Instance.CreateNewPreconditionsList();
            //p.AddRange(baseP);

            Precondition p = null;

            Character targetCharacter = target as Character;
            if (actor.homeStructure == targetCharacter.currentStructure) {
                p = atHomePrecondition;
            } else {
                p = notAtHomePrecondition;
            }
            isOverridden = true;
            return p;
        }
        return base.GetPrecondition(actor, target, otherData, jobType, out isOverridden);
    }
    //public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
    //    GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
    //    Character actor = node.actor;
    //    IPointOfInterest poiTarget = node.poiTarget;
    //    if (goapActionInvalidity.isInvalid == false) {
    //        if (actor.marker.CanDoStealthActionToTarget(poiTarget) == false) {
    //            goapActionInvalidity.isInvalid = true;
    //            goapActionInvalidity.stateName = "Killing Fail";
    //        }
    //    }
    //    return goapActionInvalidity;
    //}
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character) {
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
                reactions.Add(EMOTION.Shock);
            } else {
                reactions.Add(EMOTION.Threatened);
                reactions.Add(EMOTION.Disgust);
                reactions.Add(EMOTION.Shock);
                Character targetCharacter = target as Character;
                if (witness.relationshipContainer.IsFriendsWith(actor) && !witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Disappointment);
                }
                if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                    reactions.Add(EMOTION.Anger);
                    reactions.Add(EMOTION.Disapproval);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (!witness.traitContainer.HasTrait("Psychopath")) {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Concern);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (targetCharacter.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
                reactions.Add(EMOTION.Shock);
            } else {
                reactions.Add(EMOTION.Threatened);

                if (targetCharacter.relationshipContainer.IsFriendsWith(actor) && !targetCharacter.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Betrayal);
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Murder;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor != poiTarget && actor.traitContainer.HasTrait("Psychopath");
        }
        return false;
    }
    private bool IsTargetInWildernessOrHome(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType) {
        if(target is Character && otherData != null) {
            Character targetCharacter = target as Character;
            bool isSatisfied = false;
            if(otherData.Length == 1) {
                if(otherData[0] is AreaOtherData hex) {
                    isSatisfied = target.gridTileLocation.area == hex.area;
                } else if (otherData[0] is LocationStructureOtherData structure) {
                    isSatisfied = targetCharacter.currentStructure == structure.locationStructure;
                } else if (otherData[0] is LocationGridTileOtherData gridTile) {
                    isSatisfied = targetCharacter.gridTileLocation == gridTile.tile;
                }
            }
            return targetCharacter.carryComponent.IsNotBeingCarried() && targetCharacter.traitContainer.HasTrait("Restrained") && isSatisfied; //targetCharacter.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || 
        }
        return false;
    }
    private bool HasRestrained(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType) {
        return target.traitContainer.HasTrait("Restrained");
    }
#endregion

#region State Effects
    public void PreKillingSuccess(ActualGoapNode goapNode) {
        
    }
    public void AfterKillingSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustHappiness(10000);
        if (goapNode.poiTarget is Character targetCharacter) {
            targetCharacter.causeOfDeath = INTERACTION_TYPE.RITUAL_KILLING;
            NPCSettlement settlementOfTarget = targetCharacter.homeSettlement;
            targetCharacter.Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
            goapNode.actor.jobComponent.TriggerBuryPsychopathVictim(targetCharacter, settlementOfTarget);
            targetCharacter.reactionComponent.AddCharacterThatSawThisDead(goapNode.actor);
        }
    }
#endregion
}