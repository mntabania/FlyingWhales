using System.Diagnostics;
namespace Goap.Unique_Action_Data {
    public class FeedUAD : UniqueActionData {
        
        public bool usedPoisonedFood { get; private set; }

        public FeedUAD() {
            usedPoisonedFood = false;
        }
        public FeedUAD(SaveDataFeedUAD saveData) {
            usedPoisonedFood = saveData.usedPoisonedFood;
        }
        public void SetUsedPoisonedFood(bool state) {
            usedPoisonedFood = state;
        }
        public override SaveDataUniqueActionData Save() {
            SaveDataFeedUAD saveData = new SaveDataFeedUAD();
            saveData.Save(this);
            return saveData;
        }
    }
    
    #region Save Data
    public class SaveDataFeedUAD : SaveDataUniqueActionData {
        public bool usedPoisonedFood;
        public override void Save(UniqueActionData data) {
            base.Save(data);
            FeedUAD uad = data as FeedUAD;
            Debug.Assert(uad != null, nameof(uad) + " != null");
            usedPoisonedFood = uad.usedPoisonedFood;
        }
        public override UniqueActionData Load() {
            return new FeedUAD(this);
        }
    }
    #endregion
}