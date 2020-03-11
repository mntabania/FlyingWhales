using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpChoiceItem : PooledObject {

    private object obj;

    [SerializeField] private Image img;
    [SerializeField] private TextMeshProUGUI info;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite lockedSprite;

    public Toggle toggle;

    private System.Action<object> onSelected;

    public void SetObject(object obj, System.Action<object> onSelected) {
        this.obj = obj;
        this.onSelected = onSelected;
        img.sprite = defaultSprite;
        if (obj is SummonSlot) {
            SummonSlot summonSlot = obj as SummonSlot;
            toggle.interactable = summonSlot.level < PlayerDB.MAX_LEVEL_SUMMON;
            if (summonSlot.isLocked) {
                img.sprite = lockedSprite;
            } else if (summonSlot.summon != null) {
                img.sprite = CharacterManager.Instance.GetSummonSettings(summonSlot.summon.summonType).summonPortrait;
            }
        } else if (obj is ArtifactSlot) {
            ArtifactSlot artifactSlot = obj as ArtifactSlot;
            toggle.interactable = artifactSlot.level < PlayerDB.MAX_LEVEL_ARTIFACT;
            if (artifactSlot.isLocked) {
                img.sprite = lockedSprite;
            } else if (artifactSlot.artifact != null) {
                img.sprite = CharacterManager.Instance.GetArtifactSettings(artifactSlot.artifact.type).artifactPortrait;
            }
        } else if (obj is PlayerSpell) {
            PlayerSpell interventionAbility = obj as PlayerSpell;
            toggle.interactable = interventionAbility.level < PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY;
            img.sprite = PlayerManager.Instance.GetJobActionSprite(interventionAbility.name);
        } else if (obj is CombatAbility) {
            CombatAbility combatAbility = obj as CombatAbility;
            toggle.interactable = combatAbility.lvl < PlayerDB.MAX_LEVEL_COMBAT_ABILITY;
            img.sprite = PlayerManager.Instance.GetCombatAbilitySprite(combatAbility.name);
        }
        UpdateTextInfo();
    }

    public void OnClick(bool selected) {
        if (selected) {
            onSelected.Invoke(obj);
        }
    }

    private void UpdateTextInfo() {
        if (obj is SummonSlot) {
            SummonSlot summonSlot = obj as SummonSlot;
            string text = "Summon Slot";
            text += $"\nLevel: {summonSlot.level}";
            if (summonSlot.summon != null) {
                text += $"\nAttached Summon: {summonSlot.summon.summonType.SummonName()}";
            } else {
                text += "\nAttached Summon: None";
            }
            info.text = text;
        } else if (obj is ArtifactSlot) {
            ArtifactSlot artifactSlot = obj as ArtifactSlot;
            string text = "Artifact Slot";
            text += $"\nLevel: {artifactSlot.level}";
            if(artifactSlot.artifact != null) {
                text += $"\nAttached Artifact: {artifactSlot.artifact.name}";
            } else {
                text += "\nAttached Artifact: None";
            }
            info.text = text;
        } else if (obj is PlayerSpell) {
            PlayerSpell action = obj as PlayerSpell;
            string text = action.name;
            text += $"\nDescription: {action.description}";
            info.text = text;
        } else if (obj is CombatAbility) {
            CombatAbility ability = obj as CombatAbility;
            string text = ability.name;
            text += $"\nDescription: {ability.description}";
            info.text = text;
        }
    }

    public override void Reset() {
        base.Reset();
        obj = null;
        toggle.group = null;
        toggle.isOn = false;
    }
}
