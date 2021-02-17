using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Meddler : DemonicStructure {
        public Meddler(Region location) : base(STRUCTURE_TYPE.MEDDLER, location){ }
        public Meddler(Region location, SaveDataDemonicStructure data) : base(location, data) { }
        
        // #region Initialization
        // public override void Initialize() {
        //     base.Initialize();
        //     region.AllowNotifications();
        // }
        // #endregion
        //
        // #region Overrides
        // protected override void DestroyStructure() {
        //     base.DestroyStructure();
        //     region.BlockNotifications();
        // }
        // #endregion
    }
}