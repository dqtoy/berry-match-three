using UnityEngine;
using System.Collections;

public abstract class BlockInterface : MonoBehaviour {
	public Slot slot;

    abstract public void BlockCrush(bool force);
	abstract public bool CanBeCrushedByNearSlot ();
}
