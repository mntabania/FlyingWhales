using Inner_Maps;
using Traits;

public class GiantSpider : Summon {

    public const string ClassName = "GiantSpider";
    
    public override string raceClassName => $"Giant Spider";
    
    public GiantSpider() : base(SUMMON_TYPE.GiantSpider, ClassName, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public GiantSpider(string className) : base(SUMMON_TYPE.GiantSpider, className, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public GiantSpider(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.RemoveBehaviourComponent(typeof(DefaultMonster));
        behaviourComponent.AddBehaviourComponent(typeof(AbductorMonster));
    }
    #endregion
}