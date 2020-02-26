using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellListUI : CustomDropdownList {

    [SerializeField] private Toggle spellsToggle;
    public override void Open() {
        base.Open();
        spellsToggle.isOn = true;
    }
    public override void Close() {
        spellsToggle.isOn = false;
        base.Close();
    }
}
