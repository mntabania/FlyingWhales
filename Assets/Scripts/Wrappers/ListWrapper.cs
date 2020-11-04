using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ListWrapper<T> {
	public List<T> list;
}

[System.Serializable]
public class TextAssetListWrapper: ListWrapper<TextAsset>{}

[System.Serializable]
public class StringListWrapper : ListWrapper<string> { }