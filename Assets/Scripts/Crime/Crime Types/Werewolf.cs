using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Werewolf : CrimeType {

        #region getters
        public override string accuseText => "being a Werewolf";
        #endregion

        public Werewolf() : base(CRIME_TYPE.Werewolf) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target) {
            if (witness.traitContainer.HasTrait("Cultist") && actor.traitContainer.HasTrait("Cultist")) {
                return CRIME_SEVERITY.None;
            }
            if (witness.traitContainer.HasTrait("Lycanthrope")) {
                return CRIME_SEVERITY.None;
            }
            return base.GetCrimeSeverity(witness, actor, target);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a werewolf";
        }
        #endregion
    }
}