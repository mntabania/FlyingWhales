using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bear : SkinnableAnimal {

    //public override bool defaultDigMode => true;
    //public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public override string raceClassName => "Bear";
    public Bear() : base(SUMMON_TYPE.Bear, "Bear", RACE.BEAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Bear(string className) : base(SUMMON_TYPE.Bear, className, RACE.BEAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Bear(SaveDataSkinnableAnimal data) : base(data) { }

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.BEAR_HIDE;
}
