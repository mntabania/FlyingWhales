using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Interrupts;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Logs;
using UnityEngine.Assertions;

public class InterruptComponent : CharacterComponent {
    public InterruptHolder currentInterrupt { get; private set; }
    public int currentDuration { get; private set; }
    //public string identifier { get; private set; }
    //public string simultaneousIdentifier { get; private set; }
    public InterruptHolder triggeredSimultaneousInterrupt { get; private set; }
    public int currentSimultaneousInterruptDuration { get; private set; }
    public BaseSettlement raidTargetSettlement { get; private set; }

    public Log thoughtBubbleLog { get; private set; }

    //private List<System.Action> _pendingSimultaneousInterrupts;

    #region getters
    public bool isInterrupted => currentInterrupt != null;
    public bool hasTriggeredSimultaneousInterrupt => triggeredSimultaneousInterrupt != null;
    //public Log currentEffectLog => _currentEffectLog;
    #endregion

    public InterruptComponent() {
        //_pendingSimultaneousInterrupts = new List<Action>();
        //identifier = string.Empty;
        //simultaneousIdentifier = string.Empty;
    }
    public InterruptComponent(SaveDataInterruptComponent data) {
        //_pendingSimultaneousInterrupts = new List<Action>();
        currentDuration = data.currentDuration;
        currentSimultaneousInterruptDuration = data.currentSimultaneousInterruptDuration;
    }

