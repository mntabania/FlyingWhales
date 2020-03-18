using Inner_Maps;
using Traits;

public class GiantSpider : Summon {

    public const string ClassName = "GiantSpider";
    
    public override string raceClassName => $"Giant Spider";
    
    public GiantSpider() : base(SUMMON_TYPE.GiantSpider, ClassName, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public GiantSpider(SaveDataCharacter data) : base(data) { }
}