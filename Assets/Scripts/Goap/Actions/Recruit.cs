using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using System.Linq;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine.Assertions;

public class Recruit : GoapAction {

    public Recruit() : base(INTERACTION_TYPE.RECRUIT) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Recruit Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        int cost = 10;
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}:";
        costLog += $" +{cost}(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return cost;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = node.poiTarget as Character;
            Assert.IsNotNull(targetCharacter, $"Recruit of {node.actor.name} is not a character! {node.poiTarget?.ToString() ?? "Null"}");
            WeightedDictionary<bool> weightDictionary = new WeightedDictionary<bool>();
            int successWeight = 100;
            int failWeight = 200;

            if (node.actor.traitContainer.HasTrait("Inspiring")) {
                successWeight += 100;
            }
            if (node.actor.traitContainer.HasTrait("Persuasive")) {
                successWeight += 500;
            }

            // failWeight = 0;
            
            weightDictionary.AddElement(true, successWeight);
            weightDictionary.AddElement(false, failWeight);

            bool result = weightDictionary.PickRandomElementGivenWeights();
            if (!result) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Recruit Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            if (targetCharacter.prevFaction != null && targetCharacter.prevFaction.leader == witness) {
                reactions.Add(EMOTION.Anger);
            }
        }
    }
#endregion

    #region Effects
    public void AfterRecruitSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.ChangeFactionTo(actor.faction, bypassIdeologyChecking: true);
        targetCharacter.MigrateHomeTo(actor.homeSettlement);
        if (targetCharacter is FireElemental) {
            //NOTE: If target is fire elemental, do not set home structure, only home settlement
            //Reference: https://www.notion.so/ruinarch/27d2d290c43d40dbb0e8cf7c38b09ae2?v=e607fbe0a1ac49b8ac63649e2bdef458&p=cf8c2d7da65f458eb18378c834a42488
            //TODO: Find a way to make this more abstracted
            targetCharacter.MigrateHomeStructureTo(null, affectSettlement: false);
        } else if (targetCharacter is VengefulGhost) {
            //Just to make sure that vengeful ghosts will stop attacking a demonic structure once they  are recruited.
            //This can happen if vengeful ghost already has a target demonic structure but is restrained and recruited on its way there
            targetCharacter.behaviourComponent.SetIsAttackingDemonicStructure(false, null);
            targetCharacter.behaviourComponent.UpdateDefaultBehaviourSet();
        }
        targetCharacter.traitContainer.RemoveRestrainAndImprison(targetCharacter, goapNode.actor);
        targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Criminal");
        if (targetCharacter is Summon) {
            //Reference: https://trello.com/c/T2CnOQWD/4674-heal-newly-recruited-monsters-with-low-hp
            targetCharacter.AdjustHP(targetCharacter.maxHP, ELEMENTAL_TYPE.Normal, showHPBar: true);
        }
    }
    #endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor.faction != poiTarget.factionOwner && actor.homeSettlement != null;
        }
        return false;
    }
#endregion
}