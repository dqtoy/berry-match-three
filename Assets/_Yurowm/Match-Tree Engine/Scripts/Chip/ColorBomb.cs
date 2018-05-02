using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The class is responsible for logic ColorBomb
[RequireComponent (typeof (Chip))]
public class ColorBomb : IBomb {

	Chip chip;
	int birth; // Event count at the time of birth SessionAssistant.main.eventCount
	public Color color;
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
		chip.chipType = "ColorBomb";
        anim = GetComponent<Animation>();
		birth = SessionAssistant.main.eventCount;
		AudioAssistant.Shot ("CreateColorBomb");
	}

	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		if (birth == SessionAssistant.main.eventCount) {
			chip.destroying = false;
			yield break;
		}
        
        matching = true;

		anim.Play("ColorBump");
        AudioAssistant.Shot("ColorBombCrush");
		
		
		int width = FieldAssistant.main.field.width;
		int height = FieldAssistant.main.field.height;
		
		int sx = chip.parentSlot.slot.x;
		int sy = chip.parentSlot.slot.y;
		
		Slot s;

		FieldAssistant.main.JellyCrush(sx, sy);


		for (int x= 0; x < width; x++) {
			for (int y= 0; y < height; y++) {
				if (y == sy && x == sx) continue;
                s = Slot.GetSlot(x, y);
				if (s && s.GetChip() && s.GetChip().id == chip.id) {
					Lightning.CreateLightning(3, transform, s.GetChip().transform, color);
                    yield return new WaitForSeconds(0.03f);
				}
			}
		}
		
		yield return new WaitForSeconds(0.1f);
		
		for (int x1= 0; x1 < width; x1++) {
			for (int y1= 0; y1 < height; y1++) {
				if (y1 == sy && x1 == sx) continue;
                s = Slot.GetSlot(x1, y1);
				if (s && s.GetChip() && s.GetChip().id == chip.id) {
					s.GetChip().SetScore(0.3f);
					FieldAssistant.main.BlockCrush(x1, y1, true);
					FieldAssistant.main.JellyCrush(x1, y1);
                    s.GetChip().DestroyChip();
                    yield return new WaitForSeconds(0.02f);
				}
			}
		}
		
		yield return new WaitForSeconds(0.1f);
		matching = false;
		
		while (anim.isPlaying) yield return 0;
		chip.ParentRemove();
        Destroy(gameObject);
	}

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;

        stack.Add(chip);

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        Slot s;

        for (int x = 0; x < FieldAssistant.main.field.width; x++) {
            for (int y = 0; y < FieldAssistant.main.field.height; y++) {
                if (y == sy && x == sx)
                    continue;
                s = Slot.GetSlot(x, y);
                if (s && s.GetChip() && s.GetChip().id == chip.id) {
                    stack = s.GetChip().GetDangeredChips(stack);
                }
            }
        }
        return stack;
    }

    #region Mixes
    public void ColorMix(Chip secondary) {
        StartCoroutine(ColorMixRoutine(secondary));
    }

    IEnumerator ColorMixRoutine(Chip secondary) {
        matching = true;
        chip.destroyable = false;

        int width = FieldAssistant.main.field.width;
        int height = FieldAssistant.main.field.height;

        SimpleChip[] allChips = FindObjectsOfType<SimpleChip>();
        List<SimpleChip>[] sorted = new List<SimpleChip>[6];
        int[] count = new int[6];

        foreach (SimpleChip c in allChips) {
            if (c.chip.destroying)
                continue;
            if (!c.chip.parentSlot)
                continue;
            if (c.chip == secondary)
                continue;
            count[c.chip.id]++;
            if (sorted[c.chip.id] == null)
                sorted[c.chip.id] = new List<SimpleChip>();
            sorted[c.chip.id].Add(c);
        }

        List<SlotForChip> target = new List<SlotForChip>();

        int i;
        for (i = 0; i < 6; i++)
            if (sorted[i] != null && sorted[i].Count > 0)
                target.Add(sorted[i][Random.Range(0, sorted[i].Count)].chip.parentSlot);


        yield return new WaitForSeconds(0.1f);

        AudioAssistant.Shot("ColorBombCrush");
        anim.Play("ColorBump");

        int x;
        int y;
        for (i = 0; i < target.Count; i++) {
            x = target[i].slot.x;
            y = target[i].slot.y;
            Chip pu = FieldAssistant.main.AddPowerup(x, y, secondary.chipType);
            if (secondary.chipType != "UltraColorBomb")
                pu.can_move = false;
            target[i].SetChip(pu);
            Lightning.CreateLightning(3, transform, pu.transform, Chip.colors[i]);
            yield return new WaitForSeconds(0.1f);

        }

        yield return new WaitForSeconds(0.2f);

        SessionAssistant.main.EventCounter();
        if (secondary.chipType != "UltraColorBomb")
            for (i = 0; i < target.Count; i++) {
                if (target[i].GetChip())
                    target[i].GetChip().DestroyChip();
                yield return new WaitForSeconds(0.05f);
            }

        matching = false;

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        FieldAssistant.main.JellyCrush(sx, sy);

        while (anim.isPlaying)
            yield return 0;

        chip.ParentRemove();
        chip.HideChip(false);
    }
    #endregion
}