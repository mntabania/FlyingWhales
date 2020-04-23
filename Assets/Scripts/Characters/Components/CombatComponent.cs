using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class CombatComponent {
	public Character owner { get; private set; }
    public COMBAT_MODE combatMode { get; private set; }
    public List<IPointOfInterest> hostilesInRange { get; private set; } //POI's in this characters hostility collider
    public List<IPointOfInterest> avoidInRange { get; private set; } //POI's in this characters hostility collider
    public Dictionary<Character, bool> lethalCharacters { get; private set; }
    public string avoidReason { get; private set; }
    public ElementalDamageData elementalDamage { get; private set; }
    public ActualGoapNode actionThatTriggeredCombatState { get; private set; }
    public GoapPlanJob jobThatTriggeredCombatState { get; private set; }
    // public ActualGoapNode combatConnectedActionNode { get; private set; }

    //delegates
    public delegate void OnProcessCombat(CombatState state);
    private OnProcessCombat onProcessCombat; //actions to be executed and cleared when a character processes combat.

    private bool _willProcessCombat;

    public CombatComponent(Character owner) {
		this.owner = owner;
        hostilesInRange = new List<IPointOfInterest>();
        avoidInRange = new List<IPointOfInterest>();
        lethalCharacters = new Dictionary<Character, bool>();
        SetCombatMode(COMBAT_MODE.Aggressive);
        SetElementalType(ELEMENTAL_TYPE.Normal);
	}

    #region Fight or Flight
    public void FightOrFlight(IPointOfInterest target, bool isLethal = true) {
        string debugLog = $"FIGHT or FLIGHT response of {owner.name} against {target.nameWithID}";
        if (!owner.canPerform || !owner.canMove) {
            debugLog += "\n-Character cannot move/perform, will not fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
            return;
        }
        if (hostilesInRange.Contains(target) || avoidInRange.Contains(target)) {
            debugLog += "\n-Target is already in hostile/avoid list, will no longer trigger fight or flight";
            owner.logComponent.PrintLogIfActive(debugLog);
            return;
        }
        if (owner.faction == FactionManager.Instance.zombieFaction || owner.race == RACE.SKELETON) {
            debugLog += "\n-Character is zombie";
            debugLog += "\n-FIGHT";
            owner.logComponent.PrintLogIfActive(debugLog);
            Fight(target, isLethal);
            return;
        }
        if (target is Character) {
            debugLog += "\n-Target is character";
            Character targetCharacter = target as Character;
            if (owner.traitContainer.HasTrait("Coward")) {
                debugLog += "\n-Character is coward";
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                Flight(target, "character is a coward");
            } else {
                debugLog += "\n-Character is not coward";
                if (!owner.traitContainer.HasTrait("Combatant")) {
                    debugLog += "\n-Character is not combatant, 20% to Fight";
                    int chance = UnityEngine.Random.Range(0, 100);
                    debugLog += $"\n-Roll: {chance}";
                    if (chance < 20) {
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        Fight(target, isLethal);
                    } else {
                        debugLog += "\n-FLIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        Flight(target, "got scared");
                    }
                } else {
                    debugLog += "\n-Character is combatant";
                    if (owner.currentHP > targetCharacter.currentHP) {
                        debugLog += "\n-Character hp is higher than target";
                        debugLog += "\n-FIGHT";
                        owner.logComponent.PrintLogIfActive(debugLog);
                        Fight(target, isLethal);
                    } else {
                        debugLog += "\n-Character hp is lower or equal than target";
                        if (CombatManager.Instance.IsImmuneToElement(targetCharacter, elementalDamage.type)) {
                            debugLog += "\n-Target is immune to character elemental damage";
                            Flight(target, "got scared");
                        } else if (CombatManager.Instance.IsImmuneToElement(owner, targetCharacter.combatComponent.elementalDamage.type)) {
                            debugLog += "\n-Character is immune to target elemental damage";
                            Fight(target, isLethal);
                        } else {
                            if (owner.currentHP >= Mathf.CeilToInt(owner.maxHP * 0.5f)) {
                                debugLog += "\n-Character's hp is greater than or equal to 50% of its max hp";
                                Fight(target, isLethal);
                            } else {
                                int fightChance = 25;
                                for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                                    if (owner.marker.inVisionCharacters[i].combatComponent.hostilesInRange.Contains(target)) {
                                        debugLog += "\n-Character has another character in vision who has the same target";
                                        fightChance = 75;
                                        break;
                                    }
                                }
                                debugLog += $"\n-Fight chance: {fightChance}";
                                int roll = UnityEngine.Random.Range(0, 100);
                                debugLog += $"\n-Roll: {roll}";
                                if (roll < fightChance) {
                                    debugLog += "\n-FIGHT";
                                    owner.logComponent.PrintLogIfActive(debugLog);
                                    Fight(target, isLethal);
                                } else {
                                    debugLog += "\n-FLIGHT";
                                    owner.logComponent.PrintLogIfActive(debugLog);
                                    Flight(target, "got scared");
                                }
                            }
                        }
                    }
                }
            }
        } else if (target is TileObject tileObject) {
            debugLog += "\n-Target is object";
            if (owner.traitContainer.HasTrait("Coward")) {
                debugLog += "\n-Character is coward";
                debugLog += "\n-FLIGHT";
                owner.logComponent.PrintLogIfActive(debugLog);
                Flight(target, "character is a coward");
            } else if (tileObject.traitContainer.HasTrait("Dangerous")) {
                debugLog += "\n-Object is dangerous";
                if (string.IsNullOrEmpty(tileObject.neutralizer) == false && 
                    owner.traitContainer.HasTrait(tileObject.neutralizer)) {
                    debugLog += $"\n-Character has neutralizer trait {tileObject.neutralizer}";    
                    Fight(target, isLethal);
                } else {
                    Flight(target, "got scared");
                }
                owner.logComponent.PrintLogIfActive(debugLog);
            } else {
                debugLog += "\n-Object is not dangerous";
                Fight(target, isLethal);
                owner.logComponent.PrintLogIfActive(debugLog);
            }
        }
    }
    public bool Fight(IPointOfInterest target, bool isLethal = true) {
        bool hasFought = false;
        if (!hostilesInRange.Contains(target)) {
            string debugLog = $"Triggered FIGHT response for {owner.name} against {target.nameWithID}";
            hostilesInRange.Add(target);
            avoidInRange.Remove(target);
            SetWillProcessCombat(true);
            if (target is Character targetCharacter) {
                lethalCharacters.Add(targetCharacter, isLethal);
            } else if (target is TileObject targetTileObject) {
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
        bool hasFled = false;
        if (hostilesInRange.Remove(target)) {
            if (target.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                lethalCharacters.Remove(target as Character);
            } else if (target is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(-1);
            }
        }
        if (!avoidInRange.Contains(target)) {
            string debugLog = $"Triggered FLIGHT response for {owner.name} against {target.nameWithID}";
            if (owner.marker.inVisionPOIs.Contains(target)) {
                avoidInRange.Add(target);
                SetWillProcessCombat(true);
                avoidReason = reason;
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
        if (hostilesInRange.Count > 0) {
            if (owner.canMove) {
                for (int i = 0; i < hostilesInRange.Count; i++) {
                    IPointOfInterest hostile = hostilesInRange[i];
                    if (owner.marker.inVisionPOIs.Contains(hostile)) {
                        avoidInRange.Add(hostile);
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
            avoidReason = reason;
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
    public void RemoveHostileInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (hostilesInRange.Remove(poi)) {
            if (poi is Character character) {
                lethalCharacters.Remove(character);
            } else if (poi is TileObject targetTileObject) {
                targetTileObject.AdjustRepairCounter(-1);
            }
            string removeHostileSummary = $"{poi.name} was removed from {owner.name}'s hostile range.";
            owner.logComponent.PrintLogIfActive(removeHostileSummary);
            //When removing hostile in range, check if character is still in combat state, if it is, reevaluate combat behavior, if not, do nothing
            if (processCombatBehavior) {
                if (owner.isInCombat) {
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
        }
    }
    public void ClearHostilesInRange(bool processCombatBehavior = true) {
        if (hostilesInRange.Count > 0) {
            for (int i = 0; i < hostilesInRange.Count; i++) {
                if (hostilesInRange[i] is TileObject targetTileObject) {
                    targetTileObject.AdjustRepairCounter(-1);
                }
            }
            hostilesInRange.Clear();
            lethalCharacters.Clear();
            //When adding hostile in range, check if character is already in combat state, if it is, only reevaluate combat behavior, if not, enter combat state
            if (processCombatBehavior) {
                //if (owner.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
                SetWillProcessCombat(true);
            }
        }
    }
    public bool IsLethalCombatForTarget(Character character) {
        if (lethalCharacters.ContainsKey(character)) {
            return lethalCharacters[character];
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
            if (poi.IsValidCombatTarget()) {
                float dist = Vector2.Distance(owner.marker.transform.position, poi.worldPosition);
                if (nearest == null || dist < nearestDist) {
                    nearest = poi;
                    nearestDist = dist;
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
                IPointOfInterest pointOfInterest = hostilesInRange[i];
                if(pointOfInterest.IsValidCombatTarget() && 
                   pointOfInterest.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    nearest = hostilesInRange[i];
                    break;
                }
            }
        }
        return nearest;
    }
    //public void OnItemRemovedFromTile(SpecialToken token, LocationGridTile removedFrom) {
    //    if (hostilesInRange.Contains(token)) {
    //        RemoveHostileInRange(token);
    //    }
    //}
    #endregion

    #region Avoid
    private bool AddAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true, string reason = "") {
        if (owner.canMove) {
        //if (!poi.isDead && !poi.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) && character.traitContainer.GetNormalTrait<Trait>("Berserked") == null) {
            if (!avoidInRange.Contains(poi)) {
                avoidInRange.Add(poi);
                SetWillProcessCombat(true);
                avoidReason = reason;
                return true;
            }
        }
        return false;
    }
    public void RemoveAvoidInRange(IPointOfInterest poi, bool processCombatBehavior = true) {
        if (avoidInRange.Remove(poi)) {
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
                //if (owner.isInCombat) {
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
            if (!owner.marker.inVisionPOIs.Contains(poi)) {
                RemoveAvoidInRange(poi, processCombatBehavior);
            }
        }
    }
    public void ClearAvoidInRange(bool processCombatBehavior = true) {
        if (avoidInRange.Count > 0) {
            avoidInRange.Clear();
            if (processCombatBehavior) {
                SetWillProcessCombat(true);
                //if (owner.isInCombat) {
                //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
                //}
            }
        }
    }
    #endregion

    #region General
    public void OnThisCharacterEndedCombatState() {
        SetOnProcessCombatAction(null);
    }
    private void ProcessCombatBehavior() {
        string log = $"{owner.name} process combat switch is turned on, processing combat...";
        if (owner.interruptComponent.isInterrupted) {
            log +=
                $"\n-Character is interrupted: {owner.interruptComponent.currentInterrupt.name}, will not process combat";
        } else {
            if (owner.isInCombat) {
                log += "\n-Character is already in combat, determining combat action to do";
                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
            } else {
                log += "\n-Character is not in combat, will add Combat job if there is a hostile or avoid in range";
                if (hostilesInRange.Count > 0 || avoidInRange.Count > 0) {
                    log += "\n-Combat job added";
                    CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, owner);
                    owner.jobQueue.AddJobInQueue(job);
                } else {
                    log += "\n-Combat job not added";
                    if (owner.marker.hasFleePath && owner.isInCombat) {
                        CombatState combatState = owner.stateComponent.currentState as CombatState;
                        combatState.CheckFlee(ref log);
                    }
                }
            }
            avoidReason = string.Empty;
        }
        owner.logComponent.PrintLogIfActive(log);
        //execute any external combat actions. This assumes that this character entered combat state.
        
        //NOTE: Commented this out temporarily because we no longer immediately switch the state of the character to combat, instead we create a job and add it to its job queue
        //This means that the character will always have a null current state in this tick
        //onProcessCombat?.Invoke(owner.stateComponent.currentState as CombatState);
        //SetOnProcessCombatAction(null);
    }
    public void AddOnProcessCombatAction(OnProcessCombat action) {
        onProcessCombat += action;
    }
    public void SetOnProcessCombatAction(OnProcessCombat action) {
        onProcessCombat = action;
    }
    public void CheckCombatPerTickEnded() {
        if (_willProcessCombat) {
            ProcessCombatBehavior();
            SetWillProcessCombat(false);
        }
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
            if(currTrait.elementalType != ELEMENTAL_TYPE.Normal) {
                SetElementalType(currTrait.elementalType);
                hasSetElementalType = true;
                break;
            }
        }
        if (!hasSetElementalType) {
            SetElementalType(owner.characterClass.elementalType);
        }
    }
    public void SetActionAndJobThatTriggeredCombat(ActualGoapNode node, GoapPlanJob job) {
        actionThatTriggeredCombatState = node;
        jobThatTriggeredCombatState = job;
    }
    public void SetWillProcessCombat(bool state) {
        _willProcessCombat = state;
    }
    #endregion
}
