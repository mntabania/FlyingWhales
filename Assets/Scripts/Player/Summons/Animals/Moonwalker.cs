using System;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Moonwalker : ShearableAnimal {
    public override string raceClassName => "Moonwalker";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public Moonwalker() : base(SUMMON_TYPE.Moonwalker, "Moonwalker", RACE.MOONWALKER) {
    }
    public Moonwalker(string className) : base(SUMMON_TYPE.Moonwalker, className, RACE.MOONWALKER) {
    }
    public Moonwalker(SaveDataShearableAnimal data) : base(data) {
    }

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.MOONCRAWLER_CLOTH;
}
