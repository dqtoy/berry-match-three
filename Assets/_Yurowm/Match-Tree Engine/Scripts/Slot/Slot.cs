using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Base class for slots
public class Slot : MonoBehaviour {

    public static Dictionary<string, Slot> all = new Dictionary<string, Slot>();
    public string key {
        get {
            return x.ToString() + "_" + y.ToString();
        }
    }

	public bool gravity;
    public bool generator = false;
    public bool teleportTarget = false;
	Jelly jelly; // Jelly for this slot
    BlockInterface block; // Block for this slot

	// Position of this slot
	public int x;
	public int y;

	public Dictionary<Side, Slot> nearSlot = new Dictionary<Side, Slot> (); // Nearby slots dictionary
	public Slot this[Side index] { // access to neighby slots on the index
		get {
			return nearSlot[index];
		}
	}

	public Dictionary<Side, bool> wallMask = new Dictionary<Side, bool> (); // Dictionary walls - blocks the movement of chips in certain directions
	
	SlotForChip slotForChip;
    public SlotGravity slotGravity;
    public SlotTeleport slotTeleport;
    public bool sugarDropSlot = false;
    public static Transform folder;
	
	void  Awake (){
		slotForChip = GetComponent<SlotForChip>();
		slotGravity = GetComponent<SlotGravity>();
        slotTeleport = GetComponent<SlotTeleport>();
	}

	public static void  Initialize (){
        foreach (Slot slot in GameObject.FindObjectsOfType<Slot>())
            if (!all.ContainsKey(slot.key))
                all.Add(slot.key, slot);


        foreach (Slot slot in all.Values) {
            foreach (Side side in Utils.allSides) // Filling of the nearby slots dictionary 
                slot.nearSlot.Add(side, Slot.FindNearSlot(slot.x, slot.y, side));
            slot.nearSlot.Add(Side.Null, null);
		    foreach (Side side in Utils.straightSides) // Filling of the walls dictionary
                slot.wallMask.Add(side, false);
        }

        Side direction;
        SlotTeleport teleport;
        foreach (Slot slot in all.Values) {
            direction = slot.slotGravity.gravityDirection;
            if (slot[direction]) {
                slot[direction].slotGravity.fallingDirection = Utils.MirrorSide(direction);
            }
            teleport = slot.GetComponent<SlotTeleport>();
            if (teleport)
                teleport.Initialize();
        }
	}

	// function of assigning chip to the slot
	public void  SetChip (Chip c){
		if (slotForChip) {
			FieldAssistant.main.field.chips[x, y] = c.id;
			FieldAssistant.main.field.powerUps[x, y] = c.powerId;
			slotForChip.SetChip(c);

		}
	}

    public Chip GetChip (){
		if (slotForChip) return slotForChip.GetChip();
		return null;
	}
	
	public BlockInterface GetBlock (){
		return block;
	}
	
	public Jelly GetJelly (){
		return jelly;
	}

	
	public void  SetBlock (BlockInterface b){
		block = b;
	}

	public void  SetJelly (Jelly j){
		jelly = j;
	}
	
	void  CrushChip (){
		if (slotForChip) slotForChip.CrushChip();
	}

    public void SetScore(float s) {
        SessionAssistant.main.score += Mathf.RoundToInt(s * SessionAssistant.scoreC);
        ScoreBubble.Bubbling(Mathf.RoundToInt(s * SessionAssistant.scoreC), transform);
    }

	// Check for the presence of the "shadow" in the slot. No shadow - is a direct path from the slot up to the slot with a component SlotGenerator. Towards must have slots (without blocks and wall)
	// This concept is very important for the proper physics chips
	public bool GetShadow (){
		if (slotGravity) return slotGravity.shadow;
		else return false;
	}

	// Shadow can also discard the other chips - it's a different kind of shadow.
	public bool GetChipShadow (){
        Side direction = slotGravity.fallingDirection;
        Slot s = nearSlot[direction];
		for (int i = 0; i < 40; i ++) {
			if (!s) return false;
			if (!s.gravity)  return false;
			if (!s.GetChip() || s.slotGravity.gravityDirection != direction) {
                direction = s.slotGravity.fallingDirection;
                s = s.nearSlot[direction];
            } else return true;
		}
        return false;
	}
	
	// creating a wall between it and neighboring slot
	public void  SetWall (Side side){

		wallMask[side] = true;

		foreach (Side s in Utils.straightSides)
			if (wallMask[s]) {
				if (nearSlot[s]) nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
				nearSlot[s] = null;
			}
	
		foreach (Side s in Utils.slantedSides)
			if (wallMask[Utils.SideHorizontal(s)] && wallMask[Utils.SideVertical(s)]) {
				if (nearSlot[s]) nearSlot[s].nearSlot[Utils.MirrorSide(s)] = null;
				nearSlot[s] = null;
			}

	}

    public static Slot GetSlot(int x, int y) {
        string key = x.ToString() + "_" + y.ToString();
        if (all.ContainsKey(key))
            return all[key];
        return null;
    }

    public static Slot FindNearSlot(int x, int y, Side side) {
        string key = "Slot_" + (x + Utils.SideOffsetX(side))+ "x" + (y + Utils.SideOffsetY(side));
        GameObject result = GameObject.Find(key);
        if (result)
            return result.GetComponent<Slot>();
        return null;
    }
}