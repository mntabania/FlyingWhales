using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Watch : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public Watch() : base(INTERACTION_TYPE.WATCH) {
        actionIconString = GoapActionStateDB.Watch_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Watch Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        object[] otherData = node.otherData;
        if (otherData.Length == 1) {
            if (otherData[0] is GoapAction) {
                GoapAction actionBeingWatched = otherData[0] as GoapAction;
                //actionIconString = actionBeingWatched.actionIconString;
                log.AddToFillers(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.STRING_1);
            } else if (otherData[0] is CombatState) {
                CharacterState stateBeingWatched = otherData[0] as CombatState;
                log.AddToFillers(stateBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
            } else if (otherData[0] is DouseFireState) {
                CharacterState stateBeingWatched = otherData[0] as DouseFireState;
                log.AddToFillers(stateBeingWatched, "Douse Fire", LOG_IDENTIFIER.STRING_1);
                log.AddToFillers(stateBeingWatched, "Douse Fire", LOG_IDENTIFIER.STRING_1);
            }
        }
    }
    //public override void OnStopWhilePerforming() {
    //    base.OnStopWhilePerforming();
    //    //if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
    //    //    Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
    //    //}

    //    if (shouldAddLogs && currentState.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
    //        currentState.descriptionLog.SetDate(GameManager.Instance.Today());
    //        currentState.descriptionLog.AddLogToInvolvedObjects();
    //    }
    //}
    #endregion

    #region State Effects
    public void PreWatchSuccess(ActualGoapNode goapNode) {
        GoapAction actionBeingWatched = goapNode.otherData[0] as GoapAction;
        CharacterState stateBeingWatched = goapNode.otherData[0] as CharacterState;
        if (actionBeingWatched != null) {
            goapNode.descriptionLog.AddToFillers(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.STRING_1);
        }else if (stateBeingWatched != null) {
            if (stateBeingWatched is CombatState) {
                goapNode.descriptionLog.AddToFillers(stateBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
            } else if (stateBeingWatched is DouseFireState) {
                goapNode.descriptionLog.AddToFillers(stateBeingWatched, "Douse Fire", LOG_IDENTIFIER.STRING_1);
            }
        }
    }
    public void PerTickWatchSuccess(ActualGoapNode goapNode) {
        Character _targetCharacter = goapNode.poiTarget as Character;
        if (_targetCharacter.isDead) {
            //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
            if (goapNode.actor.currentParty.icon.isTravelling) {
                //Stop moving
                goapNode.actor.marker.StopMovement();
            }
            goapNode.EndPerTickEffect();
            return;
        }
        ActualGoapNode actionBeingWatched = goapNode.otherData[0] as ActualGoapNode;
        CharacterState stateBeingWatched = goapNode.otherData[0] as CharacterState;
        if (actionBeingWatched != null) {
            if (actionBeingWatched.actionStatus == ACTION_STATUS.SUCCESS || actionBeingWatched.actionStatus == ACTION_STATUS.FAIL || actionBeingWatched.actor.currentActionNode != actionBeingWatched) {
                //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
                if (goapNode.actor.currentParty.icon.isTravelling) {
                    //Stop moving
                    goapNode.actor.marker.StopMovement();
                }
                goapNode.EndPerTickEffect();
                return;
            }
        } else if (stateBeingWatched != null) {
            if (stateBeingWatched.isDone || (stateBeingWatched.stateComponent.currentState != stateBeingWatched && !stateBeingWatched.isPaused)) { //only end watch state if the state is done or if the watched state is no longer active and not paused
                //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
                if (goapNode.actor.currentParty.icon.isTravelling) {
                    //Stop moving
                    goapNode.actor.marker.StopMovement();
                }
                goapNode.EndPerTickEffect();
                return;
            }
        }

        if (!goapNode.actor.marker.inVisionPOIs.Contains(goapNode.poiTarget)) {
            //if no longer in vision, stop watching
            goapNode.EndPerTickEffect();
        }

        //Always face target when not travelling
        if (!goapNode.actor.currentParty.icon.isTravelling) {
            goapNode.actor.FaceTarget(goapNode.poiTarget);
        }
    }
    //public void AfterWatchSuccess(ActualGoapNode goapNode) {
    //    if (actionBeingWatched != null) {
    //        AddActionDebugLog(GameManager.Instance.TodayLogString() + actor.name + " has finished watching " + actionBeingWatched.goapName + " by " + actionBeingWatched.actor.name + ". Total ticks in watch is " + ticksInWatch.ToString() + "/" + currentState.duration.ToString());
    //    } else if (stateBeingWatched != null) {
    //        AddActionDebugLog(GameManager.Instance.TodayLogString() + actor.name + " has finished watching " + stateBeingWatched.ToString() + ". Total ticks in watch is " + ticksInWatch.ToString() + "/" + currentState.duration.ToString());
    //    }
    //    //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
    //}
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            Character target = poiTarget as Character;
            return actor != target && !target.traitContainer.HasTrait("Beast"); // target.role.roleType != CHARACTER_ROLE.BEAST;
        }
        return false;
    }
    #endregion
}

public class WatchData : GoapActionData {
    public WatchData() : base(INTERACTION_TYPE.WATCH) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.ELEMENTAL, RACE.KOBOLD };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character target = poiTarget as Character;
        return actor != target && !UtilityScripts.GameUtilities.IsRaceBeast(target.race); // target.role.roleType != CHARACTER_ROLE.BEAST
    }
}