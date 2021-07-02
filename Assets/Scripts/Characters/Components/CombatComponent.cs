using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using UtilityScripts;
using Locations.Settlements;

public class CombatComponent : CharacterComponent {
    public int attack { get; private set; }
    //public int strength { get; private set; } //used as attack if character is physical damage type
    //public int intelligence { get; private set; } //used as attack if character is magical damage type
    public int strengthModification { get; private set; }
    public float strengthPercentModification { get; private set; }
    public int intelligenceModification { get; private set; }
    public float intelligencePercentModification { get; private set; }
    public int maxHP { get; private set; }
    public int maxHPModification { get; private set; }
    public float maxHPPercentModification { get; private set; }
    public int attackSpeed { get; private set; }  //in milliseconds, The lower the amount the faster the attack rate
    public int numOfKilledCharacters { get; private set; }
    public COMBAT_MODE combatMode { get; private set; }
    public List<IPointOfInterest> hostilesInRange { get; private set; } //POI's in this characters hostility collider
    public List<IPointOfInterest> avoidInRange { get; private set; } //POI's in this characters hostility collider
    public List<Character> bannedFromHostileList { get; private set; }
    public Dictionary<IPointOfInterest, CombatData> combatDataDictionary { get; private set; }
    public ElementalDamageData elementalDamage { get; private set; }
    public ELEMENTAL_TYPE initialElementalType { get; private set; }
    public List<ELEMENTAL_TYPE> elementalStatusWaitingList = new List<ELEMENTAL_TYPE>();
    public CharacterCombatBehaviourParent combatBehaviourParent { get; private set; }
    public CombatSpecialSkillWrapper specialSkillParent { get; private set; }
    public bool willProcessCombat { get; private set; }

    public int critRate { get; private set; }

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
        specialSkillParent = new CombatSpecialSkillWrapper();
        combatBehaviourParent = new CharacterCombatBehaviourParent();
        SetCombatMode(COMBAT_MODE.Aggressive);
        //SetElementalType(ELEMENTAL_TYPE.Normal);
        //initialElementalType = ELEMENTAL_TYPE.Normal;
        //UpdateBasicData(true);
    }
    public CombatComponent(SaveDataCombatComponent data) {
        hostilesInRange = new List<IPointOfInterest>();
        avoidInRange = new List<IPointOfInterest>();
        bannedFromHostileList = new List<Character>();
        combatDataDictionary = new Dictionary<IPointOfInterest, CombatData>();

        attack = data.attack;
        //strength = data.strength;
        strengthModification = data.strengthModification;
        critRate = data.critRate;
        strengthPercentModification = data.strengthPercentModification;
        //intelligence = data.intelligence;
        intelligenceModification = data.intelligenceModification;
        intelligencePercentModification = data.intelligencePercentModification;
        maxHP = data.maxHP;
        maxHPModification = data.maxHPModification;
        maxHPPercentModification = data.maxHPPercentModification;
        attackSpeed = data.attackSpeed;
        combatMode = data.combatMode;
        elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(data.elementalDamageType);
        //initialElementalType = data.initialElementalDamageType;
        elementalStatusWaitingList = new List<ELEMENTAL_TYPE>();
        data.elementalStatusWaitingList.ForEach((eachElem) => elementalStatusWaitingList.Add(eachElem));
        willProcessCombat = data.willProcessCombat;
        numOfKilledCharacters = data.numOfKilledCharacters;
        specialSkillParent = data.specialSkillParent.Load();
        combatBehaviourParent = data.combatBehaviourParent.Load();
    }

    #region Signals
    public void SubscribeToSignals() {
        Messenger.AddListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
        Messenger.AddListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
    }
    public void UnsubscribeToSignals() {
        Messenger.RemoveListener<Prisoner>(TraitSignals.HAS_BECOME_PRISONER, OnHasBecomePrisoner);
        Messenger.RemoveListener<MovingTileObject>(TileObjectSignals.MOVING_TILE_OBJECT_EXPIRED, OnMovingTileObjectExpired);
    }
    #endregion


    #region Listeners
    private void OnHasBecomePrisoner(Prisoner prisoner) {
        OnCharacterBecomePrisoner(prisoner);
    }
    private void OnMovingTileObjectExpired(MovingTileObject p_tileObject) {
        if (IsAvoidInRange(p_tileObject)) {
            RemoveAvoidInRange(p_tileObject);    
        }
    }
    #endregion

    public struct DamageDoneType {
        public enum DamageType { Normal = 0, Crit }
        public int amount;
        public DamageType damageType;
    }
    public DamageDoneType damageDone;

    #region General
    public void ApplyInitialBonusStrengthAndIntelligenceOnCreation() {
        AdjustStrengthModifier(GameUtilities.RandomBetweenTwoNumbers(2, 5));
        AdjustIntelligenceModifier(GameUtilities.RandomBetweenTwoNumbers(2, 5));
    }
    public int GetAttackWithCritRateBonus() {
        int multiplier = 1;
        if(GameUtilities.RandomBetweenTwoNumbers(0, 99) < critRate) {
            multiplier = 2;
            damageDone.damageType = DamageDoneType.DamageType.Crit;
        } else {
            damageDone.damageType = DamageDoneType.DamageType.Normal;
        }
        damageDone.amount = attack * multiplier;
        return attack * multiplier;
    }
    //public void OnThisCharacterEndedCombatState() {
    //    SetOnProcessCombatAction(null);
    //}
    private void ProcessCombatBehavior() {
#if DEBUG_LOG
        string log = $"{owner.name} process combat switch is turned on, processing combat...";
#endif
        if (owner.interruptComponent.isInterrupted) {
#if DEBUG_LOG
            log +=
                $"\n-Character is interrupted: {owner.interruptComponent.currentInterrupt.name}, will not process combat";
#endif
        } else {
            if (owner.combatComponent.isInCombat) {
#if DEBUG_LOG
                log += "\n-Character is already in combat, determining combat action to do";
#endif
                Messenger.Broadcast(CharacterSignals.DETERMINE_COMBAT_REACTION, owner);
            } else {
#if DEBUG_LOG
                log += "\n-Character is not in combat, will add Combat job if there is a hostile or avoid in range";
#endif
                if (hostilesInRange.Count > 0 || avoidInRange.Count > 0) {
                    if (!owner.jobQueue.HasJob(JOB_TYPE.COMBAT)) {
#if DEBUG_LOG
                        log += "\n-No existing combat job, Combat job added";
#endif
                        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, owner);
                        owner.jobQueue.AddJobInQueue(job);
                    } else {
#if DEBUG_LOG
                        log += "\n-Has existing combat job, no combat job added";
#endif
                    }
                }
                //Removed this because this part will not be called because we are checking here if owner.combatComponent.isInCombat and we are already in the else condition of owner.combatComponent.isInCombat from the code above
                //So this part is useless
                //else {
                //    log += "\n-Combat job not added";
                //    if (owner.hasMarker && owner.marker.hasFleePath && owner.combatComponent.isInCombat) {
                //        CombatState combatState = owner.stateComponent.currentState as CombatState;
                //        combatState.CheckFlee(ref log);
                //    }
                //}
            }
            //avoidReason = string.Empty;
        }
