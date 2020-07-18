using Inner_Maps;
using Traits;

public class Mimic : Summon {

    public override string raceClassName => "Mimic";
    
    public bool isTreasureChest { get; private set; }
    
    public Mimic() : base(SUMMON_TYPE.Mimic, "Mimic", RACE.MIMIC,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public Mimic(string className) : base(SUMMON_TYPE.Mimic, className, RACE.MIMIC,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public Mimic(SaveDataCharacter data) : base(data) { }
    
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

    #region General
    public void SetIsTreasureChest(bool state) {
        isTreasureChest = state;
    }
    #endregion
}