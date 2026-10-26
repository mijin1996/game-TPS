using UnityEngine;

public class PlayerShooter : MonoBehaviour {
    public Gun gun;
    public Transform gunPivot;
    public Transform leftHandMount;
    public Transform rightHandMount;
    private PlayerInput playerInput;
    private Animator playerAni;

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        playerAni = GetComponent<Animator>();
    }

    private void OnEnable() {
        gun.gameObject.SetActive(true);
    }

    private void OnDisable() {
        gun.gameObject.SetActive(false);
    }

    private void Update() {
        if (playerInput.fire) {
            gun.Fire();
        } else if (playerInput.reload) {
            if (gun.Reload()) {
                playerAni.SetTrigger("Reload");
            }
        }
    }

    private void OnAnimatorIK(int layerIndex) {
        //총 기준  gunPivot을 player의 오른쪽 팔꿈치 위치에 이동
        gunPivot.position = playerAni.GetIKHintPosition(AvatarIKHint.RightElbow);
        // ik를 사용해 왼손의 위치와 회전을 총의 오른쪽 손잡이에 맞춤
        playerAni.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAni.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAni.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        playerAni.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);
        // 오른손에 맞춤
        playerAni.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAni.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAni.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        playerAni.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);
    }
}

/*
 [📌📝MEMO] p.784
Mount: 장착이라는 의미


*/
