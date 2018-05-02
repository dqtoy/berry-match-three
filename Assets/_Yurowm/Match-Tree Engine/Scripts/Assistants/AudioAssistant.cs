using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (AudioListener))]
[RequireComponent (typeof (AudioSource))]
public class AudioAssistant : MonoBehaviour {

	public static AudioAssistant main;

	AudioSource music;
	AudioSource sfx;

	public float musicVolume = 1f;

    public List<MusicTrack> tracks = new List<MusicTrack>();
    public List<Sound> sounds = new List<Sound>();
    Sound GetSoundByName(string name) {
        return sounds.Find(x => x.name == name);
    }

	static List<string> mixBuffer = new List<string>();
	static float mixBufferClearDelay = 0.05f;

    public bool mute = false;
    public bool quiet_mode = false;
    public string currentTrack;

    void Awake() {
        main = this;


        AudioSource[] sources = GetComponents<AudioSource>();
        music = sources[0];
        sfx = sources[1];
    
        StartCoroutine(MixBufferRoutine());

        mute = PlayerPrefs.GetInt("Mute") == 1;
    }

	// Coroutine responsible for limiting the frequency of playing sounds
	IEnumerator MixBufferRoutine() {
        float time = 0;

		while (true) {
            time += Time.unscaledDeltaTime;
            yield return 0;
            if (time >= mixBufferClearDelay) {
			    mixBuffer.Clear();
                time = 0;
            }
		}
	}

	// Launching a music track
    public void PlayMusic(string trackName) {
        if (trackName != "")
            currentTrack = trackName;
		AudioClip to = null;
        foreach (MusicTrack track in tracks)
            if (track.name == trackName)
                to = track.track;
        StartCoroutine(main.CrossFade(to));
	}

	// A smooth transition from one to another music
	IEnumerator CrossFade(AudioClip to) {
		float delay = 0.3f;
		if (music.clip != null) {
			while (delay > 0) {
				music.volume = delay * musicVolume;
				delay -= Time.unscaledDeltaTime;
				yield return 0;
			}
		}
		music.clip = to;
        if (to == null || mute) {
            music.Stop();
            yield break;
        }
        delay = 0;
		if (!music.isPlaying) music.Play();
		while (delay < 0.3f) {
			music.volume = delay * musicVolume;
			delay += Time.unscaledDeltaTime;
			yield return 0;
		}
		music.volume = musicVolume;
	}

	// A single sound effect
	public static void Shot(string clip) {
        Sound sound = main.GetSoundByName(clip);

        if (sound != null && !mixBuffer.Contains(clip)) {
            if (sound.clips.Count == 0) return;
			mixBuffer.Add(clip);
            main.sfx.PlayOneShot(sound.clips[Random.Range(0, sound.clips.Count)]);
		}
	}

    // Turn on/off music
    public void MuteButton() {
        mute = !mute;
        PlayerPrefs.SetInt("Mute", mute ? 1 : 0);
        PlayerPrefs.Save();
        PlayMusic(mute ? "" : currentTrack);
    }

    [System.Serializable]
    public class MusicTrack {
        public string name;
        public AudioClip track;
    }

    [System.Serializable]
    public class Sound {
        public string name;
        public List<AudioClip> clips = new List<AudioClip>();
    }

    //public void BeQuieter(bool p) {
    //    quiet_mode = p;
    //    if (music.clip) {
    //        if (quiet_mode)
    //            music.volume = 0;
    //        else
    //            music.volume = musicVolume;
    //    }
    //}
}
