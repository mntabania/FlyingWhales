using System;
using System.Diagnostics;
using System.Collections.Generic;
using Goap.Unique_Action_Data;

public class HealerCure : GoapAction {
    public override Type uniqueActionDataType => typeof(CureCharacterUAD);

    public HealerCure() : base(INTERACTION_TYPE.HEALER_CURE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Cure_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.RATMAN };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Healer Cure Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = string.Empty;
#endif
        
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    #endregion

    #region State Effects
    
    public void AfterHealerCureSuccess(ActualGoapNode goapNode) {
        goapNode.actor.moneyComponent.AdjustCoins(33);
        // UnityEngine.Debug.LogError(goapNode.actor.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level);
        if (goapNode.actor.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level >= 3) {
            Level3Effect(goapNode);
        } else if (goapNode.actor.talentComponent.GetTalent(CHARACTER_TALENT.Healing_Magic).level >= 2) {
            Level2Effect(goapNode);
        }
        goapNode.actor.talentComponent?.GetTalent(CHARACTER_TALENT.Healing_Magic).AdjustExperience(10, goapNode.actor);
    }
    #endregion

    void Level2Effect(ActualGoapNode node) {
        if (node.target.traitContainer.HasTrait("Injured")) {
            node.target.traitContainer.RemoveStatusAndStacks(node.target, "Injured");
        }
       
        if (node.target.traitContainer.HasTrait("Poison")) {
            node.target.traitContainer.RemoveStatusAndStacks(node.target, "Poison");
        }
    }

    void Level3Effect(ActualGoapNode node) {
        Level2Effect(node);
        if (node.target.traitContainer.HasTrait("Plagued")) {
            node.target.traitContainer.RemoveStatusAndStacks(node.target, "Plagued");
        }
     
    }
    #region Requirements

    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return poiTarget is Character;
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> CureSuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    Character targetCharacter = poiTarget as Character;

    //    if (isOldNews) {
    //        //Old News
    //        reactions.Add("This is old news.");
    //    } else {
    //        //Not Yet Old News
    //        if (awareCharactersOfThisAction.Contains(recipient)) {
    //            //- If Recipient is Aware
    //            reactions.Add("I know that already.");
    //        } else {
    //            //- Recipient is Actor
    //            if (recipient == actor) {
    //                reactions.Add("I know what I did.");
    //            }
    //            //- Recipient is Target
    //            else if (recipient == targetCharacter) {
    //                reactions.Add(string.Format("I am grateful for {0}'s help.", actor.name));
    //            }
    //            //- Recipient Has Positive Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //                reactions.Add(string.Format("I am grateful that {0} helped {1}.", actor.name, targetCharacter.name));
    //            }
    //            //- Recipient Has Negative Relationship with Target
    //            else if (recipient.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //                reactions.Add(string.Format("{0} is such a chore.", targetCharacter.name));
    //            }
    //            //- Recipient Has No Relationship with Target
    //            else {
    //                reactions.Add(string.Format("That was nice of {0}.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //            }
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}