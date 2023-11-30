using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CannonManager : MonoBehaviour {

    [field: Header("Cannon Settings")]
    [field: SerializeField] private GameObject cannonBallPrefab { get; set; }
    [field: SerializeField] private float shootInterval { get; set; } = 1f;
    [field: SerializeField] private float cannonBallsLifeTime { get; set; } = 5f;


    [field: SerializeField, ReadOnlyField] private List<Transform> cannons { get; set; } = new List<Transform>();
    private GameObject[] cannonsAux { get; set;} = new GameObject[8];

    private void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage/* && prefabConnected*/) {
            // Variables that will only be checked when they are in a scene
            if (!Application.isPlaying)
                // Assing the cannons on the inspector.
                if (cannons != null && (cannons.Count == 0 || cannons.Count != cannonsAux.Length)) {
                    cannons.Clear();
                    cannonsAux = GameObject.FindGameObjectsWithTag("Cannon");
                    foreach (GameObject cannon in cannonsAux) {
                        cannons.Add(cannon.transform);
                    }
                }

                /*if (waypointsParent != null && (cannons.Count == 0 || cannons.Count != waypointsParent.childCount)) {

                    cannons.Clear();
                    foreach (Transform child in waypointsParent) {
                        cannons.Add(child);
                    }
                }
                else if (waypointsParent == null && cannons.Count > 0) {
                    cannons.Clear();
                }*/
        }
#endif
    }

    void Start() {
        StartCoroutine(ShootCannonBallCo());
    }

    void Update() {
        
    }

    IEnumerator ShootCannonBallCo() {
        yield return new WaitForSeconds(shootInterval);

        // Shoot cannon balls from a offset position from a random cannon's position in the direction of the cannon's forward.
        Transform cannon = cannons[Random.Range(0, cannons.Count)];
        GameObject cannonBall = Instantiate(cannonBallPrefab, cannon.position, cannon.rotation);

        Destroy(cannonBall, cannonBallsLifeTime);

        StartCoroutine(ShootCannonBallCo());
    }
}
