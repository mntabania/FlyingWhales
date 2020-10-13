using System.Collections.Generic;
using UnityEngine;
using Traits;
using UnityEngine.EventSystems;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;

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
    /// <summary>
    /// the last object that this character fled from
    /// </summary>
    public IPointOfInterest lastFledFrom { get; private set; }
    /// <summary>
    /// the structure location of the last object that this character fled from (NOTE: This is the location of the object when the character started fleeing)
    /// </summary>
    public LocationStructure lastFledFromStructure { get; private set; }
    
    //Is this character fighting another character or has a character in hostile range list who is trying to apprehend him/her because he/she is a criminal?
    //See: https://trello.com/c/uCZfbCSa/2819-criminals-should-eventually-flee-npcSettlement-and-leave-faction
    public bool isBeingApprehended { get; private set; }
    //private int fleeChance;
    private bool isFleeToHome;
    /// <summary>
    /// The number of times this character has hit his/her current target
    /// </summary>
    private int _timesHitCurrentTarget;

    public CombatState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Combat State";
        characterState = CHARACTER_STATE.COMBAT;
        //stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        duration = 0;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _currentAttackTimer = 0;
        //fleeChance = 10;
        //Default start of combat state is attacking
        isAttacking = true;
        allCharactersThatDegradedRel = new List<Character>();
        allCharactersThatReactedToThisCombat = new List<Character>();
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        //stateComponent.character.combatComponent.SetWillProcessCombat(true);
        stateComponent.owner.combatComponent.SetWillProcessCombat(false);
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
        stateComponent.owner.logComponent.PrintLogIfActive(
            $"Starting combat state for {stateComponent.owner.name}");
        //stateComponent.character.DecreaseCanWitness();
        stateComponent.owner.marker.ShowHPBar(stateComponent.owner);
        stateComponent.owner.marker.SetAnimationBool("InCombat", true);
        stateComponent.owner.marker.visionCollider.VoteToUnFilterVision();
        if(stateComponent.owner.gatheringComponent.hasGathering && stateComponent.owner.gatheringComponent.currentGathering is SocialGathering) {
            stateComponent.owner.gatheringComponent.currentGathering.RemoveAttendee(stateComponent.owner);
        }
        //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, stateComponent.character, "combat");
        Messenger.AddListener<Character>(Signals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.AddListener<Character>(Signals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
        Messenger.AddListener<Character>(Signals.START_FLEE, OnCharacterStartFleeing);
        base.StartState();
        //if (stateComponent.character.currentActionNode is Assault && !stateComponent.character.currentActionNode.isPerformingActualAction) {
        //    stateComponent.character.currentActionNode.Perform(); //this is for when a character will assault a target, but his/her attack range is less than his/her vision range. (Because end reached distance of assault action is set to attack range)
        //}
        //stateComponent.character.StopCurrentActionNode(false);
        stateComponent.owner.UncarryPOI(); //Drop characters when entering combat
        // if(stateComponent.character is SeducerSummon) { //If succubus/incubus enters a combat, automatically change its faction to the player faction if faction is still disguised
        //     if(stateComponent.character.faction == FactionManager.Instance.disguisedFaction) {
        //         stateComponent.character.ChangeFactionTo(PlayerManager.Instance.player.playerFaction);
        //     }
        // }
        // stateComponent.character.marker.StartCoroutine(CheckIfCurrentHostileIsInRange());
    }
    protected override void EndState() {
        stateComponent.owner.marker.pathfindingAI.ClearAllCurrentPathData();
        stateComponent.owner.marker.SetHasFleePath(false);

        //stateComponent.character.IncreaseCanWitness();
        // stateComponent.character.marker.StopCoroutine(CheckIfCurrentHostileIsInRange());

        stateComponent.owner.marker.HideHPBar();
        stateComponent.owner.marker.SetAnimationBool("InCombat", false);
        stateComponent.owner.marker.visionCollider.VoteToFilterVision();
        stateComponent.owner.logComponent.PrintLogIfActive(
            $"Ending combat state for {stateComponent.owner.name}");
        Messenger.RemoveListener<Character>(Signals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.RemoveListener<Character>(Signals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
        Messenger.RemoveListener<Character>(Signals.START_FLEE, OnCharacterStartFleeing);
        
        if (stateComponent.owner.isNormalCharacter) {
            List<LocationStructure> avoidStructures = new List<LocationStructure>(stateComponent.owner.movementComponent.structuresToAvoid);
            for (int i = 0; i < avoidStructures.Count; i++) {
                LocationStructure avoid = avoidStructures[i];
                if (stateComponent.owner.currentStructure == avoid) {
                    //remove avoid structure if structure is current one.
                    stateComponent.owner.movementComponent.RemoveStructureToAvoid(avoid);
                }
            }
        }
        
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
        if (!stateComponent.owner.isDead) {
            //TEMPORARILY REMOVED THIS UNTIL FURTHER NOTICE
            if (isBeingApprehended && stateComponent.owner.traitContainer.HasTrait("Criminal") && stateComponent.owner.canPerform && stateComponent.owner.canMove) { //!stateComponent.character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                if (!stateComponent.owner.traitContainer.HasTrait("Berserked")) {
                    HexTile chosenHex = stateComponent.owner.currentRegion.GetRandomNoStructureUncorruptedNotPartOrNextToVillagePlainHex();
                    if (chosenHex != null) {
                        LocationGridTile chosenTile = chosenHex.GetRandomTile();
                        stateComponent.owner.jobComponent.CreateFleeCrimeJob(chosenTile);
                        return;
                    }
                }
            }

            //Made it so that dead characters no longer check invision characters after exiting a state.
            for (int i = 0; i < stateComponent.owner.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest currPOI = stateComponent.owner.marker.inVisionPOIs[i];
                if (!stateComponent.owner.marker.unprocessedVisionPOIs.Contains(currPOI)) {
                    // stateComponent.character.CreateJobsOnEnterVisionWith(currCharacter);
                    stateComponent.owner.marker.AddUnprocessedPOI(currPOI);
                }
            }
            stateComponent.owner.needsComponent.CheckExtremeNeeds();
        }
        stateComponent.owner.combatComponent.ClearCombatData();
        if (stateComponent.owner.traitContainer.HasTrait("Subterranean")) {
            stateComponent.owner.behaviourComponent.SetSubterraneanJustExitedCombat(true);
        }
    }
    #endregion

    /// <summary>
    /// Function that determines what a character should do in a certain point in time.
    /// Can be triggered by broadcasting signal <see cref="Signals.DETERMINE_COMBAT_REACTION"/>
    /// </summary>
    /// <param name="character">The character that should determine a reaction.</param>
    private void DetermineReaction(Character character) {
        if (stateComponent.owner == character && stateComponent.currentState == this && !isPaused && !isDone) {
            DetermineIsBeingApprehended();
            string summary = $"{character.name} will determine a combat reaction";
            if (character.marker.hasFleePath) {
                summary = $"{summary}\n-Has flee path";
                //Character is already fleeing
                CheckFlee(ref summary);
            } else {
                //NOTE: If avoid reason is Vampire Bat, the character should always flee and must not trigger cowering, 
                //the reason is we need to let him go to a place where he can transform back to human safely, that is also the reason why we put him in a flee state in the first place
                summary = $"{summary}\n-Has no flee path";
                if (HasStillAvoidPOIThatIsInRange()) {
                    string avoidReason = GetAvoidReason(stateComponent.owner.combatComponent.avoidInRange[0]);
                    summary = $"{summary}\n-Has avoid that is still in range";
                    if (character.homeStructure != null) {
                        summary = $"{summary}\n-Has home dwelling";
                        if (character.homeStructure == character.currentStructure) {
                            summary = $"{summary}\n-Is in Home Dwelling";
                            if (UnityEngine.Random.Range(0, 2) == 0 && avoidReason != CombatManager.Vampire_Bat) {
                                summary = $"{summary}\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            } else {
                                summary = $"{summary}\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            }
                        } else {
                            summary = $"{summary}\n-Is not in Home Dwelling, 40%: Flee to Home, 40%: Flee, 20%: Cowering";
                            int roll = UnityEngine.Random.Range(0, 100);
                            summary = $"{summary}\n-Roll: {roll}";
                            if (roll < 40) {
                                summary = $"{summary}\n-Triggered Flee to Home";
                                SetIsFleeToHome(true);
                                SetIsAttacking(false);
                            } else if ((roll >= 40 && roll < 80) || avoidReason == CombatManager.Vampire_Bat) {
                                summary = $"{summary}\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            } else {
                                summary = $"{summary}\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            }
                        }
                    } else if (character is Summon && (character as Summon).HasTerritory()) {
                        summary = $"{summary}\n-Has territory";
                        Summon summon = character as Summon;
                        if (summon.IsInTerritory()) {
                            summary = $"{summary}\n-Is in territory";
                            if (UnityEngine.Random.Range(0, 2) == 0 && avoidReason != CombatManager.Vampire_Bat) {
                                summary = $"{summary}\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            } else {
                                summary = $"{summary}\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            }
                        } else {
                            summary = $"{summary}\n-Is not in territory, 40%: Flee to territory, 40%: Flee, 20%: Cowering";
                            int roll = UnityEngine.Random.Range(0, 100);
                            summary = $"{summary}\n-Roll: {roll}";
                            if (roll < 40) {
                                summary = $"{summary}\n-Triggered Flee to territory";
                                SetIsFleeToHome(true);
                                SetIsAttacking(false);
                            } else if ((roll >= 40 && roll < 80) || avoidReason == CombatManager.Vampire_Bat) {
                                summary = $"{summary}\n-Triggered Flee";
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            } else {
                                summary = $"{summary}\n-Triggered Cowering";
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            }
                        }
                    } else {
                        summary = $"{summary}\n-Has no home dwelling nor territory";
                        if (UnityEngine.Random.Range(0, 2) == 0 && avoidReason != CombatManager.Vampire_Bat) {
                            summary = $"{summary}\n-Triggered Cowering";
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                            //SetIsFleeToHome(false);
                            //SetIsAttacking(false);
                        } else {
                            summary = $"{summary}\n-Triggered Flee";
                            SetIsFleeToHome(false);
                            SetIsAttacking(false);
                        }
                    }
                } else if (character.combatComponent.hostilesInRange.Count > 0) {
                    summary = $"{summary}\n-Has hostile in list";
                    summary = $"{summary}\n-Attack nereast one";
                    //SetClosestHostile(null);
                    SetIsAttacking(true);
                } else {
                    summary = $"{summary}\n-Has no hostile or avoid in list";
                    summary = $"{summary}\n-Exiting combat state";
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
        Character owner = stateComponent.owner;
        //Will stop pursuing only if current closest hostile is character, if current closest hostile is an object, whether or not the source can run, he/she will still pursue
        if (character == owner && stateComponent.currentState == this && !isPaused && !isDone && currentClosestHostile != null && currentClosestHostile is Character targetCharacter && isAttacking) {
            if(targetCharacter.combatComponent.isInCombat && !(targetCharacter.stateComponent.currentState as CombatState).isAttacking) {
                if (!owner.movementComponent.CanStillPursueTarget(targetCharacter)) {
                    if(owner.combatComponent.combatDataDictionary.ContainsKey(targetCharacter) && owner.combatComponent.combatDataDictionary[targetCharacter].reasonForCombat == CombatManager.Demon_Kill) {
                        //If the reason for combat is Demon Kill, the hostile should not be removed from hostile range, regardless if he/she can still run
                    } else {
                        owner.combatComponent.RemoveHostileInRange(targetCharacter);
                    }
                }
            }
        }
    }
    private void OnCharacterStartFleeing(Character characterThatFlee) {
        Character owner = stateComponent.owner;
        //Will stop pursuing only if current closest hostile is character, if current closest hostile is an object, whether or not the source can run, he/she will still pursue
        if (stateComponent.currentState == this && !isPaused && !isDone && owner.combatComponent.hostilesInRange.Contains(characterThatFlee)) {
            if (!owner.movementComponent.CanStillPursueTarget(characterThatFlee)) {
                if (owner.behaviourComponent.HasBehaviour(typeof(DefendBehaviour))) {
                    //if character is defending, always remove hostile that is already fleeing.
                    //Reference: https://www.notion.so/ruinarch/59a7b75436bc491eab26e0d661f382f8?v=8dcc4b7119dc4c01ba67f35a54c5258b&p=6ec4e2b8234b4da59edb7d8460815216
                    owner.combatComponent.RemoveHostileInRange(characterThatFlee);
                } else {
                    if (owner.combatComponent.combatDataDictionary.ContainsKey(characterThatFlee) && owner.combatComponent.combatDataDictionary[characterThatFlee].reasonForCombat == CombatManager.Demon_Kill) {
                        //If the reason for combat is Demon Kill, the hostile should not be removed from hostile range, regardless if he/she can still run
                    } else {
                        owner.combatComponent.RemoveHostileInRange(characterThatFlee);
                    }    
                }
                
            }
        }
    }
    public void CheckFlee(ref string debugLog) {
        if (!HasStillAvoidPOIThatIsInRange()) {
            debugLog = $"{debugLog}\n-Has no avoid that is still in range";
            if (HasStillHostilePOIThatIsInRange()) {
                debugLog = $"{debugLog}\n-Has hostile that is still in range";
                debugLog = $"{debugLog}\n-Attack nearest one";
                //SetClosestHostile(null);
                SetIsAttacking(true);
            } else {
                //No more flee chance, in the new system, instead of having flee chance, if there are no more characters in flee list, exit flee immediately, no need to finish flee path
                //https://trello.com/c/rNoVtMDD/2457-flee-movement-updates
                debugLog = $"{debugLog}\n-Has no hostile that is still in range, exit flee";
                FinishedTravellingFleePath();

                //debugLog = $"{debugLog}\n-{fleeChance.ToString()}% chance to flee";
                //int roll = UnityEngine.Random.Range(0, 100);
                //debugLog = $"{debugLog}\n-Roll: {roll.ToString()}"; 
                //if (roll < fleeChance) {
                //    debugLog = $"{debugLog}\n-Stop fleeing";
                //    FinishedTravellingFleePath();
                //} else {
                //    fleeChance += 10;
                //    debugLog = $"{debugLog}\n-Flee chance increased by 10%, new flee chance is {fleeChance.ToString()}";
                //}
            }
        } else {
            UpdateFleePath();
        }
    }
    public void CheckFlee() {
        if (!HasStillAvoidPOIThatIsInRange()) {
            if (HasStillHostilePOIThatIsInRange()) {
                //SetClosestHostile(null);
                SetIsAttacking(true);
            } else {
                //No more flee chance, in the new system, instead of having flee chance, if there are no more characters in flee list, exit flee immediately, no need to finish flee path
                //https://trello.com/c/rNoVtMDD/2457-flee-movement-updates
                FinishedTravellingFleePath();
            }
        } else {
            UpdateFleePath();
        }
    }
    private bool HasStillAvoidPOIThatIsInRange() {
        for (int i = 0; i < stateComponent.owner.combatComponent.avoidInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.owner.combatComponent.avoidInRange[i];
            if (stateComponent.owner.marker && stateComponent.owner.marker.IsStillInRange(poi)) {
                return true;
            }
        }
        return false;
    }
    private bool HasStillHostilePOIThatIsInRange() {
        for (int i = 0; i < stateComponent.owner.combatComponent.hostilesInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.owner.combatComponent.hostilesInRange[i];
            if (stateComponent.owner.marker && stateComponent.owner.marker.IsStillInRange(poi)) {
                return true;
            }
        }
        return false;
    }

    private void SetIsAttacking(bool state) {
        isAttacking = state;
        if (isAttacking) {
            actionIconString = GoapActionStateDB.Hostile_Icon;
            thoughtBubbleLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "CharacterState", "Combat State", "thought_bubble", providedTags: LOG_TAG.Combat);
            thoughtBubbleLog.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        } else {
            actionIconString = GoapActionStateDB.Flee_Icon;
        }
        stateComponent.owner.marker.UpdateActionIcon();
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
        for (int i = 0; i < stateComponent.owner.combatComponent.hostilesInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.owner.combatComponent.hostilesInRange[i];
            if (poi is Character hostile) {
                if (hostile.combatComponent.isInCombat) {
                    CombatData combatData = hostile.combatComponent.GetCombatData(stateComponent.owner);
                    if(combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.APPREHEND) {
                        isBeingApprehended = true;
                        return;
                    }
                }
            }
            
        }
        for (int i = 0; i < stateComponent.owner.combatComponent.avoidInRange.Count; i++) {
            IPointOfInterest poi = stateComponent.owner.combatComponent.avoidInRange[i];
            if (poi is Character hostile) {
                if (hostile.combatComponent.isInCombat) {
                    CombatData combatData = hostile.combatComponent.GetCombatData(stateComponent.owner);
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
        DetermineReaction(stateComponent.owner);
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
        string log = $"Reevaluating combat behavior of {stateComponent.owner.name}";
        if (isAttacking) {
            //stateComponent.character.marker.StopPerTickFlee();
            log = $"{log}\n{stateComponent.owner.name} is attacking!";
            if (stateComponent.owner.marker && stateComponent.owner.marker.hasFleePath) {
                log = $"{log}\n-Still has flee path, force finish flee path";
                stateComponent.owner.marker.SetHasFleePath(false);
                //fleeChance = 10;
            } else if (!stateComponent.owner.marker) {
                log = $"{log}\n-Has no marker!";
            }
            Trait taunted = stateComponent.owner.traitContainer.GetTraitOrStatus<Trait>("Taunted");
            if (forcedTarget != null) {
                log = $"{log}\n{stateComponent.owner.name} has a forced target. Setting {forcedTarget.name} as target.";
                SetClosestHostile(forcedTarget);
                SetForcedTarget(null);
            } else if (taunted != null) {
                log = $"{log}\n{stateComponent.owner.name} is taunted. Setting {taunted.responsibleCharacter.name} as target.";
                SetClosestHostile(taunted.responsibleCharacter);
            } else if (currentClosestHostile != null && !stateComponent.owner.combatComponent.hostilesInRange.Contains(currentClosestHostile)) {
                log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} is no longer in hostile list, setting another closest hostile...";
                SetClosestHostile();
            } else if (currentClosestHostile != null && currentClosestHostile.isDead) {
                log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} is no longer in hostile list, setting another closest hostile...";
                stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile, false);
                SetClosestHostile();
            } else if (currentClosestHostile != null && (!currentClosestHostile.mapObjectVisual || !currentClosestHostile.mapObjectVisual.gameObject)) {
                log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} no longer has a map object visual, setting another closest hostile...";
                stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile, false);
                SetClosestHostile();
            } else if (currentClosestHostile != null && currentClosestHostile is Character targetCharacter && targetCharacter.combatComponent.isInCombat &&
                    (targetCharacter.stateComponent.currentState as CombatState).isAttacking == false) {
                if (stateComponent.owner.behaviourComponent.HasBehaviour(typeof(DefendBehaviour))) {
                    log = $"{log}\nCurrent closest hostile: {targetCharacter.name} is already fleeing, and character is defending, remove character from hostile range, and set new target";
                    stateComponent.owner.combatComponent.RemoveHostileInRange(targetCharacter, false);
                    SetClosestHostile();
                } else {
                    log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} is already fleeing, will try to set another hostile character that is not fleeing...";
                    SetClosestHostilePriorityNotFleeing();
                }
            } else if (currentClosestHostile == null) {
                log = $"{log}\nNo current closest hostile, setting one...";
                SetClosestHostile();
            } else {
                log = $"{log}\nChecking if the current closest hostile is still the closest hostile, if not, set new closest hostile...";
                IPointOfInterest newClosestHostile = stateComponent.owner.combatComponent.GetNearestValidHostile();
                if(newClosestHostile != null && currentClosestHostile != newClosestHostile) {
                    SetClosestHostile(newClosestHostile);
                } else if (stateComponent.owner.marker && stateComponent.owner.marker.isMoving && currentClosestHostile != null && stateComponent.owner.marker.targetPOI == currentClosestHostile) {
                    log = $"{log}\nAlready in pursuit of current closest hostile: {currentClosestHostile.name}";
                    stateComponent.owner.logComponent.PrintLogIfActive(log);
                    return;
                }
            }
            if (currentClosestHostile == null) {
                log = $"{log}\nNo more hostile characters, exiting combat state...";
                endedInternally = true;
                stateComponent.ExitCurrentState();
            } else {
                float distance = Vector2.Distance(stateComponent.owner.marker.transform.position, currentClosestHostile.worldPosition);
                if (distance > stateComponent.owner.characterClass.attackRange || !stateComponent.owner.marker.IsCharacterInLineOfSightWith(currentClosestHostile)) {
                    log = $"{log}\nPursuing closest hostile target: {currentClosestHostile.name}";
                    PursueClosestHostile();
                } else {
                    log = $"{log}\nAlready within range of: {currentClosestHostile.name}. Skipping pursuit...";
                }
            }
            //stateComponent.character.PrintLogIfActive(log);
        } else {
            //Character closestHostile = stateComponent.character.marker.GetNearestValidAvoid();
            List<IPointOfInterest> avoidInRange = stateComponent.owner.combatComponent.avoidInRange;
            if (avoidInRange.Count <= 0) {
                log = $"{log}\nNo more avoid characters, exiting combat state...";
                stateComponent.owner.logComponent.PrintLogIfActive(log);
                endedInternally = true;
                stateComponent.ExitCurrentState();
                return;
            }
            if (stateComponent.owner.marker.hasFleePath) {
                log = $"{log}\nAlready in flee mode";
                stateComponent.owner.logComponent.PrintLogIfActive(log);
                return;
            }
            if (stateComponent.owner.canMove == false) {
                log = $"{log}\nCannot move, not fleeing";
                stateComponent.owner.logComponent.PrintLogIfActive(log);
                return;
            }
            log = $"{log}\n{stateComponent.owner.name} is fleeing!";
            stateComponent.owner.logComponent.PrintLogIfActive(log);

            StartFlee();

            if (stateComponent.owner.isNormalCharacter) {
                //character has finished fleeing and is no longer in combat.
                if (lastFledFrom != null && lastFledFromStructure != null && lastFledFrom is Character character && !character.isNormalCharacter && 
                    character.homeStructure == lastFledFromStructure && lastFledFromStructure.structureType != STRUCTURE_TYPE.WILDERNESS) { //&& stateComponent.character.currentStructure != lastFledFromStructure
                    stateComponent.owner.movementComponent.AddStructureToAvoidAndScheduleRemoval(lastFledFromStructure);
                }
            }

            Messenger.Broadcast(Signals.START_FLEE, stateComponent.owner);
        }
    }
    private string GetAvoidReason(IPointOfInterest objToAvoid) {
        string avoidReason = "got scared";
        CombatData combatData = stateComponent.owner.combatComponent.GetCombatData(objToAvoid);
        if(combatData != null && combatData.avoidReason != string.Empty) {
            avoidReason = combatData.avoidReason;
        }
        //Removed this because it's a wrong practice to do this here since this is only a getter
        //Transfered to StartFlee
        //if(avoidReason == "critically low health") {
        //    if(stateComponent.owner.partyComponent.hasParty) {
        //        Party party = stateComponent.owner.partyComponent.currentParty;
        //        if(party.isActive && party.partyState == PARTY_STATE.Working && party.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Raid) {
        //            stateComponent.owner.partyComponent.currentParty.RemoveMemberThatJoinedQuest(stateComponent.owner);
        //        }
        //    }
        //}
        return avoidReason;
    }

    #region Attacking
    private void PursueClosestHostile() {
        if (stateComponent.owner.movementComponent.isStationary) {
            return;
        }
        if (stateComponent.owner.marker && (!stateComponent.owner.marker.isMoving || stateComponent.owner.marker.targetPOI != currentClosestHostile)) {
            stateComponent.owner.marker.GoToPOI(currentClosestHostile);    
        }
    }
    private void SetClosestHostilePriorityNotFleeing() {
        IPointOfInterest newClosestHostile = stateComponent.owner.combatComponent.GetNearestValidHostilePriorityNotFleeing();
        if (newClosestHostile == currentClosestHostile) { return; } // ignore change
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = newClosestHostile;
        //StopPursueTimer(); //stop pursue timer, any time target changes. This is so that pursue timer is reset when target changes
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            _timesHitCurrentTarget = 0;
            CreateNewCombatTargetLog();
        }
    }
    private void SetClosestHostile() {
        IPointOfInterest newClosestHostile = stateComponent.owner.combatComponent.GetNearestValidHostile();
        if (newClosestHostile == currentClosestHostile) { return; } // ignore change
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = newClosestHostile;
        //StopPursueTimer(); //stop pursue timer, any time target changes. This is so that pursue timer is reset when target changes
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            _timesHitCurrentTarget = 0;
            CreateNewCombatTargetLog();
        }
    }
    private void SetClosestHostile(IPointOfInterest poi) {
        if (poi == currentClosestHostile) { return; } //ignore change
        IPointOfInterest previousClosestHostile = currentClosestHostile;
        currentClosestHostile = poi;
        //StopPursueTimer(); //stop pursue timer, any time target changes. This is so that pursue timer is reset when target changes
        if (currentClosestHostile != null && previousClosestHostile != currentClosestHostile) {
            _timesHitCurrentTarget = 0;
            CreateNewCombatTargetLog();
        }
    }
    private void CreateNewCombatTargetLog() {
        CombatData combatData = stateComponent.owner.combatComponent.GetCombatData(currentClosestHostile);
        Log log;
        string key = stateComponent.owner.combatComponent.GetCombatLogKeyReason(currentClosestHostile);
        if (key != string.Empty && LocalizationManager.Instance.HasLocalizedValue("Character", "Combat", key)) {
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Combat", "new_combat_target_with_reason", providedTags: LOG_TAG.Combat);
            string reason = LocalizationManager.Instance.GetLocalizedValue("Character", "Combat", key);
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
        } else {
            //use default log instead, because no text for combat reason was provided. This is to prevent text with %125%.
            log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Combat", "new_combat_target", providedTags: LOG_TAG.Combat);
        }
        log.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        stateComponent.owner.logComponent.RegisterLog(log, null, false);
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
                stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
            } else if (currentClosestHostile.currentRegion != stateComponent.owner.currentRegion) {
                stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
            } else if (isAttacking && isExecutingAttack == false) {
                //If character is attacking and distance is within the attack range of this character, attack
                //else, pursue again
                // Profiler.BeginSample($"{stateComponent.character.name} Distance Computation");
                // float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.worldPosition);
                // Profiler.EndSample();
                float distance = Vector2.Distance(stateComponent.owner.marker.transform.position, currentClosestHostile.worldPosition);
                if (distance <= stateComponent.owner.characterClass.attackRange) {
                    if (stateComponent.owner.movementComponent.isStationary) {
                        Attack();
                    } else {
                        Profiler.BeginSample($"{stateComponent.owner.name} Line of Sight Check");
                        bool isInLineOfSight =
                            stateComponent.owner.marker.IsCharacterInLineOfSightWith(currentClosestHostile, stateComponent.owner.characterClass.attackRange);
                        Profiler.EndSample();
                        // if (distance < stateComponent.character.characterClass.attackRange) {
                        if (isInLineOfSight || stateComponent.owner.movementComponent.isStationary) {
                            Attack();
                        } else {
                            PursueClosestHostile();
                        }
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
        string summary = $"{stateComponent.owner.name} will attack {currentClosestHostile?.name}";

        if (stateComponent.owner.marker.isMoving) {
            //When in range and in line of sight, stop movement
            stateComponent.owner.marker.StopMovement();
            //clear the marker's target poi when it reaches the target, so that the pursue closest hostile will still execute when the other character chooses to flee
            stateComponent.owner.marker.SetTargetPOI(null);
        }

        //When the character stops movement, stop pursue timer
        //StopPursueTimer();

        //Check attack speed
        if (!stateComponent.owner.marker.CanAttackByAttackSpeed()) {
            //float aspeed = stateComponent.character.marker.attackSpeedMeter;
            summary += "\nCannot attack because of attack speed. Waiting...";
            // stateComponent.character.logComponent.PrintLogIfActive(summary);
            //Debug.Log(summary);
            return;
        }
        
        summary += "\nExecuting attack...";
        InnerMapManager.Instance.FaceTarget(stateComponent.owner, currentClosestHostile);
        if (isExecutingAttack == false) {
            stateComponent.owner.marker.SetAnimationTrigger("Attack");
            isExecutingAttack = true;
        }
        //Reset Attack Speed
        stateComponent.owner.marker.ResetAttackSpeed();
        // stateComponent.character.logComponent.PrintLogIfActive(summary);
        //Debug.Log(summary);
    }
    public bool isExecutingAttack;
    public void OnAttackHit(IDamageable damageable) {
        string attackSummary =
            $"{GameManager.Instance.TodayLogString()}{stateComponent.owner.name} hit {damageable?.name ?? "Nothing"}";

        if (damageable != null && currentClosestHostile != null) {
            if (damageable != currentClosestHostile) {
                attackSummary =
                    $"{stateComponent.owner.name} hit {damageable.name} instead of {currentClosestHostile.name}!";
            }
            
            damageable.OnHitByAttackFrom(stateComponent.owner, this, ref attackSummary);

            if (damageable.currentHP > 0) {
                attackSummary += $"\n{damageable.name} still has remaining hp {damageable.currentHP.ToString()}/{damageable.maxHP.ToString()}";
                if (damageable is Character hitCharacter) {
                    //if the character that attacked is not in the hostile/avoid list of the character that was hit, this means that it is not a retaliation, so the character that was hit must reduce its opinion of the character that attacked
                    if(!hitCharacter.combatComponent.hostilesInRange.Contains(stateComponent.owner) && !hitCharacter.combatComponent.avoidInRange.Contains(stateComponent.owner)) {
                        if (!allCharactersThatDegradedRel.Contains(hitCharacter)) {
                            hitCharacter.relationshipContainer.AdjustOpinion(hitCharacter, stateComponent.owner, "Base", -15);
                            AddCharacterThatDegradedRel(hitCharacter);
                        }
                    }


                    //if the character that was hit is not the actual target of this combat, do not make him/her enter combat state
                    if (damageable == currentClosestHostile) {
                        //When the target is hit and it is still alive, add hostile
                        if ((hitCharacter.combatComponent.combatMode == COMBAT_MODE.Defend ||
                            hitCharacter.combatComponent.combatMode == COMBAT_MODE.Aggressive) && hitCharacter.canPerform) {
                            hitCharacter.combatComponent.FightOrFlight(stateComponent.owner, CombatManager.Retaliation, isLethal: stateComponent.owner.combatComponent.IsLethalCombatForTarget(hitCharacter));
                        }
                    }
                }
            }

            if (damageable == currentClosestHostile) {
                //if object cannot be damaged then 10% * X chance to trigger flight response towards target
                //Where X is number of times this character has hit that object
                if (damageable.CanBeDamaged() == false) {
                    int chance = 10 * _timesHitCurrentTarget;
                    if (GameUtilities.RollChance(chance)) {
                        stateComponent.owner.combatComponent.Flight(currentClosestHostile, "got scared");
                    }
                }
                _timesHitCurrentTarget++;

                if (damageable.gridTileLocation != null && damageable.gridTileLocation.structure is DemonicStructure demonicStructure && 
                    demonicStructure.objectsThatContributeToDamage.Contains(damageable)) {
                    demonicStructure.AddAttacker(stateComponent.owner);
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
        string log = $"Finished travelling flee path of {stateComponent.owner.name}";
        //After travelling flee path, check hostile characters if they are still in vision, every hostile character that is not in vision must be removed form hostile list
        //Consequently, the removed character must also remove this character from his/her hostile list
        //Then check if hostile list is empty
        //If it is, end state immediately
        //If not, flee again
        stateComponent.owner.marker.SetHasFleePath(false);
        //fleeChance = 10;
        log += "\nFinished travelling flee path, determining action...";
        stateComponent.owner.logComponent.PrintLogIfActive(log);
        DetermineReaction(stateComponent.owner);
        stateComponent.owner.marker.UpdateAnimation();
        stateComponent.owner.marker.UpdateActionIcon();
    }
    private void UpdateFleePath() {
        string log = $"Updating flee path of {stateComponent.owner.name}";
        stateComponent.owner.logComponent.PrintLogIfActive(log);
        StartFlee(false);
    }
    private void StartFlee(bool shouldLog = true) {
        if (stateComponent.owner.combatComponent.avoidInRange.Count == 0) {
            return;
        }
        List<IPointOfInterest> avoidInRange = stateComponent.owner.combatComponent.avoidInRange;
        IPointOfInterest objToAvoid = avoidInRange[avoidInRange.Count - 1];
        lastFledFrom = objToAvoid;
        lastFledFromStructure = objToAvoid.gridTileLocation?.structure;

        if (stateComponent.owner.marker && !stateComponent.owner.marker.hasFleePath) {
            //We check here if no flee path because that is the indicator that the character will start flee for the first time
            //Sometimes this function is called even if there is already a flee path just to update the path so there will be times that hasFleePath is already true when this is called
            //So we need to have a checker so that the OnBeforeStartFlee will only be called once per flee
            List<Trait> traitOverrideFunctions = stateComponent.owner.traitContainer.GetTraitOverrideFunctions(TraitManager.Before_Start_Flee);
            if (traitOverrideFunctions != null) {
                for (int i = 0; i < traitOverrideFunctions.Count; i++) {
                    Trait trait = traitOverrideFunctions[i];
                    trait.OnBeforeStartFlee(stateComponent.owner);
                }
            }
        }
        string reason = GetAvoidReason(objToAvoid);
        if (reason == "critically low health") {
            if (stateComponent.owner.partyComponent.hasParty) {
                Party party = stateComponent.owner.partyComponent.currentParty;
                if (party.isActive && party.partyState == PARTY_STATE.Working && party.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Raid) {
                    stateComponent.owner.partyComponent.currentParty.RemoveMemberThatJoinedQuest(stateComponent.owner);
                }
            }
        }
        //stateComponent.owner.marker.OnStartFlee();
        if (stateComponent.owner.currentStructure != null && stateComponent.owner.currentStructure.structureType.IsSpecialStructure()) {
            stateComponent.owner.marker.OnStartFleeToOutside();
        } else {
            stateComponent.owner.marker.OnStartFlee();
            //Removed Flee to Home temporarily because when triggered the character always goes home no matter how far he is, even in diff region, and we don't want his flee state to be that long
            //if (isFleeToHome) {
            //    stateComponent.owner.marker.OnStartFleeToHome();
            //} else {
            //    stateComponent.owner.marker.OnStartFlee();
            //}
        }

        if (shouldLog) {
            Log fleeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "start_flee", providedTags: LOG_TAG.Combat);
            fleeLog.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            fleeLog.AddToFillers(objToAvoid, objToAvoid is GenericTileObject ? "something" : objToAvoid.name,
                LOG_IDENTIFIER.TARGET_CHARACTER);
            fleeLog.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
            stateComponent.owner.logComponent.RegisterLog(fleeLog, null, false);
            thoughtBubbleLog = fleeLog;
        }
    }
    public void OnReachLowFleeSpeedThreshold() {
        string log = $"{stateComponent.owner.name} has reached low flee speed threshold, determining action...";
        stateComponent.owner.logComponent.PrintLogIfActive(log);
        DetermineReaction(stateComponent.owner);
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
