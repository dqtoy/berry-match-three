using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// "Switch" Booster
// This object must be in the UI-panel of the booster. During activation (OnEnable) it turn a special mode of interaction with chips
public class BoosterSwitch : MonoBehaviour {

    BoosterButton booster;

    public Animation hand;

	void OnEnable () {
		StartCoroutine (Switch());
	}

	void OnDisable () {
        ControlAssistant.swap = Chip.Swap; 
	}

	// Coroutine of special control mode
	IEnumerator Switch ()
	{
        hand.gameObject.SetActive(false);
		yield return StartCoroutine (Utils.WaitFor (SessionAssistant.main.CanIWait, 0.1f));

        Chip chipA = null;
        Chip chipB = null;
        Side side = Side.Null;
        System.Action<Chip, Side> fu = (Chip c, Side s) => {
            if (!c.parentSlot)
                return;
            if (c.parentSlot[s]) {
                chipA = c;
                chipB = c.parentSlot[s].GetChip();
                side = s;
            }
        };

        ControlAssistant.swap = fu;

        while (chipA == null || chipB == null)
            yield return 0;


        ProfileAssistant.main.local_profile["hand"]--;
        ControlAssistant.swap = Chip.Swap;

        Vector3 rotation = new Vector3();
        switch (side) {
            case Side.Bottom:
                rotation.z = 0;
                break;
            case Side.Left:
                rotation.z = -90;
                break;
            case Side.Top:
                rotation.z = 180;
                break;
            case Side.Right:
                rotation.z = 90;
                break;
        }

        hand.gameObject.SetActive(true);
        hand.transform.position = chipA.parentSlot.transform.position;
        hand.transform.eulerAngles = rotation;
        hand.Play();

        yield return new WaitForSeconds(0.5f);

        AnimationAssistant.main.SwapTwoItem(chipA, chipB, true);
        SessionAssistant.main.swapEvent--;
		SessionAssistant.main.movesCount ++;		

        while (hand.isPlaying)
            yield return 0;
		
        hand.gameObject.SetActive(false);
        UIAssistant.main.ShowPage("Field");
	}
}
