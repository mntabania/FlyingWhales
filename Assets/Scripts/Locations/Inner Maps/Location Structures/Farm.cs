using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

namespace Inner_Maps.Location_Structures {
    public class Farm : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Farm(Region location) : base(STRUCTURE_TYPE.FARM, location){
            selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        public Farm(Region location, SaveDataManMadeStructure data) : base(location, data) {
            selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
        }

        public override void Initialize() {
            base.Initialize();
            Messenger.AddListener(Signals.HOUR_STARTED, OnHourStarted);
        }
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            Messenger.RemoveListener(Signals.HOUR_STARTED, OnHourStarted);
        }

        private void OnHourStarted() {
            if(GameManager.Instance.currentTick == 120) { //6am
                List<TileObject> tileObjects = RuinarchListPool<TileObject>.Claim();
                PopulateCornCropsThatIsNotRipe(tileObjects);
                int numOfCropsToRipen = GameUtilities.RandomBetweenTwoNumbers(2, 3);
                for (int i = 0; i < numOfCropsToRipen; i++) {
                    if(tileObjects.Count > 0) {
                        int chosenIndex = GameUtilities.RandomBetweenTwoNumbers(0, tileObjects.Count - 1);
                        CornCrop chosenCrop = tileObjects[chosenIndex] as CornCrop;
                        chosenCrop.SetGrowthState(Crops.Growth_State.Ripe);
                        tileObjects.RemoveAt(chosenIndex);
                    } else {
                        break;
                    }
                }
                RuinarchListPool<TileObject>.Release(tileObjects);
            }
        }
    }
}