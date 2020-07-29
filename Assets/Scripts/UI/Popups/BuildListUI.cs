using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Settings;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class BuildListUI : PopupMenuBase {

    [SerializeField] private Toggle buildToggle;
    [SerializeField] private ScrollRect buildingsScrollRect;
    [SerializeField] private GameObject spellItemPrefab;
    private SpellItem[] buildItems;
    private void Awake() {
        buildToggle.interactable = false;
        Close();
    }
    public override void Open() {
        base.Open();
        UpdateBuildList();
        buildToggle.SetIsOnWithoutNotify(true);
    }
    public override void Close() {
        buildToggle.SetIsOnWithoutNotify(false);
        base.Close();
    }

    public void Initialize() {
        PopulateBuildingList();
        Messenger.AddListener(Signals.UPDATE_BUILD_LIST, UpdateBuildList);
        buildToggle.interactable = true;
    }
    
    public void PopulateBuildingList() {
        buildItems = new SpellItem[PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills.Count];
        for (int i = 0; i < PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills.Count; i++) {
            SPELL_TYPE structureSpell = PlayerManager.Instance.player.playerSkillComponent.demonicStructuresSkills[i];
            DemonicStructurePlayerSkill demonicStructurePlayerSkill = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureSpell);
            GameObject spellNameplate = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellItemPrefab.name,
                Vector3.zero, Quaternion.identity, buildingsScrollRect.content);
            SpellItem spellItem = spellNameplate.GetComponent<SpellItem>();
            spellItem.SetObject(demonicStructurePlayerSkill);
            spellItem.SetInteractableState(CanChooseLandmark(demonicStructurePlayerSkill.type));
            buildItems[i] = spellItem;
        }
    }
    private void UpdateBuildList() {
        for (int i = 0; i < buildItems.Length; i++) {
            SpellItem item = buildItems[i];
            item.SetInteractableState(CanChooseLandmark(item.spellData.type));
            if (item.toggle.interactable) {
                item.transform.SetAsFirstSibling();
            } else {
                item.transform.SetAsLastSibling();
            }
        }
    }
    private bool CanChooseLandmark(SPELL_TYPE structureType) {
        if (InnerMapManager.Instance.currentlyShowingLocation == null) {
            return false;
        }
        bool canChooseLandmark = true;
        
        if (WorldConfigManager.Instance.isTutorialWorld) {
            canChooseLandmark = WorldConfigManager.Instance.availableSpellsInTutorial.Contains(structureType) 
                   && PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType).CanPerformAbility();
        } else {
            if (structureType == SPELL_TYPE.EYE && InnerMapManager.Instance.currentlyShowingLocation.HasStructure(STRUCTURE_TYPE.EYE)) {
                canChooseLandmark = false; //only 1 eye per region.
            }
            if (structureType == SPELL_TYPE.MEDDLER && PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER)) {
                canChooseLandmark = false; //only 1 finger at a time.
            }
            if (canChooseLandmark) {
                canChooseLandmark = PlayerSkillManager.Instance.GetDemonicStructureSkillData(structureType).CanPerformAbility();
            }
        }
        
        if (canChooseLandmark && WorldConfigManager.Instance.isTutorialWorld) {
            if (structureType == SPELL_TYPE.TORTURE_CHAMBERS) {
                return
                    TutorialManager.Instance.HasTutorialBeenCompleted(TutorialManager.Tutorial.Prison) ||
                    TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Prison);
            } else if (structureType == SPELL_TYPE.KENNEL) {
                return
                    TutorialManager.Instance.HasTutorialBeenCompleted(TutorialManager.Tutorial.Build_A_Kennel) ||
                    TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Build_A_Kennel);
            } else if (structureType == SPELL_TYPE.EYE) {
                return
                    TutorialManager.Instance.HasTutorialBeenCompleted(TutorialManager.Tutorial.Share_An_Intel) ||
                    TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Share_An_Intel);
            } else if (structureType == SPELL_TYPE.DEFILER) {
                return
                    TutorialManager.Instance.HasTutorialBeenCompleted(TutorialManager.Tutorial.Create_A_Cultist) ||
                    TutorialManager.Instance.IsTutorialCurrentlyActive(TutorialManager.Tutorial.Create_A_Cultist);
            }
        }
        return canChooseLandmark;
    }
}