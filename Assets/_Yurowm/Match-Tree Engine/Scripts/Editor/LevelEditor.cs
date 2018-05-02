using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Collections;
using System.Text;
using System.Collections.Generic;

[CustomEditor (typeof (Level))]
public class LevelButtonEditor : Editor {

	LevelProfile profile;
	Level level;

	Rect rect;

    SerializedProperty serializedProperty;

	enum EditMode {Slot, Chip, PowerUp, Jelly, Block, Generator, Wall};
	EditMode currentMode = EditMode.Slot;
	AnimBool parametersFade = new AnimBool(true);
    AnimBool colorModeFade = new AnimBool(false);
    AnimBool sugarDropFade = new AnimBool(false);
    string toolID = "";
	Vector2 teleportID = -Vector2.right;

	static int cellSize = 40;

	static Color defaultColor;
	static Color transparentColor = new Color (0, 0, 0, 0f);
	static Color unpressedColor = new Color (0.7f, 0.7f, 0.7f, 1);
	static Color[] chipColor = {new Color(1,0.6f,0.6f,1),
		new Color(0.6f,1,0.6f,1),
		new Color(0.6f,0.8f,1,1),
		new Color(1,1,0.6f,1),
		new Color(1,0.6f,1,1),
		new Color(1,0.8f,0.6f,1)};
	static Color buttonColor = new Color(0.5f,0.5f,0.5f,1);
	static string[] alphabet = {"A", "B", "C", "D", "E", "F"};
    static string[] gravityLabel = { "v", "<", "^", ">" };
	
	static int slotOffect = 4;

	static GUIStyle mSlotStyle;
	public static GUIStyle slotStyle {
		get {
			if (mSlotStyle == null) {
				mSlotStyle = new GUIStyle (GUI.skin.button);
				mSlotStyle.wordWrap = true;

				mSlotStyle.normal.background = Texture2D.whiteTexture;
				mSlotStyle.focused.background = mSlotStyle.normal.background;
				mSlotStyle.active.background = mSlotStyle.normal.background;

				mSlotStyle.normal.textColor = Color.black;
				mSlotStyle.focused.textColor = mSlotStyle.normal.textColor;
				mSlotStyle.active.textColor = mSlotStyle.normal.textColor;

				mSlotStyle.fontSize = 8;
				mSlotStyle.margin = new RectOffset ();
				mSlotStyle.padding = new RectOffset ();
			}
			return mSlotStyle;
		}
	}

	static GUIStyle mIconStyle;
	static GUIStyle iconStyle {
		get {
			if (mIconStyle == null) {
				mIconStyle = new GUIStyle (GUI.skin.button);
				mIconStyle.wordWrap = true;
				
				mIconStyle.normal.background = Texture2D.whiteTexture;
				
				mIconStyle.normal.textColor = Color.white;

				mIconStyle.fontSize = 8;

				mIconStyle.border = new RectOffset ();
				mIconStyle.margin = new RectOffset ();
				mIconStyle.padding = new RectOffset ();
			}
			return mIconStyle;
		}
	}

	static Dictionary<int, string> powerupLabel = new Dictionary<int,string>();

	public LevelButtonEditor () {
        colorModeFade.valueChanged.AddListener(Repaint);
        sugarDropFade.valueChanged.AddListener(Repaint);
        parametersFade.valueChanged.AddListener (Repaint);
		}

	override public bool UseDefaultMargins() {
		return false;
	}

