using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Interrupts;
using Logs;
using UnityEngine.Assertions;

public class ShareInformation : GoapAction {
    public override ACTION_CATEGORY actionCategory => ACTION_CATEGORY.VERBAL;
    public ShareInformation() : base(INTERACTION_TYPE.SHARE_INFORMATION) {
        actionIconString = GoapActionStateDB.Gossip_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        doesNotStopTargetCharacter = true;
        logTags = new[] {LOG_TAG.Informed, LOG_TAG.Social};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Share Success", goapNode);
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

        if (node.disguisedActor != null) {
            actor = node.disguisedActor;
        }
        if (node.disguisedTarget != null) {
            poiTarget = node.disguisedTarget;
        }

        OtherData[] otherData = node.otherData;
        if (otherData.Length == 1 && otherData[0].obj is IReactable) {
            IReactable reactable = otherData[0].obj as IReactable;
            string articleWord = string.Empty;
            string information = string.Empty;
            if(reactable is ActualGoapNode actionReactable) {
                articleWord = "some";
                if (actionReactable.action.goapType == INTERACTION_TYPE.SHARE_INFORMATION) {
                    //TODO: Localize this
                    if (actionReactable.otherData != null && actionReactable.otherData.Length == 1) {
                        IReactable reactableRoot = actionReactable.otherData[0].obj as IReactable;
                        string fillerWord = "sharing";
                        if (reactableRoot is Rumor) {
                            fillerWord = "spreading";
                        }
                        information = actionReactable.actor.name + " is " + fillerWord + " " + reactableRoot.classificationName.ToLower() + " about " + reactableRoot.actor.name + ".";
                    }
                }
            } else {
                articleWord = UtilityScripts.Utilities.GetArticleForWord(reactable.classificationName);
            }
            string actionDescription = articleWord + " " + reactable.classificationName.ToLower();
            if (information == string.Empty) {
                information = reactable.informationLog.logText;
            }
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.OTHER);
            log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.OTHER_2);
            log.AddToFillers(null, actionDescription, LOG_IDENTIFIER.ACTION_DESCRIPTION);
            log.AddToFillers(null, information, LOG_IDENTIFIER.STRING_1);
            //log.AddToFillers(reactable.informationLog.fillers);
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
        IReactable reactable = otherData[0].obj as IReactable;

        //SPECIAL CASE: After reacting to the Share Info Action itself, witness should also react to the rumor itself
        if(reactable.name != "Share Information") {
            ProcessInformation(node.actor, witness, reactable, node);
        }
        return response;
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);

        OtherData[] otherData = node.otherData;
        IReactable reactable = otherData[0].obj as IReactable;

        REACTABLE_EFFECT reactableEffect = reactable.GetReactableEffect(witness);
        if (reactableEffect == REACTABLE_EFFECT.Negative) {
            if (witness == reactable.actor) {
                if (reactable is ActualGoapNode) {
                    reactions.Add(EMOTION.Embarassment);
                } else {
                    reactions.Add(EMOTION.Anger);
                }
                if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(actor)) {
                    reactions.Add(EMOTION.Betrayal);
                }
            } else {
                if (witness.relationshipContainer.HasRelationshipWith(reactable.actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(reactable.actor)) {
                    reactions.Add(EMOTION.Anger);
                } else if (reactable.target is Character rumorTarget) {
                    if (witness.relationshipContainer.HasRelationshipWith(rumorTarget, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(rumorTarget)) {
                        reactions.Add(EMOTION.Anger);
                    }
                }
            }
        } else {
            if (witness == reactable.actor) {
                if (reactable is ActualGoapNode) {
                    reactions.Add(EMOTION.Approval);
                } else {
                    reactions.Add(EMOTION.Embarassment);
                }
            } else {
                if (witness.relationshipContainer.IsEnemiesWith(reactable.actor)) {
                    reactions.Add(EMOTION.Disapproval);
                } else if (reactable.target is Character rumorTarget) {
                    if (witness.relationshipContainer.IsEnemiesWith(rumorTarget)) {
                        reactions.Add(EMOTION.Disapproval);
                    }
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        IReactable reactable = node.otherData[0].obj as IReactable;
        Assert.IsNotNull(reactable, $"{witness.name} is trying to get reactable effect of {node}, but reactable is null!");
        if (reactable.GetReactableEffect(witness) == REACTABLE_EFFECT.Negative) {
            return REACTABLE_EFFECT.Negative;
        }
        return REACTABLE_EFFECT.Neutral;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Rumormongering;
    }
#endregion

#region State Effects
    public void AfterShareSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        IReactable reactable = otherData[0].obj as IReactable;
        Character sharer = goapNode.actor;
        Character recipient = goapNode.poiTarget as Character;
        ProcessInformation(sharer, recipient, reactable, goapNode);
    }
    private void ProcessInformation(Character sharer, Character recipient, IReactable reactable, ActualGoapNode shareActionItself) {
        Character actor = reactable.actor;
        IPointOfInterest target = reactable.target;
        if (reactable.disguisedActor != null) {
            actor = reactable.disguisedActor;
        }
        if (reactable.disguisedTarget != null) {
            target = reactable.disguisedTarget;
        }

        if (actor != recipient) {
#if DEBUG_LOG
            string weightLog = "Share Information of " + sharer.name + " to " + recipient.name + ": " + reactable.name + " with actor " + actor.name + " and target " + target.name;
            weightLog += "\nBase Belief Weight: 50";
            weightLog += "\nBase Disbelief Weight: 50";
#endif

            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            int beliefWeight = 50;
            int disbeliefWeight = 50;
            string opinionLabelOfRecipientToSharer = recipient.relationshipContainer.GetOpinionLabel(sharer);
            string opinionLabelOfRecipientToActor = recipient.relationshipContainer.GetOpinionLabel(actor);

            if (sharer.traitContainer.HasTrait("Persuasive")) {
                beliefWeight += 500;
            }

            if ((reactable is Rumor || reactable is Assumption) && recipient.traitContainer.HasTrait("Suspicious")) {
                disbeliefWeight += 2000;
#if DEBUG_LOG
                weightLog += "\nRecipient is Suspicious: Disbelief + 2000";
#endif
            } else if (reactable is ActualGoapNode || reactable is InterruptHolder) {
                beliefWeight += 100;
#if DEBUG_LOG
                weightLog += "\nIf information is a real Action or Interrupt: Believe Weight +100";
#endif
            }
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

            REACTABLE_EFFECT reactableEffect = reactable.GetReactableEffect(recipient);
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
                recipient.reactionComponent.ReactTo(reactable, REACTION_STATUS.INFORMED, false);
            } else {
                //Recipient does not believe
                CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, recipient, sharer, REACTION_STATUS.INFORMED, reactable as ActualGoapNode);
                if (UnityEngine.Random.Range(0, 100) < 35) {
                    recipient.jobComponent.CreateConfirmRumorJob(actor, shareActionItself);
                }
            }
            Log believeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", name, result, providedTags: LOG_TAG.Informed);
            believeLog.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            believeLog.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            believeLog.AddToFillers(null, reactable.classificationName.ToLower(), LOG_IDENTIFIER.STRING_1);
            believeLog.AddLogToDatabase(true);
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