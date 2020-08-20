using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Trespassing : CrimeType {
        public Trespassing() : base(CRIME_TYPE.Trespassing) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a trespasser";
        }
        #endregion
    }
}