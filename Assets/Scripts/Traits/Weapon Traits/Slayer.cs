using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Traits {
    public class Slayer : Trait {

        protected Character owner;

        #region Getter
        public int stackCount = 0;
        #endregion
    }
}