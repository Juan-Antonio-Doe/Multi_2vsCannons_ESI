using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : /*MonoBehaviourPun, */MonoBehaviourPunCallbacks, IPunObservable {

    [field: Header("Player Settings")]
    [field: SerializeField] private float speed { get; set; } = 5f;
    [field: SerializeField] private float jumpForce { get; set; } = 8f;
    [field: SerializeField] private int maxHealth { get; set; } = 3;
    [field: SerializeField, ReadOnlyField] private int health { get; set; } = 3;
    [field: SerializeField] private LayerMask groundMask { get; set; }

    //[field: SerializeField] private PhotonView photonView { get; set; }
    [field: SerializeField] private Rigidbody rb { get; set;}
    [field: SerializeField] private MeshRenderer render { get; set; }
    [field: SerializeField] private Material remotePlayerMat { get; set; }
    [field: SerializeField] private CapsuleCollider coll { get; set; }

    private Vector3 moveDirection = Vector3.zero;

    private Vector3 networkPos;

    // End game properties
    [field: SerializeField, ReadOnlyField] private bool isDead { get; set; }
    public bool IsDead { get => isDead; }

    private GameObject victoryUI { get; set; }
    private GameObject gameOverUI { get; set; }

    void Start() {
        gameObject.name = $"_Player_ - {photonView.Owner.NickName}";

        health = maxHealth;

        if (!photonView.IsMine) {
            render.sharedMaterial = remotePlayerMat;
        }

        StartCoroutine(LateComponentsCo());
    }

    void Update() {
        if (photonView.IsMine) {
            if (!isDead) {
                Movement();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                Exit();
            }
        }
        else {
            SyncOtherPlayers();
        }
    }

    private void Movement() {
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= speed;

        rb.MovePosition(rb.position + moveDirection * Time.deltaTime);

        Jump();
    }

    private void Jump() {
        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space)) {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // Enviamos la posición y la rotación del jugador.
            stream.SendNext(transform.position);
            //Debug.Log($"TransformPos: {transform.position}");
        }
        else {
            // Recibimos la posición y la rotación del jugador.
            networkPos = (Vector3)stream.ReceiveNext();
            //Debug.Log($"NetworkPos: {networkPos}");
        }
        //Debug.Log($"IsWriting: {stream.IsWriting}");
    }

    void SyncOtherPlayers() {
        transform.position = Vector3.MoveTowards(transform.position, networkPos, Time.deltaTime * 1000f);
    }

    public void TakeDamage(int damage) {
        photonView.RPC(nameof(RPC_TakeDamage), photonView.Owner, damage);
    }

    [PunRPC]
    private void RPC_TakeDamage(int damage) {
        health -= damage;
        if (health <= 0) {
            isDead = true;
            photonView.RPC(nameof(RPC_Die), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void RPC_Die() {
        if (photonView.IsMine) {
            if (isDead) {
                DisablePlayer();
                gameOverUI.SetActive(true);
            }

            StartCoroutine(ExitRoomCo());
        }
        else {
            DisablePlayer();
            victoryUI.SetActive(true);
        }
    }

    bool IsGrounded() {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
    }

    private void DisablePlayer() {
        render.enabled = false;
        coll.enabled = false;
        transform.GetChild(0).gameObject.SetActive(false);
    }

    void OnGUI() {
        GUI.skin.label.fontSize = 50;

        if (photonView.IsMine) {
            
            if (PhotonNetwork.IsMasterClient) {
                GUI.color = Color.green;
                GUI.Label(MyHealth(), $"Vida: {health}");
            }
            else {
                GUI.color = Color.red;
                GUI.Label(EnemyHealth(), $"Vida: {health}");
            }
        }
        else {
            if (PhotonNetwork.IsMasterClient) {
                GUI.color = Color.red;
                GUI.Label(EnemyHealth(), $"Vida: {health}");
            }
            else {
                GUI.color = Color.green;
                GUI.Label(MyHealth(), $"Vida: {health}");
            }
        }
    }

    Rect MyHealth() {
        return new Rect(10, 10, 200, 100);
    }

    Rect EnemyHealth() {
        return new Rect(Screen.width - 200, 10, 200, 100);
    }

    IEnumerator LateComponentsCo() {
        yield return new WaitForSeconds(1f);

        victoryUI = GameObject.FindGameObjectWithTag("Victory");
        victoryUI = victoryUI.transform.GetChild(0).gameObject;
        gameOverUI = GameObject.FindGameObjectWithTag("GameOver");
        gameOverUI = gameOverUI.transform.GetChild(0).gameObject;
    }

    IEnumerator ExitRoomCo() {
        yield return new WaitForSeconds(5f);

        Exit();
        //SceneManager.LoadScene(0);
    }

    void Exit() {
        PhotonNetwork.LeaveRoom();
        OnLeftRoom();   // Tan listo que no se llama automaticamente. (No llega a llamarse)
    }

    public override void OnLeftRoom() {
        SceneManager.LoadScene(0);
        base.OnLeftRoom();
    }   
}
