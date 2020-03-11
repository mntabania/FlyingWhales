using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoodModifier {
	string moodModificationDescription { get; }
	int moodModifier { get; }
}
