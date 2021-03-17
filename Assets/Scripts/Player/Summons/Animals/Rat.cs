using System;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Rat : Animal {
    public override string raceClassName => "Plagued Rat";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Defend;
    public Rat() : base(SUMMON_TYPE.Rat, "Rat", RACE.RAT) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public Rat(string className) : base(SUMMON_TYPE.Rat, className, RACE.RAT) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }
    public Rat(SaveDataSummon data) : base(data) {
        //combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        traitContainer.AddTrait(this, "Plague Reservoir");
        //combatComponent.SetCombatMode(COMBAT_MODE.Defend);
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Rat_Behaviour);
    }

    //This no longer applies, since we changed the summoning of monsters from bred behaviour to per structure, all monsters summoned by player should be aggressive
    //public override void OnSummonAsPlayerMonster() {
    //    base.OnSummonAsPlayerMonster();
    //    combatComponent.SetCombatMode(COMBAT_MODE.Defend);
    //}
    #endregion
}