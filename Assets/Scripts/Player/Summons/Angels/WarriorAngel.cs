using Inner_Maps;
using Traits;
using System.Collections.Generic;

public class WarriorAngel : Summon {
    public override string raceClassName => $"Warrior Angel";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;

    public WarriorAngel() : base(SUMMON_TYPE.Warrior_Angel, "Warrior Angel", RACE.ANGEL, UtilityScripts.Utilities.GetRandomGender()) { }
    public WarriorAngel(string className) : base(SUMMON_TYPE.Warrior_Angel, className, RACE.ANGEL, UtilityScripts.Utilities.GetRandomGender()) { }
    public WarriorAngel(SaveDataSummon data) : base(data) { }

    #region Overrides
    public override void ConstructDefaultActions() {
        if (actions == null) {
            actions = new List<PLAYER_SKILL_TYPE>();
        } else {
            actions.Clear();
        }
        AddPlayerAction(PLAYER_SKILL_TYPE.ZAP);
        //AddPlayerAction(SPELL_TYPE.SEIZE_CHARACTER);
    }
    public override void Initialize() {
        base.Initialize();
        movementComponent.SetToFlying();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Default_Angel_Behaviour);
    }
    #endregion  
}