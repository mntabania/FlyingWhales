using System.Collections;
using System.Collections.Generic;
using Logs;
using Object_Pools;
using UnityEngine;
using Traits;

public class AssumptionComponent : CharacterComponent {
    public List<AssumptionData> assumptionData { get; private set; }

    public AssumptionComponent() {
        assumptionData = new List<AssumptionData>();
    }
    public AssumptionComponent(SaveDataAssumptionComponent data) {
        assumptionData = data.assumptionData;
    }

    #region General
    public void CreateAndReactToNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType, REACTION_STATUS reactionStatus) {
        if(HasAlreadyAssumedTo(assumedActionType, assumedCharacter, targetOfAssumedCharacter)) {
            //If the witness already assumed with the set of characters and action, do not assume again
            //Example: If A already assumed that B murdered C, A will never assume that B murdered C again
            return;
        }
        if (!owner.limiterComponent.canWitness) {
            //Cannot assume cannot witness characters
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

        Log assumptionLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "assumed_event", newAssumption.assumedAction, LOG_TAG.Social);
        if (reactionStatus == REACTION_STATUS.INFORMED) {
            assumptionLog.AddTag(LOG_TAG.Informed);
        } else if (reactionStatus == REACTION_STATUS.WITNESSED) {
            assumptionLog.AddTag(LOG_TAG.Witnessed);
        }
        if (newAssumption.assumedAction.crimeType != CRIME_TYPE.None) {
            assumptionLog.AddTag(LOG_TAG.Crimes);
        }
        assumptionLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.PARTY_1); //Used Party 1 identifier so there will be no conflict if reactable.informationLog is a Rumor
        assumptionLog.AddToFillers(null, newAssumption.informationLog.unReplacedText, LOG_IDENTIFIER.APPEND);
        assumptionLog.AddToFillers(newAssumption.informationLog.fillers);
        assumptionLog.AddLogToDatabase();
        newAssumption.SetAssumptionLog(assumptionLog);
        
        PlayerManager.Instance.player.ShowNotificationFrom(owner, InteractionManager.Instance.CreateNewIntel(newAssumption.assumedAction));

        owner.reactionComponent.ReactTo(newAssumption, reactionStatus, false);

        if(targetOfAssumedCharacter is TileObject targetTileObject) {
            targetTileObject.AddCharacterThatAlreadyAssumed(owner);
        }

        Messenger.Broadcast(JobSignals.CHARACTER_ASSUMED, owner, assumedCharacter, targetOfAssumedCharacter);
    }
    private Assumption CreateNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType) {
        ActualGoapNode assumedAction = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[assumedActionType], assumedCharacter, targetOfAssumedCharacter, null, 0);
        Assumption assumption = new Assumption(owner, assumedCharacter);
        assumedAction.SetAsAssumption(assumption);
        return assumption;
    }
    public ActualGoapNode CreateNewActionToReactTo(Character actor, IPointOfInterest target, INTERACTION_TYPE actionType) {
        ActualGoapNode assumedAction = ObjectPoolManager.Instance.CreateNewAction(InteractionManager.Instance.goapActionData[actionType], actor, target, null, 0);
        assumedAction.SetAsIllusion();
        assumedAction.SetCrimeType();
        return assumedAction;
    }
    private bool HasAlreadyAssumedTo(INTERACTION_TYPE actionType, Character actor, IPointOfInterest target) {
        for (int i = 0; i < assumptionData.Count; i++) {
            AssumptionData data = assumptionData[i];
            if (data.assumedActionType == actionType && data.actorID == actor.persistentID && data.targetID == target.persistentID && data.targetPOIType == target.poiType) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataAssumptionComponent data) {
        //Currently N/A
    }
    #endregion
}

[System.Serializable]
public struct AssumptionData {
    public INTERACTION_TYPE assumedActionType;
    public string actorID;
    public string targetID;
    public POINT_OF_INTEREST_TYPE targetPOIType;

    public AssumptionData(INTERACTION_TYPE actionType, Character actor, IPointOfInterest target) {
        assumedActionType = actionType;
        actorID = actor.persistentID;
        targetID = target.persistentID;
        targetPOIType = target.poiType;
    }
}

[System.Serializable]
public class SaveDataAssumptionComponent : SaveData<AssumptionComponent> {
    public List<AssumptionData> assumptionData;

    #region Overrides
    public override void Save(AssumptionComponent data) {
        assumptionData = new List<AssumptionData>(data.assumptionData);
    }

    public override AssumptionComponent Load() {
        AssumptionComponent component = new AssumptionComponent(this);
        return component;
    }
    #endregion
}