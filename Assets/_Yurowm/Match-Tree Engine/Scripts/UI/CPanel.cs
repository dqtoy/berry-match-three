using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// An important element of UI. It combines elements of the interface closest to the destination.
public class CPanel : MonoBehaviour {

    public static int uiAnimation = 0;
    bool _isPlaying = false;
    bool isPlaying {
        get {
            return _isPlaying;
        }
        set {
            if (_isPlaying != value) {
                _isPlaying = value;
                uiAnimation += _isPlaying ? 1 : -1;
            }
        }
    }

	public string hide; // Name of showing animation
	public string show; // Name of hiding animation

	private string currentClip = "";

    Animation anim;
    void Awake() {
        anim = GetComponent<Animation>();
    }

    public void SetVisible(bool visible, bool immediate = false) {
        if (gameObject.activeSelf == visible)
            return;
        currentClip = "";
        if (!visible) {
            if (hide != "")
                currentClip = hide;
            else {
                gameObject.SetActive(false);
                return;
            }
        }
        if (visible) {
            gameObject.SetActive(true);
            if (show != "")
                currentClip = show;
            else
                return;
            Update();
        }
        if (currentClip == "")
            return;
        anim.Play(currentClip);
        anim[currentClip].time = immediate ? anim[currentClip].length : 0;
    }

	// animating the panel regardless of the time settings
	void Update () {
		if (currentClip == "") return;

        anim[currentClip].time += Mathf.Min(Time.unscaledDeltaTime, Time.maximumDeltaTime);
        anim[currentClip].enabled = true;
        anim.Sample();
        anim[currentClip].enabled = false;

        if (anim[currentClip].time >= anim[currentClip].length) {
            if (currentClip == hide)
                gameObject.SetActive(false);
            currentClip = "";
            isPlaying = false;
        } else
            isPlaying = true;
	}
}
	
