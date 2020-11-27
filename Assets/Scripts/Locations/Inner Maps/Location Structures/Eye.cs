using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class Eye : DemonicStructure {
        public override Vector2 selectableSize { get; }

        public Eye(Region location) : base(STRUCTURE_TYPE.EYE, location){
            selectableSize = new Vector2(10f, 10f);
        }
        public Eye(Region location, SaveDataDemonicStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 10f);
        }

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