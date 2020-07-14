using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abomination : Summon {
    
    public override string raceClassName => "Abomination";
    
    public Abomination() : base(SUMMON_TYPE.Abomination, "Abomination", RACE.ABOMINATION,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public Abomination(string className) : base(SUMMON_TYPE.Abomination, className, RACE.ABOMINATION,
        UtilityScripts.Utilities.GetRandomGender()) { }
    public Abomination(SaveDataCharacter data) : base(data) { }
    
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Abomination_Behaviour);
    }
    public override void PerTickDuringMovement() {
        base.PerTickDuringMovement();
        if (gridTileLocation?.objHere != null && Random.Range(0, 100) < 5) {
            IPointOfInterest affectedObj = gridTileLocation.objHere;
            affectedObj.traitContainer.AddTrait(affectedObj, "Abomination Germ", this);
        }
    }
}
