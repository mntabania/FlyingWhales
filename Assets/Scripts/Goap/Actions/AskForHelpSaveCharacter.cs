using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class AskForHelpSaveCharacter : GoapAction {

    private Character troubledCharacter;

    public AskForHelpSaveCharacter() : base(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

   // #region Overrides
   // protected override void ConstructRequirement() {
   //     _requirementAction = Requirement;
   // }
   // //protected override void ConstructPreconditionsAndEffects() {
   // //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeNpcSettlement, targetPOI = poiTarget });
   // //}
   // public override void Perform(ActualGoapNode goapNode) {
   //     base.Perform(goapNode);
   //     if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
   //         SetState("Ask Success");
   //     } else {
   //         SetState("Target Missing");
   //     }
   // }
   // protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
   //     return 3;
   // }
   // protected override void AddFillersToLog(Log log) {
   //     base.AddFillersToLog(log);
   //     if (troubledCharacter != null) {
   //         log.AddToFillers(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
   //     }
   // }
   // #endregion

   // #region Requirements
   //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
   //     return actor != poiTarget;
   // }
   // #endregion

   // #region State Effects
   // public void PreAskSuccess() {
   //     goapNode.descriptionLog.AddToFillers(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
   // }
   // public void AfterAskSuccess() {
   //     Character target = poiTarget as Character;
   //     //Add Save Character Job to target
   //     target.CreateSaveCharacterJob(troubledCharacter);
   // }
   // public void PreTargetMissing() {
   //     goapNode.descriptionLog.AddToFillers(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
   // }
   // #endregion
}

public class AskForHelpSaveCharacterData : GoapActionData {
    public AskForHelpSaveCharacterData() : base(INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}