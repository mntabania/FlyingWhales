using Inner_Maps;
using Traits;

public class GiantSpider : Summon {

    public const string ClassName = "GiantSpider";
    
    public override string raceClassName => $"Giant Spider";
    
    public GiantSpider() : base(SUMMON_TYPE.GiantSpider, CharacterRole.SOLDIER, ClassName, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) {
        combatComponent.SetElementalDamage(ELEMENTAL_TYPE.Poison);
    }
    public GiantSpider(SaveDataCharacter data) : base(data) {
        combatComponent.SetElementalDamage(ELEMENTAL_TYPE.Poison);
    }
}