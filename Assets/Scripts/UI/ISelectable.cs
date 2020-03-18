using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelectable {
     /// <summary>
     /// Position of the selectable in the world
     /// </summary>
     Vector3 worldPosition { get; }
     /// <summary>
     /// size of selectable (width, height), relative to 64px -> 1,1 size = 64x64. 2,1 size = 128x64
     /// </summary>
     Vector2 selectableSize { get; }

     bool IsCurrentlySelected();
     void LeftSelectAction();
     void RightSelectAction();
     bool CanBeSelected();
}
