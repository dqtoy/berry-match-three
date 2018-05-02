using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor(typeof(ProfileAssistant))]
public class ProfileAssistantEditor : Editor {

    ProfileAssistant main;
    AnimBool inventoryFade = new AnimBool(false);
    AnimBool scoresFade = new AnimBool(false);

    public override void OnInspectorGUI() {
        main = (ProfileAssistant) target;
        Undo.RecordObject(main, "");

        #region Local Profile
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear Data", GUILayout.Width(80))) {
            main.ClearData();
        }
        if (GUILayout.Button("Unlock All Levels", GUILayout.Width(110))) {
            main.UnlockAllLevels();
        }
        EditorGUILayout.EndHorizontal();

        if (main.local_profile == null)
            main.local_profile = UserProfileUtils.ReadProfileFromDevice();

        EditorGUILayout.LabelField("Name", main.local_profile.name.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current level", main.local_profile.current_level.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Last save", main.local_profile.lastSave.ToShortDateString() + " " + main.local_profile.lastSave.ToLongTimeString(), EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Next Live time", main.local_profile.next_live_time.ToShortDateString() + " " + main.local_profile.next_live_time.ToLongTimeString(), EditorStyles.boldLabel);

        inventoryFade.target = GUILayout.Toggle(inventoryFade.target, "Inventory", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(inventoryFade.faded)) {
            foreach (KeyValuePair<string, int> inventory in main.local_profile.inventory) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField(inventory.Key, inventory.Value.ToString(), EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndFadeGroup();

        scoresFade.target = GUILayout.Toggle(scoresFade.target, "Score", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(scoresFade.faded)) {
            foreach (KeyValuePair<int, int> score in main.local_profile.score) {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Level " + score.Key.ToString(), score.Value.ToString(), EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndFadeGroup();
        #endregion
    }
   

    public ProfileAssistantEditor() {
        scoresFade.valueChanged.AddListener(Repaint);
        inventoryFade.valueChanged.AddListener(Repaint);
    }
}
