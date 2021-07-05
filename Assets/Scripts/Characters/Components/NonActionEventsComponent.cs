using System.Collections;
using System.Collections.Generic;
using Object_Pools;
using Plague.Transmission;
using UnityEngine;
using Traits;
using UnityEngine.Assertions;
using UtilityScripts;

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
    public bool CanChat(Character target) {
        Character disguisedActor = owner;
        Character disguisedTarget = target;
        if (owner.reactionComponent.disguisedCharacter != null) {
            disguisedActor = owner.reactionComponent.disguisedCharacter;
        }
        if (target.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = target.reactionComponent.disguisedCharacter;
        }
        if (target.isDead
            || !disguisedActor.limiterComponent.canWitness
            || !disguisedTarget.limiterComponent.canWitness
            || disguisedActor is Summon
            || disguisedTarget is Summon) {
            return false;
        }
        //This is to fix this issue: https://trello.com/c/QPXOCuTO/2842-dev-03345-executioner-having-a-chat-with-burning-criminal
        if (target.traitContainer.HasTrait("Burning", "Burning At Stake")) {
            return false;
        }
        if (owner.traitContainer.HasTrait("Burning", "Burning At Stake")) {
            return false;
        }
        return true;
    }
    public bool CanFlirt(Character p_character1, Character p_character2) {
        //This is to fix this issue: https://trello.com/c/QPXOCuTO/2842-dev-03345-executioner-having-a-chat-with-burning-criminal
        if (p_character2.traitContainer.HasTrait("Burning", "Burning At Stake")) {
            return false;
        }
        if (p_character1.traitContainer.HasTrait("Burning", "Burning At Stake")) {
            return false;
        }
        if (p_character1.relationshipContainer.HasRelationshipWith(p_character2, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)) {
            //character 1 and 2 are lovers/affairs
            return true;
        }
        Unfaithful character1Unfaithful = p_character1.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful"); 
        if (character1Unfaithful != null) {
            return character1Unfaithful.CanBeLoverOrAffairBasedOnPersonalConstraints(p_character1, p_character2);
        } else {
            if (p_character1.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER) == -1) {
                //character 1 has no lover, check if character 2 is a family member, if not, allow flirt
                return !p_character1.relationshipContainer.IsFamilyMember(p_character2);
            }
            return false;
        }
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
#if DEBUG_LOG
        string strLog = $"{owner.name} chat with {target.name}";
#endif

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
#if DEBUG_LOG
        strLog += $"\n\n{chatWeights.GetWeightsSummary("BASE WEIGHTS")}";
#endif

        MOOD_STATE actorMood = disguisedActor.moodComponent.moodState;
        MOOD_STATE targetMood = disguisedTarget.moodComponent.moodState;
        string actorOpinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
        string targetOpinionLabel = disguisedTarget.relationshipContainer.GetOpinionLabel(disguisedActor);
        int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(disguisedActor, disguisedTarget);

        if (actorMood == MOOD_STATE.Bad) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Argument, 15);
            chatWeights.AddWeightToElement(Insult, 20);
#if DEBUG_LOG
            strLog += "\n\nActor Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15, Insult: +20";
#endif
        } else if (actorMood == MOOD_STATE.Critical) {
            chatWeights.AddWeightToElement(Warm_Chat, -40);
            chatWeights.AddWeightToElement(Argument, 30);
            chatWeights.AddWeightToElement(Insult, 50);
#if DEBUG_LOG
            strLog += "\n\nActor Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30, Insult: +50";
#endif
        }

        if (targetMood == MOOD_STATE.Bad) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Argument, 15);
#if DEBUG_LOG
            strLog += "\n\nTarget Mood is Low, modified weights...";
            strLog += "\nWarm Chat: -20, Argument: +15";
