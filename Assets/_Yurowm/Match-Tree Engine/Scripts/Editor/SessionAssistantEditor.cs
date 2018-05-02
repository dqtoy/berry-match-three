using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using System;

[CustomEditor(typeof(SessionAssistant))]
public class SessionAssistantEditor : Editor {

    private SessionAssistant main;


    AnimBool combinationsFade = new AnimBool(false);
    AnimBool powerupsFade = new AnimBool(false);
    AnimBool mixesFade = new AnimBool(false);

    public override void OnInspectorGUI() {
        main = (SessionAssistant) target;
        Undo.RecordObject(main, "");
        Color defalutColor = GUI.color;

        if (main.combinations == null)
            main.combinations = new List<SessionAssistant.Combinations>();

        main.squareCombination = EditorGUILayout.Toggle("Square Combinations", main.squareCombination);

        #region Power Ups
        powerupsFade.target = GUILayout.Toggle(powerupsFade.target, "Power Ups", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(powerupsFade.faded)) {

            if (!ContentAssistant.main)
                ContentAssistant.main = GameObject.FindObjectOfType<ContentAssistant>();

            if (!ContentAssistant.main)
                EditorGUILayout.HelpBox("ContentAssistant is missing", MessageType.Error, true);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("Name", GUILayout.Width(120));
            GUILayout.Label("Content Name", GUILayout.Width(120));
            GUILayout.Label("Color", GUILayout.Width(40));
            GUILayout.Label("LE_ID", GUILayout.Width(40));
            GUILayout.Label("LE_Name", GUILayout.Width(60));


            EditorGUILayout.EndHorizontal();

            foreach (SessionAssistant.PowerUps powerup in main.powerups) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.powerups.Remove(powerup);
                    break;
                }
                powerup.name = EditorGUILayout.TextField(powerup.name, GUILayout.Width(120));
                powerup.contentName = EditorGUILayout.TextField(powerup.contentName, GUILayout.Width(120));
                powerup.color = EditorGUILayout.Toggle(powerup.color, GUILayout.Width(40));
                powerup.levelEditorID = EditorGUILayout.IntField(powerup.levelEditorID, GUILayout.Width(40));
                if (powerup.levelEditorID > 0) {
                    powerup.levelEditorName = EditorGUILayout.TextField(powerup.levelEditorName, GUILayout.Width(60));
                    if (powerup.levelEditorName.Length > 2)
                        powerup.levelEditorName = powerup.levelEditorName.Substring(0, 2);
                }
                
                EditorGUILayout.EndHorizontal();

                GUI.color = Color.red;
                if (ContentAssistant.main) {
                    if (powerup.color) {
                        foreach (string color in Chip.chipTypes) {
                            if (!ContentAssistant.main.cItems.Exists(x => x.item.name == powerup.contentName + color))
                                EditorGUILayout.LabelField("'" + powerup.contentName + color + "' is missing", EditorStyles.boldLabel, GUILayout.Width(250));
                        }
                    } else
                        if (!ContentAssistant.main.cItems.Exists(x => x.item.name == powerup.contentName))
                            EditorGUILayout.LabelField("'" + powerup.contentName + "' is missing", EditorStyles.boldLabel, GUILayout.Width(250));

                }
                GUI.color = defalutColor;
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add", GUILayout.Width(60)))
                main.powerups.Add(new SessionAssistant.PowerUps());
          
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region Combinations
        combinationsFade.target = GUILayout.Toggle(combinationsFade.target, "Combinations", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(combinationsFade.faded)) {
        
            if (main.powerups.Count == 0)
                EditorGUILayout.HelpBox("No power up found", MessageType.Error, true);

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("Priority", GUILayout.Width(80));
            if (main.squareCombination)
                GUILayout.Label("Square", GUILayout.Width(50));
            GUILayout.Label("Vert.", GUILayout.Width(40));
            GUILayout.Label("Horiz.", GUILayout.Width(40));
            GUILayout.Label("Count", GUILayout.Width(40));
            GUILayout.Label("PowerUp", GUILayout.Width(80));
            GUILayout.Label("Tag", GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();

            foreach (SessionAssistant.Combinations combination in main.combinations) {
                if (!main.squareCombination && combination.square)
                    continue;

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.combinations.Remove(combination);
                    break;
                }
                combination.priority = EditorGUILayout.IntField(combination.priority, GUILayout.Width(80));
                if (main.squareCombination) {
                    combination.square = EditorGUILayout.Toggle(combination.square, GUILayout.Width(50));
                    GUI.enabled = !combination.square;
                }
                combination.vertical = EditorGUILayout.Toggle(combination.vertical, GUILayout.Width(40));
                combination.horizontal = EditorGUILayout.Toggle(combination.horizontal, GUILayout.Width(40));
                combination.minCount = Mathf.Clamp(EditorGUILayout.IntField(combination.minCount, GUILayout.Width(40)), 4, 9);
                GUI.enabled = true;

                if (main.powerups.Count > 0) {
                    List<string> powerups = new List<string>();
                    foreach (SessionAssistant.PowerUps powerup in main.powerups)
                        if (!powerups.Contains(powerup.name))
                            powerups.Add(powerup.name);

                    int id = powerups.IndexOf(combination.powerup);
                    if (id == -1) id = 0;
                    id = EditorGUILayout.Popup(id, powerups.ToArray(), GUILayout.Width(80));
                    combination.powerup = powerups[id];
                }

                combination.tag = EditorGUILayout.TextField(combination.tag, GUILayout.Width(60));


                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add", GUILayout.Width(60)))
                main.combinations.Add(new SessionAssistant.Combinations());
            if (GUILayout.Button("Sort", GUILayout.Width(60)))
                main.combinations.Sort((SessionAssistant.Combinations a, SessionAssistant.Combinations b) => {
                    if (a.priority < b.priority)
                        return -1;
                    if (a.priority > b.priority)
                        return 1;
                    return 0;
                });

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region Mixes
        mixesFade.target = GUILayout.Toggle(mixesFade.target, "Mixes", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(mixesFade.faded)) {

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("First type", GUILayout.Width(100));
            GUILayout.Label("Second type", GUILayout.Width(100));
            GUILayout.Label("Target type", GUILayout.Width(100));
            GUILayout.Label("Message", GUILayout.Width(100));
            
            EditorGUILayout.EndHorizontal();

            foreach (SessionAssistant.Mix mix in main.mixes) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.mixes.Remove(mix);
                    break;
                }
                mix.pair.a = EditorGUILayout.TextField(mix.pair.a, GUILayout.Width(100));
                mix.pair.b = EditorGUILayout.TextField(mix.pair.b, GUILayout.Width(100));
                GUILayout.Label(mix.pair.a, EditorStyles.boldLabel, GUILayout.Width(100));
                mix.function = EditorGUILayout.TextField(mix.function, GUILayout.Width(100));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add", GUILayout.Width(60)))
                main.mixes.Add(new SessionAssistant.Mix());

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        GUI.color = defalutColor;
    }

    public SessionAssistantEditor () {
        powerupsFade.valueChanged.AddListener(Repaint);
        combinationsFade.valueChanged.AddListener(Repaint);
        mixesFade.valueChanged.AddListener(Repaint);
    }
}
