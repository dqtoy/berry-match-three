using UnityEngine;
using System.Collections;

// Management of the main camera
public class GameCamera : MonoBehaviour {

	public static GameCamera main;
    Vector3 playingPosition;

    Camera cam;
    public bool playing = false;

    void Awake() {
        main = this;
        cam = GetComponent<Camera>();
        UIAssistant.onScreenResize += OnScreenResize;
        OnScreenResize();
    }

    void OnScreenResize() {
        if (Screen.width > Screen.height)
            playingPosition = new Vector3(-0.375f, 0, -10);
        else
            playingPosition = new Vector3(0, -0.034f, -10);

        if (playing) {
            float targetSize = GetTargetSize();
            cam.orthographicSize = targetSize;
            Vector3 targetPosition = playingPosition;
            targetPosition.x *= targetSize * Screen.width / Screen.height;
            targetPosition.y *= targetSize;
            transform.position = targetPosition;
        }

    }

    float GetTargetSize() {
        float width;
        float height;
        if (Screen.width > Screen.height) {
            width = FieldAssistant.main.field.width * 0.6f * Screen.height / Screen.width;
            height = FieldAssistant.main.field.height * 0.37f;
        } else {
            width = FieldAssistant.main.field.width * 0.37f * Screen.height / Screen.width;
            height = FieldAssistant.main.field.height * 0.48f;
        }

        return width > height ? width : height;
    }

	// Switching to the display of the playing field
	public void ShowField (){
		StartCoroutine (ShowFieldRoutine ());
	}

	// Switching to display the game menu
	public void HideField (){
		StartCoroutine (HideFieldRoutine ());
	}
		
	// Coroutine of displaying of field
	public IEnumerator ShowFieldRoutine ()
	{
        if (playing)
            yield break;

        playing = true;

		float t = 0;

        float targetSize = GetTargetSize();
        Vector3 targetPosition = playingPosition;
        targetPosition.x *= targetSize * Screen.width / Screen.height;
        targetPosition.y *= targetSize;

        cam.orthographicSize = targetSize;

        Vector3 position = new Vector3(0,10, -10);
        while (t < 1) {
            t += (-Mathf.Abs(0.5f - t) + 0.5f + 0.05f) * Time.unscaledDeltaTime * 6;
            transform.position = Vector3.Lerp(position, targetPosition, t);
            yield return 0;
        }
	}

	// Coroutine of displaying of game menu
	public IEnumerator HideFieldRoutine () {
        if (!playing)
            yield break;

        playing = false;
        
        float t = 0;

        Vector3 position = transform.position;

        while (t < 1) {
            t += (-Mathf.Abs(0.5f - t) + 0.5f + 0.05f) * Time.unscaledDeltaTime * 6;
            transform.position = Vector3.Lerp(position, new Vector3(0, 10, -10), t);
            yield return 0;
        }


        yield break;
	}
}