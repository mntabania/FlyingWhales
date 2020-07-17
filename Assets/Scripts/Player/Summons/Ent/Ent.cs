using Inner_Maps;
using Interrupts;
using Traits;

public abstract class Ent : Summon {

    public override string raceClassName => "Ent";
    
    /// <summary>
    /// Is this ent pretending to be a tree
    /// </summary>
    public bool isTree { get; private set; }
    

    protected Ent(SUMMON_TYPE summonType, string className) : base(summonType, className, RACE.ENT,
        UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    protected Ent(SaveDataCharacter data) : base(data) {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public override void Initialize() {
        base.Initialize();
        SetDestroyMarkerOnDeath(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Ent_Behaviour);
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false) {
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar);
        if (amount < 0 && !isDead && !faction.isPlayerFaction) {
            if (elementalDamageType == ELEMENTAL_TYPE.Fire) {
                combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
            } else {
                combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            }
            JobQueueItem job = jobQueue.GetJob(JOB_TYPE.STAND_STILL);
            if (job != null) {
                job.ForceCancelJob(false);
            }
        }
    }
    protected override void AfterDeath(LocationGridTile deathTileLocation) {
        base.AfterDeath(deathTileLocation);
        LocationGridTile placeForWoodPile = deathTileLocation;
        if (deathTileLocation.objHere != null) {
            placeForWoodPile = deathTileLocation.GetNearestUnoccupiedTileFromThis();
        }
        int wood = InnerMapManager.Big_Tree_Yield;
        WoodPile woodPile = InnerMapManager.Instance.CreateNewTileObject<WoodPile>(TILE_OBJECT_TYPE.WOOD_PILE);
        woodPile.SetResourceInPile(wood);
        placeForWoodPile.structure.AddPOI(woodPile, placeForWoodPile);
        // placeForWoodPile.SetReservedType(TILE_OBJECT_TYPE.WOOD_PILE);
    }
    protected override void OnTickEnded() {
        if (isTree) {
            return;
        }
        base.OnTickEnded();
    }
    protected override void OnTickStarted() {
        if (isTree) {
            return;
        }
        base.OnTickStarted();
    }

    #region General
    public void SetIsTree(bool state) {
        isTree = state;
    }
    #endregion
}