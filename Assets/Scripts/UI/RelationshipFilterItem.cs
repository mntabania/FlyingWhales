using System;
using System.Collections;
using System.Collections.Generic;
using Ruinarch.Custom_UI;
using UnityEngine;

public class RelationshipFilterItem : MonoBehaviour {
    [SerializeField] private RELATIONS_FILTER _filterType;
    [SerializeField] private RuinarchToggle toggle;
    private Action<bool, RELATIONS_FILTER> _toggleAction;
    
    public void Initialize(System.Action<bool, RELATIONS_FILTER> toggleAction) {
        _toggleAction = toggleAction;
    }
    public void SetIsOnWithoutNotify(bool isOn) {
        toggle.SetIsOnWithoutNotify(isOn);
    }
    public void OnToggle(bool isOn) {
        _toggleAction.Invoke(isOn, _filterType);    
    }

}
