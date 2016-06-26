using UnityEngine;
using System.Collections;

public class BoatRock : MonoBehaviour {
    public float step;
    public float offset;
    private Vector3 initialPos;

	void Start () {
        initialPos = transform.position;
        offset = transform.position.y - transform.localScale.y;
	}
	
	void FixedUpdate () {
        step += 0.01f;

        if (step > 999999) step = 1;
        transform.position.Set(transform.position.x, Mathf.Sin(step) + offset, transform.position.z);
	}
}
