using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UtilityScripts;

public class CombatComponent : CharacterComponent {
    public int attack { get; private set; }
    public int attackModification { get; private set; }
    public int maxHP { get; private set; }
    public int maxHPModification { get; private set; }
    public int attackSpeed { get; private set; }  //in milliseconds, The lower the amount the faster the attack rate

    public COMBAT_MODE combatMode { get; private set; }
    public List<IPointOfInterest> hostilesInRange { get; private set; } //POI's in this characters hostility collider
    public List<IPointOfInterest> avoidInRange { get; private set; } //POI's in this characters hostility collider
    public List<Character> bannedFromHostileList { get; private set; }
    public Dictionary<IPointOfInterest, CombatData> combatDataDictionary { get; private set; }
    //public string avoidReason { get; private set; }
    public ElementalDamageData elementalDamage { get; private set; }
    //public ActualGoapNode actionThatTriggeredCombatState { get; private set; }
    //public GoapPlanJob jobThatTriggeredCombatState { get; private set; }
    // public ActualGoapNode combatConnectedActionNode { get; private set; }

    //delegates
    //public delegate void OnProcessCombat(CombatState state);
    //private OnProcessCombat onProcessCombat; //actions to be executed and cleared when a character processes combat.

    public bool willProcessCombat { get; private set; }

    #region getters
    public bool isInCombat => owner.stateComponent.currentState != null && owner.stateComponent.currentState.characterState == CHARACTER_STATE.COMBAT;
    public bool isInActualCombat => IsInActualCombat();
    public int unModifiedMaxHP => Mathf.RoundToInt(owner.characterClass.baseHP *
                                                   (owner.raceSetting.hpMultiplier == 0f ? 
                                                       1f : owner.raceSetting.hpMultiplier));
    public int unModifiedAttack => Mathf.RoundToInt(owner.characterClass.baseAttackPower *
                                                    (owner.raceSetting.attackMultiplier == 0f
                                                        ? 1f : owner.raceSetting.attackMultiplier));
    #endregion

    public CombatComponent() {
        hostilesInRange = new List<IPointOfInterest>();
        avoidInRange = new List<IPointOfInterest>();
        bannedFromHostileList = new List<Character>();
        combatDataDictionary = new Dictionary<IPointOfInterest, CombatData>();
        SetCombatMode(COMBAT_MODE.Aggressive);
        SetElementalType(ELEMENTAL_TYPE.Normal);
        //UpdateBasicData(true);
	}
    public CombatComponent(SaveDataCombatComponent data) {
        hostilesInRange = new List<IPointOfInterest>();
        avoidInRange = new List<IPointOfInterest>();
        bannedFromHostileList = new List<Character>();
        combatDataDictionary = new Dictionary<IPointOfInterest, CombatData>();

        attack = data.attack;
        attackModification = data.attackModification;
        maxHP = data.maxHP;
        maxHPModification = data.maxHPModification;
        attackSpeed = data.attackSpeed;
        combatMode = data.combatMode;
        elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(data.elementalDamageType);
        willProcessCombat = data.willProcessCombat;
    }

    #region General
    //public void OnThisCharacterEndedCombatState() {
    //    SetOnProcessCombatAction(null);
    //}
    private void ProcessCombatBehavior() {
        string log = $"{owner.name} process combat switch is turned on, processing combat...";
        if (owner.interruptComponent.isInterrupted) {
            log +=
                $"\n-Character is interrupted: {owner.interruptComponent.currentInterrupt.name}, will not process combat";
        } else {
            if (owner.combatComponent.isInCombat) {
                log += "\n-Character is already in combat, determining combat action to do";
                Messenger.Broadcast(CharacterSignals.DETERMINE_COMBAT_REACTION, owner);
            } else {
                log += "\n-Character is not in combat, will add Combat job if there is a hostile or avoid in range";
                if (hostilesInRange.Count > 0 || avoidInRange.Count > 0) {
                    if (!owner.jobQueue.HasJob(JOB_TYPE.COMBAT)) {
                        log += "\n-No existing combat job, Combat job added";
                        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, owner);
                        owner.jobQueue.AddJobInQueue(job);
                    } else {
                        log += "\n-Has existing combat job, no combat job added";
                    }
                }
                //Removed this because this part will not be called because we are checking here if owner.combatComponent.isInCombat and we are already in the else condition of owner.combatComponent.isInCombat from the code above
                //So this part is useless
                //else {
                //    log += "\n-Combat job not added";
                //    if (owner.marker != null && owner.marker.hasFleePath && owner.combatComponent.isInCombat) {
                //        CombatState combatState = owner.stateComponent.currentState as CombatState;
                //        combatState.CheckFlee(ref log);
                //    }
                //}
            }
            //avoidReason = string.Empty;
        }
        owner.logComponent.PrintLogIfActive(log);
        //execute any external combat actions. This assumes that this character entered combat state.

