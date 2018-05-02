using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Generator of playing field
public class FieldAssistant : MonoBehaviour {

	public static FieldAssistant main;
	
	float slotoffset = 0.7f;

	[HideInInspector]
	public Field field;

	public static int width {
		get {
			if (main.field != null)
				return main.field.width;
			return 0;
		}
	}

	public static int height {
		get {
			if (main.field != null)
				return main.field.height;
			return 0;
		}
	}
	
	// Names of chip colors
	
	void  Awake (){
		main = this;
	}

    public void StartLevel() {
        StartCoroutine(StartLevelRoutine());
    }

    IEnumerator StartLevelRoutine() {
        UIAssistant.main.ShowPage("Loading");

        while (CPanel.uiAnimation > 0)
            yield return 0;

        ProfileAssistant.main.local_profile["live"]--;

        SessionAssistant.main.enabled = false;

		SessionAssistant.Reset();
        
        yield return StartCoroutine(CreateField());

		SessionAssistant.main.enabled = true;
		SessionAssistant.main.eventCount ++;

        SessionAssistant.main.StartSession(LevelProfile.main.target, LevelProfile.main.limitation);

        GameCamera.main.transform.position = new Vector3(0, 20, -10);
    }

	// Field generator
	public IEnumerator  CreateField (){
        Utils.waitingStatus = "Level loading";
		RemoveField (); // Removing old field
		
		field = new Field (LevelProfile.main.width, LevelProfile.main.height);
		field.chipCount = LevelProfile.main.chipCount;

        yield return StartCoroutine(GenerateSlots());
        yield return StartCoroutine(GenerateBlocks());

        Slot.Initialize();

        yield return StartCoroutine(GenerateWalls());

        SlotGravity.Reshading();

        yield return StartCoroutine(GenerateJelly());
        yield return StartCoroutine(GenerateChips());
        yield return StartCoroutine(GeneratePowerups());

        SUBoosterButton.Generate(Slot.folder);
	}

	// Removing old field
	public void  RemoveField (){
        if (Slot.folder)
            Destroy(Slot.folder.gameObject);
	}

	// Generation slots for chips
    IEnumerator GenerateSlots() {
        Utils.waitingStatus = "Slots building";
        Slot.folder = new GameObject().transform;
        Slot.folder.name = "Slots";
        GameObject o;
        GameObject sd;

		Slot s;
		Vector3 position;

        Slot.all.Clear();

        for (int x = 0; x < field.width; x++) {
            yield return 0;
            for (int y = 0; y < field.height; y++) {
                field.slots[x, y] = LevelProfile.main.GetSlot(x, height - y - 1);
                field.generator[x, y] = LevelProfile.main.GetGenerator(x, height - y - 1);
                field.sugarDrop[x, y] = LevelProfile.main.GetSugarDrop(x, height - y - 1);
                field.teleport[x, y] = LevelProfile.main.GetTeleport(x, height - y - 1);

                if (field.slots[x, y]) {
                    position = new Vector3();
                    position.x = -slotoffset * (0.5f * (field.width - 1) - x);
                    position.y = -slotoffset * (0.5f * (field.height - 1) - y);
                    o = ContentAssistant.main.GetItem("SlotEmpty", position);
                    o.name = "Slot_" + x + "x" + y;
                    o.transform.parent = Slot.folder;
                    s = o.GetComponent<Slot>();
                    s.x = x;
                    s.y = y;

                    Slot.all.Add(s.key, s);

                    if (field.generator[x, y])
                        s.gameObject.AddComponent<SlotGenerator>();

                    if (field.teleport[x, y] > 0) {
                        Debug.Log("Add teleport");
                        s.slotTeleport.targetID = field.teleport[x, y];
                    }

                    sd = null;
                    if (LevelProfile.main.target == FieldTarget.SugarDrop && field.sugarDrop[x, y]) {
                        s.sugarDropSlot = true;
                        sd = ContentAssistant.main.GetItem("SugarDrop", position);
                        sd.name = "SugarDrop";
                        sd.transform.parent = o.transform;
                        sd.transform.localPosition = Vector3.zero;
                    }

                    int gravity = LevelProfile.main.GetGravity(x, height - y - 1);
                    switch (gravity) {
                        case 0:
                            s.slotGravity.gravityDirection = Side.Bottom;
                            break;
                        case 1:
                            s.slotGravity.gravityDirection = Side.Left;
                            break;
                        case 2:
                            s.slotGravity.gravityDirection = Side.Top;
                            break;
                        case 3:
                            s.slotGravity.gravityDirection = Side.Right;
                            break;
                    }

                    if (sd) {
                        switch (gravity) {
                            case 1:
                                sd.transform.Rotate(0, 0, -90);
                                break;
                            case 2:
                                sd.transform.Rotate(0, 0, 180);
                                break;
                            case 3:
                                sd.transform.Rotate(0, 0, 90);
                                break;
                        }
                    }
                }
            }
        }		
	}

