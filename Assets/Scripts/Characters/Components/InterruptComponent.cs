using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;
using Inner_Maps;

public class InterruptComponent {
    public Character owner { get; private set; }
    public InterruptHolder currentInterrupt { get; private set; }
    public int currentDuration { get; private set; }
    public string identifier { get; private set; }
    public string simultaneousIdentifier { get; private set; }
    public InterruptHolder triggeredSimultaneousInterrupt { get; private set; }
    public int currentSimultaneousInterruptDuration { get; private set; }

    public Log thoughtBubbleLog { get; private set; }

    //private Log _currentEffectLog;

    #region getters
    public bool isInterrupted => currentInterrupt != null;
    public bool hasTriggeredSimultaneousInterrupt => triggeredSimultaneousInterrupt != null;
    //public Log currentEffectLog => _currentEffectLog;
    #endregion

    public InterruptComponent(Character owner) {
        this.owner = owner;
        identifier = string.Empty;
        simultaneousIdentifier = string.Empty;
    }

    #region General
    public bool TriggerInterrupt(INTERRUPT interrupt, IPointOfInterest targetPOI, string identifier = "", ActualGoapNode actionThatTriggered = null) {
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
            interruptHolder.Initialize(triggeredInterrupt, owner, targetPOI, identifier);
            SetNonSimultaneousInterrupt(interruptHolder);
            this.identifier = identifier;
            
            CreateThoughtBubbleLog();

            if (ReferenceEquals(owner.marker, null) == false && owner.marker.isMoving) {
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
            Messenger.Broadcast(Signals.INTERRUPT_STARTED, currentInterrupt);
            Messenger.Broadcast(Signals.UPDATE_THOUGHT_BUBBLE, owner);

            if (currentInterrupt.interrupt.duration <= 0) {
                AddEffectLog(currentInterrupt);
                currentInterrupt.interrupt.ExecuteInterruptEndEffect(owner, currentInterrupt.target);
                EndInterrupt();
            }
        } else {
             TriggeredSimultaneousInterrupt(triggeredInterrupt, targetPOI, identifier, actionThatTriggered);
        }
        return true;
    }
    private bool TriggeredSimultaneousInterrupt(Interrupt interrupt, IPointOfInterest targetPOI, string identifier, ActualGoapNode actionThatTriggered) {
        owner.logComponent.PrintLogIfActive($"{owner.name} triggered a simultaneous interrupt: {interrupt.name}");
        bool alreadyHasSimultaneousInterrupt = hasTriggeredSimultaneousInterrupt;
        InterruptHolder interruptHolder = ObjectPoolManager.Instance.CreateNewInterrupt();
        interruptHolder.Initialize(interrupt, owner, targetPOI, identifier);
        SetSimultaneousInterrupt(interruptHolder);
        simultaneousIdentifier = identifier;
        ExecuteStartInterrupt(triggeredSimultaneousInterrupt, actionThatTriggered);
        AddEffectLog(triggeredSimultaneousInterrupt);
        interrupt.ExecuteInterruptEndEffect(owner, targetPOI);
        currentSimultaneousInterruptDuration = 0;
        if (!alreadyHasSimultaneousInterrupt) {
            Messenger.AddListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
        }
        return true;
    }
    private void ExecuteStartInterrupt(InterruptHolder interruptHolder, ActualGoapNode actionThatTriggered) {
        Log effectLog = null;
        interruptHolder.interrupt.ExecuteInterruptStartEffect(owner, interruptHolder.target, ref effectLog, actionThatTriggered);
        if(effectLog == null) {
            effectLog = interruptHolder.interrupt.CreateEffectLog(owner, interruptHolder.target);
        }
        interruptHolder.SetEffectLog(effectLog);
        if (owner.marker) {
            owner.marker.UpdateActionIcon();
        }
        InnerMapManager.Instance.FaceTarget(owner, interruptHolder.target);
    }
    public void OnTickEnded() {
        if (isInterrupted) {
            currentDuration++;
            if(currentDuration >= currentInterrupt.interrupt.duration) {
                AddEffectLog(currentInterrupt);
                currentInterrupt.interrupt.ExecuteInterruptEndEffect(owner, currentInterrupt.target);
                EndInterrupt();
            }
        }
    }
    private void PerTickSimultaneousInterrupt() {
        if (hasTriggeredSimultaneousInterrupt) {
            currentSimultaneousInterruptDuration++;
            if (currentSimultaneousInterruptDuration > 2) {
                Messenger.RemoveListener(Signals.TICK_ENDED, PerTickSimultaneousInterrupt);
                SetSimultaneousInterrupt(null);
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
        bool willCheckInVision = currentInterrupt.interrupt.duration > 0;
        Interrupt finishedInterrupt = currentInterrupt.interrupt;
        SetNonSimultaneousInterrupt(null);
        currentDuration = 0;
        if(!owner.isDead && owner.canPerform) {
            if (owner.combatComponent.isInCombat) {
                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, owner);
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
        if (owner.marker) {
            owner.marker.UpdateActionIcon();
        }
        Messenger.Broadcast(Signals.INTERRUPT_FINISHED, finishedInterrupt.type, owner);
    }
    private void CreateThoughtBubbleLog() {
        if (LocalizationManager.Instance.HasLocalizedValue("Interrupt", currentInterrupt.name, "thought_bubble")) {
            thoughtBubbleLog = new Log(GameManager.Instance.Today(), "Interrupt", currentInterrupt.name, "thought_bubble");
            thoughtBubbleLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(currentInterrupt.target, currentInterrupt.target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
    }
    private void AddEffectLog(InterruptHolder interruptHolder) {
        if(interruptHolder.effectLog != null) {
            if (interruptHolder.interrupt.shouldAddLogs) {
                if (owner != interruptHolder.target) {
                    interruptHolder.effectLog.AddLogToInvolvedObjects();
                } else {
                    owner.logComponent.AddHistory(interruptHolder.effectLog);
                }    
            }
            if (interruptHolder.interrupt.isIntel) {
                PlayerManager.Instance.player.ShowNotificationFrom(owner, InteractionManager.Instance.CreateNewIntel(interruptHolder.interrupt, interruptHolder.actor, interruptHolder.target, interruptHolder.effectLog) as IIntel);
                // PlayerManager.Instance.player.ShowNotification(InteractionManager.Instance.CreateNewIntel(interrupt, owner, target, _currentEffectLog) as IIntel);
            } else {
                PlayerManager.Instance.player.ShowNotificationFrom(owner, interruptHolder.effectLog);
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
    public void SetIdentifier(string text, bool isSimultaneous) {
        if (isSimultaneous) {
            simultaneousIdentifier = text;
        } else {
            identifier = text;
        }
    }
    public void SetNonSimultaneousInterrupt(InterruptHolder interrupt) {
        if (currentInterrupt != interrupt) {
            if (currentInterrupt != null) {
                ObjectPoolManager.Instance.ReturnInterruptToPool(currentInterrupt);
            }
            currentInterrupt = interrupt;
        }
    }
    public void SetSimultaneousInterrupt(InterruptHolder interrupt) {
        if(triggeredSimultaneousInterrupt != interrupt) {
            if(triggeredSimultaneousInterrupt != null) {
                ObjectPoolManager.Instance.ReturnInterruptToPool(triggeredSimultaneousInterrupt);
            }
            triggeredSimultaneousInterrupt = interrupt;
        }
    }
    #endregion
}