        //NOTE: Commented this out temporarily because we no longer immediately switch the state of the character to combat, instead we create a job and add it to its job queue
        //This means that the character will always have a null current state in this tick
        //onProcessCombat?.Invoke(owner.stateComponent.currentState as CombatState);
        //SetOnProcessCombatAction(null);
    }
    //public void AddOnProcessCombatAction(OnProcessCombat action) {
    //    onProcessCombat += action;
    //}
    //public void SetOnProcessCombatAction(OnProcessCombat action) {
    //    onProcessCombat = action;
    //}
    public void CheckCombatPerTickEnded() {
        if (willProcessCombat) {
            SetWillProcessCombat(false); //Moved this up here, because ProcessCombatBehavior can set process combat to true again, and we don't want to overwrite that.
            ProcessCombatBehavior();
        } 
        //else {
        //    if (owner.marker && owner.marker.hasFleePath) {
        //        if (owner.combatComponent.isInCombat) {
        //            CombatState combatState = owner.stateComponent.currentState as CombatState;
        //            combatState.CheckFlee();
        //        }
        //    }
        //}
    }
    public void SetCombatMode(COMBAT_MODE mode) {
        combatMode = mode;
    }
    public void SetElementalType(ELEMENTAL_TYPE elementalType) {
        elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
    }
    public void UpdateElementalType() {
        bool hasSetElementalType = false;
        for (int i = (owner.traitContainer.traits.Count - 1); i >= 0; i--) {
            Trait currTrait = owner.traitContainer.traits[i];
            if (currTrait.elementalType != ELEMENTAL_TYPE.Normal) {
                SetElementalType(currTrait.elementalType);
                hasSetElementalType = true;
                break;
            }
        }
        if (!hasSetElementalType) {
            SetElementalType(owner.characterClass.elementalType);
        }
    }
    //public void SetActionAndJobThatTriggeredCombat(ActualGoapNode node, GoapPlanJob job) {
    //    actionThatTriggeredCombatState = node;
    //    jobThatTriggeredCombatState = job;
    //}
    public void SetWillProcessCombat(bool state) {
        willProcessCombat = state;
    }
    private bool IsInActualCombat() {
        if(owner.marker && owner.stateComponent.currentState != null && owner.stateComponent.currentState is CombatState combatState) {
            if (combatState.isAttacking) {
                //Only become in "actual" combat in attacking mode if character is already in vision of the target or if the target is fleeing
                if(combatState.currentClosestHostile != null) {
                    if (owner.marker.IsPOIInVision(combatState.currentClosestHostile)) {
                        return true;
                    } else if (owner.marker.inVisionPOIsButDiffStructure.Contains(combatState.currentClosestHostile)) {
                        return true;
                    } else if (combatState.currentClosestHostile is Character currentHostileCharacter && currentHostileCharacter.combatComponent.isInCombat) {
                        CombatState combatStateOfCurrentHostile = currentHostileCharacter.stateComponent.currentState as CombatState;
                        if (!combatStateOfCurrentHostile.isAttacking) {
                            return true;
                        }
                    }
                }
            } else {
                //If character is fleeing, he/she is always in actual combat
                return true;
            }
        }
        return false;
    }
    public bool IsInActualCombatWith(IPointOfInterest target) {
        if (owner.marker && owner.stateComponent.currentState != null && owner.stateComponent.currentState is CombatState combatState) {
            if (combatState.isAttacking) {
                //Only become in "actual" combat in attacking mode if character is already in vision of the target or if the target is fleeing
                if (combatState.currentClosestHostile != null && combatState.currentClosestHostile == target) {
                    if(combatState.currentClosestHostile is Character hostileCharacter) {
                        if (owner.marker.IsPOIInVision(hostileCharacter)) {
                            return true;
                        }
                    } else if (combatState.currentClosestHostile is TileObject hostileTileObject) {
                        if (owner.marker.IsPOIInVision(hostileTileObject)) {
                            return true;
                        }
                    }
                    if (owner.marker.inVisionPOIsButDiffStructure.Contains(combatState.currentClosestHostile)) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Fight or Flight
    public CombatReaction GetFightOrFlightReaction(IPointOfInterest target, string fightReason) {
        string debugLog = $"FIGHT or FLIGHT response of {owner.name} against {target.nameWithID}";
        //return new CombatReaction(COMBAT_REACTION.Flight);

        if (!owner.limiterComponent.canPerform || !owner.limiterComponent.canMove) {
            debugLog += "\n-Character cannot move/perform, will not fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.None);
        }
        if (hostilesInRange.Contains(target) || avoidInRange.Contains(target)) {
            debugLog += "\n-Target is already in hostile/avoid list, will no longer trigger fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.None);
        }
        if (owner.behaviourComponent.HasBehaviour(typeof(DisablerBehaviour))) {
            debugLog += "\n-Character is a Disabler";
            debugLog += "\n-FLIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.Flight);
        }
        if (owner.traitContainer.HasTrait("Berserked") || owner is Summon || owner.characterClass.className == "Zombie" || owner.race == RACE.DEMON) {
            debugLog += "\n-Character is berserked/monster/zombie/demon";
            debugLog += "\n-FIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        } else if (owner.traitContainer.HasTrait("Drunk")) {
            debugLog += "\n-Character is drunk, 50% chance to Fight";
            if (GameUtilities.RollChance(50)) {
                debugLog += "\n-FIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
            } else {
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                return new CombatReaction(COMBAT_REACTION.Flight);
            }
        } else if (target is TileObject targetTileObject) {
            debugLog += "\n-Target is object";
            if (owner.traitContainer.HasTrait("Coward")) {
                debugLog += "\n-Character is coward";
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                return new CombatReaction(COMBAT_REACTION.Flight, "character is a coward");
            } else if (targetTileObject.traitContainer.HasTrait("Dangerous")) {
                debugLog += "\n-Object is dangerous";
                if (string.IsNullOrEmpty(targetTileObject.neutralizer) == false &&
                    owner.traitContainer.HasTrait(targetTileObject.neutralizer)) {
                    debugLog += $"\n-Character has neutralizer trait {targetTileObject.neutralizer}";
                    debugLog += "\n-FIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                } else {
                    debugLog += "\n-FLIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Flight);
                }
            } else {
                debugLog += "\n-FIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
            }
        } else if (target is Character targetCharacter) {
            debugLog += "\n-Target is character";
            bool isOwnerCombatant = owner.characterClass.IsCombatant() || owner.characterClass.className == "Noble";
            if (!isOwnerCombatant) {
                debugLog += "\n-Character is non-combatant";
                if (owner.traitContainer.HasTrait("Coward")) {
                    debugLog += "\n-Character is coward";
                    debugLog += "\n-FLIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Flight, "character is a coward");
                } else {
                    bool isTargetCombatant = targetCharacter.characterClass.IsCombatant() || targetCharacter.characterClass.className == "Noble";
                    if (!isTargetCombatant) {
                        debugLog += "\n-Target is non-combatant";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else if (HasCharacterInVisionWithSameHostile(targetCharacter) && owner.IsInHomeSettlement()) {
                        debugLog += "\n-Character has someone in vision with the same hostile target and Character is in home settlement";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else if (targetCharacter.characterClass.className == "Noble") {
                        debugLog += "\n-Target is Noble";
                        debugLog += "\n-FLIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Flight);
                    } else {
                        debugLog += "\n-95% chance to Flight";
                        if (GameUtilities.RollChance(95)) {
                            debugLog += "\n-FLIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
                            return new CombatReaction(COMBAT_REACTION.Flight);
                        } else {
                            debugLog += "\n-FIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
                            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                        }
                    }
                }
            } else {
                debugLog += "\n-Character is combatant";
                if (CombatManager.Instance.IsImmuneToElement(targetCharacter, elementalDamage.type)) {
                    debugLog += "\n-Target is immune to character elemental damage";
                    if (HasCharacterInVisionWithSameHostile(targetCharacter)) {
                        debugLog += "\n-Character has someone in vision with the same hostile target";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else {
                        debugLog += "\n-FLIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        return new CombatReaction(COMBAT_REACTION.Flight);
                    }
                } else if (owner.traitContainer.HasTrait("Coward", "Vampire") && owner.currentHP <= Mathf.CeilToInt(owner.maxHP * 0.2f)) {
                    debugLog += "\n-Character is coward and and HP is 20% or less of Max HP";
                    debugLog += "\n-FLIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Flight, "character is a coward");
                } else {
                    debugLog += "\n-FIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                }
            }
        }
        //if (target is Character) {
        //    debugLog += "\n-Target is character";
        //    Character targetCharacter = target as Character;
        //    if (owner.traitContainer.HasTrait("Coward")) {
        //        debugLog += "\n-Character is coward";
        //        if(owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
        //            debugLog += "\n-Character is a demon";
        //            debugLog += "\n-FIGHT";
        //            owner.logComponent.PrintLogIfActive(debugLog);
        //            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //        } else {
        //            debugLog += "\n-FLIGHT";
        //            owner.logComponent.PrintLogIfActive(debugLog);
        //            return new CombatReaction(COMBAT_REACTION.Flight, "character is a coward");
        //        }
        //    } else {
        //        debugLog += "\n-Character is not coward";
        //        if (!owner.traitContainer.HasTrait("Combatant", "Royalty")) {
        //            debugLog += "\n-Character is not combatant, 20% to Fight";
        //            int chance = UnityEngine.Random.Range(0, 100);
        //            debugLog += $"\n-Roll: {chance}";
        //            if (chance < 20) {
        //                debugLog += "\n-FIGHT";
        //                owner.logComponent.PrintLogIfActive(debugLog);
        //                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //            } else {
        //                if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
        //                    debugLog += "\n-Character is a demon";
        //                    debugLog += "\n-FIGHT";
        //                    owner.logComponent.PrintLogIfActive(debugLog);
        //                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //                } else {
        //                    debugLog += "\n-FLIGHT";
        //                    owner.logComponent.PrintLogIfActive(debugLog);
        //                    return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
        //                }
        //            }
        //        } else {
        //            debugLog += "\n-Character is combatant or royalty";
        //            if (owner.currentHP > targetCharacter.currentHP) {
        //                debugLog += "\n-Character hp is higher than target";
        //                debugLog += "\n-FIGHT";
        //                owner.logComponent.PrintLogIfActive(debugLog);
        //                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //            } else {
        //                debugLog += "\n-Character hp is lower or equal than target";
        //                if (CombatManager.Instance.IsImmuneToElement(targetCharacter, elementalDamage.type)) {
        //                    debugLog += "\n-Target is immune to character elemental damage";
        //                    if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
        //                        debugLog += "\n-Character is a demon";
        //                        debugLog += "\n-FIGHT";
        //                        owner.logComponent.PrintLogIfActive(debugLog);
        //                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //                    } else {
        //                        debugLog += "\n-FLIGHT";
        //                        owner.logComponent.PrintLogIfActive(debugLog);
        //                        return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
        //                    }
        //                } else if (CombatManager.Instance.IsImmuneToElement(owner, targetCharacter.combatComponent.elementalDamage.type)) {
        //                    debugLog += "\n-Character is immune to target elemental damage";
        //                    debugLog += "\n-FIGHT";
        //                    owner.logComponent.PrintLogIfActive(debugLog);
        //                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //                } else {
        //                    if (owner.currentHP >= Mathf.CeilToInt(owner.maxHP * 0.15f)) {
        //                        debugLog += "\n-Character's hp is greater than or equal to 30% of its max hp";
        //                        debugLog += "\n-FIGHT";
        //                        owner.logComponent.PrintLogIfActive(debugLog);
        //                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //                    } else {
        //                        int fightChance = 25;
        //                        for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
        //                            if (owner.marker.inVisionCharacters[i].combatComponent.hostilesInRange.Contains(target)) {
        //                                debugLog += "\n-Character has another character in vision who has the same target";
        //                                fightChance = 75;
        //                                break;
        //                            }
        //                        }
        //                        debugLog += $"\n-Fight chance: {fightChance}";
        //                        int roll = UnityEngine.Random.Range(0, 100);
        //                        debugLog += $"\n-Roll: {roll}";
        //                        if (roll < fightChance) {
        //                            debugLog += "\n-FIGHT";
        //                            owner.logComponent.PrintLogIfActive(debugLog);
        //                            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //                        } else {
        //                            if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
        //                                debugLog += "\n-Character is a demon";
        //                                debugLog += "\n-FIGHT";
        //                                owner.logComponent.PrintLogIfActive(debugLog);
        //                                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //                            } else {
        //                                debugLog += "\n-FLIGHT";
        //                                owner.logComponent.PrintLogIfActive(debugLog);
        //                                return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //} else if (target is TileObject tileObject) {
        //    debugLog += "\n-Target is object";
        //    if (owner.traitContainer.HasTrait("Coward")) {
        //        debugLog += "\n-Character is coward";
        //        if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
        //            debugLog += "\n-Character is a demon";
        //            debugLog += "\n-FIGHT";
        //            owner.logComponent.PrintLogIfActive(debugLog);
        //            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //        } else {
        //            debugLog += "\n-FLIGHT";
        //            owner.logComponent.PrintLogIfActive(debugLog);
        //            return new CombatReaction(COMBAT_REACTION.Flight, "character is a coward");
        //        }
        //    } else if (tileObject.traitContainer.HasTrait("Dangerous")) {
        //        debugLog += "\n-Object is dangerous";
        //        if (string.IsNullOrEmpty(tileObject.neutralizer) == false && 
        //            owner.traitContainer.HasTrait(tileObject.neutralizer)) {
        //            debugLog += $"\n-Character has neutralizer trait {tileObject.neutralizer}";
        //            owner.logComponent.PrintLogIfActive(debugLog);
        //            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //        } else {
        //            if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
        //                debugLog += "\n-Character is a demon";
        //                debugLog += "\n-FIGHT";
        //                owner.logComponent.PrintLogIfActive(debugLog);
        //                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //            } else {
        //                debugLog += "\n-FLIGHT";
        //                owner.logComponent.PrintLogIfActive(debugLog);
        //                return new CombatReaction(COMBAT_REACTION.Flight, "got scared");
        //            }
        //        }
        //    } else {
        //        debugLog += "\n-Object is not dangerous";
        //        owner.logComponent.PrintLogIfActive(debugLog);
        //        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        //    }
        //}
        owner.logComponent.PrintLogIfActive(debugLog);
        return new CombatReaction(COMBAT_REACTION.None);
    }
    public void FightOrFlight(IPointOfInterest target, CombatReaction combatReaction, 
        ActualGoapNode connectedAction = null, bool isLethal = true) {
        if (combatReaction.reaction == COMBAT_REACTION.Fight) {
            Fight(target, combatReaction.reason, connectedAction, isLethal);
        } else if (combatReaction.reaction == COMBAT_REACTION.Flight) {
            if (owner.movementComponent.isStationary) {
                owner.logComponent.PrintLogIfActive($"Supposed to FLIGHT for {owner.name} against {target.nameWithID} but character is STATIONARY, fight insted");
                Fight(target, combatReaction.reason, connectedAction, isLethal);
            } else {
                Flight(target, combatReaction.reason);
            }
        }
    }
    public void FightOrFlight(IPointOfInterest target, string fightReason, ActualGoapNode connectedAction = null, bool isLethal = true) {
        CombatReaction combatReaction = GetFightOrFlightReaction(target, fightReason);
        FightOrFlight(target, combatReaction, connectedAction, isLethal);
    }
    public bool Fight(IPointOfInterest target, string reason, ActualGoapNode connectedAction = null, bool isLethal = true) {
        bool hasFought = false;
        bool cannotFight = reason == CombatManager.Hostility && (target is Character targetCharacter && bannedFromHostileList.Contains(targetCharacter));
        if (!hostilesInRange.Contains(target) && !cannotFight) {
            string debugLog = $"Triggered FIGHT response for {owner.name} against {target.nameWithID}";
            hostilesInRange.Add(target);
            avoidInRange.Remove(target);
            SetWillProcessCombat(true);

            //CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
            //newCombatData.SetFightData(reason, connectedAction, isLethal);
            if (combatDataDictionary.ContainsKey(target)) {
                //CombatData prevCombatData = fightCombatData[target];
                //ObjectPoolManager.Instance.ReturnCombatDataToPool(prevCombatData);
                //fightCombatData[target] = newCombatData;
                combatDataDictionary[target].SetFightData(reason, connectedAction, isLethal);
            } else {
                CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
                newCombatData.SetFightData(reason, connectedAction, isLethal);
                combatDataDictionary.Add(target, newCombatData);
            }

            if (target is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(1);
            }
            target.CancelRemoveStatusFeedAndRepairJobsTargetingThis();
            debugLog += $"\n{target.name} was added to {owner.name}'s hostile range!";
            hasFought = true;
            owner.logComponent.PrintLogIfActive(debugLog);
        }
        return hasFought;
    }
    public bool Flight(IPointOfInterest target, string reason = "") {
        if (owner.movementComponent.isStationary) {
            owner.logComponent.PrintLogIfActive($"Triggered FLIGHT response for {owner.name} against {target.nameWithID} but character is STATIONARY, cannot flee");
            return false;
        }
        bool hasFled = false;
        if (hostilesInRange.Remove(target)) {
            //if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            //    fightCombatData.Remove(target as Character);
            //} else 
            if (target is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(-1);
            } else if (target is Character targetCharacter) {
                AddPOIToBannedFromHostile(targetCharacter);
                //if (fightCombatData.ContainsKey(targetCharacter) && fightCombatData[targetCharacter].reasonForCombat == CombatManager.Hostility) {
                //    AddPOIToBannedFromHostile(targetCharacter);
                //}
            }
        }
        if (!avoidInRange.Contains(target)) {
            string debugLog = $"Triggered FLIGHT response for {owner.name} against {target.nameWithID}";
            if (owner.marker.IsPOIInVision(target)) {
                avoidInRange.Add(target);
                SetWillProcessCombat(true);

                if (combatDataDictionary.ContainsKey(target)) {
                    combatDataDictionary[target].SetFlightData(reason);
                } else {
                    CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
                    newCombatData.SetFlightData(reason);
                    combatDataDictionary.Add(target, newCombatData);
                }

                debugLog += $"\n{target.name} was added to {owner.name}'s avoid range!";
                hasFled = true;
                if (target is Character) {
                    Character targetCharacter = target as Character;
                    if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Defend) {
                        targetCharacter.combatComponent.RemoveHostileInRange(owner);
                    }
                }
            }
            owner.logComponent.PrintLogIfActive(debugLog);
        }
        return hasFled;
    }
    public void FlightAll(string reason = "") {
        if (owner.movementComponent.isStationary) {
            owner.logComponent.PrintLogIfActive($"Triggered FLIGHT ALL response for {owner.name} but character is STATIONARY, cannot flee");
            return;
        }
        //Demons no longer trigger Flight
        //https://trello.com/c/D4bdwPhH/1104-demons-and-monsters-no-longer-trigger-flight
        if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
            return;
        }
        if (hostilesInRange.Count > 0) {
            if (owner.limiterComponent.canMove) {
                for (int i = 0; i < hostilesInRange.Count; i++) {
                    IPointOfInterest hostile = hostilesInRange[i];
                    if (owner.marker.IsPOIInVision(hostile)) {
                        avoidInRange.Add(hostile);

                        if (combatDataDictionary.ContainsKey(hostile)) {
                            combatDataDictionary[hostile].SetFlightData(reason);
                        } else {
                            CombatData newCombatData = ObjectPoolManager.Instance.CreateNewCombatData();
                            newCombatData.SetFlightData(reason);
                            combatDataDictionary.Add(hostile, newCombatData);
                        }

                        if (hostile is Character) {
                            Character targetCharacter = hostile as Character;
                            if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Defend) {
                                targetCharacter.combatComponent.RemoveHostileInRange(owner);
                            }
                        }
                    }
                }
            }
            ClearHostilesInRange(false);
            SetWillProcessCombat(true);
            //avoidReason = reason;
        }
    }
    #endregion

    #region Hostiles
    //private bool AddHostileInRange(IPointOfInterest poi, bool processCombatBehaviour = true, bool isLethal = true) {
    //    //Not yet applicable
    //    //if (!hostilesInRange.Contains(poi)) {
    //    //    hostilesInRange.Add(poi);
    //    //    if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
    //    //        lethalCharacters.Add(poi as Character, isLethal);
    //    //    }
    //    //    owner.logComponent.PrintLogIfActive(poi.name + " was added to " + owner.name + "'s hostile range!");
    //    //    willProcessCombat = true;
    //    //}
    //    return false;
    //}
    public bool RemoveHostileInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (hostilesInRange.Remove(poi)) {
            //if (poi is Character character) {
            //    fightCombatData.Remove(character);
            //} else 
            if (poi is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(-1);
            } else if (poi is Character targetCharacter) {
                AddPOIToBannedFromHostile(targetCharacter);
                //if (fightCombatData.ContainsKey(targetCharacter) && fightCombatData[targetCharacter].reasonForCombat == CombatManager.Hostility) {
                //    AddPOIToBannedFromHostile(targetCharacter);
                //}
            }
            string removeHostileSummary = $"{poi.name} was removed from {owner.name}'s hostile range.";
            owner.logComponent.PrintLogIfActive(removeHostileSummary);
            //When removing hostile in range, check if character is still in combat state, if it is, reevaluate combat behavior, if not, do nothing
            if (processCombatBehavior) {
                if (owner.combatComponent.isInCombat) {
                    CombatState combatState = owner.stateComponent.currentState as CombatState;
                    if (combatState.forcedTarget == poi) {
                        combatState.SetForcedTarget(null);
                    }
                    if (combatState.currentClosestHostile == poi) {
                        combatState.ResetClosestHostile();
                    }
                    //Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                }
                SetWillProcessCombat(true);
            }
            return true;
        }
        return false;
    }
    public void ClearHostilesInRange(bool processCombatBehavior = true) {
        if (hostilesInRange.Count > 0) {
            for (int i = 0; i < hostilesInRange.Count; i++) {
                if (hostilesInRange[i] is TileObject targetTileObject) {
                    targetTileObject.AdjustRepairCounter(-1);
                } else if (hostilesInRange[i] is Character targetCharacter) {
                    AddPOIToBannedFromHostile(targetCharacter);
                    //if (fightCombatData.ContainsKey(targetCharacter) && fightCombatData[targetCharacter].reasonForCombat == CombatManager.Hostility) {
                    //    AddPOIToBannedFromHostile(targetCharacter);
                    //}
                }
            }
            hostilesInRange.Clear();
            //fightCombatData.Clear();
            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
                SetWillProcessCombat(true);
            }
        }
    }
    public bool IsLethalCombatForTarget(Character character) {
        if (combatDataDictionary.ContainsKey(character)) {
            return combatDataDictionary[character].isLethal;
        }
        return true;
    }
    public bool HasLethalCombatTarget() {
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi is Character) {
                Character hostile = poi as Character;
                if (IsLethalCombatForTarget(hostile)) {
                    return true;
                }
            }

        }
        return false;
    }
    public IPointOfInterest GetNearestValidHostile() {
        IPointOfInterest nearest = null;
        float nearestDist = 9999f;
        //first check only the hostiles that are in the same npcSettlement as this character
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi.IsValidCombatTargetFor(owner)) {
                float dist = Vector2.Distance(owner.marker.transform.position, poi.worldPosition);
                if (nearest == null || dist < nearestDist) {
                    nearest = poi;
                    nearestDist = dist;
                }
            } else {
                //If poi in the list is no longer a valid combat target, remove it because there is no sense in keeping it in the list if you can't attack it
                if(RemoveHostileInRange(poi, false)) {
                    i--;
                }
            }
        }
        //if no character was returned, choose at random from the list, since we are sure that all characters in the list are not in the same npcSettlement as this character
        if (nearest == null) {
            //List<Character> hostileCharacters = hostilesInRange.Where(x => x.poiType == POINT_OF_INTEREST_TYPE.CHARACTER).Select(x => x as Character).ToList();
            //if (hostileCharacters.Count > 0) {
            //    nearest = hostileCharacters[UnityEngine.Random.Range(0, hostileCharacters.Count)];
            //}
            for (int i = 0; i < hostilesInRange.Count; i++) {
                IPointOfInterest poi = hostilesInRange[i];
                if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    if (poi.IsValidCombatTargetFor(owner)) {
                        nearest = hostilesInRange[i];
                        break;
                    } else {
                        //If poi in the list is no longer a valid combat target, remove it because there is no sense in keeping it in the list if you can't attack it
                        if (RemoveHostileInRange(poi, false)) {
                            i--;
                        }
                    }
                }
            }
        }
        return nearest;
    }
    public IPointOfInterest GetNearestValidHostilePriorityNotFleeing() {
        IPointOfInterest nearest = null;
        float nearestDist = 9999f;
        //first check only the hostiles that are in the same npcSettlement as this character
        for (int i = 0; i < hostilesInRange.Count; i++) {
            IPointOfInterest poi = hostilesInRange[i];
            if (poi.IsValidCombatTargetFor(owner)) {
                if(poi is Character hostileCharacter) {
                    if(hostileCharacter.combatComponent.isInCombat && (hostileCharacter.stateComponent.currentState as CombatState).isAttacking == false) {
                        continue;
                    }
                }
                float dist = Vector2.Distance(owner.marker.transform.position, poi.worldPosition);
                if (nearest == null || dist < nearestDist) {
                    nearest = poi;
                    nearestDist = dist;
                }
            } else {
                //If poi in the list is no longer a valid combat target, remove it because there is no sense in keeping it in the list if you can't attack it
                if (RemoveHostileInRange(poi, false)) {
                    i--;
                }
            }
        }
        if(nearest != null) {
            return nearest;
        }
        return GetNearestValidHostile();
    }
    //public void OnItemRemovedFromTile(SpecialToken token, LocationGridTile removedFrom) {
    //    if (hostilesInRange.Contains(token)) {
    //        RemoveHostileInRange(token);
    //    }
    //}
    private void AddPOIToBannedFromHostile(Character targetCharacter) {
        if (owner.movementComponent.isStationary) {
            //Stationary characters cannot ban hostiles because they can't move/pursue targets
            return;
        }
        //Dead/unconscious characters should not be banned because they can no longer be attacked anyway
        if (targetCharacter.isDead || targetCharacter.traitContainer.HasTrait("Unconscious")) {
            return;
        }

        if (targetCharacter.combatComponent.isInCombat && !(targetCharacter.stateComponent.currentState as CombatState).isAttacking) {
            //Only ban characters that are already fleeing when you removed them from hostile list
        } else {
            return;
        }
        if (!bannedFromHostileList.Contains(targetCharacter)) {
            bannedFromHostileList.Add(targetCharacter);
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(2);
            SchedulingManager.Instance.AddEntry(dueDate, () => RemovePOIToBannedFromHostile(targetCharacter), owner);
        }
    }
    private bool RemovePOIToBannedFromHostile(Character targetCharacter) {
        return bannedFromHostileList.Remove(targetCharacter);
    }
    public bool HasCharacterInVisionWithSameHostile(Character hostile) {
        if (owner.marker) {
            for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                Character inVision = owner.marker.inVisionCharacters[i];
                if(inVision != hostile) {
                    if (inVision.combatComponent.hostilesInRange.Contains(hostile)) {
                        return true;
                    }
                }
                
            }
        }
        return false;
    }
    #endregion

    #region Avoid
    private bool AddAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true, string reason = "") {
        if (owner.limiterComponent.canMove) {
        //if (!poi.isDead && !poi.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && character.traitContainer.GetNormalTrait<Trait>("Berserked") == null) {
            if (!avoidInRange.Contains(poi)) {
                avoidInRange.Add(poi);
                SetWillProcessCombat(true);
                //avoidReason = reason;
                return true;
            }
        }
        return false;
    }
    public void RemoveAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (avoidInRange.Remove(poi)) {
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
                owner.logComponent.PrintLogIfActive($"{poi.name} was removed from {owner.name}'s avoid range!");
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
            }
        }
    }
    public void RemoveAvoidInRangeSchedule(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (avoidInRange.Contains(poi)) {
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(3);
            SchedulingManager.Instance.AddEntry(dueDate, () => FinalCheckForRemoveAvoidSchedule(poi, processCombatBehavior), owner);
        }
    }
    private void FinalCheckForRemoveAvoidSchedule(IPointOfInterest poi, bool processCombatBehavior) {
        if (owner.marker) {
            if (!owner.marker.IsStillInRange(poi)) {
                RemoveAvoidInRange(poi, processCombatBehavior);
            }
        }
    }
    public void RemoveHostileInRangeSchedule(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (hostilesInRange.Contains(poi)) {
            if(combatDataDictionary.ContainsKey(poi) && combatDataDictionary[poi].reasonForCombat != CombatManager.Demon_Kill && combatDataDictionary[poi].connectedAction == null) {
                GameDate dueDate = GameManager.Instance.Today();
                dueDate.AddTicks(2);
                SchedulingManager.Instance.AddEntry(dueDate, () => FinalCheckForRemoveHostileSchedule(poi, processCombatBehavior), owner);
            }
        }
    }
    private void FinalCheckForRemoveHostileSchedule(IPointOfInterest poi, bool processCombatBehavior) {
        if (owner.marker) {
            if (!owner.marker.IsStillInRange(poi)) {
                RemoveHostileInRange(poi, processCombatBehavior);
            }
        }
    }
    public void ClearAvoidInRange(bool processCombatBehavior = true) {
        if (avoidInRange.Count > 0) {
            avoidInRange.Clear();
            owner.logComponent.PrintLogIfActive($"{owner.name} cleared avoid range!");
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
            }
        }
    }
    #endregion

    #region Combat Data
    public CombatData GetCombatData(IPointOfInterest target) {
        if (combatDataDictionary.ContainsKey(target)) {
            return combatDataDictionary[target];
        }
        return null;
    }
    public void ClearCombatData() {
        foreach (CombatData combatData in combatDataDictionary.Values) {
            ObjectPoolManager.Instance.ReturnCombatDataToPool(combatData);
        }
        combatDataDictionary.Clear();
    }
    public string GetCombatLogKeyReason(IPointOfInterest target) {
        string key = string.Empty;
        CombatData combatData = GetCombatData(target);
        if (combatData != null) {
            key = combatData.reasonForCombat;
            if (key == CombatManager.Action) {
                switch (combatData.connectedAction.associatedJobType) {
                    case JOB_TYPE.RESTRAIN:
                        key = "Restrain";
                        break;
                    case JOB_TYPE.PRODUCE_FOOD:
                    case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
                    case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:   
                        key = "Butcher";
                        break;
                    case JOB_TYPE.APPREHEND:
                        key = CombatManager.Apprehend;
                        break;
                    case JOB_TYPE.RITUAL_KILLING:
                        key = "Ritual Killing";
                        break;
                    case JOB_TYPE.BERSERK_ATTACK:
                        key = "Berserked";
                        break;
                    case JOB_TYPE.BRAWL:
                        key = "Snapped";
                        break;
                    case JOB_TYPE.DESTROY:
                        if(target is Guitar guitar && guitar.IsOwnedBy(owner)) {
                            if (owner.traitContainer.HasTrait("Music Hater")) {
                                key = "Destroy_Music_Hater";
                                break;
                            }
                        } else if (target is CultistKit) {
                            key = "Destroy_Cultist_Kit";
                            break;
                        }
                        if (owner.traitContainer.HasTrait("Angry")) {
                            key = "Destroy_Angry";
                        } else if (owner.traitContainer.HasTrait("Suspicious")) {
                            key = "Destroy_Suspicious";
                        }
                        break;
                    case JOB_TYPE.CAPTURE_CHARACTER:
                        if (owner is Troll) {
                            key = CombatManager.Abduct;
                        }
                        break;
                    case JOB_TYPE.KIDNAP_RAID:
                        key = CombatManager.Raid;
                        break;
                }
            } else {
                if(key == CombatManager.Anger) {
                    if (owner.traitContainer.HasTrait("Angry")) {
                        Trait trait = owner.traitContainer.GetTraitOrStatus<Trait>("Angry");
                        if(trait.IsResponsibleForTrait(target)) {
                            key = "Anger_Target";
                        }
                    }
                } else if (key == CombatManager.Hostility) {
                    key = GetHostilityReason(target, combatData);
                } else if (key == CombatManager.Retaliation) {
                    key = GetRetaliationReason(target, combatData);
                }
            }
        }
        return key;
    }
    private string GetRetaliationReason(IPointOfInterest target, CombatData combatData) {
        //Get reason of the target towards this character to know why target attacked this character in the first place
        //Because if the reason is Retaliation we assume that the target attacked this character first
        if(target is Character targetCharacter) {
            string reason = targetCharacter.combatComponent.GetCombatLogKeyReason(owner);
            if(!string.IsNullOrEmpty(reason)) {
                if(reason == CombatManager.Apprehend) {
                    return CombatManager.Resisting_Arrest;
                } else if (reason == CombatManager.Abduct) {
                    return CombatManager.Resisting_Abduction;
                }
            }
        }
        return CombatManager.Defending_Self;
    }
    private string GetHostilityReason(IPointOfInterest target, CombatData combatData) {
        Character targetCharacter = target as Character;
        if (owner.partyComponent.isMemberThatJoinedQuest) {
            PartyQuest quest = owner.partyComponent.currentParty.currentQuest;
            if(quest is RaidPartyQuest raidQuest && raidQuest.targetSettlement != null) {
                if(targetCharacter != null) {
                    if(targetCharacter.homeSettlement == raidQuest.targetSettlement) {
                        return CombatManager.Raid;
                    }
                } else if (target is TileObject) {
                    if(target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(raidQuest.targetSettlement)) {
                        return CombatManager.Raid;
                    }
                }
            }
        }
        if (targetCharacter != null) {
            if(owner.IsInHomeSettlement()) {
                if(targetCharacter.homeSettlement != owner.homeSettlement) {
                    return CombatManager.Defending_Territory;
                }
            } else if (owner.IsInTerritory()) {
                if (!targetCharacter.IsTerritory(owner.hexTileLocation)) {
                    return CombatManager.Defending_Territory;
                }
            }
        }
        if(owner.faction != null) {
            if(owner.faction == FactionManager.Instance.neutralFaction) {
                return CombatManager.Feral_Monster;
            } else if (owner.faction == FactionManager.Instance.undeadFaction) {
                return CombatManager.Hostile_Undead;
            }
        }

        if(targetCharacter != null) {
            if (owner.minion != null && targetCharacter.faction != null && targetCharacter.faction.isMajorNonPlayer) {
                if (combatData.isLethal) {
                    return CombatManager.Slaying_Villager;
                } else {
                    return CombatManager.Incapacitating_Villager;
                }
            }
            if (targetCharacter.minion != null) {
                if (combatData.isLethal) {
                    return CombatManager.Slaying_Demon;
                } else {
                    return CombatManager.Incapacitating_Demon;
                }
            }
            if(targetCharacter.faction != null) {
                if (targetCharacter.faction == FactionManager.Instance.vagrantFaction) {
                    return CombatManager.Fighting_Vagrant;
                } else if (targetCharacter.faction == FactionManager.Instance.neutralFaction) {
                    if (combatData.isLethal) {
                        return CombatManager.Slaying_Monster;
                    } else {
                        return CombatManager.Incapacitating_Monster;
                    }
                } else if (targetCharacter.faction == FactionManager.Instance.undeadFaction) {
                    if (combatData.isLethal) {
                        return CombatManager.Slaying_Undead;
                    } else {
                        return CombatManager.Incapacitating_Undead;
                    }
                }
                if (owner.faction != null && owner.faction.IsHostileWith(targetCharacter.faction)) {
                    return CombatManager.Warring_Factions;
                }
            }
        }
        //default is still Hostility
        return CombatManager.Hostility;
    }
    #endregion

    #region Jobs
    public void OnJobRemovedFromQueue(JobQueueItem job) {
        //Dropped Combat Related Jobs should remove target from hostile list if not yet in actual combat with it?
        //https://trello.com/c/GXzaAAsP/1361-dropped-combat-related-jobs-should-remove-target-from-hostile-list-if-not-yet-in-actual-combat-with-it
        if (!job.finishedSuccessfully) {
            bool hasRemoved = false;
            foreach (KeyValuePair<IPointOfInterest, CombatData> kvp in combatDataDictionary) {
                if (kvp.Value.connectedAction != null && kvp.Value.connectedAction.associatedJob == job) {
                    if (!IsInActualCombatWith(kvp.Key)) {
                        if (RemoveHostileInRange(kvp.Key, false)){
                            hasRemoved = true;
                        }
                    }
                }
            }
            if (hasRemoved) {
                SetWillProcessCombat(true);
            }
        }
    }
    #endregion

    #region Basic Data
    public void UpdateBasicData(bool resetHP) {
        UpdateAttack();
        UpdateAttackSpeed();
        if (resetHP) {
            UpdateMaxHPAndReset();
        } else {
            UpdateMaxHPAndProportionateHP();
        }
    }
    private void UpdateAttack() {
        attack = Mathf.RoundToInt(owner.characterClass.baseAttackPower * (owner.raceSetting.attackMultiplier == 0f ? 1f : owner.raceSetting.attackMultiplier)) + attackModification;
    }
    private void UpdateMaxHP() {
        maxHP = Mathf.RoundToInt(owner.characterClass.baseHP * (owner.raceSetting.hpMultiplier == 0f ? 1f : owner.raceSetting.hpMultiplier)) + maxHPModification;
        if (maxHP < 0) {
            maxHP = 1;
        }
    }
    private void UpdateAttackSpeed() {
        attackSpeed = owner.characterClass.baseAttackSpeed;
        //attackSpeed = Mathf.RoundToInt(owner.characterClass.baseAttackSpeed * (owner.raceSetting.attackSpeedMultiplier == 0f ? 1f : owner.raceSetting.attackSpeedMultiplier));
    }
    public void UpdateMaxHPAndReset() {
        UpdateMaxHP();
        owner.ResetToFullHP();
    }
    public void UpdateMaxHPAndProportionateHP() {
        float hpPercentage = owner.currentHP / (float) maxHP;
        UpdateMaxHP();
        int newCurrentHP = Mathf.RoundToInt(hpPercentage * maxHP);
        if(newCurrentHP < 0) {
            newCurrentHP = 0;
        }
        owner.SetHP(newCurrentHP);
    }
    public void AdjustMaxHPModifier(int modification) {
        maxHPModification += modification;
        UpdateMaxHP();
    }
    public void AdjustAttackModifier(int modification) {
        attackModification += modification;
        UpdateAttack();
    }
    #endregion

    #region Signals
    public void SubscribeToSignals() {
        //Messenger.AddListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
    }
    public void UnsubscribeToSignals() {
        //Messenger.RemoveListener<JobQueueItem, Character>(Signals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromQueue);
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCombatComponent data) {
        for (int i = 0; i < data.hostileCharactersInRange.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.hostileCharactersInRange[i]);
            if (!hostilesInRange.Contains(character)) {
                hostilesInRange.Add(character);
            }
        }
        for (int i = 0; i < data.hostileTileObjectsInRange.Count; i++) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.hostileTileObjectsInRange[i]);
            if (!hostilesInRange.Contains(tileObject)) {
                hostilesInRange.Add(tileObject);
            }
        }

        for (int i = 0; i < data.avoidCharactersInRange.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.avoidCharactersInRange[i]);
            if (!avoidInRange.Contains(character)) {
                avoidInRange.Add(character);
            }
        }
        for (int i = 0; i < data.avoidTileObjectsInRange.Count; i++) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.avoidTileObjectsInRange[i]);
            if (!avoidInRange.Contains(tileObject)) {
                avoidInRange.Add(tileObject);
            }
        }

        foreach (KeyValuePair<string, SaveDataCombatData> item in data.characterCombatData) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(item.Key);
            CombatData combatData = item.Value.Load();
            combatDataDictionary.Add(character, combatData);
        }
        foreach (KeyValuePair<string, SaveDataCombatData> item in data.tileObjectCombatData) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(item.Key);
            CombatData combatData = item.Value.Load();
            combatDataDictionary.Add(tileObject, combatData);
        }

        for (int i = 0; i < data.bannedFromHostileList.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.bannedFromHostileList[i]);
            if (!bannedFromHostileList.Contains(character)) {
                bannedFromHostileList.Add(character);
            }
        }
    }
    #endregion
}

