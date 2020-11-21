using UtilityScripts;

public class Torture : GoapAction {
    public Torture() : base(INTERACTION_TYPE.TORTURE) {
        actionIconString = GoapActionStateDB.Anger_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Torture Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion
    
    #region State Effects
    public void PerTickTortureSuccess(ActualGoapNode goapNode) {
        goapNode.poiTarget.AdjustHP(-35, ELEMENTAL_TYPE.Normal, source: goapNode.actor, showHPBar: true);
    }
    public void AfterTortureSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        IPointOfInterest targetPOI = goapNode.poiTarget;
        if(targetPOI is Character targetCharacter) {
            if(targetCharacter.currentHP <= 0) {
                targetCharacter.Death(deathFromAction: goapNode, responsibleCharacter: goapNode.actor);
            } else {
                string logKey = string.Empty;
                if (GameUtilities.RollChance(65)) {
                    logKey = "nothing";
                } else if (GameUtilities.RollChance(25)) {
                    logKey = "enslave";
                    targetCharacter.traitContainer.AddTrait(targetCharacter, "Enslaved", characterResponsible: actor);
                } else if (GameUtilities.RollChance(20)) {
                    logKey = "injure";
                    targetCharacter.traitContainer.AddTrait(targetCharacter, "Injured", characterResponsible: actor);
                }
                if (logKey != string.Empty) {
                    Log log = GameManager.CreateNewLog(GameManager.Instance.Today(), "GoapAction", goapName, logKey, goapNode, LOG_TAG.Life_Changes);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    log.AddLogToDatabase();
                }
            }
        }
    }
    #endregion
}