using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBallDestroyer : MonoBehaviour {

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("CannonBall")) {
            Destroy(other.gameObject);
        }
    }
}
