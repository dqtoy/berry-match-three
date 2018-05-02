using System.Collections;
using System.Text;
using System;
using UnityEngine;
using System.Collections.Generic;

// A set of useful functions
public class Utils {

    public static readonly Side[] allSides = {
                                                 Side.Top,
                                                 Side.TopRight,
                                                 Side.Right,
                                                 Side.BottomRight,
                                                 Side.Bottom,
                                                 Side.BottomLeft,
                                                 Side.Left,
                                                 Side.TopLeft
                                             };

	public static readonly Side[] straightSides = {Side.Top, Side.Bottom, Side.Right, Side.Left};
	public static readonly Side[] slantedSides = {Side.TopLeft, Side.TopRight, Side.BottomRight ,Side.BottomLeft};

    public static string waitingStatus = "";

    public static Side RotateSide(Side side, int steps) {
        int index = Array.IndexOf(allSides, side);
        index += steps;
        index = Mathf.CeilToInt(Mathf.Repeat(index, allSides.Length));
        return allSides[index];
    }

	public static Side MirrorSide(Side s) {
		switch (s) {
		case Side.Bottom: return Side.Top;
		case Side.Top: return Side.Bottom;
		case Side.Left: return Side.Right;
		case Side.Right: return Side.Left;
		case Side.BottomLeft: return Side.TopRight;
		case Side.BottomRight: return Side.TopLeft;
		case Side.TopLeft: return Side.BottomRight;
		case Side.TopRight: return Side.BottomLeft;
		}
		return Side.Null;
	}

	public static int SideOffsetX (Side s) {
		switch (s) {
		case Side.Top:
		case Side.Bottom: 
			return 0;
		case Side.TopLeft:
		case Side.BottomLeft:
		case Side.Left: 
			return -1;
		case Side.BottomRight:
		case Side.TopRight:
		case Side.Right: 
			return 1;
		}
		return 0;
	}
	
	public static int SideOffsetY (Side s) {
		switch (s) {
		case Side.Left: 
		case Side.Right: 
			return 0;
		case Side.Bottom: 
		case Side.BottomRight:
		case Side.BottomLeft:
			return -1;
		case Side.TopLeft:
		case Side.TopRight:
		case Side.Top:
			return 1;
		}
		return 0;
	}

	public static Side SideHorizontal (Side s) {
		switch (s) {
		case Side.Left: 
		case Side.TopLeft:
		case Side.BottomLeft:
			return Side.Left;
		case Side.Right:
		case Side.TopRight:
		case Side.BottomRight:
			return Side.Right;
		default:
			return Side.Null;
		}
	}

	public static Side SideVertical (Side s) {
		switch (s) {
		case Side.Top: 
		case Side.TopLeft:
		case Side.TopRight:
			return Side.Top;
		case Side.Bottom:
		case Side.BottomLeft:
		case Side.BottomRight:
			return Side.Bottom;
		default:
			return Side.Null;
		}
	}

	public static string StringReplaceAt(string value, int index, char newchar)
	{
		if (value.Length <= index)
			return value;
		StringBuilder sb = new StringBuilder(value);
		sb[index] = newchar;
		return sb.ToString();
	}

	// Coroutine wait until the function "Action" will be true for a "delay" seconds
	public static IEnumerator WaitFor (Func<bool> Action, float delay) {
		float time = 0;
		while (time <= delay) {
			if (Action())
				time += Time.deltaTime;
			else
				time = 0;
			yield return 0;
		}
		yield break;
	}

    public static string GenerateKey(int length) {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        StringBuilder res = new StringBuilder();
        System.Random rnd = new System.Random();
        while (0 < length--) {
            res.Append(valid[rnd.Next(valid.Length)]);
        }
        return res.ToString();
    }

    public static string ToTimerFormat(float _t) {
        string f = "";
        float t = Mathf.Ceil(_t);
        float min = Mathf.FloorToInt(t / 60);
        float sec = Mathf.FloorToInt(t - 60f * min);
        f += min.ToString();
        if (f.Length < 2)
            f = "0" + f;
        f += ":";
        if (sec.ToString().Length < 2)
            f += "0";
        f += sec.ToString();
        return f;
    }

    public static object GetDataValueForKey(Dictionary<string, object> dict, string key) {
        object objectForKey;
        if (dict.TryGetValue(key, out objectForKey)) {
            return objectForKey;
        } else {
            return null;
        }
    }

    public static Vector2 Vector3to2(Vector3 vector3) {
        return new Vector2(vector3.x, vector3.y);
    }
}

