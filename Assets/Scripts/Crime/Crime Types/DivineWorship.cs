using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class DivineWorship : CrimeType {

        #region getters
        public override string accuseText => "being a Divine Worshiper";
        #endregion

        public DivineWorship() : base(CRIME_TYPE.Divine_Worship) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target) {
            if (witness.religionComponent.religion == RELIGION.Demon_Worship) {
                return CRIME_SEVERITY.Serious;
            }
            return base.GetCrimeSeverity(witness, actor, target);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " worships the Divine";
        }
        #endregion
    }
}