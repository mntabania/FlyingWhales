using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps.Location_Structures;

public interface IPartyTarget {
    LocationStructure currentStructure { get; }
}
