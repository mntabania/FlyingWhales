using System.Diagnostics;
namespace Goap.Unique_Action_Data {
    public class FirstAidCharacterUAD : UniqueActionData {
        
        public bool usedPoisonedHealingPotion { get; private set; }

        public FirstAidCharacterUAD() {
            usedPoisonedHealingPotion = false;
        }
        public FirstAidCharacterUAD(SaveDataFirstAidCharacterUAD saveData) {
            usedPoisonedHealingPotion = saveData.usedPoisonedHealingPotion;
        }
        public void SetUsedPoisonedHealingPotion(bool state) {
            usedPoisonedHealingPotion = state;
        }
        public override SaveDataUniqueActionData Save() {
            SaveDataFirstAidCharacterUAD saveData = new SaveDataFirstAidCharacterUAD();
            saveData.Save(this);
            return saveData;
        }
    }
    
    #region Save Data
    public class SaveDataFirstAidCharacterUAD : SaveDataUniqueActionData {
        public bool usedPoisonedHealingPotion;
        public override void Save(UniqueActionData data) {
            base.Save(data);
            FirstAidCharacterUAD uad = data as FirstAidCharacterUAD;
            Debug.Assert(uad != null, nameof(uad) + " != null");
            usedPoisonedHealingPotion = uad.usedPoisonedHealingPotion;
        }
        public override UniqueActionData Load() {
            return new FirstAidCharacterUAD(this);
        }
    }
    #endregion
}