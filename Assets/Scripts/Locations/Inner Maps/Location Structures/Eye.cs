using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class Eye : DemonicStructure {
        public Eye(Region location) : base(STRUCTURE_TYPE.EYE, location){ }
        public Eye(Region location, SaveDataDemonicStructure data) : base(location, data) { }

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