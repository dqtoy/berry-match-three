using UnityEngine;
using System.Collections;

// The class is responsible for logic SimpleChip
[RequireComponent (typeof (Chip))]
public class SimpleChip : MonoBehaviour {

	public Chip chip;
	bool mMatching = false;
    Animation anim;
    SpriteRenderer sprite;

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
	void OnDisable () {
		matching = false;
        if (chip.id >= 0 && chip.id < 6)
            SessionAssistant.main.countOfEachTargetCount[chip.id]--;
	}

    void Awake() {
        anim = GetComponent<Animation>();
		chip = GetComponent<Chip>();
		chip.chipType = "SimpleChip";
        sprite = GetComponent<SpriteRenderer>();
	}

    public void OnHit() {

    }

	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		
		matching = true;
		AudioAssistant.Shot("ChipCrush");
        OnHit();
		
		yield return new WaitForSeconds(0.1f);
		
		chip.ParentRemove();
		matching = false;

        if (chip.id >= 0 && chip.id < 6 && SessionAssistant.main.countOfEachTargetCount[chip.id] > 0) {
            GameObject go = GameObject.Find("ColorTargetItem" + Chip.chipTypes[chip.id]);

            if (go) {
                Transform target = go.transform;
                
                sprite.sortingLayerName = "UI";
                sprite.sortingOrder = 10;

                float time = 0;
                float speed = Random.Range(1f, 1.8f);
                Vector3 startPosition = transform.position;
                Vector3 targetPosition = target.position;

                while (time < 1) {
                    transform.position = Vector3.Lerp(startPosition, targetPosition, EasingFunctions.easeInOutQuad(time));
                    time += Time.unscaledDeltaTime * speed;
                    yield return 0;
                }

                transform.position = target.position;
            }
        }       
        

        anim.Play("Minimizing");

        while (anim.isPlaying)
            yield return 0;

        Destroy(gameObject);

	}
}