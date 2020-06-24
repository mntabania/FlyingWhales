using Inner_Maps;

public class CreateCultistKit : GoapAction {
    public CreateCultistKit() : base(INTERACTION_TYPE.CREATE_CULTIST_KIT) {
        actionIconString = GoapActionStateDB.Build_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
        AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Create Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 0;
    }
    #endregion

    #region Preconditions
    private bool HasWood(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile pile && pile.resourceInPile >= TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).constructionCost; 
    }
    private bool HasStone(Character actor, IPointOfInterest poiTarget, object[] otherData, JOB_TYPE jobType) {
        return actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile pile && pile.resourceInPile >= TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).constructionCost; 
    }
    #endregion
    
    #region State Effects
    public void AfterCreateSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        WoodPile woodPile = actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) as WoodPile;
        StonePile stonePile = actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) as StonePile; 
        if(woodPile != null && stonePile != null) {
            actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT));
            woodPile.AdjustResourceInPile(-10);
            stonePile.AdjustResourceInPile(-10);
        } else {
            actor.logComponent.PrintLogErrorIfActive(actor.name + " is trying to create a Healing Potion but lacks requirements");
        }
    }
    #endregion
}