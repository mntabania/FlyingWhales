﻿using Inner_Maps;
using Traits;

public class Kobold : Summon {

    public const string ClassName = "Kobold";
    
    public override string raceClassName => "Kobold";
    
    public Kobold() : base(SUMMON_TYPE.Kobold, ClassName, RACE.KOBOLD, UtilityScripts.Utilities.GetRandomGender()) {
		//combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
        // combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public Kobold(string className) : base(SUMMON_TYPE.Kobold, className, RACE.KOBOLD, UtilityScripts.Utilities.GetRandomGender()) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
        // combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public Kobold(SaveDataSummon data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
        // combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Kobold_Behaviour);
    }
}

