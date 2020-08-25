﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Vampire : CrimeType {

        #region getters
        public override string accuseText => "being a Vampire";
        #endregion

        public Vampire() : base(CRIME_TYPE.Vampire) { }

        #region Overrides
        public override CRIME_SEVERITY GetCrimeSeverity(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            if (witness.traitContainer.HasTrait("Vampiric")) {
                return CRIME_SEVERITY.None;
            }
            return base.GetCrimeSeverity(witness, actor, target, crime);
        }
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is a vampire";
        }
        #endregion
    }
}