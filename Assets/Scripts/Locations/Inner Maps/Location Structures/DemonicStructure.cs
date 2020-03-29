using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class DemonicStructure : LocationStructure {
        public int structureHP { get; private set; }

        public DemonicStructure(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) {
            AdjustHP(2000);
        }
        public DemonicStructure(Region location, SaveDataLocationStructure data) : base(location, data) {
            AdjustHP(2000);
        }


        #region HP
        public void AdjustHP(int amount, bool shouldDestroyStructure = true) {
            structureHP += amount;
            structureHP = Mathf.Clamp(structureHP, 0, 3000);
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
