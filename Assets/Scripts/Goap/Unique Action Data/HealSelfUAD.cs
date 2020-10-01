using System.Diagnostics;
namespace Goap.Unique_Action_Data {
    public class HealSelfUAD : UniqueActionData {
        
        public bool usedPoisonedHealingPotion { get; private set; }

        public HealSelfUAD() {
            usedPoisonedHealingPotion = false;
        }
        public HealSelfUAD(SaveDataHealSelfUAD saveData) {
            usedPoisonedHealingPotion = saveData.usedPoisonedHealingPotion;
        }
        public void SetUsedPoisonedHealingPotion(bool state) {
            usedPoisonedHealingPotion = state;
        }
        public override SaveDataUniqueActionData Save() {
            SaveDataHealSelfUAD saveData = new SaveDataHealSelfUAD();
            saveData.Save(this);
            return saveData;
        }
    }
    
    #region Save Data
    public class SaveDataHealSelfUAD : SaveDataUniqueActionData {
        public bool usedPoisonedHealingPotion;
        public override void Save(UniqueActionData data) {
            base.Save(data);
            HealSelfUAD uad = data as HealSelfUAD;
            Debug.Assert(uad != null, nameof(uad) + " != null");
            usedPoisonedHealingPotion = uad.usedPoisonedHealingPotion;
        }
        public override UniqueActionData Load() {
            return new HealSelfUAD(this);
        }
    }
    #endregion
}