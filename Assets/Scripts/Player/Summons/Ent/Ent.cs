using Inner_Maps;
using Interrupts;
using Traits;

public abstract class Ent : Summon {
    
    public override string raceClassName => "Ent";
    
    protected Ent(SUMMON_TYPE summonType, string className) : base(summonType, className, RACE.ENT,
        UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    protected Ent(SaveDataCharacter data) : base(data) {
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.RemoveBehaviourComponent(typeof(DefaultMonster));
        behaviourComponent.RemoveBehaviourComponent(typeof(MovementProcessing));
        behaviourComponent.AddBehaviourComponent(typeof(EntBehaviour));
        behaviourComponent.AddBehaviourComponent(typeof(MovementProcessing));
        //traitContainer.AddTrait(this, "Fire Prone");
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null) {
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor);
        if(amount < 0 && !isDead && !faction.isPlayerFaction) {
            if(elementalDamageType == ELEMENTAL_TYPE.Fire) {
                combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
            } else {
                combatComponent.SetCombatMode(COMBAT_MODE.Defend);
            }
            JobQueueItem job = jobQueue.GetJob(JOB_TYPE.STAND_STILL);
            if(job != null) {
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
        placeForWoodPile.SetReservedType(TILE_OBJECT_TYPE.WOOD_PILE);
    }
}

