using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionLeaderPicker : MonoBehaviour {

    public Minion minion { get; private set; } 

    public Image imgHighlight;
    public CharacterPortrait portrait;

    public void SetMinion(Minion minion) {
        this.minion = minion;
        if(minion != null) {
            portrait.GeneratePortrait(minion.character);
        }
    }

    public void OnClickSetMinionLeader() {
        // PlayerUI.Instance.TemporarySetMinionLeader(this);
    }
    public void OnHover() {
        string text = minion.character.name;
        text += $"\n{minion.character.raceClassName}";
        UIManager.Instance.ShowSmallInfo(text);
    }
    public void OnHoverOut() {
        UIManager.Instance.HideSmallInfo();
    }
}
