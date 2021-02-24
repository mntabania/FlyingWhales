using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class ManaPit : DemonicStructure {
        public ManaPit(Region location) : base(STRUCTURE_TYPE.MANA_PIT, location) { }
        public ManaPit(Region location, SaveDataDemonicStructure data) : base(location, data) { }

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