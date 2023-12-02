using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CannonManager : MonoBehaviour {

    [field: Header("Cannon Settings")]
    [field: SerializeField] private GameObject cannonBallPrefab { get; set; }
    [field: SerializeField] private float shootInterval { get; set; } = 1f;
    [field: SerializeField] private float cannonBallsLifeTime { get; set; } = 5f;


    [field: SerializeField, ReadOnlyField] private List<Transform> cannons { get; set; } = new List<Transform>();
    private GameObject[] cannonsAux { get; set;} = new GameObject[8];

    [field: SerializeField] private GameObject victoryUI { get; set; }
    [field: SerializeField] private GameObject gameOverUI { get; set; }
    private bool courroutineStopped { get; set; } = true;

    [field: Header("Debug")]
    [field: SerializeField] private bool debug { get; set; }

    private bool exiting { get; set; }

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
        }
#endif
    }

    void Start() {
        courroutineStopped = false;
        StartCoroutine(ShootCannonBallCo());
    }

    void Update() {
        if ((victoryUI.activeSelf || gameOverUI.activeSelf) && !courroutineStopped) {
            courroutineStopped = true;
            StopAllCoroutines();
        }

        // Force the players to leave the room when the game is over. (Porque Photon se rie en mi cara...)
        if (PhotonNetwork.CurrentRoom != null) {
            if (PhotonNetwork.CurrentRoom.PlayerCount >= 0 && (victoryUI.activeSelf || gameOverUI.activeSelf) && !exiting) {
                exiting = true;
                StartCoroutine(BruteForceExit());
            }
        }
        
    }

    IEnumerator ShootCannonBallCo() {
        yield return new WaitForSeconds(shootInterval);

        // Shoot cannon balls from a offset position from a random cannon's position in the direction of the cannon's forward.
        Transform cannon;

        if (!debug)
            cannon = cannons[Random.Range(0, cannons.Count)];
        else
            cannon = cannons[4];

        if (!debug) {
            GameObject cannonBall = Instantiate(cannonBallPrefab, cannon.position, cannon.rotation);

            Destroy(cannonBall, cannonBallsLifeTime);
        }
        

        if (!courroutineStopped || (PhotonNetwork.CurrentRoom.PlayerCount > 0))
            StartCoroutine(ShootCannonBallCo());
    }

    IEnumerator BruteForceExit() {
        yield return new WaitForSeconds(0.5f);

        PhotonNetwork.LeaveRoom();

        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene(0);
    }
}
