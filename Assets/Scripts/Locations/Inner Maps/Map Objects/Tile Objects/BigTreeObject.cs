using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Locations.Settlements;
using UnityEngine.Tilemaps;

public class BigTreeObject : TreeObject {
	public override Vector2 selectableSize => new Vector2(1.7f, 1.7f);
	
	public override System.Type serializedData => typeof(SaveDataBigTreeObject);
	
	public BigTreeObject() {
        //advertisedActions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CHOP_WOOD, INTERACTION_TYPE.ASSAULT, INTERACTION_TYPE.REPAIR };
        //Initialize(TILE_OBJECT_TYPE.BIG_TREE_OBJECT, false);
        //AddAdvertisedAction(INTERACTION_TYPE.CHOP_WOOD);
        //AddAdvertisedAction(INTERACTION_TYPE.ASSAULT);
        //AddAdvertisedAction(INTERACTION_TYPE.RESOLVE_COMBAT);
        SetYield(InnerMapManager.Big_Tree_Yield);
        traitContainer.AddTrait(this, "Immovable");
    }
	public BigTreeObject(SaveDataBigTreeObject data) : base(data) { 
		SetYield(InnerMapManager.Big_Tree_Yield);
	}

	public override string ToString() {
		return $"Big Tree {id}";
	}
	protected override string GenerateName() { return "Big Tree"; }
}

#region Save Data
public class SaveDataBigTreeObject : SaveDataTreeObject { }
#endregion
