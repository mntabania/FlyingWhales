using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class SummonItem : PooledObject {
    public TextMeshProUGUI summonButtonText;
    public Toggle summonToggle;

    public SUMMON_TYPE summon { get; private set; }

    public void SetSummon(SUMMON_TYPE summon) {
        this.summon = summon;
        UpdateData();
        Messenger.AddListener<SUMMON_TYPE>(Signals.PLAYER_NO_ACTIVE_MONSTER, OnPlayerNoActiveSummon);
    }

    private void UpdateData() {
        summonButtonText.text = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(summon.ToString());
    }
    private void OnPlayerNoActiveSummon(SUMMON_TYPE summon) {
        if(this.summon == summon) {
            if (summonToggle.isOn) {
                summonToggle.isOn = false;
            }
        }
    }
    public void OnToggleSummon(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActiveSummon(SUMMON_TYPE.None);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActiveSummon(summon);
        } 
    }

    public override void Reset() {
        base.Reset();
        summon = SUMMON_TYPE.None;
        Messenger.RemoveListener<SUMMON_TYPE>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSummon);
    }
}
