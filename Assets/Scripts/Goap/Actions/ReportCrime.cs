using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Interrupts;
using UnityEngine.Assertions;
using Crime_System;
using Logs;
using Object_Pools;

public class ReportCrime : GoapAction {
    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;
    public ReportCrime() : base(INTERACTION_TYPE.REPORT_CRIME) {
        actionIconString = GoapActionStateDB.Report_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes, LOG_TAG.Major};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Report Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        OtherData[] otherData = node.otherData;
        if (otherData.Length == 2 && otherData[0].obj is ICrimeable crime) {
            //CrimeType crimeTypObj = CrimeManager.Instance.GetCrimeType(crime.crimeType);
            //log.AddToFillers(null, crimeTypObj.name, LOG_IDENTIFIER.STRING_1);
            Character criminal = crime.actor;
            if (crime.disguisedActor != null) {
                criminal = crime.disguisedActor;
            }
            Assert.IsNotNull(criminal, $"{GameManager.Instance.TodayLogString()} Report crime of {crime.name} by {actor.name} to {poiTarget.name} has null criminal.");
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
                goapActionInvalidity.reason = "target_carried";
            } else if (!targetCharacter.limiterComponent.canWitness) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.reason = "target_inactive";
            }
        }
        return goapActionInvalidity;
    }
    public override string ReactionToActor(Character actor, IPointOfInterest poiTarget, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(actor, poiTarget, witness, node, status);
        OtherData[] otherData = node.otherData;

        if (otherData[0].obj is ICrimeable reactable) {
            //SPECIAL CASE: After reacting to the Report Crime, witness should also react to the rumor itself
            if(status == REACTION_STATUS.INFORMED && reactable.name != "Report Crime") {
                ProcessInformation(node.actor, witness, reactable, node);
            }
        }
        return response;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        OtherData[] otherData = node.otherData;

        if (otherData[0].obj is ICrimeable reactable) {
            if (witness == reactable.actor) {
                reactions.Add(EMOTION.Anger);
                if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER) || witness.relationshipContainer.IsFriendsWith(actor)) {
                    reactions.Add(EMOTION.Betrayal);
                }
            } else if (witness.relationshipContainer.GetOpinionLabel(reactable.actor) == RelationshipManager.Close_Friend ||
                       witness.relationshipContainer.HasRelationshipWith(reactable.actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER)) {
                reactions.Add(EMOTION.Anger);
            } else {
                if (witness.relationshipContainer.IsEnemiesWith(reactable.actor)) {
                    reactions.Add(EMOTION.Approval);
                } else {
                    reactions.Add(EMOTION.Concern);
                }
            }
        }
    }
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
        OtherData[] otherData = goapNode.otherData;
        ICrimeable crime = otherData[0].obj as ICrimeable;
        CrimeData crimeData = otherData[1].obj as CrimeData;
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
            Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "dead_criminal", providedTags: LOG_TAG.Crimes);
            log.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.CHARACTER_3);
            log.AddLogToDatabase(true);
            return;
        }

        if (actor != recipient) {
#if DEBUG_LOG
            string weightLog = "Report crime of " + sharer.name + " to " + recipient.name + ": " + crime.name + " with actor " + actor.name + " and target " + target.name;
            weightLog += "\nBase Belief Weight: 50";
            weightLog += "\nBase Disbelief Weight: 50";
#endif

            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            int beliefWeight = 150;
            int disbeliefWeight = 50;
            string opinionLabelOfRecipientToSharer = recipient.relationshipContainer.GetOpinionLabel(sharer);
            string opinionLabelOfRecipientToActor = recipient.relationshipContainer.GetOpinionLabel(actor);

            if (sharer.traitContainer.HasTrait("Persuasive")) {
                beliefWeight += 500;
            }

            // if ((crime is Rumor || crime is Assumption) && recipient.traitContainer.HasTrait("Suspicious")) {
            //     disbeliefWeight += 2000;
            //     weightLog += "\nRecipient is Suspicious: Disbelief + 2000";
            // }
            if (opinionLabelOfRecipientToSharer == RelationshipManager.Friend) {
                beliefWeight += 100;
#if DEBUG_LOG
                weightLog += "\nSource is Friend: Belief + 100";
#endif
            } else if (opinionLabelOfRecipientToSharer == RelationshipManager.Close_Friend) {
                beliefWeight += 250;
#if DEBUG_LOG
                weightLog += "\nSource is Close Friend: Belief + 250";
#endif
            } else if (opinionLabelOfRecipientToSharer == RelationshipManager.Enemy) {
                disbeliefWeight += 100;
#if DEBUG_LOG
                weightLog += "\nSource is Enemy: Disbelief + 100";
#endif
            } else if (opinionLabelOfRecipientToSharer == RelationshipManager.Rival) {
                disbeliefWeight += 250;
#if DEBUG_LOG
                weightLog += "\nSource is Rival: Disbelief + 250";
#endif
            }

            REACTABLE_EFFECT reactableEffect = crime.GetReactableEffect(recipient);
            if (reactableEffect == REACTABLE_EFFECT.Positive) {
                if (opinionLabelOfRecipientToActor == RelationshipManager.Friend || opinionLabelOfRecipientToActor == RelationshipManager.Close_Friend) {
                    beliefWeight += 500;
#if DEBUG_LOG
                    weightLog += "\nActor is Friend/Close Friend: Belief + 500";
#endif
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Enemy) {
                    disbeliefWeight += 250;
#if DEBUG_LOG
                    weightLog += "\nSource is Enemy: Disbelief + 250";
#endif
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Rival) {
                    disbeliefWeight += 500;
#if DEBUG_LOG
                    weightLog += "\nSource is Rival: Disbelief + 500";
#endif
                }
            } else if (reactableEffect == REACTABLE_EFFECT.Negative) {
                if (opinionLabelOfRecipientToActor == RelationshipManager.Enemy || opinionLabelOfRecipientToActor == RelationshipManager.Rival) {
                    beliefWeight += 250;
#if DEBUG_LOG
                    weightLog += "\nActor is Enemy/Rival: Belief + 250";
#endif
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Friend) {
                    disbeliefWeight += 250;
#if DEBUG_LOG
                    weightLog += "\nSource is Friend: Disbelief + 250";
#endif
                } else if (opinionLabelOfRecipientToActor == RelationshipManager.Close_Friend) {
                    disbeliefWeight += 500;
#if DEBUG_LOG
                    weightLog += "\nSource is Close Friend: Disbelief + 500";
#endif
                }
            }
#if DEBUG_LOG
            weightLog += "\nTotal Belief Weight: " + beliefWeight;
            weightLog += "\nTotal Disbelief Weight: " + disbeliefWeight;
#endif

            weights.AddElement("Belief", beliefWeight);
            weights.AddElement("Disbelief", disbeliefWeight);

            string result = weights.PickRandomElementGivenWeights();
#if DEBUG_LOG
            weightLog += "\nResult: " + result;
            sharer.logComponent.PrintLogIfActive(weightLog);
#endif

            if (result == "Belief") {
                //Recipient believes
                CRIME_TYPE crimeType = crime.crimeType;
                CRIME_SEVERITY severity = CRIME_SEVERITY.None;
                if(crimeType != CRIME_TYPE.Unset && crimeType != CRIME_TYPE.None) {
                    severity = CrimeManager.Instance.GetCrimeSeverity(recipient, crime.actor, crime.target, crimeType);
                }
                if(severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                    recipient.reactionComponent.ReactTo(crime, REACTION_STATUS.INFORMED, false);
                } else {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, "not_crime", providedTags: LOG_TAG.Crimes);
                    log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddLogToDatabase(true);
                }
            } else {
                //Recipient does not believe
                CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, recipient, sharer, REACTION_STATUS.INFORMED, crime as ActualGoapNode);

                //Will only log on not believe because the log for believe report crime is already in the crime system
                Log believeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, result, shareActionItself, logTags);
                believeLog.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                believeLog.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                believeLog.AddToFillers(null, "crime", LOG_IDENTIFIER.STRING_1);
                believeLog.AddLogToDatabase();
                PlayerManager.Instance.player.ShowNotificationFrom(actor, believeLog);
                LogPool.Release(believeLog);
            }
        }
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            Character target = poiTarget as Character;
            return actor != target && !UtilityScripts.GameUtilities.IsRaceBeast(target.race); // target.role.roleType != CHARACTER_ROLE.BEAST
        }
        return false;
    }
#endregion
}