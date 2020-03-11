using Inner_Maps;
using Interrupts;
using Traits;

public class Ent : Summon {
    
    public override string raceClassName => "Ent";
    
    public Ent() : base(SUMMON_TYPE.Ent, "Grass Ent", RACE.ENT,
        UtilityScripts.Utilities.GetRandomGender()) {
		//combatComponent.SetElementalType(ELEMENTAL_TYPE.Earth);
        combatComponent.SetCombatMode(COMBAT_MODE.Passive);
    }
    public Ent(SaveDataCharacter data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Earth);
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
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null) {
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source);
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

