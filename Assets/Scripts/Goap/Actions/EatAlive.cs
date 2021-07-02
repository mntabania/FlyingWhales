using System.Collections.Generic;
using Traits;

public class EatAlive : GoapAction {
    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.CONSUME;

    public EatAlive() : base(INTERACTION_TYPE.EAT_ALIVE) {
        actionIconString = GoapActionStateDB.Eat_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF,
            RACE.SPIDER, RACE.DRAGON, RACE.GOLEM, RACE.DEMON, RACE.ELEMENTAL, RACE.KOBOLD, RACE.MIMIC, RACE.ABOMINATION,
            RACE.CHICKEN, RACE.SHEEP, RACE.PIG, RACE.NYMPH, RACE.WISP, RACE.SLUDGE, RACE.GHOST, RACE.LESSER_DEMON, RACE.ANGEL, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Needs};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Alive Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    //public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
    //    string reaction = base.ReactionToActor(actor, target, witness, node, status);
    //    if (!actor.isNormalCharacter && witness.homeSettlement != null && witness.faction != null && actor.homeStructure != null && target is Character targetCharacter) {
    //        Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
    //        if (node.targetStructure == actor.homeStructure || (prisoner != null && prisoner.IsConsideredPrisonerOf(actor))) {
    //            string relationshipName = witness.relationshipContainer.GetRelationshipNameWith(targetCharacter);
    //            if (relationshipName == RelationshipManager.Acquaintance || witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
    //                witness.faction.partyQuestBoard.CreateExterminatePartyQuest(witness, witness.homeSettlement, actor.homeStructure, witness.homeSettlement);    
    //            }    
    //        }
    //    }
    //    return reaction;
    //}
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Despair);
                    reactions.Add(EMOTION.Sadness);    
                }
            } else if (witness.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(targetCharacter)) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Despair);
                    reactions.Add(EMOTION.Sadness);    
                }
            }
        }
    }
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return !node.actor.isNormalCharacter && node.target is Character targetCharacter && targetCharacter.isNormalCharacter;
    }
#endregion
    
#region State Effects
    public void PerTickEatAliveSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.race == RACE.ELVES && goapNode.poiTarget is RatMeat) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poor Meal");
        }
        //Moved adjust hp here because when the target dies, the cancel jobs targeting the target of this action will trigger and it will force this action to be object pooled, resetting all values
        goapNode.poiTarget.AdjustHP(-10, ELEMENTAL_TYPE.Normal, true, goapNode.actor, showHPBar: true);
    }
    public void AfterEatAliveSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.race == RACE.ELVES && goapNode.poiTarget is Character targetCharacter && (targetCharacter.race == RACE.RAT || targetCharacter.race == RACE.RATMAN)) {
            goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Poor Meal");
        }
    }
#endregion
}