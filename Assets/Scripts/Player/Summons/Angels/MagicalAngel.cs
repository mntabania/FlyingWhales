using Inner_Maps;
using Traits;
using System.Collections.Generic;

public class MagicalAngel : Summon {
    public override string raceClassName => $"Magical Angel";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;
    public MagicalAngel() : base(SUMMON_TYPE.Magical_Angel, "Magical Angel", RACE.ANGEL, UtilityScripts.Utilities.GetRandomGender()) { }
    public MagicalAngel(string className) : base(SUMMON_TYPE.Magical_Angel, className, RACE.ANGEL, UtilityScripts.Utilities.GetRandomGender()) { }
    public MagicalAngel(SaveDataSummon data) : base(data) { }

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