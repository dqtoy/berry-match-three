using UnityEngine;
using System.Collections;

// Slot which generates new simple chips.
[RequireComponent (typeof (Slot))]
[RequireComponent (typeof (SlotForChip))]
public class SlotGenerator : MonoBehaviour {

	public Slot slot;
	public SlotForChip slotForChip;
	public Chip chip;

	float lastTime = -10;
	float delay = 0.15f; // delay between the generations
	
	void  Awake (){
		slot = GetComponent<Slot>();
        slot.generator = true;
		slotForChip = GetComponent<SlotForChip>(); 
	}
	
	void  Update (){
        if (!SessionAssistant.main.enabled) return;

		if (!SessionAssistant.main.CanIGravity ()) return; // Generation is possible only in case of mode "gravity"
		
		if (slotForChip.GetChip()) return; // Generation is impossible, if slot already contains chip
		
		if (slot.GetBlock()) return; // Generation is impossible, if the slot is blocked

		if (lastTime + delay > Time.time) return; // limit of frequency generation
		lastTime = Time.time;

        Vector3 spawnOffset = new Vector3(
            Utils.SideOffsetX(Utils.MirrorSide(slot.slotGravity.gravityDirection)),
            Utils.SideOffsetY(Utils.MirrorSide(slot.slotGravity.gravityDirection)),
            0) * 0.4f;

        if (LevelProfile.main.target == FieldTarget.SugarDrop && SessionAssistant.main.creatingSugarDropsCount > 0) {
            if (SugarChip.live_count == 0 || SessionAssistant.main.GetResource() <= 0.4f + 0.6f * SessionAssistant.main.creatingSugarDropsCount / LevelProfile.main.targetSugarDropsCount) {
                SessionAssistant.main.creatingSugarDropsCount--;
                FieldAssistant.main.GetSugarChip(slot.x, slot.y, transform.position + spawnOffset); // creating new sugar chip
                return;
            }
        }

		if (Random.value > LevelProfile.main.buttonPortion)
            FieldAssistant.main.GetNewSimpleChip(slot.x, slot.y, transform.position + spawnOffset); // creating new chip
		else
            FieldAssistant.main.GetNewStone(slot.x, slot.y, transform.position + spawnOffset); // creating new stone
	}
}