﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Succubus : SeducerSummon {

    public const string ClassName = "Succubus";
    public override string raceClassName => ClassName;
    
    public Succubus() : base(SUMMON_TYPE.Succubus, GENDER.FEMALE, ClassName){
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
    }
    public Succubus(SaveDataCharacter data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Ice);
    }
    
    public override string GetClassForRole(CharacterRole role) {
        return ClassName;
    }
}