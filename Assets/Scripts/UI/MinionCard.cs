using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinionCard : MonoBehaviour {
    public Minion minion { get; private set; }

    public Image portraitImg;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtClass;
    public TextMeshProUGUI txtCombatAbility;
    public TextMeshProUGUI txtActions;
    public TextMeshProUGUI txtResearch;
    public GameObject researchGO;

    //Minion Data
    public UnsummonedMinionData minionData { get; private set; }
    //public List<INTERVENTION_ABILITY> abilitiesToResearch { get; private set; }

    public void SetMinion(UnsummonedMinionData minionData) {
        this.minionData = minionData;
        string minionName = this.minionData.minionName;
        //if (minionName == string.Empty) {
        //    UnsummonedMinionData temp = this.minionData;
        //    temp.minionName = RandomNameGenerator.GenerateMinionName();
        //    this.minionData = temp;
        //}

        Sprite classPortrait = CharacterManager.Instance.GetWholeImagePortraitSprite(this.minionData.className);
        if(classPortrait != null) {
            portraitImg.sprite = classPortrait;
            portraitImg.gameObject.SetActive(true);
        } else {
            portraitImg.gameObject.SetActive(false);
        }
        txtName.text = minionName;
        txtClass.text = $"Demon {this.minionData.className}";
        // txtCombatAbility.text = "<link=\"0\">" + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(this.minionData.combatAbility.ToString()) + "</link>";

        // DeadlySin deadlySin = CharacterManager.Instance.GetDeadlySin(this.minionData.className);
        // string actions = string.Empty;
        // for (int i = 0; i < deadlySin.assignments.Length; i++) {
        //     if(i > 0) {
        //         actions += ", ";
        //     }
        //     actions += "<link=\"" + i.ToString() + "\">" + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(deadlySin.assignments[i].ToString()) + "</link>";
        // }
        // txtActions.text = actions;

        // if (this.minionData.interventionAbilitiesToResearch.Count > 0) {
        //     string research = string.Empty;
        //     for (int i = 0; i < this.minionData.interventionAbilitiesToResearch.Count; i++) {
        //         if (i > 0) {
        //             research += ", ";
        //         }
        //         research += "<link=\"" + i.ToString() + "\">" + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(this.minionData.interventionAbilitiesToResearch[i].ToString()) + "</link>";
        //     }
        //     txtResearch.text = research;
        //     researchGO.SetActive(true);
        // } else {
        //     researchGO.SetActive(false);
        // }
    }

    public void SetMinion(Minion minion) {
        this.minion = minion;
        if(minion != null) {
            //portrait.GeneratePortrait(minion.character);
            txtName.text = minion.character.name;
            txtClass.text = minion.character.raceClassName;

            // txtCombatAbility.text = "<link=\"0\">" + minion.combatAbility.name + "</link>";

            Sprite classPortrait = CharacterManager.Instance.GetWholeImagePortraitSprite(minion.character.characterClass.className);
            if (classPortrait != null) {
                portraitImg.sprite = classPortrait;
                portraitImg.gameObject.SetActive(true);
            } else {
                portraitImg.gameObject.SetActive(false);
            }

            // string actions = string.Empty;
            // for (int i = 0; i < minion.deadlySin.assignments.Length; i++) {
            //     if (i > 0) {
            //         actions += ", ";
            //     }
            //     actions += "<link=\"" + i.ToString() + "\">" + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(minion.deadlySin.assignments[i].ToString()) + "</link>";
            // }
            // txtActions.text = actions;

            // if(minion.interventionAbilitiesToResearch.Count > 0) {
            //     string research = string.Empty;
            //     for (int i = 0; i < minion.interventionAbilitiesToResearch.Count; i++) {
            //         if (i > 0) {
            //             research += ", ";
            //         }
            //         research += "<link=\"" + i.ToString() + "\">" + UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(minion.interventionAbilitiesToResearch[i].ToString()) + "</link>";
            //     }
            //     txtResearch.text = research;
            //     researchGO.SetActive(true);
            // } else {
            //     researchGO.SetActive(false);
            // }

            //tint
            portraitImg.material = minion.character.visuals.wholeImageMaterial;
        }
    }
    public void OnExitHoverInterventionAbility1() {
        UIManager.Instance.HideSmallInfo();
    }
    public void OnExitHoverInterventionAbility2() {
        UIManager.Instance.HideSmallInfo();
    }

    public void OnHoverActionAbility(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            DEADLY_SIN_ACTION action;
            if (minion != null) {
                action = minion.deadlySin.assignments[index];
            } else {
                DeadlySin deadlySin = CharacterManager.Instance.GetDeadlySin(this.minionData.className);
                action = deadlySin.assignments[index];
            }
            UIManager.Instance.ShowSmallInfo(action.Description(), UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(action.ToString()));
        }
    }
    public void OnHoverExitAbility() {
        UIManager.Instance.HideSmallInfo();
    }

    public void OnHoverResearchSpell(object obj) {
        // if (obj is string) {
        //     int index = System.Int32.Parse((string)obj);
        //     SPELL_TYPE spell;
        //     if (minion != null) {
        //         spell = minion.interventionAbilitiesToResearch[index];
        //     } else {
        //         spell = minionData.interventionAbilitiesToResearch[index];
        //     }
        //     UIManager.Instance.ShowSmallInfo(PlayerManager.Instance.allSpellsData[spell].description, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(spell.ToString()));
        // }
    }
    public void OnHoverExitSpell() {
        UIManager.Instance.HideSmallInfo();
    }


    public void OnHoverCombatAbility(object obj) {
        COMBAT_ABILITY action;
        if (minion != null) {
            action = minion.combatAbility.type;
        } else {
            action = minionData.combatAbility;
        }
        UIManager.Instance.ShowSmallInfo(action.Description(), UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(action.ToString()));
    }
    public void OnHoverExitCombatAbility() {
        UIManager.Instance.HideSmallInfo();
    }
}