#endif
        } else if (targetMood == MOOD_STATE.Critical) {
            chatWeights.AddWeightToElement(Warm_Chat, -40);
            chatWeights.AddWeightToElement(Argument, 30);
#if DEBUG_LOG
            strLog += "\n\nTarget Mood is Critical, modified weights...";
            strLog += "\nWarm Chat: -40, Argument: +30";
#endif
        }

        if (actorOpinionLabel == RelationshipManager.Close_Friend || actorOpinionLabel == RelationshipManager.Friend) {
            chatWeights.AddWeightToElement(Awkward_Chat, -15);
#if DEBUG_LOG
            strLog += "\n\nActor's opinion of Target is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
#endif
        } else if (actorOpinionLabel == RelationshipManager.Enemy || actorOpinionLabel == RelationshipManager.Rival) {
            chatWeights.AddWeightToElement(Awkward_Chat, 15);
#if DEBUG_LOG
            strLog += "\n\nActor's opinion of Target is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
#endif
        }

        if (targetOpinionLabel == RelationshipManager.Close_Friend || targetOpinionLabel == RelationshipManager.Friend) {
            chatWeights.AddWeightToElement(Awkward_Chat, -15);
#if DEBUG_LOG
            strLog += "\n\nTarget's opinion of Actor is Close Friend or Friend, modified weights...";
            strLog += "\nAwkward Chat: -15";
#endif
        } else if (targetOpinionLabel == RelationshipManager.Enemy || targetOpinionLabel == RelationshipManager.Rival) {
            chatWeights.AddWeightToElement(Awkward_Chat, 15);
#if DEBUG_LOG
            strLog += "\n\nTarget's opinion of Actor is Enemy or Rival, modified weights...";
            strLog += "\nAwkward Chat: +15";
#endif
        }

        if (compatibility != -1) {
#if DEBUG_LOG
            strLog += $"\n\nActor and Target Compatibility is {compatibility}, modified weights...";
#endif
            if (compatibility == 0) {
                chatWeights.AddWeightToElement(Awkward_Chat, 15);
                chatWeights.AddWeightToElement(Argument, 20);
                chatWeights.AddWeightToElement(Insult, 15);
#if DEBUG_LOG
                strLog += "\nAwkward Chat: +15, Argument: +20, Insult: +15";
#endif
            } else if (compatibility == 1) {
                chatWeights.AddWeightToElement(Awkward_Chat, 10);
                chatWeights.AddWeightToElement(Argument, 10);
                chatWeights.AddWeightToElement(Insult, 10);
#if DEBUG_LOG
                strLog += "\nAwkward Chat: +10, Argument: +10, Insult: +10";
#endif
            } else if (compatibility == 2) {
                chatWeights.AddWeightToElement(Awkward_Chat, 5);
                chatWeights.AddWeightToElement(Argument, 5);
                chatWeights.AddWeightToElement(Insult, 5);
#if DEBUG_LOG
                strLog += "\nAwkward Chat: +5, Argument: +5, Insult: +5";
#endif
            } else if (compatibility == 3) {
                chatWeights.AddWeightToElement(Praise, 5);
#if DEBUG_LOG
                strLog += "\nPraise: +5";
#endif
            } else if (compatibility == 4) {
                chatWeights.AddWeightToElement(Praise, 10);
#if DEBUG_LOG
                strLog += "\nPraise: +10";
#endif
            } else if (compatibility == 5) {
                chatWeights.AddWeightToElement(Praise, 20);
#if DEBUG_LOG
                strLog += "\nPraise: +20";
#endif
            }
        }

        if (disguisedActor.traitContainer.HasTrait("Hothead")) {
            chatWeights.AddWeightToElement(Argument, 15);
#if DEBUG_LOG
            strLog += "\n\nActor is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
#endif
        }
        if (disguisedTarget.traitContainer.HasTrait("Hothead")) {
            chatWeights.AddWeightToElement(Argument, 15);
#if DEBUG_LOG
            strLog += "\n\nTarget is Hotheaded, modified weights...";
            strLog += "\nArgument: +15";
#endif
        }

        if (disguisedActor.traitContainer.HasTrait("Diplomatic")) {
            chatWeights.AddWeightToElement(Insult, -30);
            chatWeights.AddWeightToElement(Praise, 30);
#if DEBUG_LOG
            strLog += "\n\nActor is Diplomatic, modified weights...";
            strLog += "\nInsult: -30, Praise: +30";
#endif
        }

        if (!disguisedActor.limiterComponent.isSociable) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 20);
            chatWeights.AddWeightToElement(Insult, 50);
            chatWeights.AddWeightToElement(Praise, -20);