    #region General
    public bool TriggerInterrupt(INTERRUPT interrupt, IPointOfInterest targetPOI, string identifier = "", ActualGoapNode actionThatTriggered = null, string reason = "") {
        Interrupt triggeredInterrupt = InteractionManager.Instance.GetInterruptData(interrupt);
        if (!triggeredInterrupt.isSimulateneous) {
            if (isInterrupted) {
                owner.logComponent.PrintLogIfActive(
                    $"Cannot trigger interrupt {interrupt} because there is already a current interrupt: {currentInterrupt.name}");
                return false;
            }
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} triggered a non simultaneous interrupt: {triggeredInterrupt.name}");

            InterruptHolder interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
            interruptHolder.Initialize(triggeredInterrupt, owner, targetPOI, identifier, reason);
            SetNonSimultaneousInterrupt(interruptHolder);
            //this.identifier = identifier;
            
            CreateThoughtBubbleLog(triggeredInterrupt);

            if (ReferenceEquals(owner.marker, null) == false && owner.marker.isMoving && triggeredInterrupt.shouldStopMovement) {
                owner.marker.StopMovement();
                owner.marker.SetHasFleePath(false);
            }
            if (currentInterrupt.interrupt.doesDropCurrentJob) {
                owner.currentJob?.CancelJob(false);
            }
            if (currentInterrupt.interrupt.doesStopCurrentAction) {
                owner.currentJob?.StopJobNotDrop();
            }
            ExecuteStartInterrupt(currentInterrupt, actionThatTriggered);
            Messenger.Broadcast(InterruptSignals.INTERRUPT_STARTED, currentInterrupt);
            Messenger.Broadcast(UISignals.UPDATE_THOUGHT_BUBBLE, owner);

            if (currentInterrupt.interrupt.duration <= 0) {
                AddEffectLog(currentInterrupt);
                currentInterrupt.interrupt.ExecuteInterruptEndEffect(currentInterrupt);
                EndInterrupt();
            }
        } else {
             TriggeredSimultaneousInterrupt(triggeredInterrupt, targetPOI, identifier, actionThatTriggered, reason);
        }
        return true;
    }
    private void TriggeredSimultaneousInterrupt(Interrupt interrupt, IPointOfInterest targetPOI, string identifier, ActualGoapNode actionThatTriggered, string reason) {
        owner.logComponent.PrintLogIfActive($"{owner.name} triggered a simultaneous interrupt: {interrupt.name}");
        //if (hasTriggeredSimultaneousInterrupt) {
        //    //character is currently running a simultaneous interrupt.
        //    AddPendingSimultaneousInterrupt(() => TriggeredSimultaneousInterrupt(interrupt, targetPOI, identifier, actionThatTriggered, reason));
        //    return;
        //}
        InterruptHolder newTriggeredInterrupt = ObjectPoolManager.Instance.CreateNewInterrupt();
        newTriggeredInterrupt.Initialize(interrupt, owner, targetPOI, identifier, reason);
        ExecuteStartInterrupt(newTriggeredInterrupt, actionThatTriggered);
        AddEffectLog(newTriggeredInterrupt);
        interrupt.ExecuteInterruptEndEffect(newTriggeredInterrupt);

        //Note: The stored triggeredSimultaneousInterrupt is only used for the thought bubble action icon, for the character to show the current interrupt action icon, like when chatting or flirting
        //It should not be used for anything else because the simultaneous interrupt has no duration so that means it will only go through the process once and then it is over, so there is no need for it to be stored in any way other than this
        //The reason why we moved the setting of simultaneous interrupt here because since the stored simultaneous interrupt has no connection with the actual processing of it, it is best to process it first then set the simultaneous interrupt to avoid conflicts
        //In the previous code, there are a lot of issues because once we set the simultaneous interrupt we access the triggeredSimultaneousInterrupt instead of just accessing the newTriggeredInterrupt
        //Accessing the triggeredSimultaneousInterrupt will have a lot of issues because there can have many simultaneous interrupts triggerring at the same time so the value of it can change many times in 1 frame
        //There will also be times that when a simultaneous interrupt is triggered, another simultaneous interrupt will be triggered while the other interrupt is being processed, hence, this solution
        //Example: When the Leave Faction interrupt is triggered the character that left the faction will also leave the home settlement, and when a character leaves a home settlement and he has a party, he will also leave the party triggering another simultaneous interrupt: Leave Party
        //So this means that in the process of Leave Faction interrupt, the Leave Party interrupt is triggered, causing errors in our previoud code, that is why we did this solution
        bool alreadyHasSimultaneousInterrupt = hasTriggeredSimultaneousInterrupt;
        SetSimultaneousInterrupt(newTriggeredInterrupt);

        currentSimultaneousInterruptDuration = 0;
        if (!alreadyHasSimultaneousInterrupt) {
            Messenger.AddListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
        } 
        //else {
        //    if (owner.marker) {
        //        owner.marker.UpdateActionIcon();
        //    }
        //}
    }
    //private void AddPendingSimultaneousInterrupt(System.Action pendingSimultaneousInterrupt) {
    //    _pendingSimultaneousInterrupts.Add(pendingSimultaneousInterrupt);
    //}
    //private void TryExecutePendingSimultaneousInterrupt() {
    //    if (_pendingSimultaneousInterrupts.Count > 0) {
    //        _pendingSimultaneousInterrupts.First().Invoke();
    //        _pendingSimultaneousInterrupts.RemoveAt(0);
    //    }
    //}
    private void ExecuteStartInterrupt(InterruptHolder interruptHolder, ActualGoapNode actionThatTriggered) {
        Log effectLog = GameManager.CreateNewLog();
        Assert.IsNotNull(interruptHolder, $"Interrupt Holder of {owner.name} is null!");
        Assert.IsNotNull(interruptHolder.interrupt, $"Interrupt in interrupt holder {interruptHolder} used by {owner.name} is null!");
        INTERRUPT interruptType = interruptHolder.interrupt.type;
        interruptHolder.interrupt.ExecuteInterruptStartEffect(interruptHolder, ref effectLog, actionThatTriggered);
        interruptHolder.SetCrimeType();
        
        Assert.IsNotNull(interruptHolder, $"Interrupt Holder of {owner.name} became null after executing start effect of {interruptType.ToString()}!");
        Assert.IsNotNull(interruptHolder.interrupt, $"Interrupt in interrupt holder {interruptHolder} used by {owner.name} became null after executing start effect of {interruptType.ToString()}!");
        
        if(!effectLog.hasValue) {
            effectLog = interruptHolder.interrupt.CreateEffectLog(owner, interruptHolder.target);
        }
        if (effectLog.hasValue && interruptHolder.interrupt.isIntel) {
            effectLog.AddTag(LOG_TAG.Intel);
        }
        interruptHolder.SetEffectLog(effectLog);
        //if (owner.marker) {
        //    owner.marker.UpdateActionIcon();
        //}
        InnerMapManager.Instance.FaceTarget(owner, interruptHolder.target);
    }
    public void OnTickEnded() {
        if (isInterrupted) {
            currentDuration++;
            if(currentDuration >= currentInterrupt.interrupt.duration) {
                AddEffectLog(currentInterrupt);
                currentInterrupt.interrupt.ExecuteInterruptEndEffect(currentInterrupt);
                EndInterrupt();
            } else {
                currentInterrupt.interrupt.PerTickInterrupt(currentInterrupt);
            }
        }
    }
    private void PerTickSimultaneousInterrupt() {
        if (hasTriggeredSimultaneousInterrupt) {
            currentSimultaneousInterruptDuration++;
            if (currentSimultaneousInterruptDuration > 2) {
                Messenger.RemoveListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
                SetSimultaneousInterrupt(null);
                //if (owner.marker) {
                //    owner.marker.UpdateActionIcon();
                //}
                //TryExecutePendingSimultaneousInterrupt();
            }
        }
    }
    //public void ForceEndAllInterrupt() {
    //    ForceEndNonSimultaneousInterrupt();
    //    ForceEndSimultaneousInterrupt();
    //}
    public void ForceEndNonSimultaneousInterrupt() {
        if (isInterrupted) {
            currentInterrupt.interrupt.OnForceEndInterrupt(currentInterrupt);
            EndInterrupt();
        }
    }
    //public void ForceEndSimultaneousInterrupt() {
    //    if (hasTriggeredSimultaneousInterrupt) {
    //        triggeredSimultaneousInterrupt = null;
    //        if (owner.marker) {
    //            owner.marker.UpdateActionIcon();
    //        }
    //    }
    //}
    private void EndInterrupt() { //bool shouldAddLog = false
        if (currentInterrupt == null || currentInterrupt.interrupt == null) {
            //Will not process anymore if there is no current interrupt, this means that the interrupt has already been ended before and has returned to the object pool
            //This can happen if the actor is a minion and the ExecuteInterruptEndEffect triggers the death of the actor
            //In this manner, the minion actor will have already ended the interrupt during death because we the ForceEndNonSimultaneousInterrupt is being called when a minion dies/unsummoned
            //so when the EndInterrupt is called again after the ExecuteInterruptEndEffect call, the current interrupt is already null
            //if (owner.marker) {
            //    owner.marker.UpdateActionIcon();
            //}
            return;
        }
        //if (shouldAddLog) {
        //    AddEffectLog(currentInterrupt);
        //}
        bool willCheckInVision = currentInterrupt.interrupt.duration > 0;
        Interrupt finishedInterrupt = currentInterrupt.interrupt;
        SetNonSimultaneousInterrupt(null);
        currentDuration = 0;
        if(!owner.isDead && owner.limiterComponent.canPerform) {
            if (owner.combatComponent.isInCombat) {
                Messenger.Broadcast(CharacterSignals.DETERMINE_COMBAT_REACTION, owner);
            } else {
                if (owner.combatComponent.hostilesInRange.Count > 0 || owner.combatComponent.avoidInRange.Count > 0) {
                    if (owner.jobQueue.HasJob(JOB_TYPE.COMBAT) == false) {
                        CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, owner);
                        owner.jobQueue.AddJobInQueue(job);    
                    }
                } else {
                    if (willCheckInVision) {
                        if (owner.marker) {
                            for (int i = 0; i < owner.marker.inVisionCharacters.Count; i++) {
                                Character inVisionCharacter = owner.marker.inVisionCharacters[i];
                                // owner.CreateJobsOnEnterVisionWith(inVisionCharacter);
                                owner.marker.AddUnprocessedPOI(inVisionCharacter);
                            }
                        }
                        owner.needsComponent.CheckExtremeNeeds(finishedInterrupt);
                    }
                }
            }
        }
        //if (owner.marker) {
        //    owner.marker.UpdateActionIcon();
        //}
        thoughtBubbleLog = default;
        Messenger.Broadcast(CharacterSignals.INTERRUPT_FINISHED, finishedInterrupt.type, owner);
    }
    private void CreateThoughtBubbleLog(Interrupt interrupt) {
        if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", currentInterrupt.name, "thought_bubble")) {
            thoughtBubbleLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", currentInterrupt.name, "thought_bubble", providedTags: interrupt.logTags);
            thoughtBubbleLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(currentInterrupt.target, currentInterrupt.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            interrupt.AddAdditionalFillersToThoughtLog(thoughtBubbleLog, owner);
        }
    }
    private void AddEffectLog(InterruptHolder interruptHolder) {
        if(interruptHolder.effectLog.hasValue) {
            if (interruptHolder.interrupt.ShouldAddLogs(interruptHolder) || interruptHolder.interrupt.shouldShowNotif) {
                interruptHolder.effectLog.AddLogToDatabase();
            }
            // if (interruptHolder.interrupt.shouldAddLogs) {
            //     // if (owner != interruptHolder.target) {
            //     //     interruptHolder.effectLog.AddLogToInvolvedObjects();
            //     // } else {
            //     //     owner.logComponent.AddHistory(interruptHolder.effectLog);
            //     //     interruptHolder.effectLog.AddLogToSpecificObjects(LOG_IDENTIFIER.FACTION_1,
            //     //         LOG_IDENTIFIER.FACTION_2, LOG_IDENTIFIER.FACTION_3);
            //     // }    
            // }
            if (interruptHolder.interrupt.shouldShowNotif) {
                if (interruptHolder.interrupt.type == INTERRUPT.Create_Party || interruptHolder.interrupt.type == INTERRUPT.Mental_Break) {
                    PlayerManager.Instance.player.ShowNotificationFromPlayer(interruptHolder.effectLog);
                } else {
                    if (interruptHolder.interrupt.isIntel) {
                        PlayerManager.Instance.player.ShowNotificationFrom(owner, InteractionManager.Instance.CreateNewIntel(interruptHolder) as IIntel);
                        // PlayerManager.Instance.player.ShowNotification(InteractionManager.Instance.CreateNewIntel(interrupt, owner, target, _currentEffectLog) as IIntel);
                    } else {
                        PlayerManager.Instance.player.ShowNotificationFrom(owner, interruptHolder.effectLog);
                    }
                }
            }
        }
        //if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", currentInterrupt.name, "effect")) {
        //    Log effectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", currentInterrupt.name, "effect");
        //    effectLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    effectLog.AddToFillers(currentTargetPOI, currentTargetPOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //    effectLog.AddLogToInvolvedObjects();
        //    PlayerManager.Instance.player.ShowNotificationFrom(owner, effectLog);
        //} 
        //else {
        //    Debug.LogWarning(currentInterrupt.name + " interrupt does not have effect log!");
        //}
    }
    //private void CreateAndAddEffectLog(Interrupt interrupt, IPointOfInterest target) {
    //    if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", interrupt.name, "effect")) {
    //        Log effectLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Interrupt", interrupt.name, "effect");
    //        effectLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //        effectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //        effectLog.AddLogToInvolvedObjects();
    //        PlayerManager.Instance.player.ShowNotificationFrom(owner, effectLog);
    //    }
    //}
    //public void SetIdentifier(string text, bool isSimultaneous) {
    //    if (isSimultaneous) {
    //        simultaneousIdentifier = text;
    //    } else {
    //        identifier = text;
    //    }
    //}
    public void SetNonSimultaneousInterrupt(InterruptHolder interrupt) {
        if (currentInterrupt != interrupt) {
            if (currentInterrupt != null) {
                ObjectPoolManager.Instance.ReturnInterruptToPool(currentInterrupt);
            }
            currentInterrupt = interrupt;
            if (owner.marker) {
                owner.marker.UpdateActionIcon();
            }
        }
    }
    public void SetSimultaneousInterrupt(InterruptHolder interrupt) {
        if(triggeredSimultaneousInterrupt != interrupt) {
            if(triggeredSimultaneousInterrupt != null) {
                ObjectPoolManager.Instance.ReturnInterruptToPool(triggeredSimultaneousInterrupt);
            }
            triggeredSimultaneousInterrupt = interrupt;
            if (owner.marker) {
                owner.marker.UpdateActionIcon();
            }
        }
    }
    public void OnSeizedOwner() {
        if(isInterrupted && currentInterrupt.interrupt.shouldEndOnSeize) {
            ForceEndNonSimultaneousInterrupt();
        }
    }
    #endregion

    #region Miscellaneous
    public void SetRaidTargetSettlement(BaseSettlement settlement) {
        raidTargetSettlement = settlement;
    }
    #endregion

    #region Necromancer
    public bool NecromanticTranform() {
        if (CanNecromanticTransform()) {
            if (owner.HasItem("Necronomicon")) {
                return owner.interruptComponent.TriggerInterrupt(INTERRUPT.Necromantic_Transformation, owner);
            }
        }
        return false;
    }
    public bool CanNecromanticTransform() {
        if(CharacterManager.Instance.necromancerInTheWorld == null && owner.characterClass.className != "Necromancer") {
            return owner.traitContainer.HasTrait("Evil", "Treacherous", "Cultist");
        }
        return false; 
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataInterruptComponent data) {
        if (!string.IsNullOrEmpty(data.currentInterruptID)) {
            currentInterrupt = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.currentInterruptID);
            CreateThoughtBubbleLog(currentInterrupt.interrupt);
        }
        if (!string.IsNullOrEmpty(data.triggeredSimultaneousInterruptID)) {
            triggeredSimultaneousInterrupt = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(data.triggeredSimultaneousInterruptID);
            Messenger.AddListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataInterruptComponent : SaveData<InterruptComponent> {
    public string currentInterruptID;
    public int currentDuration;
    public string triggeredSimultaneousInterruptID;
    public int currentSimultaneousInterruptDuration;

    #region Overrides
    public override void Save(InterruptComponent data) {
        currentDuration = data.currentDuration;
        currentSimultaneousInterruptDuration = data.currentSimultaneousInterruptDuration;
        if(data.currentInterrupt != null && data.currentInterrupt.interrupt != null) {
            currentInterruptID = data.currentInterrupt.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.currentInterrupt);
        }
        if (data.triggeredSimultaneousInterrupt != null && data.triggeredSimultaneousInterrupt.interrupt != null) {
            triggeredSimultaneousInterruptID = data.triggeredSimultaneousInterrupt.persistentID;
            SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(data.triggeredSimultaneousInterrupt);
        }
    }

    public override InterruptComponent Load() {
        InterruptComponent component = new InterruptComponent(this);
        return component;
    }
    #endregion
}