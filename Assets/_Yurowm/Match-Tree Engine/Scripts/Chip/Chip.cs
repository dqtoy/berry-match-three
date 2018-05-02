using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The base class for chips
public class Chip : MonoBehaviour {

	public SlotForChip parentSlot; // Slot which include this chip
	public string chipType = "None"; // Chip type name
	public int id; // Chip color ID
	public int powerId; // Chip type ID
	public bool move = false; // is chip involved in the fall (SessionAssistant.main.gravity)
    public bool destroyable = true;
	public int movementID = 0;
    public bool can_move = true;
	public Vector3 impulse = Vector3.zero;
	Vector3 impulsParent = new Vector3(0,0,-1);
	Vector3 startPosition; 
	Vector3 moveVector;
	public bool destroying = false; // in the process of destruction
	float velocity = 0; // current velocity
	float acceleration = 20f; // acceleration
	static float velocityLimit = 17f;

    bool mGravity = false;
    public bool gravity {
        set {
            if (value == mGravity)
                return;
            mGravity = value;
            if (mGravity)
                SessionAssistant.main.gravity++;
            else
                SessionAssistant.main.gravity--;
        }

        get {
            return mGravity;
        }
    }

    Animation anim;

	// Colors for each chip color ID
	public static Color[] colors = {
		new Color(0.75f, 0.3f, 0.3f),
		new Color(0.3f, 0.75f, 0.3f),
		new Color(0.3f, 0.5f, 0.75f),
		new Color(0.75f, 0.75f, 0.3f),
		new Color(0.75f, 0.3f, 0.75f),
		new Color(0.75f, 0.6f, 0.3f)
	};


    public static string[] chipTypes = {
                                           "Red",
                                           "Green",
                                           "Blue",
                                           "Yellow",
                                           "Purple",
                                           "Orange" };
	
	Vector3 lastPosition;
	Vector3 zVector;
	
	void  Awake (){
		velocity = 1;
        anim = GetComponent<Animation>();
		move = true;
        gravity = true;
	}

	// function of conditions of possibility of matching
	public bool IsMatcheble (){
		if (id < 0) return false;
		if (destroying) return false;
		if (SessionAssistant.main.gravity == 0) return true;
		if (move) return false;
		if (transform.position != parentSlot.transform.position) return false;
		if (velocity != 0) return false;
        if (chipType == "SugarChip") return false;

		foreach (Side side in Utils.straightSides)
			if (parentSlot[side]
			&& parentSlot[side].gravity
			&& !parentSlot[side].GetShadow()
			&& !parentSlot[side].GetChip())
				return false;

		return true;
	}

	// function describing the physics of chips
	void  Update () {
        if (!parentSlot) {
            if (!destroying)
                DestroyChip();
            return;
        }

		if (!SessionAssistant.main.isPlaying) return;
		if (impulse != Vector3.zero && (parentSlot || impulsParent.z != -1)) {
			if (impulsParent.z == -1) {
				if (!parentSlot) {
					impulse = Vector3.zero;
					return;
				}
				if (!move) gravity = true;
				move = true;
				impulsParent = parentSlot.transform.position;
			}
            if (impulse.sqrMagnitude > 36)
                impulse = impulse.normalized * 6;
			transform.position += impulse * Time.deltaTime;
			transform.position += (impulsParent - transform.position) * Time.deltaTime;
			impulse -= impulse * Time.deltaTime;
			impulse -= 3f * (transform.position - impulsParent);
			impulse *= 1 - 6 * Time.deltaTime;
			if ((transform.position - impulsParent).magnitude < 2 * Time.deltaTime && impulse.magnitude < 2) {

                impulse = Vector3.zero;
                transform.position = impulsParent;
                impulsParent.z = -1;
                if (move) {
                    gameObject.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);
                    gravity = false;
                }

                move = false;
               
			}
			return;
		}
		
        if (destroying) return;
		if (!SessionAssistant.main.CanIGravity() || !can_move) return;
		
		if (SessionAssistant.main.matching > 0 && !move) return;
		moveVector.x = 0;
		moveVector.y = 0;
		
