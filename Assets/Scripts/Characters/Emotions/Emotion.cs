using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Emotion {
	public string name { get; private set; }
    public EMOTION emotionType { get; private set; }
    public string[] responses { get; protected set; }
    public string[] mutuallyExclusive { get; protected set; }

    public Emotion(EMOTION emotionType) {
        this.emotionType = emotionType;
        name = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(emotionType.ToString());
        responses = new string[] { name };
    }
    public virtual string ProcessEmotion(Character actor, IPointOfInterest target, REACTION_STATUS status) {
        return responses[UnityEngine.Random.Range(0, responses.Length)];
    }
    public bool IsEmotionCompatibleWithThis(string emotionName) {
        return mutuallyExclusive == null || !mutuallyExclusive.Contains(emotionName);
    }
}
