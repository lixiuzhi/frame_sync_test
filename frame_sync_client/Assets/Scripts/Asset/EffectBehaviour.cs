using UnityEngine;
using System.Collections;

public class EffectBehaviour : MonoBehaviour {

    public float maxLife = 5;

    public float timeCount = 0;
    // Update is called once per frame
    void Update() {
        timeCount += Time.deltaTime;
        if (timeCount >= maxLife)
        {
            Destroy(gameObject);
        }
    }
}
