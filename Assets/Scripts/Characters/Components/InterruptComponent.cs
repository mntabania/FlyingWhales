using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

public class InterruptComponent {
    public Character owner { get; private set; }
    public IPointOfInterest currentTargetPOI { get; private set; }
    public Interrupt currentInterrupt { get; private set; }
    public int currentDuration { get; private set; }
    public string identifier { get; private set; }
    public Interrupt triggeredSimultaneousInterrupt { get; private set; }
    public int currentSimultaneousInterruptDuration { get; private set; }

    public Log thoughtBubbleLog { get; private set; }

    private Log _currentEffectLog;

    #region getters
    public bool isInterrupted => currentInterrupt != null;
    public bool hasTriggeredSimultaneousInterrupt => triggeredSimultaneousInterrupt != null;
    public Log currentEffectLog => _currentEffectLog;
    #endregion

    public InterruptComponent(Character owner) {
        this.owner = owner;
        identifier = string.Empty;
    }

    #region General
    public bool TriggerInterrupt(INTERRUPT interrupt, IPointOfInterest targetPOI, string identifier = "") {
        Interrupt triggeredInterrupt = InteractionManager.Instance.GetInterruptData(interrupt);
        if (!triggeredInterrupt.isSimulateneous) {
            if (isInterrupted) {
                owner.logComponent.PrintLogIfActive(
                    $"Cannot trigger interrupt {interrupt} because there is already a current interrupt: {currentInterrupt.name}");
                return false;
            }
            owner.logComponent.PrintLogIfActive(
                $"{owner.name} triggered a non simultaneous interrupt: {triggeredInterrupt.name}");
            
            currentInterrupt = triggeredInterrupt;
            currentTargetPOI = targetPOI;
            this.identifier = identifier;
            
            CreateThoughtBubbleLog();

            if (ReferenceEquals(owner.marker, null) == false && owner.marker.isMoving) {
                owner.marker.StopMovement();
                owner.marker.SetHasFleePath(false);
            }
            if (currentInterrupt.doesDropCurrentJob) {
                owner.currentJob?.CancelJob(false);
            }
            if (currentInterrupt.doesStopCurrentAction) {
                owner.currentJob?.StopJobNotDrop();
            }
            ExecuteStartInterrupt(triggeredInterrupt, targetPOI);
            Messenger.Broadcast(Signals.INTERRUPT_STARTED, owner, currentTargetPOI, currentInterrupt);
            Messenger.Broadcast(Signals.UPDATE_THOUGHT_BUBBLE, owner);

            if (currentInterrupt.duration <= 0) {
                AddEffectLog(currentInterrupt, currentTargetPOI);
                currentInterrupt.ExecuteInterruptEndEffect(owner, currentTargetPOI);
                EndInterrupt();
            }
        } else {
             TriggeredSimultaneousInterrupt(triggeredInterrupt, targetPOI, identifier);
        }
        return true;
    }
    public void SetIdentifier(string text) {
        identifier = text;
    }
    private bool TriggeredSimultaneousInterrupt(Interrupt interrupt, IPointOfInterest targetPOI, string identifier) {
        owner.logComponent.PrintLogIfActive($"{owner.name} triggered a simultaneous interrupt: {interrupt.name}");
        triggeredSimultaneousInterrupt = interrupt;
        this.identifier = identifier;
        ExecuteStartInterrupt(interrupt, targetPOI);
        AddEffectLog(triggeredSimultaneousInterrupt, currentTargetPOI);
        interrupt.ExecuteInterruptEndEffect(owner, currentTargetPOI);
        currentSimultaneousInterruptDuration = 0;
        Messenger.AddListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
        return true;
    }
    private void ExecuteStartInterrupt(Interrupt interrupt, IPointOfInterest targetPOI) {
        _currentEffectLog = null;
        interrupt.ExecuteInterruptStartEffect(owner, targetPOI, ref _currentEffectLog);
        if(_currentEffectLog == null) {
            _currentEffectLog = interrupt.CreateEffectLog(owner, targetPOI);
        }
        if (owner.marker) {
            owner.marker.UpdateActionIcon();
        }
    }
    public void OnTickEnded() {
        if (isInterrupted) {
            currentDuration++;
            if(currentDuration >= currentInterrupt.duration) {
                AddEffectLog(currentInterrupt, currentTargetPOI);
                currentInterrupt.ExecuteInterruptEndEffect(owner, currentTargetPOI);
                EndInterrupt();
            }
        }
    }
    private void PerTickSimultaneousInterrupt() {
        if (hasTriggeredSimultaneousInterrupt) {
            currentSimultaneousInterruptDuration++;
            if (currentSimultaneousInterruptDuration > 2) {
                Messenger.RemoveListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
                triggeredSimultaneousInterrupt = null;
                if (owner.marker) {
                    owner.marker.UpdateActionIcon();
                }
            }
        }
    }
    //public void ForceEndAllInterrupt() {
    //    ForceEndNonSimultaneousInterrupt();
    //    ForceEndSimultaneousInterrupt();
    //}
    public void ForceEndNonSimultaneousInterrupt() {
        if (isInterrupted) {
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
    private void EndInterrupt() {
        bool willCheckInVision = currentInterrupt.duration > 0;
        Interrupt finishedInterrupt = currentInterrupt;
        currentInterrupt = null;
        currentDuration = 0;
        if(!owner.isDead && owner.canPerform) {
            if (owner.isInCombat) {
                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
            } else {
                if (owner.combatComponent.hostilesInRange.Count > 0 || owner.combatComponent.avoidInRange.Count > 0) {
                    CharacterStateJob job = JobManager.Instance.CreateNewCharacterStateJob(JOB_TYPE.COMBAT, CHARACTER_STATE.COMBAT, owner);
                    owner.jobQueue.AddJobInQueue(job);
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
        if (owner.marker) {
            owner.marker.UpdateActionIcon();
        }
        Messenger.Broadcast(Signals.INTERRUPT_FINISHED, finishedInterrupt.interrupt, owner);
    }
    private void CreateThoughtBubbleLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", currentInterrupt.name, "thought_bubble")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "Interrupt", currentInterrupt.name, "thought_bubble");
            thoughtBubbleLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(currentTargetPOI, currentTargetPOI.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }
    private void AddEffectLog(Interrupt interrupt, IPointOfInterest target) {
        if(_currentEffectLog != null) {
            if (owner != currentTargetPOI) {
                _currentEffectLog.AddLogToInvolvedObjects();
            } else {
                owner.logComponent.AddHistory(_currentEffectLog);
            }
            if (interrupt.isIntel) {
                PlayerManager.Instance.player.ShowNotificationFrom(owner, InteractionManager.Instance.CreateNewIntel(interrupt, owner, target, _currentEffectLog) as IIntel);
                // PlayerManager.Instance.player.ShowNotification(InteractionManager.Instance.CreateNewIntel(interrupt, owner, target, _currentEffectLog) as IIntel);
            } else {
                PlayerManager.Instance.player.ShowNotificationFrom(owner, _currentEffectLog);
            }
        }
        //if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", currentInterrupt.name, "effect")) {
        //    Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", currentInterrupt.name, "effect");
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
    //        Log effectLog = new Log(GameManager.Instance.Today(), "Interrupt", interrupt.name, "effect");
    //        effectLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //        effectLog.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //        effectLog.AddLogToInvolvedObjects();
    //        PlayerManager.Instance.player.ShowNotificationFrom(owner, effectLog);
    //    }
    //}
    #endregion
}