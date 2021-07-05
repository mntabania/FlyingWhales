using System;
using Inner_Maps;
using Traits;
using UtilityScripts;

public class Rabbit : ShearableAnimal {
    public override string raceClassName => "Rabbit";
    public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public Rabbit() : base(SUMMON_TYPE.Rabbit, "Rabbit", RACE.RABBIT) {
    }
    public Rabbit(string className) : base(SUMMON_TYPE.Rabbit, className, RACE.RABBIT) {
    }
    public Rabbit(SaveDataShearableAnimal data) : base(data) {
    }

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.RABBIT_CLOTH;
}
