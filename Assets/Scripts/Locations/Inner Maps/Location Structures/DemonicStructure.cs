using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class DemonicStructure : LocationStructure {
        public int structureHP { get; private set; }

        public DemonicStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
        }
        public DemonicStructure(Region location, SaveDataLocationStructure data) : base(location, data) {
        }


        #region HP
        public void AdjustHP(int amount, bool shouldDestroyStructure = true) {
            structureHP += amount;
            if (structureHP <= 0) {
                structureHP = 0;
                if (shouldDestroyStructure) {
                    DestroyStructure();
                }
            }
        }
        #endregion
    }
}
