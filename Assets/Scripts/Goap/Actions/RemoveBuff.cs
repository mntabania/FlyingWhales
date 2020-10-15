using System.Collections.Generic;
using Logs;
using Traits;
using UnityEngine;
using UtilityScripts;

public class RemoveBuff : GoapAction {
    public RemoveBuff() : base(INTERACTION_TYPE.REMOVE_BUFF) {
        doesNotStopTargetCharacter = true;
        actionIconString = GoapActionStateDB.Stealth_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Crimes};
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", false, GOAP_EFFECT_TARGET.ACTOR), HasCultistKit);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Buff Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +0(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 0;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override void AddFillersToLog(ref Log log, ActualGoapNode node) {
        base.AddFillersToLog(ref log, node);
        OtherData[] otherData = node.otherData;
        if(otherData != null && otherData.Length == 1 && otherData[0] is StringOtherData stringOtherData) {
            log.AddToFillers(null, stringOtherData.str, LOG_IDENTIFIER.STRING_1);
        }
    }
    #endregion

    #region Preconditions
    private bool HasCultistKit(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem("Cultist Kit");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest target, OtherData[] otherData) {
        bool hasMetRequirements = base.AreRequirementsSatisfied(actor, target, otherData);
        if (hasMetRequirements) {
            return target != actor && target.traitContainer.HasTrait("Resting", "Unconscious") && target.traitContainer.HasTraitOf(TRAIT_TYPE.BUFF);
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
    public override string ReactionToActor(Character actor, IPointOfInterest poiTarget, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, poiTarget, witness, node, status);
        if (witness.traitContainer.HasTrait("Cultist") == false) {
            //not a cultist
            //CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_SEVERITY.Serious);
            CrimeManager.Instance.ReactToCrime(witness, actor, poiTarget, poiTarget.factionOwner, node.crimeType, node, status);

            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            if (witness.relationshipContainer.IsFriendsWith(actor) || witness.relationshipContainer.HasOpinionLabelWithCharacter(actor, RelationshipManager.Acquaintance)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node); 
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (witness.traitContainer.HasTrait("Psychopath") == false) {
                //witness is not a psychopath
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
            if (witness.relationshipContainer.IsEnemiesWith(actor) == false) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
            }
        }
        else {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
            if (RelationshipManager.IsSexuallyCompatible(witness, actor)) {
                int chance = 10 * witness.relationshipContainer.GetCompatibility(actor);
                int roll = Random.Range(0, 100);
                if (roll < chance) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);                    
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node,
        REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        if (target is Character targetCharacter) {
            //CrimeManager.Instance.ReactToCrime(targetCharacter, actor, node, node.associatedJobType, CRIME_SEVERITY.Serious);
            CrimeManager.Instance.ReactToCrime(targetCharacter, actor, target, target.factionOwner, node.crimeType, node, status);

            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
            if (targetCharacter.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor, status, node);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
            }

            if (targetCharacter.relationshipContainer.IsFriendsWith(actor)) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
            }
        }

        return response;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Demon_Worship;
    }
    #endregion
}