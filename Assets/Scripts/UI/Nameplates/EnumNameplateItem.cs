using System;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class EnumNameplateItem : NameplateItem<Enum> {

    [SerializeField] private Image portrait;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private Image lockedImage;
    
    public override void SetObject(Enum o) {
        base.SetObject(o);
        mainLbl.text = UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(o.ToString());
        subLbl.text = string.Empty;

        //TODO: Make this better
        if (o is SPELL_TYPE spellType) {
            if (WorldConfigManager.Instance.isDemoWorld) {
                SetLockedState(WorldConfigManager.Instance.availableSpellsInDemoBuild.Contains(spellType) == false);
            }
            SpellData spellData;
            if (PlayerSkillManager.Instance.IsAffliction(spellType)) {
                spellData = PlayerSkillManager.Instance.GetAfflictionData(spellType);
            } else if (PlayerSkillManager.Instance.IsPlayerAction(spellType)) {
                spellData = PlayerSkillManager.Instance.GetPlayerSkillData(spellType);
            } else if (PlayerSkillManager.Instance.IsDemonicStructure(spellType)) {
                spellData = PlayerSkillManager.Instance.GetDemonicStructureSkillData(spellType);
            } else {
                spellData = PlayerSkillManager.Instance.GetSpellData(spellType);
            }
            Assert.IsNotNull(spellData, $"There is no spellData for {spellType.ToString()}");
            SetCooldownState(spellData.isInCooldown);
        } else {
            SetLockedState(false);
            SetCooldownState(false);
        }
    }
    public void SetPortrait(Sprite sprite) {
        portrait.sprite = sprite;
        portrait.gameObject.SetActive(portrait.sprite != null);
        
    }

    private void SetLockedState(bool state) {
        lockedImage.gameObject.SetActive(state);
    }
    private void SetCooldownState(bool state) {
        cooldownImage.gameObject.SetActive(state);
        
    }
}
