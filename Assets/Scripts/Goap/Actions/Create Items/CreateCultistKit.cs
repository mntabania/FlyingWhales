using System.Collections.Generic;
using Inner_Maps;

public class CreateCultistKit : GoapAction {
    private Precondition _stonePrecondition;
    private Precondition _woodPrecondition;

    public CreateCultistKit() : base(INTERACTION_TYPE.CREATE_CULTIST_KIT) {
        actionIconString = GoapActionStateDB.Build_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work, LOG_TAG.Crimes};

        _stonePrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
        _woodPrecondition = new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
    }
    
    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        // AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Wood Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasWood);
        // AddPrecondition(new GoapEffect(GOAP_EFFECT_CONDITION.TAKE_POI, "Stone Pile", false, GOAP_EFFECT_TARGET.ACTOR), HasStone);
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_POI, "Cultist Kit", false, GOAP_EFFECT_TARGET.ACTOR));
    }
    public override Precondition GetPrecondition(Character actor, IPointOfInterest target, OtherData[] otherData, JOB_TYPE jobType, out bool isOverridden) {
        //List<Precondition> baseP = base.GetPrecondition(actor, target, otherData, out isOverridden);
        //List<Precondition> p = ObjectPoolManager.Instance.CreateNewPreconditionsList();
        Precondition p = null;
        //p.AddRange(baseP);
        if (actor.race == RACE.HUMANS) {
            p = _stonePrecondition;
        } else {
            p = _woodPrecondition;
        }
        isOverridden = true;
        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Create Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 0;
    }
#endregion

#region Preconditions
    private bool HasWood(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.WOOD_PILE); 
    }
    private bool HasStone(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JOB_TYPE jobType) {
        return actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) is ResourcePile pile && 
               pile.resourceInPile >= TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT).mainRecipe.GetNeededAmountForIngredient(TILE_OBJECT_TYPE.STONE_PILE); 
    }
#endregion
    
#region State Effects
    public void AfterCreateSuccess(ActualGoapNode goapNode) {
        Character actor = goapNode.actor;
        if (actor.race == RACE.HUMANS) {
            StonePile stonePile = actor.GetItem(TILE_OBJECT_TYPE.STONE_PILE) as StonePile; 
            if(stonePile != null) {
                actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT));
                stonePile.AdjustResourceInPile(-10);
            } else {
#if DEBUG_LOG
                actor.logComponent.PrintLogErrorIfActive(actor.name + " is trying to create a Cultist Kit but lacks requirements");
#endif
            }    
        }
        else {
            WoodPile woodPile = actor.GetItem(TILE_OBJECT_TYPE.WOOD_PILE) as WoodPile;
            if(woodPile != null) {
                actor.ObtainItem(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.CULTIST_KIT));
                woodPile.AdjustResourceInPile(-10);
            } else {
#if DEBUG_LOG
                actor.logComponent.PrintLogErrorIfActive(actor.name + " is trying to create a Cultist Kit but lacks requirements");
#endif
            }
        }
    }
#endregion
}