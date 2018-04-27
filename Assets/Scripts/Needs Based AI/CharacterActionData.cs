﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct CharacterActionData {
    public ACTION_TYPE actionType;
    public string actionName;
    public ActionFilterData[] filters;

    public int advertisedFullness;
    public int advertisedJoy;
    public int advertisedEnergy;
    public int advertisedPrestige;

    public int providedFullness;
    public int providedJoy;
    public int providedEnergy;
    public int providedPrestige;

    public int successRate;
    public int duration;

    public RESOURCE resourceGiven;
    public int minResourceGiven;
    public int maxResourceGiven;

    public RESOURCE resourceNeeded;
    public int resourceAmountNeeded;

    public ActionEvent successFunction;
    public ActionEvent failFunction;

    //public void SetActionName(string name) {
    //    this.actionName = name;
    //}
}

[CustomPropertyDrawer(typeof(CharacterActionData))]
public class CharacterActionDrawer : PropertyDrawer {
    private bool enableSuccessRate, enableDuration, enableResourceGiven, enableResourceNeeded;
    private SerializedProperty filtersProp;
    private float filtersHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);

        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        var actionTypeRect = new Rect(position.x, position.y, position.width, 16);
        //var actionNameRect = new Rect(position.x + 180, position.y, 70, 16);

        EditorGUI.PropertyField(actionTypeRect, property.FindPropertyRelative("actionType"), GUIContent.none);
        //EditorGUI.LabelField(new Rect(position.x + 135, position.y, 50, 16), "Name");
        //EditorGUI.PropertyField(actionNameRect, property.FindPropertyRelative("actionName"), GUIContent.none);

        float startPosY = position.y + 20;

        //float headersPosY = startPosY;
        //EditorGUI.indentLevel = -1;
        //EditorGUI.LabelField(new Rect(position.x, headersPosY, 50, 50), "Hunger");
        //EditorGUI.LabelField(new Rect(position.x + 57, headersPosY, 50, 50), "Joy");
        //EditorGUI.LabelField(new Rect(position.x + 100, headersPosY, 50, 50), "Energy");
        //EditorGUI.LabelField(new Rect(position.x + 150, headersPosY, 50, 50), "Prestige");
        //EditorGUI.indentLevel = 0;

        float advertisedPosY = startPosY;
        var advertisedLblRect = new Rect(position.x, advertisedPosY, position.width, 16);
        EditorGUI.LabelField(advertisedLblRect, "Advertised", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        var aHungerRect = new Rect(position.x, advertisedLblRect.y + 16, position.width, 16);
        var aJoyRect = new Rect(position.x, aHungerRect.y + 16, position.width, 16);
        var aEnergyRect = new Rect(position.x, aJoyRect.y + 16, position.width, 16);
        var aPrestigeRect = new Rect(position.x, aEnergyRect.y + 16, position.width, 16);
        EditorGUI.PropertyField(aHungerRect, property.FindPropertyRelative("advertisedFullness"), new GUIContent("Fullness"));
        EditorGUI.PropertyField(aJoyRect, property.FindPropertyRelative("advertisedJoy"), new GUIContent("Joy"));
        EditorGUI.PropertyField(aEnergyRect, property.FindPropertyRelative("advertisedEnergy"), new GUIContent("Energy"));
        EditorGUI.PropertyField(aPrestigeRect, property.FindPropertyRelative("advertisedPrestige"), new GUIContent("Prestige"));
        EditorGUI.indentLevel--;

        float providedPosY = aPrestigeRect.y + 20;
        var providedLblRect = new Rect(position.x, providedPosY, position.width, 16);
        EditorGUI.LabelField(providedLblRect, "Provided", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        var pHungerRect = new Rect(position.x, providedLblRect.y + 16, position.width, 16);
        var pJoyRect = new Rect(position.x, pHungerRect.y + 16, position.width, 16);
        var pEnergyRect = new Rect(position.x, pJoyRect.y + 16, position.width, 16);
        var pPrestigeRect = new Rect(position.x, pEnergyRect.y + 16, position.width, 16);
        EditorGUI.PropertyField(pHungerRect, property.FindPropertyRelative("providedFullness"), new GUIContent("Fullness"));
        EditorGUI.PropertyField(pJoyRect, property.FindPropertyRelative("providedJoy"), new GUIContent("Joy"));
        EditorGUI.PropertyField(pEnergyRect, property.FindPropertyRelative("providedEnergy"), new GUIContent("Energy"));
        EditorGUI.PropertyField(pPrestigeRect, property.FindPropertyRelative("providedPrestige"), new GUIContent("Prestige"));
        EditorGUI.indentLevel--;

        SerializedProperty successFunctionProperty = property.FindPropertyRelative("successFunction");
        SerializedProperty failFunctionProperty = property.FindPropertyRelative("failFunction");

        float successPosY = pEnergyRect.y + 40;
        var pSuccessFunctionRect = new Rect(position.x, successPosY, position.width, 100);
        EditorGUI.PropertyField(pSuccessFunctionRect, successFunctionProperty);

        float failPosY = successPosY + pSuccessFunctionRect.height;
        var pFailFunctionRect = new Rect(position.x, failPosY, position.width, 100);
        EditorGUI.PropertyField(pFailFunctionRect, failFunctionProperty);

        ACTION_TYPE actionType = (ACTION_TYPE)property.FindPropertyRelative("actionType").enumValueIndex;

        enableSuccessRate = HasSuccessRate(actionType);
        enableDuration = HasDuration(actionType);
        enableResourceGiven = GivesResource(actionType);
        enableResourceNeeded = NeedResource(actionType);

        float defaultSuccessRateYPos = failPosY + pFailFunctionRect.height;
        float defaultDurationYPos = failPosY + pFailFunctionRect.height + 16;
        float defaultResourceGivenYPos = failPosY + pFailFunctionRect.height + 32;
        float defaultResourceNeededYPos = failPosY + pFailFunctionRect.height + 48;

        float successRateYPos = defaultSuccessRateYPos;
        float durationYPos = defaultDurationYPos;
        float resourceGivenYPos = defaultResourceGivenYPos;
        float resourceNeededYPos = defaultResourceNeededYPos;

        //Success Rate
        if (enableSuccessRate) {
            LoadSuccessRateField(successRateYPos, position, property, label);
        } else {
            property.FindPropertyRelative("successRate").intValue = 0;
        }

        //Duration
        if (enableDuration) {
            if (!enableSuccessRate) {
                durationYPos = defaultSuccessRateYPos;
            }
            LoadDurationField(durationYPos, position, property, label);
        } else {
            property.FindPropertyRelative("duration").intValue = 0;
        }

        //Resource Given
        if (enableResourceGiven) {
            if (!enableDuration) {
                resourceGivenYPos = defaultDurationYPos;
                if (!enableSuccessRate) {
                    resourceGivenYPos = defaultSuccessRateYPos;
                }
            }
            LoadResourceGivenField(resourceGivenYPos, position, property, label);
        } else {
            property.FindPropertyRelative("resourceGiven").enumValueIndex = 0;
            property.FindPropertyRelative("minResourceGiven").intValue = 0;
            property.FindPropertyRelative("maxResourceGiven").intValue = 0;
        }

        //Resource Needed
        if (enableResourceNeeded) {
            if (!enableResourceGiven) {
                resourceNeededYPos = defaultResourceGivenYPos;
                if (!enableDuration) {
                    resourceNeededYPos = defaultDurationYPos;
                    if (!enableSuccessRate) {
                        resourceNeededYPos = defaultSuccessRateYPos;
                    }
                }
            }
            LoadNeedsResourceField(resourceNeededYPos, position, property, label);
        } else {
            property.FindPropertyRelative("resourceNeeded").enumValueIndex = 0;
            property.FindPropertyRelative("resourceAmountNeeded").intValue = 0;
        }

        float filtersYPos = defaultResourceNeededYPos;
        if (!enableResourceNeeded) {
            filtersYPos = defaultResourceNeededYPos;
            if (!enableResourceGiven) {
                filtersYPos = defaultResourceGivenYPos;
                if (!enableDuration) {
                    filtersYPos = defaultDurationYPos;
                    if (!enableSuccessRate) {
                        filtersYPos = defaultSuccessRateYPos;
                    }
                }
            }
        }
        filtersYPos += 16;

        //filters
        filtersProp = property.FindPropertyRelative("filters");

        filtersHeight = 16;
        if (filtersProp.isExpanded) {
            int filterCount = filtersProp.arraySize;
            filtersHeight += 8 * (filterCount + 1);
            for (int i = 0; i < filterCount; i++) {
                SerializedProperty currElement = filtersProp.GetArrayElementAtIndex(i);
                if (currElement.isExpanded) {
                    filtersHeight += 24; // add the 3 elements
                    SerializedProperty objects = currElement.FindPropertyRelative("objects");
                    if (objects.isExpanded) { //objects drawer for current element is opened
                        filtersHeight += 10 * (objects.arraySize + 1);
                    }
                }
            }
        }
        
        
        var filtersRect = new Rect(position.x, filtersYPos, position.width, filtersHeight);
        EditorGUI.PropertyField(filtersRect, filtersProp, true);

        // Set indent back to what it was
        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        float modifier = 25;
        modifier += filtersHeight / 5;
        if (enableSuccessRate) {
            modifier += 2;
        }
        if (enableDuration) {
            modifier += 2;
        }
        if (enableResourceGiven) {
            modifier += 3;
        }
        if (enableResourceNeeded) {
            modifier += 3;
        }
        return base.GetPropertyHeight(property, label) * modifier;
    }

    #region Special Fields
    private void LoadSuccessRateField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        //EditorGUI.indentLevel;
        //EditorGUI.LabelField(new Rect(position.x, yPos, 50, 50), "Success Rate");
        var successRateRect = new Rect(position.x, yPos, position.width, 16);
        EditorGUI.PropertyField(successRateRect, property.FindPropertyRelative("successRate"));
        //EditorGUI.indentLevel = 0;
    }
    private void LoadDurationField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        //EditorGUI.indentLevel = -7;
        //EditorGUI.LabelField(new Rect(position.x, yPos, 50, 50), "Duration");
        var durationRect = new Rect(position.x, yPos, position.width, 16);
        EditorGUI.PropertyField(durationRect, property.FindPropertyRelative("duration"));
        //EditorGUI.indentLevel = 0;
    }
    private void LoadResourceGivenField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        //EditorGUI.indentLevel = -7;
        var resourceGivenRect = new Rect(position.x, yPos, position.width, 16);
        var minResourceRect = new Rect(position.x, resourceGivenRect.y + 16, position.width, 16);
        var maxResourceRect = new Rect(position.x, minResourceRect.y + 16, position.width, 16);
        EditorGUI.PropertyField(resourceGivenRect, property.FindPropertyRelative("resourceGiven"));
        EditorGUI.PropertyField(minResourceRect, property.FindPropertyRelative("minResourceGiven"));
        EditorGUI.PropertyField(maxResourceRect, property.FindPropertyRelative("maxResourceGiven"));
        //EditorGUI.indentLevel = 0;
    }
    private void LoadNeedsResourceField(float yPos, Rect position, SerializedProperty property, GUIContent label) {
        //EditorGUI.indentLevel = -7;
        //EditorGUI.LabelField(new Rect(position.x, yPos, 50, 50), "Needs Resource");
        var resourceNeededRect = new Rect(position.x, yPos, position.width, 16);
        var resourceAmountNeededRect = new Rect(position.x, resourceNeededRect.y + 16, position.width, 16);
        EditorGUI.PropertyField(resourceNeededRect, property.FindPropertyRelative("resourceNeeded"));
        EditorGUI.PropertyField(resourceAmountNeededRect, property.FindPropertyRelative("resourceAmountNeeded"));
        //EditorGUI.indentLevel = 0;
    }
    #endregion

    #region Special Field Conditions
    private bool HasSuccessRate(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.HUNT:
                return true;
            case ACTION_TYPE.DESTROY:
                return true;
            default:
                return false;
        }
    }
    private bool HasDuration(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.HUNT:
                return true;
            case ACTION_TYPE.DESTROY:
                return true;
            case ACTION_TYPE.BUILD:
                return true;
            case ACTION_TYPE.REPAIR:
                return true;
            default:
                return false;
        }
    }
    private bool GivesResource(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.HARVEST:
                return true;
            default:
                return false;
        }
    }
    private bool NeedResource(ACTION_TYPE actionType) {
        switch (actionType) {
            case ACTION_TYPE.REPAIR:
                return true;
            default:
                return false;
        }
    }
    #endregion
}