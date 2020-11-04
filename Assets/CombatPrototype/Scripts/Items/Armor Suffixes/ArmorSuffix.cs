using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorSuffix {
    protected ARMOR_SUFFIX _armorSuffix;
    protected string _name;
    protected float _bonusPDefPercent;
    protected float _bonusMDefPercent;

    #region getters/setters
    public ARMOR_SUFFIX armorSuffix {
        get { return _armorSuffix; }
    }
    public string name {
        get { return _name; }
    }
    public float bonusPDefPercent {
        get { return _bonusPDefPercent; }
    }
    public float bonusMDefPercent {
        get { return _bonusMDefPercent; }
    }
    #endregion

    public ArmorSuffix(ARMOR_SUFFIX type) {
        _armorSuffix = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_armorSuffix.ToString());
    }
}
