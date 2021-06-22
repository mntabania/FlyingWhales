using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boar : SkinnableAnimal {

    //public override bool defaultDigMode => true;
    //public override COMBAT_MODE defaultCombatMode => COMBAT_MODE.Passive;
    public override string raceClassName => "Boar";
    public Boar() : base(SUMMON_TYPE.Boar, "Boar", RACE.BOAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Boar(string className) : base(SUMMON_TYPE.Boar, className, RACE.BOAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Boar(SaveDataSkinnableAnimal data) : base(data) { }

    public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.BOAR_HIDE;
}
