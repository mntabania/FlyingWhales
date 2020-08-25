using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public class AssumptionComponent {
    public Character owner { get; private set; }

    public List<AssumptionData> assumptionData { get; private set; }

    public AssumptionComponent(Character owner) {
        this.owner = owner;
        assumptionData = new List<AssumptionData>();
    }

    #region General
    public void CreateAndReactToNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType, REACTION_STATUS reactionStatus) {
        if(HasAlreadyAssumedTo(assumedActionType, assumedCharacter, targetOfAssumedCharacter)) {
            return;
        }
        assumptionData.Add(new AssumptionData(assumedActionType, assumedCharacter, targetOfAssumedCharacter));
        Assumption newAssumption = CreateNewAssumption(assumedCharacter, targetOfAssumedCharacter, assumedActionType);
        newAssumption.assumedAction.SetCrimeType();

        if(assumedActionType == INTERACTION_TYPE.ASSAULT) {
            //When assuming assault, always assume that the reason for assault is to abduct
            if (LocalizationManager.Instance.HasLocalizedValue("Character", "Combat", "Abduct")) {
                string reason = LocalizationManager.Instance.GetLocalizedValue("Character", "Combat", "Abduct");
                newAssumption.assumedAction.descriptionLog.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);
            }
        }

        Log assumptionLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "assumed_event", newAssumption.assumedAction);
        assumptionLog.SetLogType(LOG_TYPE.Assumption);
        assumptionLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
        assumptionLog.AddToFillers(null, UtilityScripts.Utilities.LogDontReplace(newAssumption.informationLog), LOG_IDENTIFIER.APPEND);
        assumptionLog.AddToFillers(newAssumption.informationLog.fillers);
        assumptionLog.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotificationFrom(owner, assumptionLog);
        //owner.logComponent.AddHistory(assumptionLog);

        owner.reactionComponent.ReactTo(newAssumption, reactionStatus, false);

        if(targetOfAssumedCharacter is TileObject targetTileObject) {
            targetTileObject.AddCharacterThatAlreadyAssumed(owner);
        }

        Messenger.Broadcast(Signals.CHARACTER_ASSUMED, owner, assumedCharacter, targetOfAssumedCharacter);
    }
    private Assumption CreateNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType) {
        ActualGoapNode assumedAction = new ActualGoapNode(InteractionManager.Instance.goapActionData[assumedActionType], assumedCharacter, targetOfAssumedCharacter, null, 0);
        return new Assumption(owner, assumedAction);
    }
    public ActualGoapNode CreateNewActionToReactTo(Character actor, IPointOfInterest target, INTERACTION_TYPE actionType) {
        ActualGoapNode assumedAction = new ActualGoapNode(InteractionManager.Instance.goapActionData[actionType], actor, target, null, 0);
        assumedAction.SetAsIllusion();
        assumedAction.SetCrimeType();
        return assumedAction;
    }
    private bool HasAlreadyAssumedTo(INTERACTION_TYPE actionType, Character actor, IPointOfInterest target, CRIME_TYPE crimeType) {
        if (actor.traitContainer.HasTrait("Criminal")) {
            Criminal criminalTrait = actor.traitContainer.GetNormalTrait<Criminal>("Criminal");
            if (criminalTrait.IsCrimeAlreadyWitnessedBy(actor, crimeType)) {
                return true;
            }
        }
        return false;
    }
    private bool HasAlreadyAssumedTo(INTERACTION_TYPE actionType, Character actor, IPointOfInterest target) {
        for (int i = 0; i < assumptionData.Count; i++) {
            AssumptionData data = assumptionData[i];
            if (data.assumedActionType == actionType && data.actorID == actor.id && data.targetID == target.id && data.targetPOIType == target.poiType) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

public struct AssumptionData {
    public INTERACTION_TYPE assumedActionType;
    public int actorID;
    public int targetID;
    public POINT_OF_INTEREST_TYPE targetPOIType;

    public AssumptionData(INTERACTION_TYPE actionType, Character actor, IPointOfInterest target) {
        assumedActionType = actionType;
        actorID = actor.id;
        targetID = target.id;
        targetPOIType = target.poiType;
    }
}