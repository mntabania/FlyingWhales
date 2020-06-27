using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ChatCharacter : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public ChatCharacter() : base(INTERACTION_TYPE.CHAT_CHARACTER) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Social_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Chat Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 1;
    }
    #endregion

    #region State Effects
    public void PreChatSuccess(ActualGoapNode goapNode) {
        Character targetCharacter = goapNode.poiTarget as Character;

        // CHARACTER_MOOD thisCharacterMood = goapNode.actor.currentMoodType;
        // CHARACTER_MOOD targetCharacterMood = targetCharacter.currentMoodType;

        WeightedFloatDictionary<string> weights = new WeightedFloatDictionary<string>();
        weights.AddElement("Quick Chat", 250);

        IRelationshipData relData = goapNode.actor.relationshipContainer.GetRelationshipDataWith(targetCharacter);
        RELATIONSHIP_EFFECT relationshipEffectWithTarget = goapNode.actor.relationshipContainer.GetRelationshipEffectWith(targetCharacter);
        //**if no relationship yet, may become friends**
        if (relData == null) {
            if (!goapNode.actor.traitContainer.HasTrait("Psychopath") && !targetCharacter.traitContainer.HasTrait("Psychopath")) {
                int weight = 0;
                // if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                //     weight += -30;
                // } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                //     weight += -10;
                // } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                //     weight += 10;
                // } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                //     weight += 30;
                // }
                // if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                //     weight += -30;
                // } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                //     weight += -10;
                // } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                //     weight += 10;
                // } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                //     weight += 30;
                // }
                if (weight > 0) {
                    weights.AddElement("Become Friends", weight);
                }
            }
        } else {
            //**if no relationship other than relative, may become enemies**
            List<RELATIONSHIP_TYPE> relTraits = goapNode.actor.relationshipContainer.GetRelationshipDataWith(targetCharacter)?.relationships ?? null;
            if (relTraits != null && relTraits.Count == 1 && relTraits[0] == RELATIONSHIP_TYPE.RELATIVE) {
                int weight = 0;
                // if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                //     weight += 30;
                // } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                //     weight += 10;
                // } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                //     weight += -10;
                // } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                //     weight += -30;
                // }
                // if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                //     weight += 30;
                // } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                //     weight += 10;
                // } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                //     weight += -10;
                // } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                //     weight += -30;
                // }
                if (goapNode.actor.traitContainer.HasTrait("Hothead")) {
                    weight += 200;
                }
                if (targetCharacter.traitContainer.HasTrait("Hothead")) {
                    weight += 200;
                }
                if (weight > 0) {
                    weights.AddElement("Become Enemies", weight);
                }
            }

            //**if already has a positive relationship, knowledge may be transferred**
            if (relationshipEffectWithTarget == RELATIONSHIP_EFFECT.POSITIVE) {
                weights.AddElement("Share Information", 200);
            }

            //**if already has a negative relationship, Argument may occur**
            if (relationshipEffectWithTarget == RELATIONSHIP_EFFECT.NEGATIVE) {
                weights.AddElement("Argument", 500);

                //**if already has a negative relationship, relationship may be Resolve Enmityd**
                int weight = 0;
                // if (thisCharacterMood == CHARACTER_MOOD.DARK) {
                //     weight += -50;
                // } else if (thisCharacterMood == CHARACTER_MOOD.BAD) {
                //     weight += -20;
                // } else if (thisCharacterMood == CHARACTER_MOOD.GOOD) {
                //     weight += 20;
                // } else if (thisCharacterMood == CHARACTER_MOOD.GREAT) {
                //     weight += 50;
                // }
                // if (targetCharacterMood == CHARACTER_MOOD.DARK) {
                //     weight += -50;
                // } else if (targetCharacterMood == CHARACTER_MOOD.BAD) {
                //     weight += -20;
                // } else if (targetCharacterMood == CHARACTER_MOOD.GOOD) {
                //     weight += 20;
                // } else if (targetCharacterMood == CHARACTER_MOOD.GREAT) {
                //     weight += 50;
                // }
                if (goapNode.actor.traitContainer.HasTrait("Hothead")) {
                    weight -= 40;
                }
                if (targetCharacter.traitContainer.HasTrait("Hothead")) {
                    weight -= 40;
                }
                if (weight > 0) {
                    weights.AddElement("Resolve Enmity", weight);
                }
            }
        }

        //Flirtation
        // float flirtationWeight = goapNode.actor.GetFlirtationWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        // if (flirtationWeight > 0f) {
        //     weights.AddElement("Flirt", flirtationWeight);
        // }

        //Become Lovers weight
        // float becomeLoversWeight = goapNode.actor.GetBecomeLoversWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        // if (becomeLoversWeight > 0f) {
        //     weights.AddElement("Become Lovers", becomeLoversWeight);
        // }

        //Become Affairs
        // float becomeAffairsWeight = goapNode.actor.GetBecomeAffairsWeightWith(targetCharacter, relData, thisCharacterMood, targetCharacterMood);
        // if (becomeAffairsWeight > 0f) {
        //     weights.AddElement("Become Affairs", becomeAffairsWeight);
        // }

        if (goapNode.actor.traitContainer.HasTrait("Angry") || targetCharacter.traitContainer.HasTrait("Angry")) {
            weights.RemoveElement("Quick Chat");
            weights.RemoveElement("Become Friends");
            weights.RemoveElement("Share Information");
            weights.RemoveElement("Resolve Enmity");
            weights.RemoveElement("Flirt");
            weights.RemoveElement("Become Lovers");
            weights.RemoveElement("Become Affairs");
            weights.AddWeightToElement("Become Enemies", 100);
            weights.AddWeightToElement("Argument", 100);
        }

        string chatResult = weights.PickRandomElementGivenWeights();

        weights.LogDictionaryValues($"Chat Weights of {goapNode.actor.name} and {targetCharacter.name}");

        CreateChatLog(goapNode, chatResult);

        if (chatResult == "Quick Chat") {
            QuickChat(goapNode);
        } else if (chatResult == "Share Information") {
            ShareInformation(goapNode);
        } else if (chatResult == "Flirt") {
            Flirt(goapNode, targetCharacter);
        } else if (chatResult == "Become Lovers") {
            BecomeLovers(goapNode, targetCharacter);
        } else if (chatResult == "Become Affairs") {
            BecomeAffairs(goapNode, targetCharacter);
        } else if (chatResult == "Argument") {
            Argument(goapNode, targetCharacter);
        }
    }
    private void QuickChat(ActualGoapNode goapNode) {
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        //adjust opinion +2 both ways
        Character target = goapNode.poiTarget as Character;
        goapNode.actor.relationshipContainer.AdjustOpinion(goapNode.actor, target, "Good Chat", 2);
        goapNode.actor.relationshipContainer.AdjustOpinion(goapNode.actor, target, "Good Chat", 2);
        
    }
    private void ShareInformation(ActualGoapNode goapNode) { }
    private void Argument(ActualGoapNode goapNode, Character targetCharacter) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        if (goapNode.actor.traitContainer.HasTrait("Angry")) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "angry_chat");
            log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            goapNode.actor.logComponent.RegisterLog(log, onlyClickedCharacter: false);
        }
        if (targetCharacter.traitContainer.HasTrait("Angry")) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "angry_chat");
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            targetCharacter.logComponent.RegisterLog(log, onlyClickedCharacter: false);
        }

        //if (goapNode.actor.traitContainer.GetNormalTrait<Trait>("Hothead") != null) {
        //    goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Angry");
        //}
        //if (targetCharacter.traitContainer.GetNormalTrait<Trait>("Hothead") != null) {
        //    targetCharacter.traitContainer.AddTrait(targetCharacter, "Angry");
        //}
        
        //adjust opinion -2 both ways
        Character target = goapNode.poiTarget as Character;
        goapNode.actor.relationshipContainer.AdjustOpinion(goapNode.actor, target, "Argument", -2);
        goapNode.actor.relationshipContainer.AdjustOpinion(goapNode.actor, target, "Argument", -2);
    }
    private void Flirt(ActualGoapNode goapNode, Character targetCharacter) {
        // if (!goapNode.actor.relationshipContainer.HasRelationshipWith(targetCharacter)) {
        //     goapNode.actor.relationshipContainer.CreateNewRelationship(targetCharacter);
        // }
        // (goapNode.actor.relationshipContainer.GetRelationshipDataWith(targetCharacter) as BaseRelationshipData).IncreaseFlirtationCount();
    }
    private void BecomeLovers(ActualGoapNode goapNode, Character targetCharacter) {
        RelationshipManager.Instance.CreateNewRelationshipBetween(goapNode.actor, targetCharacter, RELATIONSHIP_TYPE.LOVER);
    }
    private void BecomeAffairs(ActualGoapNode goapNode, Character targetCharacter) {
        RelationshipManager.Instance.CreateNewRelationshipBetween(goapNode.actor, targetCharacter, RELATIONSHIP_TYPE.AFFAIR);
    }
    #endregion

    private void CreateChatLog(ActualGoapNode goapNode, string logKey) {
        Log log = new Log(GameManager.Instance.Today(), "GoapAction", goapName, logKey, goapNode);
        if (goapNode != null) {
            log.SetLogType(LOG_TYPE.Action);
        }
        log.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        goapNode.OverrideDescriptionLog(log);
    }

    //    #region Intel Reactions
    //    private List<string> QuickChatIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;


    // 	    if (recipient == actor || recipient == target) {
    //            //- **Recipient Response Text**: I know what I did.
    //            reactions.Add("I know what I did.");
    //            //-**Recipient Effect * *: no effect
    //        } 
    //        else if(recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR) || recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.AFFAIR)) {
    //            reactions.Add("Wait what?! What were they talking about?!");
    //        } 
    //        else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //            reactions.Add("Huh? What were they talking about?");
    //        } 
    //        else {
    //            reactions.Add("Is this important?");
    //        }
    //        return reactions;
    //    }
    //    private List<string> ShareInformationIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;

    //        //Recipient and Actor is the same or Recipient and Target is the same:
    //        if (recipient == actor || recipient == target) {
    //            //- **Recipient Response Text**: I know what I did.
    //            reactions.Add("I know what I did.");
    //            //-**Recipient Effect * *: no effect
    //        } 
    //        else {
    //            reactions.Add("I wonder what they discussed.");
    //        }
    //        return reactions;
    //    }
    //    private List<string> BecomeFriendsIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;

    //        //Recipient and Actor is the same or Recipient and Target is the same:
    //        if (recipient == actor || recipient == target) {
    //            //- **Recipient Response Text**: I know what I did.
    //            reactions.Add("I know what I did.");
    //            //-**Recipient Effect * *: no effect
    //        } 
    //        else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY) && recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            reactions.Add("Are they conspiring against me?!");
    //        } 
    //        else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            reactions.Add(string.Format("So {0} is gathering allies, huh?", actor.name));
    //        } 
    //        else if (recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            reactions.Add(string.Format("So {0} is gathering allies, huh?", target.name));
    //        }
    //        else {
    //            reactions.Add("Is this important?");
    //        }
    //        return reactions;
    //    }
    //    private List<string> ArgumentIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;

    //        //Recipient and Actor is the same or Recipient and Target is the same:
    //        if (recipient == actor || recipient == target) {
    //            //- **Recipient Response Text**: I know what I did.
    //            reactions.Add("I know what I did.");
    //            //-**Recipient Effect * *: no effect
    //        }
    //        else {
    //            reactions.Add("I wonder what they argued about.");
    //        }
    //        return reactions;
    //    }
    //    private List<string> FlirtIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;
    //        Relatable actorLover = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //        Relatable actorParamour = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);
    //        Relatable targetLover = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //        Relatable targetParamour = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);

    //#if TRAILER_BUILD
    //        if (recipient.name == "Audrey" && actor.name == "Jamie" && target.name == "Fiona") {
    //            reactions.Add(string.Format("This is the last straw! I'm leaving that cur {0}, and this godforsaken town!", actor.name));
    //            recipient.CancelAllJobsAndPlans();
    //            recipient.marker.GoTo(recipient.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS).GetRandomUnoccupiedTile(), () => CreatePoisonTable(recipient));
    //            return reactions;
    //        }
    //#endif

    //        //Recipient and Actor is the same:
    //        if (recipient == actor) {
    //            if (recipient.currentAlterEgo == targetLover) {
    //                reactions.Add("Hey! That's private!");
    //            } else if (recipient.currentAlterEgo == targetParamour) {
    //                reactions.Add("Don't tell anyone. *wink**wink*");
    //            }
    //        } 
    //        else if (recipient == target) {
    //            if (recipient.currentAlterEgo == actorLover) {
    //                reactions.Add("Hey! That's private!");
    //            } else if (recipient.currentAlterEgo == actorParamour) {
    //                reactions.Add("Don't you dare judge me!");
    //            }
    //        } 
    //        else if (recipient.currentAlterEgo == actorLover) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", actor.name));
    //            } else {
    //                reactions.Add("It's just harmless flirtation.");
    //            }
    //        } 
    //        else if (recipient.currentAlterEgo == targetLover) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                reactions.Add(string.Format("I've had enough of {0}'s shenanigans!", target.name));
    //            } else {
    //                reactions.Add("It's just harmless flirtation.");
    //            }
    //        } 
    //        else if (recipient.currentAlterEgo == actorParamour || recipient.currentAlterEgo == targetParamour) {
    //            reactions.Add("I thought I was the only snake in town.");
    //            AddTraitTo(recipient, "Annoyed");
    //        } 
    //        else {
    //            reactions.Add("This isn't relevant to me..");
    //        }
    //        return reactions;
    //    }
    //    private List<string> BecomeLoversIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;
    //        Relatable actorParamour = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);
    //        Relatable targetParamour = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);
    //        if (recipient == actor) {
    //            if(recipient.currentAlterEgo == targetParamour) {
    //                reactions.Add(string.Format("Yes that's true! I am so happy {0} finally chose me. This is what I've been dreaming for and at last, it came true!", target.name));
    //            } else {
    //                reactions.Add("Yes that's true and I am very happy. I hope we can be happy together, forever.");
    //            }
    //        }
    //        else if (recipient == target) {
    //            if (recipient.currentAlterEgo == actorParamour) {
    //                reactions.Add(string.Format("Yes that's true! I am so happy {0} finally chose me. This is what I've been dreaming for and at last, it came true!", actor.name));
    //            } else {
    //                reactions.Add("Yes that's true and I am very happy. I hope we can be happy together, forever.");
    //            }
    //        } 
    //        else if (recipient.currentAlterEgo == actorParamour) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this)) {
    //                reactions.Add(string.Format("I'm done being the appetizer in {0}'s full course meal!", actor.name));
    //            } else {
    //                reactions.Add(string.Format("Why can't I let {0} go? Perhaps, I'm truly, madly, deeply in love with {0}.", Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //            }
    //            AddTraitTo(recipient, "Heartbroken");
    //        } 
    //        else if (recipient.currentAlterEgo == targetParamour) {
    //            if (RelationshipManager.Instance.RelationshipDegradation(target, recipient, this)) {
    //                reactions.Add(string.Format("I'm done being the appetizer in {0}'s full course meal!", target.name));
    //            } else {
    //                reactions.Add(string.Format("Why can't I let {0} go? Perhaps, I'm truly, madly, deeply in love with {0}.", Utilities.GetPronounString(target.gender, PRONOUN_TYPE.OBJECTIVE, false)));
    //            }
    //            AddTraitTo(recipient, "Heartbroken");
    //        } 
    //        else if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY) || recipient.relationshipContainer.HasRelationshipWith(target.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            reactions.Add("That won't last. Mark my words!");
    //            AddTraitTo(recipient, "Annoyed");
    //        }
    //        else {
    //            reactions.Add("I guess there are two less lonely people in the world.");
    //        }
    //        return reactions;
    //    }
    //    private List<string> BecomeAffairsIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;
    //        Relatable actorLover = actor.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);
    //        Relatable targetLover = target.relationshipContainer.GetFirstRelatableWithRelationship(RELATIONSHIP_TRAIT.LOVER);

    //        //Recipient and Actor is the same or Recipient and Target is the same:
    //        if (recipient == actor || recipient == target) {
    //            //- **Recipient Response Text**: Please do not tell anyone else about this. I beg you!
    //            reactions.Add("Please do not tell anyone else about this. I beg you!");
    //            //-**Recipient Effect * *: no effect
    //        }
    //        //Recipient considers Actor as a Lover:
    //        else if (recipient.relationshipContainer.HasRelationshipWith(actorAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //            //- **Recipient Response Text**: [Actor Name] is cheating on me!?
    //            reactions.Add(string.Format("{0} is cheating on me!?", actor.name));
    //            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between and Recipient and Target.
    //            //Add an Undermine Job to Recipient versus Target (choose at random). Add a Breakup Job to Recipient versus Actor.
    //            RelationshipManager.Instance.RelationshipDegradation(target, recipient, this);
    //            recipient.ForceCreateUndermineJob(target, "snake");
    //            //recipient.CreateBreakupJob(actor);
    //        }
    //        //Recipient considers Target as a Lover:
    //        else if (recipient.relationshipContainer.HasRelationshipWith(poiTargetAlterEgo, RELATIONSHIP_TRAIT.LOVER)) {
    //            //- **Recipient Response Text**: [Target Name] is cheating on me!?
    //            reactions.Add(string.Format("{0} is cheating on me!?", target.name));
    //            //- **Recipient Effect**: https://trello.com/c/mqor1Ddv/1884-relationship-degradation between and Recipient and Actor.
    //            //Add an Undermine Job to Recipient versus Actor (choose at random). Add a Breakup Job to Recipient versus Target.
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //            recipient.ForceCreateUndermineJob(actor, "snake");
    //            //recipient.CreateBreakupJob(target);
    //        }
    //        //Actor has a Lover. Actor's Lover is not the Target. Recipient does not have a positive relationship with Actor. Recipient has a relationship (positive or negative) with Actor's Lover.
    //        else if (recipient != actor && recipient != target && actorLover != null && target.currentAlterEgo != actorLover
    //            && recipient.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.relationshipContainer.HasRelationshipWith(actorLover)) {
    //            //- **Recipient Response Text**: I should let [Actor's Lover's Name] know about this.
    //            reactions.Add(string.Format("I should let {0} know about this.", actorLover.relatableName));
    //            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Actor's Lover using this event as the information.
    //            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

    //            recipient.CreateShareInformationJob((actorLover as AlterEgoData).owner, this);

    //            //RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //        }
    //        //Target has a Lover. Target's Lover is not the Actor. Recipient does not have a positive relationship with Target. Recipient has a positive relationship with Target's Lover.
    //        else if (recipient != actor && recipient != target && targetLover != null && actor.currentAlterEgo != targetLover
    //            && recipient.relationshipContainer.GetRelationshipEffectWith(poiTargetAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE && recipient.relationshipContainer.GetRelationshipEffectWith(targetLover) == RELATIONSHIP_EFFECT.POSITIVE) {
    //            //- **Recipient Response Text**: I should let [Target's Lover's Name] know about this.
    //            reactions.Add(string.Format("I should let {0} know about this.", targetLover.relatableName));
    //            //- **Recipient Effect**: Recipient will perform Share Information Job targeting Target's Lover using this event as the information.
    //            //Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.

    //            recipient.CreateShareInformationJob((targetLover as AlterEgoData).owner, this);

    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //        }
    //        //Recipient and Actor are from the same faction and Actor has a Lover. Actor's Lover is not the Target.
    //        else if (recipient.faction == actor.faction && actorLover != null && actorLover != target.currentAlterEgo) {
    //            //- **Recipient Response Text**: [Actor Name] is playing with fire.
    //            reactions.Add(string.Format("{0} is playing with fire.", actor.name));
    //            //- **Recipient Effect**: Recipient will have https://trello.com/c/mqor1Ddv/1884-relationship-degradation with Actor.
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient, this);
    //        }
    //        //Else Catcher:
    //        else {
    //            //- **Recipient Response Text**: I don't care what those two do with their personal lives.
    //            reactions.Add("I don't care what those two do with their personal lives.");
    //            //- **Recipient Effect**: no effect
    //        }
    //        return reactions;
    //    }
    //    private List<string> ResolveEnmityIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //        List<string> reactions = new List<string>();
    //        Character target = poiTarget as Character;
    //        //Character actorParamour = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);
    //        //Character targetParamour = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.AFFAIR);

    //        //Recipient and Actor is the same or Recipient and Target is the same:
    //        if (recipient == actor || recipient == target) {
    //            //- **Recipient Response Text**: I know what I did.
    //            reactions.Add("I'm thankful we cleared that out.");
    //            //-**Recipient Effect * *: no effect
    //        } 
    //        else if (recipient.relationshipContainer.GetRelationshipEffectWith(actorAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE || recipient.relationshipContainer.GetRelationshipEffectWith(poiTargetAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
    //            reactions.Add("I'm glad they cleared things out.");
    //        } 
    //        else if (recipient.relationshipContainer.GetRelationshipEffectWith(actorAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //            reactions.Add(string.Format("{0} has tricked another one.", actor.name));
    //        } 
    //        else if (recipient.relationshipContainer.GetRelationshipEffectWith(poiTargetAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //            reactions.Add(string.Format("{0} has tricked another one.", target.name));
    //        } 
    //        else {
    //            reactions.Add("Good for them.");
    //        }
    //        return reactions;
    //    }
    //    #endregion
}

public class ChatCharacterData : GoapActionData {
    public ChatCharacterData() : base(INTERACTION_TYPE.CHAT_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
}
