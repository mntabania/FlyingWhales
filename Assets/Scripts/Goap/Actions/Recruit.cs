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
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        //isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Recruit Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 10;
        costLog += $" +{cost}(Constant)";
        actor.logComponent.AppendCostLog(costLog);
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
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);

        if (target is Character targetCharacter) {
            if (targetCharacter.prevFaction != null && witness.prevFaction.leader == witness) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
            }
        }
        return response;
    }
    #endregion

    #region Effects
    public void AfterRecruitSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        Character targetCharacter = goapNode.poiTarget as Character;
        targetCharacter.ChangeFactionTo(actor.faction, bypassIdeologyChecking: true);
        targetCharacter.MigrateHomeTo(targetCharacter.currentSettlement);
        targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Restrained");
        targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Criminal");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor.faction != poiTarget.factionOwner;
        }
        return false;
    }
    #endregion
}