#if DEBUG_LOG
            strLog += "\n\nActor is unsociable";
#endif
        }
        if (!disguisedTarget.limiterComponent.isSociable) {
            chatWeights.AddWeightToElement(Warm_Chat, -20);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 20);
#if DEBUG_LOG
            strLog += "\n\nTarget is unsociable";
#endif
        }

        Trait angryActor = disguisedActor.traitContainer.GetTraitOrStatus<Trait>("Angry");
        Trait angryTarget = disguisedTarget.traitContainer.GetTraitOrStatus<Trait>("Angry");
        
        if (angryActor != null && angryActor.responsibleCharacters != null && angryActor.responsibleCharacters.Contains(disguisedTarget)) {
            //actor is angry with target
            chatWeights.AddWeightToElement(Warm_Chat, -50);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 50);
            chatWeights.AddWeightToElement(Insult, 100);
            chatWeights.AddWeightToElement(Praise, -50);
#if DEBUG_LOG
            strLog += "\n\nActor is angry with target";
#endif
        }
        if (angryTarget != null && angryTarget.responsibleCharacters != null && angryTarget.responsibleCharacters.Contains(disguisedActor)) {
            //target is angry with actor
            chatWeights.AddWeightToElement(Warm_Chat, -50);
            chatWeights.AddWeightToElement(Awkward_Chat, 20);
            chatWeights.AddWeightToElement(Argument, 50);
#if DEBUG_LOG
            strLog += "\n\nTarget is angry with actor";
#endif
        }

        //Vampire
        if (disguisedActor.traitContainer.HasTrait("Hemophobic")) {
            bool isKnownVampire = false;
            Vampire vampire = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            isKnownVampire = vampire != null && vampire.DoesCharacterKnowThisVampire(disguisedActor);
            if (isKnownVampire) {
                chatWeights.AddWeightToElement(Warm_Chat, -50);
                chatWeights.AddWeightToElement(Insult, 50);
            }
        } else if (disguisedActor.traitContainer.HasTrait("Hemophiliac")) {
            bool isKnownVampire = false;
            Vampire vampire = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            isKnownVampire = vampire != null && vampire.DoesCharacterKnowThisVampire(disguisedActor);
            if (isKnownVampire) {
                chatWeights.AddWeightToElement(Warm_Chat, 50);
                chatWeights.AddWeightToElement(Praise, 50);
            }
        }
        if (disguisedTarget.traitContainer.HasTrait("Hemophobic")) {
            bool isKnownVampire = false;
            Vampire vampire = disguisedActor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            isKnownVampire = vampire != null && vampire.DoesCharacterKnowThisVampire(disguisedTarget);
            if (isKnownVampire) {
                chatWeights.AddWeightToElement(Warm_Chat, -50);
            }
        } else if (disguisedTarget.traitContainer.HasTrait("Hemophiliac")) {
            bool isKnownVampire = false;
            Vampire vampire = disguisedActor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            isKnownVampire = vampire != null && vampire.DoesCharacterKnowThisVampire(disguisedTarget);
            if (isKnownVampire) {
                chatWeights.AddWeightToElement(Warm_Chat, 50);
            }
        }

        //Lycanthrope
        if (disguisedActor.traitContainer.HasTrait("Lycanphobic")) {
            bool isKnownWerewolf = false;
            isKnownWerewolf = disguisedTarget.isLycanthrope && disguisedTarget.lycanData.DoesCharacterKnowThisLycan(disguisedActor);
            if (isKnownWerewolf) {
                chatWeights.AddWeightToElement(Warm_Chat, -50);
                chatWeights.AddWeightToElement(Insult, 50);
            }
        } else if (disguisedActor.traitContainer.HasTrait("Lycanphiliac")) {
            bool isKnownWerewolf = false;
            isKnownWerewolf = disguisedTarget.isLycanthrope && disguisedTarget.lycanData.DoesCharacterKnowThisLycan(disguisedActor);
            if (isKnownWerewolf) {
                chatWeights.AddWeightToElement(Warm_Chat, 50);
                chatWeights.AddWeightToElement(Praise, 50);
            }
        }
        if (disguisedTarget.traitContainer.HasTrait("Lycanphobic")) {
            bool isKnownWerewolf = false;
            isKnownWerewolf = disguisedActor.isLycanthrope && disguisedActor.lycanData.DoesCharacterKnowThisLycan(disguisedTarget);
            if (isKnownWerewolf) {
                chatWeights.AddWeightToElement(Warm_Chat, -50);
            }
        } else if (disguisedTarget.traitContainer.HasTrait("Lycanphiliac")) {
            bool isKnownWerewolf = false;
            isKnownWerewolf = disguisedActor.isLycanthrope && disguisedActor.lycanData.DoesCharacterKnowThisLycan(disguisedTarget);
            if (isKnownWerewolf) {
                chatWeights.AddWeightToElement(Warm_Chat, 50);
            }
        }

        if (disguisedActor.traitContainer.HasTrait("Hero") || disguisedTarget.traitContainer.HasTrait("Hero")) {
            chatWeights.RemoveElement(Argument);
#if DEBUG_LOG
            strLog += "\n\nActor or target is Hero, removing argument weight...";
#endif
        }