	// Generation jelly in slots
	IEnumerator  GenerateJelly (){
        Utils.waitingStatus = "Tahwing jellies";

        if (LevelProfile.main.target != FieldTarget.Jelly)
            yield break;
		GameObject o;
		Slot s;
		Jelly j;
		
		for (int x = 0; x < field.width; x++)				
		for (int y = 0; y < field.height; y++) {
			field.jellies[x,y] = LevelProfile.main.GetJelly(x,height-y-1);
			if (field.slots[x,y] && field.jellies[x,y] > 0) {
                s = Slot.GetSlot(x, y);
                switch (field.jellies[x,y]) {
                    case 1:
                        o = ContentAssistant.main.GetItem("SingleLayerJelly");
                        break;
                    case 2:
                    default:
                        o = ContentAssistant.main.GetItem("Jelly");
                        break;
                }
				o.transform.position = s.transform.position;
				o.name = "Jelly_" + x + "x" + y;
				o.transform.parent = s.transform;
				j = o.GetComponent<Jelly>();
				s.SetJelly(j);

                yield return 0;
			}
		}
	}

	// Generation of destructible blocks
    IEnumerator GenerateBlocks() {
        Utils.waitingStatus = "Block building";
		GameObject o;
		Slot s;
		Block b;
		Weed w;
		Branch brch;
		
		for (int x = 0; x < field.width; x++)					
		for (int y = 0; y < field.height; y++) {
			field.blocks[x,y] = LevelProfile.main.GetBlock(x,height-y-1);
			if (field.slots[x,y]) {
				if (field.blocks[x,y] > 0) {
					if (field.blocks[x,y] <= 3) {
                        s = Slot.GetSlot(x, y);
						o = ContentAssistant.main.GetItem("Block");
						o.transform.position = s.transform.position;
						o.name = "Block_" + x + "x" + y;
						o.transform.parent = s.transform;
						b = o.GetComponent<Block>();
						s.SetBlock(b);
						b.slot = s;
						b.level = field.blocks[x,y];
						b.Initialize();
					}
					if (field.blocks[x,y] == 4) {
                        s = Slot.GetSlot(x, y);
						o = ContentAssistant.main.GetItem("Weed");
						o.transform.position = s.transform.position;
						o.name = "Weed_" + x + "x" + y;
						o.transform.parent = s.transform;
						w = o.GetComponent<Weed>();
						s.SetBlock(w);
						w.slot = s;
						w.Initialize();
					}
					if (field.blocks[x,y] == 5) {
                        s = Slot.GetSlot(x, y);
						o = ContentAssistant.main.GetItem("Branch");
						o.transform.position = s.transform.position;
						o.name = "Branch_" + x + "x" + y;
						o.transform.parent = s.transform;
						brch = o.GetComponent<Branch>();
						s.SetBlock(brch);
						brch.slot = s;
						brch.Initialize();
					}
                    yield return 0;
				}
			}
		}
	}

