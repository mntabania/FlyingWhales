using System;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Mink : Animal {
    public override string raceClassName => "Mink";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public Mink() : base(SUMMON_TYPE.Mink, "Mink", RACE.MINK) {
    }
    public Mink(string className) : base(SUMMON_TYPE.Mink, className, RACE.MINK) {
    }
    public Mink(SaveDataSummon data) : base(data) {
    }

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.MINK_CLOTH;
}
