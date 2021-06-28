using Inner_Maps;
using Interrupts;
using Traits;
using UnityEngine;

public abstract class Ent : Summon {

    public override string raceClassName => "Ent";
    public override System.Type serializedData => typeof(SaveDataEnt);

    /// <summary>
    /// Is this ent pretending to be a tree
    /// </summary>
    public bool isTree { get; private set; }
    
    private System.Action<Ent> _awakenEntEvent;
    
    protected Ent(SUMMON_TYPE summonType, string className) : base(summonType, className, RACE.ENT, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    protected Ent(SaveDataEnt data) : base(data) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        isTree = data.isTree;
    }
    public override void Initialize() {
        base.Initialize();
        SetDestroyMarkerOnDeath(true);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Ent_Behaviour);
    }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false,
        object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false, float piercingPower = 0f, bool isPlayerSource = false) {
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource);
        if (amount < 0 && !isDead) {
            if (!faction.isPlayerFaction) {
                if (elementalDamageType == ELEMENTAL_TYPE.Fire) {
                    combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
                } else {
                    combatComponent.SetCombatMode(COMBAT_MODE.Defend);
                }
                JobQueueItem job = jobQueue.GetJob(JOB_TYPE.STAND_STILL);
                if (job != null) {
                    job.ForceCancelJob();
                }   
            }
            if (isTree) {
                ExecuteAwakenEntEvent();
            }
        }
    }
    protected override void AfterDeath(LocationGridTile deathTileLocation) {
        base.AfterDeath(deathTileLocation);
        LocationGridTile placeForWoodPile = deathTileLocation;
        if (deathTileLocation.tileObjectComponent.objHere != null) {
            placeForWoodPile = deathTileLocation.GetFirstNearestTileFromThisWithNoObject();
        }
        int wood = 300;
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
    public override void OnSeizePOI() {
        if (isTree) {
            ExecuteAwakenEntEvent();
        }
        base.OnSeizePOI();
    }

    #region General
    public void EntAgitatedHandling() {
        if (isTree) {
            ExecuteAwakenEntEvent();    
        }
    }
    public void SetIsTree(bool state) {
        isTree = state;
    }
    public void SubscribeToAwakenEntEvent(TreeObject p_tree) {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{p_tree.nameWithID} subscribed to {name} ({id.ToString()})({persistentID}) Awaken Event");
#endif
        _awakenEntEvent += p_tree.TryAwakenEnt;
    }
    public void UnsubscribeToAwakenEntEvent(TreeObject p_tree) {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{p_tree.nameWithID} unsubscribed from {name} ({id.ToString()})({persistentID}) Awaken Event");
#endif
        _awakenEntEvent -= p_tree.TryAwakenEnt;
    }
    private void ExecuteAwakenEntEvent() {
        _awakenEntEvent?.Invoke(this);
    }
    #endregion
}

[System.Serializable]
public class SaveDataEnt : SaveDataSummon {
    public bool isTree;

    public override void Save(Character data) {
        base.Save(data);
        if (data is Ent summon) {
            isTree = summon.isTree;
        }
    }
}