	// Generation impassable walls
    IEnumerator GenerateWalls() {
        Utils.waitingStatus = "Walls building";
		int x;
		int y;
        Slot near;
        Slot current;

		for (x = 0; x < field.width-1; x++)		
			for (y = 0; y < field.height; y++) {
			    field.wallsV[x,y] = LevelProfile.main.GetWallV(x,height-y-1);
				if (field.wallsV[x,y] && field.slots[x,y]) {
                    current = Slot.GetSlot(x, y);
                    if (current) {
                        current.SetWall(Side.Right);
                        near = current[Side.Right];
                        if (near)
                            near.SetWall(Side.Left);
                    }
				}
			}

		for (x = 0; x < field.width; x++)	
			for (y = 0; y < field.height-1; y++) {
			field.wallsH[x,y] = LevelProfile.main.GetWallH(x, height-y-2);
				if (field.wallsH[x,y] && field.slots[x,y]) {
                    current = Slot.GetSlot(x, y);
                    if (current) {
                        current.SetWall(Side.Top);
                        near = current[Side.Top];
                        if (near)
                            near.SetWall(Side.Bottom);
                    }
                }
			}

        List<Pair> walls = new List<Pair>();
        Pair pair;
        Vector3 position;
        GameObject wall;
        foreach (Slot slot in Slot.all.Values) {
            yield return 0;
            foreach (Side side in Utils.straightSides) {
                if (slot[side] != null)
                    continue;
                pair = new Pair(slot.key, (slot.x + Utils.SideOffsetX(side)) + "_" + (slot.y + Utils.SideOffsetY(side)));
                if (walls.Contains(pair))
                    continue;

                position = new Vector3();
                position.x = Utils.SideOffsetX(side) * 0.353f;
                position.y = Utils.SideOffsetY(side) * 0.353f;

                wall = ContentAssistant.main.GetItem("Wall", position);
                wall.transform.parent = slot.transform;
                wall.transform.localPosition = position;
                wall.name = "Wall_" + side;
                if (Utils.SideOffsetY(side) != 0)
                    wall.transform.Rotate(0, 0, 90);

                walls.Add(pair);

            }
        }
	}

	// Generation bombs
	IEnumerator GeneratePowerups ()
	{

        Utils.waitingStatus = "Making bombs";

		int x;
		int y;
		
		for (x = 0; x < field.width; x++)				
			for (y = 0; y < field.height; y++)
				field.powerUps[x,y] = LevelProfile.main.GetPowerup(x,height-y-1);

        SessionAssistant.PowerUps powerup;

		for (x = 0; x < field.width; x++)				
		for (y = 0; y < field.height; y++) {
			if (field.powerUps[x,y] > 0 && field.slots[x,y]) {
                powerup = SessionAssistant.main.powerups.Find(pu => pu.levelEditorID == field.powerUps[x, y]);
                if (powerup != null)
                    AddPowerup(x, y, powerup.name);
                yield return 0;
			}
		}
	}

	// Generation chips
	IEnumerator GenerateChips (){

        Utils.waitingStatus = "Gathering berries";

		int x;
		int y;

		for (x = 0; x < field.width; x++)				
			for (y = 0; y < field.height; y++)
				field.chips[x,y] = LevelProfile.main.GetChip(x,height-y-1);

		field.FirstChipGeneration();
        yield return 0;
		
		int id;

        for (x = 0; x < field.width; x++) {
            yield return 0;
			for (y = 0; y < field.height; y++) {
                if (field.blocks[x, y] == 4)
                    continue;
				id = field.GetChip(x, y);
				if (id >= 0 && id != 9 && (field.blocks[x,y] == 0 || field.blocks[x,y] == 5)) 
					GetNewSimpleChip(x, y, new Vector3(0, 4, 0), id);
				if (id == 9 && (field.blocks[x,y] == 0))
                    GetNewStone(x, y, new Vector3(0, 4, 0));
			}
        }			
	}

	// Creating a simple random color chips
	public Chip GetNewSimpleChip (int x, int y, Vector3 position){
		return GetNewSimpleChip(x, y, position, SessionAssistant.main.colorMask[Random.Range(0, field.chipCount)]);
	}

    // Creating a sugar chips
    public Chip GetSugarChip(int x, int y, Vector3 position) {
        GameObject o = ContentAssistant.main.GetItem("Sugar");
        o.transform.position = position;
        o.name = "Sugar";
        if (Slot.GetSlot(x, y).GetChip())
            o.transform.position = Slot.GetSlot(x, y).GetChip().transform.position;
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(x, y).SetChip(chip);
        return chip;
    }

