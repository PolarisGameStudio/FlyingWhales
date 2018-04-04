﻿using UnityEngine;
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

	internal void ChangeElement(T element, int newWeight){
		if (_dictionary.ContainsKey(element)) {
			_dictionary[element] = newWeight;
		}else{
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

    internal void AddWeightToElement(T key, int weight) {
        if (_dictionary.ContainsKey(key)) {
            _dictionary[key] += weight;
		}else{
			_dictionary.Add(key, weight);
		}
    }

    internal void SubtractWeightFromElement(T key, int weight) {
        if (_dictionary.ContainsKey(key)) {
            _dictionary[key] -= weight;
        }
    }

    /*
     * This will get a random element in the weighted
     * dictionary.
     * */
    internal T PickRandomElementGivenWeights() {
        return Utilities.PickRandomElementWithWeights(_dictionary);
    }

    internal void LogDictionaryValues(string title) {
        Debug.Log(Utilities.GetWeightsSummary(_dictionary, title));
    }

    internal int GetTotalOfWeights() {
        return Utilities.GetTotalOfWeights(_dictionary);
    }

	internal void Clear(){
		_dictionary.Clear ();
	}
}
