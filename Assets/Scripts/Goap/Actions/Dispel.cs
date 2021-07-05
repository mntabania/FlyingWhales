using Traits;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class Dispel : GoapAction {
    public Dispel() : base(INTERACTION_TYPE.DISPEL) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Life_Changes};
        showNotification = true;
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Dispel Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Positive;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        string traitToRemove = (string)node.otherData[0].obj;
        string logString = GetTraitLogString(traitToRemove);
        log.AddToFillers(null, logString, LOG_IDENTIFIER.STRING_1);
    }
#endregion
    
#region State Effects
    public void PreDispelSuccess(ActualGoapNode goapNode) {
        string traitToRemove = (string)goapNode.otherData[0].obj;
        if (goapNode.poiTarget.traitContainer.HasTrait(traitToRemove)) {
            //create result logs
            //NOTE: This can become an issue if something external can remove the trait of the target character while they are being targeted by this action.
            Character targetCharacter = goapNode.poiTarget as Character;
            Assert.IsNotNull(targetCharacter);
            string logString = GetTraitLogString(traitToRemove);
            if (traitToRemove == "Lycanthrope") {
                if (targetCharacter.lycanData != null && targetCharacter.lycanData.isMaster && !targetCharacter.lycanData.dislikesBeingLycan) {
                    CreateResultLog(targetCharacter.limiterComponent.canPerform ? "refuse" : "success", logString, goapNode);
                } else {
                    //lycanthropy is removed
                    CreateResultLog("success", logString, goapNode);
                }
            } else if (traitToRemove == "Vampire") {
                Vampire vampire = targetCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampire != null && !vampire.dislikedBeingVampire) {
                    CreateResultLog(targetCharacter.limiterComponent.canPerform ? "refuse" : "success", logString, goapNode);
                } else {
                    //lycanthropy is removed
                    CreateResultLog("success", logString, goapNode);
                }
            }    
        }
    }
    public void AfterDispelSuccess(ActualGoapNode goapNode) {
        goapNode.actor.moneyComponent.AdjustCoins(83);
        string traitToRemove = (string)goapNode.otherData[0].obj;
        if (goapNode.poiTarget.traitContainer.HasTrait(traitToRemove)) {
            Character targetCharacter = goapNode.poiTarget as Character;
            Character actor = goapNode.actor;
            Assert.IsNotNull(targetCharacter);
            if (traitToRemove == "Lycanthrope") {
                if (targetCharacter.interruptComponent.isInterrupted && 
                    (targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Transform_To_Wolf || 
                     targetCharacter.interruptComponent.currentInterrupt.interrupt.type == INTERRUPT.Transform_To_Werewolf)) {
                    targetCharacter.interruptComponent.ForceEndNonSimultaneousInterrupt();
                }
                if (targetCharacter.lycanData.isMaster && !targetCharacter.lycanData.dislikesBeingLycan) {
                    if (targetCharacter.limiterComponent.canPerform) {
                        //lycanthropy is kept
                        actor.relationshipContainer.AdjustOpinion(actor, targetCharacter, "Refused my help.", -8);
                    } else {
                        //lycanthropy is removed
                        targetCharacter.traitContainer.RemoveTrait(targetCharacter, traitToRemove, actor);
                        targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, actor, "Removed my Lycanthropy!", -15);
                    }
                } else {
                    //lycanthropy is removed
                    targetCharacter.traitContainer.RemoveTrait(targetCharacter, traitToRemove, actor);
                    targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, actor, "Removed my Lycanthropy!", 15);
                }
            } else if (traitToRemove == "Vampire") {
                Vampire vampire = targetCharacter.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                if (vampire != null && !vampire.dislikedBeingVampire) {
                    if (targetCharacter.limiterComponent.canPerform) {
                        //Vampire is kept
                        actor.relationshipContainer.AdjustOpinion(actor, targetCharacter, "Refused my help.", -8);
                    } else {
                        //Vampire is removed
                        targetCharacter.traitContainer.RemoveTrait(targetCharacter, traitToRemove, actor);
                        //NOTE: Moved this to OnRemoveTrait of Vampire
                        // if (targetCharacter.characterClass.className == "Vampire Lord") {
                        //     targetCharacter.AssignClass("Peasant");
                        // }
                        targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, actor, "Removed my Vampirism!", -15);
                    }
                } else {
                    //Vampire is removed
                    targetCharacter.traitContainer.RemoveTrait(targetCharacter, traitToRemove, actor);
                    //NOTE: Moved this to OnRemoveTrait of Vampire
                    // if (targetCharacter.characterClass.className == "Vampire Lord") {
                    //     targetCharacter.AssignClass("Peasant");
                    // }
                    targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, actor, "Removed my Vampirism!", 15);
                }
            }    
        }
        goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(10, goapNode.actor);
    }
#endregion

    private string GetTraitLogString(string traitName) {
        if (traitName == "Lycanthrope") {
            return "Lycanthropy";
        } else {
            return "Vampirism";
        }
    }
    private void CreateResultLog(string result, string logTraitName, ActualGoapNode goapNode) {
        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Dispel", result, goapNode, logTags);
        log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddToFillers(null, logTraitName, LOG_IDENTIFIER.STRING_1);
        goapNode.OverrideDescriptionLog(log);
    }
}