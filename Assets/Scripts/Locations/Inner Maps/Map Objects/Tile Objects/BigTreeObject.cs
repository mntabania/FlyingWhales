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
	
	public BigTreeObject() : base(TILE_OBJECT_TYPE.BIG_TREE_OBJECT) {
        traitContainer.AddTrait(this, "Immovable");
    }
	public BigTreeObject(SaveDataBigTreeObject data) : base(data) { }

	public override string ToString() {
		return $"Big Tree {id.ToString()}";
	}
	protected override string GenerateName() { return "Big Tree"; }
}

#region Save Data
public class SaveDataBigTreeObject : SaveDataTreeObject { }
#endregion
