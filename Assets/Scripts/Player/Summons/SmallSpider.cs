using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class SmallSpider : Summon {

    public const string ClassName = "Small Spider";
    public override string raceClassName => $"Small Spider";
    public override SUMMON_TYPE adultSummonType => SUMMON_TYPE.Giant_Spider;

    private GameDate _growUpDate;
    private bool _shouldGrowUpOnUnSeize;
    
    public SmallSpider() : base(SUMMON_TYPE.Small_Spider, ClassName, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Poison);
        combatComponent.SetCombatMode(COMBAT_MODE.Aggressive);
    }
    public SmallSpider(string className) : base(SUMMON_TYPE.Small_Spider, className, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Poison);
    }
    public SmallSpider(SaveDataCharacter data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Poison);
    }
    
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
        SchedulingManager.Instance.AddEntry(_growUpDate, GrowUp, this);
    }
    private void DetermineGrowUpDate() {
        GameDate date = GameManager.Instance.Today();
        date.AddTicks(5);
        _growUpDate = date;
    }
    public override void OnSeizePOI() {
        base.OnSeizePOI();
        //need to reschedule grow up on seize, since schedules by this character are cleared upon seizing 
        ScheduleGrowUp();
    }
    public override void OnUnseizePOI(LocationGridTile tileLocation) {
        base.OnUnseizePOI(tileLocation);
        if (_shouldGrowUpOnUnSeize) {
            //this should only happen when this spider is scheduled to grow up while it is being seized.
            _shouldGrowUpOnUnSeize = false;
            GrowUp();
        }
    }
    private void GrowUp() {
        if (isDead) { return; }
        if (isBeingSeized && PlayerManager.Instance.player.seizeComponent.isPreparingToBeUnseized) {
            //if spider is currently seized and is not being unseized when it should grow up,
            //set it to grow up when it is unseized.
            _shouldGrowUpOnUnSeize = true;
            return;
        }
        SetDestroyMarkerOnDeath(true);
        LocationGridTile tile = gridTileLocation;
        Faction targetFaction = faction;

        LocationStructure home = homeStructure;
        List<HexTile> ogTerritories = territorries;
        
        SetShowNotificationOnDeath(false);
        Death("Transform Giant Spider");
        
        //create giant spider
        Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Giant_Spider, targetFaction, homeSettlement, homeRegion, homeStructure);
        summon.SetName(name);
        if (ogTerritories.Count > 0) {
            for (int i = 0; i < ogTerritories.Count; i++) {
                summon.AddTerritory(ogTerritories[i]);    
            }
        }
        
        Log growUpLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "become_giant_spider");
        growUpLog.AddToFillers(summon, summon.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        growUpLog.AddLogToInvolvedObjects();
        
        CharacterManager.Instance.PlaceSummon(summon, tile);
        if (UIManager.Instance.characterInfoUI.isShowing && 
            UIManager.Instance.characterInfoUI.activeCharacter == this) {
            UIManager.Instance.characterInfoUI.CloseMenu();    
        }
    }
}