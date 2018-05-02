using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (Chip))]
public class RainbowHeart : IBomb {

    int birth; // Event count at the time of birth SessionAssistant.main.eventCount
	public Chip chip;
	bool mMatching = false;

    List<Chip> chips = new List<Chip>();
    int branchCount;

    Animation anim;

	public bool matching {
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

    void Awake() {
        anim = GetComponent<Animation>();
        birth = SessionAssistant.main.eventCount;
		chip = GetComponent<Chip>();
        chip.chipType = "RainbowHeart";
        AudioAssistant.Shot("RainbowHeartCreate");
	}


    // Coroutine destruction / activation

    void DestroyChipFunction() {
        StartCoroutine(DestroyChipFunction(""));
    }

    IEnumerator  DestroyChipFunction (string powerup) {
        if (birth == SessionAssistant.main.eventCount) {
            chip.destroying = false;
            yield break;
        }

        if (!chip.destroying)
            yield break;

        

        anim.Play("RainbowHeartCrush");

        //yield return StartCoroutine(Utils.WaitFor(SessionAssistant.main.CanIMatch, 0.1f));

        matching = true;

        AudioAssistant.Shot("RainbowHeartCrush");
        SessionAssistant.main.EventCounter();

        int sx = chip.parentSlot.slot.x;
        int sy = chip.parentSlot.slot.y;

        FieldAssistant.main.JellyCrush(sx, sy);

        chips.Add(chip);
        branchCount = GetBranchCount();

        for (int i = 0; i < branchCount; i++)
            StartCoroutine(LightningBranch(powerup));

        while (branchCount != -1) {
            Debug.Log("Wait");
            yield return 0;
        }
    
        matching = false;

        anim.Play("RainbowHeartDestroy");
        while (anim.isPlaying)
            yield return 0;
        chip.ParentRemove();
        Destroy(gameObject);
	}

    IEnumerator LightningBranch(string powerup = "") {
        yield return new WaitForSeconds(0.1f);
       
        Slot currentSlot = chip.parentSlot.slot;
        Chip nextChip;
        Slot nextSlot;
        Lightning lightning = null;
        int iter = 10;
        int count = 10;

        List<Chip> branch = new List<Chip>();

        while (true) {

            if (iter <= 0 || count <= 0)
                break;

            nextSlot = currentSlot[Utils.allSides[Random.Range(0, Utils.allSides.Length)]];
            if (!nextSlot) {
                iter--;
                continue;
            }
            nextChip = nextSlot.GetChip();
            if (!nextChip || nextChip.destroying) {
                iter--;
                continue;
            }
            if (!currentSlot.GetChip()) {
                iter--;
                continue;
            }

            if (chips.Contains(nextChip) || branch.Contains(nextChip)) {
                iter--;
                continue;
            }

            chips.Add(nextChip);
            branch.Add(nextChip);

            int id = nextChip.id;

            if (lightning != null) {
                lightning.Remove();
            }
            lightning = Lightning.CreateLightning(0, currentSlot.GetChip().transform, nextChip.transform, id == Mathf.Clamp(id, 0, 5) ? Chip.colors[id] : Color.white);

            count--;

            currentSlot.GetChip().SetScore(0.3f);
            currentSlot.GetChip().DestroyChip();
            currentSlot = nextSlot;

            yield return new WaitForSeconds(0.02f);
        }


        if (powerup != "") {
            bool dontDestroy = powerup == "RainbowHeart" || powerup == "UltraColorBomb";
            Chip pu = FieldAssistant.main.AddPowerup(currentSlot.x, currentSlot.y, powerup);
            if (!dontDestroy)
                pu.can_move = false;
            pu.transform.localPosition = Vector3.zero;

            yield return 0;
            while (!dontDestroy && pu != null && pu.GetComponent<Animation>().isPlaying)
                yield return 0;
            if (pu != null && !dontDestroy) {
                SessionAssistant.main.EventCounter();
                pu.DestroyChip();
            }
        }

        if (lightning != null) {
            lightning.Remove();
            if (currentSlot.GetChip())
                currentSlot.GetChip().DestroyChip();
        }

        branchCount --;
       
        while (branchCount > 0)
            yield return 0;

        yield return 0;
        branchCount = -1;


        //yield return new WaitForSeconds(0.02f);

        //for (int i = 0; i < branch.Count; i++) {
        //    yield return new WaitForSeconds(0.03f);
        //    if (branch[i].destroing || !branch[i].parentSlot)
        //        continue;

        //    branch[i].SetScore(0.1f);
        //    FieldAssistant.main.BlockCrush(branch[i].parentSlot.slot.x, branch[i].parentSlot.slot.y, true);
        //    FieldAssistant.main.JellyCrush(branch[i].parentSlot.slot.x, branch[i].parentSlot.slot.y);
        //    if (branch[i] != chip)
        //        branch[i].DestroyChip();
        //}

        //branchCount--;

        //while (branchCount > 0)
        //    yield return 0;

        //branchCount = -1;
    }

    int GetBranchCount() {
        return 6;
    }

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(chip))
            return stack;
        foreach (Chip c in GameObject.FindObjectsOfType<Chip>())
            if (!c.destroying && c.parentSlot)
                stack.Add(c);
        return stack;
    }

    #region Mixes
    public void RainbowMix(Chip secondary) {
        chip.destroying = true;
        StartCoroutine(DestroyChipFunction(secondary.chipType));
    }
    #endregion
}