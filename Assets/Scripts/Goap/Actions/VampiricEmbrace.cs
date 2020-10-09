using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class VampiricEmbrace : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public VampiricEmbrace() : base(INTERACTION_TYPE.VAMPIRIC_EMBRACE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Drink_Blood_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Embrace Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest target, Character witness,
        ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, target, witness, node, status);
        Vampire vampire = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
        if (vampire != null) {
            vampire.AddAwareCharacter(witness);
        }
        CrimeManager.Instance.ReactToCrime(witness, actor, target, target.factionOwner, node.crimeType, node, status);

        CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(witness, actor, target, CRIME_TYPE.Vampire);
        if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, actor, status, node);
            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(actor);
            if (opinionLabel == RelationshipManager.Acquaintance || opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Despair, witness, actor, status, node);
            }
            if (witness.traitContainer.HasTrait("Coward")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, witness, actor, status, node);
            } else if (!witness.traitContainer.HasTrait("Psychopath")) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, witness, actor, status, node);
            }
            if (target is Character targetCharacter) {
                string opinionToTarget = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionToTarget == RelationshipManager.Friend || opinionToTarget == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                } else if ((witness.relationshipContainer.IsFamilyMember(targetCharacter) || witness.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                     && opinionToTarget != RelationshipManager.Rival) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                } else if (opinionToTarget == RelationshipManager.Acquaintance || witness.faction == targetCharacter.faction || witness.homeSettlement == targetCharacter.homeSettlement) {
                    if (!witness.traitContainer.HasTrait("Psychopath")) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    }
                }
            }
        } else {
            if (witness.traitContainer.HasTrait("Hemophiliac")) {
                if(RelationshipManager.IsSexuallyCompatible(witness.sexuality, actor.sexuality, witness.gender, actor.gender)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, witness, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                }
            }
        }
        return response;
    }
    public override string ReactionOfTarget(Character actor, IPointOfInterest target, ActualGoapNode node,
        REACTION_STATUS status) {
        string response = base.ReactionOfTarget(actor, target, node, status);
        if (target is Character targetCharacter) {
            Vampire vampire = actor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            if (vampire != null) {
                vampire.AddAwareCharacter(targetCharacter);
            }
            CrimeManager.Instance.ReactToCrime(targetCharacter, actor, targetCharacter, target.factionOwner, node.crimeType, node, status);

            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(targetCharacter, actor, target, CRIME_TYPE.Vampire);
            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, targetCharacter, actor, status, node);

                string opinionLabel = targetCharacter.relationshipContainer.GetOpinionLabel(actor);
                if (targetCharacter.traitContainer.HasTrait("Coward")) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, targetCharacter, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Threatened, targetCharacter, actor, status, node);
                }
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, targetCharacter, actor, status, node);
                }
            } else {
                if (targetCharacter.traitContainer.HasTrait("Hemophiliac")) {
                    if (RelationshipManager.IsSexuallyCompatible(targetCharacter.sexuality, actor.sexuality, targetCharacter.gender, actor.gender)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Arousal, targetCharacter, actor, status, node);
                    } else {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, targetCharacter, actor, status, node);
                    }
                }
            }
        }
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Vampire;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if(poiTarget is Character targetCharacter) {
                return actor != targetCharacter && actor.traitContainer.HasTrait("Vampire") && !targetCharacter.isDead && targetCharacter.carryComponent.IsNotBeingCarried();
            }
            return actor != poiTarget && actor.traitContainer.HasTrait("Vampire");
        }
        return false;
    }
    #endregion

    #region Effects
    public void AfterEmbraceSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        if (goapNode.poiTarget is Character targetCharacter) {
            if (targetCharacter.HasItem("Phylactery")) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "activate_phylactery", goapNode, LOG_TAG.Misc);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToDatabase();

                targetCharacter.UnobtainItem("Phylactery");
                actor.AdjustHP(-500, ELEMENTAL_TYPE.Normal);
                if(actor.currentHP <= 0) {
                    actor.Death(deathFromAction: goapNode, responsibleCharacter: targetCharacter, _deathLog: goapNode.descriptionLog);
                } else {
                    actor.traitContainer.AddTrait(actor, "Unconscious", targetCharacter, goapNode);
                }
            } else {
                if (targetCharacter.isDead) {
                    targetCharacter.ReturnToLife();
                }

                Vampire vampire = TraitManager.Instance.CreateNewInstancedTraitClass<Vampire>("Vampire");
                targetCharacter.traitContainer.AddTrait(targetCharacter, vampire, actor);
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, "contracted", goapNode, LOG_TAG.Life_Changes);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFrom(actor, log);

                targetCharacter.traitContainer.RemoveStatusAndStacks(targetCharacter, "Injured");
                targetCharacter.traitContainer.RemoveStatusAndStacks(targetCharacter, "Infected");
                targetCharacter.traitContainer.RemoveStatusAndStacks(targetCharacter, "Plagued");

            }
        }

        //Infected infected = goapNode.poiTarget.traitContainer.GetTraitOrStatus<Infected>("Infected");
        //infected?.InfectTarget(actor);
    }
    #endregion
}