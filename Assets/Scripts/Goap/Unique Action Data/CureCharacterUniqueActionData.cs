using System.Diagnostics;
namespace Goap.Unique_Action_Data {
    public class CureCharacterUniqueActionData : UniqueActionData {
        
        public bool usedPoisonedHealingPotion { get; private set; }

        public CureCharacterUniqueActionData() {
            usedPoisonedHealingPotion = false;
        }
        public CureCharacterUniqueActionData(SaveDataCureCharacterUniqueActionData saveData) {
            usedPoisonedHealingPotion = saveData.usedPoisonedHealingPotion;
        }
        public void SetUsedPoisonedHealingPotion(bool state) {
            usedPoisonedHealingPotion = state;
        }
        public override SaveDataUniqueActionData Save() {
            SaveDataCureCharacterUniqueActionData saveData = new SaveDataCureCharacterUniqueActionData();
            saveData.Save(this);
            return saveData;
        }
    }
    
    #region Save Data
    public class SaveDataCureCharacterUniqueActionData : SaveDataUniqueActionData {
        public bool usedPoisonedHealingPotion;
        public bool willUsePoisonedHealingPotion;
        public override void Save(UniqueActionData data) {
            base.Save(data);
            CureCharacterUniqueActionData cureCharacterUniqueActionData = data as CureCharacterUniqueActionData;
            Debug.Assert(cureCharacterUniqueActionData != null, nameof(cureCharacterUniqueActionData) + " != null");
            usedPoisonedHealingPotion = cureCharacterUniqueActionData.usedPoisonedHealingPotion;
        }
        public override UniqueActionData Load() {
            return new CureCharacterUniqueActionData(this);
        }
    }
    #endregion
}

