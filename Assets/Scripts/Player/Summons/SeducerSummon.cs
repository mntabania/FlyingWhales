using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;

public class SeducerSummon : Summon {


    public SeducerSummon(SUMMON_TYPE type, GENDER gender, string className) : base(type, className, RACE.LESSER_DEMON, gender) { }
    public SeducerSummon(SaveDataCharacter data) : base(data) { }
    
}   