		if (parentSlot && transform.position != parentSlot.transform.position) {
			if (!move) {
				move = true;
                gravity = true;
				velocity = 3;
			}
			
			velocity += acceleration * Time.deltaTime;
			if (velocity > velocityLimit) velocity = velocityLimit;
			
			lastPosition = transform.position;
			
			if (Mathf.Abs(transform.position.x - parentSlot.transform.position.x) < velocity * Time.deltaTime) {
				zVector = transform.position;
				zVector.x = parentSlot.transform.position.x;
				transform.position = zVector;
			}
			if (Mathf.Abs(transform.position.y - parentSlot.transform.position.y) < velocity * Time.deltaTime) {
				zVector = transform.position;
				zVector.y = parentSlot.transform.position.y;
				transform.position = zVector;
			}
			
			if (transform.position == parentSlot.transform.position) {
				parentSlot.SendMessage("GravityReaction");
				if (transform.position != parentSlot.transform.position) 
					transform.position = lastPosition;
			}
			
			if (transform.position.x < parentSlot.transform.position.x)
				moveVector.x = 10;
			if (transform.position.x > parentSlot.transform.position.x)
				moveVector.x = -10;
			if (transform.position.y < parentSlot.transform.position.y)
				moveVector.y = 10;
			if (transform.position.y > parentSlot.transform.position.y)
				moveVector.y = -10;
			moveVector = moveVector.normalized * velocity;
			transform.position += moveVector * Time.deltaTime;
		} else {
			if (move) {
                move = false;
                velocity = 0;
                movementID = SessionAssistant.main.GetMovementID();
                gameObject.SendMessage("OnHit", SendMessageOptions.DontRequireReceiver);
                gravity = false;
			}
		}
	}

	// returns the value of the potential of the current chips. needs for estimation of solution potential.
    public int GetPotencial() {
        int potential;
        Slot slot;
        switch (LevelProfile.main.target) {
            case FieldTarget.Jelly:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>())) {
                    if (c.parentSlot)
                        potential += Jelly.GetPotention(c.parentSlot.slot);
                        foreach (Side side in Utils.straightSides) {
                            slot = Slot.GetSlot(c.parentSlot.slot.x + Utils.SideOffsetX(side), c.parentSlot.slot.y + Utils.SideOffsetY(side));
                            if (slot && slot.GetBlock())
                                potential += 5;
                        }
                }
                return potential;
            case FieldTarget.Block:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>()))
                    foreach (Side side in Utils.straightSides) {
                        slot = Slot.GetSlot(c.parentSlot.slot.x + Utils.SideOffsetX(side), c.parentSlot.slot.y + Utils.SideOffsetY(side));
                        if (slot && slot.GetBlock())
                            potential += 10;
                    }
                return potential;
            case FieldTarget.Color:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>()))
                    if (c.id == Mathf.Clamp(c.id, 0, 5))
                        if (SessionAssistant.main.countOfEachTargetCount[c.id] > 0)
                            potential += 10;
                return potential;
            case FieldTarget.None:
                potential = 1;
                foreach (Chip c in GetDangeredChips(new List<Chip>())) {
                    potential += GetPotencial(c.powerId);
                    foreach (Side side in Utils.straightSides) {
                        slot = Slot.GetSlot(c.parentSlot.slot.x + Utils.SideOffsetX(side), c.parentSlot.slot.y + Utils.SideOffsetY(side));
                        if (slot && slot.GetBlock())
                            potential += 10;
                    }
                }
                return potential;
            case FieldTarget.SugarDrop:
                Slot s;
                potential = 1;
                int plus;
                foreach (Chip c in GetDangeredChips(new List<Chip>())) {
                    s = c.parentSlot.slot;
                    if (c.chipType == "SugarChip")
                        continue;
                    plus = 0;
                    while (true) {
                        s = s[s.slotGravity.fallingDirection];

                        if (!s || !s.GetChip())
                            break;
                        else 
                            if (s.GetChip() && s.GetChip().chipType == "SugarChip")
                                plus ++;

                    }
                    if (plus > 0)
                        potential += plus * 10;
                    else
                        continue;
                    plus = 0;
                    s = c.parentSlot.slot;
                    while (true) {
                        if (!s)
                            break;
                        if (s.sugarDropSlot) {
                            plus = 1;
                            break;
                        }
                        s = s[s.slotGravity.gravityDirection];
                    }
                    if (plus == 1)
                        potential += 100;
                }

                return potential;
        }
        return 1;
		
	}

	// potential depending on powerID
	public static int GetPotencial (int i){
		if (i == 0) return 1; // Simple Chip
		if (i == 1) return 7; // Simple Bomb
		if (i == 2) return 12; // Cross Bomb
		if (i == 3) return 12; // Color Bomb
		if (i == 4) return 30; // Lightning Bomb
		return 0;
	}

    public void OnHit() {
        AudioAssistant.Shot("ChipHit");
    }

	// separation of the chips from the parent slot
	public void  ParentRemove (){
		if (!parentSlot) return;
		parentSlot.chip = null;
		parentSlot = null;
	}
	
	void  OnDestroy (){
        gravity = false;
	}

    void OnDisable() {
        gravity = false;
    }

	// Starting the process of destruction of the chips
	public void  DestroyChip (){
        if (!destroyable) return;
		if (destroying) return;
		if (parentSlot && parentSlot.slot.GetBlock ()) {
			parentSlot.slot.GetBlock ().BlockCrush(false);
			return;
		}
		destroying = true;
		SendMessage("DestroyChipFunction", SendMessageOptions.DontRequireReceiver); // It sends a message to another component. It assumes that there is another component to the logic of a specific type of chips
	}

	// Physically destroy the chip without activation and adding score points
	public void  HideChip (bool collection){
		if (destroying) return;
		destroying = true;

        if (collection && id == Mathf.Clamp(id, 0, 5))
			SessionAssistant.main.countOfEachTargetCount [id] --;
		ParentRemove();

        StartCoroutine(HidingRoutine());
	}

    IEnumerator HidingRoutine() {
        yield return StartCoroutine(MinimizingRoutine());
        Destroy(gameObject);
    }

    public void Minimize() {
        StartCoroutine(MinimizingRoutine());
    }

    IEnumerator MinimizingRoutine() {
        while (true) {
            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime * 6f);
            if (transform.localScale.x == 0) {
                yield break;
            }
            yield return 0;
        }
    }

    // Adding score points
    public void  SetScore (float s){
		SessionAssistant.main.score += Mathf.RoundToInt(s * SessionAssistant.scoreC);
		ScoreBubble.Bubbling(Mathf.RoundToInt(s * SessionAssistant.scoreC), transform, id);
	}

	// To begin the process of flashing (for hints - SessionAssistant.main.ShowHint)
	public void Flashing (int eventCount){
		StartCoroutine (FlashingUntil (eventCount));
	}

	// Coroutinr of flashing chip until a specified count of events
	IEnumerator  FlashingUntil (int eventCount){
        anim.Play("Flashing");
		while (eventCount == SessionAssistant.main.eventCount) yield return 0;
		if (!this) yield break;
        while (anim["Flashing"].time % anim["Flashing"].length > 0.1f)
            yield return 0;
        anim["Flashing"].time = 0;
		yield return 0;
        anim.Stop("Flashing");
	}

    public List<Chip> GetDangeredChips(List<Chip> stack) {
        if (stack.Contains(this))
            return stack;


        if (powerId == 0)
            stack.Add(this);
        if (powerId == 1)
            stack = gameObject.GetComponent<SimpleBomb>().GetDangeredChips(stack);
        if (powerId == 2)
            stack = gameObject.GetComponent<CrossBomb>().GetDangeredChips(stack);
        if (powerId == 3)
            stack = gameObject.GetComponent<ColorBomb>().GetDangeredChips(stack);
        if (powerId == 4)
            stack = gameObject.GetComponent<RainbowHeart>().GetDangeredChips(stack);
        if (powerId == 5)
            stack = gameObject.GetComponent<Ladybird>().GetDangeredChips(stack);


        return stack;
    }

    public static void Swap(Chip chip, Side side) {
        if (!chip.parentSlot)
            return;
        if (chip.parentSlot[side])
            AnimationAssistant.main.SwapTwoItem(chip, chip.parentSlot[side].GetChip(), false);
    }

    public void Reset() {
        transform.localScale = Vector3.one;
        move = true;
        gravity = false;
        velocity = 1;
        movementID = 0;
        impulse = Vector3.zero;
        impulsParent = new Vector3(0, 0, -1);
        destroying = false;
        transform.localScale = Vector3.one;
        transform.localEulerAngles = Vector3.zero;
    }
}