#if DEBUG_LOG
        strLog += $"\n\n{chatWeights.GetWeightsSummary("FINAL WEIGHTS")}";
#endif

        string result = chatWeights.PickRandomElementGivenWeights();
        if (!string.IsNullOrEmpty(result)) {
#if DEBUG_LOG
            strLog += $"\nResult: {result}";
            owner.logComponent.PrintLogIfActive(strLog);
#endif
            bool adjustOpinionBothSides = false;
            int opinionValue = 0;

            if (result == Warm_Chat) {
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

            if (owner.traitContainer.HasTrait("Plagued")) {
                AirborneTransmission.Instance.Transmit(owner, target, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
            }
            if (target.traitContainer.HasTrait("Plagued")) {
                AirborneTransmission.Instance.Transmit(target, owner, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
            }
            
            GameDate dueDate = GameManager.Instance.Today();
            if (overrideLog != null) { LogPool.Release(overrideLog); }
            overrideLog = GameManager.CreateNewLog(dueDate, "Interrupt", "Chat", result, providedTags: LOG_TAG.Social);
            overrideLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            owner.SetIsConversing(true);
            target.SetIsConversing(true);
            
            dueDate.AddTicks(2);
            SchedulingManager.Instance.AddEntry(dueDate, () => owner.SetIsConversing(false), owner);
            SchedulingManager.Instance.AddEntry(dueDate, () => target.SetIsConversing(false), target);
        }
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
        if (!string.IsNullOrEmpty(reason)) {
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Break Up", "break_up_reason", null, LogUtilities.Break_Up_Tags);
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
        } else {
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", "Break Up", "break_up", null, LogUtilities.Break_Up_Tags);
        }
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        owner.logComponent.RegisterLog(log, true);

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
    public bool NormalFlirtCharacter(Character target, Log overrideLog) {
        Character disguisedActor = owner;
        Character disguisedTarget = target;
        if (owner.reactionComponent.disguisedCharacter != null) {
            disguisedActor = owner.reactionComponent.disguisedCharacter;
        }
        if (target.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = target.reactionComponent.disguisedCharacter;
        }
        if (!disguisedActor.IsHostileWith(disguisedTarget) || disguisedTarget.combatComponent.combatMode == COMBAT_MODE.Passive) {
            string result = TriggerFlirtCharacter(target);
            
            if (owner.traitContainer.HasTrait("Plagued")) {
                AirborneTransmission.Instance.Transmit(owner, target, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
            }
            if (target.traitContainer.HasTrait("Plagued")) {
                AirborneTransmission.Instance.Transmit(target, owner, PlagueDisease.Instance.GetTransmissionLevel(PLAGUE_TRANSMISSION.Airborne));
            }
            
            GameDate dueDate = GameManager.Instance.Today();
            if (overrideLog != null) { LogPool.Release(overrideLog); }
            overrideLog = GameManager.CreateNewLog(dueDate, "Interrupt", "Flirt", result, providedTags: LOG_TAG.Social);
            overrideLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            overrideLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
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
        
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 50) {
            if (disguisedActor.traitContainer.HasTrait("Unattractive")) {
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "ugly";
            }
        }
        if (!disguisedTarget.limiterComponent.isSociable) {
            owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
            target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
            return "unsociable";
        }
        if (disguisedTarget.traitContainer.HasTrait("Hemophobic")) {
            bool isKnownVampire = false;
            Vampire vampire = disguisedActor.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
            isKnownVampire = vampire != null && vampire.DoesCharacterKnowThisVampire(disguisedTarget);
            if (isKnownVampire) {
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "vampire";
            }
        }
        if (disguisedTarget.traitContainer.HasTrait("Lycanphobic")) {
            bool isKnownWerewolf = false;
            isKnownWerewolf = disguisedActor.isLycanthrope && disguisedActor.lycanData.DoesCharacterKnowThisLycan(disguisedTarget);
            if (isKnownWerewolf) {
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "werewolf";
            }
        }
        if (chance < 70) {
            Trait angry = disguisedTarget.traitContainer.GetTraitOrStatus<Trait>("Angry");
            if (angry?.responsibleCharacters != null && angry.responsibleCharacters.Contains(disguisedActor)) {
                //target is angry at actor
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "angry";
            }
        }
        if(chance < 90) {
            Unfaithful unfaithful = disguisedActor.traitContainer.GetTraitOrStatus<Unfaithful>("Unfaithful");
            bool isCompatible = unfaithful?.IsCompatibleBasedOnSexualityAndOpinions(disguisedActor, disguisedTarget) ?? RelationshipManager.IsSexuallyCompatibleOneSided(disguisedTarget, disguisedActor);
            if(!isCompatible) {
                owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Rebuffed courtship", -8, "engaged in disastrous flirting");
                target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", -12, "engaged in disastrous flirting");
                return "incompatible";
            }
        }
        string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);

        //do not develop relationships if either actor or target is disguised
        //Note: Do not trigger new relationships if the 2 characters are from different factions
        //https://trello.com/c/G2bItJDj/4791-ibang-faction-tumira-sa-bahay
        if (!actorIsDisguised && !targetIsDisguised && disguisedActor.faction == disguisedTarget.faction) { //&& disguisedActor.isNormalCharacter && disguisedTarget.isNormalCharacter
            // If Opinion of Target towards Actor is already in Acquaintance range
            if (string.IsNullOrEmpty(opinionLabel) || opinionLabel == RelationshipManager.Acquaintance) {
                // 25% chance to develop Lover relationship if both characters have no Lover yet
                if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER)
                    && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.LOVER)) {
                    if (ChanceData.RollChance(CHANCE_TYPE.Flirt_Acquaintance_Become_Lover_Chance)) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER);
                    }
                }
                // 35% chance to develop Affair if at least one of the characters already have a Lover
                else if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR)
                         && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.AFFAIR)) {
                    if (ChanceData.RollChance(CHANCE_TYPE.Flirt_Acquaintance_Become_Affair_Chance)) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR);
                    }
                }
            }
            // If Opinion of Target towards Actor is already in Friend or Close Friend range
            else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                // 35 % chance to develop Lover relationship if both characters have no Lover yet
                if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER)
                    && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.LOVER)) {
                    if (ChanceData.RollChance(CHANCE_TYPE.Flirt_Friend_Become_Lover_Chance)) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.LOVER);
                    }
                }
                // 50% chance to develop Affair if at least one of the characters already have a Lover 
                else if (disguisedActor.relationshipValidator.CanHaveRelationship(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR)
                         && disguisedTarget.relationshipValidator.CanHaveRelationship(disguisedTarget, disguisedActor, RELATIONSHIP_TYPE.AFFAIR)) { 
                    if (ChanceData.RollChance(CHANCE_TYPE.Flirt_Friend_Become_Affair_Chance)) {
                        RelationshipManager.Instance.CreateNewRelationshipBetween(disguisedActor, disguisedTarget, RELATIONSHIP_TYPE.AFFAIR);
                    }
                }
            }
        }
        
        owner.relationshipContainer.AdjustOpinion(owner, disguisedTarget, "Reciprocated courtship", 6);
        target.relationshipContainer.AdjustOpinion(target, disguisedActor, "Conversations", 18);
        
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