using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Incubus : SeducerSummon {

    public const string ClassName = "Incubus";
    
    public Incubus() : base (SUMMON_TYPE.Incubus, GENDER.MALE, ClassName) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Electric);
    }
    public Incubus(string className) : base(SUMMON_TYPE.Incubus, GENDER.MALE, className) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Electric);
    }
    public Incubus(SaveDataCharacter data) : base(data) {
        //combatComponent.SetElementalType(ELEMENTAL_TYPE.Electric);
    }
    public override string GetClassForRole(CharacterRole role) {
        return ClassName;
    }
}