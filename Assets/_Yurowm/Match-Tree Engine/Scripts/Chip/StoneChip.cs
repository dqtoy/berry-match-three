using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Chip))]
public class StoneChip : MonoBehaviour {
	
	public Chip chip;
	bool mMatching = false;
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
	
	void  Awake (){
		chip = GetComponent<Chip>();
		chip.chipType = "SimpleChip";
	}
	
	// Coroutine destruction / activation
	IEnumerator  DestroyChipFunction (){
		
		matching = true;
        AudioAssistant.Shot("StoneCrush");
		GetComponent<Animation>().Play("Minimizing");
		
		yield return new WaitForSeconds(0.1f);
		matching = false;
		
        chip.SetScore(1);

		chip.ParentRemove();

		GameObject o = ContentAssistant.main.GetItem ("StoneCrush");
		o.transform.position = transform.position;


		while (GetComponent<Animation>().isPlaying) yield return 0;
		Destroy(gameObject);
	}
}