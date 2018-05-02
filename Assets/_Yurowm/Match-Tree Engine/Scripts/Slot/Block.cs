using UnityEngine;
using System.Collections;

// Destroyable blocks on playing field
public class Block : BlockInterface {

	public int level = 1; // Level of block. From 1 to 3. Each "BlockCrush"-call fall level by one. If it becomes zero, this block will be destroyed.
	public Sprite[] sprites; // Images of blocks of different levels. The size of the array must be equal to 3
	SpriteRenderer sr;
	int eventCountBorn;
    Animation anim;
    bool destroying = false;
    public string crush_effect;

	public void Initialize (){
		slot.gravity = false;
		sr = GetComponent<SpriteRenderer>();
		eventCountBorn = SessionAssistant.main.eventCount;
		sr.sprite = sprites[level-1];
        anim = GetComponent<Animation>();
	}

	
	#region implemented abstract members of BlockInterface
	
	// Crush block funtion
	override public void  BlockCrush (bool force) {
        if (destroying)
            return;
		if (eventCountBorn == SessionAssistant.main.eventCount && !force) return;
		eventCountBorn = SessionAssistant.main.eventCount;
		level --;
		FieldAssistant.main.field.blocks [slot.x, slot.y] = level;
		if (level == 0) {
			slot.gravity = true;
            slot.SetScore(1);
            slot.SetBlock(null);
            SlotGravity.Reshading();
            StartCoroutine(DestroyingRoutine());
			return;
		}
		if (level > 0) {
			anim.Play("BlockCrush");
            AudioAssistant.Shot("BlockHit");
			sr.sprite = sprites[level-1];
		}
	}

	public override bool CanBeCrushedByNearSlot () {
		return true;
	}

	#endregion

    IEnumerator DestroyingRoutine() {
        destroying = true;

        GameObject o = ContentAssistant.main.GetItem(crush_effect);
        o.transform.position = transform.position;

        anim.Play("BlockDestroy");
        AudioAssistant.Shot("BlockCrush");
        while (anim.isPlaying) {
            yield return 0;
        }

        Destroy(gameObject);
    }
}