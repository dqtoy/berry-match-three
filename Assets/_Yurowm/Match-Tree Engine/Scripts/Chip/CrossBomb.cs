using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The class is responsible for logic CrossBomb
[RequireComponent (typeof (Chip))]
public class CrossBomb : IBomb {

    public string type = "CrossBomb";
    public Side[] sides = new Side[4] {
        Side.Left,
        Side.Right,
        Side.Top,
        Side.Bottom
    };

	Chip chip;
	int birth; // Event count at the time of birth SessionAssistant.main.eventCount
    int branchCount;

    Animation anim;

    bool mMatching = false;
	bool matching {
		set {
			if (value == mMatching) return;
			mMatching = value;
			if (mMatching)
				SessionAssistant.main.matching ++;
			else
				SessionAssistant.main.matching --;
		}

		get {
			return mMatching;
		}
	}
	void OnDestroy () {
		matching = false;
	}
	
	void  Awake (){
        anim = GetComponent<Animation>();
		chip = GetComponent<Chip>();
		chip.chipType = type;
		birth = SessionAssistant.main.eventCount;
		AudioAssistant.Shot ("CreateCrossBomb");
	}

    // Coroutine destruction / activation
    IEnumerator DestroyChipFunction() {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }

        while (transform.localPosition != Vector3.zero) {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, Vector3.zero, Time.deltaTime * 10);
            yield return 0;
        }

        matching = true;

        List<Side> _sides = new List<Side>(sides);
        if (!_sides.Contains(Side.Left))
            Destroy(transform.FindChild("SideLeft").gameObject);
        if (!_sides.Contains(Side.Right))
            Destroy(transform.FindChild("SideRight").gameObject);
        if (!_sides.Contains(Side.Top))
            Destroy(transform.FindChild("SideTop").gameObject);
        if (!_sides.Contains(Side.Bottom))
            Destroy(transform.FindChild("SideBottom").gameObject);

        anim.Play("CrossBump");
        AudioAssistant.Shot("CrossBombCrush");
        
        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        chip.ParentRemove();

        FieldAssistant.main.BlockCrush(sx, sy, false);
        FieldAssistant.main.JellyCrush(sx, sy);

        int count = 4;
        for (int path = 1; count > 0; path++) {
            count = 0;
            foreach (Side side in sides) {
                if (Freez(sx + Utils.SideOffsetX(side) * path, sy + Utils.SideOffsetY(side) * path))
                    count++;
            }
        }

        count = 4;
        for (int path = 1; count > 0; path++) {
            count = 0;
            yield return new WaitForSeconds(0.02f);
            foreach (Side side in sides)
                if (Crush(sx + Utils.SideOffsetX(side) * path, sy + Utils.SideOffsetY(side) * path))
                    count++;
        }

        yield return new WaitForSeconds(0.1f);

        matching = false;

        while (anim.isPlaying)
            yield return 0;
        Destroy(gameObject);
    }

    public static bool Crush(int x, int y) {
        Slot s = Slot.GetSlot(x, y);
        FieldAssistant.main.BlockCrush(x, y, false, true);
        FieldAssistant.main.JellyCrush(x, y);
        if (s && s.GetChip()) {
            Chip c = s.GetChip();
            c.SetScore(0.3f);
            c.DestroyChip();
            AnimationAssistant.main.Explode(s.transform.position, 3, 20);
        }
        return x >= 0 && y >= 0 && x < FieldAssistant.main.field.width && y < FieldAssistant.main.field.height;
    }

    public static bool Freez(int x, int y) {
        Slot s = Slot.GetSlot(x, y);
        if (s && s.GetChip() && s.GetChip().destroyable && !(s.GetBlock() && s.GetBlock() is Branch))
            s.GetChip().can_move = false;
        return x >= 0 && y >= 0 && x < FieldAssistant.main.field.width && y < FieldAssistant.main.field.height;
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip) || !chip.parentSlot)
            return stack;

        stack.Add(chip);

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        Slot s;

        for (int x = 0; x < FieldAssistant.main.field.width; x++) {
            if (x == sx)
                continue;
            s = Slot.GetSlot(x, sy);
            if (s && s.GetChip() && s.GetChip().id == chip.id) {
                stack = s.GetChip().GetDangeredChips(stack);
            }
        }

        for (int y = 0; y < FieldAssistant.main.field.height; y++) {
            if (y == sy)
                continue;
            s = Slot.GetSlot(sx, y);
            if (s && s.GetChip()) {
                stack = s.GetChip().GetDangeredChips(stack);
            }
        }

        return stack;
    }



    #region Mixes

    public void CrossSimpleMix(Chip secondary) {
        StartCoroutine(CrossSimpleMixRoutine(secondary));
    }

    IEnumerator CrossSimpleMixRoutine(Chip secondary) { 
        matching = true;
        chip.destroyable = false;
        SessionAssistant.main.EventCounter();

        Transform effect = ContentAssistant.main.GetItem("SimpleCrossMixEffect").transform;
        effect.SetParent(Slot.folder);
        effect.position = transform.position;
        effect.GetComponent<Animation>().Play();
        AudioAssistant.Shot("CrossBombCrush");

        List<Side> _sides = new List<Side>(sides);
        if (!_sides.Contains(Side.Left)) 
            Destroy(effect.FindChild("WaveLeft").gameObject);
        if (!_sides.Contains(Side.Right))
            Destroy(effect.FindChild("WaveRight").gameObject);
        if (!_sides.Contains(Side.Top))
            Destroy(effect.FindChild("WaveTop").gameObject);
        if (!_sides.Contains(Side.Bottom))
            Destroy(effect.FindChild("WaveBottom").gameObject);

        chip.Minimize();


        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        yield return new WaitForSeconds(0.1f);

        FieldAssistant.main.JellyCrush(sx, sy);
        FieldAssistant.main.BlockCrush(sx, sy, false);

        for (int i = 0; i < 12; i++) {
            if (_sides.Contains(Side.Right)) {
                Freez(sx + i, sy + 1);
                Freez(sx + i, sy);
                Freez(sx + i, sy - 1);
            }
            if (_sides.Contains(Side.Top)) {
                Freez(sx - 1, sy + i);
                Freez(sx, sy + i);
                Freez(sx + 1, sy + i);
            }
            if (_sides.Contains(Side.Left)) {
                Freez(sx - i, sy + 1);
                Freez(sx - i, sy);
                Freez(sx - i, sy - 1);
            }
            if (_sides.Contains(Side.Bottom)) {
                Freez(sx - 1, sy - i);
                Freez(sx, sy - i);
                Freez(sx + 1, sy - i);
            }
        }

        System.Action<int, int> Wave = (int x, int y) => {
            Slot s = Slot.GetSlot(x, y);
            if (s)
                AnimationAssistant.main.Explode(s.transform.position, 3, 10);
        };

        for (int i = 1; i < 12; i++) {
            SessionAssistant.main.EventCounter();
            if (i == 1) {
                Crush(sx + 1, sy);
                Crush(sx + 1, sy + 1);
                Crush(sx    , sy + 1);
                Crush(sx - 1, sy + 1);
                Crush(sx - 1, sy);
                Crush(sx - 1, sy - 1);
                Crush(sx   , sy - 1);
                Crush(sx + 1, sy - 1);
            } else {
                if (_sides.Contains(Side.Right)) {
                    Crush(sx + i, sy + 1);
                    Crush(sx + i, sy);
                    Crush(sx + i, sy - 1);
                }
                if (_sides.Contains(Side.Top)) {
                    Crush(sx - 1, sy + i);
                    Crush(sx, sy + i);
                    Crush(sx + 1, sy + i);
                }
                if (_sides.Contains(Side.Left)) {
                    Crush(sx - i, sy + 1);
                    Crush(sx - i, sy);
                    Crush(sx - i, sy - 1);
                }
                if (_sides.Contains(Side.Bottom)) {
                    Crush(sx - 1, sy - i);
                    Crush(sx, sy - i);
                    Crush(sx + 1, sy - i);
                }
            }

            foreach (Side side in sides)
                Wave(sx + Utils.SideOffsetX(side) * i, sy + Utils.SideOffsetY(side) * i);

            yield return new WaitForSeconds(0.05f);
        }

        matching = false;
        chip.HideChip(false);
    }

    public void CrossMix(Chip secondary) {
        StartCoroutine(CrossMixRoutine(secondary));
    }

    IEnumerator CrossMixRoutine(Chip secondary) {
        matching = true;
        chip.destroyable = false;
        SessionAssistant.main.EventCounter();

        Transform effect = ContentAssistant.main.GetItem("CrossMixEffect").transform;
        effect.SetParent(Slot.folder);
        effect.position = transform.position;
        effect.GetComponent<Animation>().Play();
        AudioAssistant.Shot("CrossBombCrush");

        chip.Minimize();

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        chip.ParentRemove();

        FieldAssistant.main.BlockCrush(sx, sy, false);
        FieldAssistant.main.JellyCrush(sx, sy);

        int count = 8;
        for (int path = 1; count > 0; path++) {
            count = 0;
            foreach (Side side in Utils.allSides) {
                if (Freez(sx + Utils.SideOffsetX(side) * path, sy + Utils.SideOffsetY(side) * path))
                    count++;
            }
        }

        count = 8;
        for (int path = 1; count > 0; path++) {
            count = 0;
            yield return new WaitForSeconds(0.05f);
            foreach (Side side in Utils.allSides)
                if (Crush(sx + Utils.SideOffsetX(side) * path, sy + Utils.SideOffsetY(side) * path)) 
                    count++;
        }

        yield return new WaitForSeconds(0.1f);

        matching = false;
        chip.HideChip(false);
    }

    public void LineMix(Chip secondary) {
        sides = Utils.straightSides;
        chip.DestroyChip();
    }
    #endregion

}