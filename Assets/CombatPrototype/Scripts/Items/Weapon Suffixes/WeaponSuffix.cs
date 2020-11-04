using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSuffix {
    protected WEAPON_SUFFIX _weaponSuffix;
    protected string _name;

    #region getters/setters
    public WEAPON_SUFFIX weaponSuffix {
        get { return _weaponSuffix; }
    }
    public string name {
        get { return _name; }
    }
    #endregion

    public WeaponSuffix(WEAPON_SUFFIX type) {
        _weaponSuffix = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_weaponSuffix.ToString());
    }
}
