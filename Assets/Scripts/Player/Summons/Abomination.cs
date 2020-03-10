using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abomination : Summon {
    
    public override string raceClassName => "Abomination";
    
    public Abomination() : base(SUMMON_TYPE.Abomination, "Abomination", RACE.ABOMINATION,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public Abomination(SaveDataCharacter data) : base(data) { }
    
}
