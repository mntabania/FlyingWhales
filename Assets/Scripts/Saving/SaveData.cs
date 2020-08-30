using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData<T> {

    #region Saving
    public virtual void Save() { }
    public virtual void Save(T data) { }
    #endregion

    #region Loading
    public virtual T Load() { return default; }
    #endregion
}