public struct CombatReaction {
    public COMBAT_REACTION reaction;
    public string reason;

    public CombatReaction(COMBAT_REACTION reaction, string reason = "") {
        this.reaction = reaction;
        this.reason = reason;
    }
}

public class CombatData {
    public string reasonForCombat;
    public string avoidReason;
    public ActualGoapNode connectedAction;
    public bool isLethal;

    public CombatData() {
        Initialize();
    }
    public CombatData(SaveDataCombatData data) {
        reasonForCombat = data.reasonForCombat;
        avoidReason = data.avoidReason;
        isLethal = data.isLethal;
        if (!string.IsNullOrEmpty(data.connectedAction)) {
            connectedAction = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(data.connectedAction);
        }
    }
    public void Initialize() {
        reasonForCombat = string.Empty;
        avoidReason = string.Empty;
        connectedAction = null;
        isLethal = false;
    }
    public void Reset() {
        Initialize();
    }
    public void SetFightData(string reasonForCombat, ActualGoapNode connectedAction, bool isLethal) {
        this.reasonForCombat = reasonForCombat;
        this.connectedAction = connectedAction;
        this.isLethal = isLethal;
    }
    public void SetFlightData(string avoidReason) {
        this.avoidReason = avoidReason;
    }
}

[System.Serializable]
public class SaveDataCombatData : SaveData<CombatData> {
    public string reasonForCombat;
    public string avoidReason;
    public string connectedAction;
    public bool isLethal;

