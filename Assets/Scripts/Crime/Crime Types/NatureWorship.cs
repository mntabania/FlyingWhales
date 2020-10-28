using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class NatureWorship : CrimeType {

        #region getters
        public override string accuseText => "being a Nature Worshiper";
        #endregion

        public NatureWorship() : base(CRIME_TYPE.Nature_Worship) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " worships Nature";
        }
        #endregion
    }
}