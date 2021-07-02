using System;
using System.Collections;
using System.Collections.Generic;
using Goap.Unique_Action_Data;
using UnityEngine;  
using Traits;

public class FirstAidCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.DIRECT;
    public override Type uniqueActionDataType => typeof(FirstAidCharacterUAD);
    public FirstAidCharacter() : base(INTERACTION_TYPE.FIRST_AID_CHARACTER) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        SetPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Healing Potion", false, GOAP_EFFECT_TARGET.ACTOR), HasHealingPotion);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Injured", false, GOAP_EFFECT_TARGET.TARGET));
        //AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Unconscious", false, GOAP_EFFECT_TARGET.TARGET));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("First Aid Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = string.Empty;
#endif
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            //target is at structure that character is avoiding
#if DEBUG_LOG
            costLog += $" +2000(Location of target is in avoid structure)";
            actor.logComponent.AppendCostLog(costLog);
#endif
            return 2000;
        }
#if DEBUG_LOG
        costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).carryComponent.IsNotBeingCarried() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_carried";
            }
        }
        return goapActionInvalidity;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character targetCharacter) {
            string opinionOfTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
            FirstAidCharacterUAD data = node.GetConvertedUniqueActionData<FirstAidCharacterUAD>();
            if (data.usedPoisonedHealingPotion) {
                reactions.Add(EMOTION.Shock);
                if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                    reactions.Add(EMOTION.Anger);
                }
            } else {
                if (opinionOfTarget == RelationshipManager.Friend || opinionOfTarget == RelationshipManager.Close_Friend) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        reactions.Add(EMOTION.Gratefulness);
                    }
                } else if (opinionOfTarget == RelationshipManager.Rival) {
                    reactions.Add(EMOTION.Disapproval);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character targetCharacter) {
            FirstAidCharacterUAD data = node.GetConvertedUniqueActionData<FirstAidCharacterUAD>();
            if (data.usedPoisonedHealingPotion) {
                reactions.Add(EMOTION.Shock);
                reactions.Add(EMOTION.Betrayal);
            } else {
                if (!targetCharacter.traitContainer.HasTrait("Psychopath")) {
                    if (targetCharacter.relationshipContainer.IsEnemiesWith(actor)) {
                        if (UnityEngine.Random.Range(0, 100) < 30) {
                            reactions.Add(EMOTION.Gratefulness);
                        }
                        if (UnityEngine.Random.Range(0, 100) < 20) {
                            reactions.Add(EMOTION.Embarassment);

                        }
                    } else {
                        reactions.Add(EMOTION.Gratefulness);
                    }
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.poiTarget is Character character) {
            FirstAidCharacterUAD data = node.GetConvertedUniqueActionData<FirstAidCharacterUAD>();
            if (witness.IsHostileWith(character) || data.usedPoisonedHealingPotion) {
                return REACTABLE_EFFECT.Negative;
            }
        }
        return REACTABLE_EFFECT.Positive;
    }
#endregion

#region State Effects
    public void PreFirstAidSuccess(ActualGoapNode goapNode) {
        TileObject chosenHealingPotion = goapNode.actor.GetItem(TILE_OBJECT_TYPE.HEALING_POTION);
        if (chosenHealingPotion != null && chosenHealingPotion.traitContainer.HasTrait("Poisoned")) {
            FirstAidCharacterUAD data = goapNode.GetConvertedUniqueActionData<FirstAidCharacterUAD>();
            data.SetUsedPoisonedHealingPotion(true);
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", "First Aid Character", "used_poison", goapNode, logTags);
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            goapNode.OverrideDescriptionLog(log);
        }
    }
    public void AfterFirstAidSuccess(ActualGoapNode goapNode) {
        FirstAidCharacterUAD data = goapNode.GetConvertedUniqueActionData<FirstAidCharacterUAD>();
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
            goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Injured", goapNode.actor);
            goapNode.actor.UnobtainItem(TILE_OBJECT_TYPE.HEALING_POTION);
        }
        
        
        // Character targetCharacter = goapNode.poiTarget as Character;
        // if (targetCharacter != goapNode.actor) {
        //     targetCharacter.relationshipContainer.AdjustOpinion(targetCharacter, goapNode.actor, "Base", 3);    
        // }
        // goapNode.poiTarget.traitContainer.RemoveStatusAndStacks(goapNode.poiTarget, "Injured", goapNode.actor);
    }
#endregion

#region Precondition
    private bool HasHealingPotion(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.HasItem(TILE_OBJECT_TYPE.HEALING_POTION);
    }
#endregion
}