using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewAbilityUI : PopupMenuBase {
    [Header("General")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI obtainText;

    [Header("Minion")]
    [SerializeField] private CharacterPortrait minionPortrait;
    [SerializeField] private TextMeshProUGUI minionText;
    [SerializeField] private GameObject minionGO;

    [Header("New Ability")]
    [SerializeField] private Image abilityIcon;
    [SerializeField] private TextMeshProUGUI abilityText;

    private Minion minion;

    //string identifierToLevelUp = this identifies what to level up for the particular minion, whether it's Combat Ability, Intervention Ability, Summon, or Artifact
    //if it is a Summon or Artifact, since it is not attached to a minion, load all player summons or artifacts
    public void ShowNewAbilityUI(Minion minionToLevelUp, object ability) {
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowNewAbilityUI(minionToLevelUp, ability));
            return;
        }
        if (!GameManager.Instance.isPaused) {
            UIManager.Instance.Pause();
            UIManager.Instance.SetSpeedTogglesState(false);
        }
        UpdateMinionToLevelUp(minionToLevelUp);
        UpdateNewAbility(ability);
        base.Open();
    }

    private void UpdateMinionToLevelUp(Minion minion) {
        if(minion != null) {
            this.minion = minion;
            minionPortrait.GeneratePortrait(this.minion.character);
            string text = this.minion.character.name;
            text += $"\n{this.minion.character.raceClassName}";
            minionText.text = text;
        }
    }

    public void UpdateNewAbility(object obj) {
        obtainText.gameObject.SetActive(false);
        minionGO.SetActive(false);
        obtainText.gameObject.SetActive(false);
        if (obj is PlayerSpell) {
            titleText.text = "New Spell";
            obtainText.text = "You obtained a new Spell!";
            PlayerSpell action = obj as PlayerSpell;
            abilityIcon.sprite = PlayerManager.Instance.GetJobActionSprite(action.name);
            string text = action.name;
            //text += "\nLevel: " + action.level;
            text += $"\nDescription: {action.description}";
            abilityText.text = text;
            obtainText.gameObject.SetActive(true);
        } else if (obj is CombatAbility) {
            titleText.text = "New Combat Ability";
            CombatAbility ability = obj as CombatAbility;
            abilityIcon.sprite = PlayerManager.Instance.GetCombatAbilitySprite(ability.name);
            string text = ability.name;
            text += $"\nLevel: {ability.lvl}";
            text += $"\nDescription: {ability.description}";
            abilityText.text = text;
            minionGO.SetActive(true);
        } else if (obj is Summon) {
            titleText.text = "New Summon";
            obtainText.text = "You obtained a new Summon!";
            Summon summon = obj as Summon;
            abilityIcon.sprite = CharacterManager.Instance.GetSummonSettings(summon.summonType).summonPortrait;
            string text = $"{summon.name} ({summon.summonType.SummonName()})";
            //text += $"\nLevel: {summon.level}";
            text += $"\nDescription: {PlayerManager.Instance.player.playerSkillComponent.GetSummonDescription(summon.summonType)}";
            abilityText.text = text;
            obtainText.gameObject.SetActive(true);
        }
        // else if (obj is Artifact) {
        //     titleText.text = "New Artifact";
        //     obtainText.text = "You obtained a new Artifact!";
        //     Artifact artifact = obj as Artifact;
        //     abilityIcon.sprite = CharacterManager.Instance.GetArtifactSettings(artifact.type).artifactPortrait;
        //     string text = artifact.name;
        //     text += "\nLevel: " + artifact.level;
        //     text += "\nDescription: " + PlayerManager.Instance.player.GetArtifactDescription(artifact.type);
        //     abilityText.text = text;
        //     obtainText.gameObject.SetActive(true);
        // }
    }

    public override void Close() {
        base.Close();
        if (!PlayerUI.Instance.TryShowPendingUI()) {
            UIManager.Instance.ResumeLastProgressionSpeed(); //if no other UI was shown, unpause game
        }
    }

    public void OnClickOk() {
        Close();
    }
}
