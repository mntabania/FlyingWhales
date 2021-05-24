using System;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Rabbit : Animal {
    public override string raceClassName => "Rabbit";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public Rabbit() : base(SUMMON_TYPE.Rabbit, "Rabbit", RACE.RABBIT) {
    }
    public Rabbit(string className) : base(SUMMON_TYPE.Rabbit, className, RACE.RABBIT) {
    }
    public Rabbit(SaveDataSummon data) : base(data) {
    }
}
