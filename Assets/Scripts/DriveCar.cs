using UnityEngine;
using System.Collections;

public class DriveCar : MonoBehaviour {
    private bool active = false;

    public void activate()
    {
        active = true;
    }

    void Update()
    {
        if (!active) return;

        transform.Translate(transform.forward * 0.05f);
    }
}
