using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorPrefix {
    protected ARMOR_PREFIX _armorPrefix;
    protected string _name;
    protected float _bonusPDefPercent;
    protected float _bonusMDefPercent;

    #region getters/setters
    public ARMOR_PREFIX armorPrefix {
        get { return _armorPrefix; }
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

    public ArmorPrefix(ARMOR_PREFIX type) {
        _armorPrefix = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_armorPrefix.ToString());
    }
}
