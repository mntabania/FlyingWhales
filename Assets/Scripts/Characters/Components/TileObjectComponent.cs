using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class TileObjectComponent {
    public Character owner { get; private set; }
    public Bed primaryBed { get; private set; }

    public TileObjectComponent(Character owner) {
        this.owner = owner;
    }

    #region General
    public void SetPrimaryBed(Bed bed) {
        primaryBed = bed;
    }
    #endregion
}
