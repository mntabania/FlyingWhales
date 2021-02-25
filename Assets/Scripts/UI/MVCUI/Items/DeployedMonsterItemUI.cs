using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeployedMonsterItemUI : MonoBehaviour {
    public Button myButton;
    public Button unlockButton;

    public RuinarchText txtName;
    public RuinarchText txtHP;
    public RuinarchText txtAtk;
    public RuinarchText txtAtkSpd;

    public RuinarchText txtUnlockPrice;
    public GameObject disabler;

    public void InitializeItem() {

    }

    public void EnableButton() {
        disabler.SetActive(false);
    }

    public void DisableButton() {
        disabler.SetActive(true);
    }
}