	public override void OnInspectorGUI () {
        level = (Level) target;
        Undo.RecordObject(level, "");
		profile = level.profile;

        if (profile == null)
            profile = new LevelProfile();

        if (SessionAssistant.main == null)
            SessionAssistant.main = GameObject.FindObjectOfType<SessionAssistant>();

		if (profile.levelID == 0 || profile.levelID != target.GetInstanceID ()) {
			if (profile.levelID != target.GetInstanceID ())
				profile = profile.GetClone();
			profile.levelID = target.GetInstanceID ();
		}
     
        profile.level = level.transform.GetSiblingIndex() + 1;

		level.name = "Level:" + profile.level.ToString() + "," + profile.target + "," + profile.limitation;

		parametersFade.target = GUILayout.Toggle(parametersFade.target, "Level Parameters", EditorStyles.foldout);

		if (EditorGUILayout.BeginFadeGroup (parametersFade.faded)) {

			profile.width = Mathf.RoundToInt (EditorGUILayout.Slider ("Width", 1f * profile.width, 5f, 12f));
			profile.height = Mathf.RoundToInt (EditorGUILayout.Slider ("Height", 1f * profile.height, 5f, 12f));
			profile.chipCount = Mathf.RoundToInt (EditorGUILayout.Slider ("Count of Possible Colors", 1f * profile.chipCount, 3f, 6f));
			profile.buttonPortion = Mathf.Round(EditorGUILayout.Slider (
                "Stone Portion", profile.buttonPortion, 0f, 0.7f) * 100) / 100;

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.LabelField ("Score Stars", GUILayout.ExpandWidth(true));
			profile.firstStarScore = Mathf.Max(EditorGUILayout.IntField (profile.firstStarScore, GUILayout.ExpandWidth(true)), 1);
			profile.secondStarScore = Mathf.Max(EditorGUILayout.IntField (profile.secondStarScore, GUILayout.ExpandWidth(true)), profile.firstStarScore+1);
			profile.thirdStarScore = Mathf.Max(EditorGUILayout.IntField (profile.thirdStarScore, GUILayout.ExpandWidth(true)), profile.secondStarScore+1);
			EditorGUILayout.EndHorizontal ();
						
			profile.limitation = (Limitation) EditorGUILayout.EnumPopup ("Limitation", profile.limitation);
			switch (profile.limitation) {
				case Limitation.Moves:
					profile.moveCount = Mathf.Clamp(EditorGUILayout.IntField("Move Count", profile.moveCount), 5, 50);
					break;
				case Limitation.Time:
					profile.duration = Mathf.Max(0, EditorGUILayout.IntField("Session duration", profile.duration));
					break;
			}
			
			profile.target = (FieldTarget) EditorGUILayout.EnumPopup ("Target", profile.target);
			
			colorModeFade.target = profile.target == FieldTarget.Color;

			if (EditorGUILayout.BeginFadeGroup (colorModeFade.faded)) {
				defaultColor = GUI.color;
				profile.targetColorCount = Mathf.RoundToInt(EditorGUILayout.Slider("Targets Count", profile.targetColorCount, 1, profile.chipCount));
				for (int i = 0; i < 6; i++) {
					GUI.color = chipColor[i];
					if (i < profile.targetColorCount)
						profile.SetTargetCount(i, Mathf.Clamp(EditorGUILayout.IntField("Color " + alphabet[i].ToString(), profile.GetTargetCount(i)), 1, 999));
					else 
						profile.SetTargetCount(i, 0);
					}
				GUI.color = defaultColor;
				}
			EditorGUILayout.EndFadeGroup ();

            sugarDropFade.target = profile.target == FieldTarget.SugarDrop;

            if (EditorGUILayout.BeginFadeGroup(sugarDropFade.faded)) {
                profile.targetSugarDropsCount = Mathf.RoundToInt(EditorGUILayout.Slider("Sugar Count", profile.targetSugarDropsCount, 1, 20));
            }
            EditorGUILayout.EndFadeGroup();
        }

		EditorGUILayout.EndFadeGroup ();


        EditorGUILayout.Space ();
		EditorGUILayout.BeginHorizontal (EditorStyles.toolbar, GUILayout.ExpandWidth(true));
		
		defaultColor = GUI.color;
		GUI.color = currentMode == EditMode.Slot ? unpressedColor : defaultColor;
		if (GUILayout.Button("Slot", EditorStyles.toolbarButton, GUILayout.Width(40)))
			currentMode = EditMode.Slot;
		GUI.color = currentMode == EditMode.Chip ? unpressedColor : defaultColor;
		if (GUILayout.Button("Chip", EditorStyles.toolbarButton, GUILayout.Width(40)))
			currentMode = EditMode.Chip;
		GUI.color = currentMode == EditMode.PowerUp ? unpressedColor : defaultColor;
		if (GUILayout.Button("PowerUp", EditorStyles.toolbarButton, GUILayout.Width(70)))
			currentMode = EditMode.PowerUp;
        if (profile.target == FieldTarget.Jelly) {
		    GUI.color = currentMode == EditMode.Jelly ? unpressedColor : defaultColor;
		    if (GUILayout.Button("Jelly", EditorStyles.toolbarButton, GUILayout.Width(50)))
			    currentMode = EditMode.Jelly;
        }
		GUI.color = currentMode == EditMode.Block ? unpressedColor : defaultColor;
		if (GUILayout.Button("Block", EditorStyles.toolbarButton, GUILayout.Width(50)))
			currentMode = EditMode.Block;
		GUI.color = currentMode == EditMode.Wall ? unpressedColor : defaultColor;
		if (GUILayout.Button("Wall", EditorStyles.toolbarButton, GUILayout.Width(40)))
			currentMode = EditMode.Wall;
		GUI.color = defaultColor;
		
		GUILayout.FlexibleSpace ();

        if (GUILayout.Button("Reset", EditorStyles.toolbarButton, GUILayout.Width(40)))
			profile = new LevelProfile ();
		
		EditorGUILayout.EndVertical ();

		// Slot modes
		if (currentMode == EditMode.Slot) {
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			
			defaultColor = GUI.color;
			
			GUI.color = toolID == "Slots" ? unpressedColor : defaultColor;
			if (GUILayout.Button("Slots", EditorStyles.toolbarButton, GUILayout.Width(40)))
				toolID = "Slots";
			
			GUI.color = toolID == "Generators" ? unpressedColor : defaultColor;
			if (GUILayout.Button("Generators", EditorStyles.toolbarButton, GUILayout.Width(70)))
				toolID = "Generators";

			GUI.color = toolID == "Teleports" ? unpressedColor : defaultColor;
			if (GUILayout.Button("Teleports", EditorStyles.toolbarButton, GUILayout.Width(70)))
				toolID = "Teleports";

            if (profile.target == FieldTarget.SugarDrop) {
                GUI.color = toolID == "Sugar Drop" ? unpressedColor : defaultColor;
                if (GUILayout.Button("Sugar Drop", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    toolID = "Sugar Drop";
            }
            GUI.color = toolID == "Gravity" ? unpressedColor : defaultColor;
            if (GUILayout.Button("Gravity", EditorStyles.toolbarButton, GUILayout.Width(50)))
                toolID = "Gravity";

            GUI.color = defaultColor;		
			GUILayout.FlexibleSpace ();
			
			EditorGUILayout.EndHorizontal ();
		}

        // Slot modes
        if (currentMode == EditMode.PowerUp) {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

            defaultColor = GUI.color;


            if (SessionAssistant.main) {
                foreach (SessionAssistant.PowerUps powerup in SessionAssistant.main.powerups) {
                    if (powerup.levelEditorID > 0) {
                    GUI.color = toolID == powerup.levelEditorName ? unpressedColor : defaultColor;
                    if (GUILayout.Button(powerup.levelEditorName, EditorStyles.toolbarButton, GUILayout.Width(30)))
                        toolID = powerup.levelEditorName;
                    }
                }
            }

            GUI.color = defaultColor;
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

		// Chip modes
		if (currentMode == EditMode.Chip) {
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			
			string  key;
			defaultColor = GUI.color;

			GUI.color = toolID == "Random" ? unpressedColor : defaultColor;
			if (GUILayout.Button("Random", EditorStyles.toolbarButton, GUILayout.Width(50)))
				toolID = "Random";

			for (int i = 0; i < profile.chipCount; i++) {
				key = "Color " + alphabet[i];
				GUI.color = toolID == key ? unpressedColor * chipColor[i] : defaultColor * chipColor[i];
				if (GUILayout.Button(key, EditorStyles.toolbarButton, GUILayout.Width(50)))
					toolID = key;
			}

			GUI.color = toolID == "Stone" ? unpressedColor : defaultColor;
            if (GUILayout.Button("Stone", EditorStyles.toolbarButton, GUILayout.Width(50)))
                toolID = "Stone";
			
			GUI.color = defaultColor;		
			GUILayout.FlexibleSpace ();
			
			EditorGUILayout.EndHorizontal ();
		}

		// Block modes
		if (currentMode == EditMode.Block) {
			EditorGUILayout.BeginHorizontal (EditorStyles.toolbar, GUILayout.ExpandWidth(true));
			
			defaultColor = GUI.color;
			GUI.color = toolID == "Simple Block" ? unpressedColor : defaultColor;
			if (GUILayout.Button("Simple Block", EditorStyles.toolbarButton, GUILayout.Width(80)))
				toolID = "Simple Block";
			GUI.color = toolID == "Weed" ? unpressedColor : defaultColor;
			if (GUILayout.Button("Weed", EditorStyles.toolbarButton, GUILayout.Width(40)))
				toolID = "Weed";
			GUI.color = toolID == "Branch" ? unpressedColor : defaultColor;
			if (GUILayout.Button("Branch", EditorStyles.toolbarButton, GUILayout.Width(50)))
				toolID = "Branch";
			GUI.color = defaultColor;		
			GUILayout.FlexibleSpace ();
			
			EditorGUILayout.EndHorizontal ();
		}



		EditorGUILayout.BeginVertical (EditorStyles.inspectorDefaultMargins);

		rect = GUILayoutUtility.GetRect (profile.width * (cellSize + slotOffect), profile.height * (cellSize + slotOffect));
		rect.x += slotOffect; 
		rect.y += slotOffect;

		EditorGUILayout.BeginHorizontal ();
		DrawModeTools ();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.EndVertical ();

        if (SessionAssistant.main) {
            powerupLabel.Clear();
            foreach (SessionAssistant.PowerUps powerup in SessionAssistant.main.powerups)
                if (powerup.levelEditorID > 0)
                    if (!powerupLabel.ContainsKey(powerup.levelEditorID))
                        powerupLabel.Add(powerup.levelEditorID, powerup.levelEditorName);
        }

		switch (currentMode) {
			case EditMode.Slot: DrawSlot(); break;
			case EditMode.Chip: DrawChip(); break;
			case EditMode.PowerUp: DrawPowerUp(); break;
			case EditMode.Jelly: DrawJelly(); break;
			case EditMode.Block: DrawBlock(); break;
			case EditMode.Wall: DrawWall(); break;
		}
		
		level.profile = profile; 
	}

	void DrawModeTools ()
	{
		switch (currentMode) {
		case EditMode.Slot:
			if (GUILayout.Button("Reset", GUILayout.Width(70))) 
				ResetSlots();		
			break;
		case EditMode.Chip:
			if (GUILayout.Button("Clear", GUILayout.Width(50))) 
				SetAllChips(-1);
			if (GUILayout.Button("Randomize", GUILayout.Width(90))) 
				SetAllChips(0);
			break;
		case EditMode.PowerUp:
			if (GUILayout.Button("Clear", GUILayout.Width(50))) 
				PowerUpClear();
			break;
		case EditMode.Jelly:
			if (GUILayout.Button("Clear", GUILayout.Width(50))) 
				JellyClear();
			break;
		case EditMode.Block:
			if (GUILayout.Button("Clear", GUILayout.Width(50))) 
				BlockClear();
			break;
		case EditMode.Wall:
			if (GUILayout.Button("Clear", GUILayout.Width(50))) 
				WallClear();
			break;	
		}
	}

	bool DrawSlotButton (int x, int y, Rect r, LevelProfile lp) {
		defaultColor = GUI.backgroundColor;
		Color color = Color.white;
		string label = "";
		bool btn = false;
		int block = lp.GetBlock (x, y);
		int jelly = lp.GetJelly (x, y);
		int chip = lp.GetChip (x, y);
		if (!lp.GetSlot(x, y)) color *= 0;
		else {
			if (block == 0) {
				if (chip == 9) {
					color *= buttonColor;
					lp.SetPowerup(x, y, 0);
				} else if (chip > 0) {
					if (chip > lp.chipCount)
						lp.SetChip(x, y, -1);
					color *= chipColor[chip - 1];
				}
			}
			if (block == 5) {
				if (chip > 0) {
					if (chip > lp.chipCount)
						lp.SetChip(x, y, -1);
					color *= chipColor[chip - 1];
				}
			}
			if (block == 0 && chip == -1 && lp.GetPowerup(x, y) == 0) {
				color *= unpressedColor;
			}
			if (block == 0 && lp.GetPowerup(x, y) > 0) {
				label += (label.Length == 0 ? "" : "\n");
				label += powerupLabel[lp.GetPowerup(x, y)];
			}

			if (block > 0 && block <= 3)
				label += (label.Length == 0 ? "" : "\n") + "B:" + block.ToString();
			if (block == 4)
				label += (label.Length == 0 ? "" : "\n") + "Weed";
			if (block == 5)
				label += (label.Length == 0 ? "" : "\n") + "Brch";
            if (jelly > 0 && lp.target == FieldTarget.Jelly) {
                label += (label.Length == 0 ? "" : "\n");
                switch (jelly) {
                    case 1: label += "JS"; break;
                    case 2: label += "JT"; break;
                }
            }

		}
		GUI.backgroundColor = color;
		btn = GUI.Button(new Rect(r.xMin + x * (cellSize + slotOffect), r.yMin + y * (cellSize + slotOffect), cellSize, cellSize), label, slotStyle);

        float cursor = -2;

		if (lp.GetSlot(x, y) && lp.GetGenerator (x, y)) {
			GUI.backgroundColor = Color.black;
			GUI.Box(new Rect(r.xMin + x * (cellSize + slotOffect) + cursor, r.yMin + y * (cellSize + slotOffect) - 2, 10, 10), "G", iconStyle);
            cursor += 10 + 2;
        }

        if (lp.target == FieldTarget.SugarDrop && lp.GetSlot(x, y) && lp.GetSugarDrop(x, y)) {
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(r.xMin + x * (cellSize + slotOffect) + cursor, r.yMin + y * (cellSize + slotOffect) - 2, 10, 10), "S", iconStyle);
            cursor += 10 + 2;
        }

        if (lp.GetSlot(x, y)) {
            GUI.backgroundColor = Color.black;
            GUI.Box(new Rect(r.xMin + x * (cellSize + slotOffect) + cursor, r.yMin + y * (cellSize + slotOffect) - 2, 10, 10), gravityLabel[profile.GetGravity(x, y)], iconStyle);
            cursor += 10 + 2;
        }

        if (lp.GetSlot(x, y) && lp.GetTeleport (x, y) > 0) {
			GUI.backgroundColor = Color.black;
			GUI.Box(new Rect(r.xMin + x * (cellSize + slotOffect) + cursor, r.yMin + y * (cellSize + slotOffect) - 2, cellSize - 12, 10), "T:" + lp.GetTeleport (x, y).ToString(), iconStyle);
		}

		if (lp.GetSlot (x, y)) {
			GUI.backgroundColor = transparentColor;
			GUI.Box (new Rect (r.xMin + x * (cellSize + slotOffect), r.yMin + y * (cellSize + slotOffect) + cellSize - 10, 20, 10), (y * 12 + x + 1).ToString (), slotStyle);
		}

        GUI.backgroundColor = defaultColor;
		return btn;
	}

	bool DrawSlotButtonTeleport (int x, int y, Rect r, LevelProfile lp) {
		if (!lp.GetSlot(x, y)) return false;

		defaultColor = GUI.backgroundColor;
		Color color = Color.cyan;
		if (teleportID.x == x && teleportID.y == y) color = Color.magenta;
		if (lp.GetTeleport(Mathf.FloorToInt(teleportID.x), Mathf.FloorToInt(teleportID.y)) == 12 * y + x + 1) color = Color.yellow;
		string label = "";

		bool btn = false;

		GUI.backgroundColor = color;
		btn = GUI.Button (new Rect (r.xMin + x * (cellSize + slotOffect), r.yMin + y * (cellSize + slotOffect), cellSize, cellSize), label, slotStyle);
		
		if (lp.GetSlot(x, y) && lp.GetGenerator (x, y)) {
			GUI.backgroundColor = Color.black;
			GUI.Box(new Rect(r.xMin + x * (cellSize + slotOffect) - 2, r.yMin + y * (cellSize + slotOffect) - 2, 10, 10), "G", iconStyle);
		}
		if (lp.GetSlot(x, y) && lp.GetTeleport (x, y) > 0) {
			GUI.backgroundColor = Color.black;
			GUI.Box(new Rect(r.xMin + x * (cellSize + slotOffect) + 10, r.yMin + y * (cellSize + slotOffect) - 2, cellSize - 12, 10), "T:" + lp.GetTeleport (x, y).ToString(), iconStyle);
		}

		if (lp.GetSlot (x, y)) {
			GUI.backgroundColor = transparentColor;
			GUI.Box (new Rect (r.xMin + x * (cellSize + slotOffect), r.yMin + y * (cellSize + slotOffect) + cellSize - 10, 20, 10), (y * 12 + x + 1).ToString (), slotStyle);
		}

		GUI.backgroundColor = defaultColor;

		return btn;
	}

	static bool DrawWallButton (int x, int y, string t, Rect r, LevelProfile lp)
	{
		bool btn = false;
		if (t == "H") btn = lp.GetWallH(x,y);
		if (t == "V") btn = lp.GetWallV(x,y);
		
		defaultColor = GUI.color;
		Color color = defaultColor;
		
		if (btn)
			color *= Color.red;
		GUI.color = color;
		
		if (t == "V") btn = GUI.Button(new Rect(r.xMin + (x + 1) * (cellSize + slotOffect) - 4 - slotOffect / 2,
		                                        r.yMin + y * (cellSize + slotOffect) - 10 + 20, 8, 20), "", slotStyle);
		if (t == "H") btn = GUI.Button(new Rect(r.xMin + x * (cellSize + slotOffect) - 10 + 20,
		                                        r.yMin + (y + 1) * (cellSize + slotOffect) - 4 - slotOffect / 2, 20, 8), "", slotStyle);
		GUI.color = defaultColor;
		return btn;
	}

	public static void DrawWallPreview (Rect r, LevelProfile lp) {
		int x;
		int y;
		GUI.enabled = false;
		for (x = 0; x < lp.width-1; x++)
			for (y = 0; y < lp.height; y++)
				if (lp.GetWallV(x,y) && lp.GetSlot(x,y) && lp.GetSlot(x+1,y))
					DrawWallButton(x, y, "V", r, lp);
		for (x = 0; x < lp.width; x++)
			for (y = 0; y < lp.height-1; y++)
				if (lp.GetWallH(x,y) && lp.GetSlot(x,y) && lp.GetSlot(x,y+1))
					DrawWallButton(x, y, "H", r, lp);
		GUI.enabled = true;
	}
	
	public void DrawSlotPreview (Rect r, LevelProfile lp) {
		int x;
		int y;
		GUI.enabled = false;
		for (x = 0; x < lp.width; x++)
			for (y = 0; y < lp.height; y++)
				if (lp.GetSlot(x, y))
					DrawSlotButton(x, y, r, lp);
		GUI.enabled = true;
	}

	void DrawSlot () {
		for (int x = 0; x < profile.width; x++) {
			for (int y = 0; y < profile.height; y++) {
				if (teleportID != -Vector2.right) {
					if (DrawSlotButtonTeleport(x, y, rect, profile)) {
						if (x == teleportID.x && y == teleportID.y)
							profile.SetTeleport(Mathf.CeilToInt(teleportID.x), Mathf.CeilToInt(teleportID.y), 0);
						else
							profile.SetTeleport(Mathf.CeilToInt(teleportID.x), Mathf.CeilToInt(teleportID.y), y * 12 + x + 1);
						teleportID = -Vector2.right;
					}
					continue;
				}


				if (DrawSlotButton(x, y, rect, profile)) {
					switch (toolID) {
					case "Slots": 
						profile.SetSlot(x, y, !profile.GetSlot(x,y));
						break;
					case "Generators": 
						profile.SetGenerator(x, y, !profile.GetGenerator(x,y));
						break;
					case "Teleports": 
						teleportID = new Vector2(x, y);
                        break;
                    case "Sugar Drop":
                        profile.SetSugarDrop(x, y, !profile.GetSugarDrop(x, y));
                        break;
                    case "Gravity":
                        profile.SetGravity(x, y, Mathf.CeilToInt(Mathf.Repeat( profile.GetGravity(x, y) + 1, 4)));
                        break;

                            
                    }
				}
			}
		}
		DrawWallPreview (rect, profile);
	}
	
	void DrawPowerUp () {
		for (int x = 0; x < profile.width; x++)
			for (int y = 0; y < profile.height; y++)
			if (DrawSlotButton(x, y, rect, profile)) {
                if (SessionAssistant.main) {
                    if (profile.GetPowerup(x, y) == 0) {
                        SessionAssistant.PowerUps powerup = SessionAssistant.main.powerups.Find(pu => pu.levelEditorName == toolID);
                        if (powerup != null)
                            profile.SetPowerup(x, y, powerup.levelEditorID);
                    } else
                        profile.SetPowerup(x, y, 0);
                }
			}
		DrawWallPreview (rect, profile);
	}
	
	void DrawJelly () {
		for (int x = 0; x < profile.width; x++)
			for (int y = 0; y < profile.height; y++)
			if (DrawSlotButton(x, y, rect, profile)) {
				profile.SetJelly(x,y, profile.GetJelly(x, y) + 1);
				if (profile.GetJelly(x,y) > 2)
					profile.SetJelly(x, y, 0);
			}
		DrawWallPreview (rect, profile);
	}
	
	void DrawBlock() {
		for (int x = 0; x < profile.width; x++) {
			for (int y = 0; y < profile.height; y++) {
				if (DrawSlotButton(x, y, rect, profile)) {
					switch (toolID) {
						case "Simple Block":
							profile.SetBlock(x, y, profile.GetBlock(x, y) + 1);
						if (profile.GetBlock(x, y) > 3)
							profile.SetBlock(x, y, 0);
						break;
						case "Weed":
							if (profile.GetBlock(x, y) != 4)
								profile.SetBlock(x, y, 4);
							else 
								profile.SetBlock(x, y, 0);
							break;
						case "Branch": 
							if (profile.GetBlock(x, y) != 5)
								profile.SetBlock(x, y, 5);
							else
								profile.SetBlock(x, y, 0);
							break;
					}
				}
			}
		}
		DrawWallPreview (rect, profile);
	}
	
	void DrawChip () {
		for (int x = 0; x < profile.width; x++) {
			for (int y = 0; y < profile.height; y++) {
				if (DrawSlotButton(x, y, rect, profile)) {
					switch (toolID) {
					case "Random": 
						if (profile.GetChip(x, y) != 0)
							profile.SetChip(x, y, 0);
						else
							profile.SetChip(x, y, -1);
							break;
					case "Color A": 
						if (profile.GetChip(x, y) != 1)
							profile.SetChip(x, y, 1);
						else
							profile.SetChip(x, y, -1);
						break;
					case "Color B": 
						if (profile.GetChip(x, y) != 2)
							profile.SetChip(x, y, 2);
						else
							profile.SetChip(x, y, -1);
						break;
					case "Color C": 
						if (profile.GetChip(x, y) != 3)
							profile.SetChip(x, y, 3);
						else
							profile.SetChip(x, y, -1);
						break;
					case "Color D": 
						if (profile.GetChip(x, y) != 4)
							profile.SetChip(x, y, 4);
						else
							profile.SetChip(x, y, -1);
						break;
					case "Color E": 
						if (profile.GetChip(x, y) != 5)
							profile.SetChip(x, y, 5);
						else
							profile.SetChip(x, y, -1);
						break;
					case "Color F": 
						if (profile.GetChip(x, y) != 6)
							profile.SetChip(x, y, 6);
						else
							profile.SetChip(x, y, -1);
						break;
                    case "Stone": 
						if (profile.GetChip(x, y) != 9)
							profile.SetChip(x, y, 9);
						else
							profile.SetChip(x, y, -1);
						break;
					}
				}
			}
		}
		DrawWallPreview (rect, profile);
	}
	
	void DrawWall () {
		int x;
		int y;
		DrawSlotPreview (rect, profile);
		for (x = 0; x < profile.width-1; x++)
			for (y = 0; y < profile.height; y++)
				if (profile.GetSlot(x,y) && profile.GetSlot(x+1,y))
					if (DrawWallButton(x, y, "V", rect, profile))
						profile.SetWallV(x, y, !profile.GetWallV(x, y));
		for (x = 0; x < profile.width; x++)
			for (y = 0; y < profile.height-1; y++)
				if (profile.GetSlot(x,y) && profile.GetSlot(x,y+1))
					if (DrawWallButton(x, y, "H", rect, profile))
						profile.SetWallH(x, y, !profile.GetWallH(x, y));
	}

	void ResetSlots () {
        
        if (toolID == "Slots")
            for (int x = 0; x < 12; x++)
			    for (int y = 0; y < 12; y++)
				    profile.SetSlot(x, y, true);

        if (toolID == "Generators")
            for (int x = 0; x < 12; x++)
                for (int y = 0; y < 12; y++)
                    profile.SetGenerator(x, y, y == 0);

        if (toolID == "Teleports")
            for (int x = 0; x < 12; x++)
                for (int y = 0; y < 12; y++)
                    profile.SetTeleport(x, y, 0);

        if (toolID == "Sugar Drop")
            for (int x = 0; x < 12; x++)
                for (int y = 0; y < 12; y++)
                    profile.SetSugarDrop(x, y, y == profile.height - 1);
	}
	
	void SetAllChips (int c) {
		for (int x = 0; x < 12; x++)
			for (int y = 0; y < 12; y++)
				profile.SetChip(x, y, c);
	}
	
	void PowerUpClear ()
	{
		for (int x = 0; x < 12; x++)
			for (int y = 0; y < 12; y++)
				profile.SetPowerup(x, y, 0);
	}
	
	void JellyClear ()
	{
		for (int x = 0; x < 12; x++)
			for (int y = 0; y < 12; y++)
				profile.SetJelly(x, y, 0);
	}
	
	void BlockClear ()
	{
		for (int x = 0; x < 12; x++)
			for (int y = 0; y < 12; y++)
				profile.SetBlock(x, y, 0);
	}
	
	void WallClear ()
	{
		for (int x = 0; x < 12; x++)
		for (int y = 0; y < 12; y++) {
			profile.SetWallH (x, y, false);
			profile.SetWallV (x, y, false);
		}
	}
}
