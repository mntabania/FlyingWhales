using System.Diagnostics;
namespace Goap.Unique_Action_Data {
    public class CureCharacterUAD : UniqueActionData {
        
        public bool usedPoisonedHealingPotion { get; private set; }

        public CureCharacterUAD() {
            usedPoisonedHealingPotion = false;
        }
        public CureCharacterUAD(SaveDataCureCharacterUAD saveData) {
            usedPoisonedHealingPotion = saveData.usedPoisonedHealingPotion;
        }
        public void SetUsedPoisonedHealingPotion(bool state) {
            usedPoisonedHealingPotion = state;
        }
        public override SaveDataUniqueActionData Save() {
            SaveDataCureCharacterUAD saveData = new SaveDataCureCharacterUAD();
            saveData.Save(this);
            return saveData;
        }
    }
    
    #region Save Data
    public class SaveDataCureCharacterUAD : SaveDataUniqueActionData {
        public bool usedPoisonedHealingPotion;
        public override void Save(UniqueActionData data) {
            base.Save(data);
            CureCharacterUAD cureCharacterUad = data as CureCharacterUAD;
            Debug.Assert(cureCharacterUad != null, nameof(cureCharacterUad) + " != null");
            usedPoisonedHealingPotion = cureCharacterUad.usedPoisonedHealingPotion;
        }
        public override UniqueActionData Load() {
            return new CureCharacterUAD(this);
        }
    }
    #endregion
}

