using Inner_Maps;
using UnityEngine;

public class CharacterMoneyComponent : CharacterComponent {
    public int coins { get; private set; }

    public CharacterMoneyComponent() {
    }

    public CharacterMoneyComponent(SaveDataCharacterMoneyComponent data) {
        coins = data.coins;
    }

    #region General
    public void AdjustCoins(int amount) {
        coins += amount;
        if (coins < 0) {
            coins = 0;
        }
    }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataCarryComponent data) {
    }
    #endregion
}


[System.Serializable]
public class SaveDataCharacterMoneyComponent : SaveData<CharacterMoneyComponent> {
    public int coins;

    #region Overrides
    public override void Save(CharacterMoneyComponent data) {
        coins = data.coins;
    }

    public override CharacterMoneyComponent Load() {
        CharacterMoneyComponent component = new CharacterMoneyComponent(this);
        return component;
    }
    #endregion
}