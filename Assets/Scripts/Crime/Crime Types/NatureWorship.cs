using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class NatureWorship : CrimeType {
        public NatureWorship() : base(CRIME_TYPE.Nature_Worship) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " worships Nature";
        }
        #endregion
    }
}