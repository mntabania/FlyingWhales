using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssumptionComponent {
    public Character owner { get; private set; }

    public AssumptionComponent(Character owner) {
        this.owner = owner;
    }

    #region General
    public void CreateAndReactToNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType, REACTION_STATUS reactionStatus) {
        Assumption newAssumption = CreateNewAssumption(assumedCharacter, targetOfAssumedCharacter, assumedActionType);

        Log assumptionLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "assumed_event");
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
    public Assumption CreateNewAssumption(Character assumedCharacter, IPointOfInterest targetOfAssumedCharacter, INTERACTION_TYPE assumedActionType) {
        ActualGoapNode assumedAction = new ActualGoapNode(InteractionManager.Instance.goapActionData[assumedActionType], assumedCharacter, targetOfAssumedCharacter, null, 0);
        return new Assumption(owner, assumedAction);
    }
    #endregion
}
