﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Werewolf : CrimeType {
        public Werewolf() : base(CRIME_TYPE.Werewolf) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            if (witness.traitContainer.HasTrait("Lycanthrope")) {
                return CRIME_SEVERITY.None;
            }
            return base.GetCrimeSeverity(witness, actor, target, crime);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a werewolf";
        }
        #endregion
    }
}