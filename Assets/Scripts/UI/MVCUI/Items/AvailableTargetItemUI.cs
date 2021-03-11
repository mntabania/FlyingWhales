using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AvailableTargetItemUI : MonoBehaviour {
    public Action<AvailableTargetItemUI> onClicked;
    public Button myButton;

    public RuinarchText txtName;

    public Image imgIcon;
    public IStoredTarget target;

    public void InitializeItem(IStoredTarget p_target) {
        target = p_target;

        txtName.text = p_target.name;
    }

    private void OnEnable() {
        myButton.onClick.AddListener(Click);
    }

    private void OnDisable() {
        myButton.onClick.RemoveListener(Click);
    }

    void Click() {
        onClicked?.Invoke(this);
    }
}