using UnityEngine;
using System.Collections;

public class AnimationAssistant : MonoBehaviour {
	// This script is responsible for procedural animations in the game. Such as change of place 2 chips and the effect of the explosion.
	
	public static AnimationAssistant main; // Main instance. Need for quick access to functions.
	void  Awake (){
		main = this;
	}

	float swapDuration = 0.2f;

	// Temporary Variables
    bool swaping = false; // ИTRUE when the animation plays swapping 2 chips
    public bool iteraction = false;
	[HideInInspector]

	// Function immediate swapping 2 chips
	public void SwapTwoItemNow (Chip a, Chip b) {
		if (!a || !b) return;
		if (a == b) return;
		if (a.parentSlot.slot.GetBlock() || b.parentSlot.slot.GetBlock()) return;

        //Vector3 posA = a.parentSlot.transform.position;
        //Vector3 posB = b.parentSlot.transform.position;
		
        //a.transform.position = posB;
        //b.transform.position = posA;
		
		a.movementID = SessionAssistant.main.GetMovementID();
		b.movementID = SessionAssistant.main.GetMovementID();
		
		SlotForChip slotA = a.parentSlot;
		SlotForChip slotB = b.parentSlot;
		
		slotB.SetChip(a);
		slotA.SetChip(b);
	}

	// The function of swapping 2 chips
	public void SwapTwoItem (Chip a, Chip b, bool force) {
		if (!SessionAssistant.main.isPlaying) return;
        StartCoroutine(SwapTwoItemRoutine(a, b, force)); // Starting corresponding coroutine
	}


	// Coroutine swapping 2 chips
	IEnumerator SwapTwoItemRoutine (Chip a, Chip b, bool force){
        if (!iteraction) yield break;
		// cancellation terms
		if (swaping) yield break; // If the process is already running
		if (!a || !b) yield break; // If one of the chips is missing
        if (a.destroying || b.destroying) yield break;
		if (a.parentSlot.slot.GetBlock() || b.parentSlot.slot.GetBlock()) yield break; // If one of the chips is blocked

		if (!SessionAssistant.main.CanIAnimate()) yield break; // If the core prohibits animation
		switch (LevelProfile.main.limitation) {
			case Limitation.Moves:
				if (SessionAssistant.main.movesCount <= 0) yield break; break; // If not enough moves
			case Limitation.Time:
				if (SessionAssistant.main.timeLeft <= 0) yield break; break; // If not enough time
		}

        SessionAssistant.Mix mix = SessionAssistant.main.mixes.Find(x => x.Compare(a.chipType, b.chipType));

		int move = 0; // Number of points movement which will be expend
		
		SessionAssistant.main.animate++;
		swaping = true;
		
		Vector3 posA = a.parentSlot.transform.position;
		Vector3 posB = b.parentSlot.transform.position;
		
		float progress = 0;

        Vector3 normal = a.parentSlot.slot.x == b.parentSlot.slot.x ? Vector3.right : Vector3.up;

        float time = 0;
		// Animation swapping 2 chips
		while (progress < swapDuration) {
            if (!a || !b)
                yield break;

            time = EasingFunctions.easeInOutQuad(progress / swapDuration);
            a.transform.position = Vector3.Lerp(posA, posB, time) + normal * Mathf.Sin(3.14f * time) * 0.2f;
			if (mix == null) b.transform.position = Vector3.Lerp(posB, posA, time) - normal * Mathf.Sin(3.14f * time) * 0.2f;
			
			progress += Time.deltaTime;
			
			yield return 0;
		}
		
		a.transform.position = posB;
		if (mix == null) b.transform.position = posA;
		
		a.movementID = SessionAssistant.main.GetMovementID();
		b.movementID = SessionAssistant.main.GetMovementID();

        if (mix != null) { // Scenario mix effect
			swaping = false;
            SessionAssistant.main.MixChips(a, b);
            yield return new WaitForSeconds(0.3f);
            SessionAssistant.main.movesCount--;
			SessionAssistant.main.animate--;
			yield break;
		}

		// Scenario the effect of swapping two chips
		SlotForChip slotA = a.parentSlot;
		SlotForChip slotB = b.parentSlot;
		
		slotB.SetChip(a);
		slotA.SetChip(b);
		
		
		move++;

		// searching for solutions of matching
		int count = 0; 
		SessionAssistant.Solution solution;
		
		solution = slotA.MatchAnaliz();
		if (solution != null) count += solution.count;

        solution = slotA.MatchSquareAnaliz();
		if (solution != null) count += solution.count;

		solution = slotB.MatchAnaliz();
		if (solution != null) count += solution.count;

        solution = slotB.MatchSquareAnaliz();
		if (solution != null) count += solution.count;

		// Scenario canceling of changing places of chips
		if (count == 0 && !force) {
			AudioAssistant.Shot("SwapFailed");
			while (progress > 0) {
                time = EasingFunctions.easeInOutQuad(progress / swapDuration);
                a.transform.position = Vector3.Lerp(posA, posB, time) - normal * Mathf.Sin(3.14f * time) * 0.2f;
                b.transform.position = Vector3.Lerp(posB, posA, time) + normal * Mathf.Sin(3.14f * time) * 0.2f;
				
				progress -= Time.deltaTime;
				
				yield return 0;
			}
			
			a.transform.position = posA;
			b.transform.position = posB;
			
			a.movementID = SessionAssistant.main.GetMovementID();
			b.movementID = SessionAssistant.main.GetMovementID();
			
			slotB.SetChip(b);
			slotA.SetChip(a);
			
			move--;
		} else {
			AudioAssistant.Shot("SwapSuccess");
			SessionAssistant.main.swapEvent ++;
		}

        SessionAssistant.main.firstChipGeneration = false;

		SessionAssistant.main.movesCount -= move;
		SessionAssistant.main.EventCounter ();
		
		SessionAssistant.main.animate--;
		swaping = false;
	}

