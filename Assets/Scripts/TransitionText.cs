using UnityEngine;
using System.Collections;

public class TransitionText : MonoBehaviour {
    public void activate()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.up * 0.1f, ForceMode.Impulse);
    }

}
