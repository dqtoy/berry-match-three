using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//[RequireComponent (typeof (LevelButton))]
[RequireComponent (typeof (Button))]
public class LevelButtonLocker : MonoBehaviour {

    //LevelButton level;
    //Button button;
    //public bool alwaysUnlocked = false; // if true, will be unlocked always
    //public GameObject[] lockedElements; // Elements that appear only when the level is locked
    //public GameObject[] unlockedElements; // Elements that appear only when the level is unlocked


    //void Awake () {
    //    level = GetComponent<LevelButton> ();
    //    button = GetComponent<Button> ();
    //}
    
    //public void Refresh() {
    //    bool l = IsLocked();

    //    foreach (GameObject go in lockedElements)
    //        go.SetActive (l);
    //    foreach(GameObject go in unlockedElements)
    //        go.SetActive (!l);
    //    button.enabled = !l;
    //}

    //public bool IsLocked() {
    //    return !alwaysUnlocked && IsLocked(level);
    //}

    //public static bool IsLocked(LevelButton level) {
    //    return ProfileAssistant.main.local_profile.current_level < level.profile.level; // terms of locking
    
    //}
}
