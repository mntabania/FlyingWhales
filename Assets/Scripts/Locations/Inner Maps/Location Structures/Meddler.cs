using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Meddler : DemonicStructure {
        public override string scenarioDescription => "The Meddler allows the player to interact with Villagers and offer them various benefits in exchange of something else. The player can tempt Villagers to leave their Faction, instigate a War or even break up with their partner.";

        public Meddler(Region location) : base(STRUCTURE_TYPE.MEDDLER, location){
            SetMaxHPAndReset(5000);
        }
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