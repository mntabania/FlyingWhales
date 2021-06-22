using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;

public class SmallSpider : SkinnableAnimal {

    public const string ClassName = "Small Spider";
    public override string raceClassName => $"Small Spider";
    public override SUMMON_TYPE adultSummonType => SUMMON_TYPE.Giant_Spider;
    public override System.Type serializedData => typeof(SaveDataSmallSpider);

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.SPIDER_SILK;

    public GameDate growUpDate { get; private set; }
    public bool shouldGrowUpOnUnSeize { get; private set; }

    public SmallSpider() : base(SUMMON_TYPE.Small_Spider, ClassName, RACE.SPIDER, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public SmallSpider(string className) : base(SUMMON_TYPE.Small_Spider, className, RACE.SPIDER, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public SmallSpider(SaveDataSmallSpider data) : base(data) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
        growUpDate = data.growUpDate;
        shouldGrowUpOnUnSeize = data.shouldGrowUpOnUnSeize;
    }

    #region Loading
    public override void LoadReferences(SaveDataCharacter data) {
        base.LoadReferences(data);
        //after all other references have been loaded, schedule small spider grow up
        ScheduleGrowUp();
    }
    #endregion

    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Small_Spider_Behaviour);
    }
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        DetermineGrowUpDate();
        ScheduleGrowUp();    
    }
    private void ScheduleGrowUp() {
        if (!traitContainer.HasTrait("Baby Infestor") && faction != null && faction != null && faction.factionType.type != FACTION_TYPE.Demons) {
            //only grow up if spider is not a baby infestor
            //because growing up is handled by Baby Infestor trait
            //also don't grow up if small spider is part of the player faction because of this issue:
            //https://trello.com/c/ArYdxExc/4739-live-v040-the-baby-spiders-you-capture-still-grow-up-into-giant-spiders-snatcher
#if DEBUG_LOG
            Debug.Log($"{name} scheduled grow up date on {growUpDate.ToString()}");
#endif
            SchedulingManager.Instance.AddEntry(growUpDate, GrowUp, this);
        }
    }
    private void DetermineGrowUpDate() {
        GameDate date = GameManager.Instance.Today();
        date.AddDays(1);
        growUpDate = date;
    }
    public override void OnSeizePOI() {
        base.OnSeizePOI();
        //need to reschedule grow up on seize, since schedules by this character are cleared upon seizing 
        ScheduleGrowUp();
    }
    public override void OnUnseizePOI(LocationGridTile tileLocation) {
        base.OnUnseizePOI(tileLocation);
        if (shouldGrowUpOnUnSeize) {
            //this should only happen when this spider is scheduled to grow up while it is being seized.
            shouldGrowUpOnUnSeize = false;
            GrowUp();
        }
    }
    /// <summary>
    /// Make this spider grow up into a small spider. NOTE: This is only used by normal small spiders.
    /// Small spiders hatched from infestors use the baby infestor trait. <see cref="BabyInfestor"/> <seealso cref="SpiderEgg.Hatch"/>
    /// </summary>
    private void GrowUp() {
        if (isDead) { return; }
        if (faction != null && faction.factionType.type == FACTION_TYPE.Demons) {
            //Reference: https://trello.com/c/ArYdxExc/4739-live-v040-the-baby-spiders-you-capture-still-grow-up-into-giant-spiders-snatcher
            return;
        }
        if (isBeingSeized && PlayerManager.Instance.player.seizeComponent.isPreparingToBeUnseized) {
            //if spider is currently seized and is not being unseized when it should grow up,
            //set it to grow up when it is unseized.
            shouldGrowUpOnUnSeize = true;
            return;
        }
        if (IsPOICurrentlyTargetedByAPerformingAction()) {
            //If target is currently targeted by an action, do not grow up, instead, comeback after 1 hour
            SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.ticksPerHour), GrowUp, this);
            return;
        }
        SetDestroyMarkerOnDeath(true);
        LocationGridTile tile = gridTileLocation;
        Faction targetFaction = faction;

        LocationStructure home = homeStructure;
        NPCSettlement settlement = homeSettlement;
        Region region = homeRegion;
        Area ogTerritory = territory;
        
        SetShowNotificationOnDeath(false);
        
        //create giant spider
        Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Giant_Spider, targetFaction, settlement, region, home, bypassIdeologyChecking: true);
        if (!this.isUsingDefaultName) {
            summon.SetFirstAndLastName(firstName, surName);    
        }
        if (ogTerritory != null) {
            summon.SetTerritory(ogTerritory);
        }

        Log growUpLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "become_giant_spider", null, LOG_TAG.Life_Changes);
        growUpLog.AddToFillers(summon, summon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        growUpLog.AddLogToDatabase(true);
        
        CharacterManager.Instance.PlaceSummonInitially(summon, tile);
        TraitManager.Instance.CopyStatuses(this, summon);

        Death("Transform Giant Spider");
        if (UIManager.Instance.IsContextMenuShowingForTarget(this)) {
            UIManager.Instance.RefreshPlayerActionContextMenuWithNewTarget(summon);
        }
        if (UIManager.Instance.monsterInfoUI.isShowing && 
            UIManager.Instance.monsterInfoUI.activeMonster == this) {
            UIManager.Instance.monsterInfoUI.CloseMenu();    
        }
    }
}

[System.Serializable]
public class SaveDataSmallSpider : SaveDataSkinnableAnimal {
    public GameDate growUpDate;
    public bool shouldGrowUpOnUnSeize;

    public override void Save(Character data) {
        base.Save(data);
        if (data is SmallSpider summon) {
            growUpDate = summon.growUpDate;
            shouldGrowUpOnUnSeize = summon.shouldGrowUpOnUnSeize;
        }
    }
}