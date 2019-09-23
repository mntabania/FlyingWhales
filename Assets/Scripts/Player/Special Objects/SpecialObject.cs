﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObject : IWorldObject {

    public int id { get; private set; }
    public string name { get; private set; }
    public SPECIAL_OBJECT_TYPE specialObjType { get; private set; }

    #region getters/setters
    public string worldObjectName {
        get { return name; }
    }
    public WORLD_OBJECT_TYPE worldObjectType {
        get { return WORLD_OBJECT_TYPE.SPECIAL_OBJECT; }
    }
    #endregion

    public SpecialObject(SPECIAL_OBJECT_TYPE specialObjType) {
        id = Utilities.SetID(this);
        this.specialObjType = specialObjType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(specialObjType.ToString());
        TokenManager.Instance.AddSpecialObject(this);
    }

    #region Virtuals
    public virtual void Obtain() { }
    #endregion
}

public class SaveDataSpecialObject {
    public SPECIAL_OBJECT_TYPE specialObjType;

    public virtual void Save(SpecialObject specialObject) {
        specialObjType = specialObject.specialObjType;
    }

    public virtual SpecialObject Load() {
        return new SpecialObject(specialObjType);
    }
}
