using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class Spire : DemonicStructure {
        public Spire(Region location) : base(STRUCTURE_TYPE.SPIRE, location) { }
        public Spire(Region location, SaveDataDemonicStructure data) : base(location, data) { }

        #region Initialization
        public override void Initialize() {
            base.Initialize();
            region.AllowNotifications();
        }
        #endregion

        #region Overrides
        protected override void DestroyStructure() {
            base.DestroyStructure();
            region.BlockNotifications();
        }
        #endregion
    }
}