using Inner_Maps;
using Traits;

public class SmallSpider : Summon {

    public const string ClassName = "Small Spider";
    
    public override string raceClassName => $"Small Spider";
    
    public SmallSpider() : base(SUMMON_TYPE.Small_Spider, ClassName, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Poison);
    }
    public SmallSpider(SaveDataCharacter data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Poison);
    }
}