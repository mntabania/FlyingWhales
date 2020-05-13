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
    //[SerializeField] private GameObject hoverPortrait;

    public SpellData spellData { get; private set; }

    #region Overrides
    public override void SetObject(SpellData o) {
        base.SetObject(o);
        spellData = o;
        mainLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(spellData.type.ToString());
        subLbl.text = string.Empty;
        SetPortrait();
    }
    private void SetPortrait() {
        string[] raceClass = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(spellData.type.ToString()).Split(' ');
        string race = raceClass[0];
        string className = raceClass[1];

        classPortrait.GeneratePortrait(CharacterManager.Instance.GenerateRandomPortrait((RACE)System.Enum.Parse(typeof(RACE), race.ToUpper()), GENDER.MALE, className));
        //string className = string.Empty;
        //if (spellData is SummonPlayerSkill summonSkill) {
        //    className = summonSkill.className;
        //} else if (spellData is MinionPlayerSkill minionSkill) {
        //    className = minionSkill.className;
        //}
        //classPortrait.sprite = CharacterManager.Instance.GetWholeImagePortraitSprite(className);
    }
    public void SetCount(int count) {
        countText.text = count + "/" + spellData.charges;
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
