﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;

public class NonActionEventsComponent : CharacterComponent {

    private const string Warm_Chat = "Warm Chat";
    private const string Awkward_Chat = "Awkward Chat";
    private const string Argument = "Argument";
    private const string Insult = "Insult";
    private const string Praise = "Praise";

    private readonly WeightedDictionary<string> chatWeights;

    /// <summary>
    /// When did this character last chat/flirt?
    /// </summary>
    public GameDate lastConversationDate { get; private set; }
    
    public NonActionEventsComponent() {
        chatWeights = new WeightedDictionary<string>();
        lastConversationDate = GameManager.Instance.Today();
    }
    public NonActionEventsComponent(SaveDataNonActionEventsComponent data) {
        chatWeights = new WeightedDictionary<string>();
        lastConversationDate = data.lastConversationDate;
    }

    #region Utilities
    public bool CanInteract(Character target) {
        Character disguisedActor = owner;
        Character disguisedTarget = target;
        if (owner.reactionComponent.disguisedCharacter != null) {
            disguisedActor = owner.reactionComponent.disguisedCharacter;
        }
        if (target.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = target.reactionComponent.disguisedCharacter;
        }
        if (target.isDead
            || !disguisedActor.canWitness
            || !disguisedTarget.canWitness
            || disguisedActor is Summon
            || disguisedTarget is Summon) {
            return false;
        }
        return true;
    }
    #endregion