    public Chip GetNewStone (int x, int y, Vector3 position) {
		GameObject o = ContentAssistant.main.GetItem ("Stone");
		o.transform.position = position;
        o.name = "Stone";
        if (Slot.GetSlot(x, y).GetChip())
            o.transform.position = Slot.GetSlot(x, y).GetChip().transform.position;
		Chip chip = o.GetComponent<Chip> ();
        Slot.GetSlot(x, y).SetChip(chip);
		return chip;
	}

	// Creating a simple chip specified color
	public Chip GetNewSimpleChip (int x, int y, Vector3 position, int id) {
        GameObject o = ContentAssistant.main.GetItem("SimpleChip" + Chip.chipTypes[id]);
		o.transform.position = position;
        o.name = "Chip_" + Chip.chipTypes[id];
        if (Slot.GetSlot(x, y).GetChip())
            o.transform.position = Slot.GetSlot(x, y).GetChip().transform.position;
		Chip chip = o.GetComponent<Chip> ();
		Slot.GetSlot(x, y).SetChip(chip);
		return chip;
	}

	// Creating a cross-bombs specified color
	public Chip GetNewCrossBomb (int x, int y,Vector3 position, int id){
        GameObject o = ContentAssistant.main.GetItem("CrossBomb" + Chip.chipTypes[id]);
		o.transform.position = position;
        o.name = "CrossBomb_" + Chip.chipTypes[id];
        if (Slot.GetSlot(x, y).GetChip())
            o.transform.position = Slot.GetSlot(x, y).GetChip().transform.position;
		Chip chip = o.GetComponent<Chip> ();
        Slot.GetSlot(x, y).SetChip(chip);
		return chip;
	}

    public Chip GetNewBomb(int x, int y, string powerup, Vector3 position, int id) {
        SessionAssistant.PowerUps p = SessionAssistant.main.powerups.Find(pu => pu.name == powerup);
        if (p == null)
            return null;

        GameObject o = ContentAssistant.main.GetItem(p.contentName + (p.color ? Chip.chipTypes[id] : ""));
        o.transform.position = position;
        o.name = p.contentName + (p.color ? Chip.chipTypes[id] : "");
        if (Slot.GetSlot(x, y).GetChip())
            o.transform.position = Slot.GetSlot(x, y).GetChip().transform.position;
        Chip chip = o.GetComponent<Chip>();
        Slot.GetSlot(x, y).SetChip(chip);
        return chip;
    }

	// Make a bomb in the specified location with the ability to transform simple chips in a bomb
	public Chip AddPowerup(int x, int y, string powerup) {
        SlotForChip slot = Slot.GetSlot(x, y).GetComponent<SlotForChip>();
		Chip chip = slot.chip;
		int id;
		if (chip)
			id = chip.id;
		else 
			id = Random.Range(0, LevelProfile.main.chipCount);
		if (chip)
			Destroy (chip.gameObject);

        chip = GetNewBomb(slot.slot.x, slot.slot.y, powerup, slot.transform.position, id);
		return chip;
	}

	// Create a bomb with the possibility of transformation of simple chips in bomb
    public void AddPowerup(string powerup) {
		SimpleChip[] chips = GameObject.FindObjectsOfType<SimpleChip>();
		if (chips.Length == 0) return;
		SimpleChip chip = null;
		while (chip == null || chip.matching)
			chip = chips[Random.Range(0, chips.Length - 1)];
		SlotForChip slot = chip.chip.parentSlot;
		if (slot)
            AddPowerup(slot.slot.x, slot.slot.y, powerup);
	}
	
	// Request of jelly object
	public Jelly GetJelly ( int x ,   int y  ){
		if (field.GetSlot(x, y))
			return Slot.GetSlot(x, y).GetJelly();		
		return null;
	}

	// Request of block object
	public BlockInterface GetBlock ( int x ,   int y  ){
		if (field.GetSlot(x, y))
            return Slot.GetSlot(x, y).GetBlock();		
		return null;
	}

	// Crush jelly function
	public void JellyCrush (int x, int y) {
		GameObject j = GameObject.Find("Jelly_" + x + "x" + y);
		if (j) j.SendMessage("JellyCrush", SendMessageOptions.DontRequireReceiver);	
	}

