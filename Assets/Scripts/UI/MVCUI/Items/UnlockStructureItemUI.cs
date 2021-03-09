using System;
using Ruinarch.Custom_UI;
using TMPro;
using UnityEngine;

public class UnlockStructureItemUI : MonoBehaviour {
    
    public static Action<PLAYER_SKILL_TYPE, int> onClickUnlockStructure;
    
    [SerializeField] private TextMeshProUGUI lblName;
    [SerializeField] private TextMeshProUGUI lblCosts;
    [SerializeField] private RuinarchButton btn;
    [SerializeField] private GameObject goCover;
    
    private PLAYER_SKILL_TYPE _structureType;
    private SkillData _skillData;
    
    public void SetStructureType(PLAYER_SKILL_TYPE p_type) {
        _structureType = p_type;
        _skillData = PlayerSkillManager.Instance.GetPlayerSkillData(p_type);
        lblName.text = _skillData.name;
        lblCosts.text = $"{_skillData.unlockCost.ToString()}{UtilityScripts.Utilities.ManaIcon()}";
        btn.onClick.AddListener(OnClickStructureItem);
    }
    private void OnClickStructureItem() {
        onClickUnlockStructure?.Invoke(_structureType, _skillData.unlockCost);
    }

    public void SetCoverState(bool p_state) {
        goCover.SetActive(p_state);
    }
    public void UpdateSelectableState() {
        if (PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills.Contains(_structureType)) {
            //player already has minion type
            SetCoverState(true);
            btn.interactable = false;
        } else {
            //player does not yet have minion type, check if player can afford to unlock this
            bool canAfford = PlayerManager.Instance.player.mana >= _skillData.unlockCost;
            btn.interactable = canAfford;
            SetCoverState(!canAfford);
        }
    }
}