    #region Chat
    //public bool NormalChatCharacter(Character target) {
    //    //if (!CanInteract(target)) {
    //    //    return false;
    //    //}
    //    if (UnityEngine.Random.Range(0, 100) < 50) {
    //        if (!owner.IsHostileWith(target)) {
    //            TriggerChatCharacter(target);
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public bool ForceChatCharacter(Character target, ref Log overrideLog) {
        //if (!CanInteract(target)) {
        //    return false;
        //}
        Character disguisedActor = owner;
        Character disguisedTarget = target;
        if (owner.reactionComponent.disguisedCharacter != null) {
            disguisedActor = owner.reactionComponent.disguisedCharacter;
        }
        if (target.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = target.reactionComponent.disguisedCharacter;
        }
        if (!disguisedActor.IsHostileWith(disguisedTarget)) {
            TriggerChatCharacter(target, ref overrideLog);
            return true;
        }
        return false;
    }
    private void TriggerChatCharacter(Character target, ref Log overrideLog) {
        string strLog = $"{owner.name} chat with {target.name}";

        Character disguisedActor = owner;
        Character disguisedTarget = target;
        if (owner.reactionComponent.disguisedCharacter != null) {
            disguisedActor = owner.reactionComponent.disguisedCharacter;
        }
        if (target.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = target.reactionComponent.disguisedCharacter;
        }

#if UNITY_EDITOR
        Assert.IsTrue(disguisedActor != disguisedTarget, $"{disguisedActor} is chatting with itself.");
#endif

        chatWeights.Clear();
        chatWeights.AddElement(Warm_Chat, 100);
        chatWeights.AddElement(Awkward_Chat, 30);
        chatWeights.AddElement(Argument, 20);
        chatWeights.AddElement(Insult, 20);
        chatWeights.AddElement(Praise, 20);

        strLog += $"\n\n{chatWeights.GetWeightsSummary("BASE WEIGHTS")}";

        MOOD_STATE actorMood = disguisedActor.moodComponent.moodState;
        MOOD_STATE targetMood = disguisedTarget.moodComponent.moodState;
        string actorOpinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
        string targetOpinionLabel = disguisedTarget.relationshipContainer.GetOpinionLabel(disguisedActor);
        int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(disguisedActor, disguisedTarget);

        if (actorMood == MOOD_STATE.Bad) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Argument, 15);
            chatWeights.AddWeightToElement(Insult, 20);
            strLog += "\n\nActor Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15, Insult: +20";
        } else if (actorMood == MOOD_STATE.Critical) {
            chatWeights.AddWeightToElement(Warm_Chat, -40);
            chatWeights.AddWeightToElement(Argument, 30);
            chatWeights.AddWeightToElement(Insult, 50);
            strLog += "\n\nActor Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30, Insult: +50";
        }

        if (targetMood == MOOD_STATE.Bad) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Argument, 15);
            strLog += "\n\nTarget Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15";
        } else if (targetMood == MOOD_STATE.Critical) {
            chatWeights.AddWeightToElement(Warm_Chat, -40);
            chatWeights.AddWeightToElement(Argument, 30);
            strLog += "\n\nTarget Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30";
        }

        if (actorOpinionLabel == RelationshipManager.Close_Friend || actorOpinionLabel == RelationshipManager.Friend) {
            chatWeights.AddWeightToElement(Awkward_Chat, -15);
            strLog += "\n\nActor's opinion of Target is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
        } else if (actorOpinionLabel == RelationshipManager.Enemy || actorOpinionLabel == RelationshipManager.Rival) {
            chatWeights.AddWeightToElement(Awkward_Chat, 15);
            strLog += "\n\nActor's opinion of Target is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
        }

        if (targetOpinionLabel == RelationshipManager.Close_Friend || targetOpinionLabel == RelationshipManager.Friend) {
            chatWeights.AddWeightToElement(Awkward_Chat, -15);
            strLog += "\n\nTarget's opinion of Actor is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
        } else if (targetOpinionLabel == RelationshipManager.Enemy || targetOpinionLabel == RelationshipManager.Rival) {
            chatWeights.AddWeightToElement(Awkward_Chat, 15);
            strLog += "\n\nTarget's opinion of Actor is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
        }

        if(compatibility != -1) {
            strLog += $"\n\nActor and Target Compatibility is {compatibility}, modified weights...";
            if (compatibility == 0) {
                chatWeights.AddWeightToElement(Awkward_Chat, 15);
                chatWeights.AddWeightToElement(Argument, 20);
                chatWeights.AddWeightToElement(Insult, 15);
                strLog += "\nAwkward Chat: +15, Argument: +20, Insult: +15";
            } else if (compatibility == 1) {
                chatWeights.AddWeightToElement(Awkward_Chat, 10);
                chatWeights.AddWeightToElement(Argument, 10);
                chatWeights.AddWeightToElement(Insult, 10);
                strLog += "\nAwkward Chat: +10, Argument: +10, Insult: +10";
            } else if (compatibility == 2) {
                chatWeights.AddWeightToElement(Awkward_Chat, 5);
                chatWeights.AddWeightToElement(Argument, 5);
                chatWeights.AddWeightToElement(Insult, 5);
                strLog += "\nAwkward Chat: +5, Argument: +5, Insult: +5";
            } else if (compatibility == 3) {
                chatWeights.AddWeightToElement(Praise, 5);
                strLog += "\nPraise: +5";
            } else if (compatibility == 4) {
                chatWeights.AddWeightToElement(Praise, 10);
                strLog += "\nPraise: +10";
            } else if (compatibility == 5) {
                chatWeights.AddWeightToElement(Praise, 20);
                strLog += "\nPraise: +20";
            }
        }

        if (disguisedActor.traitContainer.HasTrait("Hothead")) {
            chatWeights.AddWeightToElement(Argument, 15);
            strLog += "\n\nActor is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
        }
        if (disguisedTarget.traitContainer.HasTrait("Hothead")) {
            chatWeights.AddWeightToElement(Argument, 15);
            strLog += "\n\nTarget is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
        }

        if (disguisedActor.traitContainer.HasTrait("Diplomatic")) {
            chatWeights.AddWeightToElement(Insult, -30);
            chatWeights.AddWeightToElement(Praise, 30);
            strLog += "\n\nActor is Diplomatic, modified weights...";
            strLog += "\nInsult: -30, Praise: +30";
        }

        if (!disguisedActor.isSociable) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 20);
            chatWeights.AddWeightToElement(Insult, 50);
            chatWeights.AddWeightToElement(Praise, -20);
            strLog += "\n\nActor is unsociable";
        }
        if (!disguisedTarget.isSociable) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 20);
            strLog += "\n\nTarget is unsociable";
        }

        Trait angryActor = disguisedActor.traitContainer.GetNormalTrait<Trait>("Angry");
        Trait angryTarget = disguisedTarget.traitContainer.GetNormalTrait<Trait>("Angry");
        
        if (angryActor != null && angryActor.responsibleCharacters != null && angryActor.responsibleCharacters.Contains(disguisedTarget)) {
            //actor is angry with target
            chatWeights.AddWeightToElement(Warm_Chat, -50);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 50);
            chatWeights.AddWeightToElement(Insult, 100);
            chatWeights.AddWeightToElement(Praise, -50);
            strLog += "\n\nActor is angry with target";
        }
        if (angryTarget != null && angryTarget.responsibleCharacters != null && angryTarget.responsibleCharacters.Contains(disguisedActor)) {
            //target is angry with actor
            chatWeights.AddWeightToElement(Warm_Chat, -50);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 50);
            strLog += "\n\nTarget is angry with actor";
        }
        
        if (disguisedActor.traitContainer.HasTrait("Hero") || disguisedTarget.traitContainer.HasTrait("Hero")) {
            chatWeights.RemoveElement(Argument);
            strLog += "\n\nActor or target is Hero, removing argument weight...";
        }
        

        strLog += $"\n\n{chatWeights.GetWeightsSummary("FINAL WEIGHTS")}";

        string result = chatWeights.PickRandomElementGivenWeights();
        strLog += $"\nResult: {result}";

        //if (owner.traitContainer.HasTrait("Plagued") && !target.traitContainer.HasTrait("Plagued")) {
        //    strLog += "\n\nCharacter has Plague, 25% chance to infect the Target";
        //    int roll = UnityEngine.Random.Range(0, 100);
        //    strLog += $"\nRoll: {roll}";
        //    if (roll < 35) {
        //        target.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, target);
        //        // target.traitContainer.AddTrait(target, "Plagued", owner);
        //    }
        //} else if (!owner.traitContainer.HasTrait("Plagued") && target.traitContainer.HasTrait("Plagued")) {
        //    strLog += "\n\nTarget has Plague, 25% chance to infect the Character";
        //    int roll = UnityEngine.Random.Range(0, 100);
        //    strLog += $"\nRoll: {roll}";
        //    if (roll < 35) {
        //        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Plagued, owner);
        //        // owner.traitContainer.AddTrait(owner, "Plagued", target);
        //    }
        //}
        
        owner.logComponent.PrintLogIfActive(strLog);

        bool adjustOpinionBothSides = false;
        int opinionValue = 0;

        if(result == Warm_Chat) {
            opinionValue = 6;
            adjustOpinionBothSides = true;
        } else if (result == Awkward_Chat) {
            opinionValue = -3;
            adjustOpinionBothSides = true;
        } else if (result == Argument) {
            opinionValue = -5;
            adjustOpinionBothSides = true;
        } else if (result == Insult) {
            opinionValue = -6;
        } else if (result == Praise) {
            opinionValue = 6;
        }

        if (adjustOpinionBothSides) {
            owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Conversations", opinionValue, "engaged in disastrous conversation");
            target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", opinionValue, "engaged in disastrous conversation");
        } else {
            //If adjustment of opinion is not on both sides, this must mean that the result is either Insult or Praise, so adjust opinion of target to actor
            target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", opinionValue);
        }

        GameDate dueDate = GameManager.Instance.Today();
        overrideLog = GameManager.CreateNewLog(dueDate, "Interrupt", "Chat", result, providedTags: LOG_TAG.Social);
        overrideLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        overrideLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //owner.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        owner.SetIsConversing(true);
        target.SetIsConversing(true);

        Traits.Plagued ownerPlague = null;
        Traits.Plagued targetPlague = null;
        if (owner.traitContainer.HasTrait("Plagued")) {
            ownerPlague = owner.traitContainer.GetNormalTrait<Traits.Plagued>("Plagued");
        }
        if (target.traitContainer.HasTrait("Plagued")) {
            targetPlague = target.traitContainer.GetNormalTrait<Traits.Plagued>("Plagued");
        }
        if (ownerPlague != null && targetPlague == null) {
            ownerPlague.ChatInfection(target);
        }
        if (targetPlague != null && ownerPlague == null) {
            targetPlague.ChatInfection(owner);
        }
        dueDate.AddTicks(2);
        SchedulingManager.Instance.AddEntry(dueDate, () => owner.SetIsConversing(false), owner);
        SchedulingManager.Instance.AddEntry(dueDate, () => target.SetIsConversing(false), target);

    }
    public void SetLastConversationDate(GameDate date) {
        lastConversationDate = date;
    }
    #endregion

    #region Break Up
    public void NormalBreakUp(Character target, string reason) {
        RELATIONSHIP_TYPE relationship = owner.relationshipContainer.GetRelationshipFromParametersWith(target, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR);
        TriggerBreakUp(target, relationship, reason);
    }
    private void TriggerBreakUp(Character target, RELATIONSHIP_TYPE relationship, string reason) {
        RelationshipManager.Instance.RemoveRelationshipBetween(owner, target, relationship);
        //upon break up, if one of them still has a Positive opinion of the other, he will gain Heartbroken trait
        if (!owner.traitContainer.HasTrait("Psychopath")) { //owner.RelationshipManager.GetTotalOpinion(target) >= 0
            owner.traitContainer.AddTrait(owner, "Heartbroken", target);
        }
        if (!target.traitContainer.HasTrait("Psychopath")) { //target.RelationshipManager.GetTotalOpinion(owner) >= 0
            target.traitContainer.AddTrait(target, "Heartbroken", owner);
        }
        RelationshipManager.Instance.CreateNewRelationshipBetween(owner, target, RELATIONSHIP_TYPE.EX_LOVER);

        Log log;
        if (reason != string.Empty) {
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Break Up", "break_up_reason", null, LOG_TAG.Social, LOG_TAG.Life_Changes);
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
        } else {
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Break Up", "break_up", null, LOG_TAG.Social, LOG_TAG.Life_Changes);
        }
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.logComponent.RegisterLog(log, onlyClickedCharacter: false);

        if (relationship == RELATIONSHIP_TYPE.LOVER) {
            //**Effect 1**: Actor - Remove Lover relationship with Character 2
            //if the relationship that was removed is lover, change home to a random unoccupied dwelling,
            //otherwise, no home. Reference: https://trello.com/c/JUSt9bEa/1938-broken-up-characters-should-live-in-separate-house
            owner.MigrateHomeStructureTo(null, affectSettlement: false);
            //owner.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, null);
        }
    }
    #endregion

    #region Flirt
    public bool NormalFlirtCharacter(Character target, ref Log overrideLog) {
        //if (!CanInteract(target)) {
        //    return false;
        //}
        Character disguisedActor = owner;
        Character disguisedTarget = target;
        if (owner.reactionComponent.disguisedCharacter != null) {
            disguisedActor = owner.reactionComponent.disguisedCharacter;
        }
        if (target.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = target.reactionComponent.disguisedCharacter;
        }
        if (!disguisedActor.IsHostileWith(disguisedTarget)) {
            string result = TriggerFlirtCharacter(target);
            GameDate dueDate = GameManager.Instance.Today();
            overrideLog = GameManager.CreateNewLog(dueDate, "Interrupt", "Flirt", result, providedTags: LOG_TAG.Social);
            overrideLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            //owner.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            owner.SetIsConversing(true);
            target.SetIsConversing(true);

            dueDate.AddTicks(2);
            SchedulingManager.Instance.AddEntry(dueDate, () => owner.SetIsConversing(false), owner);
            SchedulingManager.Instance.AddEntry(dueDate, () => target.SetIsConversing(false), target);
            return true;
        }
        return false;
    }
    private string TriggerFlirtCharacter(Character target) {
        Character disguisedActor = owner;
        Character disguisedTarget = target;

        bool actorIsDisguised = false;
        bool targetIsDisguised = false;
        
        if(owner.reactionComponent.disguisedCharacter != null) {
            actorIsDisguised = true;
            disguisedActor = owner.reactionComponent.disguisedCharacter;
        }
        if (target.reactionComponent.disguisedCharacter != null) {
            targetIsDisguised = true;
            disguisedTarget = target.reactionComponent.disguisedCharacter;
        }
        
        if (!disguisedTarget.isSociable) {
            owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
            target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
            return "unsociable";
        }
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 50) {
            if (disguisedActor.traitContainer.HasTrait("Unattractive")) {
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "ugly";
            }
        }
        if(chance < 70) {
            Trait angry = disguisedTarget.traitContainer.GetNormalTrait<Trait>("Angry");
            if (angry?.responsibleCharacters != null && angry.responsibleCharacters.Contains(disguisedActor)) {
                //target is angry at actor
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "angry";
            }
        }
        if(chance < 90) {
            if(!RelationshipManager.IsSexuallyCompatibleOneSided(disguisedTarget.sexuality, disguisedActor.sexuality, disguisedTarget.gender, disguisedActor.gender)) {
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "incompatible";
            }
        }
        owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Reciprocated courtship", 6);
        target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", 18);
        
        string relationshipName = disguisedActor.relationshipContainer.GetRelationshipNameWith(disguisedTarget);

        //do not develop relationships if either actor or target is disguised
        if (!actorIsDisguised && !targetIsDisguised && disguisedActor.isNormalCharacter && disguisedTarget.isNormalCharacter) {
            // If Opinion of Target towards Actor is already in Acquaintance range
            if (relationshipName == RelationshipManager.Acquaintance) {
                // 25% chance to develop Lover relationship if both characters have no Lover yet
                if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER)
                    && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.LOVER)) {
                    if (UnityEngine.Random.Range(0, 100) < 25) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER);
                    }
                }
                // 35% chance to develop Affair if at least one of the characters already have a Lover
                else if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR)
                         && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.AFFAIR)) {
                    if (UnityEngine.Random.Range(0, 100) < 35) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR);
                    }
                }
            }
            // If Opinion of Target towards Actor is already in Friend or Close Friend range
            else if (relationshipName == RelationshipManager.Friend || relationshipName == RelationshipManager.Close_Friend) {
                // 35 % chance to develop Lover relationship if both characters have no Lover yet
                if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER)
                    && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.LOVER)) {
                    if (UnityEngine.Random.Range(0, 100) < 35) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER);
                    }
                }
                // 50% chance to develop Affair if at least one of the characters already have a Lover 
                else if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR)
                         && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.AFFAIR)) { 
                    if (UnityEngine.Random.Range(0, 100) < 50) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR);
                    }
                }
            }
        }
        
        return "flirted_back";
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataNonActionEventsComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public class SaveDataNonActionEventsComponent : SaveData<NonActionEventsComponent> {
    public GameDate lastConversationDate;

    #region Overrides
    public override void Save(NonActionEventsComponent data) {
        lastConversationDate = data.lastConversationDate;
    }

    public override NonActionEventsComponent Load() {
        NonActionEventsComponent component = new NonActionEventsComponent(this);
        return component;
    }
    #endregion
}