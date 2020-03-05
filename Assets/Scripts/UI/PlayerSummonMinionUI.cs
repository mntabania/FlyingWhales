using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSummonMinionUI : MonoBehaviour {
    //public ThePortal portal { get; private set; }
    
    #region General
    public void OnClickSummon(BaseLandmark landmark) {
        //portal = landmark as ThePortal;
        //show dual object picker, and allow only 1 object to be picked
        //column 1 should contain all minions to be summoned and column 2 should contain the players minions
        // UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(PlayerManager.Instance.player.minions.Select(x => x.character).ToList(), PlayerManager.Instance.player.minionsToSummon.ToList(),
        //     "Choose Minion to Help (Optional)", "Choose Minion to Summon",
        //     CanChooseMinion, null,
        //     OnHoverEnterMinion, OnHoverMinionToSummonChoice,
        //     OnHoverExitMinion, OnHoverExitMinionToSummonChoice,
        //     ConfirmSummon, "Summon", column1Optional: true
        // );
    }
    private void OnHoverEnterMinion(Character character) {
        if (!CanChooseMinion(character)) {
            string message = string.Empty;
            if (character.minion.isAssigned) {
                message = $"{character.name} is already doing something else.";
            }
            UIManager.Instance.ShowSmallInfo(message);
        }
    }
    private void OnHoverExitMinion(Character character) {
        UIManager.Instance.HideSmallInfo();
    }
    private void ConfirmSummon(object minionObj, object summonObj) {
        UnsummonedMinionData data = (UnsummonedMinionData)summonObj;
        Minion minion = (minionObj as Character)?.minion ?? null;

        Debug.Log($"Will summon {data.className} helped by {(minion?.character.name ?? "No one")}");

        int summonDuration = LandmarkManager.SUMMON_MINION_DURATION;
        if (minion != null) {
            int speedUpDuration = Mathf.CeilToInt(LandmarkManager.SUMMON_MINION_DURATION * 0.25f);
            summonDuration -= speedUpDuration;
        }

        //if (minion != null) {
        //    minion.SetAssignedRegion(portal.tileLocation.region);
        //    portal.tileLocation.region.SetAssignedMinion(minion);
        //}
        // portal.StartSummon(System.Array.IndexOf(PlayerManager.Instance.player.minionsToSummon, data), 0, summonDuration);
        
    }
    #endregion

    #region Minion
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    #endregion

    #region Summon
    public void OnHoverMinionToSummonChoice(UnsummonedMinionData minionClassName) {
        string info = $"Duration: {GameManager.Instance.GetCeilingHoursBasedOnTicks(LandmarkManager.SUMMON_MINION_DURATION).ToString()} hours";
        UIManager.Instance.ShowSmallInfo(info);
    }
    public void OnHoverExitMinionToSummonChoice(UnsummonedMinionData minionClassName) {
        UIManager.Instance.HideSmallInfo();
    }
    #endregion

}
