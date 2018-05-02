using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent (typeof (Text))]
public class TargetDescriptionDisplay : MonoBehaviour {
	
	Text text;
	
	void Awake () {
		text = GetComponent<Text> ();	
	}
	
	void OnEnable () {
		if (LevelProfile.main == null) {
			text.text = "";
			return;
		}
		string descrition = "";

		switch (LevelProfile.main.target) {
			case FieldTarget.None: descrition += string.Format("You need to reach {0} points.", LevelProfile.main.firstStarScore); break;
			case FieldTarget.Jelly: descrition += string.Format("You need to destroy {0} jellies in this level.", "all"); break;
			case FieldTarget.Block: descrition += string.Format("You need to destroy {0} chocolate blocks in this level.", "all"); break;
			case FieldTarget.Color: descrition += "You need to destroy a certain number of chips of a certain color."; break;
            case FieldTarget.SugarDrop: descrition += "You need to drop all arcon chips."; break;
        }

		descrition += " ";

		switch (LevelProfile.main.limitation) {
		    case Limitation.Moves: descrition += string.Format("You have only {0} moves to accomplish this.", LevelProfile.main.moveCount); break;
		    case Limitation.Time: descrition += string.Format("You have only {0} of time to accomplish this.", Utils.ToTimerFormat(LevelProfile.main.duration)); break;
		}

		text.text = descrition;
	}
}
