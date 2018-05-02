using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The class is responsible for logic SimpleBomb
[RequireComponent (typeof (Chip))]
public class SimpleBomb : IBomb {

	Chip chip;
	int birth; // Event count at the time of birth SessionAssistant.main.eventCount

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
		chip = GetComponent<Chip>();
        chip.chipType = "SimpleBomb";
		birth = SessionAssistant.main.eventCount;
        anim = GetComponent<Animation>();
		AudioAssistant.Shot ("CreateBomb");
	}

	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		if (birth == SessionAssistant.main.eventCount) {
			chip.destroying = false;
			yield break;
		}
		
		matching = true;

        yield return new WaitForSeconds(0.1f);

        anim.Play("SimpleBump");
		AudioAssistant.Shot("BombCrush");

		int sx = chip.parentSlot.slot.x;
		int sy = chip.parentSlot.slot.y;

        chip.ParentRemove();
		
		yield return new WaitForSeconds(0.05f);

		FieldAssistant.main.JellyCrush(sx, sy);

        foreach (Side side in Utils.allSides)
            NeighborMustDie(sx + Utils.SideOffsetX(side), sy + Utils.SideOffsetY(side));


        AnimationAssistant.main.Explode(transform.position, 5, 10);
        		
		yield return new WaitForSeconds(0.1f);
		matching = false;
		
		while (anim.isPlaying) yield return 0;
		Destroy(gameObject);
	}
	
	void  NeighborMustDie ( int x ,   int y  ){
        Slot s = Slot.GetSlot(x, y);
		if (s) {
			if (s.GetChip()) {
                s.GetChip().SetScore(0.3f);
                s.GetChip().DestroyChip();
			}
			FieldAssistant.main.BlockCrush(x, y, false);
			FieldAssistant.main.JellyCrush(x, y);
		}
		
	}

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);

        Slot slot;

        foreach (Side s in Utils.allSides) {
            slot = chip.parentSlot[s];
            if (slot && slot.GetChip()) {
                stack = slot.GetChip().GetDangeredChips(stack);
            }
        }

        return stack;
    }

    #region Mixes
    public void SimpleMix(Chip secondary) {
        StartCoroutine(SimpleMixRoutine(secondary));
    }

    IEnumerator SimpleMixRoutine(Chip secondary) {
        matching = true;
        chip.destroyable = false;
        SessionAssistant.main.EventCounter();

        Transform effect = ContentAssistant.main.GetItem("SimpleMixEffect").transform;
        effect.SetParent(Slot.folder);
        effect.position = transform.position;
        effect.GetComponent<Animation>().Play();
        AudioAssistant.Shot("BombCrush");
        SessionAssistant.main.EventCounter();

        chip.Minimize();

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        int width = FieldAssistant.main.field.width;
        int height = FieldAssistant.main.field.height;

        SessionAssistant.main.EventCounter();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (Mathf.Abs(sx - x) + Mathf.Abs(sy - y) <= 3)
                    Crush(x, y);

        AnimationAssistant.main.Explode(transform.position, 5, 30);

        yield return new WaitForSeconds(0.6f);

        matching = false;

        while (GetComponent<Animation>().isPlaying)
            yield return 0;

        FieldAssistant.main.BlockCrush(sx, sy, false);

        chip.HideChip(false);
    }

    public static void Crush(int x, int y) {
        Slot s = Slot.GetSlot(x, y);
        FieldAssistant.main.BlockCrush(x, y, false, true);
        FieldAssistant.main.JellyCrush(x, y);
        if (s && s.GetChip()) {
            Chip c = s.GetChip();
            c.SetScore(0.3f);
            c.DestroyChip();
        }
    }
    #endregion
}