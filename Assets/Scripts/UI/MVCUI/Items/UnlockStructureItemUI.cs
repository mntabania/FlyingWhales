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
    private PlayerSkillData _playerSkillData;
    
    public void SetStructureType(PLAYER_SKILL_TYPE p_type) {
        _structureType = p_type;
        _playerSkillData = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(p_type);
        lblName.text = _playerSkillData.name;
        lblCosts.text = $"{_playerSkillData.GetUnlockCost()}{UtilityScripts.Utilities.ManaIcon()}";
        btn.onClick.AddListener(OnClickStructureItem);
    }
    private void OnClickStructureItem() {
        onClickUnlockStructure?.Invoke(_structureType, _playerSkillData.GetUnlockCost());
    }

    public void SetCoverState(bool p_state) {
        goCover.SetActive(p_state);
    }
    public void UpdateSelectableState() {
        SkillData skillData = PlayerSkillManager.Instance.GetSkillData(_structureType);
        if (skillData.isInUse) {
            //player already has structure
            SetCoverState(true);
            btn.interactable = false;
        } else {
            //player does not yet have structure, check if player can afford to unlock this
            bool canAfford = PlayerManager.Instance.player.mana >= _playerSkillData.GetUnlockCost();
            btn.interactable = canAfford;
            SetCoverState(!canAfford);
        }
    }
}
