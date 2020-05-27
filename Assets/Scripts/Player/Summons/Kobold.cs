using Inner_Maps;
using Traits;

public class Kobold : Summon {

    public const string ClassName = "Kobold";
    
    public override string raceClassName => "Kobold";
    
    public Kobold() : base(SUMMON_TYPE.Kobold, ClassName, RACE.KOBOLD,
        UtilityScripts.Utilities.GetRandomGender()) {
		//combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
    }
    public Kobold(string className) : base(SUMMON_TYPE.Kobold, className, RACE.KOBOLD,
        UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
    }
    public Kobold(SaveDataCharacter data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
    }
    public override void Initialize() {
        base.Initialize();
        traitContainer.AddTrait(this, "Cold Blooded");
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Kobold_Behaviour);
    }
}

