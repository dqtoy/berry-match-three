using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// slots which can contain the chips
[RequireComponent (typeof (Slot))]
public class SlotForChip : MonoBehaviour {

	public Chip chip;
	public Slot slot;

	public Slot this[Side index] { // access to neighby slots on the index
		get {
			return slot.nearSlot[index];
		}
	}

	void  Awake (){
		slot = GetComponent<Slot>();
	}
	
	public Chip GetChip (){
		return chip;
	}

	// function of assigning chip to the slot
	public void  SetChip (Chip c){
		if (chip) chip.parentSlot = null;

		chip = c;
		chip.transform.parent = transform;
		if (chip.parentSlot) {
			chip.parentSlot.chip = null;
			FieldAssistant.main.field.chips[chip.parentSlot.slot.x, chip.parentSlot.slot.y] = -1;
		}
		chip.parentSlot = this;
		FieldAssistant.main.field.chips[slot.x, slot.y] = chip.id;
	}

	public void  CrushChip (){
		chip.DestroyChip();
		chip = null;
	}

	// Analysis of chip for combination
	public SessionAssistant.Solution MatchAnaliz (){
		
		if (!GetChip()) return null;
		if (!GetChip().IsMatcheble()) return null;
        if (GetChip().id < 0) return null;


        if (GetChip().id == 10) { // multicolor
            List<SessionAssistant.Solution> solutions = new List<SessionAssistant.Solution>();
            SessionAssistant.Solution z;
            Chip multicolorChip = GetChip();
            for (int i = 0; i < 6; i++) {
                multicolorChip.id = i;
                z = MatchAnaliz();
                if (z != null)
                    solutions.Add(z);
                z = MatchSquareAnaliz();
                if (z != null)
                    solutions.Add(z);
            }
            multicolorChip.id = 10;
            z = null;
            foreach (SessionAssistant.Solution sol in solutions)
                if (z == null || z.potential < sol.potential)
                    z = sol;
            return z;
        }

        Slot s;
        Dictionary<Side, List<Chip>> sides = new Dictionary<Side, List<Chip>>();
        int count;
        string key;
        foreach (Side side in Utils.straightSides) {
            count = 1;
            sides.Add(side, new List<Chip>());
            while (true) {
                key = (slot.x + Utils.SideOffsetX(side) * count).ToString() + "_" + (slot.y + Utils.SideOffsetY(side) * count).ToString();
                if (!Slot.all.ContainsKey(key))
                    break;
                s = Slot.all[key];
                if (!s.GetChip())
                    break;
                if (s.GetChip().id != chip.id && s.GetChip().id != 10)
                    break;
                if (!s.GetChip().IsMatcheble())
                    break;
                sides[side].Add(s.GetChip());
                count++;
            }
        }

        bool h = sides[Side.Right].Count + sides[Side.Left].Count >= 2;
        bool v = sides[Side.Top].Count + sides[Side.Bottom].Count >= 2;
        
		if (h || v) {
			SessionAssistant.Solution solution = new SessionAssistant.Solution();

            solution.h = h;
            solution.v = v;

            solution.chips = new List<Chip>();
            solution.chips.Add(GetChip());

            if (h) {
                solution.chips.AddRange(sides[Side.Right]);
                solution.chips.AddRange(sides[Side.Left]);
            }
            if (v) {
                solution.chips.AddRange(sides[Side.Top]);
                solution.chips.AddRange(sides[Side.Bottom]);
            }

			solution.count = solution.chips.Count;
            
            solution.x = slot.x;
			solution.y = slot.y;
			solution.id = chip.id;

            foreach (Chip c in solution.chips)
			    solution.potential += c.GetPotencial();

			return solution;
		}
		return null;
	}

    public SessionAssistant.Solution MatchSquareAnaliz() {

        if (!SessionAssistant.main.squareCombination)
            return null;
        if (!GetChip())
            return null;
        if (!GetChip().IsMatcheble())
            return null;
        if (GetChip().id < 0)
            return null;


        if (GetChip().id == 10) { // multicolor
            List<SessionAssistant.Solution> solutions = new List<SessionAssistant.Solution>();
            SessionAssistant.Solution z;
            Chip multicolorChip = GetChip();
            for (int i = 0; i < 6; i++) {
                multicolorChip.id = i;
                z = MatchSquareAnaliz();
                if (z != null)
                    solutions.Add(z);
            }
            multicolorChip.id = 10;
            z = null;
            foreach (SessionAssistant.Solution sol in solutions)
                if (z == null || z.potential < sol.potential)
                    z = sol;
            return z;
        }

        List<Chip> square = new List<Chip>();
        List<Chip> buffer = new List<Chip>();
        Side sideR;
        string key;
        Slot s;


        buffer.Clear();
        foreach (Side side in Utils.straightSides) {
            for (int r = 0; r <= 2; r++) {
                sideR = Utils.RotateSide(side, r);
                key = (slot.x + Utils.SideOffsetX(sideR)).ToString() + "_" + (slot.y + Utils.SideOffsetY(sideR)).ToString();
                if (Slot.all.ContainsKey(key)) {
                    s = Slot.all[key];
                    if (s.GetChip() && (s.GetChip().id == chip.id || s.GetChip().id == 10) && s.GetChip().IsMatcheble())
                        buffer.Add(s.GetChip());
                    else
                        break;
                } else
                    break;
            }
            if (buffer.Count == 3) {
                foreach (Chip chip_b in buffer)
                    if (!square.Contains(chip_b))
                        square.Add(chip_b);
            }
            buffer.Clear();
        }
        

        bool q = square.Count >= 3;

        if (q) {
            SessionAssistant.Solution solution = new SessionAssistant.Solution();

            solution.q = q;

            solution.chips = new List<Chip>();
            solution.chips.Add(GetChip());

            solution.chips.AddRange(square);
  
            solution.count = solution.chips.Count;

            solution.x = slot.x;
            solution.y = slot.y;
            solution.id = chip.id;

            foreach (Chip c in solution.chips)
                solution.potential += c.GetPotencial();

            return solution;
        }
        return null;
    }

}
