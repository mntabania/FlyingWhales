using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class TheEye : DemonicStructure {
        public override Vector2 selectableSize { get; }

        public TheEye(Region location) : base(STRUCTURE_TYPE.THE_EYE, location){
            selectableSize = new Vector2(10f, 10f);
        }

        #region Initialization
        public override void Initialize() {
            base.Initialize();
            location.AllowNotifications();
        }
        #endregion

        #region Overrides
        protected override void DestroyStructure() {
            base.DestroyStructure();
            location.BlockNotifications();
        }
        #endregion
    }
}