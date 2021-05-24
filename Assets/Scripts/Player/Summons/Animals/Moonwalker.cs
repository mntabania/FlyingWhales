using System;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Moonwalker : Animal {
    public override string raceClassName => "Moonwalker";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public Moonwalker() : base(SUMMON_TYPE.Moonwalker, "Moonwalker", RACE.MOONWALKER) {
    }
    public Moonwalker(string className) : base(SUMMON_TYPE.Moonwalker, className, RACE.MOONWALKER) {
    }
    public Moonwalker(SaveDataSummon data) : base(data) {
    }
}
