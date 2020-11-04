using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPrefix {
    protected WEAPON_PREFIX _weaponPrefix;
    protected string _name;
    protected float _flatModifier;
    protected float _percentModifier;

    #region getters/setters
    public WEAPON_PREFIX weaponPrefix {
        get { return _weaponPrefix; }
    }
    public string name {
        get { return _name; }
    }
    public float flatModifier {
        get { return _flatModifier; }
    }
    public float percentModifier {
        get { return _percentModifier; }
    }
    #endregion

    public WeaponPrefix(WEAPON_PREFIX type) {
        _weaponPrefix = type;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters(_weaponPrefix.ToString());
    }
}
