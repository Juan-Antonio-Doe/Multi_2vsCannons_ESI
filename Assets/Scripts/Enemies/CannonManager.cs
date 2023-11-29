using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CannonManager : MonoBehaviour {

    [field: Header("Cannon Settings")]
    [field: SerializeField] private List<Transform> cannons { get; set; } = new List<Transform>();

    private void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage/* && prefabConnected*/) {
            // Variables that will only be checked when they are in a scene
            if (!Application.isPlaying)
                // Assing the cannons on the inspector.
                if (cannons != null && (cannons.Count == 0)) {
                    cannons.Clear();
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
        
    }

    void Update() {
        
    }
}
