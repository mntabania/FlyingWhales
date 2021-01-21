using Inner_Maps;
using Traits;

public class Scorpion : Summon {

    public const string ClassName = "Scorpion";

    public override string raceClassName => "Scorpion";

    public Character heldCharacter { set; get; }

    public Scorpion() : base(SUMMON_TYPE.Scorpion, ClassName, RACE.SCORPION, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
        // combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public Scorpion(string className) : base(SUMMON_TYPE.Scorpion, className, RACE.SCORPION, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
        // combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public Scorpion(SaveDataSummon data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
        // combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Scorpion_Behaviour);
        Messenger.AddListener(Signals.HOUR_STARTED, OnHourTicked);
    }

    void OnHourTicked() {
        if(GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 6 || 
            GameManager.Instance.GetHoursBasedOnTicks(GameManager.Instance.Today().tick) == 18){
            jobQueue.CancelAllJobs();
        }
    }
}

