using Inner_Maps;

public class CultistTransform : GoapAction {
    public CultistTransform() : base(INTERACTION_TYPE.CULTIST_TRANSFORM) {
        actionIconString = GoapActionStateDB.No_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Player};
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Transform Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion
    
#region State Effects
    public void AfterTransformSuccess(ActualGoapNode goapNode) {
        Character character = goapNode.actor;
        LocationGridTile gridTileLocation = character.gridTileLocation;
        character.SetDestroyMarkerOnDeath(true);
        character.Death(_deathLog: goapNode.descriptionLog);
        Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Abomination, FactionManager.Instance.neutralFaction);
        summon.SetFirstAndLastName(character.firstName, character.surName);
        goapNode.descriptionLog.AddInvolvedObjectManual(summon.persistentID);
        
        CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
        if (UIManager.Instance.characterInfoUI.isShowing && 
            UIManager.Instance.characterInfoUI.activeCharacter == character) {
            UIManager.Instance.characterInfoUI.CloseMenu();    
        }
    }
#endregion
}