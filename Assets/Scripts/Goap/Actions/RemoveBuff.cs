using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;

public class RemoveBuff : GoapAction {
    public RemoveBuff() : base(INTERACTION_TYPE.REMOVE_BUFF) {
        doesNotStopTargetCharacter = true;
        actionIconString = GoapActionStateDB.Cult_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }
    
    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Buff Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +0(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 0;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        OtherData[] otherData = node.otherData;
        if(otherData != null && otherData.Length == 1 && otherData[0] is StringOtherData stringOtherData) {
            log.AddToFillers(null, stringOtherData.str, LOG_IDENTIFIER.STRING_1);
        }
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData, JobQueueItem job) {
        bool hasMetRequirements = base.AreRequirementsSatisfied(actor, target, otherData, job);
        if (hasMetRequirements) {
            return target != actor && target.traitContainer.HasTrait("Resting", "Unconscious") && target.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF) && actor.HasItem(TILE_OBJECT_TYPE.CULTIST_KIT);
        }
        return false;
    }
#endregion
    
#region State Effects
    public void AfterRemoveBuffSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        if (otherData != null && otherData.Length == 1 && otherData[0] is StringOtherData stringOtherData) {
            string traitName = stringOtherData.str;
            goapNode.target.traitContainer.RemoveTrait(goapNode.target, traitName, goapNode.actor);
            goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
        }

        //List<Trait> buffs = goapNode.target.traitContainer.GetAllTraitsOf(TRAIT_TYPE.BUFF);
        //Trait randomBuff = CollectionUtilities.GetRandomElement(buffs);
        //goapNode.target.traitContainer.RemoveTrait(goapNode.target, randomBuff, goapNode.actor);
        //goapNode.descriptionLog.AddToFillers(null, randomBuff.name, LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.UpdateLogInInvolvedObjects();
        //goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.CULTIST_KIT);
        //Messenger.Broadcast(Signals.UPDATE_ALL_NOTIFICATION_LOGS, goapNode.descriptionLog);
    }
#endregion

#region Reactions
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            //not a cultist
            reactions.Add(EMOTION.Shock);
            if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Acquaintance)) {
                reactions.Add(EMOTION.Despair);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                //witness is not a psychopath
                reactions.Add(EMOTION.Threatened);
            }
            if (witness.relationshipContainer.IsEnemiesWith(actor) == false) {
                reactions.Add(EMOTION.Disapproval);
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
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            reactions.Add(EMOTION.Shock);
            if (targetCharacter.traitContainer.HasTrait("Coward")) {
                reactions.Add(EMOTION.Fear);
            } else {
                reactions.Add(EMOTION.Threatened);
            }

            if (targetCharacter.relationshipContainer.IsFriendsWith(actor)) {
                reactions.Add(EMOTION.Betrayal);
            }
        }
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Demon_Worship;
    }
#endregion
}