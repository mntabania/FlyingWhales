using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class SmallSpider : Summon {

    public const string ClassName = "Small Spider";
    
    public override string raceClassName => $"Small Spider";

    public override SUMMON_TYPE adultSummonType => SUMMON_TYPE.Giant_Spider;

    public SmallSpider() : base(SUMMON_TYPE.Small_Spider, ClassName, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Poison);
        combatComponent.SetCombatMode(COMBAT_MODE.Defend);
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
        GameDate date = GameManager.Instance.Today();
        date.AddDays(2);
        SchedulingManager.Instance.AddEntry(date, GrowUp, this);
    }
    private void GrowUp() {
        if (isDead) { return; }
        SetDestroyMarkerOnDeath(true);
        LocationGridTile tile = gridTileLocation;
        Faction targetFaction = faction;

        LocationStructure home = homeStructure;
        List<HexTile> ogTerritories = territorries;
        
        Death("Transform Giant Spider");
        
        //create giant spider
        Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Giant_Spider, targetFaction, homeSettlement, homeRegion, homeStructure);
        summon.SetName(name);
        if (ogTerritories.Count > 0) {
            for (int i = 0; i < ogTerritories.Count; i++) {
                summon.AddTerritory(ogTerritories[i]);    
            }
        }
        summon.logComponent.AddHistory(deathLog);
        CharacterManager.Instance.PlaceSummon(summon, tile);
        if (UIManager.Instance.characterInfoUI.isShowing && 
            UIManager.Instance.characterInfoUI.activeCharacter == this) {
            UIManager.Instance.characterInfoUI.CloseMenu();    
        }
    }
}