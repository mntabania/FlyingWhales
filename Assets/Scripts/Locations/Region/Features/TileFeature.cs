using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileFeature  {

	public string name { get; protected set; }
    public string description { get; protected set; }

    #region Virtuals
    public virtual void OnAddFeature(HexTile tile) { }
    public virtual void OnRemoveFeature(HexTile tile) { }
    public virtual void OnDemolishLandmark(HexTile tile, LANDMARK_TYPE demolishedLandmarkType) { }
    public virtual void GameStartActions(HexTile tile) { }
    #endregion

}
