using UnityEngine;
using System.Collections.Generic;

public class DarkRitual : GoapAction {
    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;
    public DarkRitual() : base(INTERACTION_TYPE.DARK_RITUAL) {
        actionIconString = GoapActionStateDB.Cult_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", false, GOAP_EFFECT_TARGET.ACTOR), HasCultistKit);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Ritual Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = 10;
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
                cost = 2000;
#if DEBUG_LOG
                costLog += $" +{cost}(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
#endif
                return cost;
            }
        }
        return cost;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            reactions.Add(EMOTION.Shock);

            if (witness.relationshipContainer.IsFriendsWith(actor) ||
                witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Acquaintance)) {
                reactions.Add(EMOTION.Despair);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                reactions.Add(EMOTION.Threatened);
            }
        } else {
            reactions.Add(EMOTION.Approval);
            if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                int roll = Random.Range(0, 100);
                if (roll < chance) {
                    reactions.Add(EMOTION.Arousal);
                }
            }
        }
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Demon_Worship;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, target, otherData, job);
        if (satisfied) {
            return target.gridTileLocation != null;
        }
        return false;
    }
#endregion
    
#region Preconditions
    private bool HasCultistKit(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem("Cultist Kit");
    }
#endregion
    
#region State Effects
    public void AfterRitualSuccess(ActualGoapNode goapNode) {
        Messenger.Broadcast(JobSignals.ON_FINISH_PRAYING, goapNode);
        goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
    }
#endregion
}