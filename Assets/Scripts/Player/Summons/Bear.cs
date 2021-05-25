using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bear : Summon {
	
	//public override bool defaultDigMode => true;
    public override string raceClassName => "Bear";
    public Bear() : base(SUMMON_TYPE.Bear, "Bear", RACE.BEAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Bear(string className) : base(SUMMON_TYPE.Bear, className, RACE.BEAR, UtilityScripts.Utilities.GetRandomGender()) { }
    public Bear(SaveDataSummon data) : base(data) { }
}
