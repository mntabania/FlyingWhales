using System;
using System.Diagnostics;
using Goap.Unique_Action_Data;

public class CureCharacter : GoapAction {
    public override Type uniqueActionDataType => typeof(CureCharacterUAD);
    
    public CureCharacter() : base(INTERACTION_TYPE.CURE_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Cure_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Healing Potion", false, GOAP_EFFECT_TARGET.ACTOR), HasItemInInventory);
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Poisoned", false, GOAP_EFFECT_TARGET.TARGET));
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Plagued", false, GOAP_EFFECT_TARGET.TARGET));

    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Cure Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = "";
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            //target is at structure that character is avoiding
            costLog += $" +2000(Location of target is in avoid structure)";
            actor.logComponent.AppendCostLog(costLog);
            return 2000;
        }
        costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).carryComponent.IsNotBeingCarried() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        Character targetCharacter = target as Character;
        string opinionOfTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
        CureCharacterUAD data = node.GetConvertedUniqueActionData<CureCharacterUAD>();
        if (data.usedPoisonedHealingPotion) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);    
            }
        } else {
            if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                if (!witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, witness, actor, status, node);
                }
            } else if (opinionOfTarget == RelationshipManager.Rival) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
            }    
        }
        
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        Character targetCharacter = target as Character;
        Debug.Assert(targetCharacter != null, nameof(targetCharacter) + " != null");
        CureCharacterUAD data = node.GetConvertedUniqueActionData<CureCharacterUAD>();
        if (data.usedPoisonedHealingPotion) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
        } else {
            if (!targetCharacter.traitContainer.HasTrait("Psychopath")) {
                if (targetCharacter.relationshipContainer.IsEnemiesWith(actor)) {
                    if(UnityEngine.Random.Range(0, 100) < 30) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor, status, node);
                    }
                    if (UnityEngine.Random.Range(0, 100) < 20) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, targetCharacter, actor, status, node);
                    }
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Gratefulness, targetCharacter, actor, status, node);
                }
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is Character character) {
            CureCharacterUAD data = node.GetConvertedUniqueActionData<CureCharacterUAD>();
            if (witness.IsHostileWith(character) || data.usedPoisonedHealingPotion) {
                return REACTABLE_EFFECT.Negative;
            }    
        }
        return REACTABLE_EFFECT.Positive;
    }
    #endregion

    #region State Effects
    public void PreCureSuccess(ActualGoapNode goapNode) {
        TileObject chosenHealingPotion = goapNode.actor.GetItem(TILE_OBJECT_TYPE.HEALING_POTION);
        if (chosenHealingPotion != null && chosenHealingPotion.traitContainer.HasTrait("Poisoned")) {
            CureCharacterUAD data = goapNode.GetConvertedUniqueActionData<CureCharacterUAD>();
            data.SetUsedPoisonedHealingPotion(true);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "Cure Character", "used_poison", goapNode, logTags);
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            goapNode.OverrideDescriptionLog(log);
        }
    }
    public void AfterCureSuccess(ActualGoapNode goapNode) {
        CureCharacterUAD data = goapNode.GetConvertedUniqueActionData<CureCharacterUAD>();
        if(goapNode.poiTarget is Character targetCharacter && goapNode.actor != targetCharacter) {
            if (data.usedPoisonedHealingPotion) {
                targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, goapNode.actor, "Poisoned me.", -10);
            } else {
                targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, goapNode.actor, "Helped me.", 5);
            }
        }
        if (data.usedPoisonedHealingPotion) {
            goapNode.poiTarget.traitContainer.AddTrait(goapNode.poiTarget, "Poisoned", goapNode.actor, bypassElementalChance: true);
            goapNode.poiTarget.AdjustHP(-300, ELEMENTAL_TYPE.Normal, true, goapNode.actor);
            //specifically remove poisoned healing potion from inventory, if none exist just remove a random one.
            bool foundPoisonedPotion = false;
            for (int i = 0; i < goapNode.actor.items.Count; i++) {
                TileObject item = goapNode.actor.items[i];
                if (item.tileObjectType == TILE_OBJECT_TYPE.HEALING_POTION && item.traitContainer.HasTrait("Poisoned")) {
                    goapNode.actor.UnobtainItem(item);
                    foundPoisonedPotion = true;
                    break;
                }
            }
            if (!foundPoisonedPotion) {
                goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
            }
        } else {
            goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Poisoned", goapNode.actor);
            goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Plagued", goapNode.actor);
            goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Infected", goapNode.actor);
            goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
        }
    }
    #endregion

    #region Preconditions
    private bool HasItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
        //return true;
    }
    #endregion

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