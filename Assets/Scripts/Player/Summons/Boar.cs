﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boar : Summon {
	
	//public override bool defaultDigMode => true;
    public override string raceClassName => "Boar";
    public Boar() : base(SUMMON_TYPE.Boar, "Boar", RACE.BOAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Boar(string className) : base(SUMMON_TYPE.Boar, className, RACE.BOAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Boar(SaveDataSummon data) : base(data) { }
}
