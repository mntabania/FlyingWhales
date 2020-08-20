using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class DivineWorship : CrimeType {
        public DivineWorship() : base(CRIME_TYPE.Divine_Worship) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " worships the Divine";
        }
        #endregion
    }
}