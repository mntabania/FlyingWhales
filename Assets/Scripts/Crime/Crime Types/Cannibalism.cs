using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Cannibalism : CrimeType {

        public Cannibalism() : base(CRIME_TYPE.Cannibalism) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            if (witness.traitContainer.HasTrait("Cannibal")) {
                return CRIME_SEVERITY.None;
            }
            return base.GetCrimeSeverity(witness, actor, target, crime);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a cannibal";
        }
        #endregion
    }
}