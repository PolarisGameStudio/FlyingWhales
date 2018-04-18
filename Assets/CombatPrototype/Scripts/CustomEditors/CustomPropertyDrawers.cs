﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;

namespace ECS {
    [CustomPropertyDrawer(typeof(SkillRequirement))]
    public class SkillRequirementDrawer : PropertyDrawer {

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate rects
            var requirementTypeRect = new Rect(position.x, position.y, 50, position.height);
			var requirementAttributeRect = new Rect(position.x + 55, position.y, 150, position.height);

//            var requirementEquipmentRect = new Rect(position.x + 55, position.y, 150, position.height);
//            var requirementAttributeRect = new Rect(position.x + 205, position.y, 150, position.height);
            //var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
            //var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);

            
            // Draw fields - passs GUIContent.none to each so they are drawn without labels
            EditorGUI.PropertyField(requirementTypeRect, property.FindPropertyRelative("itemQuantity"), GUIContent.none);
//            EditorGUI.PropertyField(requirementEquipmentRect, property.FindPropertyRelative("equipmentType"), GUIContent.none);
            EditorGUI.PropertyField(requirementAttributeRect, property.FindPropertyRelative("attributeRequired"), GUIContent.none);
            //EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("unit"), GUIContent.none);
            //EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }

    [CustomPropertyDrawer(typeof(Skill))]
    public class SkillDrawer : PropertyDrawer {

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);     

            EditorGUI.EndProperty();
        }
    }
}
#endif