using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [field: Header("Player Settings")]
    [field: SerializeField] private float speed { get; set; } = 5f;
    [field: SerializeField] private float jumpForce { get; set; } = 8f;
    [field: SerializeField] private int maxHealth { get; set; } = 3;
    [field: SerializeField, ReadOnlyField] private int health { get; set; } = 3;
    [field: SerializeField] private LayerMask groundMask { get; set; }

    [field: SerializeField] private PhotonView photonView { get; set; }
    [field: SerializeField] private Rigidbody rb { get; set;}

    private Vector3 moveDirection = Vector3.zero;

    private Vector3 networkPos { get; set; }
	
    void Start() {
        //gameObject.name = photonView.Owner.NickName;

        health = maxHealth;
    }

    void Update() {
        if (photonView.IsMine) {
            if (health > 0) {
                Movement();
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
        }
        else {
            // Recibimos la posición y la rotación del jugador.
            networkPos = (Vector3)stream.ReceiveNext();

        }
    }

    void SyncOtherPlayers() {
        transform.position = Vector3.MoveTowards(transform.position, networkPos, Time.deltaTime * 5);
    }

    public void TakeDamage(int damage) {
        health -= damage;
        if (health <= 0) {
            Die();
        }
    }

    private void Die() {
        gameObject.SetActive(false);
    }

    bool IsGrounded() {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundMask);
    }
}