class EasingFunctions {
    // no easing, no acceleration
    public static float linear(float t) {
        return t;
    }
    // accelerating from zero velocity
    public static float easeInQuad(float t) {
        return t * t;
    }
    // decelerating to zero velocity
    public static float easeOutQuad(float t) {
        return t * (2 - t);
    }
    // acceleration until halfway, then deceleration
    public static float easeInOutQuad(float t) {
        return t < .5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
    // accelerating from zero velocity 
    public static float easeInCubic(float t) {
        return t * t * t;
    }
    // decelerating to zero velocity 
    public static float easeOutCubic(float t) {
        return (--t) * t * t + 1;
    }
    // acceleration until halfway, then deceleration 
    public static float easeInOutCubic(float t) {
        return t < .5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
    }
    // accelerating from zero velocity 
    public static float easeInQuart(float t) {
        return t * t * t * t;
    }
    // decelerating to zero velocity 
    public static float easeOutQuart(float t) {
        return 1 - (--t) * t * t * t;
    }
    // acceleration until halfway, then deceleration
    public static float easeInOutQuart(float t) {
        return t < .5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
    }
    // accelerating from zero velocity
    public static float easeInQuint(float t) {
        return t * t * t * t * t;
    }
    // decelerating to zero velocity
    public static float easeOutQuint(float t) {
        return 1 + (--t) * t * t * t * t;
    }
    // acceleration until halfway, then deceleration 
    public static float easeInOutQuint(float t) {
        return t < .5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
    }
}

public class FacebookUtils {
    public static string GetPictureURL(string facebookID, int? width = null, int? height = null, string type = null) {
        string url = string.Format("/{0}/picture", facebookID);
        string query = width != null ? "&width=" + width.ToString() : "";
        query += height != null ? "&height=" + height.ToString() : "";
        query += type != null ? "&type=" + type : "";
        query += "&redirect=false";
        if (query != "")
            url += ("?g" + query);
        return url;
    }


    public static Dictionary<string, string> RandomFriend(List<object> friends) {
        var fd = ((Dictionary<string, object>) (friends[UnityEngine.Random.Range(0, friends.Count)]));
        var friend = new Dictionary<string, string>();
        friend["id"] = (string) fd["id"];
        friend["first_name"] = (string) fd["first_name"];
        var pictureDict = ((Dictionary<string, object>) (fd["picture"]));
        var pictureDataDict = ((Dictionary<string, object>) (pictureDict["data"]));
        friend["image_url"] = (string) pictureDataDict["url"];
        return friend;
    }

    public static void DrawActualSizeTexture(Vector2 pos, Texture texture, float scale = 1.0f) {
        Rect rect = new Rect(pos.x, pos.y, texture.width * scale, texture.height * scale);
        GUI.DrawTexture(rect, texture);
    }
    public static void DrawSimpleText(Vector2 pos, GUIStyle style, string text) {
        Rect rect = new Rect(pos.x, pos.y, Screen.width, Screen.height);
        GUI.Label(rect, text, style);
    }

    private static void JavascriptLog(string msg) {
        Application.ExternalCall("console.log", msg);
    }

    public static void Log(string message) {
        Debug.Log(message);
        if (Application.isWebPlayer)
            JavascriptLog(message);
    }
    public static void LogError(string message) {
        Debug.LogError(message);
        if (Application.isWebPlayer)
            JavascriptLog(message);
    }

}


// Directions. Used as an index for links to neighboring slots.
public enum Side {
	Null, Top, Bottom, Right, Left,
	TopRight, TopLeft,
	BottomRight, BottomLeft
}

public struct int2 {
	public int x;
	public int y;
}

[Serializable]
public class Pair {
    public string a;
    public string b;

    public Pair(string pa, string pb) {
        a = pa;
        b = pb;
    }


    public static bool operator ==(Pair a, Pair b) {
        return Equals(a, b);
    }
    public static bool operator !=(Pair a, Pair b) {
        return !Equals(a, b);
    }

    public override bool Equals(object obj) {
        Pair sec = (Pair) obj;
        return (a == sec.a && b == sec.b) ||
            (a == sec.b && b == sec.a);
    }

    public override int GetHashCode() {
        return a.GetHashCode() + b.GetHashCode();
    }
}

[System.Serializable]
public class Coord {
    public int x = 0;
    public int y = 0;

    public Coord(int x, int y) {
        this.x = x;
        this.y = y;
    }
}


[System.Serializable]
public struct HSBColor {
    public float h;
    public float s;
    public float b;
    public float a;