#if DEBUG_LOG
        owner.logComponent.PrintLogIfActive(log);
#endif
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
        if (!owner.equipmentComponent.HasEquips()) {
            bool hasSetElementalType = false;
            elementalStatusWaitingList.ForEach((eachElem) => Debug.LogError(eachElem));
            if (elementalStatusWaitingList.Count > 0) {
                int index = UnityEngine.Random.Range(0, elementalStatusWaitingList.Count);
                hasSetElementalType = true;
                SetElementalType(elementalStatusWaitingList[index]);
            }
            if (!hasSetElementalType) {
                SetElementalType(owner.characterClass.elementalType);
            }
        } else {
            
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
#if DEBUG_LOG
        string debugLog = $"FIGHT or FLIGHT response of {owner.name} against {target.nameWithID}";
#endif
        //return new CombatReaction(COMBAT_REACTION.Flight);

        if (!owner.limiterComponent.canPerform || !owner.limiterComponent.canMove) {
#if DEBUG_LOG
            debugLog += "\n-Character cannot move/perform, will not fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
#endif
            return new CombatReaction(COMBAT_REACTION.None);
        }
        if (IsHostileInRange(target) || IsAvoidInRange(target)) {
#if DEBUG_LOG
            debugLog += "\n-Target is already in hostile/avoid list, will no longer trigger fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
#endif
            return new CombatReaction(COMBAT_REACTION.None);
        }
        if (owner.behaviourComponent.HasBehaviour(typeof(DisablerBehaviour))) {
#if DEBUG_LOG
            debugLog += "\n-Character is a Disabler";
            debugLog += "\n-FLIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
#endif
            return new CombatReaction(COMBAT_REACTION.Flight);
        }
        if (owner.traitContainer.HasTrait("Enslaved") && owner.isNormalCharacter && target is Character targetChar && targetChar.isNormalCharacter) {
#if DEBUG_LOG
            debugLog += "\n-Character is a villager slave and target is a villager";
            debugLog += "\n-FLIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
#endif
            return new CombatReaction(COMBAT_REACTION.Flight);
        }
        if (target is Character) {
            if (owner.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Glass_Cannon) 
                || owner.combatComponent.combatBehaviourParent.IsCombatBehaviour(CHARACTER_COMBAT_BEHAVIOUR.Healer)) {
#if DEBUG_LOG
                debugLog += "\n-Owner is glass cannon/healer and is part of an active party";
#endif
                if (owner.partyComponent.HasReachablePartymateToFleeTo()) {
                    if (!owner.partyComponent.HasPartymateInVision()) {
#if DEBUG_LOG
                        debugLog += "\n-Owner has no party member in vision, will flee";
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return new CombatReaction(COMBAT_REACTION.Flight, CombatManager.Vulnerable);
                    } else {
#if DEBUG_LOG
                        debugLog += "\n-Owner has party member in vision";
#endif
                    }
                }
            }
        }
        if (owner.traitContainer.HasTrait("Berserked") || owner is Summon || owner.characterClass.IsZombie() || owner.race == RACE.DEMON) {
#if DEBUG_LOG
            debugLog += "\n-Character is berserked/monster/zombie/demon";
            debugLog += "\n-FIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
#endif
            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
        } else if (owner.race == RACE.RATMAN && owner.faction?.factionType.type == FACTION_TYPE.Ratmen) {
#if DEBUG_LOG
            debugLog += "\n-Character is Ratman and in a Ratmen faction";
#endif
            BaseSettlement settlement = null;
            if(owner.gridTileLocation != null && owner.gridTileLocation.IsPartOfSettlement(out settlement) && settlement.owner != null && settlement.owner != owner.faction) {
#if DEBUG_LOG
                debugLog += "\n-Character is inside an occupied Settlement owned by a different faction, Flight";
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
#endif
                return new CombatReaction(COMBAT_REACTION.Flight);
            } else {
#if DEBUG_LOG
                debugLog += "\n-FIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
#endif
                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
            }
        } else if (owner.traitContainer.HasTrait("Drunk")) {
#if DEBUG_LOG
            debugLog += "\n-Character is drunk, 50% chance to Fight";
#endif
            if (GameUtilities.RollChance(50)) {
#if DEBUG_LOG
                debugLog += "\n-FIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
#endif
                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
            } else {
#if DEBUG_LOG
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
#endif
                return new CombatReaction(COMBAT_REACTION.Flight);
            }
        } else if (target is TileObject targetTileObject) {
#if DEBUG_LOG
            debugLog += "\n-Target is object";
#endif
            if (owner.traitContainer.HasTrait("Coward")) {
#if DEBUG_LOG
                debugLog += "\n-Character is coward";
                debugLog += "\n-FLIGHT";
#endif
                Coward coward = owner.traitContainer.GetTraitOrStatus<Coward>("Coward");
                if (!coward.TryActivatePassOut(owner)) {
#if DEBUG_LOG
                    owner.logComponent.PrintLogIfActive(debugLog);
#endif
                    return new CombatReaction(COMBAT_REACTION.Flight, CombatManager.Coward);
                }
#if DEBUG_LOG
                debugLog += "\n-Coward character passed out instead";
                owner.logComponent.PrintLogIfActive(debugLog);
#endif
                return default;
            } else if (targetTileObject.traitContainer.HasTrait("Dangerous")) {
#if DEBUG_LOG
                debugLog += "\n-Object is dangerous";
#endif
                if (string.IsNullOrEmpty(targetTileObject.neutralizer) == false &&
                    owner.traitContainer.HasTrait(targetTileObject.neutralizer)) {
#if DEBUG_LOG
                    debugLog += $"\n-Character has neutralizer trait {targetTileObject.neutralizer}";
                    debugLog += "\n-FIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
#endif
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                } else {
#if DEBUG_LOG
                    debugLog += "\n-FLIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
#endif
                    return new CombatReaction(COMBAT_REACTION.Flight);
                }
            } else {
#if DEBUG_LOG
                debugLog += "\n-FIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
#endif
                return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
            }
        } else if (target is Character targetCharacter) {
#if DEBUG_LOG
            debugLog += "\n-Target is character";
#endif
            bool isOwnerCombatant = owner.characterClass.IsCombatant() || owner.characterClass.className == "Noble";
            if (!isOwnerCombatant) {
#if DEBUG_LOG
                debugLog += "\n-Character is non-combatant";
#endif
                if (owner.traitContainer.HasTrait("Coward")) {
#if DEBUG_LOG
                    debugLog += "\n-Character is coward";
                    debugLog += "\n-FLIGHT";
#endif
                    Coward coward = owner.traitContainer.GetTraitOrStatus<Coward>("Coward");
                    if (!coward.TryActivatePassOut(owner)) {
#if DEBUG_LOG
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return new CombatReaction(COMBAT_REACTION.Flight, CombatManager.Coward);
                    }
#if DEBUG_LOG
                    debugLog += "\n-Coward character passed out instead";
                    owner.logComponent.PrintLogIfActive(debugLog);
#endif
                    return default;
                } else {
                    bool isTargetCombatant = targetCharacter.characterClass.IsCombatant() || targetCharacter.characterClass.className == "Noble";
                    if (!isTargetCombatant) {
#if DEBUG_LOG
                        debugLog += "\n-Target is non-combatant";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else if (HasCharacterInVisionWithSameHostile(targetCharacter) && owner.IsInHomeSettlement()) {
#if DEBUG_LOG
                        debugLog += "\n-Character has someone in vision with the same hostile target and Character is in home settlement";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else if (targetCharacter.characterClass.className == "Noble") {
#if DEBUG_LOG
                        debugLog += "\n-Target is Noble";
                        debugLog += "\n-FLIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return new CombatReaction(COMBAT_REACTION.Flight);
                    } else {
#if DEBUG_LOG
                        debugLog += "\n-95% chance to Flight";
#endif
                        if (GameUtilities.RollChance(95)) {
#if DEBUG_LOG
                            debugLog += "\n-FLIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
#endif
                            return new CombatReaction(COMBAT_REACTION.Flight);
                        } else {
#if DEBUG_LOG
                            debugLog += "\n-FIGHT";
                            owner.logComponent.PrintLogIfActive(debugLog);
#endif
                            return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                        }
                    }
                }
            } else {
#if DEBUG_LOG
                debugLog += "\n-Character is combatant";
#endif
                if (CombatManager.Instance.IsImmuneToElement(targetCharacter, elementalDamage.type)) {
#if DEBUG_LOG
                    debugLog += "\n-Target is immune to character elemental damage";
#endif
                    if (HasCharacterInVisionWithSameHostile(targetCharacter)) {
#if DEBUG_LOG
                        debugLog += "\n-Character has someone in vision with the same hostile target";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                    } else {
#if DEBUG_LOG
                        debugLog += "\n-FLIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return new CombatReaction(COMBAT_REACTION.Flight);
                    }
                } else if (owner.traitContainer.HasTrait("Coward", "Vampire") && owner.currentHP <= Mathf.CeilToInt(owner.maxHP * 0.2f)) {
#if DEBUG_LOG
                    debugLog += "\n-Character is coward/vampire bat and and HP is 20% or less of Max HP";
#endif
                    Coward coward = owner.traitContainer.GetTraitOrStatus<Coward>("Coward");
                    if(coward != null) {
                        if (!coward.TryActivatePassOut(owner)) {
#if DEBUG_LOG
                            owner.logComponent.PrintLogIfActive(debugLog);
#endif
                            return new CombatReaction(COMBAT_REACTION.Flight, CombatManager.Coward);
                        }
#if DEBUG_LOG
                        debugLog += "\n-Coward character passed out instead";
                        owner.logComponent.PrintLogIfActive(debugLog);
#endif
                        return default;
                    }
                    Vampire vampire = owner.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
                    if (vampire != null) {
                        if (vampire.CanTransformIntoBat()) {
#if DEBUG_LOG
                            debugLog += "\n-Character can transform into a bat, flee";
                            owner.logComponent.PrintLogIfActive(debugLog);
#endif
                            return new CombatReaction(COMBAT_REACTION.Flight, "can escape as a vampire bat");
                        }
                    }
#if DEBUG_LOG
                    debugLog += "\n-Character is a coward/vampire but cannot flee, fight instead";
                    owner.logComponent.PrintLogIfActive(debugLog);
#endif
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                } else {
#if DEBUG_LOG
                    debugLog += "\n-FIGHT";
                    owner.logComponent.PrintLogIfActive(debugLog);
#endif
                    return new CombatReaction(COMBAT_REACTION.Fight, fightReason);
                }
            }
        }
#if DEBUG_LOG
        owner.logComponent.PrintLogIfActive(debugLog);
#endif
        return new CombatReaction(COMBAT_REACTION.None);
    }
    public void FightOrFlight(IPointOfInterest target, CombatReaction combatReaction, 
        ActualGoapNode connectedAction = null, bool isLethal = true) {
        if (combatReaction.reaction == COMBAT_REACTION.Fight) {
            Fight(target, combatReaction.reason, connectedAction, isLethal);
        } else if (combatReaction.reaction == COMBAT_REACTION.Flight) {
            if (owner.movementComponent.isStationary) {
#if DEBUG_LOG
                owner.logComponent.PrintLogIfActive($"Supposed to FLIGHT for {owner.name} against {target.nameWithID} but character is STATIONARY, fight insted");
#endif
                Fight(target, combatReaction.reason, connectedAction, isLethal);
            } else {
                Flight(target, combatReaction.reason);
            }
        }
    }
    public void FightOrFlight(IPointOfInterest target, string fightReason, ActualGoapNode connectedAction = null, bool isLethal = true) {
        CombatReaction combatReaction = GetFightOrFlightReaction(target, fightReason);
        if(combatReaction.reaction != COMBAT_REACTION.None) {
            FightOrFlight(target, combatReaction, connectedAction, isLethal);
        }
    }
    public bool Fight(IPointOfInterest target, string reason, ActualGoapNode connectedAction = null, bool isLethal = true) {
#if DEBUG_LOG
        string debugLog = $"Triggered FIGHT response for {owner.name} against {target.nameWithID}";
#endif
        bool hasFought = false;
        bool cannotFight = (reason == CombatManager.Hostility && target is Character targetCharacter && bannedFromHostileList.Contains(targetCharacter)) || !owner.limiterComponent.canPerform;
        if (!cannotFight) {
            if (!IsHostileInRange(target)) {
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
#if DEBUG_LOG
                debugLog += $"\n{target.name} was added to {owner.name}'s hostile range!";
#endif
                hasFought = true;
            } else {
#if DEBUG_LOG
                debugLog += $"\n{target.name} is already in {owner.name}'s hostile range!";
#endif
            }
        } else {
#if DEBUG_LOG
            debugLog += $"\n{owner.name} cannot fight!";
#endif
        }
#if DEBUG_LOG
        owner.logComponent.PrintLogIfActive(debugLog);
#endif
        return hasFought;
    }
    public bool Flight(IPointOfInterest target, string reason = "") {
        if (owner.movementComponent.isStationary) {
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"Triggered FLIGHT response for {owner.name} against {target.nameWithID} but character is STATIONARY, cannot flee");
#endif
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
#if DEBUG_LOG
        string debugLog = $"Triggered FLIGHT response for {owner.name} against {target.nameWithID}";
#endif
        if (owner.limiterComponent.canMove) {
            if (!IsAvoidInRange(target)) {
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
#if DEBUG_LOG
                    debugLog += $"\n{target.name} was added to {owner.name}'s avoid range!";
#endif
                    hasFled = true;
                    if (target is Character) {
                        Character targetCharacter = target as Character;
                        if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Defend) {
                            targetCharacter.combatComponent.RemoveHostileInRange(owner);
                        }
                    }
                }
            } else {
#if DEBUG_LOG
                debugLog += $"\n{target.name} is already {owner.name}'s avoid range!";
#endif
            }
        } else {
#if DEBUG_LOG
            debugLog += $"\n{owner.name} cannot move!";
#endif
            if (target is Character) {
                Character targetCharacter = target as Character;
                if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Defend) {
                    targetCharacter.combatComponent.RemoveHostileInRange(owner);
                }
            }
        }
