using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class ThePortal : DemonicStructure {

        public ThePortal(Region location) : base(STRUCTURE_TYPE.THE_PORTAL, location){
            name = "Portal";
            SetMaxHPAndReset(5000);
        }
        public ThePortal(Region location, SaveDataDemonicStructure data) : base(location, data) { }
        protected override void DestroyStructure() {
            base.DestroyStructure();
            PlayerUI.Instance.LoseGameOver();
        }
    }
}