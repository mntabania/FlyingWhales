using Inner_Maps;
using Traits;
using UnityEngine;
using System.Linq;

public class Mimic : Summon {

    public override string raceClassName => "Mimic";
    public override System.Type serializedData => typeof(SaveDataMimic);

    public bool isTreasureChest { get; private set; }
    private System.Action<Mimic> _awakenMimicEvent;
    
    public Mimic() : base(SUMMON_TYPE.Mimic, "Mimic", RACE.MIMIC, UtilityScripts.Utilities.GetRandomGender()) { }
    public Mimic(string className) : base(SUMMON_TYPE.Mimic, className, RACE.MIMIC, UtilityScripts.Utilities.GetRandomGender()) { }
    public Mimic(SaveDataMimic data) : base(data) {
        isTreasureChest = data.isTreasureChest;
    }
    
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Mimic_Behaviour);
    }
    protected override void OnTickEnded() {
        if (isTreasureChest) {
            return;
        }
        base.OnTickEnded();
    }
    protected override void OnTickStarted() {
        if (isTreasureChest) {
            return;
        }
        base.OnTickStarted();
    }
    public override void OnSeizePOI() {
        if (isTreasureChest) {
            ExecuteAwakenMimicEvent();
        }
        base.OnSeizePOI();
    }
    // public override bool CanBeSeenBy(Character p_character) {
    //     bool canBeSeen = base.CanBeSeenBy(p_character);
    //     if (!canBeSeen) {
    //         if (isTreasureChest && p_character.hasMarker) {
    //             //if mimic is a treasure chest, check if character can see the treasure chest that it is disguised as.
    //             return p_character.marker.inVisionTileObjects.Any(t => t is TreasureChest treasureChest && treasureChest.objectInside == this);
    //         }
    //     }
    //     return canBeSeen;
    // }
    public override void AdjustHP(int amount, ELEMENTAL_TYPE elementalDamageType, bool triggerDeath = false, object source = null, CombatManager.ElementalTraitProcessor elementalTraitProcessor = null, bool showHPBar = false,
        float piercingPower = 0, bool isPlayerSource = false) {
        base.AdjustHP(amount, elementalDamageType, triggerDeath, source, elementalTraitProcessor, showHPBar, piercingPower, isPlayerSource);
        if (amount < 0 && !isDead) {
            if (isTreasureChest) {
                ExecuteAwakenMimicEvent();
            }
        }
    }

    #region General
    public void MimicAgitatedHandling() {
        if (isTreasureChest) {
            ExecuteAwakenMimicEvent();
        }
    }
    public void SetIsTreasureChest(bool state) {
        isTreasureChest = state;
        //if mimic is a treasure chest, set its combat mode to defend so that characters that see it as a treasure chest will not attack it immediately.
        if (isTreasureChest) {
            combatComponent.SetCombatMode(COMBAT_MODE.Passive);
        } else {
            combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        }
    }
    public void SubscribeToAwakenMimicEvent(TreasureChest p_chest) {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{p_chest.nameWithID} subscribed to {name} ({id.ToString()})({persistentID}) Awaken Event");
#endif
        _awakenMimicEvent += p_chest.TryAwakenMimic;
    }
    public void UnsubscribeToAwakenMimicEvent(TreasureChest p_chest) {
#if DEBUG_LOG
        Debug.Log($"{GameManager.Instance.TodayLogString()}{p_chest.nameWithID} unsubscribed from {name} ({id.ToString()})({persistentID}) Awaken Event");
#endif
        _awakenMimicEvent -= p_chest.TryAwakenMimic;
    }
    private void ExecuteAwakenMimicEvent() {
        _awakenMimicEvent?.Invoke(this);
    }
#endregion
}

[System.Serializable]
public class SaveDataMimic : SaveDataSummon {
    public bool isTreasureChest;

    public override void Save(Character data) {
        base.Save(data);
        if (data is Mimic summon) {
            isTreasureChest = summon.isTreasureChest;
        }
    }
}