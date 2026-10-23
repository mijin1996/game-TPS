using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour {
    public float moveSpeed = 5f;
    //public float rotateSpeed = 180f;
    private PlayerInput playerInput;
    private Rigidbody playerRb;
    private Animator playerAni;

    public float jumpForce = 5f; 
    public int maxJumpCnt = 3;
    private int jumpCnt = 0;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.5f;

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        playerRb = GetComponent<Rigidbody>();
        playerAni = GetComponent<Animator>();
    }

    private void FixedUpdate() {
        Move();
        playerAni.SetFloat("Move-FrontBack", playerInput.move);
        playerAni.SetFloat("Move-RightLeft", playerInput.rotate);

        bool grounded = IsGrounded();
        playerAni.SetBool("Ground", grounded);

        if (grounded && playerRb.linearVelocity.y <= 0.1f) {
            jumpCnt = 0;
        }
        if (playerInput.jump) {
            playerInput.ConsumeJump();
            if (jumpCnt < maxJumpCnt) {
                playerRb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                playerAni.SetTrigger("Jump");
                jumpCnt++;
            }
        }
    }


    private bool IsGrounded() {
        Vector3 origin = groundCheck.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckRadius + 0.1f, groundLayer);
    }


    // ↕️ : 상대적으로 이동할 거리 계산; rb를 이용해 게임 오브젝 위치 변경;
    private void Move() {
        Vector3 moveDir = (playerRb.rotation * Vector3.forward * playerInput.move + playerRb.rotation * Vector3.right * playerInput.rotate) * moveSpeed;
        Vector3 currentVel = playerRb.linearVelocity;
        playerRb.linearVelocity = new Vector3(moveDir.x, currentVel.y, moveDir.z);
    }


}
