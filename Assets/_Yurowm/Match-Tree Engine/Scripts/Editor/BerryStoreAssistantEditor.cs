using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;
using System;

[CustomEditor(typeof(BerryStoreAssistant))]
public class BerryStoreAssistantEditor : Editor {

    private BerryStoreAssistant main;


    AnimBool iapsFade = new AnimBool(false);
    AnimBool itemsFade = new AnimBool(false);

    public override void OnInspectorGUI() {
        main = (BerryStoreAssistant) target;
        Undo.RecordObject(main, "");
        Color defalutColor = GUI.color;

        if (main.items == null)
            main.items = new List<BerryStoreAssistant.ItemInfo>();

        if (main.iaps == null)
            main.iaps = new List<BerryStoreAssistant.IAP>();
        
        #region Items
        itemsFade.target = GUILayout.Toggle(itemsFade.target, "Items", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(itemsFade.faded)) {

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("Name", GUILayout.Width(80));
            GUILayout.Label("ID", GUILayout.Width(80));
            GUILayout.Label("Description", GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            foreach (BerryStoreAssistant.ItemInfo item in main.items) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.items.Remove(item);
                    break;
                }
                item.name = EditorGUILayout.TextField(item.name, GUILayout.Width(80));
                item.id = EditorGUILayout.TextField(item.id, GUILayout.Width(80));
                item.description = EditorGUILayout.TextField(item.description, GUILayout.ExpandWidth(true));
                
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add", GUILayout.Width(60)))
                main.items.Add(new BerryStoreAssistant.ItemInfo());
          
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        #region IAPs
        iapsFade.target = GUILayout.Toggle(iapsFade.target, "IAPs", EditorStyles.foldout);

        if (EditorGUILayout.BeginFadeGroup(iapsFade.faded)) {

            EditorGUILayout.BeginHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("ID", GUILayout.Width(100));
            GUILayout.Label("SKU", GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            foreach (BerryStoreAssistant.IAP iap in main.iaps) {
                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("X", GUILayout.Width(20))) {
                    main.iaps.Remove(iap);
                    break;
                }

                iap.id = EditorGUILayout.TextField(iap.id, GUILayout.Width(100));
                iap.sku = EditorGUILayout.TextField(iap.sku, GUILayout.ExpandWidth(true));

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Add", GUILayout.Width(60)))
                main.iaps.Add(new BerryStoreAssistant.IAP());

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndFadeGroup();
        #endregion

        GUI.color = defalutColor;
    }

    public BerryStoreAssistantEditor () {
        itemsFade.valueChanged.AddListener(Repaint);
        iapsFade.valueChanged.AddListener(Repaint);
    }
}
