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
        if(playerInput.fire) {
            gun.Fire();
        } else if (playerInput.reload) {
            if(gun.Reload()) {
                playerAni.SetTrigger("Reload");
            }
        }

        UpdateUI();
    }

    private void UpdateUI() {
        if (gun != null && UIM.ins != null) {
            UIM.ins.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);
        }
    }

    private void OnAnimatorIK(int layerIndex) {
        gunPivot.position = playerAni.GetIKHintPosition(AvatarIKHint.RightElbow);

        playerAni.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        playerAni.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);

        playerAni.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        playerAni.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        playerAni.SetIKPositionWeight(AvatarIKGoal.RightHand, 1.0f);
        playerAni.SetIKRotationWeight(AvatarIKGoal.RightHand, 1.0f);

        playerAni.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        playerAni.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);

    }
}