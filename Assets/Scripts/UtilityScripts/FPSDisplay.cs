﻿using UnityEngine;
using System.Collections;
using Steamworks;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class FPSDisplay : MonoBehaviour {
    float deltaTime = 0.0f;

    void Update() {
        if (GameManager.Instance.displayFPS) {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        }
    }

    void OnGUI() {
        if (GameManager.Instance.displayFPS) {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            //style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            style.normal.textColor = Color.white;

            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
      
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            //text += "\nAgents: " + PathfindingManager.Instance.allAgents.Count.ToString();
            //text += "\nCharacters: " + CharacterManager.Instance.allCharacters.Count.ToString();
            if (SteamManager.Initialized) {
                string name = SteamFriends.GetPersonaName();
                text += "\nSteam Name: " + name;
                text += "\nCharacters Snatched: " + AchievementManager.Instance.charactersSnatched;
            }
            GUI.Label(rect, text, style);
        }
    }
}