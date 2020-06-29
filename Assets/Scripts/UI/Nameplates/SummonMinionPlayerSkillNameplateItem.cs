using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummonMinionPlayerSkillNameplateItem : SpellItem {

    [Header("Summon/Minion PlayerSkill Nameplate Attributes")]
    [SerializeField] private CharacterPortrait classPortrait;
    [SerializeField] private TextMeshProUGUI countText;
    
    #region Overrides
    public override void SetObject(SpellData o) {
        base.SetObject(o);
        mainLbl.text = spellData.name;
        subLbl.text = string.Empty;
        SetPortrait();
    }
    private void SetPortrait() {
        RACE race = RACE.NONE;
        string className = string.Empty;

        if(spellData is MinionPlayerSkill minionPlayerSkill) {
            race = minionPlayerSkill.race;
            className = minionPlayerSkill.className;
        } else if (spellData is SummonPlayerSkill summonPlayerSkill) {
            race = summonPlayerSkill.race;
            className = summonPlayerSkill.className;
        }

        if(race != RACE.NONE && className != string.Empty) {
            classPortrait.GeneratePortrait(CharacterManager.Instance.GenerateRandomPortrait(race, GENDER.MALE, className));
        } else {
            throw new System.Exception("Trying to create portrait for " + spellData.name + " but Race or Class is None");
        }
    }
    [System.Obsolete("Use UpdateData function instead")]
    public void SetCount(int count, bool useCountOnly = false) {
        if (!useCountOnly) {
            countText.text = count + "/" + spellData.charges;
        } else {
            countText.text = "" + count;
        }
    }
    public override void OnHoverSpell() {
        PlayerUI.Instance.OnHoverSpell(spellData, PlayerUI.Instance.minionListHoverPosition);
    }
    #endregion
}
