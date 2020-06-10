using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Traits;
using Inner_Maps;
using Interrupts;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class ReactionComponent {
    public Character owner { get; private set; }

    private List<Character> _assumptionSuspects;

    public ReactionComponent(Character owner) {
        this.owner = owner;
        _assumptionSuspects = new List<Character>();
    }

    #region Processes
    public void ReactTo(IPointOfInterest targetTileObject, ref string debugLog) {
        if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            ReactTo(targetTileObject as Character, ref debugLog);
        } else if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            ReactTo(targetTileObject as TileObject, ref debugLog);
        } 
        // else if (targetTileObject.poiType == POINT_OF_INTEREST_TYPE.ITEM) {
        //     ReactTo(targetTileObject as SpecialToken, ref debugLog);
        // }
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to its own traits
            return;
        }
        if (owner.isInCombat) {
            return;
        }
        debugLog += "\n-Character will loop through all his/her traits to react to Target";
        List<Trait> traitOverrideFunctions = owner.traitContainer.GetTraitOverrideFunctions(TraitManager.See_Poi_Trait);
        if (traitOverrideFunctions != null) {
            for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                Trait trait = traitOverrideFunctions[i];
                debugLog += $"\n - {trait.name}";
                if (trait.OnSeePOI(targetTileObject, owner)) {
                    debugLog += ": triggered";
                } else {
                    debugLog += ": not triggered";
                }
            }
        }
        //for (int i = 0; i < owner.traitContainer.allTraitsAndStatuses.Count; i++) {
        //    debugLog += $"\n - {owner.traitContainer.allTraitsAndStatuses[i].name}";
        //    if (owner.traitContainer.allTraitsAndStatuses[i].OnSeePOI(targetTileObject, owner)) {
        //        debugLog += ": triggered";
        //    } else {
        //        debugLog += ": not triggered";
        //    }
        //}
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
        if (status == REACTION_STATUS.WITNESSED) {
            ReactToWitnessedReactable(reactable, addLog);
        } else {
            return ReactToInformedReactable(reactable, addLog);
        }
        return string.Empty;
    }
    private void ReactToWitnessedReactable(IReactable reactable, bool addLog) {
        if (owner.isInCombat) {
            return;
        }
        if (owner.faction != reactable.actor.faction && owner.faction.IsHostileWith(reactable.actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return;
        }
        //if (witnessedEvent.currentStateName == null) {
        //    throw new System.Exception(GameManager.Instance.TodayLogString() + this.name + " witnessed event " + witnessedEvent.action.goapName + " by " + witnessedEvent.actor.name + " but it does not have a current state!");
        //}
        //if (string.IsNullOrEmpty(reactable.currentStateName)) {
        //    return;
        //}
        if (reactable.informationLog == null) {
            throw new Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} witnessed event {reactable.name} by {reactable.actor.name} does not have a log!");
        }
        IPointOfInterest target = reactable.target;
        if(reactable.target is TileObject item && reactable is ActualGoapNode node) {
            if (node.action.goapType == INTERACTION_TYPE.STEAL) {
                if (item.isBeingCarriedBy != null) {
                    target = item.isBeingCarriedBy;
                }
            }
        }
        if(reactable.actor != owner && target != owner) {
            if (addLog) {
                //Only log witness event if event is not an action. If it is an action, the CharacterManager.Instance.CanAddCharacterLogOrShowNotif must return true
                if (reactable is ActualGoapNode action && (!action.action.shouldAddLogs || !CharacterManager.Instance.CanAddCharacterLogOrShowNotif(action.goapType))) {
                    //Should not add witness log if the action log itself is not added to the actor
                } else {
                    Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event", reactable as ActualGoapNode);
                    witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
                    witnessLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(reactable.informationLog), LOG_IDENTIFIER.APPEND);
                    witnessLog.AddToFillers(reactable.informationLog.fillers);
                    owner.logComponent.AddHistory(witnessLog);
                }
            }
            string emotionsToActor = reactable.ReactionToActor(owner, REACTION_STATUS.WITNESSED);
            if(emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {reactable.actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(reactable.actor, reactable.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            string emotionsToTarget = reactable.ReactionToTarget(owner, REACTION_STATUS.WITNESSED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {reactable.actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(reactable.target, reactable.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.Comafy(emotionsToTarget), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            string response =
                $"Witness action reaction of {owner.name} to {reactable.name} of {reactable.actor.name} with target {reactable.target.name}: {emotionsToActor}{emotionsToTarget}";
            owner.logComponent.PrintLogIfActive(response);
        } else if (target == owner) {
            if (!reactable.isStealth || target.traitContainer.HasTrait("Vigilant")) {
                string emotionsOfTarget = reactable.ReactionOfTarget(REACTION_STATUS.WITNESSED);
                if (emotionsOfTarget != string.Empty) {
                    if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                        string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                        error += $"\n-Witness: {owner}";
                        error += $"\n-Action: {reactable.name}";
                        error += $"\n-Actor: {reactable.actor.name}";
                        error += $"\n-Target: {reactable.target.nameWithID}";
                        owner.logComponent.PrintLogErrorIfActive(error);
                    } else {
                        //add log of emotions felt
                        Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(reactable.actor, reactable.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        log.AddToFillers(null, UtilityScripts.Utilities.Comafy(emotionsOfTarget), LOG_IDENTIFIER.STRING_1);
                        log.AddLogToInvolvedObjects();
                    }
                }
                string response =
                    $"Witness action reaction of {owner.name} to {reactable.name} of {reactable.actor.name} with target {reactable.target.name}: {emotionsOfTarget}";
                owner.logComponent.PrintLogIfActive(response);
            }
        }

        //CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(node);
        //if (crimeType != CRIME_TYPE.NONE) {
        //    CrimeManager.Instance.ReactToCrime(owner, node, node.associatedJobType, crimeType);
        //}
    }
    private string ReactToInformedReactable(IReactable reactable, bool addLog) {
        if (reactable.informationLog == null) {
            throw new Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} informed event {reactable.name} by {reactable.actor.name} does not have a log!");
        }
        if (addLog) {
            Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event", reactable as ActualGoapNode);
            informedLog.AddToFillers(reactable.informationLog.fillers);
            informedLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
            informedLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(reactable.informationLog), LOG_IDENTIFIER.APPEND);
            owner.logComponent.AddHistory(informedLog);
        }

        string response = string.Empty;
        if (reactable.actor != owner && reactable.target != owner) {
            string emotionsToActor = reactable.ReactionToActor(owner, REACTION_STATUS.INFORMED);
            if (emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Action Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {reactable.actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(reactable.actor, reactable.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            string emotionsToTarget = reactable.ReactionToTarget(owner, REACTION_STATUS.INFORMED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Action Error in Witness Reaction To Target (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {reactable.actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(reactable.target, reactable.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToTarget, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                }
            }
            response += $"{emotionsToActor}/{emotionsToTarget}";
        } else if(reactable.target == owner && reactable.target is Character) {
            string emotionsOfTarget = reactable.ReactionOfTarget(REACTION_STATUS.INFORMED);
            if (emotionsOfTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                    string error = "Action Error in Witness Reaction Of Target (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Action: {reactable.name}";
                    error += $"\n-Actor: {reactable.actor.name}";
                    error += $"\n-Target: {reactable.target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    //add log of emotions felt
                    Log log = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(reactable.actor, reactable.actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsOfTarget, 2), LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
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
    public string ReactTo(Interrupt interrupt, Character actor, IPointOfInterest target, Log log, REACTION_STATUS status) {
        if (owner.isInCombat) {
            return string.Empty;
        }
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to interrupts
            return string.Empty;
        }
        if (owner.faction != actor.faction && owner.faction.IsHostileWith(actor.faction)) {
            //Must not react if the faction of the actor of witnessed action is hostile with the faction of the witness
            return string.Empty;
        }
        if(status == REACTION_STATUS.WITNESSED) {
            ReactToWitnessedInterrupt(interrupt, actor, target, log);
        } else if (status == REACTION_STATUS.INFORMED) {
            return ReactToInformedInterrupt(interrupt, actor, target, log);
        }
        return string.Empty;
    }
    private void ReactToWitnessedInterrupt(Interrupt interrupt, Character actor, IPointOfInterest target, Log log) {
        if (actor != owner && target != owner) {
            if (actor.interruptComponent.currentInterrupt == interrupt && log != null) {
                Log witnessLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "witness_event");
                witnessLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
                witnessLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(log), LOG_IDENTIFIER.APPEND);
                witnessLog.AddToFillers(log.fillers);
                owner.logComponent.AddHistory(witnessLog);
            }
            string emotionsToActor = interrupt.ReactionToActor(owner, actor, target, interrupt, REACTION_STATUS.WITNESSED);
            if (emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Interrupt: {interrupt.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                    emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    emotionsLog.AddLogToInvolvedObjects();
                }
            }
            string emotionsToTarget = interrupt.ReactionToTarget(owner, actor, target, interrupt, REACTION_STATUS.WITNESSED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Interrupt: {interrupt.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                    emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    emotionsLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToTarget, 2), LOG_IDENTIFIER.STRING_1);
                    emotionsLog.AddLogToInvolvedObjects();
                }
            }
            string response =
                $"Witness interrupt reaction of {owner.name} to {interrupt.name} of {actor.name} with target {target.name}: {emotionsToActor}{emotionsToTarget}";
            owner.logComponent.PrintLogIfActive(response);
        } else if (target == owner) {
            string emotionsOfTarget = interrupt.ReactionOfTarget(actor, target, interrupt, REACTION_STATUS.WITNESSED);
            if (emotionsOfTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                    string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Interrupt: {interrupt.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction_witness");
                    emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsOfTarget, 2), LOG_IDENTIFIER.STRING_1);
                    emotionsLog.AddLogToInvolvedObjects();
                }
            }
            string response =
                $"Witness interrupt reaction of {owner.name} to {interrupt.name} of {actor.name} with target {target.name}: {emotionsOfTarget}";
            owner.logComponent.PrintLogIfActive(response);
        }
    }
    private string ReactToInformedInterrupt(Interrupt interrupt, Character actor, IPointOfInterest target, Log log) {
        if (log == null) {
            throw new Exception(
                $"{GameManager.Instance.TodayLogString()}{owner.name} informed interrupt {interrupt.name} by {actor.name} with target {target.name} but it does not have a log!");
        }
        Log informedLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "informed_event");
        informedLog.AddToFillers(log.fillers);
        informedLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
        informedLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(log), LOG_IDENTIFIER.APPEND);
        owner.logComponent.AddHistory(informedLog);

        string response = string.Empty;
        if (actor != owner && target != owner) {
            string emotionsToActor = interrupt.ReactionToActor(owner, actor, target, interrupt, REACTION_STATUS.INFORMED);
            if (emotionsToActor != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToActor)) {
                    string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Interrupt: {interrupt.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToActor, 2), LOG_IDENTIFIER.STRING_1);
                    emotionsLog.AddLogToInvolvedObjects();
                }
            }
            string emotionsToTarget = interrupt.ReactionToTarget(owner, actor, target, interrupt, REACTION_STATUS.INFORMED);
            if (emotionsToTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsToTarget)) {
                    string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Interrupt: {interrupt.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    emotionsLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsToTarget, 2), LOG_IDENTIFIER.STRING_1);
                    emotionsLog.AddLogToInvolvedObjects();
                }
            }
            response += $"{emotionsToActor}/{emotionsToTarget}";
            owner.logComponent.PrintLogIfActive(response);
        } else if (target == owner) {
            string emotionsOfTarget = interrupt.ReactionOfTarget(actor, target, interrupt, REACTION_STATUS.INFORMED);
            if (emotionsOfTarget != string.Empty) {
                if (!CharacterManager.Instance.EmotionsChecker(emotionsOfTarget)) {
                    string error = "Interrupt Error in Witness Reaction To Actor (Duplicate/Incompatible Emotions Triggered)";
                    error += $"\n-Witness: {owner}";
                    error += $"\n-Interrupt: {interrupt.name}";
                    error += $"\n-Actor: {actor.name}";
                    error += $"\n-Target: {target.nameWithID}";
                    owner.logComponent.PrintLogErrorIfActive(error);
                } else {
                    Log emotionsLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "emotions_reaction");
                    emotionsLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    emotionsLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    emotionsLog.AddToFillers(null, UtilityScripts.Utilities.GetFirstFewEmotionsAndComafy(emotionsOfTarget, 2), LOG_IDENTIFIER.STRING_1);
                    emotionsLog.AddLogToInvolvedObjects();
                }
            }
            response = emotionsOfTarget;
            owner.logComponent.PrintLogIfActive(response);
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

    private void ReactTo(Character targetCharacter, ref string debugLog) {
        debugLog += $"{owner.name} is reacting to {targetCharacter.name}";
        if(owner.faction.IsHostileWith(targetCharacter.faction)) {
            debugLog += "\n-Target is hostile";
            if (!targetCharacter.isDead && targetCharacter.combatComponent.combatMode != COMBAT_MODE.Passive) {
                debugLog += "\n-Target is not dead";
                debugLog += "\n-Fight or Flight response";
                //Fight or Flight
                if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                    //If the source is harassing or defending, combat should not be lethal
                    //There is a special case, even if the source is defending if he/she is a demon and the target is an angel and vice versa, make the combat lethal
                    bool isLethal = (!owner.behaviourComponent.isHarassing && !owner.behaviourComponent.isDefending)
                        || ((owner.race == RACE.DEMON && targetCharacter.race == RACE.ANGEL) || (owner.race == RACE.ANGEL && targetCharacter.race == RACE.DEMON));
                    bool isTopPrioJobLethal = owner.jobQueue.jobsInQueue.Count <= 0 || owner.jobQueue.jobsInQueue[0].jobType.IsJobLethal();
                    if (owner.jobQueue.jobsInQueue.Count > 0) {
                        debugLog += $"\n-{owner.jobQueue.jobsInQueue[0].jobType}";
                    }
                    //If the target is already unconscious (it cannot fight back), attack it again only if the source is not harassing and not defending and the top priority job is considered lethal
                    if (!targetCharacter.traitContainer.HasTrait("Unconscious") || (isLethal && isTopPrioJobLethal)) {
                        owner.combatComponent.FightOrFlight(targetCharacter, CombatManager.Hostility, isLethal: isLethal);
                    }
                }
            } else {
                debugLog += "\n-Target is dead or is passive";
                debugLog += "\n-Do nothing";
            }
        } else if (!owner.isInCombat) {
            debugLog += "\n-Target is not hostile and Character is not in combat";
            if (owner.isNormalCharacter && !IsPOICurrentlyTargetedByAPerformingAction(targetCharacter)) {
                debugLog += "\n-Character is a villager and Target is not being targeted by an action, continue reaction";
                if (!targetCharacter.isDead) {
                    debugLog += "\n-Target is not dead";
                    if (!owner.isConversing && !targetCharacter.isConversing && owner.nonActionEventsComponent.CanInteract(targetCharacter) 
                        //only allow chat if characters current action is not have affair or if his action is have affair but the character he is reacting to is not the target of that action.
                        && (owner.currentActionNode == null || (owner.currentActionNode.action.goapType != INTERACTION_TYPE.HAVE_AFFAIR || owner.currentActionNode.poiTarget != targetCharacter))) {
                        debugLog += "\n-Character and Target are not Chatting or Flirting and Character can interact with Target, has 3% chance to Chat";
                        int chance = UnityEngine.Random.Range(0, 100);
                        debugLog += $"\n-Roll: {chance.ToString()}";
                        if (chance < 3) {
                            debugLog += "\n-Chat triggered";
                            owner.interruptComponent.TriggerInterrupt(INTERRUPT.Chat, targetCharacter);
                        } else {
                            debugLog += "\n-Chat did not trigger, will now trigger Flirt if Character is Sexually Compatible with Target and Character is Unfaithful, or Target is Lover or Affair, or Character has no Lover";
                            if (RelationshipManager.IsSexuallyCompatibleOneSided(owner.sexuality, targetCharacter.sexuality, owner.gender, targetCharacter.gender)
                                && owner.relationshipContainer.IsFamilyMember(targetCharacter) == false) {
                                if (owner.relationshipContainer.HasRelationshipWith(targetCharacter, RELATIONSHIP_TYPE.LOVER, RELATIONSHIP_TYPE.AFFAIR)
                                    || owner.relationshipContainer.GetFirstRelatableIDWithRelationship(RELATIONSHIP_TYPE.LOVER) == -1
                                    || owner.traitContainer.HasTrait("Unfaithful")) {
                                    debugLog += "\n-Flirt has 1% (multiplied by Compatibility value) chance to trigger";
                                    int compatibility = RelationshipManager.Instance.GetCompatibilityBetween(owner, targetCharacter);
                                    int value = 2;
                                    if (compatibility != -1) {
                                        value = 1 * compatibility;
                                        debugLog += $"\n-Chance: {value}";
                                    } else {
                                        debugLog += $"\n-Chance: {value} (No Compatibility)";
                                    }
                                    int flirtChance = UnityEngine.Random.Range(0, 100);
                                    debugLog += $"\n-Roll: {flirtChance.ToString()}";
                                    if (flirtChance < value) {
                                        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Flirt, targetCharacter);
                                    } else {
                                        debugLog += "\n-Flirt did not trigger";
                                    }
                                } else {
                                    debugLog += "\n-Flirt did not trigger";
                                }
                            }
                        }
                    }

                    if (owner.faction == targetCharacter.faction || owner.homeSettlement == targetCharacter.homeSettlement) {
                        debugLog += "\n-Character and Target are with the same faction or npcSettlement";
                        if (owner.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                            debugLog += "\n-Character considers Target as Enemy or Rival";
                            if ((!targetCharacter.canMove || !targetCharacter.canPerform) && owner.moodComponent.moodState != MOOD_STATE.NORMAL) {
                                debugLog += "\n-Target can neither move or perform, will trigger Mock or Laugh At interrupt";
                                if (UnityEngine.Random.Range(0, 2) == 0) {
                                    debugLog += "\n-Character triggered Mock interrupt";
                                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                                } else {
                                    debugLog += "\n-Character triggered Laugh At interrupt";
                                    owner.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                                }
                            }
                        } else if (!owner.traitContainer.HasTrait("Psychopath")) {
                            debugLog += "\n-Character is not Psychopath and does not consider Target as Enemy or Rival";
                            if (!targetCharacter.canMove/* || !targetCharacter.canWitness*/) {
                                debugLog += "\n-Target cannot move"; // or cannot witness
                                if (targetCharacter.needsComponent.isHungry || targetCharacter.needsComponent.isStarving) {
                                    debugLog += "\n-Target is hungry or starving, will create feed job";
                                    owner.jobComponent.TryTriggerFeed(targetCharacter);
                                } else if (targetCharacter.needsComponent.isTired || targetCharacter.needsComponent.isExhausted) {
                                    debugLog += "\n-Target is tired or exhausted, will create Move Character job to bed if Target has a home and an available bed";
                                    if (targetCharacter.homeStructure != null) {
                                        Bed bed = targetCharacter.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED) as Bed;
                                        if (bed != null && bed.gridTileLocation != targetCharacter.gridTileLocation) {
                                            debugLog += "\n-Target has a home and an available bed, will trigger Move Character job to bed";
                                            owner.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.homeStructure, bed.gridTileLocation);
                                        } else {
                                            debugLog += "\n-Target has a home but does not have an available bed or already in bed, will not trigger Move Character job";
                                        }
                                    } else {
                                        debugLog += "\n-Target does not have a home, will not trigger Move Character job";
                                    }
                                } else if (targetCharacter.needsComponent.isBored || targetCharacter.needsComponent.isSulking) {
                                    debugLog += "\n-Target is bored or sulking, will trigger Move Character job if character is not in the right place to do Daydream or Pray";
                                    if (UnityEngine.Random.Range(0, 2) == 0 && targetCharacter.homeStructure != null) {
                                        //Pray
                                        if (targetCharacter.currentStructure != targetCharacter.homeStructure) {
                                            debugLog += "\n-Target chose Pray and is not inside his/her house, will trigger Move Character job";
                                            owner.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.homeStructure);
                                        } else {
                                            debugLog += "\n-Target chose Pray but is already inside his/her house, will not trigger Move Character job";
                                        }
                                    } else {
                                        //Daydream
                                        if (!targetCharacter.currentStructure.structureType.IsOpenSpace()) {
                                            debugLog += "\n-Target chose Daydream and is not in an open space structure, will trigger Move Character job";
                                            owner.jobComponent.TryTriggerMoveCharacter(targetCharacter, targetCharacter.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                                        } else {
                                            debugLog += "\n-Target chose Daydream but is already in an open space structure, will not trigger Move Character job";
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {
                    debugLog += "\n-Target is dead";
                    Dead targetDeadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
                    if(targetDeadTrait != null && !targetDeadTrait.charactersThatSawThisDead.Contains(owner)) {
                        targetDeadTrait.AddCharacterThatSawThisDead(owner);
                        debugLog += "\n-Target saw dead for the first time";
                        string opinionLabel = owner.relationshipContainer.GetOpinionLabel(targetCharacter);
                        if(opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                            debugLog += "\n-Target is Friend/Close Friend";
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                debugLog += "\n-Target will Cry";
                                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, "saw dead " + targetCharacter.name);
                            } else {
                                debugLog += "\n-Target will Puke";
                                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter);
                            }
                        } else if (opinionLabel == RelationshipManager.Rival) {
                            debugLog += "\n-Target is Rival";
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                debugLog += "\n-Target will Mock";
                                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                            } else {
                                debugLog += "\n-Target will Laugh At";
                                owner.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                            }
                        }

                        if (owner.marker && targetCharacter.isNormalCharacter) {
                            if(owner.traitContainer.HasTrait("Suspicious") 
                                || owner.moodComponent.moodState == MOOD_STATE.CRITICAL 
                                || (owner.moodComponent.moodState == MOOD_STATE.LOW && UnityEngine.Random.Range(0, 2) == 0)) {
                                debugLog += "\n-Owner is Suspicious or Critical Mood or Low Mood";

                                _assumptionSuspects.Clear();
                                for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                                    Character inVision = owner.marker.inVisionCharacters[i];
                                    if (inVision != targetCharacter && inVision.relationshipContainer.IsEnemiesWith(targetCharacter)) {
                                        _assumptionSuspects.Add(inVision);
                                    }
                                }
                                if(_assumptionSuspects.Count > 0) {
                                    debugLog += "\n-There are in vision characters that considers target character as Enemy/Rival";
                                    Character chosenSuspect = _assumptionSuspects[UnityEngine.Random.Range(0, _assumptionSuspects.Count)];

                                    debugLog += "\n-Will create Murder assumption on " + chosenSuspect.name;
                                    owner.assumptionComponent.CreateAndReactToNewAssumption(chosenSuspect, targetCharacter, INTERACTION_TYPE.MURDER, REACTION_STATUS.WITNESSED);
                                }
                            }

                        }
                    }
                }
            } else {
                debugLog += "\n-Character is minion or summon or Target is currently being targeted by an action, not going to react";
            }
        }
    }
    private void ReactTo(TileObject targetTileObject, ref string debugLog) {
        if (!owner.isNormalCharacter /*|| owner.race == RACE.SKELETON*/) {
            //Minions or Summons cannot react to objects
            return;
        }
        debugLog += $"{owner.name} is reacting to {targetTileObject.nameWithID}";
        if (!owner.isInCombat && !owner.hasSeenFire) {
            if (targetTileObject.traitContainer.HasTrait("Burning")
                && targetTileObject.gridTileLocation != null
                && owner.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(owner.homeSettlement)
                && !owner.traitContainer.HasTrait("Pyrophobic")
                && !owner.traitContainer.HasTrait("Dousing")) {
                debugLog += "\n-Target is Burning and Character is not Pyrophobic";
                owner.SetHasSeenFire(true);
                owner.homeSettlement.settlementJobTriggerComponent.TriggerDouseFire();
                if (owner.homeSettlement.HasJob(JOB_TYPE.DOUSE_FIRE) == false) {
                    Debug.LogWarning($"{owner.name} saw a fire in a settlement but no douse fire jobs were created.");
                }

                List<JobQueueItem> douseFireJobs = owner.homeSettlement.GetJobs(JOB_TYPE.DOUSE_FIRE)
                    .Where(j => j.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(j)).ToList();

                if (douseFireJobs.Count > 0) {
                    owner.jobQueue.AddJobInQueue(douseFireJobs[0]);
                } else {
                    if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                        owner.combatComponent.Flight(targetTileObject, "saw fire");
                    }
                }

                // for (int i = 0; i < owner.homeSettlement.availableJobs.Count; i++) {
                //     JobQueueItem job = owner.homeSettlement.availableJobs[i];
                //     if (job.jobType == JOB_TYPE.DOUSE_FIRE) {
                //         if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
                //             owner.jobQueue.AddJobInQueue(job);
                //         } else {
                //             if (owner.combatComponent.combatMode == COMBAT_MODE.Aggressive) {
                //                 owner.combatComponent.Flight(targetTileObject, "saw fire");
                //             }
                //         }
                //     }
                // }
            }
        }
        if (!owner.isInCombat && !owner.hasSeenWet) {
            if (targetTileObject.traitContainer.HasTrait("Wet")
                && targetTileObject.gridTileLocation != null
                && owner.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(owner.homeSettlement)
                && !owner.jobQueue.HasJob(JOB_TYPE.DRY_TILES)) {
                debugLog += "\n-Target is Wet";
                owner.SetHasSeenWet(true);
                owner.homeSettlement.settlementJobTriggerComponent.TriggerDryTiles();
                for (int i = 0; i < owner.homeSettlement.availableJobs.Count; i++) {
                    JobQueueItem job = owner.homeSettlement.availableJobs[i];
                    if (job.jobType == JOB_TYPE.DRY_TILES) {
                        if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
                            owner.jobQueue.AddJobInQueue(job);
                        }
                    }
                }
            }
        }
        if (!owner.isInCombat && !owner.hasSeenPoisoned) {
            if (targetTileObject.traitContainer.HasTrait("Poisoned")
                && targetTileObject.gridTileLocation != null
                && owner.homeSettlement != null
                && targetTileObject.gridTileLocation.IsPartOfSettlement(owner.homeSettlement)
                && !owner.jobQueue.HasJob(JOB_TYPE.CLEANSE_TILES)) {
                debugLog += "\n-Target is Poisoned";
                owner.SetHasSeenPoisoned(true);
                owner.homeSettlement.settlementJobTriggerComponent.TriggerCleanseTiles();
                for (int i = 0; i < owner.homeSettlement.availableJobs.Count; i++) {
                    JobQueueItem job = owner.homeSettlement.availableJobs[i];
                    if (job.jobType == JOB_TYPE.CLEANSE_TILES) {
                        if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
                            owner.jobQueue.AddJobInQueue(job);
                        }
                    }
                }
            }
        }
        if (targetTileObject.traitContainer.HasTrait("Dangerous") && targetTileObject.gridTileLocation != null) {
            if (targetTileObject is TornadoTileObject || owner.currentStructure == targetTileObject.gridTileLocation.structure || (!owner.currentStructure.isInterior && !targetTileObject.gridTileLocation.structure.isInterior)) {
                if (owner.traitContainer.HasTrait("Berserked")) {
                    owner.combatComponent.FightOrFlight(targetTileObject, CombatManager.Berserked);
                } else if (owner.stateComponent.currentState == null || owner.stateComponent.currentState.characterState != CHARACTER_STATE.FOLLOW) {
                    if (owner.traitContainer.HasTrait("Suicidal")) {
                        if (!owner.jobQueue.HasJob(JOB_TYPE.SUICIDE_FOLLOW)) {
                            CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.SUICIDE_FOLLOW, CHARACTER_STATE.FOLLOW, targetTileObject, owner);
                            owner.jobQueue.AddJobInQueue(job);
                        }
                    } else if (owner.moodComponent.moodState == MOOD_STATE.NORMAL) {
                        string neutralizingTraitName = TraitManager.Instance.GetNeutralizingTraitFor(targetTileObject);
                        if (neutralizingTraitName != string.Empty) {
                            if (owner.traitContainer.HasTrait(neutralizingTraitName)) {
                                if (!owner.jobQueue.HasJob(JOB_TYPE.NEUTRALIZE_DANGER, targetTileObject)) {
                                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.NEUTRALIZE_DANGER,
                                        INTERACTION_TYPE.NEUTRALIZE, targetTileObject, owner);
                                    owner.jobQueue.AddJobInQueue(job);
                                }
                            } else {
                                owner.combatComponent.Flight(targetTileObject, "saw a " + targetTileObject.name);
                            }
                        } else {
                            throw new Exception("Trying to neutralize " + targetTileObject.nameWithID + " but it does not have a neutralizing trait!");
                        }
                    } else {
                        owner.combatComponent.Flight(targetTileObject, "saw a " + targetTileObject.name);
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
        if (targetTileObject.traitContainer.HasTrait("Danger Remnant")) {
            if (!owner.traitContainer.HasTrait("Berserked")) {
                if (owner.traitContainer.HasTrait("Coward")) {
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, owner, targetTileObject, REACTION_STATUS.WITNESSED);
                } else {
                    int shockChance = 30;
                    if (owner.traitContainer.HasTrait("Combatant")) {
                        shockChance = 70;
                    }
                    if (UnityEngine.Random.Range(0, 100) < shockChance) {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, owner, targetTileObject, REACTION_STATUS.WITNESSED);
                    } else {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, owner, targetTileObject, REACTION_STATUS.WITNESSED);
                    }
                }
            }
        }
        if (targetTileObject.traitContainer.HasTrait("Surprised Remnant")) {
            if (!owner.traitContainer.HasTrait("Berserked")) {
                if (owner.traitContainer.HasTrait("Coward")) {
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, owner, targetTileObject, REACTION_STATUS.WITNESSED);
                } else {
                    if (UnityEngine.Random.Range(0, 100) < 95) {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, owner, targetTileObject, REACTION_STATUS.WITNESSED);
                    } else {
                        CharacterManager.Instance.TriggerEmotion(EMOTION.Fear, owner, targetTileObject, REACTION_STATUS.WITNESSED);
                    }
                }
            }
        }


        if (targetTileObject is Tombstone tombstone) {
            Character targetCharacter = tombstone.character;
            Dead targetDeadTrait = targetCharacter.traitContainer.GetNormalTrait<Dead>("Dead");
            if (targetDeadTrait != null && !targetDeadTrait.charactersThatSawThisDead.Contains(owner)) {
                targetDeadTrait.AddCharacterThatSawThisDead(owner);
                debugLog += "\n-Target saw dead for the first time";
                string opinionLabel = owner.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    debugLog += "\n-Target is Friend/Close Friend";
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        debugLog += "\n-Target will Cry";
                        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, "saw a dead loved one");
                    } else {
                        debugLog += "\n-Target will Puke";
                        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Puke, targetCharacter);
                    }
                } else if (opinionLabel == RelationshipManager.Rival) {
                    debugLog += "\n-Target is Rival";
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        debugLog += "\n-Target will Mock";
                        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Mock, targetCharacter);
                    } else {
                        debugLog += "\n-Target will Laugh At";
                        owner.interruptComponent.TriggerInterrupt(INTERRUPT.Laugh_At, targetCharacter);
                    }
                }
            }
        }

        if (targetTileObject.characterOwner == owner 
            && targetTileObject.gridTileLocation != null 
            && targetTileObject.gridTileLocation.structure != null
            && targetTileObject.gridTileLocation.structure is Dwelling
            && targetTileObject.gridTileLocation.structure != owner.homeStructure
            && targetTileObject.gridTileLocation.structure.residents.Count > 0) {

            if (owner.traitContainer.HasTrait("Suspicious")
                || owner.moodComponent.moodState == MOOD_STATE.CRITICAL
                || (owner.moodComponent.moodState == MOOD_STATE.LOW && UnityEngine.Random.Range(0, 2) == 0)) {
                debugLog += "\n-Owner is Suspicious or Critical Mood or Low Mood";

                debugLog += "\n-There is at least 1 resident of the structure";
                Character chosenSuspect = targetTileObject.gridTileLocation.structure.residents[UnityEngine.Random.Range(0, targetTileObject.gridTileLocation.structure.residents.Count)];

                debugLog += "\n-Will create Steal assumption on " + chosenSuspect.name;
                owner.assumptionComponent.CreateAndReactToNewAssumption(chosenSuspect, targetTileObject, INTERACTION_TYPE.STEAL, REACTION_STATUS.WITNESSED);
            }
        }
    }
    //The reason why we pass the character that was hit instead of just getting the current closest hostile in combat state is because 
    public void ReactToCombat(CombatState combat, IPointOfInterest poiHit) {
        Character attacker = combat.stateComponent.character;
        Character reactor = owner;
        if (reactor.isInCombat) {
            string inCombatLog = reactor.name + " is in combat and reacting to combat of " + attacker.name + " against " + poiHit.nameWithID;
            if (reactor == poiHit) {
                inCombatLog += "\n-Reactor is the Hit Character";
                CombatState reactorCombat = reactor.stateComponent.currentState as CombatState;
                if (reactorCombat.isAttacking && reactorCombat.currentClosestHostile != null && reactorCombat.currentClosestHostile != attacker) {
                    inCombatLog += "\n-Reactor is currently attacking another character";
                    if (reactorCombat.currentClosestHostile is Character currentPursuingCharacter) {
                        if (currentPursuingCharacter.isInCombat && (currentPursuingCharacter.stateComponent.currentState as CombatState).isAttacking == false) {
                            inCombatLog += "\n-Character that is being attacked by reactor is currently fleeing";
                            inCombatLog += "\n-Reactor will determine combat reaction";
                            reactor.combatComponent.SetWillProcessCombat(true);
                            //if (reactor.combatComponent.hostilesInRange.Contains(attacker) || reactor.combatComponent.avoidInRange.Contains(attacker)) {
                            //log += "\n-Attacker of reactor is in hostile/avoid list of the reactor, rector will determine combat reaction";
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
        if(owner.isDead || !owner.canPerform) {
            return;
        }
        string log = reactor.name + " is reacting to combat of " + attacker.name + " against " + poiHit.nameWithID;
        if (reactor.IsHostileWith(attacker)) {
            log += "\n-Hostile with attacker, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        if (combat.DidCharacterAlreadyReactToThisCombat(reactor)) {
            log += "\n-Already reacted to the combat, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        if (poiHit is Character targetHit && reactor.IsHostileWith(targetHit)) {
            log += "\n-Reactor is hostile with the hit character, will skip processing";
            reactor.logComponent.PrintLogIfActive(log);
            return;
        }
        combat.AddCharacterThatReactedToThisCombat(reactor);
        if(poiHit is Character characterHit) {
            if (combat.currentClosestHostile != characterHit) {
                log += "\n-Hit Character is not the same as the actual target which is: " + combat.currentClosestHostile?.name;
                if (characterHit.isInCombat) {
                    log += "\n-Hit Character is in combat";
                    log += "\n-Do nothing";
                } else {
                    log += "\n-Reactor felt Shocked";
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                }
            } else {
                CombatData combatDataAgainstCharacterHit = attacker.combatComponent.GetCombatData(characterHit);
                if (combatDataAgainstCharacterHit != null && combatDataAgainstCharacterHit.connectedAction != null && combatDataAgainstCharacterHit.connectedAction.associatedJobType == JOB_TYPE.APPREHEND) {
                    log += "\n-Combat is part of Apprehend Job";
                    log += "\n-Reactor felt Shocked";
                    CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                } else {
                    if (characterHit == reactor) {
                        log += "\n-Hit Character is the Reactor";
                        if (characterHit.relationshipContainer.IsFriendsWith(attacker)) {
                            log += "\n-Hit Character is Friends/Close Friends with Attacker";
                            log += "\n-Reactor felt Betrayal";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Betrayal, reactor, attacker, REACTION_STATUS.WITNESSED);
                        } else if (characterHit.relationshipContainer.IsEnemiesWith(attacker)) {
                            log += "\n-Hit Character is Enemies/Rivals with Attacker";
                            log += "\n-Reactor felt Anger";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, reactor, attacker, REACTION_STATUS.WITNESSED);
                        }
                    } else {
                        log += "\n-Hit Character is NOT the Reactor";
                        if (reactor.relationshipContainer.IsFriendsWith(characterHit)) {
                            log += "\n-Reactor is Friends/Close Friends with Hit Character";
                            if (reactor.relationshipContainer.IsFriendsWith(attacker)) {
                                log += "\n-Reactor is Friends/Close Friends with Attacker";
                                log += "\n-Reactor felt Shock, Disappointment";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else if (reactor.relationshipContainer.IsEnemiesWith(attacker)) {
                                log += "\n-Reactor is Enemies/Rivals with Attacker";
                                log += "\n-Reactor felt Rage";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Rage, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else {
                                log += "\n-Reactor felt Anger";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, reactor, attacker, REACTION_STATUS.WITNESSED);
                            }
                        } else if (reactor.relationshipContainer.IsEnemiesWith(characterHit)) {
                            log += "\n-Reactor is Enemies/Rivals with Hit Character";
                            if (reactor.relationshipContainer.IsFriendsWith(attacker)) {
                                log += "\n-Reactor is Friends/Close Friends with Attacker";
                                log += "\n-Reactor felt Approval";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else if (reactor.relationshipContainer.IsEnemiesWith(attacker)) {
                                log += "\n-Reactor is Enemies/Rivals with Attacker";
                                log += "\n-Reactor felt Shock";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                            } else {
                                log += "\n-Reactor felt Approval";
                                CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, reactor, attacker, REACTION_STATUS.WITNESSED);
                            }
                        } else {
                            log += "\n-Reactor felt Shock";
                            CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, reactor, attacker, REACTION_STATUS.WITNESSED);
                        }
                    }
                }
            }
            //Check for crime
            if ((reactor.faction != null && reactor.faction == attacker.faction) || (reactor.homeSettlement != null && reactor.homeSettlement == attacker.homeSettlement)) {
                log += "\n-Reactor is the same faction/home settlement as Attacker";
                log += "\n-Reactor is checking for crime";
                CombatData combatDataAgainstPOIHit = attacker.combatComponent.GetCombatData(characterHit);
                if (combatDataAgainstPOIHit != null && combatDataAgainstPOIHit.connectedAction != null) {
                    ActualGoapNode possibleCrimeAction = combatDataAgainstPOIHit.connectedAction;
                    CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(possibleCrimeAction);
                    log += "\n-Crime committed is: " + crimeType.ToString();
                    if (crimeType != CRIME_TYPE.NONE) {
                        log += "\n-Reactor will react to crime";
                        CrimeManager.Instance.ReactToCrime(reactor, attacker, possibleCrimeAction, possibleCrimeAction.associatedJobType, crimeType);
                    }
                }
            }

        } else if (poiHit is TileObject objectHit) {
            if(objectHit.characterOwner != attacker) {
                //CrimeManager.Instance.ReactToCrime()
                log += "\n-Object Hit is not owned by the Attacker";
                log += "\n-Reactor is checking for crime";
                CombatData combatDataAgainstPOIHit = attacker.combatComponent.GetCombatData(objectHit);
                if (combatDataAgainstPOIHit != null && combatDataAgainstPOIHit.connectedAction != null) {
                    ActualGoapNode possibleCrimeAction = combatDataAgainstPOIHit.connectedAction;
                    CRIME_TYPE crimeType = CrimeManager.Instance.GetCrimeTypeConsideringAction(possibleCrimeAction);
                    log += "\n-Crime committed is: " + crimeType.ToString();
                    if (crimeType != CRIME_TYPE.NONE) {
                        log += "\n-Reactor will react to crime";
                        CrimeManager.Instance.ReactToCrime(reactor, attacker, possibleCrimeAction, possibleCrimeAction.associatedJobType, crimeType);
                    }
                }
            }
        }

        reactor.logComponent.PrintLogIfActive(log);
    }
    // private void ReactTo(SpecialToken targetItem, ref string debugLog) {
    //     if (owner.minion != null || owner is Summon) {
    //         //Minions or Summons cannot react to items
    //         return;
    //     }
    //     debugLog += owner.name + " is reacting to " + targetItem.nameWithID;
    //     if (!owner.hasSeenFire) {
    //         if (targetItem.traitContainer.HasTrait("Burning")
    //             && targetItem.gridTileLocation != null
    //             && targetItem.gridTileLocation.IsPartOfSettlement(owner.homeNpcSettlement)
    //             && !owner.traitContainer.HasTrait("Pyrophobic")) {
    //             debugLog += "\n-Target is Burning and Character is not Pyrophobic";
    //             owner.SetHasSeenFire(true);
    //             owner.homeNpcSettlement.settlementJobTriggerComponent.TriggerDouseFire();
    //             for (int i = 0; i < owner.homeNpcSettlement.availableJobs.Count; i++) {
    //                 JobQueueItem job = owner.homeNpcSettlement.availableJobs[i];
    //                 if (job.jobType == JOB_TYPE.DOUSE_FIRE) {
    //                     if (job.assignedCharacter == null && owner.jobQueue.CanJobBeAddedToQueue(job)) {
    //                         owner.jobQueue.AddJobInQueue(job);
    //                     } else {
    //                         owner.combatComponent.Flight(targetItem);
    //                     }
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    // }
    #endregion

    #region General
    private bool IsPOICurrentlyTargetedByAPerformingAction(IPointOfInterest poi) {
        for (int i = 0; i < poi.allJobsTargetingThis.Count; i++) {
            if(poi.allJobsTargetingThis[i] is GoapPlanJob) {
                GoapPlanJob planJob = poi.allJobsTargetingThis[i] as GoapPlanJob;
                if(planJob.assignedPlan != null && planJob.assignedPlan.currentActualNode.actionStatus == ACTION_STATUS.PERFORMING) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion
}
