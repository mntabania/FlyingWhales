using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps.Location_Structures;
using Inner_Maps;

public class RitualKilling : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    //TODO: This is one of the ways to optimize actions, situational preconditions can be cached at the beginning so that we will not call new Precondition every time
    //TODO: This also applies to expected effects
    //CREATE A SYSTEM FOR THIS
    private Precondition atHomePrecondition;
    private Precondition notAtHomePrecondition;
    
    public RitualKilling() : base(INTERACTION_TYPE.RITUAL_KILLING) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        isNotificationAnIntel = true;
        atHomePrecondition = new Precondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", target = GOAP_EFFECT_TARGET.TARGET }, HasRestrained);
        notAtHomePrecondition = new Precondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetInWildernessOrHome);
        logTags = new[] {LOG_TAG.Crimes, LOG_TAG.Life_Changes};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Killing Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override List<Precondition> GetPreconditions(Character actor, IPointOfInterest target, OtherData[] otherData) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            List<Precondition> p = new List<Precondition>();
            if (actor.homeStructure == targetCharacter.currentStructure) {
                p.Add(atHomePrecondition);
            } else {
                p.Add(notAtHomePrecondition);
            }
            return p;
        }
        return base.GetPreconditions(actor, target, otherData);
    }
    //public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
    //    GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
    //    Character actor = node.actor;
    //    IPointOfInterest poiTarget = node.poiTarget;
    //    if (goapActionInvalidity.isInvalid == false) {
    //        if (actor.marker.CanDoStealthActionToTarget(poiTarget) == false) {
    //            goapActionInvalidity.isInvalid = true;
    //            goapActionInvalidity.stateName = "Killing Fail";
    //        }
    //    }
    //    return goapActionInvalidity;
    //}
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        if (target is Character) {
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, actor, status, node);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);

                Character targetCharacter = target as Character;
                if (witness.relationshipContainer.IsFriendsWith(actor) && !witness.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, actor, status, node);
                }
                if (witness.relationshipContainer.IsFriendsWith(targetCharacter)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                }
            }
        }
        //CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_SEVERITY.Serious);
        CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);
        return response;
    }
    public override string ReactionToTarget(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToTarget(actor, target, witness, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (!witness.traitContainer.HasTrait("Psychopath")) {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Concern, witness, target, status, node);
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node,
        REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (targetCharacter.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor, status, node);
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);
            } else {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);

                if (targetCharacter.relationshipContainer.IsFriendsWith(actor) && !targetCharacter.traitContainer.HasTrait("Psychopath")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
                }
            }
            //CrimeManager.Instance.ReactToCrime(targetCharacter, actor, node, node.associatedJobType, CRIME_SEVERITY.Serious);
            CrimeManager.Instance.ReactToCrime(targetCharacter, actor, target, target.factionOwner, node.crimeType, node, status);
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Murder;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor != poiTarget && actor.traitContainer.HasTrait("Psychopath");
        }
        return false;
    }
    private bool IsTargetInWildernessOrHome(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType) {
        if(target is Character && otherData != null) {
            Character targetCharacter = target as Character;
            bool isSatisfied = false;
            if(otherData.Length == 1) {
                if(otherData[0] is HexTileOtherData hex) {
                    isSatisfied = targetCharacter.gridTileLocation.collectionOwner.isPartOfParentRegionMap && target.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == hex.hexTile;
                } else if (otherData[0] is LocationStructureOtherData structure) {
                    isSatisfied = targetCharacter.currentStructure == structure.locationStructure;
                } else if (otherData[0] is LocationGridTileOtherData gridTile) {
                    isSatisfied = targetCharacter.gridTileLocation == gridTile.tile;
                }
            }
            return targetCharacter.carryComponent.IsNotBeingCarried() && targetCharacter.traitContainer.HasTrait("Restrained") && isSatisfied; //targetCharacter.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || 
        }
        return false;
    }
    private bool HasRestrained(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType) {
        return target.traitContainer.HasTrait("Restrained");
    }
    #endregion

    #region State Effects
    public void PreKillingSuccess(ActualGoapNode goapNode) {
        
    }
    public void AfterKillingSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.needsComponent.AdjustHappiness(10000);
        if (goapNode.poiTarget is Character targetCharacter) {
            NPCSettlement settlementOfTarget = targetCharacter.homeSettlement;
            targetCharacter.Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
            goapNode.actor.jobComponent.TriggerBuryPsychopathVictim(targetCharacter, settlementOfTarget);
            targetCharacter.reactionComponent.AddCharacterThatSawThisDead(goapNode.actor);
        }
    }
    #endregion
}

public class RitualKillingData : GoapActionData {
    public RitualKillingData() : base(INTERACTION_TYPE.RITUAL_KILLING) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && actor.traitContainer.HasTrait("Psychopath");
    }
}

