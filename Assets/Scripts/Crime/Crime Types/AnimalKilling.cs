using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class AnimalKilling : CrimeType {

        public AnimalKilling() : base(CRIME_TYPE.Animal_Killing) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " killed an animal";
        }
        #endregion
    }
}