	// Crush block function
   	public void  BlockCrush (int x, int y, bool radius, bool force = false) {
		BlockInterface b;
		Slot s;
		Chip c;
		StoneChip sc;


		if (radius) {
			foreach (Side side in Utils.straightSides) {
				b = null;
				s = null;
				c = null;
				sc = null;

				b = GetBlock(x + Utils.SideOffsetX(side), y + Utils.SideOffsetY(side));
                if (b && b.CanBeCrushedByNearSlot())
                    b.BlockCrush(force);
                				
				s = Slot.GetSlot(x + Utils.SideOffsetX(side), y + Utils.SideOffsetY(side));
				if (s) c = s.GetChip();
				if (c) sc = c.GetComponent<StoneChip>();
				if (sc) c.DestroyChip();
			}
		} 

		b = GetBlock(x, y);
		if (b) b.BlockCrush(force);
	}
}

// The class information about the playing field and the target level
public class Field {
	public int width;
	public int height;
	public int chipCount;
	public bool[,] slots;
	public int[,] teleport;
    public bool[,] generator;
    public bool[,] sugarDrop;
    public int[,] chips;
	public int[,] powerUps;
	public int[,] blocks;
	public int[,] jellies;
	public bool[,] wallsH;
	public bool[,] wallsV;

	public FieldTarget target = FieldTarget.None;
	public int targetValue = 0;
	
	
	public Field (int w, int h){
		width = w;
		height = h;
		slots = new bool [w,h];
        generator = new bool[w, h];
        sugarDrop = new bool[w, h];
        teleport = new int[w,h];
		chips = new int [w,h];
		powerUps = new int [w,h];
		blocks = new int [w,h];
		jellies = new int [w,h];
		wallsV = new bool [w,h];
		wallsH = new bool [w,h];
	}
	
	public bool GetSlot (int x, int y){
		if (x >= 0 && x < width && y >= 0 && y < height) return slots[x,y];
		return false;
	}
	
	public int GetChip (int x, int y){
		if (x >= 0 && x < width && y >= 0 && y < height) return chips[x,y];
		return 0;
	}
	
	public void  NewRandomChip (int x, int y, bool unmatching){
		if (chips[x,y] == -1) return;
		
		chips[x,y] = Random.Range(1, chipCount + 1);
		
		if (unmatching) {
			while (GetChip(x, y) == GetChip(x, y+1)
			       || GetChip(x, y) == GetChip(x+1, y)
			       || GetChip(x, y) == GetChip(x, y-1)
			       || GetChip(x, y) == GetChip(x-1, y))
				chips[x,y] = Random.Range(1, chipCount + 1);
		}
	}
	
	public void  FirstChipGeneration (){
		int x;
		int y;
		
		// impose a mask slots
		for (x = 0; x < width; x++) {
			for (y = 0; y < height; y++) {
				if (!slots[x,y] || (blocks[x,y] > 0 && blocks[x,y] != 5))
					chips[x,y] = -1;
				if (blocks[x,y] == 5 && chips[x,y] == -1)
					chips[x,y] = 0;
			}
		}
		
		// replace random chips on nonrandom
		for (x = 0; x < width; x++)					
			for (y = 0; y < height; y++) 
				if (chips[x,y] == 0 && chips[x,y] != 9)
					NewRandomChip(x, y, true);	
		
		// nonrandom give chips to the normal (0 to 5)
		for (x = 0; x < width; x++)					
			for (y = 0; y < height; y++) 
				if (chips[x,y] > 0 && chips[x,y] != 9)
					chips[x,y] --;

		
		// shuffling color
		// create a deck of colors and shuffling its
		int[] a = {0, 1, 2, 3, 4, 5};
		int j;
		for (int i = 5; i > 0; i--) {
			j = Random.Range(0, i);
			a[j] = a[j] + a[i];
			a[i] = a[j] - a[i];
			a[j] = a[j] - a[i];
		}

		SessionAssistant.main.colorMask = a;
		
		// apply the results to the matrix shuffling chips	
		for (x = 0; x < width; x++)					
			for (y = 0; y < height; y++) 
				if (chips[x,y] >= 0 && chips[x,y] != 9)
					chips[x,y] = a[chips[x,y]];
	}
	
}

public enum FieldTarget {
	None = 0,
	Jelly = 1,
	Block = 2,
    Color = 3,
    SugarDrop = 4
}

public enum Limitation {
	Moves,
	Time
}