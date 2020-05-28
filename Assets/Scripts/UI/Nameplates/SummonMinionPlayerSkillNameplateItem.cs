using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SummonMinionPlayerSkillNameplateItem : NameplateItem<SpellData> {

    [Header("Summon/Minion PlayerSkill Nameplate Attributes")]
    [SerializeField] private CharacterPortrait classPortrait;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI countTextWithIcon;
    //[SerializeField] private GameObject hoverPortrait;

    public SpellData spellData { get; private set; }

    #region Overrides
    public override void SetObject(SpellData o) {
        base.SetObject(o);
        spellData = o;
        mainLbl.text = spellData.name;
        subLbl.text = string.Empty;
        SetPortrait();
    }
    private void SetPortrait() {
        //string[] raceClass = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(spellData.type.ToString()).Split(' ');
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
        //string className = string.Empty;
        //if (spellData is SummonPlayerSkill summonSkill) {
        //    className = summonSkill.className;
        //} else if (spellData is MinionPlayerSkill minionSkill) {
        //    className = minionSkill.className;
        //}
        //classPortrait.sprite = CharacterManager.Instance.GetWholeImagePortraitSprite(className);
    }
    public void SetCount(int count, bool useCountOnly = false) {
        if (!useCountOnly) {
            countText.text = count + "/" + spellData.charges;
            countTextWithIcon.gameObject.SetActive(false);
            countText.gameObject.SetActive(true);
        } else {
            countTextWithIcon.text = "" + count;
            countTextWithIcon.gameObject.SetActive(true);
            countText.gameObject.SetActive(false);
        }
    }
    //public override void OnHoverEnter() {
    //    hoverPortrait.SetActive(true);
    //    //UIManager.Instance.ShowMinionCardTooltip(minionData);
    //    base.OnHoverEnter();
    //}
    //public override void OnHoverExit() {
    //    hoverPortrait.SetActive(false);
    //    //UIManager.Instance.HideMinionCardTooltip();
    //    base.OnHoverExit();
    //}
    #endregion
}
