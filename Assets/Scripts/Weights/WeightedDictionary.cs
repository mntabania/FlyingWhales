using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * This is the new weights system.
 * To create a new weighted dictionary, just create
 * a new instance of this class.
 * */
public class WeightedDictionary<T> {

    private Dictionary<T, int> _dictionary;

	#region getters/setters
    public Dictionary<T, int> dictionary {
        get { return _dictionary; }
    }
    public int Count{
		get { return _dictionary.Count; }
	}
	#endregion

    public WeightedDictionary() {
        _dictionary = new Dictionary<T, int>();
    }

    public WeightedDictionary(Dictionary<T, int> dictionary) {
        _dictionary = new Dictionary<T, int>();
        foreach (KeyValuePair<T, int> kvp in dictionary) {
            _dictionary.Add(kvp.Key, kvp.Value);
        }
    }

    /*
     * Add a new element of the given type.
     * If the dictionary already has an element with that key,
     * the specified weight will instead be added to that key.
     * */
    internal void AddElement(T newElement, int weight = 0) {
        if (!_dictionary.ContainsKey(newElement)) {
            weight = Mathf.Max(0, weight); //clamp to 0
            _dictionary.Add(newElement, weight);
        } else {
            AddWeightToElement(newElement, weight);
        }
    }

    internal void AddElements(Dictionary<T, int> otherDictionary) {
        foreach (KeyValuePair<T, int> kvp in otherDictionary) {
            T key = kvp.Key;
            int value = kvp.Value;
            AddElement(key, value);
        }
    }

    internal void AddElements(WeightedDictionary<T> otherDictionary) {
        AddElements(otherDictionary._dictionary);
    }

	internal void ChangeElementWeight(T element, int newWeight){
		if (_dictionary.ContainsKey(element)) {
			_dictionary[element] = newWeight;
		}else{
			_dictionary.Add(element, newWeight);
		}
	}
    internal void ReplaceElement(T element, T newElement) {
        if (_dictionary.ContainsKey(element)) {
            AddElement(newElement, _dictionary[element]);
            RemoveElement(element);
        }
    }
    /*
     * This will remove an element with a specific key
     * */
    internal void RemoveElement(T element) {
        if (_dictionary.ContainsKey(element)) {
            _dictionary.Remove(element);
        }
    }

    internal void AddWeightToElement(T key, int weight) {
        if (_dictionary.ContainsKey(key)) {
            int newWeight = _dictionary[key] + weight;
            newWeight = Mathf.Max(0, newWeight);
            _dictionary[key] = newWeight;
		}else{
            weight = Mathf.Max(0, weight);
			_dictionary.Add(key, weight);
		}
    }

    internal void SubtractWeightFromElement(T key, int weight) {
        if (_dictionary.ContainsKey(key)) {
            _dictionary[key] -= weight;
        }
    }

    public bool HasElement(T key) {
        return _dictionary.ContainsKey(key);
    }
    /*
     * This will get a random element in the weighted
     * dictionary.
     * */
    internal T PickRandomElementGivenWeights() {
        return UtilityScripts.Utilities.PickRandomElementWithWeights(_dictionary);
    }

    internal void LogDictionaryValues(string title) {
        Debug.Log(UtilityScripts.Utilities.GetWeightsSummary(_dictionary, title));
    }
    internal string GetWeightsSummary(string title) {
        return UtilityScripts.Utilities.GetWeightsSummary(_dictionary, title);
    }
    internal int GetTotalOfWeights() {
        return UtilityScripts.Utilities.GetTotalOfWeights(_dictionary);
    }

	internal void Clear(){
		_dictionary.Clear ();
	}
}

public class WeightedFloatDictionary<T> {

    private Dictionary<T, float> _dictionary;

    #region getters/setters
    public Dictionary<T, float> dictionary {
        get { return _dictionary; }
    }
    public int Count {
        get { return _dictionary.Count; }
    }
    #endregion

    public WeightedFloatDictionary() {
        _dictionary = new Dictionary<T, float>();
    }

    public WeightedFloatDictionary(Dictionary<T, float> dictionary) {
        _dictionary = new Dictionary<T, float>();
        foreach (KeyValuePair<T, float> kvp in dictionary) {
            _dictionary.Add(kvp.Key, kvp.Value);
        }
    }

    /*
     * Add a new element of the given type.
     * If the dictionary already has an element with that key,
     * the specified weight will instead be added to that key.
     * */
    internal void AddElement(T newElement, float weight = 0f) {
        if (!_dictionary.ContainsKey(newElement)) {
            _dictionary.Add(newElement, weight);
        } else {
            AddWeightToElement(newElement, weight);
        }
    }

    internal void AddElements(Dictionary<T, float> otherDictionary) {
        foreach (KeyValuePair<T, float> kvp in otherDictionary) {
            T key = kvp.Key;
            float value = kvp.Value;
            AddElement(key, value);
        }
    }

    internal void AddElements(WeightedFloatDictionary<T> otherDictionary) {
        AddElements(otherDictionary._dictionary);
    }

    internal void ChangeElement(T element, float newWeight) {
        if (_dictionary.ContainsKey(element)) {
            _dictionary[element] = newWeight;
        } else {
            _dictionary.Add(element, newWeight);
        }
    }

    /*
     * This will remove an element with a specific key
     * */
    internal void RemoveElement(T element) {
        if (_dictionary.ContainsKey(element)) {
            _dictionary.Remove(element);
        }
    }

    internal void AddWeightToElement(T key, float weight) {
        if (_dictionary.ContainsKey(key)) {
            _dictionary[key] += weight;
        } else {
            _dictionary.Add(key, weight);
        }
    }

    internal void SubtractWeightFromElement(T key, float weight) {
        if (_dictionary.ContainsKey(key)) {
            _dictionary[key] -= weight;
        }
    }

    /*
     * This will get a random element in the weighted
     * dictionary.
     * */
    internal T PickRandomElementGivenWeights() {
        return UtilityScripts.Utilities.PickRandomElementWithWeights(_dictionary);
    }

    internal void LogDictionaryValues(string title) {
        Debug.Log(UtilityScripts.Utilities.GetWeightsSummary(_dictionary, title));
    }
    internal string GetWeightsSummary(string title) {
        return UtilityScripts.Utilities.GetWeightsSummary(_dictionary, title);
    }
    internal float GetTotalOfWeights() {
        return UtilityScripts.Utilities.GetTotalOfWeights(_dictionary);
    }

    internal void Clear() {
        _dictionary.Clear();
    }
}
