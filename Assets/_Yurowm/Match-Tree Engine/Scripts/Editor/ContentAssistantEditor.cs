using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;

[CustomEditor (typeof (ContentAssistant))]
public class ContentAssistantEditor : Editor {

	private ContentAssistant main;
	private GameObject obj;
	private string category;
	private List<string> categories = new List<string>();
	private Dictionary<string, bool> visible = new Dictionary<string, bool>();

	public override void OnInspectorGUI () {
        main = (ContentAssistant) target;
        Undo.RecordObject(main, "");
		if (main.cItems == null) main.cItems = new List<ContentAssistant.ContentAssistantItem> ();

		obj = (GameObject) EditorGUILayout.ObjectField("Item", obj, typeof (GameObject), false);
		category = EditorGUILayout.TextField ("Category", category);
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button ("Add", GUILayout.Width (50))) {
			if (obj == null || category == "") return;
			ContentAssistant.ContentAssistantItem item;
			item.category = category;
			item.item = obj;
			main.cItems.Add(item);
			if (!visible.ContainsKey(category))
				visible.Add(category, true);
			else
				visible[category] = true;
			obj = null;
			category = "";
			return;
		}
		if (GUILayout.Button ("Clear", GUILayout.Width (50))) {
			obj = null;
			category = "";
		}
		if (GUILayout.Button ("Sort", GUILayout.Width (50))) {
			main.cItems.Sort(delegate(ContentAssistant.ContentAssistantItem x, ContentAssistant.ContentAssistantItem y) {
				return x.item.name.CompareTo(y.item.name);
			});
			categories.Sort (delegate(string x, string y) {
				return x.CompareTo(y);
			});
		}
		if (GUILayout.Button ("Show all", GUILayout.Width (60))) {
			foreach (string c in categories)
				visible[c] = true;
		}
		if (GUILayout.Button ("Hide all", GUILayout.Width (60))) {
			foreach (string c1 in categories)
				visible[c1] = false;
		}
		EditorGUILayout.EndHorizontal();
		categories.Clear ();
		foreach (ContentAssistant.ContentAssistantItem i in main.cItems)
			if (!categories.Contains (i.category)) {
				categories.Add (i.category);
				if (!visible.ContainsKey(i.category))
					visible.Add(i.category, false);
			}
		foreach (string s in categories) {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(visible[s] ? "-" : "+", GUILayout.Width(20)))
				visible[s] = !visible[s];
			GUILayout.Label(s, EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();
			if (!visible[s]) continue;
			foreach (ContentAssistant.ContentAssistantItem j in main.cItems) {
				if (j.category == s) {
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(20);
					if (GUILayout.Button ("X", GUILayout.Width (20))) {
						obj = j.item;
						category = j.category;
						main.cItems.Remove(j);
						return;
					}
					EditorGUILayout.ObjectField(j.item, typeof (GameObject), false, GUILayout.Width(250));
					EditorGUILayout.EndHorizontal();	
				}
			}
		}
	}
}
