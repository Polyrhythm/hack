using UnityEngine;
using System.Collections;

public class TransitionUp : MonoBehaviour {
    private bool move = false;
    public GameObject text;

    public void Start()
    {
        StartCoroutine(MoveDown());
        StartCoroutine(ActivateText());
    }

    IEnumerator ActivateText()
    {
        yield return new WaitForSeconds(2);
        text.GetComponent<TransitionText>().activate();
    }

    IEnumerator MoveDown()
    {
        yield return new WaitForSeconds(8);
        move = true;
    }

    void Update()
    {
        if (!move) return;
        transform.position -= transform.up * 0.075f;
    }
}
