using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Interrupts;
using UnityEngine.Assertions;

public class ShareInformation : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public ShareInformation() : base(INTERACTION_TYPE.SHARE_INFORMATION) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Share Success", goapNode);
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
        if (otherData.Length == 1 && otherData[0] is IReactable) {
            IReactable reactable = otherData[0] as IReactable;
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.OTHER);
            log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.OTHER_2);
            log.AddToFillers(null, reactable.typeName.ToLower(), LOG_IDENTIFIER.ACTION_DESCRIPTION);
            log.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(reactable.informationLog ), LOG_IDENTIFIER.APPEND);
            log.AddToFillers(reactable.informationLog.fillers);
        }
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
        string response = base.ReactionToActor(witness, node, status);
        object[] otherData = node.otherData;
        IReactable reactable = otherData[0] as IReactable;
        Character actor = node.actor;
        Character target = node.poiTarget as Character;

        REACTABLE_EFFECT reactableEffect = reactable.GetReactableEffect(witness);
        if (reactableEffect == REACTABLE_EFFECT.Negative) {
            //TODO: Rumormongering Crime

            if(witness == reactable.actor) {
                if(reactable is ActualGoapNode) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, witness, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                }
                if (witness.relationshipContainer.HasRelationshipWith(actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, witness, actor, status, node);
                }
            } else {
                if (witness.relationshipContainer.HasRelationshipWith(reactable.actor, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(reactable.actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                } else if (reactable.target is Character rumorTarget) {
                    if (witness.relationshipContainer.HasRelationshipWith(rumorTarget, RELATIONSHIP_TYPE.AFFAIR, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.RELATIVE)
                    || witness.relationshipContainer.IsFriendsWith(rumorTarget)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
                    }
                }
            }
        } else {
            if (witness == reactable.actor) {
                if (reactable is ActualGoapNode) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
                } else {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Embarassment, witness, actor, status, node);
                }
            } else {
                if (witness.relationshipContainer.IsEnemiesWith(reactable.actor)) {
                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                } else if (reactable.target is Character rumorTarget) {
                    if (witness.relationshipContainer.IsEnemiesWith(rumorTarget)) {
                        response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
                    }
                }
            }
        }
        //SPECIAL CASE: After reacting to the Share Info Action itself, witness should also react to the rumor itself
        ProcessInformation(node.actor, witness, reactable);
        return response;
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        IReactable reactable = node.otherData[0] as IReactable;
        Assert.IsNotNull(reactable, $"{witness.name} is trying to get reactable effect of {node}, but reactable is null!");
        if (reactable.GetReactableEffect(witness) == REACTABLE_EFFECT.Negative) {
            return REACTABLE_EFFECT.Negative;
        }
        return REACTABLE_EFFECT.Neutral;
    }
    #endregion

    #region State Effects
    public void AfterShareSuccess(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        IReactable reactable = otherData[0] as IReactable;
        Character sharer = goapNode.actor;
        Character recipient = goapNode.poiTarget as Character;
        ProcessInformation(sharer, recipient, reactable);
    }
    private void ProcessInformation(Character sharer, Character recipient, IReactable reactable) {
        if (reactable is ActualGoapNode || reactable is InterruptHolder) {
            if (reactable.actor != recipient) {
                recipient.reactionComponent.ReactTo(reactable, REACTION_STATUS.INFORMED, false);
            }
        } else if (reactable is Rumor) {
            if (reactable.actor != recipient) {
                string weightLog = "Share Information of " + sharer.name + " to " + recipient.name + ": " + reactable.name + " with actor " + reactable.actor.name + " and target " + reactable.target.name;
                weightLog += "\nBase Belief Weight: 50";
                weightLog += "\nBase Disbelief Weight: 50";

                WeightedDictionary<string> weights = new WeightedDictionary<string>();
                int beliefWeight = 50;
                int disbeliefWeight = 50;
                string opinionLabelOfRecipientToSharer = recipient.relationshipContainer.GetOpinionLabel(sharer);
                string opinionLabelOfRecipientToActor = recipient.relationshipContainer.GetOpinionLabel(reactable.actor);

                if (recipient.traitContainer.HasTrait("Suspicious")) {
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

                REACTABLE_EFFECT reactableEffect = reactable.GetReactableEffect(recipient);
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
                    recipient.reactionComponent.ReactTo(reactable, REACTION_STATUS.INFORMED, false);
                } else {
                    //Recipient does not believe
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, recipient, sharer, REACTION_STATUS.INFORMED, reactable as ActualGoapNode);
                    if (UnityEngine.Random.Range(0, 100) < 35) {
                        //TODO: Confirm Rumor
                    }
                }
                Log believeLog = new Log(GameManager.Instance.Today(), "GoapAction", name, result);
                believeLog.AddToFillers(sharer, sharer.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                believeLog.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                believeLog.AddToFillers(null, reactable.typeName.ToLower(), LOG_IDENTIFIER.STRING_1);
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