#if DEBUG_LOG
        owner.logComponent.PrintLogIfActive(debugLog);
#endif
        return hasFled;
    }
    public void FlightAll(string reason = "") {
        if (owner.movementComponent.isStationary) {
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"Triggered FLIGHT ALL response for {owner.name} but character is STATIONARY, cannot flee");
#endif
            return;
        }
        //Demons no longer trigger Flight
        //https://trello.com/c/D4bdwPhH/1104-demons-and-monsters-no-longer-trigger-flight
        if (owner.race == RACE.DEMON || owner.race == RACE.DRAGON) {
            return;
        }
#if DEBUG_LOG
        string debugLog = $"Triggered FLIGHT ALL response for {owner.name}";
#endif
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
            } else {
                for (int i = 0; i < hostilesInRange.Count; i++) {
                    IPointOfInterest hostile = hostilesInRange[i];
                    if (hostile is Character) {
                        Character targetCharacter = hostile as Character;
                        if (targetCharacter.combatComponent.combatMode == COMBAT_MODE.Defend) {
                            targetCharacter.combatComponent.RemoveHostileInRange(owner);
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
    //    //if (!IsHostileInRange(poi)) {
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
            if (poi is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(-1);
            } else if (poi is Character targetCharacter) {
                AddPOIToBannedFromHostile(targetCharacter);
            }
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"{poi.name} was removed from {owner.name}'s hostile range.");
#endif
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
    public bool GetCurrentTargetCombatLethality() {
        bool isLethal = true;
        if (isInCombat) {
            CombatState state = owner.stateComponent.currentState as CombatState;
            if (state.currentClosestHostile != null && state.currentClosestHostile is Character currentHostileTarget) {
                if (!IsLethalCombatForTarget(currentHostileTarget)) {
                    isLethal = false;
                    return isLethal;
                }
            }
        }
        if (hostilesInRange.Count > 0) {
            for (int i = 0; i < hostilesInRange.Count; i++) {
                IPointOfInterest poi = hostilesInRange[i];
                if(poi is Character hostile) {
                    if (!IsLethalCombatForTarget(hostile)) {
                        isLethal = false;
                        return isLethal;
                    }
                    break;
                }
            }
        }
        return isLethal;
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
    //    if (IsHostileInRange(token)) {
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
                    if (inVision.combatComponent.IsHostileInRange(hostile)) {
                        return true;
                    }
                }
                
            }
        }
        return false;
    }
    public bool IsCurrentlyAttackingFriendlyWith(Character p_character) {
        if (isInCombat) {
            CombatState state = owner.stateComponent.currentState as CombatState;
            if(state.currentClosestHostile != null && state.currentClosestHostile is Character currentHostileTarget) {
                if (currentHostileTarget.faction != null && p_character.faction != null && currentHostileTarget.faction.IsFriendlyWith(p_character.faction)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsCurrentlyAttackingDemonicStructure() {
        if (isInCombat) {
            CombatState state = owner.stateComponent.currentState as CombatState;
            if (state.currentClosestHostile != null && state.currentClosestHostile is TileObject t) {
                if ((t.gridTileLocation != null && t.gridTileLocation.structure.structureType.IsPlayerStructure()) || t.tileObjectType.IsDemonicStructureTileObject()) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsCurrentlyAttackingPartyMateOf(Character p_character) {
        if (isInCombat) {
            CombatState state = owner.stateComponent.currentState as CombatState;
            if (state.currentClosestHostile != null && state.currentClosestHostile is Character currentHostileTarget) {
                if (p_character.partyComponent.hasParty && currentHostileTarget.partyComponent.IsAMemberOfParty(p_character.partyComponent.currentParty)) {
                    return true;
                }
            }
        }
        return false;
    }
    public bool IsHostileInRange(IPointOfInterest p_poi) {
        return hostilesInRange.Contains(p_poi);
    }
#endregion

#region Avoid
    private bool AddAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true, string reason = "") {
        if (owner.limiterComponent.canMove) {
        //if (!poi.isDead && !poi.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && character.traitContainer.GetNormalTrait<Trait>("Berserked") == null) {
            if (!IsAvoidInRange(poi)) {
                avoidInRange.Add(poi);
                SetWillProcessCombat(true);
                //avoidReason = reason;
                return true;
            }
        }
        return false;
    }
    public bool RemoveAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (avoidInRange.Remove(poi)) {
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
#if DEBUG_LOG
                owner.logComponent.PrintLogIfActive($"{poi.name} was removed from {owner.name}'s avoid range!");
#endif
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
            }
            return true;
        }
        return false;
    }
    public void RemoveAvoidInRangeSchedule(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (IsAvoidInRange(poi)) {
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
        if (IsHostileInRange(poi)) {
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
#if DEBUG_LOG
            owner.logComponent.PrintLogIfActive($"{owner.name} cleared avoid range!");
#endif
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
                //if (owner.combatComponent.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
            }
        }
    }
    public bool IsAvoidInRange(IPointOfInterest p_poi) {
        return avoidInRange.Contains(p_poi);
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
        if(target != null) {
            CombatData combatData = GetCombatData(target);
            if (combatData != null) {
                key = combatData.reasonForCombat;
                if (key == CombatManager.Action) {
                    key = GetCombatActionReason(combatData.connectedAction, target);
                } else {
                    if (key == CombatManager.Anger) {
                        if (owner.traitContainer.HasTrait("Angry")) {
                            Trait trait = owner.traitContainer.GetTraitOrStatus<Trait>("Angry");
                            if (trait.IsResponsibleForTrait(target as Character)) {
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
        }
        return key;
    }
    public string GetCombatStateIconString(IPointOfInterest target) {
        string iconString = GoapActionStateDB.Hostile_Icon;
        if(target != null) {
            CombatData combatData = GetCombatData(target);
            if (combatData != null && combatData.connectedAction != null) {
                iconString = GetCombatStateIconString(target, combatData.connectedAction);
            }
        }
        return iconString;
    }
    public string GetCombatStateIconString(IPointOfInterest target, ActualGoapNode action) {
        string iconString = GoapActionStateDB.Hostile_Icon;
        switch (action.associatedJobType) {
            case JOB_TYPE.MONSTER_ABDUCT:
            case JOB_TYPE.RITUAL_KILLING:
            case JOB_TYPE.CAPTURE_CHARACTER:
            case JOB_TYPE.KIDNAP_RAID:
                iconString = GoapActionStateDB.Stealth_Icon;
                break;
            case JOB_TYPE.PRODUCE_FOOD:
            case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
            case JOB_TYPE.MONSTER_BUTCHER:
                iconString = GoapActionStateDB.Butcher_Icon;
                break;
            case JOB_TYPE.RESTRAIN:
            case JOB_TYPE.APPREHEND:
                iconString = GoapActionStateDB.Restrain_Icon;
                break;
            case JOB_TYPE.BERSERK_ATTACK:
            case JOB_TYPE.BRAWL:
            case JOB_TYPE.DESTROY:
                iconString = GoapActionStateDB.Anger_Icon;
                break;
        }
        return iconString;
    }
    public string GetCombatActionReason(ActualGoapNode action, IPointOfInterest target) {
       if(action != null) {
            switch (action.associatedJobType) {
                case JOB_TYPE.RESTRAIN:
                    return "Restrain";
                case JOB_TYPE.PRODUCE_FOOD:
                case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
                case JOB_TYPE.MONSTER_BUTCHER:
                    return "Butcher";
                case JOB_TYPE.APPREHEND:
                    return CombatManager.Apprehend;
                case JOB_TYPE.RITUAL_KILLING:
                    return "Ritual Killing";
                case JOB_TYPE.BERSERK_ATTACK:
                    return "Berserked";
                case JOB_TYPE.BRAWL:
                    return "Snapped";
                case JOB_TYPE.DESTROY:
                    if (target is Guitar guitar && guitar.IsOwnedBy(owner)) {
                        if (owner.traitContainer.HasTrait("Music Hater")) {
                            return "Destroy_Music_Hater";
                        }
                    } else if (target is CultistKit) {
                        return "Destroy_Cultist_Kit";
                    }
                    if (owner.traitContainer.HasTrait("Angry")) {
                        return "Destroy_Angry";
                    } else if (owner.traitContainer.HasTrait("Suspicious")) {
                        return "Destroy_Suspicious";
                    }
                    break;
                case JOB_TYPE.MONSTER_ABDUCT:
                case JOB_TYPE.CAPTURE_CHARACTER:
                    return CombatManager.Abduct;
                case JOB_TYPE.KIDNAP_RAID:
                    return CombatManager.Raid;
                case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
                case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
                case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
                    return CombatManager.Fullness_Recovery;
                case JOB_TYPE.SNATCH:
                case JOB_TYPE.SNATCH_RESTRAIN:
                    return CombatManager.Snatch;
            }
       }
       return string.Empty;
    }
    private string GetRetaliationReason(IPointOfInterest target, CombatData combatData) {
        //Get reason of the target towards this character to know why target attacked this character in the first place
        //Because if the reason is Retaliation we assume that the target attacked this character first
        if(target is Character targetCharacter) {
            CombatData targetCombatData = targetCharacter.combatComponent.GetCombatData(owner);
            if(targetCombatData != null) {
                string reason = targetCombatData.reasonForCombat;
                if (!string.IsNullOrEmpty(reason)) {
                    string key = string.Empty;
                    if (reason == CombatManager.Action) {
                        key = targetCharacter.combatComponent.GetCombatActionReason(targetCombatData.connectedAction, owner);
                    }
                    if (key == CombatManager.Apprehend) {
                        return CombatManager.Resisting_Arrest;
                    } else if (key == CombatManager.Abduct) {
                        return CombatManager.Resisting_Abduction;
                    }
                }
            }
        }
        return CombatManager.Defending_Self;
    }
    private string GetHostilityReason(IPointOfInterest target, CombatData combatData) {
        Character targetCharacter = target as Character;
        if (owner.partyComponent.isMemberThatJoinedQuest) {
            PartyQuest quest = owner.partyComponent.currentParty.currentQuest;
            BaseSettlement targetSettlement = null;
            if (quest is RaidPartyQuest raidQuest) {
                targetSettlement = raidQuest.targetSettlement;
            } else if (quest is DemonRaidPartyQuest demonRaidQuest) {
                targetSettlement = demonRaidQuest.targetSettlement;
            }
            if(targetSettlement != null) {
                if (targetCharacter != null) {
                    if (targetCharacter.homeSettlement == targetSettlement) {
                        return CombatManager.Raid;
                    }
                } else if (target is TileObject) {
                    if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(targetSettlement)) {
                        return CombatManager.Raid;
                    }
                }    
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
                if (targetCharacter.faction.factionType.type == FACTION_TYPE.Vagrants) {
                    return CombatManager.Fighting_Vagrant;
                } else if (targetCharacter.faction.factionType.type == FACTION_TYPE.Wild_Monsters) {
                    if (combatData.isLethal) {
                        return CombatManager.Slaying_Monster;
                    } else {
                        return CombatManager.Incapacitating_Monster;
                    }
                } else if (targetCharacter.faction.factionType.type == FACTION_TYPE.Undead) {
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
    public void UpdateAttack() {
        int modifier = owner.characterClass.attackType == ATTACK_TYPE.PHYSICAL ? strengthModification : intelligenceModification;
        float modifierPercent = owner.characterClass.attackType == ATTACK_TYPE.PHYSICAL ? strengthPercentModification : intelligencePercentModification;

        int modifiedAttack = unModifiedAttack + modifier;
        attack = Mathf.RoundToInt(modifiedAttack * ((modifierPercent / 100f) + 1f));
    }
    public int GetComputedStrength() {
        int baseStrength = 0;
        if (owner.characterClass.attackType == ATTACK_TYPE.PHYSICAL) {
            baseStrength = unModifiedAttack;
        }
        int modifiedStrength = baseStrength + strengthModification;
        int finalStrength = Mathf.RoundToInt(modifiedStrength * ((strengthPercentModification / 100f) + 1f));
        return finalStrength;
    }
    public int GetComputedIntelligence() {
        int baseIntelligence = 0;
        if (owner.characterClass.attackType == ATTACK_TYPE.MAGICAL) {
            baseIntelligence = unModifiedAttack;
        }
        int modifiedIntelligence = baseIntelligence + intelligenceModification;
        int finalIntelligence = Mathf.RoundToInt(modifiedIntelligence * ((intelligencePercentModification / 100f) + 1f));
        return finalIntelligence;
    }
    private void UpdateMaxHP() {
        int modifiedHP = unModifiedMaxHP + maxHPModification;
        maxHP = Mathf.RoundToInt(modifiedHP * ((maxHPPercentModification / 100f) + 1f));
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
        UpdateMaxHPAndProportionateHP();
    }
    public void AdjustMaxHPPercentModifier(float modification) {
        maxHPPercentModification += modification;
        UpdateMaxHPAndProportionateHP();
    }
    public void AdjustAttackModifier(int modification) {
        strengthModification += modification;
        intelligenceModification += modification;
        UpdateAttack();
    }
    public void AdjustAttackPercentModifier(float modification) {
        strengthPercentModification += modification;
        intelligencePercentModification += modification; 
        UpdateAttack();
    }
    public void AdjustStrengthModifier(int modification) {
        strengthModification += modification;
        UpdateAttack();
    }
    public void AdjustStrengthPercentModifier(float modification) {
        strengthPercentModification += modification;
        UpdateAttack();
    }
    public void AdjustIntelligenceModifier(int modification) {
        intelligenceModification += modification;
        UpdateAttack();
    }
    public void AdjustIntelligencePercentModifier(float modification) {
        intelligencePercentModification += modification;
        UpdateAttack();
    }
    public void AdjustCritRate(int modification) {
        critRate += modification;
    }
    #endregion

    #region Prisoner
    private void OnCharacterBecomePrisoner(Prisoner prisoner) {
        if (prisoner.IsConsideredPrisonerOf(owner)) {
            CombatData combatData = GetCombatData(prisoner.owner);
            if(combatData != null && (combatData.reasonForCombat == CombatManager.Hostility || combatData.reasonForCombat == CombatManager.Retaliation)) {
                RemoveHostileInRange(prisoner.owner);
            }
        }
    }
#endregion

#region Killed Characters
    public void AdjustNumOfKilledCharacters(int amount) {
        numOfKilledCharacters += amount;
    }
#endregion

#region Loading
    public void LoadReferences(SaveDataCombatComponent data) {
        for (int i = 0; i < data.hostileCharactersInRange.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.hostileCharactersInRange[i]);
            if (character != null && !IsHostileInRange(character)) {
                hostilesInRange.Add(character);
            }
        }
        for (int i = 0; i < data.hostileTileObjectsInRange.Count; i++) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(data.hostileTileObjectsInRange[i]);
            if (tileObject != null && !IsHostileInRange(tileObject)) {
                hostilesInRange.Add(tileObject);
            }
        }

        for (int i = 0; i < data.avoidCharactersInRange.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.avoidCharactersInRange[i]);
            if (character != null && !IsAvoidInRange(character)) {
                avoidInRange.Add(character);
            }
        }
        for (int i = 0; i < data.avoidTileObjectsInRange.Count; i++) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.avoidTileObjectsInRange[i]);
            if (!IsAvoidInRange(tileObject)) {
                avoidInRange.Add(tileObject);
            }
        }

        foreach (KeyValuePair<string, SaveDataCombatData> item in data.characterCombatData) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(item.Key);
            if (character != null) {
                CombatData combatData = item.Value.Load();
                combatDataDictionary.Add(character, combatData);
            }
        }
        foreach (KeyValuePair<string, SaveDataCombatData> item in data.tileObjectCombatData) {
            TileObject tileObject = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentIDSafe(item.Key);
            if (tileObject != null) {
                CombatData combatData = item.Value.Load();
                combatDataDictionary.Add(tileObject, combatData);    
            }
        }

        for (int i = 0; i < data.bannedFromHostileList.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.bannedFromHostileList[i]);
            if (character != null && !bannedFromHostileList.Contains(character)) {
                bannedFromHostileList.Add(character);
            }
        }
        //initialElementalType = data.initialElementalDamageType;
        elementalStatusWaitingList = new List<ELEMENTAL_TYPE>();
        data.elementalStatusWaitingList.ForEach((eachElem) => elementalStatusWaitingList.Add(eachElem));
        combatBehaviourParent.LoadReferences(data.combatBehaviourParent);
        specialSkillParent.LoadReferences();
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
        base.Save(data);
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
    //public int strength;
    //public int intelligence;
    public int strengthModification;
    public float strengthPercentModification;
    public int intelligenceModification;
    public float intelligencePercentModification;
    public int maxHP;
    public int maxHPModification;
    public float maxHPPercentModification;
    public int attackSpeed;
    public int numOfKilledCharacters;

    public COMBAT_MODE combatMode;
    public List<string> hostileCharactersInRange;
    public List<string> hostileTileObjectsInRange;

    public List<string> avoidCharactersInRange;
    public List<string> avoidTileObjectsInRange;

    public List<string> bannedFromHostileList;

    public Dictionary<string, SaveDataCombatData> characterCombatData;
    public Dictionary<string, SaveDataCombatData> tileObjectCombatData;

    public ELEMENTAL_TYPE elementalDamageType;
    public ELEMENTAL_TYPE initialElementalDamageType;
    public List<ELEMENTAL_TYPE> elementalStatusWaitingList = new List<ELEMENTAL_TYPE>();
    public SaveDataCharacterCombatBehaviourParent combatBehaviourParent;
    public SaveDataCombatSpecialSkillWrapper specialSkillParent;

    public bool willProcessCombat;
    public int critRate;

#region Overrides
    public override void Save(CombatComponent data) {
        attack = data.attack;
        //strength = data.strength;
        strengthModification = data.strengthModification;
        critRate = data.critRate;
        strengthPercentModification = data.strengthPercentModification;
        //intelligence = data.intelligence;
        intelligenceModification = data.intelligenceModification;
        intelligencePercentModification = data.intelligencePercentModification; 
        maxHP = data.maxHP;
        maxHPModification = data.maxHPModification;
        maxHPPercentModification = data.maxHPPercentModification;
        attackSpeed = data.attackSpeed;
        combatMode = data.combatMode;
        //initialElementalDamageType = data.initialElementalType;
        data.elementalStatusWaitingList.ForEach((eachElem) => elementalStatusWaitingList.Add(eachElem));
        elementalDamageType = data.elementalDamage.type;
        willProcessCombat = data.willProcessCombat;
        numOfKilledCharacters = data.numOfKilledCharacters;

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

        combatBehaviourParent = new SaveDataCharacterCombatBehaviourParent();
        combatBehaviourParent.Save(data.combatBehaviourParent);

        specialSkillParent = new SaveDataCombatSpecialSkillWrapper(); 
        specialSkillParent.Save(data.specialSkillParent);
    }

    public override CombatComponent Load() {
        CombatComponent component = new CombatComponent(this);
        return component;
    }
#endregion
}