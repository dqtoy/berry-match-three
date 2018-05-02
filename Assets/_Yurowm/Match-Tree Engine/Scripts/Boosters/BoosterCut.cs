using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// "Cut" booster
// This object must be in the UI-panel of the booster. During activation (OnEnable) it turn a special mode of interaction with chips (ControlAssistant ignored)
[RequireComponent (typeof (BoosterButton))]
public class BoosterCut : MonoBehaviour {

    BoosterButton booster;
    public Animation spoon;

	void OnEnable () {
		TurnController (false);
		StartCoroutine (Cut());
	}

	void OnDisable () {
		TurnController (true);
	}

	// Enable/Disable ControlAssistant
	void TurnController(bool b) {
		if (ControlAssistant.main == null) return;
		ControlAssistant.main.enabled = b;
	}

	// Coroutine of special control mode
	IEnumerator Cut () {
        spoon.gameObject.SetActive(false);

		yield return StartCoroutine (Utils.WaitFor (SessionAssistant.main.CanIWait, 0.1f));

		Slot target = null;
		while (true) {
			if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
				target = ControlAssistant.main.GetSlotFromTouch();
            if (target != null && (!target.GetChip() ||  target.GetChip().chipType != "SugarChip")) {

                spoon.transform.position = target.transform.position;
                spoon.gameObject.SetActive(true);
                spoon.Play();

                CPanel.uiAnimation++;

                yield return new WaitForSeconds(0.91f);

                ProfileAssistant.main.local_profile["spoon"] --;
               
				FieldAssistant.main.BlockCrush(target.x, target.y, false);
				FieldAssistant.main.JellyCrush(target.x, target.y);
				
                SessionAssistant.main.EventCounter();
                
                if (target.GetChip())
                    target.GetChip().DestroyChip();

                while (spoon.isPlaying)
                    yield return 0;

                spoon.gameObject.SetActive(false);

                CPanel.uiAnimation--;

                break;
			}
			yield return 0;
		}

        UIAssistant.main.ShowPage("Field");
	}
}
