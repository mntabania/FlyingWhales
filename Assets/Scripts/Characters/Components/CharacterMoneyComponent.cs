using Inner_Maps;
using UnityEngine;
using UtilityScripts;
public class CharacterMoneyComponent : CharacterComponent {
    public int coins { get; private set; }

    public CharacterMoneyComponent() {
        coins = 50;
    }

    public CharacterMoneyComponent(SaveDataCharacterMoneyComponent data) {
        coins = data.coins;
    }

    #region General
    public void Initialize() {
        if (!owner.isNormalCharacter || owner.isConsideredRatman) {
            coins = 0; //set initial coins of monsters and ratmen to 0
        }
    }
    public void AdjustCoins(int amount) {
        coins += amount;
        if (coins < 0) {
            coins = 0;
        }
        if (owner.hasMarker) {
            Color textColor = Color.green;
            string text = string.Empty;
            if (amount < 0) {
                textColor = Color.red;
                text = $"{amount}{UtilityScripts.Utilities.TileObjectIcon()}";
            } else {
                text = $"+{amount}{UtilityScripts.Utilities.TileObjectIcon()}";
            }
            if (text != string.Empty) {
                InnerMapManager.Instance.ShowAreaMapTextPopup(text, owner.worldPosition, textColor);
            }
        }
    }
    public bool HasCoins() {
        return coins > 0;
    }
    public bool CanAfford(int p_amount) {
        return coins >= p_amount;
    }
    #endregion

    #region Gain Coins
    public void GainCoinsAfterDoingAction(ActualGoapNode p_action) {
        //if (p_action.goapType == INTERACTION_TYPE.HARVEST_PLANT || p_action.goapType == INTERACTION_TYPE.CRAFT_TILE_OBJECT 
        //    || p_action.goapType == INTERACTION_TYPE.REPAIR || p_action.goapType == INTERACTION_TYPE.REPAIR_STRUCTURE || p_action.goapType == INTERACTION_TYPE.PATROL
        //    || p_action.goapType == INTERACTION_TYPE.BUTCHER || p_action.goapType == INTERACTION_TYPE.BURY_CHARACTER || p_action.goapType == INTERACTION_TYPE.CHOP_WOOD
        //    || p_action.goapType == INTERACTION_TYPE.MINE_METAL || p_action.goapType == INTERACTION_TYPE.MINE_STONE) {
        //    AdjustCoins(GameUtilities.RandomBetweenTwoNumbers(3, 6));
        //}
    }
    public void GainCoinsAfterDoingJob(JobQueueItem p_job) {
        //if (p_job.jobType == JOB_TYPE.HAUL) {
        //    AdjustCoins(GameUtilities.RandomBetweenTwoNumbers(3, 6));
        //}
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