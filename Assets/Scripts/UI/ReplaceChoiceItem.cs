using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReplaceChoiceItem : PooledObject {

    private object obj;

    [SerializeField] private Image img;
    [SerializeField] private CharacterPortrait portrait;
    [SerializeField] private TextMeshProUGUI info;
    public Toggle toggle;

    private System.Action<object> onSelected;

    public void SetObject(object obj, System.Action<object> onSelected) {
        this.obj = obj;
        this.onSelected = onSelected;
        img.gameObject.SetActive(false);
        portrait.gameObject.SetActive(false);
        if (obj is Summon) {
            img.sprite = CharacterManager.Instance.GetSummonSettings((obj as Summon).summonType).summonPortrait;
            img.gameObject.SetActive(true);
        } else if (obj is Artifact) {
            img.sprite = CharacterManager.Instance.GetArtifactSettings((obj as Artifact).type).artifactPortrait;
            img.gameObject.SetActive(true);
        } else if (obj is PlayerSpell) {
            img.sprite = PlayerManager.Instance.GetJobActionSprite((obj as PlayerSpell).name);
            img.gameObject.SetActive(true);
        } else if (obj is CombatAbility) {
            img.sprite = PlayerManager.Instance.GetCombatAbilitySprite((obj as CombatAbility).name);
            img.gameObject.SetActive(true);
        } else if (obj is Minion) {
            Minion minion = obj as Minion;
            portrait.GeneratePortrait(minion.character);
            portrait.gameObject.SetActive(true);
        }
        UpdateTextInfo();
    }

    public void OnClick(bool selected) {
        if (selected) {
            onSelected.Invoke(obj);
        }
    }

    private void UpdateTextInfo() {
        if (obj is Summon) {
            Summon summon = obj as Summon;
            string text = $"{summon.name} ({summon.summonType.SummonName()})";
            //text += $"\nLevel: {summon.level}";
            text += $"\nDescription: {PlayerManager.Instance.player.playerSkillComponent.GetSummonDescription(summon.summonType)}";
            info.text = text;
        } 
        // else if (obj is Artifact) {
        //     Artifact artifact = obj as Artifact;
        //     string text = artifact.name;
        //     text += "\nLevel: " + artifact.level.ToString();
        //     text += "\nDescription: " + PlayerManager.Instance.player.GetArtifactDescription(artifact.type);
        //     info.text = text;
        // } 
        else if (obj is PlayerSpell) {
            PlayerSpell action = obj as PlayerSpell;
            string text = action.name;
            text += $"\nSlot Level: {action.level}";
            info.text = text;
        } else if (obj is CombatAbility) {
            CombatAbility ability = obj as CombatAbility;
            string text = ability.name;
            text += $"\nDescription: {ability.description}";
            info.text = text;
        } else if (obj is Minion) {
            Minion minion = obj as Minion;
            string text = minion.character.name;
            text += $"\n{minion.character.raceClassName}";
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
