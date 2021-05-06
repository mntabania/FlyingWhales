using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Traits;
using Inner_Maps;
using Interrupts;
using Inner_Maps.Location_Structures;
using Logs;
using UnityEngine.Assertions;
using Tutorial;
using UtilityScripts;
using Locations.Settlements;
using Object_Pools;
using Prison = Inner_Maps.Location_Structures.Prison;
using Random = System.Random;

public class ReactionComponent : CharacterComponent {
    private List<Character> _assumptionSuspects;
    public List<Character> charactersThatSawThisDead { get; private set; }
    public bool isHidden { get; private set; }
    public Character disguisedCharacter { get; private set; }

    #region getters
    public bool isDisguised => disguisedCharacter != null;
    #endregion
    
    public ReactionComponent() {
        _assumptionSuspects = new List<Character>();
        charactersThatSawThisDead = new List<Character>();
    }
    public ReactionComponent(SaveDataReactionComponent data) {
        _assumptionSuspects = new List<Character>();
        charactersThatSawThisDead = new List<Character>();
        isHidden = data.isHidden;
    }

    #region Processes
    public void ReactTo(IPointOfInterest target, ref string debugLog) {
        Character actor = owner;
        //if (actor.reactionComponent.disguisedCharacter != null) {
        //    actor = actor.reactionComponent.disguisedCharacter;
        //}
        if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            Character targetCharacter = target as Character; 
            Assert.IsNotNull(targetCharacter);
            ReactTo(actor, targetCharacter, ref debugLog);

            //If reacting to a disguised character, checking the carried poi must be from the disguised one, but the reaction must to the one he is disguised as.
            if (targetCharacter.carryComponent.carriedPOI is TileObject tileObject) {
                ReactToCarriedObject(actor, tileObject, targetCharacter, ref debugLog);
            }
        } else if (target.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            ReactTo(actor, target as TileObject, ref debugLog);
        } 
        // else if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
        //     ReactTo(targetTileObject as SpecialToken, ref debugLog);
        // }
        if (!actor.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to its own traits
            return;
        }
        if (actor.combatComponent.isInActualCombat) {
            return;
        }
        debugLog = $"{debugLog}\n-Character will loop through all his/her traits to react to Target";
        List<Trait> traitOverrideFunctions = actor.traitContainer.GetTraitOverrideFunctions(TraitManager.See_Poi_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                debugLog = $"{debugLog}\n - {trait.name}";
                if (trait.OnSeePOI(target, actor)) {
                    debugLog = $"{debugLog}: triggered";
                } else {
                    debugLog = $"{debugLog}: not triggered";
                }
            }
        }
    }
    public void ReactToDisguised(Character targetCharacter, Character copiedCharacter, ref string debugLog) {
        if(owner == copiedCharacter) {
            debugLog = $"{debugLog}{owner.name} is reacting to a copy of himself/herself";
            debugLog = $"{debugLog}Surprise interrupt and Fight response";
            owner.combatComponent.Fight(targetCharacter, CombatManager.Hostility);
            owner.interruptComponent.TriggerInterrupt(INTERRUPT.Surprised, targetCharacter, reason: Shocked.Copycat_Reason);
        } else {
            ReactTo(targetCharacter, ref debugLog);
            return;
        }

        //If reacting to a disguised character, checking the carried poi must be from the disguised one, but the reaction must to the one he is disguised as.
        if (targetCharacter.carryComponent.carriedPOI is TileObject tileObject) {
            ReactToCarriedObject(owner, tileObject, copiedCharacter, ref debugLog);
        }
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to its own traits
            return;
        }
        if (owner.combatComponent.isInActualCombat) {
            return;
        }
        debugLog = $"{debugLog}\n-Character will loop through all his/her traits to react to Target";
        List<Trait> traitOverrideFunctions = owner.traitContainer.GetTraitOverrideFunctions(TraitManager.See_Poi_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                debugLog = $"{debugLog}\n - {trait.name}";
                if (trait.OnSeePOI(copiedCharacter, owner)) {
                    debugLog = $"{debugLog}: triggered";
                } else {
                    debugLog = $"{debugLog}: not triggered";
                }
            }
        }
    }
    public string ReactTo(IReactable reactable, REACTION_STATUS status, bool addLog = true) {
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to actions
            return string.Empty;
        }
        if (reactable.awareCharacters.Contains(owner)) {
            return "aware";
        }
        reactable.AddAwareCharacter(owner);
        if (reactable.GetReactableEffect(owner) == REACTABLE_EFFECT.Negative) {
            if (reactable is ActualGoapNode node) {
                owner.rumorComponent.AddAssumedWitnessedOrInformedNegativeInfo(node);    
            } else if (reactable is Assumption assumption) {
                owner.rumorComponent.AddAssumedWitnessedOrInformedNegativeInfo(assumption.assumedAction);
            }
        }
        if (status == REACTION_STATUS.WITNESSED) {
            ReactToWitnessedReactable(reactable, addLog);
        } else {
            return ReactToInformedReactable(reactable, addLog);
        }
        return string.Empty;
    }
    private void ReactToWitnessedReactable(IReactable reactable, bool addLog) {
        if (owner.combatComponent.isInActualCombat) {
            return;
        }

        Character actor = reactable.actor;
        IPointOfInterest target = reactable.target;
        //Whenever a disguised character is being set as actor/target, set the original as the actor/target, as if they are the ones who did it
        if (actor.reactionComponent.disguisedCharacter != null) {
            actor = actor.reactionComponent.disguisedCharacter;
        }
        if (target is Character targetCharacter && targetCharacter.reactionComponent.disguisedCharacter != null) {
            target = targetCharacter.reactionComponent.disguisedCharacter;
        }

        if (owner.faction != null && actor.faction != null && owner.faction != actor.faction && owner.faction.IsHostileWith(actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return;
        }
        //if (witnessedEvent.currentStateName == null) {
        //    throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
        //}
        //if (string.IsNullOrEmpty(reactable.currentStateName)) {
        //    return;
        //}
        if (reactable.informationLog == null || !reactable.informationLog.hasValue) {
            //throw new Exception($"{GameManager.Instance.TodayLogString()}{owner.name} witnessed event {reactable.name} by {reactable.actor.name} does not have a log!");
            Debug.LogWarning($"{GameManager.Instance.TodayLogString()}{owner.name} witnessed event {reactable.name} by {reactable.actor.name} does not have a log!");
            return;
        }
        //if(reactable.target is TileObject item && reactable is ActualGoapNode node) {
        //    if (node.action.goapType == INTERACTION_TYPE.STEAL) {
        //        if (item.isBeingCarriedBy != null) {
        //            target = item.isBeingCarriedBy;
        //        }
        //    }
        //}
        if(actor != owner && target != owner) {
            string emotionsToActor = reactable.ReactionToActor(actor, target, owner, REACTION_STATUS.WITNESSED);
            if(emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error = $"{error}\n-Witness: {owner}";
                    error = $"{error}\n-Action: {reactable.name}";
                    error = $"{error}\n-Actor: {actor.name}";
                    error = $"{error}\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness", providedTags: LOG_TAG.Witnessed);
                    //// log.AddTag(reactable.logTags);
                    //log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    //log.AddLogToDatabase();
                }
            }
            string emotionsToTarget = reactable.ReactionToTarget(actor, target, owner, REACTION_STATUS.WITNESSED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                    error = $"{error}\n-Witness: {owner}";
                    error = $"{error}\n-Action: {reactable.name}";
                    error = $"{error}\n-Actor: {actor.name}";
                    error = $"{error}\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //NOTE: Do not log emotions to target
                    //add log of emotions felt
                    //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness", providedTags: LOG_TAG.Witnessed);
                    //// log.AddTag(reactable.logTags);
                    //log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //log.AddToFillers(reactable.target, reactable.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //log.AddToFillers(null, UtilityScripts.Utilities.Comafy(emotionsToTarget), LOG_IDENTIFIER.STRING_1);
                    //log.AddLogToDatabase();
                }
            }
            string response =
                $"Witness action reaction of {owner.name} to {reactable.name} of {actor.name} with target {reactable.target.name}: {emotionsToActor}{emotionsToTarget}";
            owner.logComponent.PrintLogIfActive(response);

            //Should not add witnessed log if there are no reaction/emotion felt
            //Reason: Assault action still logs that the character witnessed actor is assaulting target even if there are no reactions trigger4
            //NOTE: Only check for emotions to actor because we do not log emotions to target anymore to avoid log duplication
            if (addLog && emotionsToActor != string.Empty) {
                //Only log witness event if event is not an action. If it is an action, the CharacterManager.Instance.CanAddCharacterLogOrShowNotif must return true
                if (reactable is ActualGoapNode action && (!action.action.shouldAddLogs || !CharacterManager.Instance.CanAddCharacterLogOrShowNotif(action.goapType))) {
                    //Should not add witness log if the action log itself is not added to the actor
                } else {
                    Log witnessLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "witness_event", reactable as ActualGoapNode, LOG_TAG.Witnessed);
                    witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
                    witnessLog.AddToFillers(null, reactable.informationLog.unReplacedText, LOG_IDENTIFIER.APPEND);
                    witnessLog.AddToFillers(reactable.informationLog.fillers);

                    Log emotionsLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction", providedTags: LOG_TAG.Witnessed);
                    emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);

                    witnessLog.AddToFillers(null, emotionsLog.logText, LOG_IDENTIFIER.PARTY_2);
                    witnessLog.AddLogToDatabase(true);
                    LogPool.Release(emotionsLog);
                }
            }
        } else if (target == owner) {
            if (!reactable.isStealth || target.traitContainer.HasTrait("Vigilant")) {
                string emotionsOfTarget = reactable.ReactionOfTarget(actor, target, REACTION_STATUS.WITNESSED);
                if (emotionsOfTarget != string.Empty) {
                    if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                        string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                        error = $"{error}\n-Witness: {owner}";
                        error = $"{error}\n-Action: {reactable.name}";
                        error = $"{error}\n-Actor: {actor.name}";
                        error = $"{error}\n-Target: {reactable.target.nameWithID}";
                        owner.logComponent.PrintLogErrorIfActive(error);
                    } else {
                        //add log of emotions felt
                        Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddToFillers(null, UtilityScripts.Utilities.Comafy(emotionsOfTarget), LOG_IDENTIFIER.STRING_1);
                        log.AddLogToDatabase(true);
                    }
                }
                string response =
                    $"Witness action reaction of {owner.name} to {reactable.name} of {actor.name} with target {reactable.target.name}: {emotionsOfTarget}";
                owner.logComponent.PrintLogIfActive(response);
            }
        }

        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    private string ReactToInformedReactable(IReactable reactable, bool addLog) {
        if (reactable.informationLog == null || !reactable.informationLog.hasValue) {
            //throw new Exception($"{GameManager.Instance.TodayLogString()}{owner.name} informed event {reactable.name} by {reactable.actor.name} does not have a log!");
            Debug.LogWarning ($"{GameManager.Instance.TodayLogString()}{owner.name} informed event {reactable.name} by {reactable.actor.name} does not have a log!");
            return string.Empty;
        }

        Character actor = reactable.actor;
        IPointOfInterest target = reactable.target;
        //Whenever a disguised character is being set as actor/target, set the original as the actor/target, as if they are the ones who did it
        if (actor.reactionComponent.disguisedCharacter != null) {
            actor = actor.reactionComponent.disguisedCharacter;
        }
        if (target is Character targetCharacter && targetCharacter.reactionComponent.disguisedCharacter != null) {
            target = targetCharacter.reactionComponent.disguisedCharacter;
        }

        string response = string.Empty;
        if (actor != owner && target != owner) {
            string emotionsToActor = reactable.ReactionToActor(actor, target, owner, REACTION_STATUS.INFORMED);
            if (emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error = $"{error}\n-Witness: {owner}";
                    error = $"{error}\n-Action: {reactable.name}";
                    error = $"{error}\n-Actor: {actor.name}";
                    error = $"{error}\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_information", providedTags: LOG_TAG.Informed);
                    //// log.AddTag(reactable.logTags);
                    //log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    //log.AddLogToDatabase();
                }
            }
            string emotionsToTarget = reactable.ReactionToTarget(actor, target, owner, REACTION_STATUS.INFORMED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                    error = $"{error}\n-Witness: {owner}";
                    error = $"{error}\n-Action: {reactable.name}";
                    error = $"{error}\n-Actor: {actor.name}";
                    error = $"{error}\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    //Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_information", providedTags: LOG_TAG.Informed);
                    //// log.AddTag(reactable.logTags);
                    //log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    //log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToTarget, 2), LOG_IDENTIFIER.STRING_1);
                    //log.AddLogToDatabase();
                }
            }
            response = $"{response}{emotionsToActor}/{emotionsToTarget}";


            if (addLog && emotionsToActor != string.Empty) {
                Log informedLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "informed_event", reactable as ActualGoapNode, LOG_TAG.Informed);
                informedLog.AddToFillers(reactable.informationLog.fillers);
                informedLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
                informedLog.AddToFillers(null, reactable.informationLog.unReplacedText, LOG_IDENTIFIER.APPEND);

                Log emotionsLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction", providedTags: LOG_TAG.Informed);
                emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);

                informedLog.AddToFillers(null, emotionsLog.logText, LOG_IDENTIFIER.PARTY_2);
                informedLog.AddLogToDatabase(true);
                LogPool.Release(emotionsLog);
            }
        } else if(target == owner && target is Character) {
            string emotionsOfTarget = reactable.ReactionOfTarget(actor, target, REACTION_STATUS.INFORMED);
            if (emotionsOfTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                    string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                    error = $"{error}\n-Witness: {owner}";
                    error = $"{error}\n-Action: {reactable.name}";
                    error = $"{error}\n-Actor: {actor.name}";
                    error = $"{error}\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction", providedTags: LOG_TAG.Informed);
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsOfTarget, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToDatabase(true);
                }
            }
            response = emotionsOfTarget;
        }
        // else if (node.actor == owner) {
        //     response = "I know what I did.";
        // }
        return response;
        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }

    private void ReactTo(Character actor, Character targetCharacter, ref string debugLog) {
        debugLog = $"{debugLog}{actor.name} is reacting to {targetCharacter.name}";
        Character disguisedActor = actor;
        Character disguisedTarget = targetCharacter;
        if (actor.reactionComponent.disguisedCharacter != null) {
            disguisedActor = actor.reactionComponent.disguisedCharacter;
        }
        if (targetCharacter.reactionComponent.disguisedCharacter != null) {
            disguisedTarget = targetCharacter.reactionComponent.disguisedCharacter;
        }
        bool isHostile = false;
        if (disguisedTarget != targetCharacter && targetCharacter is SeducerSummon && !disguisedActor.isNormalCharacter) {
            //If actor is not a villager and the one that he saw is a Succubus/Incubus, check hostility with the Succubus/Incubus, not the disguised succubus so that the monsters will still not attack the disguised succubus even if he is disguised
            isHostile = disguisedActor.IsHostileWith(targetCharacter) && disguisedActor.IsLycanHostileWith(targetCharacter);
        } else {
            isHostile = disguisedActor.IsHostileWith(disguisedTarget) && disguisedActor.IsLycanHostileWith(disguisedTarget);
        }

        //TODO: Check if demooder and disabler can be set as cannot witness to achieve the same effect
        if (actor.behaviourComponent.HasBehaviour(typeof(DeMooderBehaviour)) ||
            actor.behaviourComponent.HasBehaviour(typeof(DisablerBehaviour))) {
            debugLog = $"{debugLog}\n-actor is demooder or disabler, do not react!";
            return;    
        }
        if (actor is VengefulGhost) {
            debugLog = $"{debugLog}\n-actor is vengeful ghost, do not react!";
            return;
        }
        if (actor.traitContainer.HasTrait("Dazed")) {
            debugLog = $"{debugLog}\n-Is dazed do not react!";
            return;
        }
        if(disguisedTarget is Dragon dragon && (!disguisedTarget.limiterComponent.canMove || !disguisedTarget.limiterComponent.canPerform) && actor.isNormalCharacter) {
            debugLog = $"{debugLog}\n-Target is dragon and Actor is normal character, will wary if has not yet wary";
            if (!dragon.charactersThatAreWary.Contains(actor)) {
                debugLog = $"{debugLog}\n-Will wary to dragon";
                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, dragon);
                dragon.AddCharacterThatWary(actor);
            }
        }

        if (disguisedTarget.characterClass.className == "Werewolf" && disguisedActor.homeSettlement != null && 
            disguisedTarget.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(disguisedActor.homeSettlement) && disguisedTarget.lycanData != null && 
            !disguisedTarget.lycanData.DoesCharacterKnowThisLycan(disguisedActor) && disguisedActor.homeSettlement.eventManager.CanHaveEvents()) {
            debugLog = $"{debugLog}\n-Target is a werewolf and is near {disguisedActor.homeSettlement.name}";
            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(disguisedActor, disguisedTarget, disguisedTarget, CRIME_TYPE.Werewolf);
            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable && !disguisedActor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt)) {
                debugLog = $"{debugLog}\n-Witness considers werewolf as a crime and there is no active werewolf hunt at active settlement";    
                if (GameUtilities.RollChance(25, ref debugLog)) {
                    debugLog = $"{debugLog}\n-Created new werewolf hunt event at {disguisedActor.homeSettlement.name}";
                    disguisedActor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt);       
                }
            }
        }

        if (actor.race == RACE.RATMAN) {
            Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
            if (prisoner != null && targetCharacter.traitContainer.HasTrait("Restrained")) {
                if (actor.faction == prisoner.prisonerOfFaction && !actor.IsAtHome()) {
                    LocationStructure targetStructureToDrop = null;
                    if (actor.homeStructure != null) {
                        if (!(actor.homeStructure is ThePortal)) {
                            targetStructureToDrop = actor.homeStructure;
                        }
                    } else if (actor.homeSettlement != null && actor.homeSettlement.mainStorage != null) {
                        targetStructureToDrop = actor.homeSettlement.mainStorage;
                    }
                    if (targetStructureToDrop != null) {
                        actor.jobComponent.CreateAbductJob(targetCharacter, targetStructureToDrop);
                    }
                    return;
                }
            }
        }
        
        if (isHostile) {
            debugLog = $"{debugLog}\n-Target is hostile";
            if(actor.currentJob != null && actor.currentActionNode != null && actor.currentActionNode.avoidCombat && actor.currentActionNode.actionStatus == ACTION_STATUS.STARTED
                && !targetCharacter.isDead && targetCharacter.limiterComponent.canPerform && targetCharacter.combatComponent.combatMode != COMBAT_MODE.Passive) {
                debugLog = $"{debugLog}\n-Actor encountered a hostile";
                actor.currentJob.CancelJob(false);
                actor.combatComponent.Flight(targetCharacter, CombatManager.Encountered_Hostile);
                return;
            }
            bool shouldRelease = disguisedActor.isNormalCharacter && !disguisedActor.traitContainer.HasTrait("Enslaved") && targetCharacter.traitContainer.HasTrait("Enslaved") && disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget)
                && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget) && !targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Enslaved").IsResponsibleForTrait(disguisedActor) && disguisedActor.faction != targetCharacter.faction;
            bool isPartOfRescueJob = actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.currentQuest is IRescuePartyQuest rescueQuest && rescueQuest.targetCharacter == targetCharacter
                && targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved");

            bool shouldNotAttackSkeletons = disguisedActor.traitContainer.HasTrait("Necromancer") && targetCharacter.race == RACE.SKELETON && targetCharacter.faction == disguisedActor.prevFaction && disguisedActor.prevFaction != null;
            bool shouldSwitchFaction = actor.race == RACE.SKELETON && disguisedTarget.traitContainer.HasTrait("Necromancer") && actor.faction == disguisedTarget.prevFaction && disguisedTarget.prevFaction != null;

            if (shouldNotAttackSkeletons) {
                debugLog = $"{debugLog}\n-Actor is a necromancer and target is a skeleton from the previous faction, will not attack even if hostile";
            } else if (shouldSwitchFaction) {
                debugLog = $"{debugLog}\n-Actor is a skeleton and target is a necromancer and actor is from the necromancer's previous faction, actor should switch to the faction of target";
                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, disguisedTarget, identifier: "join_faction_necro");
            } else if (shouldRelease || isPartOfRescueJob) {
                actor.jobComponent.TriggerReleaseJob(targetCharacter);
            } else if(disguisedActor is Troll && disguisedTarget.isNormalCharacter && disguisedActor.homeStructure != null && !targetCharacter.isDead) {
                debugLog = $"{debugLog}\n-Actor is a Troll and target is a Villager and actor has a home structure";
                if (targetCharacter.currentStructure != disguisedActor.homeStructure) {
                    if(!targetCharacter.limiterComponent.canPerform || !targetCharacter.limiterComponent.canMove) {
                        debugLog += "\n-Target cannot perform/move";
                        if (!actor.combatComponent.isInCombat) {
                            debugLog += "\n-Actor is not in combat, will try to bring target back to home";
                            if (!actor.jobQueue.HasJob(JOB_TYPE.CAPTURE_CHARACTER)) {
                                actor.jobComponent.TryTriggerCaptureCharacter(targetCharacter, disguisedActor.homeStructure, true);
                            } else {
                                debugLog += "\n-Actor already has a move character job, will ignore this target";
                            }
                        } else {
                            debugLog += "\n-Actor is in combat, will ignore this target";
                        }
                    } else {
                        debugLog += "\n-Will engage in combat and move it to its home";
                        //Determine whether to fight or flight.
                        //There is a special case, even if the source is defending if he/she is a demon and the target is an angel and vice versa, make the combat lethal
                        CombatReaction combatReaction = actor.combatComponent.GetFightOrFlightReaction(targetCharacter, CombatManager.Hostility);
                        if (combatReaction.reaction != COMBAT_REACTION.None) {
                            if (combatReaction.reaction == COMBAT_REACTION.Flight) {
                                //if flight was decided
                                //if target is restrained or resting, do nothing
                                if (!targetCharacter.traitContainer.HasTrait("Restrained", "Resting")) {
                                    actor.combatComponent.FightOrFlight(targetCharacter, combatReaction, isLethal: false);
                                }
                                //else {
                                //    actor.combatComponent.Fight(targetCharacter, combatReaction.reason, isLethal: false);
                                //}
                            } else {
                                actor.combatComponent.FightOrFlight(targetCharacter, combatReaction, isLethal: false);
                            }
                        }
                    }
                    
                } else {
                    if (!targetCharacter.traitContainer.HasTrait("Restrained")) {
                        debugLog = $"{debugLog}\n-Will engage in combat and restrain it";
                        actor.jobComponent.TriggerRestrainJob(targetCharacter, JOB_TYPE.CAPTURE_CHARACTER);
                    } else {
                        debugLog = $"{debugLog}\n-Target is already restrained, will do nothing";
                    }
                }
            } else if (disguisedActor.traitContainer.HasTrait("Cultist") && (disguisedTarget.faction.isPlayerFaction || disguisedTarget.traitContainer.HasTrait("Cultist"))) {
                debugLog = $"{debugLog}\n-{disguisedActor.name} is a cultist and {disguisedTarget.name} is part of the demon faction or is also a cultist.";
                int roll = UnityEngine.Random.Range(0, 100);
                int inspireChance = 30;
                if (roll < inspireChance) {
                    debugLog = $"{debugLog}\n-{actor.name} triggered inspired.";
                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Inspired, targetCharacter);
                } else {
                    //pray
                    debugLog = $"{debugLog}\n-{actor.name} triggered pray.";
                    actor.jobComponent.TriggerPray();
                }
            } else if (!targetCharacter.isDead && (disguisedTarget.combatComponent.combatMode != COMBAT_MODE.Passive || targetCharacter.race == RACE.HARPY) && !targetCharacter.traitContainer.HasTrait("Hibernating")) {
                //NOTE: Special case for Harpies: Even if harpies are passive, they should still be attacked. Reason: When a harpy tries to abduct a character in a village, all other villagers just ignores it
                //https://trello.com/c/nOPHWxxk/3518-unity-034080127-harpy-not-being-attacked-while-doing-abduct
                debugLog = $"{debugLog}\n-If Target is alive and not in Passive State and not Hibernating:";
                debugLog = $"{debugLog}\n-Fight or Flight response";
                //Fight or Flight
                if (disguisedActor.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                    //If the source is harassing or defending, combat should not be lethal
                    //There is a special case, even if the source is defending if he/she is a demon and the target is an angel and vice versa, make the combat lethal
                    // bool isLethal = /*(!disguisedActor.behaviourComponent.isHarassing && !disguisedActor.behaviourComponent.isDefending) || */ 
                    //     ((disguisedActor.race == RACE.DEMON && disguisedTarget.race == RACE.ANGEL) || (disguisedActor.race == RACE.ANGEL && disguisedTarget.race == RACE.DEMON));
                    bool isLethal = true;
                    bool isTopPrioJobLethal = actor.jobQueue.jobsInQueue.Count <= 0 || actor.jobQueue.jobsInQueue[0].jobType.IsJobLethal();
                    if(actor.partyComponent.hasParty && actor.partyComponent.currentParty.isActive && actor.partyComponent.currentParty.partyState == PARTY_STATE.Working) {
                        //If actor is in a raid party quest, all hostile attacks should be non lethal
                        //This is because of the changes in raid
                        //Now, the raid is for stealing or kidnapping only, that is why all hostile attacks should be non lethal
                        if(actor.partyComponent.currentParty.currentQuest is RaidPartyQuest) {
                            if (actor.partyComponent.isActiveMember) {
                                isLethal = false;
                            }
                        }
                    }
                    if (actor.jobQueue.jobsInQueue.Count > 0) {
                        debugLog = $"{debugLog}\n-{actor.jobQueue.jobsInQueue[0].jobType}";
                    }
                    
                    if (targetCharacter.defaultCharacterTrait.hasBeenAbductedByWildMonster && disguisedActor.faction?.factionType.type == FACTION_TYPE.Wild_Monsters) {
                        debugLog = $"{debugLog}\nActor is a wild monster and target has been abducted by a wild monster, did not trigger Fight or Flight response.";
                        return;
                    }
                    if (targetCharacter.defaultCharacterTrait.hasBeenAbductedByPlayerMonster && disguisedActor.faction.isPlayerFaction) {
                        debugLog = $"{debugLog}\nActor is part if player faction and target has been abducted by player faction, did not trigger Fight or Flight response.";
                        return;
                    }
                    // //NOTE: Added checking for webbed so that spiders won't attack characters that they've webbed up
                    // if (disguisedActor.race == RACE.SPIDER && targetCharacter.traitContainer.HasTrait("Webbed")) {
                    //     debugLog = $"{debugLog}\nActor is a spider and target is webbed, did not trigger Fight or Flight response.";
                    //     return;
                    // }
                    //NOTE: Added checking for minions/skeletons owned by the player so that they won't attack characters that have just been tortured/brainwashed (aka. Dazed)
                    if (disguisedActor.faction.isPlayerFaction && targetCharacter.traitContainer.HasTrait("Dazed")) {
                        debugLog = $"{debugLog}\nActor is part of player faction and target character is dazed, do not combat!.";
                        return;
                    }
                    Prisoner targetPrisonerStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                    if (!targetCharacter.isDead && targetPrisonerStatus != null && targetPrisonerStatus.IsConsideredPrisonerOf(disguisedActor)) {
                        LocationStructure intendedPrison = targetPrisonerStatus.GetIntendedPrisonAccordingTo(disguisedActor);
                        if(targetCharacter.currentStructure == intendedPrison) {
                            if (targetCharacter.needsComponent.isStarving) {
                                Vampire vampireTrait = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                                bool targetIsKnownVampire = vampireTrait != null && vampireTrait.DoesCharacterKnowThisVampire(disguisedActor);
                                if (targetIsKnownVampire) {
                                    if (disguisedActor.traitContainer.HasTrait("Hemophiliac") || (disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget) == RelationshipManager.Close_Friend && !disguisedActor.traitContainer.HasTrait("Hemophobic"))) {
                                        debugLog = $"{debugLog}\n-Target is starving and is known vampire and actor is Hemophiliac/Non-Hemophobic Close Friend, feed self";
                                        if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED, JOB_TYPE.OFFER_BLOOD)) {
                                            actor.jobComponent.CreateFeedSelfToVampireJob(targetCharacter);
                                        } else {
                                            debugLog = $"{debugLog}\n-Already has a feed/offer blood job targeting character";
                                        }
                                    }

                                } else {
                                    debugLog = $"{debugLog}\n-Target is starving, will create feed job";
                                    if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED)) {
                                        actor.jobComponent.TryTriggerFeed(targetCharacter);
                                    } else {
                                        debugLog = $"{debugLog}\n-Already has a feed job targeting character";
                                    }
                                }
                            }
                        } else {
                            bool canDoJob = false;
                            actor.jobComponent.TryCreateApprehend(targetCharacter, ref canDoJob, intendedPrison: intendedPrison);
                        }
                    } else {
                        //If the target is already unconscious/restrained (it cannot fight back), attack it again only if this character's top priority job is considered lethal
                        //Should not fight/flight if carrying a poi, they will only fight/flight when already attacked. Reason is that snatching characters will not be distracted when carrying the snatch victim
                        if ((!targetCharacter.traitContainer.HasTrait("Unconscious", "Restrained") || (isLethal && isTopPrioJobLethal)) && !actor.carryComponent.isCarryingAnyPOI) {
                            //Determine whether to fight or flight.
                            CombatReaction combatReaction = actor.combatComponent.GetFightOrFlightReaction(targetCharacter, CombatManager.Hostility);
                            if (combatReaction.reaction != COMBAT_REACTION.None) {
                                if (combatReaction.reaction == COMBAT_REACTION.Flight) {
                                    //if flight was decided
                                    //if target is restrained or resting, do nothing
                                    if (targetCharacter.traitContainer.HasTrait("Restrained", "Resting") == false) {
                                        actor.combatComponent.FightOrFlight(targetCharacter, combatReaction, isLethal: isLethal);
                                    }
                                } else {
                                    actor.combatComponent.FightOrFlight(targetCharacter, combatReaction, isLethal: isLethal);
                                }
                            }
                        }
                    }
                }
            } else {
                debugLog = $"{debugLog}\n-Target is dead or is passive or is hibernating";
                debugLog = $"{debugLog}\n-Do nothing";
            }
        } else if (!actor.combatComponent.isInActualCombat) {
            debugLog = $"{debugLog}\n-Target is not hostile and Character is not in combat";
            if (disguisedActor.isNormalCharacter) { // && !IsPOICurrentlyTargetedByAPerformingAction(targetCharacter)
                debugLog = $"{debugLog}\n-Character is a villager, continue reaction"; //and Target is not being targeted by an action
                if (!targetCharacter.isDead) {
                    debugLog = $"{debugLog}\n-Target is not dead";
                    if (!actor.isConversing && !targetCharacter.isConversing 
                        //only allow chat if characters current action is not have affair or if his action is have affair but the character he is reacting to is not the target of that action.
                        && (actor.currentActionNode == null || (actor.currentActionNode.action.goapType != INTERACTION_TYPE.HAVE_AFFAIR || actor.currentActionNode.poiTarget != targetCharacter))) {
                        debugLog = $"{debugLog}\n-Character and Target are not Chatting or Flirting and Character can interact with Target, has 3% chance to Chat";
                        int chance = UnityEngine.Random.Range(0, 100);
                        debugLog = $"{debugLog}\n-Roll: {chance.ToString()}";
                        if (actor.nonActionEventsComponent.CanChat(targetCharacter) && chance < 3) {
                            debugLog = $"{debugLog}\n-Chat triggered";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, targetCharacter);
                        } else {
                            debugLog = $"{debugLog}\n-Chat did not trigger, will now trigger Flirt if Character is Sexually Compatible with Target and Character is Unfaithful, or Target is Lover or Affair, or Character has no Lover and character is sociable.";
                            if (RelationshipManager.Instance.IsCompatibleBasedOnSexualityAndOpinion(disguisedActor, disguisedTarget) && disguisedActor.limiterComponent.isSociable) {
                                if (disguisedActor.nonActionEventsComponent.CanFlirt(disguisedActor, disguisedTarget)) {
                                    int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(disguisedActor, disguisedTarget);
                                    int baseChance = ChanceData.GetChance(CHANCE_TYPE.Flirt_On_Sight_Base_Chance);
                                    debugLog = $"{debugLog}\n-Flirt has {baseChance}% (multiplied by Compatibility value) chance to trigger";
                                    if (actor.moodComponent.moodState == MOOD_STATE.Normal) {
                                        debugLog = $"{debugLog}\n-Flirt has +2% chance to trigger because character is in a normal mood";
                                        baseChance += 2;
                                    }

                                    float flirtChance;
                                    if (compatibility != -1) {
                                        flirtChance = baseChance * compatibility;
                                        debugLog = $"{debugLog}\n-Chance: {flirtChance.ToString()}";
                                    } else {
                                        flirtChance = baseChance * 2;
                                        debugLog = $"{debugLog}\n-Chance: {flirtChance.ToString()} (No Compatibility)";
                                    }
                                    if (actor.relationshipContainer.GetFirstCharacterWithRelationship(RELATIONSHIP_TYPE.LOVER) == targetCharacter) {
                                        flirtChance = 0.2f;
                                    }
                                    bool succeed = GameUtilities.RollChance(flirtChance);
                                    
                                    debugLog = $"{debugLog}\n-Chance: {flirtChance.ToString()}";
                                    if (succeed) {
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, targetCharacter);
                                    } else {
                                        debugLog = $"{debugLog}\n-Flirt did not trigger";
                                    }
                                } else {
                                    debugLog = $"{debugLog}\n-Flirt did not trigger";
                                }
                            }
                        }
                    }

                    //Wanted Criminal Reaction Code:
                    Prisoner targetPrisonerStatus = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                    if (disguisedTarget.isNormalCharacter && disguisedActor.isNormalCharacter && disguisedActor.faction != null && disguisedTarget.crimeComponent.IsWantedBy(disguisedActor.faction)
                        && (!targetCharacter.traitContainer.HasTrait("Restrained") || (targetPrisonerStatus != null && targetPrisonerStatus.IsConsideredPrisonerOf(disguisedActor) && !targetPrisonerStatus.IsInIntendedPrisonAccordingTo(disguisedActor)))) { //if target is not restrained or not in prison, will create 
                        debugLog = $"{debugLog}\n-Target Character is a criminal";
                        bool cannotReactToCriminal = false;
                        if (actor.currentJob != null && actor.currentJob is GoapPlanJob planJob) {
                            cannotReactToCriminal = planJob.jobType == JOB_TYPE.APPREHEND && planJob.targetPOI == targetCharacter;
                            debugLog = $"{debugLog}\n-Character is current job is already apprehend targeting target";
                        }
                        if (!cannotReactToCriminal) {
                            string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
                            if ((opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) || 
                                disguisedActor.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(disguisedTarget)) {
                                debugLog = $"{debugLog}\n-Character is friends/close friend/family member/lover/affair/not rival with target";

                                Criminal targetCriminalTrait = disguisedTarget.traitContainer.GetTraitOrStatus<Criminal>("Criminal");
                                
                                if (targetCriminalTrait != null && !targetCriminalTrait.HasCharacterThatIsAlreadyWorried(disguisedActor) && 
                                    !disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor)) {
                                    debugLog = $"{debugLog}\n-Character will worry";
                                    targetCriminalTrait.AddCharacterThatIsAlreadyWorried(disguisedActor);
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
                                } else {
                                    debugLog = $"{debugLog}\n-Character already worried about this target or has reacted to it.";
                                }
                            } else {
                                debugLog = $"{debugLog}\n-Character is not friends with target";
                                debugLog = $"{debugLog}\n-Character will try to apprehend";
                                bool canDoJob = false;
                                if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.APPREHEND)) {
                                    actor.jobComponent.TryCreateApprehend(targetCharacter, ref canDoJob);
                                } else if (actor.homeSettlement != null) {
                                    JobQueueItem job = actor.homeSettlement.GetJob(JOB_TYPE.APPREHEND, targetCharacter);
                                    if (job != null && job.assignedCharacter == null) { canDoJob = owner.jobQueue.AddJobInQueue(job); }
                                }
                                if (!canDoJob) {
                                    if (!disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor)) {
                                        debugLog = $"{debugLog}\n-Character cannot do apprehend and has not yet reacted to the target, will become wary instead";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);    
                                    }
                                    BaseSettlement homeSettlement = actor.homeSettlement;
                                    if(homeSettlement != null && homeSettlement.locationType == LOCATION_TYPE.VILLAGE && homeSettlement is NPCSettlement npcSettlement) {
                                        npcSettlement.settlementJobTriggerComponent.TryCreateApprehend(targetCharacter);
                                    }
                                }
                            }
                        }
                    }
                    
                    //Accused Criminal Code:
                    if (disguisedTarget.isNormalCharacter && disguisedActor.isNormalCharacter && disguisedActor.faction != null && 
                        !disguisedTarget.crimeComponent.IsWantedBy(disguisedActor.faction) && disguisedTarget.crimeComponent.IsWitnessOfAnyActiveCrime(disguisedActor)) {
                        
                        string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
                        if ((opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) || 
                            disguisedActor.relationshipContainer.IsRelativeLoverOrAffairAndNotRival(disguisedTarget)) {
                            debugLog = $"{debugLog}\n-Character is friends/close friend/family member/lover/affair/not rival with target";

                            Criminal targetCriminalTrait = disguisedTarget.traitContainer.GetTraitOrStatus<Criminal>("Criminal");

                            if (targetCriminalTrait != null && !targetCriminalTrait.HasCharacterThatIsAlreadyWorried(disguisedActor) && 
                                !disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor)) {
                                debugLog = $"{debugLog}\n-Character will worry";
                                targetCriminalTrait.AddCharacterThatIsAlreadyWorried(disguisedActor);
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);
                            } else {
                                debugLog = $"{debugLog}\n-Character already worried about this target or has reacted to it.";
                            }
                        } else {
                            debugLog = $"{debugLog}\n-Character is not friends with target";
                            if (!disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor)) {
                                debugLog = $"{debugLog}\n-Has not yet reacted to target, will Wary";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
                            }
                            if (GameUtilities.RollChance(30) && disguisedActor.crimeComponent.HasUnreportedCrimeOf(disguisedTarget)) {
                                debugLog = $"{debugLog}\n-Character has unreported crime towards target, will try to apprehend.";
                                if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.APPREHEND)) {
                                    debugLog = $"{debugLog}\n-No active apprehend jobs yet, will create personal apprehend.";
                                    bool canDoJob = false;
                                    actor.jobComponent.TryCreateApprehend(targetCharacter, ref canDoJob);
                                } else {
                                    debugLog = $"{debugLog}\n-Already has active apprehend job. Check if can take settlement apprehend job";
                                    if (disguisedActor.homeSettlement != null) {
                                        JobQueueItem job = actor.homeSettlement.GetJob(JOB_TYPE.APPREHEND, targetCharacter);
                                        if (job != null && job.assignedCharacter == null) { owner.jobQueue.AddJobInQueue(job); }
                                    }
                                }
                            }
                        }
                    }

                    if (disguisedActor.faction == disguisedTarget.faction || disguisedActor.homeSettlement == disguisedTarget.homeSettlement) {
                        debugLog = $"{debugLog}\n-Character and Target are with the same faction or npcSettlement";
                        if (disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget)) {
                            debugLog = $"{debugLog}\n-Character considers Target as Enemy or Rival";
                            if ((!targetCharacter.limiterComponent.canMove || !targetCharacter.limiterComponent.canPerform) && 
                                !targetCharacter.defaultCharacterTrait.HasReactedToThis(owner) && !targetCharacter.traitContainer.HasTrait("Resting")) {
                                debugLog = $"{debugLog}\n-Target can neither move or perform and actor has not yet reacted to target.";
                                // if (disguisedActor.moodComponent.moodState == MOOD_STATE.Bad || disguisedActor.moodComponent.moodState == MOOD_STATE.Critical) {
                                // debuglog = log +"\n-Actor is in Bad or Critical mood";
                                if (UnityEngine.Random.Range(0, 2) == 0) {
                                    debugLog = $"{debugLog}\n-Character triggered Mock interrupt";
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                                } else {
                                    debugLog = $"{debugLog}\n-Character triggered Laugh At interrupt";
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                                }
                                // } else {
                                //     debuglog = log +"\n-Actor is in Normal mood, will trigger shocked interrupt";
                                //     actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                                // }
                            }
                        } else if (!disguisedActor.traitContainer.HasTrait("Psychopath")) {
                            debugLog = $"{debugLog}\n-Character is not Psychopath and does not consider Target as Enemy or Rival";
                            bool targetIsParalyzedOrEnsnared = targetCharacter.traitContainer.HasTrait("Paralyzed", "Ensnared");
                            bool targetIsQuarantined = targetCharacter.traitContainer.HasTrait("Quarantined");
                            bool targetIsRestrainedCriminal = targetCharacter.traitContainer.HasTrait("Restrained") && disguisedTarget.traitContainer.HasTrait("Criminal");
                            bool targetIsCatatonic = targetCharacter.traitContainer.HasTrait("Catatonic");

                            Vampire vampireTrait = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");

                            bool targetIsKnownVampire = vampireTrait != null && vampireTrait.DoesCharacterKnowThisVampire(disguisedActor);
                            bool targetIsKnownWerewolf = disguisedTarget.isLycanthrope && disguisedTarget.lycanData.DoesCharacterKnowThisLycan(disguisedActor);


                            if (targetIsParalyzedOrEnsnared || targetIsRestrainedCriminal || targetIsCatatonic || targetIsQuarantined) {
                                debugLog = $"{debugLog}\n-Target is Restrained Criminal({targetIsRestrainedCriminal.ToString()}) or " +
                                           $"is Paralyzed or Ensnared({targetIsParalyzedOrEnsnared.ToString()}) or is Catatonic {targetIsCatatonic.ToString()}";
                                if ((targetCharacter.needsComponent.isHungry || targetCharacter.needsComponent.isStarving) && !targetIsKnownVampire) {
                                    debugLog = $"{debugLog}\n-Target is hungry or starving and not known vampire, will create feed job";
                                    if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED)) {
                                        actor.jobComponent.TryTriggerFeed(targetCharacter);
                                    } else {
                                        debugLog = $"{debugLog}\n-Already has a feed job targeting character";
                                    }
                                } else if ((targetCharacter.needsComponent.isTired || targetCharacter.needsComponent.isExhausted) && targetIsParalyzedOrEnsnared && !targetIsQuarantined) {
                                    debugLog = $"{debugLog}\n-Target is tired or exhausted, will create Move Character job to bed if Target has a home and an available bed";
                                    if (disguisedTarget.homeStructure != null) {
                                        Bed bed = disguisedTarget.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED) as Bed;
                                        if (bed != null && bed.gridTileLocation != targetCharacter.gridTileLocation) {
                                            debugLog = $"{debugLog}\n-Target has a home and an available bed, will trigger Move Character job to bed";
                                            if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER)) {
                                                if(targetCharacter.currentActionNode == null) {
                                                    actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, disguisedTarget.homeStructure, bed.gridTileLocation);
                                                }
                                            } else {
                                                debugLog = $"{debugLog}\n-Already has a move character job targeting character";
                                            }
                                        } else {
                                            debugLog = $"{debugLog}\n-Target has a home but does not have an available bed or already in bed, will not trigger Move Character job";
                                        }
                                    } else {
                                        debugLog = $"{debugLog}\n-Target does not have a home, will not trigger Move Character job";
                                    }
                                } else if ((targetCharacter.needsComponent.isBored || targetCharacter.needsComponent.isSulking) && targetIsParalyzedOrEnsnared && !targetIsQuarantined) {
                                    debugLog = $"{debugLog}\n-Target is bored or sulking, will trigger Move Character job if character is not in the right place to do Daydream or Pray";
                                    if (UnityEngine.Random.Range(0, 2) == 0 && disguisedTarget.homeStructure != null) {
                                        //Pray
                                        if (targetCharacter.currentStructure != disguisedTarget.homeStructure) {
                                            debugLog = $"{debugLog}\n-Target chose Pray and is not inside his/her house, will trigger Move Character job";
                                            if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER)) {
                                                if (targetCharacter.currentActionNode == null) {
                                                    actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, disguisedTarget.homeStructure);
                                                }
                                            } else {
                                                debugLog = $"{debugLog}\n-Already has a move character job targeting character";
                                            }
                                        } else {
                                            debugLog = $"{debugLog}\n-Target chose Pray but is already inside his/her house, will not trigger Move Character job";
                                        }
                                    } else {
                                        //Daydream
                                        if (!targetCharacter.currentStructure.structureType.IsOpenSpace()) {
                                            debugLog = $"{debugLog}\n-Target chose Daydream and is not in an open space structure, will trigger Move Character job";
                                            if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.MOVE_CHARACTER)) {
                                                if (targetCharacter.currentActionNode == null) {
                                                    actor.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                                                }
                                            } else {
                                                debugLog = $"{debugLog}\n-Already has a move character job targeting character";
                                            }
                                        } else {
                                            debugLog = $"{debugLog}\n-Target chose Daydream but is already in an open space structure, will not trigger Move Character job";
                                        }
                                    }
                                }
                            }

                            if (targetIsKnownVampire) {
                                debugLog = $"{debugLog}\n-Target is known vampire";
                                if (disguisedActor.characterClass.className == "Shaman" && disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget) && 
                                    !disguisedActor.traitContainer.HasTrait("Hemophiliac") && disguisedActor.faction != null && 
                                    !disguisedActor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Vampires)) {
                                    //Cure Magical Affliction
                                    debugLog = $"{debugLog}\n-Actor is Shaman and is Friend/Close Friend with target";
                                    actor.jobComponent.TriggerCureMagicalAffliction(disguisedTarget, "Vampire");
                                } else if (disguisedActor.traitContainer.HasTrait("Hemophobic")) {
                                    debugLog = $"{debugLog}\n-Actor is Hemophobic, will wary";
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
                                } else if (targetCharacter.needsComponent.isStarving) {
                                    if (disguisedActor.traitContainer.HasTrait("Hemophiliac") || disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget) == RelationshipManager.Close_Friend) {
                                        debugLog = $"{debugLog}\n-Target is starving and is known vampire and actor is Hemophiliac/Close Friend, feed self";
                                        if (!targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.FEED, JOB_TYPE.OFFER_BLOOD)) {
                                            actor.jobComponent.CreateFeedSelfToVampireJob(targetCharacter);
                                        } else {
                                            debugLog = $"{debugLog}\n-Already has a feed/offer blood job targeting character";
                                        }
                                    }
                                }
                            }

                            if (targetIsKnownWerewolf) {
                                debugLog = $"{debugLog}\n-Target is known werewolf";
                                if (disguisedActor.characterClass.className == "Shaman" && disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget) && 
                                    !disguisedActor.traitContainer.HasTrait("Lycanphiliac") && disguisedActor.faction != null && 
                                    !disguisedActor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                                    //Cure Magical Affliction
                                    debugLog = $"{debugLog}\n-Actor is Shaman and is Friend/Close Friend with target";
                                    actor.jobComponent.TriggerCureMagicalAffliction(disguisedTarget, "Lycanthrope");
                                } else if (disguisedActor.traitContainer.HasTrait("Lycanphobic")) {
                                    debugLog = $"{debugLog}\n-Actor is Lycanphobic, will wary";
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetCharacter);
                                }
                            }

                            //Add personal Remove Status - Restrained job when seeing a restrained non-enemy villager
                            //https://trello.com/c/Pe6wuHQc/1197-add-personal-remove-status-restrained-job-when-seeing-a-restrained-non-enemy-villager
                            Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                            Lazy lazy = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
                            if (lazy == null ||!lazy.TryIgnoreUrgentTask(JOB_TYPE.REMOVE_STATUS)) {
                                if(prisoner == null || !prisoner.IsConsideredPrisonerOf(disguisedActor)) {
                                    bool isRestrained = targetCharacter.traitContainer.HasTrait("Restrained");
                                    bool isEnsnared = targetCharacter.traitContainer.HasTrait("Ensnared");
                                    bool isFrozen = targetCharacter.traitContainer.HasTrait("Frozen");
                                    bool isUnconscious = targetCharacter.traitContainer.HasTrait("Unconscious");
                                    bool isEnslaved = targetCharacter.traitContainer.HasTrait("Enslaved");
                                    bool isEnslavedAndNotEnemy = isEnslaved && disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget) && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget);

                                    if (disguisedActor.isNormalCharacter && ((disguisedTarget.isNormalCharacter && (isRestrained || isEnsnared || isFrozen || isUnconscious)) || isEnslavedAndNotEnemy) &&
                                        !disguisedTarget.crimeComponent.IsWantedBy(disguisedActor.faction)) {

                                        bool isResponsibleForRestrained = isRestrained && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Restrained").IsResponsibleForTrait(disguisedActor);
                                        bool isResponsibleForEnsnared = isEnsnared && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Ensnared").IsResponsibleForTrait(disguisedActor);
                                        bool isResponsibleForFrozen = isFrozen && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Frozen").IsResponsibleForTrait(disguisedActor);
                                        bool isResponsibleForUnconscious = isUnconscious && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Unconscious").IsResponsibleForTrait(disguisedActor);
                                        bool isResponsibleForEnslaved = isEnslaved && targetCharacter.traitContainer.GetTraitOrStatus<Trait>("Enslaved").IsResponsibleForTrait(disguisedActor);

                                        //NOTE: Manyayari na lang ito kapag same home settlement na lang yung actor and target
                                        if (!disguisedActor.traitContainer.HasTrait("Enslaved") && isEnslaved && !isResponsibleForEnslaved && disguisedActor.faction != targetCharacter.faction) {
                                            actor.jobComponent.TriggerReleaseJob(targetCharacter);
                                        }
                                        if (!targetCharacter.HasJobTargetingThis(JOB_TYPE.REMOVE_STATUS)) {
                                            if (isRestrained && !isResponsibleForRestrained) {
                                                actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Restrained");
                                            }
                                            if (isEnsnared && !isResponsibleForEnsnared) {
                                                actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Ensnared");
                                            }
                                            if (isFrozen && !isResponsibleForFrozen) {
                                                actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Frozen");
                                            }
                                            if (isUnconscious && !isResponsibleForUnconscious) {
                                                actor.jobComponent.TriggerRemoveStatusTarget(targetCharacter, "Unconscious");
                                            }
                                        } else {
                                            if(!isResponsibleForRestrained && !isResponsibleForEnsnared && !isResponsibleForFrozen && !isResponsibleForUnconscious) {
                                                if (!disguisedTarget.defaultCharacterTrait.HasReactedToThis(disguisedActor)) {
                                                    if (GameUtilities.RollChance(35)) {
                                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Worried, targetCharacter);    
                                                    } else {
                                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter, reason: "someone they know is in a bind");  
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }    
                            }
                        }

                        //Plagued Settlement Event
                        if (disguisedActor.homeSettlement != null && disguisedActor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event)) {
                            Lethargic lethargic = disguisedTarget.traitContainer.GetTraitOrStatus<Lethargic>("Lethargic");
                            if (lethargic != null && !lethargic.IsResponsibleForTrait(disguisedActor) && !disguisedActor.defaultCharacterTrait.IsAwareOfTrait(disguisedActor, lethargic)) {
                                disguisedActor.defaultCharacterTrait.BecomeAwareOfTrait(disguisedTarget, lethargic);
                                if (GameUtilities.RollChance(25) && !disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget)) { //25
                                    disguisedActor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_PLAGUED, REACTION_STATUS.WITNESSED);
                                }
                            }
                        }
                    } else if (disguisedActor.faction != disguisedTarget.faction && disguisedActor.faction != null && disguisedTarget.faction != null) {
                        //https://trello.com/c/Rictd9YD/2569-on-sight-of-restrained-ensnared-frozen-and-unconscious
                        FactionRelationship factionRel = disguisedActor.faction.GetRelationshipWith(disguisedTarget.faction);
                        if(factionRel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Neutral || factionRel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Friendly) {
                            //If actor's faction is friendly/neutral with prisoner's faction and prisoner is restrained
                            Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                            Lazy lazy = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
                            if (lazy == null || !lazy.TryIgnoreUrgentTask(JOB_TYPE.RELEASE_CHARACTER)) {
                                if(prisoner != null && !prisoner.IsConsideredPrisonerOf(disguisedActor)) {
                                    bool isRestrained = targetCharacter.traitContainer.HasTrait("Restrained");
                                    if (isRestrained) {
                                        Faction factionThatImprisoned = prisoner.GetFactionThatImprisoned();
                                        //Release prisoner if actor's faction is hostile with the faction that imprisoned the prisoner and prisoner is not enemy/rival of the actor
                                        //Or if the prisoner is friend/close friend/acquaintance
                                        bool shouldRelease = false;
                                        if(factionThatImprisoned != null && disguisedActor.faction != factionThatImprisoned) {
                                            FactionRelationship relWithFactionTheImprisoned = disguisedActor.faction.GetRelationshipWith(factionThatImprisoned);
                                            if(relWithFactionTheImprisoned.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile && !disguisedActor.relationshipContainer.IsEnemiesWith(disguisedTarget)) {
                                                shouldRelease = true;
                                            }
                                        }
                                        if (!shouldRelease) {
                                            string opinionTowardsPrisoner = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
                                            if(opinionTowardsPrisoner == RelationshipManager.Close_Friend || opinionTowardsPrisoner == RelationshipManager.Friend || opinionTowardsPrisoner == RelationshipManager.Acquaintance) {
                                                shouldRelease = true;
                                            }
                                        }
                                        if (shouldRelease) {
                                            actor.jobComponent.TriggerReleaseJob(targetCharacter);
                                        }
                                    }
                                }    
                            }
                        }
                    }
                    
                    if (disguisedTarget.race == RACE.WOLF && disguisedTarget.traitContainer.HasTrait("Restrained") && disguisedActor.faction.factionType.HasIdeology(FACTION_IDEOLOGY.Reveres_Werewolves)) {
                        //Reference: https://trello.com/c/NPXg3GZs/2828-restrained-wolves-should-be-freed-by-reveres-werewolves-faction-members
                        actor.jobComponent.TriggerReleaseJob(targetCharacter);
                    }
                    
                    //nocturnal
                    if (targetCharacter.limiterComponent.canPerform && !targetCharacter.partyComponent.isMemberThatJoinedQuest && !disguisedTarget.crimeComponent.IsCrimeAlreadyWitnessedBy(disguisedActor, CRIME_TYPE.Vampire)) {
                        debugLog = $"{debugLog}\n-Target can perform and not an active member of a party that has a quest and has not yet witnessed a vampire crime of actor";
                        TIME_IN_WORDS timeInWords = GameManager.Instance.GetCurrentTimeInWordsOfTick();
                        if (timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
                            debugLog = $"{debugLog}\n-Current time is After midnight";
                            if (disguisedActor.homeSettlement != null && disguisedActor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Vampire_Hunt)) {
                                CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(disguisedActor, disguisedTarget, disguisedTarget, CRIME_TYPE.Vampire);
                                if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable) {
                                    debugLog = $"{debugLog}\n-Witness' home settlement considers vampirism as a crime";
                                    Vampire vampire = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                                    if (vampire == null || !vampire.DoesCharacterKnowThisVampire(disguisedActor)) {
                                        if (disguisedActor.traitContainer.HasTrait("Suspicious") || disguisedActor.moodComponent.moodState == MOOD_STATE.Critical) {
                                            debugLog = $"{debugLog}\n-Witness is suspicious or in critical mood, will create assumption that {disguisedTarget.name} is a vampire";
                                            actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_VAMPIRE, REACTION_STATUS.WITNESSED);
                                        } else if (disguisedActor.moodComponent.moodState == MOOD_STATE.Bad && !disguisedActor.relationshipContainer.IsFriendsWith(disguisedTarget)) {
                                            debugLog = $"{debugLog}\n-Witness is in bad mood, and is not friend/close friend with {disguisedTarget.name}";
                                            if (disguisedTarget.currentStructure is Dwelling dwelling && dwelling != disguisedTarget.homeStructure) {
                                                debugLog = $"{debugLog}\n-{disguisedTarget.name} is at another dwelling {dwelling.name}. Rolling for chance to create assumption";
                                                if (GameUtilities.RollChance(50, ref debugLog)) {
                                                    debugLog = $"{debugLog}\n-Created new is vampire assumption";
                                                    actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_VAMPIRE, REACTION_STATUS.WITNESSED);        
                                                }
                                            } else if (disguisedTarget.gridTileLocation != null && disguisedTarget.gridTileLocation.IsPartOfSettlement(disguisedActor.homeSettlement)) {
                                                debugLog = $"{debugLog}\n-{disguisedTarget.name} is inside settlement at night. Rolling for chance to create assumption";
                                                if (GameUtilities.RollChance(35, ref debugLog)) {
                                                    debugLog = $"{debugLog}\n-Created new is vampire assumption";
                                                    actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_VAMPIRE, REACTION_STATUS.WITNESSED);        
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    
                    if (disguisedTarget.characterClass.className == "Vampire Lord") {
                        debugLog = $"{debugLog}\n-Target is Vampire Lord";
                        //saw a vampire lord
                        Vampire vampire = disguisedTarget.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                        Assert.IsNotNull(vampire, $"{disguisedActor.name} saw Vampire Lord {disguisedTarget.name}, but {disguisedTarget.name} does not have a Vampire trait!");
                        if (!vampire.DoesCharacterKnowThisVampire(disguisedActor)) {
                            debugLog = $"{debugLog}\n-Will create is vampire assumption";
                            actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, disguisedTarget, INTERACTION_TYPE.IS_VAMPIRE, REACTION_STATUS.WITNESSED);
                        }
                    }
                    
                } else {
                    debugLog = $"{debugLog}\n-Target is dead";
                    //Dead targetDeadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
                    Dead deadTrait = targetCharacter.traitContainer.GetTraitOrStatus<Dead>("Dead");
                    if (!targetCharacter.reactionComponent.charactersThatSawThisDead.Contains(disguisedActor)) { //targetDeadTrait != null && !targetDeadTrait.charactersThatSawThisDead.Contains(owner)
                        targetCharacter.reactionComponent.AddCharacterThatSawThisDead(disguisedActor);
                        debugLog = $"{debugLog}\n-Target saw dead for the first time";
                        if (disguisedActor.traitContainer.HasTrait("Psychopath")) {
                            debugLog = $"{debugLog}\n-Actor is Psychopath";
                            if (targetCharacter.isNormalCharacter) {
                                debugLog = $"{debugLog}\n-Target is a normal character";
                                if (deadTrait == null || !deadTrait.IsResponsibleForTrait(actor)) {
                                    if (UnityEngine.Random.Range(0, 2) == 0) {
                                        debugLog = $"{debugLog}\n-Target will Mock";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                                    } else {
                                        debugLog = $"{debugLog}\n-Target will Laugh At";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                                    }
                                }
                            }
                        } else {
                            debugLog = $"{debugLog}\n-Actor is not Psychopath";
                            string opinionLabel = disguisedActor.relationshipContainer.GetOpinionLabel(disguisedTarget);
                            if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                                debugLog = $"{debugLog}\n-Target is Friend/Close Friend";
                                if (actor.traitContainer.HasTrait("Vampire") && targetCharacter.grave == null && GameUtilities.RollChance(15)) {
                                    debugLog = $"{debugLog}\n-Actor is Vampire and Target has no grave, will Cry and add Vampiric Embrace Job";
                                    actor.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.VAMPIRIC_EMBRACE, targetCharacter);
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, $"saw dead {disguisedTarget.name}");
                                } else {
                                    if(deadTrait == null || !deadTrait.IsResponsibleForTrait(actor)) {
                                        if (UnityEngine.Random.Range(0, 2) == 0) {
                                            debugLog = $"{debugLog}\n-Target will Cry";
                                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, $"saw dead {disguisedTarget.name}");
                                        } else {
                                            debugLog = $"{debugLog}\n-Target will Puke";
                                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, $"saw dead {disguisedTarget.name}");
                                        }
                                    }
                                }
                            } else if ((disguisedActor.relationshipContainer.IsFamilyMember(disguisedTarget) ||
                                        disguisedActor.relationshipContainer.HasRelationshipWith(disguisedTarget, RELATIONSHIP_TYPE.AFFAIR)) &&
                                      !disguisedActor.relationshipContainer.HasOpinionLabelWithCharacter(disguisedTarget, RelationshipManager.Rival)) {
                                debugLog = $"{debugLog}\n-Target is Relative, Lover or Affair and not Rival";
                                // if Actor is Relative, Lover, Affair and not a Rival
                                if (deadTrait == null || !deadTrait.IsResponsibleForTrait(actor)) {
                                    if (UnityEngine.Random.Range(0, 2) == 0) {
                                        debugLog = $"{debugLog}\n-Target will Cry";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, $"saw dead {disguisedTarget.name}");
                                    } else {
                                        debugLog = $"{debugLog}\n-Target will Puke";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, $"saw dead {disguisedTarget.name}");
                                    }
                                }
                            } else if (opinionLabel == RelationshipManager.Enemy) {
                                debugLog = $"{debugLog}\n-Target is Enemy";
                                if (deadTrait == null || !deadTrait.IsResponsibleForTrait(actor)) {
                                    if (UnityEngine.Random.Range(0, 100) < 25) {
                                        if (UnityEngine.Random.Range(0, 2) == 0) {
                                            debugLog = $"{debugLog}\n-Target will Mock";
                                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                                        } else {
                                            debugLog = $"{debugLog}\n-Target will Laugh At";
                                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                                        }
                                    } else {
                                        debugLog = $"{debugLog}\n-Shock";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                                    }
                                }
                            } else if (opinionLabel == RelationshipManager.Rival) {
                                debugLog = $"{debugLog}\n-Target is Rival";
                                if (deadTrait == null || !deadTrait.IsResponsibleForTrait(actor)) {
                                    if (UnityEngine.Random.Range(0, 2) == 0) {
                                        debugLog = $"{debugLog}\n-Target will Mock";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                                    } else {
                                        debugLog = $"{debugLog}\n-Target will Laugh At";
                                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                                    }
                                }
                            } else if (targetCharacter.isNormalCharacter && actor.relationshipContainer.HasRelationshipWith(targetCharacter)) {
                                debugLog = $"{debugLog}\n-Otherwise, Shock";
                                if (deadTrait == null || !deadTrait.IsResponsibleForTrait(actor)) {
                                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                                }
                            }
                        }

                        if (actor.marker && disguisedTarget.isNormalCharacter && deadTrait != null && (deadTrait.gainedFromDoing == null || deadTrait.gainedFromDoing.goapType != INTERACTION_TYPE.EXECUTE)) {
                            if(disguisedActor.traitContainer.HasTrait("Suspicious") || actor.moodComponent.moodState == MOOD_STATE.Critical || 
                               (actor.moodComponent.moodState == MOOD_STATE.Bad && UnityEngine.Random.Range(0, 2) == 0) || UnityEngine.Random.Range(0, 100) < 15) {
                                debugLog = $"{debugLog}\n-Owner is Suspicious or Critical Mood or Low Mood";

                                _assumptionSuspects.Clear();
                                for (int i = 0; i < actor.marker.inVisionCharacters.Count; i++) {
                                    Character inVision = actor.marker.inVisionCharacters[i];
                                    if (inVision != targetCharacter && !inVision.isDead && inVision.relationshipContainer.IsEnemiesWith(disguisedTarget)) {
                                        if(inVision.currentJob != null && inVision.currentJob.jobType == JOB_TYPE.BURY) {
                                            //If the in vision character is going to bury the dead, do not assume
                                            continue;
                                        }
                                        _assumptionSuspects.Add(inVision);
                                    }
                                }
                                if(_assumptionSuspects.Count > 0) {
                                    debugLog = $"{debugLog}\n-There are in vision characters that considers target character as Enemy/Rival";
                                    Character chosenSuspect = _assumptionSuspects[UnityEngine.Random.Range(0, _assumptionSuspects.Count)];

                                    debugLog = debugLog + ("\n-Will create Murder assumption on " + chosenSuspect.name);
                                    actor.assumptionComponent.CreateAndReactToNewAssumption(chosenSuspect, disguisedTarget, INTERACTION_TYPE.MURDER, REACTION_STATUS.WITNESSED);
                                }
                            }
                        }

                        if (disguisedTarget.traitContainer.HasTrait("Mangled") && disguisedActor.homeSettlement != null && disguisedActor.homeSettlement.eventManager.CanHaveEvents()) {
                            CRIME_SEVERITY severity = CrimeManager.Instance.GetCrimeSeverity(disguisedActor, disguisedTarget, disguisedTarget, CRIME_TYPE.Werewolf);
                            if (severity != CRIME_SEVERITY.None && severity != CRIME_SEVERITY.Unapplicable && !disguisedActor.homeSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt)) {
                                debugLog = $"{debugLog}\n-Saw a mangled body and no active werewolf hunt is active yet and considers werewolf as a crime.";
                                if (disguisedTarget.gridTileLocation.IsNextToSettlementAreaOrPartOfSettlement(disguisedActor.homeSettlement)) {
                                    debugLog = $"{debugLog}\n-Mangled body is near settlement, will roll for chance to create werewolf hunt event";
                                    if (GameUtilities.RollChance(25, ref debugLog)) {
                                        debugLog = $"{debugLog}\n-Created new werewolf hunt event!";
                                        disguisedActor.homeSettlement.eventManager.AddNewActiveEvent(SETTLEMENT_EVENT.Werewolf_Hunt);
                                    }
                                }    
                            }
                        }
                    }
                }

                if (targetCharacter.marker && targetCharacter.isNormalCharacter) {
                    if (targetCharacter.carryComponent.isCarryingAnyPOI && targetCharacter.carryComponent.carriedPOI is Character carriedCharacter) {
                        debugLog = $"{debugLog}\n-Target is carrying a character";
                        if(carriedCharacter.traitContainer.HasTrait("Restrained", "Unconscious") && !carriedCharacter.isDead && !carriedCharacter.crimeComponent.IsWantedBy(actor.faction)) {
                            debugLog = debugLog + ("\n-Will create Assault assumption on " + targetCharacter.name);

                            //If carried character is a prisoner, and the reactor considers that carried character as a prisoner also, do not create assumption
                            bool willCreateAssumption = true;
                            if (carriedCharacter.traitContainer.HasTrait("Prisoner")) {
                                Prisoner prisoner = carriedCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
                                if (prisoner.IsConsideredPrisonerOf(actor)) {
                                    willCreateAssumption = false;
                                }
                            }
                            if (willCreateAssumption) {
                                actor.assumptionComponent.CreateAndReactToNewAssumption(targetCharacter, carriedCharacter, INTERACTION_TYPE.ASSAULT, REACTION_STATUS.WITNESSED);
                            }
                        } else if (targetCharacter.currentJob != null && targetCharacter.currentJob.jobType == JOB_TYPE.BURY_SERIAL_KILLER_VICTIM) {
                            debugLog = debugLog + ("\n-Will create Murder assumption on " + targetCharacter.name);
                            actor.assumptionComponent.CreateAndReactToNewAssumption(targetCharacter, carriedCharacter, INTERACTION_TYPE.MURDER, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
            } else {
                debugLog = $"{debugLog}\n-Character is minion or summon or Target is currently being targeted by an action, not going to react";
            }
        }

        if(CanCharacterEatCorpseOf(actor, targetCharacter)) {
            if(targetCharacter.isDead && targetCharacter.grave == null && actor.limiterComponent.canPerform && actor.limiterComponent.canMove && actor.limiterComponent.canDoFullnessRecovery) {
                if(actor.currentActionNode == null || actor.currentActionNode.action.goapType == INTERACTION_TYPE.ROAM || !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.MONSTER_EAT_CORPSE)) {
                    actor.jobComponent.TriggerEatCorpse(targetCharacter);
                }
            }
        }

        //set owner of this component to has reacted to the target character
        targetCharacter.defaultCharacterTrait.AddCharacterThatHasReactedToThis(owner);
    }
    private bool CanCharacterEatCorpseOf(Character p_actor, Character p_target) {
        if (p_actor is Animal) {
            return false;
        }
        if (p_target is Animal) {
            return (p_actor is Summon && p_actor.faction?.factionType.type == FACTION_TYPE.Wild_Monsters) || p_actor.isConsideredRatman;
        } else {
            //If ratmen in ratman faction is starving trigger eat corpse on anyone
            return p_actor.isConsideredRatman && p_actor.needsComponent.isStarving;
        }
    }
    private void ReactTo(Character actor, TileObject targetTileObject, ref string debugLog) {
        //TODO: USE DISGUISED ACTOR AND TARGET FOR CHECKING
        if(actor is Troll) {
            if(targetTileObject is BallLightning || targetTileObject.traitContainer.HasTrait("Lightning Remnant")) {
                actor.combatComponent.Flight(targetTileObject, "saw something frightening");
            } else if(targetTileObject is WoodPile || targetTileObject is StonePile || targetTileObject is MetalPile || targetTileObject is Gold || targetTileObject is Diamond) {
                if (actor.homeStructure != null && targetTileObject.gridTileLocation.structure != actor.homeStructure && !actor.jobQueue.HasJob(JOB_TYPE.DROP_ITEM)) {
                    actor.jobComponent.CreateHoardItemJob(targetTileObject, actor.homeStructure, true);
                }
            }
        }
        if (targetTileObject is ResourcePile resourcePile && actor.homeSettlement != null) {
            //if character sees a resource pile that is outside his/her home settlement or
            //is not at his/her settlement's main storage
            if (resourcePile.gridTileLocation.IsPartOfSettlement(actor.homeSettlement) == false ||
                resourcePile.gridTileLocation.structure != actor.homeSettlement.mainStorage) {
                //do not create haul job for human and elven meat if actor is part of major faction
                if(actor.faction?.factionType.type == FACTION_TYPE.Ratmen) {
                    if(resourcePile is FoodPile) {
                        actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(resourcePile);
                    }
                } else {
                    bool cannotCreateHaulJob = (resourcePile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || resourcePile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) && actor.faction != null && actor.faction.isMajorNonPlayer;
                    if (!cannotCreateHaulJob) {
                        actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(resourcePile);
                    }
                }
            }
            if (actor.race.IsSapient() && (resourcePile.tileObjectType == TILE_OBJECT_TYPE.ELF_MEAT || resourcePile.tileObjectType == TILE_OBJECT_TYPE.HUMAN_MEAT) && resourcePile is FoodPile foodPile && 
                !actor.traitContainer.HasTrait("Cannibal") && !actor.traitContainer.HasTrait("Malnourished")) {
                if (!actor.defaultCharacterTrait.HasAlreadyReactedToFoodPile(foodPile)) {
                    actor.defaultCharacterTrait.AddFoodPileAsReactedTo(foodPile);
                    actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, foodPile, $"saw {foodPile.name}");    
                }
                actor.jobComponent.TryCreateDisposeFoodPileJob(foodPile);
            }
        }
        if(targetTileObject is FishingSpot && targetTileObject.gridTileLocation != null) {
            if(actor.race != RACE.TRITON) {
                if (GameUtilities.RollChance(0.05f)) {
                    if (actor.canBeTargetedByLandActions) {
                        if (!actor.traitContainer.HasTrait("Sturdy", "Hibernating") && !actor.HasJobTargetingThis(JOB_TYPE.TRITON_KIDNAP)) {
                            Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Triton, FactionManager.Instance.neutralFaction, homeRegion: targetTileObject.currentRegion, bypassIdeologyChecking: true);
                            summon.SetIsVolatile(true);
                            CharacterManager.Instance.PlaceSummonInitially(summon, targetTileObject.gridTileLocation);
                            (summon as Triton).TriggerTritonKidnap(actor);
                        }
                    }
                }
            }
        }
        if (targetTileObject.isDamageContributorToStructure) {
            LocationStructure structure = targetTileObject.currentStructure;
            if (structure != null && structure.structureType.IsPlayerStructure()) {
                if (actor.partyComponent.isMemberThatJoinedQuest && actor.partyComponent.currentParty.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Counterattack) {
                    actor.combatComponent.Fight(targetTileObject, CombatManager.Clear_Demonic_Intrusion);
                } else if (actor.behaviourComponent.isAttackingDemonicStructure && actor.race == RACE.ANGEL) {
                    actor.combatComponent.Fight(targetTileObject, CombatManager.Clear_Demonic_Intrusion);
                }
            }
        }

        if (!actor.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to objects
            return;
        }
        debugLog = $"{debugLog}{actor.name} is reacting to {targetTileObject.nameWithID}";
        Lazy lazy = actor.traitContainer.GetTraitOrStatus<Lazy>("Lazy");
        if (!actor.combatComponent.isInActualCombat && !actor.hasSeenFire) {
            bool hasHigherPrioJob = actor.jobQueue.jobsInQueue.Count > 0 && actor.jobQueue.jobsInQueue[0].priority > JOB_TYPE.DOUSE_FIRE.GetJobTypePriority();
            if (!hasHigherPrioJob 
                && targetTileObject.traitContainer.HasTrait("Burning")
                && targetTileObject.gridTileLocation != null
                && actor.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
                && !actor.traitContainer.HasTrait("Pyrophobic")
                && !actor.traitContainer.HasTrait("Dousing")
                && !actor.jobQueue.HasJob(JOB_TYPE.DOUSE_FIRE)) {
                debugLog = $"{debugLog}\n-Target is Burning and Character is not Pyrophobic";
                actor.SetHasSeenFire(true);
                if (lazy == null || !lazy.TryIgnoreUrgentTask(JOB_TYPE.DOUSE_FIRE)) {
                    actor.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
                    if (!actor.homeSettlement.HasJob(JOB_TYPE.DOUSE_FIRE)) {
                        Debug.LogWarning($"{actor.name} saw a fire in a settlement but no douse fire jobs were created.");
                    }

                    List<JobQueueItem> douseFireJobs = actor.homeSettlement.GetJobs(JOB_TYPE.DOUSE_FIRE)
                        .Where(j => j.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(j)).ToList();

                    if (douseFireJobs.Count > 0) {
                        actor.jobQueue.AddJobInQueue(douseFireJobs[0]);
                    } else {
                        if (actor.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                            actor.combatComponent.Flight(targetTileObject, "saw fire");
                        }
                    }    
                }
            }
        }
        if (!actor.combatComponent.isInActualCombat && !actor.hasSeenWet) {
            if (targetTileObject.traitContainer.HasTrait("Wet")
                && targetTileObject.gridTileLocation != null
                && actor.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
                && !actor.jobQueue.HasJob(JOB_TYPE.DRY_TILES)) {
                debugLog = $"{debugLog}\n-Target is Wet";
                actor.SetHasSeenWet(true);
                actor.homeSettlement.settlementJobTriggerComponent.TriggerDryTiles();
                for (int i = 0; i < actor.homeSettlement.availableJobs.Count; i++) {
                    JobQueueItem job = actor.homeSettlement.availableJobs[i];
                    if (job.jobType == JOB_TYPE.DRY_TILES) {
                        if (job.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(job)) {
                            actor.jobQueue.AddJobInQueue(job);
                        }
                    }
                }
            }
        }
        if (!actor.combatComponent.isInActualCombat && !actor.hasSeenPoisoned) {
            if (targetTileObject.traitContainer.HasTrait("Poisoned")
                && (lazy == null || !lazy.TryIgnoreUrgentTask(JOB_TYPE.CLEANSE_TILES))
                && targetTileObject.gridTileLocation != null
                && actor.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(actor.homeSettlement)
                && !actor.jobQueue.HasJob(JOB_TYPE.CLEANSE_TILES)) {
                debugLog = $"{debugLog}\n-Target is Poisoned";
                actor.SetHasSeenPoisoned(true);
                actor.homeSettlement.settlementJobTriggerComponent.TriggerCleanseTiles();
                for (int i = 0; i < actor.homeSettlement.availableJobs.Count; i++) {
                    JobQueueItem job = actor.homeSettlement.availableJobs[i];
                    if (job.jobType == JOB_TYPE.CLEANSE_TILES) {
                        if (job.assignedCharacter == null && actor.jobQueue.CanJobBeAddedToQueue(job)) {
                            actor.jobQueue.AddJobInQueue(job);
                        }
                    }
                }
            }
        }
        if (targetTileObject.traitContainer.HasTrait("Dangerous") && targetTileObject.gridTileLocation != null) {
            if (targetTileObject is Tornado || actor.currentStructure == targetTileObject.gridTileLocation.structure || (!actor.currentStructure.isInterior && !targetTileObject.gridTileLocation.structure.isInterior)) {
                if (actor.traitContainer.HasTrait("Berserked")) {
                    actor.combatComponent.FightOrFlight(targetTileObject, CombatManager.Berserked);
                } else if (actor.stateComponent.currentState == null || actor.stateComponent.currentState.characterState != CHARACTER_STATE.FOLLOW) {
                    if (actor.traitContainer.HasTrait("Suicidal")) {
                        if (!actor.jobQueue.HasJob(JOB_TYPE.SUICIDE_FOLLOW)) {
                            CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.SUICIDE_FOLLOW, CHARACTER_STATE.FOLLOW, targetTileObject, actor);
                            actor.jobQueue.AddJobInQueue(job);
                        }
                    } else if (actor.moodComponent.moodState == MOOD_STATE.Normal) {
                        string neutralizingTraitName = TraitManager.Instance.GetNeutralizingTraitFor(targetTileObject);
                        if (neutralizingTraitName != string.Empty) {
                            if (actor.traitContainer.HasTrait(neutralizingTraitName)) {
                                if (!actor.jobQueue.HasJob(JOB_TYPE.NEUTRALIZE_DANGER, targetTileObject)) {
                                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.NEUTRALIZE_DANGER,
                                        INTERACTION_TYPE.NEUTRALIZE, targetTileObject, actor);
                                    actor.jobQueue.AddJobInQueue(job);
                                }
                            } else {
                                actor.combatComponent.Flight(targetTileObject, $"saw a {targetTileObject.name}");
                            }
                        } else {
                            throw new Exception($"Trying to neutralize {targetTileObject.nameWithID} but it does not have a neutralizing trait!");
                        }
                    } else {
                        actor.combatComponent.Flight(targetTileObject, $"saw a {targetTileObject.name}");
                    }
                }
            }
        }
        //if (targetTileObject.tileObjectType.IsTileObjectAnItem()) {
        //    if (targetTileObject.gridTileLocation != null && owner.homeSettlement != null
        //        && targetTileObject.gridTileLocation.structure != owner.homeSettlement.mainStorage
        //        && !(targetTileObject.gridTileLocation.structure is Dwelling) 
        //        && !owner.IsInventoryAtFullCapacity()
        //        && (owner.jobQueue.jobsInQueue.Count == 0 || owner.jobQueue.jobsInQueue[0].priority < JOB_TYPE.TAKE_ITEM.GetJobTypePriority())) {
        //        owner.jobComponent.CreateTakeItemJob(targetTileObject);
        //    }
        //}
        if (targetTileObject.traitContainer.HasTrait("Danger Remnant", "Lightning Remnant")) {
            if (!actor.traitContainer.HasTrait("Berserked")) {
                if (targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.corruptionComponent.isCorrupted) {
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                } else {
                    if (actor.traitContainer.HasTrait("Coward")) {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                    } else {
                        int shockChance = 30;
                        if (actor.traitContainer.HasTrait("Combatant")) {
                            shockChance = 70;
                        }
                        if (UnityEngine.Random.Range(0, 100) < shockChance) {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        } else {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
                
            }
        }
        if (targetTileObject.traitContainer.HasTrait("Surprised Remnant")) {
            if (!actor.traitContainer.HasTrait("Berserked")) {
                if (targetTileObject.gridTileLocation != null && targetTileObject.gridTileLocation.corruptionComponent.isCorrupted) {
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                } else {
                    if (actor.traitContainer.HasTrait("Coward")) {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                    } else {
                        if (UnityEngine.Random.Range(0, 100) < 95) {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        } else {
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, actor, targetTileObject, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
            }
        }


        if (targetTileObject is Tombstone tombstone) {
            Character targetCharacter = tombstone.character;
            //Dead targetDeadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
            if (!targetCharacter.reactionComponent.charactersThatSawThisDead.Contains(actor)) { //targetDeadTrait != null && !targetDeadTrait.charactersThatSawThisDead.Contains(owner)
                targetCharacter.reactionComponent.AddCharacterThatSawThisDead(actor);
                debugLog = $"{debugLog}\n-Target saw dead for the first time";
                if (actor.traitContainer.HasTrait("Psychopath")) {
                    debugLog = $"{debugLog}\n-Actor is Psychopath";
                    if (targetCharacter.isNormalCharacter) {
                        debugLog = $"{debugLog}\n-Target is a normal character";
                        if (UnityEngine.Random.Range(0, 2) == 0) {
                            debugLog = $"{debugLog}\n-Target will Mock";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                        } else {
                            debugLog = $"{debugLog}\n-Target will Laugh At";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                        }
                    }
                } else {
                    debugLog = $"{debugLog}\n-Actor is not Psychopath";
                    string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                    if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        debugLog = $"{debugLog}\n-Target is Friend/Close Friend";
                        if (UnityEngine.Random.Range(0, 2) == 0) {
                            debugLog = $"{debugLog}\n-Target will Cry";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, $"saw dead {targetCharacter.name}");
                        } else {
                            debugLog = $"{debugLog}\n-Target will Puke";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, $"saw dead {targetCharacter.name}");
                        }
                    } else if ((actor.relationshipContainer.IsFamilyMember(targetCharacter) || actor.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR))
                                    && opinionLabel != RelationshipManager.Rival) {
                        debugLog = $"{debugLog}\n-Target is Relative/Lover/Affair and not Rival";
                        if (UnityEngine.Random.Range(0, 2) == 0) {
                            debugLog = $"{debugLog}\n-Target will Cry";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, $"saw dead {targetCharacter.name}");
                        } else {
                            debugLog = $"{debugLog}\n-Target will Puke";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter, $"saw dead {targetCharacter.name}");
                        }
                    } else if (opinionLabel == RelationshipManager.Enemy) {
                        debugLog = $"{debugLog}\n-Target is Enemy";
                        if (UnityEngine.Random.Range(0, 100) < 25) {
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                debugLog = $"{debugLog}\n-Target will Mock";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                            } else {
                                debugLog = $"{debugLog}\n-Target will Laugh At";
                                actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                            }
                        } else {
                            debugLog = $"{debugLog}\n-Shock";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                        }
                    } else if (opinionLabel == RelationshipManager.Rival) {
                        debugLog = $"{debugLog}\n-Target is Rival";
                        if (UnityEngine.Random.Range(0, 2) == 0) {
                            debugLog = $"{debugLog}\n-Target will Mock";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                        } else {
                            debugLog = $"{debugLog}\n-Target will Laugh At";
                            actor.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                        }
                    } else if (targetCharacter.isNormalCharacter && actor.relationshipContainer.HasRelationshipWith(targetCharacter)) {
                        debugLog = $"{debugLog}\n-Otherwise, Shock";
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Shocked, targetCharacter);
                    }
                }
            }
        }

        if (targetTileObject.IsOwnedBy(actor)
            && targetTileObject.gridTileLocation != null 
            && targetTileObject.gridTileLocation.structure != null
            && targetTileObject.gridTileLocation.structure is Dwelling
            && targetTileObject.gridTileLocation.structure != actor.homeStructure) {

            if (targetTileObject.gridTileLocation.structure.residents.Count > 0 && !targetTileObject.HasCharacterAlreadyAssumed(actor)) {
                if (actor.traitContainer.HasTrait("Suspicious")
                || actor.moodComponent.moodState == MOOD_STATE.Critical
                || (actor.moodComponent.moodState == MOOD_STATE.Bad && UnityEngine.Random.Range(0, 2) == 0)
                || UnityEngine.Random.Range(0, 100) < 15
                || TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Frame_Up)) {
                    debugLog = $"{debugLog}\n-Owner is Suspicious or Critical Mood or Low Mood";

                    debugLog = $"{debugLog}\n-There is at least 1 resident of the structure";
                    _assumptionSuspects.Clear();
                    for (int i = 0; i < targetTileObject.gridTileLocation.structure.residents.Count; i++) {
                        Character resident = targetTileObject.gridTileLocation.structure.residents[i];
                        AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(resident);
                        if (awarenessState == AWARENESS_STATE.Available) {
                            _assumptionSuspects.Add(resident);
                        } else if (awarenessState == AWARENESS_STATE.None) {
                            if (!resident.isDead) {
                                _assumptionSuspects.Add(resident);
                            }
                        }
                    }
                    if(_assumptionSuspects.Count > 0) {
                        Character chosenSuspect = _assumptionSuspects[UnityEngine.Random.Range(0, _assumptionSuspects.Count)];
                        debugLog = debugLog + ("\n-Will create Steal assumption on " + chosenSuspect.name);
                        actor.assumptionComponent.CreateAndReactToNewAssumption(chosenSuspect, targetTileObject, INTERACTION_TYPE.STEAL, REACTION_STATUS.WITNESSED);
                        actor.jobComponent.CreateDropItemJob(JOB_TYPE.RETURN_STOLEN_THING, targetTileObject, actor.homeStructure);
                    }
                } else {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "no_steal_assumption", providedTags: LOG_TAG.Crimes);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(targetTileObject, targetTileObject.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(targetTileObject.gridTileLocation.structure,  targetTileObject.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
                    log.AddLogToDatabase(true);
                }
            }
            if(targetTileObject.tileObjectType.IsTileObjectAnItem() && !actor.jobQueue.HasJob(JOB_TYPE.TAKE_ITEM, targetTileObject) && targetTileObject.Advertises(INTERACTION_TYPE.PICK_UP) && actor.limiterComponent.canMove) {
                //NOTE: Added checker if character can move, so that Paralyzed characters will not try to pick up items
                actor.jobComponent.CreateTakeItemJob(JOB_TYPE.TAKE_ITEM, targetTileObject);
            }
        }

        if (targetTileObject is CultistKit && !targetTileObject.IsOwnedBy(actor)) {
            debugLog = $"{debugLog}\n-Object is a cultist kit";
            if (targetTileObject.gridTileLocation != null) {
                if (targetTileObject.structureLocation is ManMadeStructure && 
                    targetTileObject.structureLocation.GetNumberOfResidentsExcluding(out var validResidents,actor) > 0) {
                    debugLog = $"{debugLog}\n-Cultist kit is at structure with residents excluding the witness";
                    int chanceToCreateAssumption = 0;
                    if (actor.traitContainer.HasTrait("Suspicious") || actor.moodComponent.moodState == MOOD_STATE.Critical) {
                        chanceToCreateAssumption = 100;
                    } else if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
                        chanceToCreateAssumption = 50;
                    } else {
                        chanceToCreateAssumption = 15;
                    }
                    debugLog = $"{debugLog}\n-Rolling for chance to create assumption";
                    if (GameUtilities.RollChance(chanceToCreateAssumption, ref debugLog)) {
                        _assumptionSuspects.Clear();
                        if(validResidents != null) {
                            for (int i = 0; i < validResidents.Count; i++) {
                                Character resident = validResidents[i];
                                AWARENESS_STATE awarenessState = actor.relationshipContainer.GetAwarenessState(resident);
                                if (awarenessState == AWARENESS_STATE.Available) {
                                    _assumptionSuspects.Add(resident);
                                } else if (awarenessState == AWARENESS_STATE.None) {
                                    if (!resident.isDead) {
                                        _assumptionSuspects.Add(resident);
                                    }
                                }
                            }
                        }
                        Character chosenTarget = CollectionUtilities.GetRandomElement(_assumptionSuspects);
                        if(chosenTarget != null && CrimeManager.Instance.IsConsideredACrimeByCharacter(actor, chosenTarget, targetTileObject, CRIME_TYPE.Demon_Worship)) {
                            actor.assumptionComponent.CreateAndReactToNewAssumption(chosenTarget, targetTileObject, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
            } 
        }

        if (targetTileObject.traitContainer.HasTrait("Interesting")) {
            Interesting interestingTrait = targetTileObject.traitContainer.GetTraitOrStatus<Interesting>("Interesting");
            if (!interestingTrait.HasAlreadyBeenSeenByCharacter(actor)) {
                interestingTrait.AddCharacterThatSaw(actor);

                if (actor.traitContainer.HasTrait("Suspicious")) {
                    if (GameUtilities.RollChance(50)) {
                        actor.jobComponent.TriggerDestroy(targetTileObject);
                    } else {
                        actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, targetTileObject);
                    }
                } else {
                    if (GameUtilities.RollChance(50) && !actor.jobQueue.HasJob(JOB_TYPE.INSPECT, targetTileObject) && !actor.defaultCharacterTrait.HasAlreadyInspectedObject(targetTileObject)) {
                        actor.jobComponent.TriggerInspect(targetTileObject);
                    } else if (!actor.IsInventoryAtFullCapacity() && !actor.HasItem(targetTileObject.name) && !actor.HasOwnedItemInHomeStructure(targetTileObject.name)) {
                        actor.jobComponent.CreateTakeItemJob(JOB_TYPE.TAKE_ITEM, targetTileObject);
                    }
                }
            }
        }
    }
    private void ReactToCarriedObject(Character actor, TileObject targetTileObject, Character carrier, ref string debugLog) {
        debugLog = $"{debugLog}{actor.name} is reacting to {targetTileObject.nameWithID} carried by {carrier.name}";
        if (targetTileObject is CultistKit) {
            debugLog = $"{debugLog}Object is cultist kit, creating assumption...";
            Character disguisedTarget = carrier;
            if (carrier.reactionComponent.disguisedCharacter != null) {
                disguisedTarget = carrier.reactionComponent.disguisedCharacter;
            }
            if (!disguisedTarget.isDead && CrimeManager.Instance.IsConsideredACrimeByCharacter(actor, disguisedTarget, targetTileObject, CRIME_TYPE.Demon_Worship)) {
                actor.assumptionComponent.CreateAndReactToNewAssumption(disguisedTarget, targetTileObject, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED);
            }
        }
    }
    //The reason why we pass the character that was hit instead of just getting the current closest hostile in combat state is because 
    public void ReactToCombat(CombatState combat, IPointOfInterest poiHit) {
        Character attacker = combat.stateComponent.owner;
        Character reactor = owner;
        if (reactor.combatComponent.isInCombat) {
            string inCombatLog = $"{reactor.name} is in combat and reacting to combat of {attacker.name} against {poiHit.nameWithID}";
            if (reactor == poiHit) {
                inCombatLog = $"{inCombatLog}\n-Reactor is the Hit Character";
                CombatState reactorCombat = reactor.stateComponent.currentState as CombatState;
                if (reactorCombat.isAttacking && reactorCombat.currentClosestHostile != null && reactorCombat.currentClosestHostile != attacker) {
                    inCombatLog = $"{inCombatLog}\n-Reactor is currently attacking another character";
                    if (reactorCombat.currentClosestHostile is Character currentPursuingCharacter) {
                        if (currentPursuingCharacter.combatComponent.isInCombat && (currentPursuingCharacter.stateComponent.currentState as CombatState).isAttacking == false) {
                            inCombatLog = $"{inCombatLog}\n-Character that is being attacked by reactor is currently fleeing";
                            inCombatLog = $"{inCombatLog}\n-Reactor will determine combat reaction";
                            reactor.combatComponent.SetWillProcessCombat(true);
                            //if (reactor.combatComponent.IsHostileInRange(attacker) || reactor.combatComponent.IsAvoidInRange(attacker)) {
                            //log = log +"\n-Attacker of reactor is in hostile/avoid list of the reactor, rector will determine combat reaction";
                            //}
                        }
                    }
                }
            }
            reactor.logComponent.PrintLogIfActive(inCombatLog);
            return;
        }
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to objects
            return;
        }
        if(owner.isDead || !owner.limiterComponent.canPerform) {
            return;
        }
        string log = $"{reactor.name} is reacting to combat of {attacker.name} against {poiHit.nameWithID}";
        if (reactor.IsHostileWith(attacker)) {
            log = $"{log}\n-Hostile with attacker, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        if (combat.DidCharacterAlreadyReactToThisCombat(reactor)) {
            log = $"{log}\n-Already reacted to the combat, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        if (poiHit is Character targetHit && reactor.IsHostileWith(targetHit)) {
            log = $"{log}\n-Reactor is hostile with the hit character, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        combat.AddCharacterThatReactedToThisCombat(reactor);
        if(poiHit is Character characterHit) {
            if (combat.currentClosestHostile != characterHit) {
                log = $"{log}\n-Hit Character is not the same as the actual target which is: {combat.currentClosestHostile?.name}";
                if (characterHit.combatComponent.isInCombat) {
                    log = $"{log}\n-Hit Character is in combat";
                    log = $"{log}\n-Do nothing";
                } else {
                    log = $"{log}\n-Reactor felt Shocked";
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED, reason: "accidentally attacked");
                }
            } else {
                CombatData combatDataAgainstCharacterHit = attacker.combatComponent.GetCombatData(characterHit);
                if (combatDataAgainstCharacterHit != null && combatDataAgainstCharacterHit.connectedAction != null && combatDataAgainstCharacterHit.connectedAction.associatedJobType == JOB_TYPE.APPREHEND) {
                    log = $"{log}\n-Combat is part of Apprehend Job";
                    log = $"{log}\n-Reactor felt Shocked";
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED, reason: "being apprehended");
                } else {
                    if (characterHit == reactor) {
                        log = $"{log}\n-Hit Character is the Reactor";
                        if (characterHit.relationshipContainer.IsFriendsWith(attacker)) {
                            log = $"{log}\n-Hit Character is Friends/Close Friends with Attacker";
                            log = $"{log}\n-Reactor felt Betrayal";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, reactor, attacker, REACTION_STATUS.WITNESSED);
                        } else if (characterHit.relationshipContainer.IsEnemiesWith(attacker)) {
                            log = $"{log}\n-Hit Character is Enemies/Rivals with Attacker";
                            log = $"{log}\n-Reactor felt Anger";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, reactor, attacker, REACTION_STATUS.WITNESSED);
                        }
                    } else {
                        log = $"{log}\n-Hit Character is NOT the Reactor";
                        if (reactor.relationshipContainer.IsFriendsWith(characterHit)) {
                            log = $"{log}\n-Reactor is Friends/Close Friends with Hit Character";
                            if (reactor.relationshipContainer.IsFriendsWith(attacker)) {
                                log = $"{log}\n-Reactor is Friends/Close Friends with Attacker";
                                log = $"{log}\n-Reactor felt Shock, Disappointment";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED, reason: "attacked by a friend");
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else if (reactor.relationshipContainer.IsEnemiesWith(attacker)) {
                                log = $"{log}\n-Reactor is Enemies/Rivals with Attacker";
                                log = $"{log}\n-Reactor felt Rage";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else {
                                log = $"{log}\n-Reactor felt Anger";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, reactor, attacker, REACTION_STATUS.WITNESSED);
                            }
                        } else if (reactor.relationshipContainer.IsEnemiesWith(characterHit)) {
                            log = $"{log}\n-Reactor is Enemies/Rivals with Hit Character";
                            if (reactor.relationshipContainer.IsFriendsWith(attacker)) {
                                log = $"{log}\n-Reactor is Friends/Close Friends with Attacker";
                                log = $"{log}\n-Reactor felt Approval";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else if (reactor.relationshipContainer.IsEnemiesWith(attacker)) {
                                log = $"{log}\n-Reactor is Enemies/Rivals with Attacker";
                                log = $"{log}\n-Reactor felt Shock";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED, reason: "sudden attack");
                            } else {
                                log = $"{log}\n-Reactor felt Approval";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, reactor, attacker, REACTION_STATUS.WITNESSED);
                            }
                        } else {
                            log = $"{log}\n-Reactor felt Shock";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED, reason: "sudden attack");
                        }
                    }
                }
            }
            //Check for crime
            if ((reactor.faction != null && reactor.faction == attacker.faction) || (reactor.homeSettlement != null && reactor.homeSettlement == attacker.homeSettlement)) {
                log = $"{log}\n-Reactor is the same faction/home settlement as Attacker";
                log = $"{log}\n-Reactor is checking for crime";
                CombatData combatDataAgainstPOIHit = attacker.combatComponent.GetCombatData(characterHit);
                if (combatDataAgainstPOIHit != null) {
                    if (combatDataAgainstPOIHit.connectedAction != null) {
                        ActualGoapNode possibleCrimeAction = combatDataAgainstPOIHit.connectedAction;
                        CRIME_TYPE crimeType = possibleCrimeAction.crimeType;
                        log = $"{log}\n-Crime committed is: {crimeType}";
                        if (crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset) {
                            log = $"{log}\n-Reactor will react to crime";
                            CrimeManager.Instance.ReactToCrime(reactor, attacker, characterHit, characterHit.faction, crimeType, possibleCrimeAction, REACTION_STATUS.WITNESSED);
                        }
                    } else {
                        //create assault action so that witness can react to it as a crime.
                        ActualGoapNode action = new ActualGoapNode(InteractionManager.Instance.goapActionData[INTERACTION_TYPE.ASSAULT], attacker, characterHit, null, 0);
                        action.SetAsIllusion();
                        action.SetCrimeType();
                        CrimeManager.Instance.ReactToCrime(reactor, attacker, characterHit, characterHit.faction, action.crimeType, action, REACTION_STATUS.WITNESSED);
                    }
                }
            }

        } else if (poiHit is TileObject objectHit) {
            if (!objectHit.IsOwnedBy(attacker)) {
                //CrimeManager.Instance.ReactToCrime()
                log = $"{log}\n-Object Hit is not owned by the Attacker";
                log = $"{log}\n-Reactor is checking for crime";
                CombatData combatDataAgainstPOIHit = attacker.combatComponent.GetCombatData(objectHit);
                if (combatDataAgainstPOIHit != null && combatDataAgainstPOIHit.connectedAction != null) {
                    ActualGoapNode possibleCrimeAction = combatDataAgainstPOIHit.connectedAction;
                    CRIME_TYPE crimeType = possibleCrimeAction.crimeType;
                    log = $"{log}\n-Crime committed is: {crimeType}";
                    if (crimeType != CRIME_TYPE.None && crimeType != CRIME_TYPE.Unset) {
                        log = $"{log}\n-Reactor will react to crime";
                        Faction targetFaction = objectHit.factionOwner;
                        if(targetFaction == null) {
                            BaseSettlement settlement = null;
                            if(objectHit.gridTileLocation != null && objectHit.gridTileLocation.IsPartOfSettlement(out settlement)) {
                                targetFaction = settlement.owner;
                            }
                        }
                        if (targetFaction == null) {
                            targetFaction = reactor.faction;
                        }
                        CrimeManager.Instance.ReactToCrime(reactor, attacker, objectHit, targetFaction, crimeType, possibleCrimeAction, REACTION_STATUS.WITNESSED);
                    }
                }
            }
        }

        reactor.logComponent.PrintLogIfActive(log);
    }
    #endregion

    #region Reaction To Intel
    public string ReactToIntel(IIntel intel) {
        string response = owner.reactionComponent.ReactTo(intel.reactable, REACTION_STATUS.INFORMED);
        if ((string.IsNullOrEmpty(response) || string.IsNullOrWhiteSpace(response)) && intel.actor != owner) {
            ActualGoapNode action = null;
            if (intel is ActionIntel actionIntel) {
                action = actionIntel.node;
            }
            response = CharacterManager.Instance.TriggerEmotion(EMOTION.Disinterest, owner, intel.actor, REACTION_STATUS.INFORMED, action);
        }
        return response;
    }
    #endregion

    #region General
    public void AddCharacterThatSawThisDead(Character character) {
        charactersThatSawThisDead.Add(character);
    }
    public void SetIsHidden(bool state) {
        if(isHidden != state) {
            isHidden = state;
            owner.OnSetIsHidden();
            UpdateHiddenState();
            if (!isHidden) {
                //If character comes out from being hidden, all characters in vision should process this character
                if (owner.marker) {
                    for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                        Character inVision = owner.marker.inVisionCharacters[i];
                        inVision.marker.AddUnprocessedPOI(owner);
                    }
                }
            }
        }
    }
    public void UpdateHiddenState() {
        if (owner.marker) {
            if (isHidden) {
                owner.marker.SetVisualAlpha(0.5f);
            } else {
                owner.marker.SetVisualAlpha(1f);
            }
        }
    }
    public void SetDisguisedCharacter(Character character) {
        if(disguisedCharacter != character) {
            disguisedCharacter = character;
            if (disguisedCharacter != null) {
                owner.visuals.UpdateAllVisuals(owner);
                Messenger.Broadcast(CharacterSignals.CHARACTER_DISGUISED, owner, character);
            } else {
                owner.visuals.UpdateAllVisuals(owner);
                if (!owner.isDead && owner.marker) {
                    for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                        Character inVisionCharacter = owner.marker.inVisionCharacters[i];
                        if (!inVisionCharacter.isDead && inVisionCharacter.marker) {
                            inVisionCharacter.marker.AddUnprocessedPOI(owner);
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataReactionComponent data) {
        for (int i = 0; i < data.charactersThatSawThisDead.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.charactersThatSawThisDead[i]);
            if (character != null) {
                charactersThatSawThisDead.Add(character);
            }
        }
        if (!string.IsNullOrEmpty(data.disguisedCharacter)) {
            disguisedCharacter = CharacterManager.Instance.GetCharacterByPersistentID(data.disguisedCharacter);
        }
    }
    #endregion

}

[System.Serializable]
public class SaveDataReactionComponent : SaveData<ReactionComponent> {
    public List<string> charactersThatSawThisDead;
    public string disguisedCharacter;
    public bool isHidden;

    #region Overrides
    public override void Save(ReactionComponent data) {
        charactersThatSawThisDead = new List<string>();
        for (int i = 0; i < data.charactersThatSawThisDead.Count; i++) {
            charactersThatSawThisDead.Add(data.charactersThatSawThisDead[i].persistentID);
        }
        if(data.disguisedCharacter != null) {
            disguisedCharacter = data.disguisedCharacter.persistentID;
        }
        isHidden = data.isHidden;
    }

    public override ReactionComponent Load() {
        ReactionComponent component = new ReactionComponent(this);
        return component;
    }
    #endregion
}