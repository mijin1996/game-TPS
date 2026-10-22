using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f;
    public float rotateSpeed = 180f;


    private PlayerInput playerInput;
    private Rigidbody playerRb;
    private Animator playerAni;

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        playerRb = GetComponent<Rigidbody>();
        playerAni = GetComponent<Animator>();
    }

    private void FixedUpdate() {
        Rotate();
        Move();

        playerAni.SetFloat("Move", playerInput.move);
    }

    private void Move() {
        Vector3 moveDir =
            playerInput.move * transform.forward * moveSpeed * Time.deltaTime;
        
        playerRb.MovePosition(playerRb.position + moveDir);
    }

    private void Rotate() {
        float turn = playerInput.rotate * rotateSpeed * Time.deltaTime;

        playerRb.rotation =
            playerRb.rotation * Quaternion.Euler(0, turn, 0f);
    }
}