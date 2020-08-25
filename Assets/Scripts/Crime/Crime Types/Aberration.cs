﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Crime_System {
    public class Aberration : CrimeType {

        #region getters
        public override string accuseText => "being an Aberration";
        #endregion

        public Aberration() : base(CRIME_TYPE.Aberration) { }

        #region Overrides
        public override string GetLastStrawReason(Character witness, Character actor, IPointOfInterest target, ICrimeable crime) {
            return actor.name + " is an aberration";
        }
        #endregion
    }
}