﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Interrupts;
using UnityEngine.Assertions;
using Crime_System;

public class ReportCrime : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public ReportCrime() : base(INTERACTION_TYPE.REPORT_CRIME) {
        actionIconString = GoapActionStateDB.Social_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        doesNotStopTargetCharacter = true;
        isNotificationAnIntel = true;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Report Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        object[] otherData = node.otherData;
        if (otherData.Length == 2 && otherData[0] is ICrimeable) {
            ICrimeable crime = otherData[0] as ICrimeable;
            //CrimeType crimeTypObj = CrimeManager.Instance.GetCrimeType(crime.crimeType);
            //log.AddToFillers(null, crimeTypObj.name, LOG_IDENTIFIER.STRING_1);
            Character criminal = crime.actor;
            if (crime.disguisedActor != null) {
                criminal = crime.disguisedActor;
            }
            log.AddToFillers(criminal, criminal.name, LOG_IDENTIFIER.CHARACTER_3);
        }
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.carryComponent.IsNotBeingCarried() == false) {
                goapActionInvalidity.isInvalid = true;
            } else if (!targetCharacter.canWitness) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    //public override string ReactionToActor(Character actor, IPointOfInterest poiTarget, Character witness,
    //    ActualGoapNode node, REACTION_STATUS status) {
    //    string response = base.ReactionToActor(actor, poiTarget, witness, node, status);
    //    object[] otherData = node.otherData;
    //    IReactable reactable = otherData[0] as IReactable;

    //    REACTABLE_EFFECT reactableEffect = reactable.GetReactableEffect(witness);
    //    if (reactableEffect == REACTABLE_EFFECT.Negative) {
    //        if (witness == reactable.actor) {
    //            if(reactable is ActualGoapNode) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, witness, actor, status, node);
    //            } else {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
    //            }
    //            if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
    //                || witness.relationshipContainer.IsFriendsWith(actor)) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status, node);
    //            }
    //        } else {
    //            if (witness.relationshipContainer.HasRelationshipWith(reactable.actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
    //                || witness.relationshipContainer.IsFriendsWith(reactable.actor)) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
    //            } else if (reactable.target is Character rumorTarget) {
    //                if (witness.relationshipContainer.HasRelationshipWith(rumorTarget, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
    //                || witness.relationshipContainer.IsFriendsWith(rumorTarget)) {
    //                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
    //                }
    //            }
    //        }
    //        CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_SEVERITY.Infraction);
    //    } else {
    //        if (witness == reactable.actor) {
    //            if (reactable is ActualGoapNode) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
    //            } else {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, witness, actor, status, node);
    //            }
    //        } else {
    //            if (witness.relationshipContainer.IsEnemiesWith(reactable.actor)) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
    //            } else if (reactable.target is Character rumorTarget) {
    //                if (witness.relationshipContainer.IsEnemiesWith(rumorTarget)) {
    //                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
    //                }
    //            }
    //        }
    //    }
    //    //SPECIAL CASE: After reacting to the Share Info Action itself, witness should also react to the rumor itself
    //    if(reactable.name != "Share Information") {
    //        ProcessInformation(node.actor, witness, reactable, node);
    //    }
    //    return response;
    //}
    //public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
    //    IReactable reactable = node.otherData[0] as IReactable;
    //    Assert.IsNotNull(reactable, $"{witness.name} is trying to get reactable effect of {node}, but reactable is null!");
    //    if (reactable.GetReactableEffect(witness) == REACTABLE_EFFECT.Negative) {
    //        return REACTABLE_EFFECT.Negative;
    //    }
    //    return REACTABLE_EFFECT.Neutral;
    //}
    #endregion

    #region State Effects
    public void AfterReportSuccess(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        ICrimeable crime = otherData[0] as ICrimeable;
        CrimeData crimeData = otherData[1] as CrimeData;
        Character sharer = goapNode.actor;
        Character recipient = goapNode.poiTarget as Character;
        sharer.crimeComponent.AddReportedCrime(crimeData);
        ProcessInformation(sharer, recipient, crime, goapNode);
    }
    private void ProcessInformation(Character sharer, Character recipient, ICrimeable crime, ActualGoapNode shareActionItself) {
        Character actor = crime.actor;
        IPointOfInterest target = crime.target;
        if(crime.disguisedActor != null) {
            actor = crime.disguisedActor;
        }
        if (crime.disguisedTarget != null) {
            target = crime.disguisedTarget;
        }

        if (actor.isDead) {
            //Report crime is still a success but will recipient will not do anything since criminal is already dead
            Log log = new Log(GameManager.Instance.Today(), "GoapAction", name, "dead_criminal");
            log.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.CHARACTER_3);
            log.AddLogToInvolvedObjects();
            return;
        }

        if (actor != recipient) {
            string weightLog = "Report crime of " + sharer.name + " to " + recipient.name + ": " + crime.name + " with actor " + actor.name + " and target " + target.name;
            weightLog += "\nBase Belief Weight: 50";
            weightLog += "\nBase Disbelief Weight: 50";

            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            int beliefWeight = 50;
            int disbeliefWeight = 50;
            string opinionLabelOfRecipientToSharer = recipient.relationshipContainer.GetOpinionLabel(sharer);
            string opinionLabelOfRecipientToActor = recipient.relationshipContainer.GetOpinionLabel(actor);

            if (sharer.traitContainer.HasTrait("Persuasive")) {
                beliefWeight += 500;
            }

            if ((crime is Rumor || crime is Assumption) && recipient.traitContainer.HasTrait("Suspicious")) {
                disbeliefWeight += 2000;
                weightLog += "\nRecipient is Suspicious: Disbelief + 2000";
            }
            if (opinionLabelOfRecipientToSharer == RelationshipManager.Friend) {
                beliefWeight += 100;
                weightLog += "\nSource is Friend: Belief + 100";
            } else if (opinionLabelOfRecipientToSharer == RelationshipManager.Close_Friend) {
                beliefWeight += 250;
                weightLog += "\nSource is Close Friend: Belief + 250";
            } else if (opinionLabelOfRecipientToSharer == RelationshipManager.Enemy) {
                disbeliefWeight += 100;
                weightLog += "\nSource is Enemy: Disbelief + 100";
            } else if (opinionLabelOfRecipientToSharer == RelationshipManager.Rival) {
                disbeliefWeight += 250;
                weightLog += "\nSource is Rival: Disbelief + 250";
            }

            REACTABLE_EFFECT reactableEffect = crime.GetReactableEffect(recipient);
            if (reactableEffect == REACTABLE_EFFECT.Positive) {
                if (opinionLabelOfRecipientToActor == RelationshipManager.Friend || opinionLabelOfRecipientToActor == RelationshipManager.Close_Friend) {
                    beliefWeight += 500;
                    weightLog += "\nActor is Friend/Close Friend: Belief + 500";
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Enemy) {
                    disbeliefWeight += 250;
                    weightLog += "\nSource is Enemy: Disbelief + 250";
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Rival) {
                    disbeliefWeight += 500;
                    weightLog += "\nSource is Rival: Disbelief + 500";
                }
            } else if (reactableEffect == REACTABLE_EFFECT.Negative) {
                if (opinionLabelOfRecipientToActor == RelationshipManager.Enemy || opinionLabelOfRecipientToActor == RelationshipManager.Rival) {
                    beliefWeight += 250;
                    weightLog += "\nActor is Enemy/Rival: Belief + 250";
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Friend) {
                    disbeliefWeight += 250;
                    weightLog += "\nSource is Friend: Disbelief + 250";
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Close_Friend) {
                    disbeliefWeight += 500;
                    weightLog += "\nSource is Close Friend: Disbelief + 500";
                }
            }
            weightLog += "\nTotal Belief Weight: " + beliefWeight;
            weightLog += "\nTotal Disbelief Weight: " + disbeliefWeight;

            weights.AddElement("Belief", beliefWeight);
            weights.AddElement("Disbelief", disbeliefWeight);

            string result = weights.PickRandomElementGivenWeights();
            weightLog += "\nResult: " + result;
            sharer.logComponent.PrintLogIfActive(weightLog);

            if (result == "Belief") {
                //Recipient believes
                recipient.reactionComponent.ReactTo(crime, REACTION_STATUS.INFORMED, false);
            } else {
                //Recipient does not believe
                CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, recipient, sharer, REACTION_STATUS.INFORMED, crime as ActualGoapNode);

                //Will only log on not believe because the log for believe report crime is already in the crime system
                Log believeLog = new Log(GameManager.Instance.Today(), "GoapAction", name, result);
                believeLog.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                believeLog.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                believeLog.AddToFillers(null, "crime", LOG_IDENTIFIER.STRING_1);
                believeLog.AddLogToInvolvedObjects();
            }
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            Character target = poiTarget as Character;
            return actor != target && !UtilityScripts.GameUtilities.IsRaceBeast(target.race); // target.role.roleType != CHARACTER_ROLE.BEAST
        }
        return false;
    }
    #endregion
}