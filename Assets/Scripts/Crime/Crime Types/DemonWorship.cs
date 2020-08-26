using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class DemonWorship : CrimeType {

        #region getters
        public override string accuseText => "being a Demon Worshiper";
        #endregion

        public DemonWorship() : base(CRIME_TYPE.Demon_Worship) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            if (witness.traitContainer.HasTrait("Cultist") && actor.traitContainer.HasTrait("Cultist")) {
                return CRIME_SEVERITY.None;
            }
            if (witness.traitContainer.HasTrait("Cultist")) {
                return CRIME_SEVERITY.None;
            }
            return base.GetCrimeSeverity(witness, actor, target, crime);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " worships demons";
        }
        #endregion
    }
}