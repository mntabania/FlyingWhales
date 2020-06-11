using System.Collections.Generic;
using UnityEngine;
using Traits;
using UnityEngine.EventSystems;
using Inner_Maps;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public class CombatState : CharacterState {

    private int _currentAttackTimer; //When this timer reaches max, remove currently hostile target from hostile list
    private bool _hasTimerStarted;

    public bool isAttacking { get; private set; } //if not attacking, it is assumed that the character is fleeing
    public IPointOfInterest currentClosestHostile { get; private set; }
    //public GoapPlanJob jobThatTriggeredThisState { get; private set; }
    //public ActualGoapNode actionThatTriggeredThisState { get; private set; }
    public Character forcedTarget { get; private set; }
    public List<Character> allCharactersThatDegradedRel { get; private set; }
    public List<Character> allCharactersThatReactedToThisCombat { get; private set; }
    public bool endedInternally { get; private set; } //was this combat ended from within this combat state?

    //Is this character fighting another character or has a character in hostile range list who is trying to apprehend him/her because he/she is a criminal?
    //See: https://trello.com/c/uCZfbCSa/2819-criminals-should-eventually-flee-npcSettlement-and-leave-faction
    public bool isBeingApprehended { get; private set; }
    private int fleeChance;
    private bool isFleeToHome;

    public CombatState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Combat State";
        characterState = CHARACTER_STATE.COMBAT;
        //stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _currentAttackTimer = 0;
        fleeChance = 10;
        //Default start of combat state is attacking
        isAttacking = true;
        allCharactersThatDegradedRel = new List<Character>();
        allCharactersThatReactedToThisCombat = new List<Character>();
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        //stateComponent.character.combatComponent.SetWillProcessCombat(true);
        stateComponent.character.combatComponent.SetWillProcessCombat(false);
        StartCombatMovement();
    }
    //public override void PerTickInState() {
    //    //if (currentClosestHostile != null && !PathfindingManager.Instance.HasPathEvenDiffRegion(stateComponent.character.gridTileLocation, currentClosestHostile.gridTileLocation)) {
    //    //    SetClosestHostile(null);
    //    //    stateComponent.character.combatComponent.RemoveHostileInRange(currentClosestHostile);
    //    //    return;
    //    //}
    //    if (_hasTimerStarted) {
    //        //timer has been started, increment timer
    //        _currentAttackTimer += 1;
    //        if (_currentAttackTimer >= CombatManager.pursueDuration) {
    //            StopPursueTimer();
    //            //When pursue timer reaches max, character must remove the current closest hostile in hostile list, then stop pursue timer
    //            stateComponent.character.combatComponent.RemoveHostileInRange(currentClosestHostile);
    //        }
    //    } else {
    //        //If character is pursuing the current closest hostile, check if that hostile is in range, if it is, start pursue timer
    //        //&& stateComponent.character.currentParty.icon.isTravelling
    //        if (isAttacking && currentClosestHostile != null && stateComponent.character.marker.targetPOI == currentClosestHostile &&
    //            stateComponent.character.marker.inVisionPOIs.Contains(currentClosestHostile)) {
    //            StartPursueTimer();
    //        }
    //    }
    //}
    protected override void StartState() {
        stateComponent.character.logComponent.PrintLogIfActive(
            $"Starting combat state for {stateComponent.character.name}");
        //stateComponent.character.DecreaseCanWitness();
        stateComponent.character.marker.ShowHPBar(stateComponent.character);
        stateComponent.character.marker.SetAnimationBool("InCombat", true);
        stateComponent.character.marker.visionCollider.VoteToUnFilterVision();
        //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, stateComponent.character, "combat");
        Messenger.AddListener<Character>(Signals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.AddListener<Character>(Signals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
        Messenger.AddListener<Character>(Signals.START_FLEE, OnCharacterStartFleeing);
        base.StartState();
        //if (stateComponent.character.currentActionNode is Assault && !stateComponent.character.currentActionNode.isPerformingActualAction) {
        //    stateComponent.character.currentActionNode.Perform(); //this is for when a character will assault a target, but his/her attack range is less than his/her vision range. (Because end reached distance of assault action is set to attack range)
        //}
        //stateComponent.character.StopCurrentActionNode(false);
        stateComponent.character.UncarryPOI(); //Drop characters when entering combat
        // if(stateComponent.character is SeducerSummon) { //If succubus/incubus enters a combat, automatically change its faction to the player faction if faction is still disguised
        //     if(stateComponent.character.faction == FactionManager.Instance.disguisedFaction) {
        //         stateComponent.character.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
        //     }
        // }
        // stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
    }
    protected override void EndState() {
        stateComponent.character.marker.pathfindingAI.ClearAllCurrentPathData();
        stateComponent.character.marker.SetHasFleePath(false);

        //stateComponent.character.IncreaseCanWitness();
        // stateComponent.character.marker.StopCoroutine(CheckIfCurrentHostileIsInRange());

        stateComponent.character.marker.HideHPBar();
        stateComponent.character.marker.SetAnimationBool("InCombat", false);
        stateComponent.character.marker.visionCollider.VoteToFilterVision();
        stateComponent.character.logComponent.PrintLogIfActive(
            $"Ending combat state for {stateComponent.character.name}");
        Messenger.RemoveListener<Character>(Signals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.RemoveListener<Character>(Signals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
        Messenger.RemoveListener<Character>(Signals.START_FLEE, OnCharacterStartFleeing);
        base.EndState();
    }
    //public override void OnExitThisState() {
    //    stateComponent.character.marker.pathfindingAI.ClearAllCurrentPathData();
    //    stateComponent.character.marker.SetHasFleePath(false);
    //    base.OnExitThisState();
    //}
    //public override void SetOtherDataOnStartState(object otherData) {
    //    //Notice I didn't call the SetIsAttackingState because I only want to change the value of the boolean, I do not want to process the combat behavior
    //    if(otherData != null) {
    //        isAttacking = (bool) otherData;
    //    }
    //}
    public override void AfterExitingState() {
        base.AfterExitingState();
        if (!stateComponent.character.isDead) {
            if (isBeingApprehended && stateComponent.character.traitContainer.HasTrait("Criminal") && stateComponent.character.canPerform && stateComponent.character.canMove) { //!stateComponent.character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                //stateComponent.character.traitContainer.RemoveTrait(stateComponent.character, "Criminal");

                //If this criminal character is being apprehended and survived (meaning he did not die, or is not unconscious or restrained)
                if (!stateComponent.character.isFriendlyFactionless) {
                    //Leave current faction and become banned from the current faction
                    if(stateComponent.character.faction != null) {
                        stateComponent.character.faction.AddBannedCharacter(stateComponent.character);
                    }
                    stateComponent.character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
                }
                stateComponent.character.MigrateHomeStructureTo(null);

                string log =
                    $"{stateComponent.character.name} is a criminal and survived being apprehended. Changed faction to: {stateComponent.character.faction.name} and home to: null";
                stateComponent.character.logComponent.PrintLogIfActive(log);

                //stateComponent.character.CancelAllJobsAndPlans();
                //stateComponent.character.PlanIdleReturnHome(true);
                //stateComponent.character.defaultCharacterTrait.SetHasSurvivedApprehension(true);

                Log successfulEscapeLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "successful_escape_crime");
                successfulEscapeLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                successfulEscapeLog.AddLogToInvolvedObjects();
                PlayerManager.Instance.player.ShowNotificationFrom(stateComponent.character, successfulEscapeLog);
                return;
            }

            //Made it so that dead characters no longer check invision characters after exiting a state.
            for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest currPOI = stateComponent.character.marker.inVisionPOIs[i];
                if (!stateComponent.character.marker.unprocessedVisionPOIs.Contains(currPOI)) {
                    // stateComponent.character.CreateJobsOnEnterVisionWith(currCharacter);
                    stateComponent.character.marker.AddUnprocessedPOI(currPOI);
                }
            }
            stateComponent.character.needsComponent.CheckExtremeNeeds();
        }
        stateComponent.character.combatComponent.ClearCombatData();
    }
    #endregion

    /// <summary>
    /// Function that determines what a character should do in a certain point in time.
    /// Can be triggered by broadcasting signal <see cref="Signals.DETERMINE_COMBAT_REACTION"/>
    /// </summary>
    /// <param name="character">The character that should determine a reaction.</param>
    private void DetermineReaction(Character character) {
        if (stateComponent.character == character && stateComponent.currentState == this && !isPaused && !isDone) {
            DetermineIsBeingApprehended();
            string summary = $"{character.name} will determine a combat reaction";
            if (character.marker.hasFleePath) {
                summary += "\n-Has flee path";
                //Character is already fleeing
                CheckFlee(ref summary);
            } else {
                summary += "\n-Has no flee path";
                if (HasStillAvoidPOIThatIsInRange()) {
                    summary += "\n-Has avoid that is still in range";
                    if (character.homeStructure != null) {
                        summary += "\n-Has home dwelling";
                        if (character.homeStructure == character.currentStructure) {
                            summary += "\n-Is in Home Dwelling";
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                summary += "\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
                            } else {
                                summary += "\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            }
                        } else {
                            summary += "\n-Is not in Home Dwelling, 40%: Flee to Home, 40%: Flee, 20%: Cowering";
                            int roll = UnityEngine.Random.Range(0, 100);
                            summary += $"\n-Roll: {roll}";
                            if (roll < 40) {
                                summary += "\n-Triggered Flee to Home";
                                SetIsFleeToHome(true);
                                SetIsAttacking(false);
                            } else if (roll >= 40 && roll < 80) {
                                summary += "\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            } else {
                                summary += "\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
                            }
                        }
                    } else if (character is Summon && (character as Summon).HasTerritory()) {
                        summary += "\n-Has territory";
                        Summon summon = character as Summon;
                        if (summon.IsInTerritory()) {
                            summary += "\n-Is in territory";
                            if (UnityEngine.Random.Range(0, 2) == 0) {
                                summary += "\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
                            } else {
                                summary += "\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            }
                        } else {
                            summary += "\n-Is not in territory, 40%: Flee to territory, 40%: Flee, 20%: Cowering";
                            int roll = UnityEngine.Random.Range(0, 100);
                            summary += $"\n-Roll: {roll}";
                            if (roll < 40) {
                                summary += "\n-Triggered Flee to territory";
                                SetIsFleeToHome(true);
                                SetIsAttacking(false);
                            } else if (roll >= 40 && roll < 80) {
                                summary += "\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            } else {
                                summary += "\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
                            }
                        }
                    } else {
                        summary += "\n-Has no home dwelling nor territory";
                        if (UnityEngine.Random.Range(0, 2) == 0) {
                            summary += "\n-Triggered Cowering";
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character);
                        } else {
                            summary += "\n-Triggered Flee";
                            SetIsFleeToHome(false);
                            SetIsAttacking(false);
                        }
                    }
                } else if (character.combatComponent.hostilesInRange.Count > 0) {
                    summary += "\n-Has hostile in list";
                    summary += "\n-Attack nereast one";
                    //SetClosestHostile(null);
                    SetIsAttacking(true);
                } else {
                    summary += "\n-Has no hostile or avoid in list";
                    summary += "\n-Exiting combat state";
                    character.combatComponent.ClearAvoidInRange(false);
                    endedInternally = true;
                    character.stateComponent.ExitCurrentState();
                }
            }
            character.logComponent.PrintLogIfActive(summary);
            //if (stateComponent.character.combatComponent.hostilesInRange.Count > 0) {
            //    summary += "\nStill has hostiles, will attack...";
            //    stateComponent.character.logComponent.PrintLogIfActive(summary);
            //    SetIsAttacking(true);
            //} else if (stateComponent.character.combatComponent.avoidInRange.Count > 0) {
            //    summary += "\nStill has characters to avoid, checking if those characters are still in range...";
            //    for (int i = 0; i < stateComponent.character.combatComponent.avoidInRange.Count; i++) {
            //        IPointOfInterest currAvoid = stateComponent.character.combatComponent.avoidInRange[i];
            //        if (!stateComponent.character.marker.inVisionPOIs.Contains(currAvoid) 
            //            && !stateComponent.character.marker.visionCollision.poisInRangeButDiffStructure.Contains(currAvoid)) {
            //            //I added checking for poisInRangeButDiffStructure beacuse characters are being removed from the character's avoid range when they exit a structure. (Myk)
            //            OnFinishedFleeingFrom(currAvoid);
            //            stateComponent.character.combatComponent.RemoveAvoidInRange(currAvoid, false);
            //            i--;
            //        }
            //    }
            //    if (stateComponent.character.combatComponent.avoidInRange.Count > 0) {
            //        summary += "\nStill has characters to avoid in range, fleeing...";
            //        stateComponent.character.logComponent.PrintLogIfActive(summary);
            //        SetIsAttacking(false);
            //    } else {
            //        summary += "\nNo more hostile or avoid characters, exiting combat state...";
            //        stateComponent.character.logComponent.PrintLogIfActive(summary);
            //        stateComponent.ExitCurrentState();
            //    }
            //} else {
            //    summary += "\nNo more hostile or avoid characters, exiting combat state...";
            //    stateComponent.character.logComponent.PrintLogIfActive(summary);
            //    stateComponent.ExitCurrentState();
            //}
        }
    }
    private void OnUpdateMovementState(Character character) {
        Character owner = stateComponent.character;
        //Will stop pursuing only if current closest hostile is character, if current closest hostile is an object, whether or not the source can run, he/she will still pursue
        if (character == owner && stateComponent.currentState == this && !isPaused && !isDone && currentClosestHostile != null && currentClosestHostile is Character targetCharacter && isAttacking) {
            if(targetCharacter.isInCombat && !(targetCharacter.stateComponent.currentState as CombatState).isAttacking) {
                if (!owner.movementComponent.CanStillPursueTarget(targetCharacter)) {
                    if(owner.combatComponent.fightCombatData.ContainsKey(currentClosestHostile) && owner.combatComponent.fightCombatData[currentClosestHostile].reasonForCombat == CombatManager.Demon_Kill) {
                        //If the reason for combat is Demon Kill, the hostile should not be removed from hostile range, regardless if he/she can still run
                    } else {
                        owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
                    }
                }
            }
        }
    }
    private void OnCharacterStartFleeing(Character character) {
        Character owner = stateComponent.character;
        //Will stop pursuing only if current closest hostile is character, if current closest hostile is an object, whether or not the source can run, he/she will still pursue
        if (stateComponent.currentState == this && !isPaused && !isDone && owner.combatComponent.hostilesInRange.Contains(character)) {
            if (!owner.movementComponent.CanStillPursueTarget(character)) {
                if (owner.combatComponent.fightCombatData.ContainsKey(currentClosestHostile) && owner.combatComponent.fightCombatData[currentClosestHostile].reasonForCombat == CombatManager.Demon_Kill) {
                    //If the reason for combat is Demon Kill, the hostile should not be removed from hostile range, regardless if he/she can still run
                } else {
                    owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
                }
            }
        }
    }
    public void CheckFlee(ref string debugLog) {
        if (!HasStillAvoidPOIThatIsInRange()) {
            debugLog += "\n-Has no avoid that is still in range";
            if (HasStillHostilePOIThatIsInRange()) {
                debugLog += "\n-Has hostile that is still in range";
                debugLog += "\n-Attack nearest one";
                //SetClosestHostile(null);
                SetIsAttacking(true);
            } else {
                debugLog += "\n-Has no hostile that is still in range";
                debugLog += $"\n-{fleeChance}% chance to flee";
                int roll = UnityEngine.Random.Range(0, 100);
                debugLog += $"\n-Roll: {roll}"; 
                if (roll < fleeChance) {
                    debugLog += "\n-Stop fleeing";
                    FinishedTravellingFleePath();
                } else {
                    fleeChance += 10;
                    debugLog += $"\n-Flee chance increased by 10%, new flee chance is {fleeChance}";
                }
            }
        }
    }
    private bool HasStillAvoidPOIThatIsInRange() {
        for (int i = 0; i < stateComponent.character.combatComponent.avoidInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.character.combatComponent.avoidInRange[i];
            if (IsStillInRange(poi)) {
                return true;
            }
        }
        return false;
    }
    private bool HasStillHostilePOIThatIsInRange() {
        for (int i = 0; i < stateComponent.character.combatComponent.hostilesInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.character.combatComponent.hostilesInRange[i];
            if (IsStillInRange(poi)) {
                return true;
            }
        }
        return false;
    }
    private bool IsStillInRange(IPointOfInterest poi) {
        //I added checking for poisInRangeButDiffStructure beacuse characters are being removed from the character's avoid range when they exit a structure. (Myk)
        return stateComponent.character.marker.inVisionPOIs.Contains(poi) || stateComponent.character.marker.visionCollider.poisInRangeButDiffStructure.Contains(poi);
    }

    private void SetIsAttacking(bool state) {
        isAttacking = state;
        if (isAttacking) {
            actionIconString = GoapActionStateDB.Hostile_Icon;
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "CharacterState", "Combat State", "thought_bubble");
            thoughtBubbleLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        } else {
            actionIconString = GoapActionStateDB.Flee_Icon;
        }
        stateComponent.character.marker.UpdateActionIcon();
        DoCombatBehavior();
    }
    private void SetIsFleeToHome(bool state) {
        isFleeToHome = state;
    }
    //Determine if this character is being apprehended by one of his hostile/avoid in range
    private void DetermineIsBeingApprehended() {
        if (isBeingApprehended) {
            return;
        }
        for (int i = 0; i < stateComponent.character.combatComponent.hostilesInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.character.combatComponent.hostilesInRange[i];
            if (poi is Character hostile) {
                if (hostile.isInCombat) {
                    CombatData combatData = hostile.combatComponent.GetCombatData(stateComponent.character);
                    if(combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.APPREHEND) {
                        isBeingApprehended = true;
                        return;
                    }
                }
            }
            
        }
        for (int i = 0; i < stateComponent.character.combatComponent.avoidInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.character.combatComponent.avoidInRange[i];
            if (poi is Character hostile) {
                if (hostile.isInCombat) {
                    CombatData combatData = hostile.combatComponent.GetCombatData(stateComponent.character);
                    if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.APPREHEND) {
                        isBeingApprehended = true;
                        return;
                    }
                }
            }
        }
    }
    private void StartCombatMovement() {
        //string log = GameManager.Instance.TodayLogString() + "Starting combat movement for " + stateComponent.character.name;
        //Debug.Log(log);
        //I set the value to its own because I only want to trigger the movement behavior, I do not want to change the boolean value
        //SetIsAttacking(isAttacking);
        DetermineReaction(stateComponent.character);
    }
    //public void SwitchTarget(Character newTarget) {
    //    //stateComponent.character.combatComponent.AddHostileInRange(newTarget, false);
    //    currentClosestHostile = newTarget;
    //    DoCombatBehavior();
    //}
    //Returns true if there is a hostile left, otherwise, returns false
    private void DoCombatBehavior(Character newTarget = null) {
        if(stateComponent.currentState != this) {
            return;
        }
        string log = $"Reevaluating combat behavior of {stateComponent.character.name}";
        if (isAttacking) {
            //stateComponent.character.marker.StopPerTickFlee();
            log += $"\n{stateComponent.character.name} is attacking!";
            if (stateComponent.character.marker && stateComponent.character.marker.hasFleePath) {
                log += $"\n-Still has flee path, force finish flee path";
                stateComponent.character.marker.SetHasFleePath(false);
                fleeChance = 10;
            } else if (!stateComponent.character.marker) {
                log += $"\n-Has no marker!";
            }
            Trait taunted = stateComponent.character.traitContainer.GetNormalTrait<Trait>("Taunted");
            if (forcedTarget != null) {
                log += $"\n{stateComponent.character.name} has a forced target. Setting {forcedTarget.name} as target.";
                SetClosestHostile(forcedTarget);
                SetForcedTarget(null);
            } else if (taunted != null) {
                log +=
                    $"\n{stateComponent.character.name} is taunted. Setting {taunted.responsibleCharacter.name} as target.";
                SetClosestHostile(taunted.responsibleCharacter);
            } else if (currentClosestHostile != null && !stateComponent.character.combatComponent.hostilesInRange.Contains(currentClosestHostile)) {
                log +=
                    $"\nCurrent closest hostile: {currentClosestHostile.name} is no longer in hostile list, setting another closest hostile...";
                SetClosestHostile();
            } else if (currentClosestHostile != null && currentClosestHostile.isDead) {
                log +=
                    $"\nCurrent closest hostile: {currentClosestHostile.name} is no longer in hostile list, setting another closest hostile...";
                stateComponent.character.combatComponent.RemoveHostileInRange(currentClosestHostile, false);
                SetClosestHostile();
            } else if (currentClosestHostile != null && (!currentClosestHostile.mapObjectVisual || !currentClosestHostile.mapObjectVisual.gameObject)) {
                log +=
                    $"\nCurrent closest hostile: {currentClosestHostile.name} no longer has a map object visual, setting another closest hostile...";
                stateComponent.character.combatComponent.RemoveHostileInRange(currentClosestHostile, false);
                SetClosestHostile();
            } else if (currentClosestHostile != null && currentClosestHostile is Character targetCharacter) {
                if(targetCharacter.isInCombat && (targetCharacter.stateComponent.currentState as CombatState).isAttacking == false)
                log +=
                    $"\nCurrent closest hostile: {currentClosestHostile.name} is already fleeing, will try to set another hostile character that is not fleeing...";
                SetClosestHostilePriorityNotFleeing();
            } else if (currentClosestHostile == null) {
                log += "\nNo current closest hostile, setting one...";
                SetClosestHostile();
            } else {
                log += "\nChecking if the current closest hostile is still the closest hostile, if not, set new closest hostile...";
                IPointOfInterest newClosestHostile = stateComponent.character.combatComponent.GetNearestValidHostile();
                if(newClosestHostile != null && currentClosestHostile != newClosestHostile) {
                    SetClosestHostile(newClosestHostile);
                } else if (currentClosestHostile != null && stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.marker.targetPOI == currentClosestHostile) {
                    log += $"\nAlready in pursuit of current closest hostile: {currentClosestHostile.name}";
                    stateComponent.character.logComponent.PrintLogIfActive(log);
                    return;
                }
            }
            if (currentClosestHostile == null) {
                log += "\nNo more hostile characters, exiting combat state...";
                endedInternally = true;
                stateComponent.ExitCurrentState();
            } else {
                float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.worldPosition);
                if (distance > stateComponent.character.characterClass.attackRange || !stateComponent.character.marker.IsCharacterInLineOfSightWith(currentClosestHostile)) {
                    log += $"\nPursuing closest hostile target: {currentClosestHostile.name}";
                    PursueClosestHostile();
                } else {
                    log += $"\nAlready within range of: {currentClosestHostile.name}. Skipping pursuit...";
                }
            }
            //stateComponent.character.PrintLogIfActive(log);
        } else {
            //Character closestHostile = stateComponent.character.marker.GetNearestValidAvoid();
            List<IPointOfInterest> avoidInRange = stateComponent.character.combatComponent.avoidInRange;
            if (avoidInRange.Count <= 0) {
                log += "\nNo more avoid characters, exiting combat state...";
                stateComponent.character.logComponent.PrintLogIfActive(log);
                endedInternally = true;
                stateComponent.ExitCurrentState();
                return;
            }
            if (stateComponent.character.marker.hasFleePath) {
                log += "\nAlready in flee mode";
                stateComponent.character.logComponent.PrintLogIfActive(log);
                return;
            }
            if (stateComponent.character.canMove == false) {
                log += "\nCannot move, not fleeing";
                stateComponent.character.logComponent.PrintLogIfActive(log);
                return;
            }
            log += $"\n{stateComponent.character.name} is fleeing!";
            stateComponent.character.logComponent.PrintLogIfActive(log);

            IPointOfInterest objToAvoid = avoidInRange[avoidInRange.Count - 1];

            if (isFleeToHome) {
                stateComponent.character.marker.OnStartFleeToHome();
            } else {
                stateComponent.character.marker.OnStartFlee();
            }
            Messenger.Broadcast(Signals.START_FLEE, stateComponent.character);

            string avoidReason = "got scared";
            CombatData combatData = stateComponent.character.combatComponent.GetCombatData(objToAvoid);
            if(combatData != null && combatData.avoidReason != string.Empty) {
                avoidReason = combatData.avoidReason;
            }
            Log fleeLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "start_flee");
            fleeLog.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            fleeLog.AddToFillers(objToAvoid, objToAvoid is GenericTileObject ? "something" : objToAvoid.name,
                LOG_IDENTIFIER.TARGET_CHARACTER);
            fleeLog.AddToFillers(null, avoidReason, LOG_IDENTIFIER.STRING_1);
            stateComponent.character.logComponent.RegisterLog(fleeLog, null, false);
            thoughtBubbleLog = fleeLog;
        }
    }

    #region Attacking
    private void PursueClosestHostile() {
        if (!stateComponent.character.currentParty.icon.isTravelling || stateComponent.character.marker.targetPOI != currentClosestHostile) {
            stateComponent.character.marker.GoToPOI(currentClosestHostile);    
        }
    }
    private void SetClosestHostilePriorityNotFleeing() {
        IPointOfInterest newClosestHostile = stateComponent.character.combatComponent.GetNearestValidHostilePriorityNotFleeing();
        if (newClosestHostile == currentClosestHostile) { return; } // ignore change
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = newClosestHostile;
        //StopPursueTimer(); //stop pursue timer, any time target changes. This is so that pursue timer is reset when target changes
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_combat_target");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            stateComponent.character.logComponent.RegisterLog(log, null, false);
        }
    }
    private void SetClosestHostile() {
        IPointOfInterest newClosestHostile = stateComponent.character.combatComponent.GetNearestValidHostile();
        if (newClosestHostile == currentClosestHostile) { return; } // ignore change
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = newClosestHostile;
        //StopPursueTimer(); //stop pursue timer, any time target changes. This is so that pursue timer is reset when target changes
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_combat_target");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            stateComponent.character.logComponent.RegisterLog(log, null, false);
        }
    }
    private void SetClosestHostile(IPointOfInterest poi) {
        if (poi == currentClosestHostile) { return; } //ignore change
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = poi;
        //StopPursueTimer(); //stop pursue timer, any time target changes. This is so that pursue timer is reset when target changes
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "new_combat_target");
            log.AddToFillers(stateComponent.character, stateComponent.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            stateComponent.character.logComponent.RegisterLog(log, null, false);
        }
    }
    private float timeElapsed;
    public void LateUpdate() {
        if (GameManager.Instance.isPaused) { return; }
        //timeElapsed += Time.deltaTime;
        //if (timeElapsed >= 0.3f) {
        //    timeElapsed = 0;
            //Profiler.BeginSample($"{stateComponent.character.name} Combat State Late Update");
        if (currentClosestHostile != null) {
            if (currentClosestHostile.isDead) {
                stateComponent.character.combatComponent.RemoveHostileInRange(currentClosestHostile);
            } else if (currentClosestHostile.currentRegion != stateComponent.character.currentRegion) {
                stateComponent.character.combatComponent.RemoveHostileInRange(currentClosestHostile);
            } else if (isAttacking && isExecutingAttack == false) {
                //If character is attacking and distance is within the attack range of this character, attack
                //else, pursue again
                // Profiler.BeginSample($"{stateComponent.character.name} Distance Computation");
                // float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.worldPosition);
                // Profiler.EndSample();
                float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.worldPosition);
                if (distance <= stateComponent.character.characterClass.attackRange) {
                    Profiler.BeginSample($"{stateComponent.character.name} Line of Sight Check");
                    bool isInLineOfSight =
                        stateComponent.character.marker.IsCharacterInLineOfSightWith(currentClosestHostile, stateComponent.character.characterClass.attackRange);
                    Profiler.EndSample();
                    // if (distance < stateComponent.character.characterClass.attackRange) {
                    if (isInLineOfSight) {
                        Attack();
                    } else {
                        PursueClosestHostile();
                    }
                } else {
                    PursueClosestHostile();
                }
            }
        }
            //Profiler.EndSample();
        //}
    }
    
    //Will be constantly checked every frame
    private void Attack() {
        string summary = $"{stateComponent.character.name} will attack {currentClosestHostile?.name}";
        
        //When in range and in line of sight, stop movement
        if (stateComponent.character.currentParty.icon.isTravelling && stateComponent.character.currentParty.icon.travelLine == null) {
            stateComponent.character.marker.StopMovement();
            //clear the marker's target poi when it reaches the target, so that the pursue closest hostile will still execute when the other character chooses to flee
            stateComponent.character.marker.SetTargetPOI(null);
        }
        //When the character stops movement, stop pursue timer
        //StopPursueTimer();

        //Check attack speed
        if (!stateComponent.character.marker.CanAttackByAttackSpeed()) {
            //float aspeed = stateComponent.character.marker.attackSpeedMeter;
            summary += "\nCannot attack because of attack speed. Waiting...";
            // stateComponent.character.logComponent.PrintLogIfActive(summary);
            //Debug.Log(summary);
            return;
        }
        
        summary += "\nExecuting attack...";
        InnerMapManager.Instance.FaceTarget(stateComponent.character, currentClosestHostile);
        if (isExecutingAttack == false) {
            stateComponent.character.marker.SetAnimationTrigger("Attack");
            isExecutingAttack = true;
        }
        //Reset Attack Speed
        stateComponent.character.marker.ResetAttackSpeed();
        // stateComponent.character.logComponent.PrintLogIfActive(summary);
        //Debug.Log(summary);
    }
    public bool isExecutingAttack;
    public void OnAttackHit(IDamageable damageable) {
        string attackSummary =
            $"{GameManager.Instance.TodayLogString()}{stateComponent.character.name} hit {damageable?.name ?? "Nothing"}";
        
        if (damageable != null && currentClosestHostile != null) {
            if (damageable != currentClosestHostile) {
                attackSummary =
                    $"{stateComponent.character.name} hit {damageable.name} instead of {currentClosestHostile.name}!";
            }
            
            damageable.OnHitByAttackFrom(stateComponent.character, this, ref attackSummary);

            //If the hostile reaches 0 hp, evaluate if he/she dies, get knock out, or get injured
            if (damageable.currentHP > 0) {
                attackSummary += $"\n{damageable.name} still has remaining hp {damageable.currentHP.ToString()}/{damageable.maxHP.ToString()}";
                if (damageable is Character hitCharacter) {
                    //if the character that attacked is not in the hostile/avoid list of the character that was hit, this means that it is not a retaliation, so the character that was hit must reduce its opinion of the character that attacked
                    if(!hitCharacter.combatComponent.hostilesInRange.Contains(stateComponent.character) && !hitCharacter.combatComponent.avoidInRange.Contains(stateComponent.character)) {
                        if (!allCharactersThatDegradedRel.Contains(hitCharacter)) {
                            hitCharacter.relationshipContainer.AdjustOpinion(hitCharacter, stateComponent.character, "Base", -15);
                            AddCharacterThatDegradedRel(hitCharacter);
                        }
                    }


                    //if the character that was hit is not the actual target of this combat, do not make him/her enter combat state
                    if (damageable == currentClosestHostile) {
                        //When the target is hit and it is still alive, add hostile
                        if ((hitCharacter.combatComponent.combatMode == COMBAT_MODE.Defend ||
                            hitCharacter.combatComponent.combatMode == COMBAT_MODE.Aggressive) && hitCharacter.canPerform) {
                            hitCharacter.combatComponent.FightOrFlight(stateComponent.character, CombatManager.Retaliation, isLethal: stateComponent.character.combatComponent.IsLethalCombatForTarget(hitCharacter));
                        }
                    }
                }
            }
        }
        
        // if (stateComponent.currentState == this) { //so that if the combat state has been exited, this no longer executes that results in endless execution of this coroutine.
        //     attackSummary += $"\n{stateComponent.character.name}'s state is still this, running check coroutine.";
        //     stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
        // } else {
        //     attackSummary +=
        //         $"\n{stateComponent.character.name}'s state no longer this, NOT running check coroutine. Current state is{stateComponent.currentState?.stateName}" ?? "Null";
        // }
        //Debug.Log(attackSummary);
    }
    //private void StartPursueTimer() {
    //    if (!_hasTimerStarted) {
    //        stateComponent.character.logComponent.PrintLogIfActive(
    //            $"Starting pursue timer for {stateComponent.character.name} targeting {currentClosestHostile?.name ?? "Null"}");
    //        _currentAttackTimer = 0;
    //        _hasTimerStarted = true;
    //    }
    //}
    //private void StopPursueTimer() {
    //    if (_hasTimerStarted) {
    //        stateComponent.character.logComponent.PrintLogIfActive(
    //            $"Stopping pursue timer for {stateComponent.character.name}");
    //        _hasTimerStarted = false;
    //        _currentAttackTimer = 0;
    //    }
    //}
    #endregion

    #region Flee
    public void FinishedTravellingFleePath() {
        string log = $"Finished travelling flee path of {stateComponent.character.name}";
        //After travelling flee path, check hostile characters if they are still in vision, every hostile character that is not in vision must be removed form hostile list
        //Consequently, the removed character must also remove this character from his/her hostile list
        //Then check if hostile list is empty
        //If it is, end state immediately
        //If not, flee again
        stateComponent.character.marker.SetHasFleePath(false);
        fleeChance = 10;
        log += "\nFinished travelling flee path, determining action...";
        stateComponent.character.logComponent.PrintLogIfActive(log);
        DetermineReaction(stateComponent.character);
        stateComponent.character.marker.UpdateAnimation();
        stateComponent.character.marker.UpdateActionIcon();
    }
    public void OnReachLowFleeSpeedThreshold() {
        string log = $"{stateComponent.character.name} has reached low flee speed threshold, determining action...";
        stateComponent.character.logComponent.PrintLogIfActive(log);
        DetermineReaction(stateComponent.character);
    }
    private void OnFinishedFleeingFrom(IPointOfInterest fledFrom) {
        //if (fledFrom is Character) {
        //    Character character = fledFrom as Character;
        //    if (stateComponent.character.IsHostileWith(character)) {
        //        stateComponent.character.marker.AddTerrifyingObject(fledFrom);
        //    }
        //}
    }
    #endregion

    #region Utilities
    public void ResetClosestHostile() {
        currentClosestHostile = null;
    }
    public void SetForcedTarget(Character character) {
        forcedTarget = character;
        //if (forcedTarget == null) {
        //    stateComponent.character.SetIsFollowingPlayerInstruction(false); //the force target has been removed.
        //}
    }
    public void AddCharacterThatDegradedRel(Character character) {
        if (!allCharactersThatDegradedRel.Contains(character)) {
            allCharactersThatDegradedRel.Add(character);
        }
    }
    public void AddCharacterThatReactedToThisCombat(Character character) {
        allCharactersThatReactedToThisCombat.Add(character);
    }
    public bool DidCharacterAlreadyReactToThisCombat(Character character) {
        for (int i = 0; i < allCharactersThatReactedToThisCombat.Count; i++) {
            if(allCharactersThatReactedToThisCombat[i] == character) {
                return true;
            }
        }
        return false;
    }
    //public void SetJobThatTriggeredThisState(GoapPlanJob job) {
    //    jobThatTriggeredThisState = job;
    //}
    //public void SetActionThatTriggeredThisState(ActualGoapNode action) {
    //    actionThatTriggeredThisState = action;
    //}
    #endregion
}
