using System.Collections.Generic;
using UnityEngine;
using Traits;
using UnityEngine.EventSystems;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using UnityEngine.Profiling;
using UtilityScripts;
using Pathfinding;

public class CombatState : CharacterState {

    private int _currentAttackTimer; //When this timer reaches max, remove currently hostile target from hostile list
    private bool _hasTimerStarted;
    private const float Wall_Attack_Range_Tolerance = 0.4f;
    private const float Moving_Target_Tolerance = 0.9f;

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

    public bool isExecutingAttack;
    public GridNodeBase repositioningTo { get; private set; }

    public bool isRepositioning => repositioningTo != null;

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
    //            stateComponent.character.marker.IsPOIInVision(currentClosestHostile)) {
    //            StartPursueTimer();
    //        }
    //    }
    //}
    protected override void StartState() {
        stateComponent.owner.isBeingCarriedBy?.StopCurrentActionNode();
#if DEBUG_LOG
        stateComponent.owner.logComponent.PrintLogIfActive($"Starting combat state for {stateComponent.owner.name}");
#endif
        //stateComponent.character.DecreaseCanWitness();
        stateComponent.owner.marker.ShowHPBar(stateComponent.owner);
        stateComponent.owner.marker.SetAnimationBool("InCombat", true);
        stateComponent.owner.marker.visionCollider.VoteToUnFilterVision();
        if(stateComponent.owner.gatheringComponent.hasGathering && stateComponent.owner.gatheringComponent.currentGathering is SocialGathering) {
            stateComponent.owner.gatheringComponent.currentGathering.RemoveAttendee(stateComponent.owner);
        }
        //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, stateComponent.character, "combat");
        Messenger.AddListener<Character>(CharacterSignals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.AddListener<Character>(CharacterSignals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
        Messenger.AddListener<Character>(CharacterSignals.START_FLEE, OnCharacterStartFleeing);
        base.StartState();
        //if (stateComponent.character.currentActionNode is Assault && !stateComponent.character.currentActionNode.isPerformingActualAction) {
        //    stateComponent.character.currentActionNode.Perform(); //this is for when a character will assault a target, but his/her attack range is less than his/her vision range. (Because end reached distance of assault action is set to attack range)
        //}
        //stateComponent.character.StopCurrentActionNode(false);
        stateComponent.owner.UncarryPOI(); //Drop characters when entering combat
        // if(stateComponent.character is SeducerSummon) { //If succubus/incubus enters a combat, automatically change its faction to the player faction if faction is still disguised
        //     if(stateComponent.character.faction?.factionType.type == FACTION_TYPE.Disguised) {
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
#if DEBUG_LOG
        stateComponent.owner.logComponent.PrintLogIfActive(
            $"Ending combat state for {stateComponent.owner.name}");
#endif
        Messenger.RemoveListener<Character>(CharacterSignals.DETERMINE_COMBAT_REACTION, DetermineReaction);
        Messenger.RemoveListener<Character>(CharacterSignals.UPDATE_MOVEMENT_STATE, OnUpdateMovementState);
        Messenger.RemoveListener<Character>(CharacterSignals.START_FLEE, OnCharacterStartFleeing);
        
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
        if (stateComponent.owner.marker) {
            stateComponent.owner.marker.visionCollider.ReCategorizeVision();
        }
        if (!stateComponent.owner.isDead) {
            //TEMPORARILY REMOVED THIS UNTIL FURTHER NOTICE
            if (isBeingApprehended && stateComponent.owner.traitContainer.HasTrait("Criminal") && stateComponent.owner.limiterComponent.canPerform && stateComponent.owner.limiterComponent.canMove) { //!stateComponent.character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                if (!stateComponent.owner.traitContainer.HasTrait("Berserked")) {
                    Area chosenArea = stateComponent.owner.currentRegion.GetRandomAreaThatIsUncorruptedAndNotMountainWaterAndNoStructureAndNotNextToOrPartOfVillage();
                    if (chosenArea != null) {
                        LocationGridTile chosenTile = chosenArea.gridTileComponent.GetRandomPassableTile();
                        stateComponent.owner.jobComponent.CreateFleeCrimeJob(chosenTile);
                        return;
                    }
                }
            }

            //Made it so that dead characters no longer check invision characters after exiting a state.
            if (stateComponent.owner.marker) {
                for (int i = 0; i < stateComponent.owner.marker.inVisionPOIs.Count; i++) {
                    IPointOfInterest currPOI = stateComponent.owner.marker.inVisionPOIs[i];
                    if (!stateComponent.owner.marker.unprocessedVisionPOIs.Contains(currPOI)) {
                        // stateComponent.character.CreateJobsOnEnterVisionWith(currCharacter);
                        stateComponent.owner.marker.AddUnprocessedPOI(currPOI);
                    }
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
            string summary = string.Empty;
#if DEBUG_LOG
            summary = $"{character.name} will determine a combat reaction";
#endif
            if (character.marker.hasFleePath) {
#if DEBUG_LOG
                summary = $"{summary}\n-Has flee path";
#endif
                //Character is already fleeing
                CheckFlee(ref summary);
            } else {
                //NOTE: If avoid reason is Vampire Bat, the character should always flee and must not trigger cowering, 
                //the reason is we need to let him go to a place where he can transform back to human safely, that is also the reason why we put him in a flee state in the first place
#if DEBUG_LOG
                summary = $"{summary}\n-Has no flee path";
#endif
                if (HasStillAvoidPOIThatIsInRange()) {
                    IPointOfInterest avoidedPOI = stateComponent.owner.combatComponent.avoidInRange[stateComponent.owner.combatComponent.avoidInRange.Count - 1];
                    string avoidReason = GetAvoidReason(avoidedPOI);
                    bool doNotCower = avoidReason == CombatManager.Avoiding_Witnesses || avoidReason == CombatManager.Encountered_Hostile || avoidReason == CombatManager.Vulnerable;
#if DEBUG_LOG
                    summary = $"{summary}\n-Has avoid that is still in range";
#endif
                    if (avoidedPOI is Character avoidedCharacter && avoidedCharacter.isNormalCharacter && stateComponent.owner.traitContainer.HasTrait("Enslaved") && stateComponent.owner.isNormalCharacter) {
                        //If character is a slave and the target being avoided is a villager, always cower, so that the target will be able to reach this slave
#if DEBUG_LOG
                        summary = $"{summary}\n-Character is a slave and avoided character is a villageer, will only cower";
                        summary = $"{summary}\n-Triggered Cowering";
                        character.logComponent.PrintLogIfActive(summary);
#endif
                        character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                        return;
                    }
                    if (character.homeStructure != null) {
#if DEBUG_LOG
                        summary = $"{summary}\n-Has home dwelling";
#endif
                        if (character.homeStructure == character.currentStructure) {
#if DEBUG_LOG
                            summary = $"{summary}\n-Is in Home Dwelling";
#endif
                            if (UnityEngine.Random.Range(0, 2) == 0 && !doNotCower) {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Cowering";
#endif
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            } else {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Flee";
#endif
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            }
                        } else {
                            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                            summary = $"{summary}\n-Is not in Home Dwelling, 40%: Flee to Home, 40%: Flee, 20%: Cowering";
                            summary = $"{summary}\n-Roll: {roll}";
#endif
                            if (roll < 40) {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Flee to Home";
#endif
                                SetIsFleeToHome(true);
                                SetIsAttacking(false);
                            } else if ((roll >= 40 && roll < 80) || doNotCower) {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Flee";
#endif
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            } else {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Cowering";
#endif
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            }
                        }
                    } else if (character is Summon summon && summon.HasTerritory()) {
#if DEBUG_LOG
                        summary = $"{summary}\n-Has territory";
#endif
                        if (summon.IsInTerritory()) {
#if DEBUG_LOG
                            summary = $"{summary}\n-Is in territory";
#endif
                            if (UnityEngine.Random.Range(0, 2) == 0 && !doNotCower) {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Cowering";
#endif
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            } else {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Flee";
#endif
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            }
                        } else {
                            int roll = UnityEngine.Random.Range(0, 100);
#if DEBUG_LOG
                            summary = $"{summary}\n-Is not in territory, 40%: Flee to territory, 40%: Flee, 20%: Cowering";
                            summary = $"{summary}\n-Roll: {roll}";
#endif
                            if (roll < 40) {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Flee to territory";
#endif
                                SetIsFleeToHome(true);
                                SetIsAttacking(false);
                            } else if ((roll >= 40 && roll < 80) || doNotCower) {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Flee";
#endif
                                SetIsFleeToHome(false);
                                SetIsAttacking(false);
                            } else {
#if DEBUG_LOG
                                summary = $"{summary}\n-Triggered Cowering";
#endif
                                character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                                //SetIsFleeToHome(false);
                                //SetIsAttacking(false);
                            }
                        }
                    } else {
#if DEBUG_LOG
                        summary = $"{summary}\n-Has no home dwelling nor territory";
#endif
                        if (UnityEngine.Random.Range(0, 2) == 0 && !doNotCower) {
#if DEBUG_LOG
                            summary = $"{summary}\n-Triggered Cowering";
#endif
                            character.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, character, reason: avoidReason);
                            //SetIsFleeToHome(false);
                            //SetIsAttacking(false);
                        } else {
#if DEBUG_LOG
                            summary = $"{summary}\n-Triggered Flee";
#endif
                            SetIsFleeToHome(false);
                            SetIsAttacking(false);
                        }
                    }
                } else if (character.combatComponent.hostilesInRange.Count > 0) {
#if DEBUG_LOG
                    summary = $"{summary}\n-Has hostile in list";
                    summary = $"{summary}\n-Attack nereast one";
#endif
                    //SetClosestHostile(null);
                    SetIsAttacking(true);
                } else {
#if DEBUG_LOG
                    summary = $"{summary}\n-Has no hostile or avoid in list";
                    summary = $"{summary}\n-Exiting combat state";
#endif
                    character.combatComponent.ClearAvoidInRange(false);
                    endedInternally = true;
                    character.stateComponent.ExitCurrentState();
                }
            }
#if DEBUG_LOG
            character.logComponent.PrintLogIfActive(summary);
#endif
            //if (stateComponent.character.combatComponent.hostilesInRange.Count > 0) {
            //    summary += "\nStill has hostiles, will attack...";
            //    stateComponent.character.logComponent.PrintLogIfActive(summary);
            //    SetIsAttacking(true);
            //} else if (stateComponent.character.combatComponent.avoidInRange.Count > 0) {
            //    summary += "\nStill has characters to avoid, checking if those characters are still in range...";
            //    for (int i = 0; i < stateComponent.character.combatComponent.avoidInRange.Count; i++) {
            //        IPointOfInterest currAvoid = stateComponent.character.combatComponent.avoidInRange[i];
            //        if (!stateComponent.character.marker.IsPOIInVision(currAvoid) 
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
        if (stateComponent.currentState == this && !isPaused && !isDone && owner.combatComponent.IsHostileInRange(characterThatFlee)) {
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
#if DEBUG_LOG
            debugLog = $"{debugLog}\n-Has no avoid that is still in range";
#endif
            if (HasStillHostilePOIThatIsInRange()) {
#if DEBUG_LOG
                debugLog = $"{debugLog}\n-Has hostile that is still in range";
                debugLog = $"{debugLog}\n-Attack nearest one";
#endif
                //SetClosestHostile(null);
                SetIsAttacking(true);
            } else {
                //No more flee chance, in the new system, instead of having flee chance, if there are no more characters in flee list, exit flee immediately, no need to finish flee path
                //https://trello.com/c/rNoVtMDD/2457-flee-movement-updates
#if DEBUG_LOG
                debugLog = $"{debugLog}\n-Has no hostile that is still in range, exit flee";
#endif
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
            //When attacking, a Mastered Lycanthrope will transform to werewolf first
            if (stateComponent.owner.isLycanthrope && stateComponent.owner.lycanData.isMaster) {
                if (!stateComponent.owner.isInWerewolfForm) {
                    if (!stateComponent.owner.crimeComponent.HasNonHostileVillagerInRangeThatConsidersCrimeTypeACrime(CRIME_TYPE.Werewolf, currentClosestHostile as Character)) {
                        if(stateComponent.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Werewolf, stateComponent.owner)) {
                            stateComponent.owner.combatComponent.SetWillProcessCombat(true);
                            return;
                        }
                    } else {
                        bool shouldProcess = true;
                        string log = string.Empty;
                        SetClosestHostileProcessing(ref shouldProcess, ref log);
                        if (currentClosestHostile is Character hostileCharacter) {
                            CombatData data = stateComponent.owner.combatComponent.GetCombatData(hostileCharacter);
                            if (data != null && data.connectedAction != null && data.connectedAction.associatedJobType == JOB_TYPE.LYCAN_HUNT_PREY) {
                                //If hostile character is the target of lycan hunt, always transform to wolf when attacking it, do not check criminilaty of werewolf anymore
                                if (stateComponent.owner.interruptComponent.TriggerInterrupt(INTERRUPT.Transform_To_Werewolf, stateComponent.owner)) {
                                    stateComponent.owner.combatComponent.SetWillProcessCombat(true);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        if(stateComponent.owner.combatComponent.combatBehaviourParent.TryDoCombatBehaviour(stateComponent.owner, this)) {
            //Do not further process combat if the TryDoCombatBehaviour returns true because the aspects of the combat probably has changed
            //Instead, process the combat again
            stateComponent.owner.combatComponent.SetWillProcessCombat(true);
            return;
        }

        DoCombatBehavior();
        if (isAttacking) {
            actionIconString = stateComponent.owner.combatComponent.GetCombatStateIconString(currentClosestHostile);
            string reasonKey = stateComponent.owner.combatComponent.GetCombatLogKeyReason(currentClosestHostile);

            if (string.IsNullOrEmpty(reasonKey)) {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "CharacterState", "Combat State", "thought_bubble_no_reason", providedTags: LOG_TAG.Combat);
                log.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                SetThoughtBubbleLog(log);
            } else {
                Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "CharacterState", "Combat State", "thought_bubble_with_reason", providedTags: LOG_TAG.Combat);
                log.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                if (LocalizationManager.Instance.HasLocalizedValue("Character", "Combat", reasonKey)) {
                    string reason = LocalizationManager.Instance.GetLocalizedValue("Character", "Combat", reasonKey);
                    log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
                }
                SetThoughtBubbleLog(log);
            }
        } else {
            actionIconString = GoapActionStateDB.Flee_Icon;
        }
        stateComponent.owner.marker.UpdateActionIcon();
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
                    if(combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.APPREHEND && hostile.faction == stateComponent.owner.faction) {
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
                    if (combatData != null && combatData.connectedAction != null && combatData.connectedAction.associatedJobType == JOB_TYPE.APPREHEND && hostile.faction == stateComponent.owner.faction) {
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
    private void SetClosestHostileProcessing(ref bool shouldStillProcessAfterwards, ref string log) {
        shouldStillProcessAfterwards = true;
        if (forcedTarget != null) {
#if DEBUG_LOG
            log = $"{log}\n{stateComponent.owner.name} has a forced target. Setting {forcedTarget.name} as target.";
#endif
            SetClosestHostile(forcedTarget);
            SetForcedTarget(null);
        } else if (currentClosestHostile != null && !stateComponent.owner.combatComponent.IsHostileInRange(currentClosestHostile)) {
#if DEBUG_LOG
            log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} is no longer in hostile list, setting another closest hostile...";
#endif
            SetClosestHostile();
        } else if (currentClosestHostile != null && currentClosestHostile.isDead) {
#if DEBUG_LOG
            log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} is no longer in hostile list, setting another closest hostile...";
#endif
            stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile, false);
            SetClosestHostile();
        } else if (currentClosestHostile != null && (!currentClosestHostile.mapObjectVisual || !currentClosestHostile.mapObjectVisual.gameObject)) {
#if DEBUG_LOG
            log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} no longer has a map object visual, setting another closest hostile...";
#endif
            stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile, false);
            SetClosestHostile();
        } else if (currentClosestHostile != null && currentClosestHostile is Character targetCharacter && targetCharacter.combatComponent.isInCombat &&
                (targetCharacter.stateComponent.currentState as CombatState).isAttacking == false) {
            if (stateComponent.owner.behaviourComponent.HasBehaviour(typeof(DefendBehaviour))) {
#if DEBUG_LOG
                log = $"{log}\nCurrent closest hostile: {targetCharacter.name} is already fleeing, and character is defending, remove character from hostile range, and set new target";
#endif
                stateComponent.owner.combatComponent.RemoveHostileInRange(targetCharacter, false);
                SetClosestHostile();
            } else {
#if DEBUG_LOG
                log = $"{log}\nCurrent closest hostile: {currentClosestHostile.name} is already fleeing, will try to set another hostile character that is not fleeing...";
#endif
                SetClosestHostilePriorityNotFleeing();
            }
        } else if (currentClosestHostile == null) {
#if DEBUG_LOG
            log = $"{log}\nNo current closest hostile, setting one...";
#endif
            SetClosestHostile();
        } else {
#if DEBUG_LOG
            log = $"{log}\nChecking if the current closest hostile is still the closest hostile, if not, set new closest hostile...";
#endif
            IPointOfInterest newClosestHostile = stateComponent.owner.combatComponent.GetNearestValidHostile();
            if (newClosestHostile != null && currentClosestHostile != newClosestHostile) {
                SetClosestHostile(newClosestHostile);
            } else if (stateComponent.owner.marker && stateComponent.owner.marker.isMoving && currentClosestHostile != null && stateComponent.owner.marker.targetPOI == currentClosestHostile) {
#if DEBUG_LOG
                log = $"{log}\nAlready in pursuit of current closest hostile: {currentClosestHostile.name}";
                stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
                shouldStillProcessAfterwards = false;
            }
        }
    }
    private void DoCombatBehavior() {
        if(stateComponent.currentState != this) {
            return;
        }
        string log = string.Empty;
#if DEBUG_LOG
        log = $"Reevaluating combat behavior of {stateComponent.owner.name}";
#endif
        if (isAttacking) {
            //stateComponent.character.marker.StopPerTickFlee();
#if DEBUG_LOG
            log = $"{log}\n{stateComponent.owner.name} is attacking!";
#endif
            if (stateComponent.owner.marker && stateComponent.owner.marker.hasFleePath) {
#if DEBUG_LOG
                log = $"{log}\n-Still has flee path, force finish flee path";
#endif
                stateComponent.owner.marker.SetHasFleePath(false);
                //fleeChance = 10;
            } else if (!stateComponent.owner.marker) {
#if DEBUG_LOG
                log = $"{log}\n-Has no marker!";
#endif
            }
            bool shouldProcessAfterwards = true;
            SetClosestHostileProcessing(ref shouldProcessAfterwards, ref log);
            if (!shouldProcessAfterwards) {
#if DEBUG_LOG
                stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
                return;
            }
            if (currentClosestHostile == null) {
#if DEBUG_LOG
                log = $"{log}\nNo more hostile characters, exiting combat state...";
#endif
                endedInternally = true;
                stateComponent.ExitCurrentState();
            } else {
                float distance = Vector2.Distance(stateComponent.owner.marker.transform.position, currentClosestHostile.worldPosition);
                if (distance > stateComponent.owner.characterClass.attackRange || !stateComponent.owner.marker.IsCharacterInLineOfSightWith(currentClosestHostile)) {
#if DEBUG_LOG
                    log = $"{log}\nPursuing closest hostile target: {currentClosestHostile.name}";
#endif
                    PursueClosestHostile();
                } else {
#if DEBUG_LOG
                    log = $"{log}\nAlready within range of: {currentClosestHostile.name}. Skipping pursuit...";
#endif
                }
            }
#if DEBUG_LOG
            stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
        } else {
            //Character closestHostile = stateComponent.character.marker.GetNearestValidAvoid();
            List<IPointOfInterest> avoidInRange = stateComponent.owner.combatComponent.avoidInRange;
            if (avoidInRange.Count <= 0) {
#if DEBUG_LOG
                log = $"{log}\nNo more avoid characters, exiting combat state...";
                stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
                endedInternally = true;
                stateComponent.ExitCurrentState();
                return;
            }
            if (stateComponent.owner.marker.hasFleePath) {
#if DEBUG_LOG
                log = $"{log}\nAlready in flee mode";
                stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
                return;
            }
            if (stateComponent.owner.limiterComponent.canMove == false) {
#if DEBUG_LOG
                log = $"{log}\nCannot move, not fleeing";
                stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
                return;
            }
#if DEBUG_LOG
            log = $"{log}\n{stateComponent.owner.name} is fleeing!";
            stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif

            StartFlee();

            if (stateComponent.owner.isNormalCharacter) {
                //character has finished fleeing and is no longer in combat.
                if (lastFledFrom != null && lastFledFromStructure != null && lastFledFrom is Character character && !character.isNormalCharacter && 
                    character.homeStructure == lastFledFromStructure && lastFledFromStructure.structureType != STRUCTURE_TYPE.WILDERNESS) { //&& stateComponent.character.currentStructure != lastFledFromStructure
                    stateComponent.owner.movementComponent.AddStructureToAvoidAndScheduleRemoval(lastFledFromStructure);
                }
            }

            Messenger.Broadcast(CharacterSignals.START_FLEE, stateComponent.owner);
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
        if(combatData.connectedAction == null) {
            //If the combat data regarding the hostile is from a job/action, do not log "[Actor] started attacking [Hostile]..."
            //The reason for this is so we can eliminate the duplication of logs during Assault action
            //Because Assault action already logs "[Actor] is assaulting [Hostile]", it would be redundant to log "[Actor] started attacking [Hostile]..."
            //So, only when there is no connected action should we log the "started attacking"
            Log log;
            string key = stateComponent.owner.combatComponent.GetCombatLogKeyReason(currentClosestHostile);
            if (!string.IsNullOrEmpty(key) && LocalizationManager.Instance.HasLocalizedValue("Character", "Combat", key)) {
                log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Combat", "new_combat_target_with_reason", providedTags: LOG_TAG.Combat);
                string reason = LocalizationManager.Instance.GetLocalizedValue("Character", "Combat", key);
                log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
            } else {
                //use default log instead, because no text for combat reason was provided. This is to prevent text with %125%.
                log = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Combat", "new_combat_target", providedTags: LOG_TAG.Combat);
            }
            log.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(currentClosestHostile, currentClosestHostile.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            stateComponent.owner.logComponent.RegisterLog(log, true);
        }
    }
    private float timeElapsed;
    public void LateUpdate() {
        if (GameManager.Instance.isPaused) { return; }
        //timeElapsed += Time.deltaTime;
        //if (timeElapsed >= 0.3f) {
        //    timeElapsed = 0;
        //Profiler.BeginSample($"{stateComponent.character.name} Combat State Late Update");
        bool specialSkillExecuted = ExecuteSpecialSkill();
        if (!specialSkillExecuted && currentClosestHostile != null) {
            if (currentClosestHostile.isDead || currentClosestHostile.currentHP <= 0) {
                stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
            } else if (currentClosestHostile.currentRegion != stateComponent.owner.currentRegion) {
                stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
            } else if (isAttacking && isExecutingAttack == false && isRepositioning == false) {
                //If character is attacking and distance is within the attack range of this character, attack
                //else, pursue again
                // Profiler.BeginSample($"{stateComponent.character.name} Distance Computation");
                // float distance = Vector2.Distance(stateComponent.character.marker.transform.position, currentClosestHostile.worldPosition);
                // Profiler.EndSample();

                //We use attackRangePosition instead of worldPosition when calculating distance if attacker can already reach target with his attack range
                //because there are now differences between the two
                //One major difference is the demonic structure tile object, see GetAttackRangePosForDemonicStructureTileObject in TileObject
                float distance = Vector2.Distance(stateComponent.owner.worldPosition, currentClosestHostile.attackRangePosition);
                if (stateComponent.owner.characterClass.rangeType == RANGE_TYPE.MELEE) {
                    if (currentClosestHostile.IsUnpassable()) {
                        distance -= Wall_Attack_Range_Tolerance; //because sometimes melee characters cannot reach wall/door    
                    } else if (currentClosestHostile is Character character && character.hasMarker && character.marker.isMoving) {
                        distance -= Moving_Target_Tolerance;
                    }
                }
                // Debug.Log($"{stateComponent.owner.name} current attack distance {distance.ToString()}");
                if (stateComponent.owner.characterClass.attackRange >= distance) {
                    if (stateComponent.owner.movementComponent.isStationary) {
                        AttackOrReposition();
                    } else {
#if DEBUG_PROFILER
                        Profiler.BeginSample($"{stateComponent.owner.name} Line of Sight Check");
#endif
                        bool isInLineOfSight =
                            stateComponent.owner.marker.IsCharacterInLineOfSightWith(currentClosestHostile, stateComponent.owner.characterClass.attackRange);
#if DEBUG_PROFILER
                        Profiler.EndSample();
#endif
                        // if (distance < stateComponent.character.characterClass.attackRange) {5
                        if (isInLineOfSight || stateComponent.owner.movementComponent.isStationary) {
                            AttackOrReposition();
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
    
    //Will be checked constantly
    private bool ExecuteSpecialSkill() {
        if (!stateComponent.owner.combatComponent.specialSkillParent.HasSpecialSkill()) {
            return false;
        }
        if (!stateComponent.owner.marker.CanAttackByAttackSpeed()) {
            return false;
        }
        if (stateComponent.owner.combatComponent.specialSkillParent.TryActivateSpecialSkill(stateComponent.owner)) {
            if (stateComponent.owner.marker.isMoving) {
                stateComponent.owner.marker.StopMovement();
            }
            InnerMapManager.Instance.FaceTarget(stateComponent.owner, currentClosestHostile);
            stateComponent.owner.marker.ResetAttackSpeed();
            return true;
        }
        return false;
    }

    //Will be constantly checked every frame
    private void AttackOrReposition() {
        if (stateComponent.owner.movementComponent.IsCurrentGridNodeOccupiedByOtherNonRepositioningActiveCharacter()) {
            if (!TryReposition()) {
                stateComponent.owner.combatComponent.RemoveHostileInRange(currentClosestHostile);
            }
        } else {
            Attack();
        }
    }

#region Reposition
    private void SetGridNodeToReposition(GridNodeBase p_gridNode) {
        if (repositioningTo != p_gridNode) {
            repositioningTo = p_gridNode;
            if (isRepositioning) {
                if (stateComponent.owner.hasMarker) {
                    stateComponent.owner.marker.pathfindingAI.SetEndReachedDistance(0.05f);
                }
            } else {
                if (stateComponent.owner.hasMarker) {
                    stateComponent.owner.marker.pathfindingAI.ResetEndReachedDistance();
                }
            }
        }
    }
    private bool TryReposition() {
        string summary = string.Empty;
#if DEBUG_LOG
        summary = $"{stateComponent.owner.name} will reposition relative to {currentClosestHostile?.name}";
#endif
        bool hasRepositioned = RepositionToAnotherUnoccupiedNodeWithinDistance(stateComponent.owner.characterClass.attackRange, currentClosestHostile, ref summary);
#if DEBUG_LOG
        stateComponent.owner.logComponent.PrintLogIfActive(summary);
#endif
        return hasRepositioned;
    }
    private bool RepositionToAnotherUnoccupiedNodeWithinDistance(float p_distanceLimit, IPointOfInterest p_relativePOI, ref string summary) {
        if (stateComponent.owner.hasMarker && !isRepositioning) {
            LocationGridTile initialTile = stateComponent.owner.gridTileLocation;
            LocationGridTile relativeTile = p_relativePOI.gridTileLocation;
            if (initialTile != null && relativeTile != null) {
                LocationGridTile chosenGridTile = null;
                //List<LocationGridTile> alreadyCheckedTiles = RuinarchListPool<LocationGridTile>.Claim();
                Vector3 position = GetPositionToReposition(initialTile, p_distanceLimit, p_relativePOI.worldPosition, ref chosenGridTile); //alreadyCheckedTiles
                //RuinarchListPool<LocationGridTile>.Release(alreadyCheckedTiles);
                if (!position.Equals(Vector3.positiveInfinity)) {
                    GridNodeBase gridNode = chosenGridTile.GetGridNodeByWorldPosition(position);
                    SetGridNodeToReposition(gridNode);
                    stateComponent.owner.marker.GoTo(position, OnArriveAfterCombatRepositioning);
#if DEBUG_LOG
                    summary += "\nWill reposition to " + position;
#endif
                } else {
#if DEBUG_LOG
                    summary += "\nCannot find position";
#endif
                    return false;
                }
            } else {
#if DEBUG_LOG
                summary += "\nNo initial/relative tile, skipping";
#endif
            }
        } else {
#if DEBUG_LOG
            summary += "\nNo marker or is already repositioning, skipping";
#endif
        }
        return true;
    }
    private Vector3 GetPositionToReposition(LocationGridTile p_gridTile, float p_distanceLimit, Vector3 p_relativeToPos, ref LocationGridTile p_chosenPositionGridTile) {
        List<LocationGridTile> tilesToCheck = RuinarchListPool<LocationGridTile>.Claim();
        p_gridTile.PopulateTilesInRadius(tilesToCheck, Mathf.CeilToInt(p_distanceLimit), includeCenterTile: true, includeTilesInDifferentStructure: true);
        Vector3 chosenPosition = Vector3.positiveInfinity;
        if (tilesToCheck.Count > 0) {
            for (int i = tilesToCheck.Count - 1; i >= 0; i--) {
                LocationGridTile tile = tilesToCheck[i];
                Vector3 pos = tile.GetUnoccupiedWalkablePositionInTileWithDistanceLimitOf(p_distanceLimit, p_relativeToPos);
                if (!pos.Equals(Vector3.positiveInfinity)) {
                    p_chosenPositionGridTile = tile;
                    chosenPosition = pos;
                    break;
                }
            }
        }
        RuinarchListPool<LocationGridTile>.Release(tilesToCheck);
        return chosenPosition;
    }
    private Vector3 GetPositionToRepositionRecursively(LocationGridTile p_gridTile, float p_distanceLimit, Vector3 p_relativeToPos, List<LocationGridTile> checkedTiles, ref LocationGridTile p_chosenPositionGridTile) {
        if (!checkedTiles.Contains(p_gridTile)) {
            checkedTiles.Add(p_gridTile);
            Vector3 pos = p_gridTile.GetUnoccupiedWalkablePositionInTileWithDistanceLimitOf(p_distanceLimit, p_relativeToPos);
            if (!pos.Equals(Vector3.positiveInfinity)) {
                p_chosenPositionGridTile = p_gridTile;
                return pos;
            }
        }
        for (int i = 0; i < p_gridTile.neighbourList.Count; i++) {
            LocationGridTile neighbourTile = p_gridTile.neighbourList[i];
            if (!checkedTiles.Contains(neighbourTile)) {
                checkedTiles.Add(neighbourTile);
                Vector3 pos = neighbourTile.GetUnoccupiedWalkablePositionInTileWithDistanceLimitOf(p_distanceLimit, p_relativeToPos);
                if (!pos.Equals(Vector3.positiveInfinity)) {
                    p_chosenPositionGridTile = neighbourTile;
                    return pos;
                }
            }
        }
        for (int i = 0; i < p_gridTile.neighbourList.Count; i++) {
            LocationGridTile neighbourTile = p_gridTile.neighbourList[i];
            Vector3 pos = GetPositionToRepositionRecursively(neighbourTile, p_distanceLimit, p_relativeToPos, checkedTiles, ref p_chosenPositionGridTile);
            if (!pos.Equals(Vector3.positiveInfinity)) {
                return pos;
            }
        }
        return Vector3.positiveInfinity;
    }
    private void OnArriveAfterCombatRepositioning() {
        SetGridNodeToReposition(null);
    }
#endregion

#region Attack
    private void Attack() {
        string summary = $"{stateComponent.owner.name} will attack {currentClosestHostile?.name}";

        if (stateComponent.owner.marker.isMoving) {
            if (currentClosestHostile is Character character) {
                if (character.hasMarker) {
                    if (!character.marker.isMoving) {
                        //When in range and in line of sight, and target is not moving, stop movement
                        stateComponent.owner.marker.StopMovement();    
                    }
                } else {
                    stateComponent.owner.marker.StopMovement();
                }
            }else {
                stateComponent.owner.marker.StopMovement();
            }
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
            isExecutingAttack = true;
            stateComponent.owner.marker.SetAnimationTrigger("Attack");
        }
        //Reset Attack Speed
        stateComponent.owner.marker.ResetAttackSpeed();
        // stateComponent.character.logComponent.PrintLogIfActive(summary);
        //Debug.Log(summary);
    }
#endregion

    public void OnAttackHit(IDamageable damageable) {
        string attackSummary = string.Empty;
#if DEBUG_LOG
        attackSummary =
            $"{GameManager.Instance.TodayLogString()}{stateComponent.owner.name} hit {damageable?.name ?? "Nothing"}";
#endif

        if (damageable != null && currentClosestHostile != null) {
            if (damageable != currentClosestHostile) {
#if DEBUG_LOG
                attackSummary =
                    $"{stateComponent.owner.name} hit {damageable.name} instead of {currentClosestHostile.name}!";
#endif
            }

            damageable.OnHitByAttackFrom(stateComponent.owner, this, ref attackSummary);

            if (damageable.currentHP > 0) {
#if DEBUG_LOG
                attackSummary += $"\n{damageable.name} still has remaining hp {damageable.currentHP.ToString()}/{damageable.maxHP.ToString()}";
#endif
                if (damageable is Character hitCharacter) {
                    //if the character that attacked is not in the hostile/avoid list of the character that was hit, this means that it is not a retaliation, so the character that was hit must reduce its opinion of the character that attacked
                    if(!hitCharacter.combatComponent.IsHostileInRange(stateComponent.owner) && !hitCharacter.combatComponent.IsAvoidInRange(stateComponent.owner)) {
                        if (!allCharactersThatDegradedRel.Contains(hitCharacter)) {
                            hitCharacter.relationshipContainer.AdjustOpinion(hitCharacter, stateComponent.owner, "Base", -15);
                            AddCharacterThatDegradedRel(hitCharacter);
                        }
                    }


                    //if the character that was hit is not the actual target of this combat, do not make him/her enter combat state
                    if (damageable == currentClosestHostile) {
                        //When the target is hit and it is still alive, add hostile
                        bool isAnAngelAttackingDemonicStructure = hitCharacter.race == RACE.ANGEL && hitCharacter.behaviourComponent.isAttackingDemonicStructure;
                        if (((hitCharacter.combatComponent.combatMode == COMBAT_MODE.Defend && !isAnAngelAttackingDemonicStructure) ||
                            hitCharacter.combatComponent.combatMode == COMBAT_MODE.Aggressive) && hitCharacter.limiterComponent.canPerform) {
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
#if DEBUG_LOG
        stateComponent.owner.logComponent.PrintLogIfActive(attackSummary);
#endif
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
#if DEBUG_LOG
        string log = $"Finished travelling flee path of {stateComponent.owner.name}";
        log += "\nFinished travelling flee path, determining action...";
        stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
        //After travelling flee path, check hostile characters if they are still in vision, every hostile character that is not in vision must be removed form hostile list
        //Consequently, the removed character must also remove this character from his/her hostile list
        //Then check if hostile list is empty
        //If it is, end state immediately
        //If not, flee again
        stateComponent.owner.marker.SetHasFleePath(false);
        //fleeChance = 10;
        EvaluateFleeingBecauseOfVulnerability(false);
        DetermineReaction(stateComponent.owner);
        stateComponent.owner.marker.UpdateAnimation();
        stateComponent.owner.marker.UpdateActionIcon();
    }
    private void UpdateFleePath() {
#if DEBUG_LOG
        string log = $"Updating flee path of {stateComponent.owner.name}";
        stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
        StartFlee(false);
    }
    private void StartFlee(bool shouldLog = true) {
        if (stateComponent.owner.combatComponent.avoidInRange.Count == 0) {
            return;
        }
        //Every time the character flees, he must set the closest hostile to null so that when he attacks again, he will recreate path towards the new closest hostile
        //We did this because of the bug that after a character flees, he will get stuck in combat because the combat state still thinks that he is pursuing the current closest hostile
        //That is why he will no longer reevaluate the hostile list and the will no longer create a path
        //https://trello.com/c/4JzJGQDR/2950-fleeing-while-butchering-makes-character-stuck-in-combat
        ResetClosestHostile();
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
        if (reason == CombatManager.Coward) {
            if (stateComponent.owner.HasAfflictedByPlayerWith("Coward")) {
                Coward coward = stateComponent.owner.traitContainer.GetTraitOrStatus<Coward>("Coward");
                coward.DispenseChaosOrbsForAffliction(stateComponent.owner, 1);
            }
        }
        if (reason == CombatManager.Vulnerable) {
            //Will go to party mate
            stateComponent.owner.marker.OnStartFleeToPartyMate();
        } else if (stateComponent.owner.currentStructure != null && stateComponent.owner.currentStructure.structureType.IsSpecialStructure()) {
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

        if (reason == "critically low health") {
            if (stateComponent.owner.partyComponent.hasParty) {
                Party party = stateComponent.owner.partyComponent.currentParty;
                if (party.isActive && party.partyState == PARTY_STATE.Working && party.currentQuest.partyQuestType == PARTY_QUEST_TYPE.Raid) {
                    stateComponent.owner.partyComponent.currentParty.RemoveMemberThatJoinedQuest(stateComponent.owner);
                }
            }
        }

        if (shouldLog) {
            Log fleeLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "NonIntel", "start_flee", providedTags: LOG_TAG.Combat);
            fleeLog.AddToFillers(stateComponent.owner, stateComponent.owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            fleeLog.AddToFillers(objToAvoid, objToAvoid is GenericTileObject ? "something" : objToAvoid.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            fleeLog.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
            stateComponent.owner.logComponent.RegisterLog(fleeLog);
            SetThoughtBubbleLog(fleeLog);
        }
    }
    public void OnReachLowFleeSpeedThreshold() {
#if DEBUG_LOG
        string log = $"{stateComponent.owner.name} has reached low flee speed threshold, determining action...";
        stateComponent.owner.logComponent.PrintLogIfActive(log);
#endif
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
    private void EvaluateFleeingBecauseOfVulnerability(bool processCombatBehaviour) {
        bool hasPartymateInVision = stateComponent.owner.partyComponent.HasPartymateInVision();
        bool hasRemovedAvoidInRange = false;
        if (hasPartymateInVision) {
            for (int i = 0; i < stateComponent.owner.combatComponent.avoidInRange.Count; i++) {
                IPointOfInterest poi = stateComponent.owner.combatComponent.avoidInRange[i];
                CombatData data = stateComponent.owner.combatComponent.GetCombatData(poi);
                if(data.avoidReason == CombatManager.Vulnerable) {
                    if(stateComponent.owner.combatComponent.RemoveAvoidInRange(poi, false)) {
                        hasRemovedAvoidInRange = true;
                        i--;
                    }
                }
            }
        }
        if(hasRemovedAvoidInRange && processCombatBehaviour) {
            stateComponent.owner.combatComponent.SetWillProcessCombat(true);
        }
    }
#endregion

#region Utilities
    public void ResetClosestHostile() {
        IPointOfInterest prevHostile = currentClosestHostile;
        currentClosestHostile = null;
        if (prevHostile != null && stateComponent.owner.marker.targetPOI == prevHostile && stateComponent.owner.marker) {
            stateComponent.owner.marker.SetTargetPOI(null);
        }
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

    public override void Reset() {
        base.Reset();
        _currentAttackTimer = 0;
        _hasTimerStarted = false;
        isAttacking = false;
        currentClosestHostile = null;
        forcedTarget = null;
        allCharactersThatDegradedRel.Clear();
        allCharactersThatReactedToThisCombat.Clear();
        endedInternally = false;
        lastFledFrom = null;
        lastFledFromStructure = null;
        isBeingApprehended = false;
        isFleeToHome = false;
        _timesHitCurrentTarget = 0;
        isExecutingAttack = false;
        repositioningTo = null;
    }
}
