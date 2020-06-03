using Inner_Maps;
using Traits;
using System.Collections.Generic;

public class WarriorAngel : Summon {
    public override string raceClassName => $"Warrior Angel";
    
    public WarriorAngel() : base(SUMMON_TYPE.Warrior_Angel, "Warrior Angel", RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }
    public WarriorAngel(string className) : base(SUMMON_TYPE.Warrior_Angel, className, RACE.ANGEL,
        UtilityScripts.Utilities.GetRandomGender()) {
    }

    #region Overrides
    public override void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<SPELL_TYPE>();
        } else {
            actions.Clear();
        }
        AddPlayerAction(SPELL_TYPE.ZAP);
        //AddPlayerAction(SPELL_TYPE.SEIZE_CHARACTER);
    }
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Angel_Behaviour);
    }
    #endregion  
}