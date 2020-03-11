using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseRelatable {
 
    public abstract int id { get; }
    public abstract string relatableName { get; }
    public abstract GENDER gender { get; }
}
