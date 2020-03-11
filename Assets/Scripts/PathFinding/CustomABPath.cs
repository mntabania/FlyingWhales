using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System.Linq;

public class CustomABPath : ABPath {
    public STRUCTURE_TYPE[] onlyAllowedStructures { get; private set; }
    public STRUCTURE_TYPE[] notAllowedStructures { get; private set; }
    public Region region { get; private set; }

    public new static CustomABPath Construct(Vector3 start, Vector3 end, OnPathDelegate callback = null) {
        var p = PathPool.GetPath<CustomABPath>();

        p.Setup(start, end, callback);
        return p;
    }

    public void SetRegion(Region region) {
        this.region = region;
    }
    public void SetNotAllowedStructures(STRUCTURE_TYPE[] notAllowedStructures) {
        if(notAllowedStructures != null) {
            this.notAllowedStructures = new STRUCTURE_TYPE[notAllowedStructures.Length];
            for (int i = 0; i < notAllowedStructures.Length; i++) {
                this.notAllowedStructures[i] = notAllowedStructures[i];
            }
        } else {
            this.notAllowedStructures = null;
        }
    }
    public void SetOnlyAllowedStructures(STRUCTURE_TYPE[] onlyAllowedStructures) {
        if (onlyAllowedStructures != null) {
            this.onlyAllowedStructures = new STRUCTURE_TYPE[onlyAllowedStructures.Length];
            for (int i = 0; i < onlyAllowedStructures.Length; i++) {
                this.onlyAllowedStructures[i] = onlyAllowedStructures[i];
            }
        } else {
            this.onlyAllowedStructures = null;
        }
    }
}
