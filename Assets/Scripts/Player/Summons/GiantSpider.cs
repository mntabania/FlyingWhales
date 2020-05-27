using Inner_Maps;
using Traits;

public class GiantSpider : Summon {

    public const string ClassName = "Giant Spider";
    
    public override string raceClassName => $"Giant Spider";
    
    public GiantSpider() : base(SUMMON_TYPE.Giant_Spider, ClassName, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public GiantSpider(string className) : base(SUMMON_TYPE.Giant_Spider, className, RACE.SPIDER,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public GiantSpider(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        //behaviourComponent.RemoveBehaviourComponent(typeof(DefaultMonster));
        // behaviourComponent.AddBehaviourComponent(typeof(AbductorMonster));
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Giant_Spider_Behaviour);
    }
    #endregion
}