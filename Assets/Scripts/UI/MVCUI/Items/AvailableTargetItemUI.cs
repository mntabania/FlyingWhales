using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AvailableTargetItemUI : MonoBehaviour {

    public Action<AvailableTargetItemUI> onClicked;
    public Button myButton;

    public RuinarchText txtName;
    public IStoredTarget target;

    public GameObject goCover;

    public void InitializeItem(IStoredTarget p_target) {
        target = p_target;
        if (target.isTargetted) {
            ShowCover();
        } else {
            HideCover();
        }
        txtName.text = $"{p_target.iconRichText} {p_target.name}";
    }

    private void OnEnable() {
        myButton.onClick.AddListener(Click);
    }

    private void OnDisable() {
        myButton.onClick.RemoveListener(Click);
    }

    public void ShowCover() {
        myButton.interactable = false;
        goCover.SetActive(true);
    }
    public void HideCover() {
        myButton.interactable = true;
        goCover.SetActive(false);
    }

    void Click() {
        onClicked?.Invoke(this);
    }
}