    public HSBColor(float h, float s, float b, float a) {
        this.h = h;
        this.s = s;
        this.b = b;
        this.a = a;
    }

    public HSBColor(float h, float s, float b) {
        this.h = h;
        this.s = s;
        this.b = b;
        this.a = 1f;
    }

    public HSBColor(Color col) {
        HSBColor temp = FromColor(col);
        h = temp.h;
        s = temp.s;
        b = temp.b;
        a = temp.a;
    }

    public static HSBColor FromColor(Color color) {
        HSBColor ret = new HSBColor(0f, 0f, 0f, color.a);

        float r = color.r;
        float g = color.g;
        float b = color.b;

        float max = Mathf.Max(r, Mathf.Max(g, b));

        if (max <= 0) {
            return ret;
        }

        float min = Mathf.Min(r, Mathf.Min(g, b));
        float dif = max - min;

        if (max > min) {
            if (g == max) {
                ret.h = (b - r) / dif * 60f + 120f;
            } else if (b == max) {
                ret.h = (r - g) / dif * 60f + 240f;
            } else if (b > g) {
                ret.h = (g - b) / dif * 60f + 360f;
            } else {
                ret.h = (g - b) / dif * 60f;
            }
            if (ret.h < 0) {
                ret.h = ret.h + 360f;
            }
        } else {
            ret.h = 0;
        }

        ret.h *= 1f / 360f;
        ret.s = (dif / max) * 1f;
        ret.b = max;

        return ret;
    }

    public static Color ToColor(HSBColor hsbColor) {
        float r = hsbColor.b;
        float g = hsbColor.b;
        float b = hsbColor.b;
        if (hsbColor.s != 0) {
            float max = hsbColor.b;
            float dif = hsbColor.b * hsbColor.s;
            float min = hsbColor.b - dif;

            float h = hsbColor.h * 360f;

            if (h < 60f) {
                r = max;
                g = h * dif / 60f + min;
                b = min;
            } else if (h < 120f) {
                r = -(h - 120f) * dif / 60f + min;
                g = max;
                b = min;
            } else if (h < 180f) {
                r = min;
                g = max;
                b = (h - 120f) * dif / 60f + min;
            } else if (h < 240f) {
                r = min;
                g = -(h - 240f) * dif / 60f + min;
                b = max;
            } else if (h < 300f) {
                r = (h - 240f) * dif / 60f + min;
                g = min;
                b = max;
            } else if (h <= 360f) {
                r = max;
                g = min;
                b = -(h - 360f) * dif / 60 + min;
            } else {
                r = 0;
                g = 0;
                b = 0;
            }
        }

        return new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), hsbColor.a);
    }

    public Color ToColor() {
        return ToColor(this);
    }

    public override string ToString() {
        return "H:" + h + " S:" + s + " B:" + b;
    }

    public static HSBColor Lerp(HSBColor a, HSBColor b, float t) {
        float h, s;

        //check special case black (color.b==0): interpolate neither hue nor saturation!
        //check special case grey (color.s==0): don't interpolate hue!
        if (a.b == 0) {
            h = b.h;
            s = b.s;
        } else if (b.b == 0) {
            h = a.h;
            s = a.s;
        } else {
            if (a.s == 0) {
                h = b.h;
            } else if (b.s == 0) {
                h = a.h;
            } else {
                // works around bug with LerpAngle
                float angle = Mathf.LerpAngle(a.h * 360f, b.h * 360f, t);
                while (angle < 0f)
                    angle += 360f;
                while (angle > 360f)
                    angle -= 360f;
                h = angle / 360f;
            }
            s = Mathf.Lerp(a.s, b.s, t);
        }
        return new HSBColor(h, s, Mathf.Lerp(a.b, b.b, t), Mathf.Lerp(a.a, b.a, t));
    }

    public static void Test() {
        HSBColor color;

        color = new HSBColor(Color.red);
        Debug.Log("red: " + color);

        color = new HSBColor(Color.green);
        Debug.Log("green: " + color);

        color = new HSBColor(Color.blue);
        Debug.Log("blue: " + color);

        color = new HSBColor(Color.grey);
        Debug.Log("grey: " + color);

        color = new HSBColor(Color.white);
        Debug.Log("white: " + color);

        color = new HSBColor(new Color(0.4f, 1f, 0.84f, 1f));
        Debug.Log("0.4, 1f, 0.84: " + color);

        Debug.Log("164,82,84   .... 0.643137f, 0.321568f, 0.329411f  :" + ToColor(new HSBColor(new Color(0.643137f, 0.321568f, 0.329411f))));
    }
}