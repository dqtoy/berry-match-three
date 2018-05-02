using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using System;

[CustomEditor(typeof(UIAssistant))]
public class UIAssistantEditor : Editor {

    UIAssistant main;
    UIAssistant.Page edit = null;


    AnimBool pagesFade = new AnimBool(false);
    AnimBool modulesFade = new AnimBool(false);

    public override void OnInspectorGUI() {
        main = (UIAssistant) target;
        Undo.RecordObject(main, "");
        Color defColor = GUI.color;
        
        if (main.UImodules == null)
            main.UImodules = new List<Transform>();

        if (main.pages == null)
            main.pages = new List<UIAssistant.Page>();
        
        #region UI Modules
        modulesFade.target = GUILayout.Toggle(modulesFade.target, "UI Modules: " + main.UImodules.Count.ToString() + " item(s)", EditorStyles.foldout);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(20);
        if (EditorGUILayout.BeginFadeGroup(modulesFade.faded)) {
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < main.UImodules.Count; i++) {
                EditorGUILayout.BeginHorizontal();
                main.UImodules[i] = (Transform) EditorGUILayout.ObjectField(main.UImodules[i], typeof(Transform), true, GUILayout.Width(200));
                if (main.UImodules[i] == null) {
                    main.UImodules.RemoveAt(i);
                    break;
                } else {
                    EditorGUILayout.LabelField(main.UImodules[i].GetComponentsInChildren<CPanel>(true).Length.ToString() + " panel(s)", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
                }
                EditorGUILayout.EndHorizontal();
            }
            Transform new_module = (Transform) EditorGUILayout.ObjectField(null, typeof(Transform), true, GUILayout.Width(150));
            if (new_module)
                main.UImodules.Add(new_module);
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndHorizontal();
        #endregion

        #region Pages

        pagesFade.target = GUILayout.Toggle(pagesFade.target, "Pages", EditorStyles.foldout);
        if (EditorGUILayout.BeginFadeGroup(pagesFade.faded)) {
            main.ArraysConvertation();
            
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(75);
            GUILayout.Label("Name", GUILayout.Width(200));

            EditorGUILayout.EndHorizontal();

            foreach (UIAssistant.Page page in main.pages) {

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(10);
                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.pages.Remove(page);
                    break;
                }
                if (GUILayout.Button("Edit", GUILayout.Width(35))) {
                    if (edit == page)
                        edit = null;
                    else
                        edit = page;
                }
                page.name = EditorGUILayout.TextField(page.name, GUILayout.Width(200));

                UIAssistant.Page default_page = main.pages.Find(x => x.default_page);

                if (default_page == null) {
                    default_page = page;
                    page.default_page = true;
                }

                if (page.default_page && default_page != page)
                    page.default_page = false;


                if (page.default_page) 
                    GUILayout.Label("DEFAULT", GUILayout.Width(80));
                else
                    if (GUILayout.Button("Make default", EditorStyles.miniButton, GUILayout.Width(80))) {
                        default_page.default_page = false;
                        default_page = page;
                        page.default_page = true;
                    }
                
                EditorGUILayout.EndHorizontal();

                if (edit == page) {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    EditorGUILayout.BeginVertical();

                    if (!AudioAssistant.main)
                        AudioAssistant.main = FindObjectOfType<AudioAssistant>();

                    if (!AudioAssistant.main)
                        EditorGUILayout.HelpBox("AudioAssistant is missing", MessageType.Error, true);
                    else if (AudioAssistant.main.tracks.Count > 0) {
                        List<string> tracks = new List<string>();
                        tracks.Add("-");
                        tracks.Add("None");
                        tracks.AddRange(AudioAssistant.main.tracks.Select(x => x.name).ToList());
                        int selected = -1;
                        selected = tracks.FindIndex(x => x == page.soundtrack);
                        if (selected == -1)
                            selected = 0;
                        selected = EditorGUILayout.Popup("Soundtrack", selected, tracks.ToArray(), GUILayout.Width(320));
                        page.soundtrack = tracks[selected];
                    }

                    EditorGUILayout.BeginHorizontal();
                    page.setTimeScale = EditorGUILayout.Toggle(page.setTimeScale, GUILayout.Width(20));
                    GUILayout.Label("Time Scale", GUILayout.Width(100));
                    if (page.setTimeScale)
                        page.timeScale = EditorGUILayout.Slider(page.timeScale, 0, 1, GUILayout.Width(200));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Name", EditorStyles.boldLabel, GUILayout.Width(150));
                    GUILayout.Label("Show", EditorStyles.boldLabel, GUILayout.Width(60));
                    GUILayout.Label("Ignor", EditorStyles.boldLabel, GUILayout.Width(60));
                    GUILayout.Label("Hide", EditorStyles.boldLabel, GUILayout.Width(60));
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginVertical();
                    Dictionary<CPanel, int> mask = new Dictionary<CPanel, int>();
                    foreach (CPanel panel in main.panels) {
                        mask.Add(panel, -1);
                        if (page.panels.Contains(panel))
                            mask[panel] = 1;
                        else if (page.ignoring_panels.Contains(panel))
                            mask[panel] = 0;
                    }

                    foreach (CPanel panel in main.panels) {
                        EditorGUILayout.BeginHorizontal();
                        switch (mask[panel]) {
                            case -1: GUI.color = Color.red; break;
                            case 0: GUI.color = Color.yellow; break;
                            case 1: GUI.color = Color.green; break;
                        }
                        EditorGUILayout.LabelField(panel.name, GUILayout.Width(150));
                        GUI.color = defColor;

                        if (EditorGUILayout.Toggle(mask[panel] == 1, GUILayout.Width(60)))
                            mask[panel] = 1;
                        if (EditorGUILayout.Toggle(mask[panel] == 0, GUILayout.Width(60)))
                            mask[panel] = 0;
                        if (EditorGUILayout.Toggle(mask[panel] == -1, GUILayout.Width(60)))
                            mask[panel] = -1;
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();

                    page.panels.Clear();
                    page.ignoring_panels.Clear();
                    foreach (KeyValuePair<CPanel, int> pair in mask) {
                        if (pair.Value == 1)
                            page.panels.Add(pair.Key);
                        else if (pair.Value == 0)
                            page.ignoring_panels.Add(pair.Key);
                    }

                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                }

            }

            if (GUILayout.Button("Add", GUILayout.Width(60)))
                main.pages.Add(new UIAssistant.Page());


        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        GUI.color = defColor;
    }

    public UIAssistantEditor() {
        modulesFade.valueChanged.AddListener(Repaint);
        pagesFade.valueChanged.AddListener(Repaint);
    }
}
