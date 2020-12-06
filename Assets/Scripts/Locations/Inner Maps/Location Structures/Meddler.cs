using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Meddler : DemonicStructure {
        public override Vector2 selectableSize { get; }
        public Meddler(Region location) : base(STRUCTURE_TYPE.MEDDLER, location){
            selectableSize = new Vector2(10f, 10f);
        }
        public Meddler(Region location, SaveDataDemonicStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 10f);
        }
    }
}