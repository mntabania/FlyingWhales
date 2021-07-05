using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Traits;
public class IsImprisoned : GoapAction {
    public IsImprisoned() : base(INTERACTION_TYPE.IS_IMPRISONED) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Imprisoned Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        if (node.otherData.Length == 1 && node.otherData[0].obj is LocationStructure structure) {
            log.AddToFillers(structure, structure.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (witness.relationshipContainer.IsFriendsWith(actor)) {
            reactions.Add(EMOTION.Concern);
        } else if (witness.relationshipContainer.IsEnemiesWith(actor)) {
            reactions.Add(EMOTION.Scorn);
        } else if (witness.relationshipContainer.GetOpinionLabel(actor) == RelationshipManager.Acquaintance) {
            Prisoner prisoner = actor.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
            bool isPrisonerOfHostileFaction = false;
            if (prisoner != null && witness.faction != null) {
                Faction factionThatImprisoned = prisoner.GetFactionThatImprisoned();
                if (factionThatImprisoned != null && witness.faction.IsHostileWith(factionThatImprisoned)) {
                    isPrisonerOfHostileFaction = true;
                }
            }
            if (isPrisonerOfHostileFaction) {
                reactions.Add(EMOTION.Concern);
            } else {
                reactions.Add(EMOTION.Disgust);
                reactions.Add(EMOTION.Fear);
            }
        }
    }
    //public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
    //    string reaction = base.ReactionToActor(actor, target, witness, node, status);
    //    if (node.otherData.Length == 1 && node.otherData[0].obj is LocationStructure structure) {
    //        if (witness.faction.isMajorNonPlayer && witness.homeSettlement != null && 
    //            (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasSpecialPositiveRelationshipWith(actor)) && 
    //            !witness.relationshipContainer.IsEnemiesWith(actor) && structure == actor.currentStructure) {
    //            if (!witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Rescue, actor) 
    //                && !witness.faction.partyQuestBoard.HasPartyQuestWithTarget(PARTY_QUEST_TYPE.Demon_Rescue, actor)) {
    //                witness.faction.partyQuestBoard.CreateRescuePartyQuest(witness, witness.homeSettlement, actor);
    //            }
    //        }
    //    }
    //    return reaction;
    //}
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Neutral;
    }
#endregion

#region State Effects
    public void PreImprisonedSuccess(ActualGoapNode goapNode) { }
    public void PerTickImprisonedSuccess(ActualGoapNode goapNode) { }
    public void AfterImprisonedSuccess(ActualGoapNode goapNode) { }
#endregion
}