    #region Overrides
    public override void Save(CombatData data) {
        reasonForCombat = data.reasonForCombat;
        avoidReason = data.avoidReason;
        isLethal = data.isLethal;
        if(data.connectedAction != null) {
            connectedAction = data.connectedAction.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.connectedAction);
        }
    }

    public override CombatData Load() {
        CombatData component = new CombatData(this);
        return component;
    }
    #endregion
}

[System.Serializable]
public class SaveDataCombatComponent : SaveData<CombatComponent> {
    public int attack;
    public int attackModification;
    public int maxHP;
    public int maxHPModification;
    public int attackSpeed;

    public COMBAT_MODE combatMode;
    public List<string> hostileCharactersInRange;
    public List<string> hostileTileObjectsInRange;

    public List<string> avoidCharactersInRange;
    public List<string> avoidTileObjectsInRange;

    public List<string> bannedFromHostileList;

    public Dictionary<string, SaveDataCombatData> characterCombatData;
    public Dictionary<string, SaveDataCombatData> tileObjectCombatData;

    public ELEMENTAL_TYPE elementalDamageType;

    public bool willProcessCombat;

    #region Overrides
    public override void Save(CombatComponent data) {
        attack = data.attack;
        attackModification = data.attackModification;
        maxHP = data.maxHP;
        maxHPModification = data.maxHPModification;
        attackSpeed = data.attackSpeed;
        combatMode = data.combatMode;
        elementalDamageType = data.elementalDamage.type;
        willProcessCombat = data.willProcessCombat;

        hostileCharactersInRange = new List<string>();
        hostileTileObjectsInRange = new List<string>();
        for (int i = 0; i < data.hostilesInRange.Count; i++) {
            IPointOfInterest poi = data.hostilesInRange[i];
            if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                hostileCharactersInRange.Add(poi.persistentID);
            } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                hostileTileObjectsInRange.Add(poi.persistentID);
            }
        }

        avoidCharactersInRange = new List<string>();
        avoidTileObjectsInRange = new List<string>();
        for (int i = 0; i < data.avoidInRange.Count; i++) {
            IPointOfInterest poi = data.avoidInRange[i];
            if (poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                avoidCharactersInRange.Add(poi.persistentID);
            } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                avoidTileObjectsInRange.Add(poi.persistentID);
            }
        }

        characterCombatData = new Dictionary<string, SaveDataCombatData>();
        tileObjectCombatData = new Dictionary<string, SaveDataCombatData>();
        foreach (KeyValuePair<IPointOfInterest, CombatData> item in data.combatDataDictionary) {
            SaveDataCombatData saveCombatData = new SaveDataCombatData();
            saveCombatData.Save(item.Value);
            if(item.Key.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                characterCombatData.Add(item.Key.persistentID, saveCombatData);
            } else if (item.Key.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                tileObjectCombatData.Add(item.Key.persistentID, saveCombatData);
            }
        }

        bannedFromHostileList = new List<string>();
        for (int i = 0; i < data.bannedFromHostileList.Count; i++) {
            bannedFromHostileList.Add(data.bannedFromHostileList[i].persistentID);
        }
    }

    public override CombatComponent Load() {
        CombatComponent component = new CombatComponent(this);
        return component;
    }
    #endregion
}