	// Function of creating of explosion effect
	public void  Explode (Vector3 center, float radius, float force){
		Chip[] chips = GameObject.FindObjectsOfType<Chip>();
		Vector3 impuls;
		foreach(Chip chip in chips) {
			if ((chip.transform.position - center).magnitude > radius) continue;
			impuls = (chip.transform.position - center) * force;
			impuls *= Mathf.Pow((radius - (chip.transform.position - center).magnitude) / radius, 2);
			chip.impulse += impuls;
		}
	}

	public void TeleportChip(Chip chip, Slot target) {
		StartCoroutine (TeleportChipRoutine (chip, target));
	}

	IEnumerator TeleportChipRoutine (Chip chip, Slot target) {
		if (!chip.parentSlot) yield break;
        if (chip.destroying) yield break;
        if (target.GetChip() || !target.gravity) yield break;

        Vector3 scale_target = Vector3.zero;
        chip.can_move = false;
        target.SetChip(chip);

    
        scale_target.z = 1;
        while (chip.transform.localScale.x > 0) {
            chip.transform.localScale = Vector3.MoveTowards(chip.transform.localScale, scale_target, Time.deltaTime * 8);
            yield return 0;
        }

        chip.transform.localPosition = Vector3.zero;
        scale_target.x = 1;
        scale_target.y = 1;
        while (chip.transform.localScale.x < 1) {
            chip.transform.localScale = Vector3.MoveTowards(chip.transform.localScale, scale_target, Time.deltaTime * 12);
            yield return 0;
        }

        chip.can_move = true;
        //Chip new_chip = Instantiate(chip.gameObject).GetComponent<Chip>();
        //new_chip.parentSlot = null;
        //new_chip.transform.position = target.transform.position;
        //target.SetChip(new_chip);

        //chip.HideChip(false);
	}

}