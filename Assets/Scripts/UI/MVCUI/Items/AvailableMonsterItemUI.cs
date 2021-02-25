using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvailableMonsterItemUI : MonoBehaviour
{
    public Button myButton;

    public RuinarchText txtName;
    public RuinarchText txtHP;
    public RuinarchText txtAtk;
    public RuinarchText txtAtkSpd;
    public RuinarchText txtManaCost;
    public RuinarchText txtChargeCount;

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