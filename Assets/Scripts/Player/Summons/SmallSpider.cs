using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class SmallSpider : Summon {

    public const string ClassName = "Small Spider";
    public override string raceClassName => $"Small Spider";
    public override SUMMON_TYPE adultSummonType => SUMMON_TYPE.Giant_Spider;
    public override System.Type serializedData => typeof(SaveDataSmallSpider);

    public GameDate growUpDate { get; private set; }
    public bool shouldGrowUpOnUnSeize { get; private set; }

    public SmallSpider() : base(SUMMON_TYPE.Small_Spider, ClassName, RACE.SPIDER, UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public SmallSpider(string className) : base(SUMMON_TYPE.Small_Spider, className, RACE.SPIDER, UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public SmallSpider(SaveDataSmallSpider data) : base(data) {
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
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
        if (traitContainer.HasTrait("Baby Infestor") == false) {
            //only grow up if spider is not a baby infestor
            //because growing up is handled by Baby Infestor trait
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
        if (isBeingSeized && PlayerManager.Instance.player.seizeComponent.isPreparingToBeUnseized) {
            //if spider is currently seized and is not being unseized when it should grow up,
            //set it to grow up when it is unseized.
            shouldGrowUpOnUnSeize = true;
            return;
        }
        if (IsPOICurrentlyTargetedByAPerformingAction()) {
            //If target is currently targeted by an action, do not grow up, instead, comeback after 1 hour
            SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(12), GrowUp, this);
            return;
        }
        SetDestroyMarkerOnDeath(true);
        LocationGridTile tile = gridTileLocation;
        Faction targetFaction = faction;

        LocationStructure home = homeStructure;
        NPCSettlement settlement = homeSettlement;
        Region region = homeRegion;
        HexTile ogTerritory = territory;
        
        SetShowNotificationOnDeath(false);
        Death("Transform Giant Spider");
        
        //create giant spider
        Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Giant_Spider, targetFaction, settlement, region, home);
        if (!this.isUsingDefaultName) {
            summon.SetFirstAndLastName(firstName, surName);    
        }
        if (ogTerritory != null) {
            summon.SetTerritory(ogTerritory);
        }

        Log growUpLog = GameManager.CreateNewLog(GameManager.Instance.Today(), "Character", "Generic", "become_giant_spider", null, LOG_TAG.Life_Changes);
        growUpLog.AddToFillers(summon, summon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        growUpLog.AddLogToDatabase();
        
        CharacterManager.Instance.PlaceSummon(summon, tile);
        if (UIManager.Instance.characterInfoUI.isShowing && 
            UIManager.Instance.characterInfoUI.activeCharacter == this) {
            UIManager.Instance.characterInfoUI.CloseMenu();    
        }
    }
}

[System.Serializable]
public class SaveDataSmallSpider : SaveDataSummon {
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