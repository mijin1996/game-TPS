using UnityEngine;
using UnityEngine.Serialization;

// 수류탄 던지기 — 개수 관리와 투척을 담당 (Player에 부착)
public class PlayerGrenade : MonoBehaviour {
    public GameObject grenadePrefab;
    public Transform throwPoint;  // 수류탄이 생성될 위치 (Player 자식 빈 오브젝트, 이 오브젝트의 앞 방향으로 던짐)
    public float throwForce = 12f; //Force는 rigidBody한테 주는 것이므로 꼭 필요 📌
    public float upwardForce = 4f;
    public int startGrenadeCnt = 3;
    public int maxGrenadeCnt = 50; // 최대 보유 개수 📌

    private PlayerInput playerInput;

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Update() {
        if (playerInput != null && playerInput.grenade && startGrenadeCnt > 0) {
            ThrowGrenade();
        }
    }

    // 수류탄 아이템을 먹었을 때 호출 — 최대 보유 개수를 넘지 않게 더한다
    public void AddGrenade(int amount) {
        startGrenadeCnt = Mathf.Min(startGrenadeCnt + amount, maxGrenadeCnt);
    }

    private void ThrowGrenade() {
        startGrenadeCnt--;

        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();

        // throwPoint가 바라보는 방향(플레이어 앞)으로 던지고, 위로 살짝 띄워 포물선이 되게 한다
        Vector3 throwDirection = throwPoint.forward * throwForce + Vector3.up * upwardForce;
        grenadeRb.AddForce(throwDirection, ForceMode.Impulse);
    }
}
