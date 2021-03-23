using EZObjectPools;
using TMPro;
using UnityEngine;

public class UpgradePortalItemUI : PooledObject {

    [SerializeField] private BaseCharacterPortrait characterPortrait;
    [SerializeField] private BaseLocationPortrait locationPortrait;
    [SerializeField] private TextMeshProUGUI lblName;

    public void SetData(PLAYER_SKILL_TYPE p_type) {
        SkillData skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_type);
        if (skillData.category == PLAYER_SKILL_CATEGORY.MINION) {
            MinionPlayerSkill minionPlayerSkill = PlayerSkillManager.Instance.GetMinionPlayerSkillData(p_type);
            locationPortrait.gameObject.SetActive(false);
            characterPortrait.gameObject.SetActive(true);
            characterPortrait.GeneratePortrait(minionPlayerSkill.minionType);
            lblName.text = minionPlayerSkill.name;
        } else if (skillData.category == PLAYER_SKILL_CATEGORY.DEMONIC_STRUCTURE) {
            DemonicStructurePlayerSkill demonicStructurePlayerSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(p_type);
            locationPortrait.gameObject.SetActive(true);
            characterPortrait.gameObject.SetActive(false);
            locationPortrait.SetPortrait(demonicStructurePlayerSkill.structureType);
            lblName.text = demonicStructurePlayerSkill.name;
        }
    }
}
