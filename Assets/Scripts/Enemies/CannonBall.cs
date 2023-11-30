using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonBall : MonoBehaviour {

    [field: Header("Cannon Ball Settings")]
    [field: SerializeField] private float cannonBallSpeed { get; set; } = 10f;

    void Update() {
        transform.Translate(Vector3.forward * cannonBallSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            other.GetComponent<PlayerController>().TakeDamage(1);
            Destroy(gameObject);
        }
    }

    private void OnDestroy() {
        if (gameObject == null)
            return;
    }
}
