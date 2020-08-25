using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Disturbances : CrimeType {

        #region getters
        public override string accuseText => "Disturbance";
        #endregion

        public Disturbances() : base(CRIME_TYPE.Disturbances) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is unruly";
        }
        #endregion
    }
}