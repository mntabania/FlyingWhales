using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Pray : GoapAction {

    public Pray() : base(INTERACTION_TYPE.PRAY) {
        this.goapName = "Pray";
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.EARLY_NIGHT, TIME_IN_WORDS.LATE_NIGHT, TIME_IN_WORDS.AFTER_MIDNIGHT };
        actionIconString = GoapActionStateDB.Pray_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        logTags = new[] {LOG_TAG.Needs};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Pray Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = UtilityScripts.Utilities.Rng.Next(90, 131);
        costLog += $" +{cost}(Initial)";
        int numOfTimesActionDone = actor.jobComponent.GetNumOfTimesActionDone(this);
        if (numOfTimesActionDone > 5) {
            cost += 2000;
            costLog += " +2000(Times Prayed > 5)";
        }
        if (actor.religionComponent.religion != RELIGION.Demon_Worship && actor.traitContainer.HasTrait("Evil", "Psychopath")) {
            cost += 2000;
            costLog += " +2000(Evil/Psychopath)";
        }
        if (actor.traitContainer.HasTrait("Chaste")) {
            cost -= 15;
            costLog += " -15(Chaste)";
        }
        int timesCost = 10 * numOfTimesActionDone;
        cost += timesCost;
        costLog += $" +{timesCost}(10 x Times Prayed)";
        
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.needsComponent.AdjustDoNotGetBored(-1);
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if (actor.religionComponent.religion == RELIGION.Demon_Worship) {
            return CRIME_TYPE.Demon_Worship;
        } else if (actor.religionComponent.religion == RELIGION.Nature_Worship) {
            return CRIME_TYPE.Nature_Worship;
        } else if (actor.religionComponent.religion == RELIGION.Divine_Worship) {
            return CRIME_TYPE.Divine_Worship;
        }
        return base.GetCrimeType(actor, target, crime);
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        Character targetCharacter = target as Character;
        
        CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, node.crimeType);
        
        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
            CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            if (witness.relationshipContainer.IsFriendsWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);    
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }

            if (targetCharacter != null && !witness.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
            }
        } else if (witness.religionComponent.religion == actor.religionComponent.religion) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
        }
        return response;
    }
    #endregion

    #region State Effects
    public void PrePraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        goapNode.actor.jobComponent.IncreaseNumOfTimesActionDone(this);
    }
    public void PerTickPraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustHappiness(1.6f);
        //goapNode.actor.needsComponent.AdjustStamina(0.5f);
    }
    public void AfterPraySuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        if (goapNode.actor.religionComponent.religion == RELIGION.Demon_Worship) {
            //Demon Worshippers produce 1 Chaos Orb when they Pray
            //https://trello.com/c/qnZzSwcW/2590-demon-worshippers-produce-1-chaos-orb-when-they-pray
            Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, goapNode.poiTarget.worldPosition, 1, goapNode.poiTarget.gridTileLocation.parentMap);
        }
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.IsTrappedAndTrapStructureIsNot(poiTarget.gridTileLocation.structure)) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.trapStructure.IsTrappedAndTrapHexIsNot(poiTarget.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner)) {
                return false;
            }
            if (actor.traitContainer.HasTrait("Evil")) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion
}