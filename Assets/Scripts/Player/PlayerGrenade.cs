using System;
using UnityEngine;

// 수류탄 던지기 — 개수 관리와 투척을 담당 (Player에 부착)
public class PlayerGrenade : MonoBehaviour {
    public GameObject grenadePrefab;
    public Transform throwPoint;  // 수류탄이 생성될 위치 (Player 자식 빈 오브젝트, 이 오브젝트의 앞 방향으로 던짐)
    public float throwForce = 12f; //Force는 rigidBody한테 주는 것이므로 꼭 필요 📌
    public float upwardForce = 4f;
    public int startGrenadeCnt = 3;
    public int maxGrenadeCnt = 50; // 최대 보유 개수 📌

    private PlayerInput playerInput;

    // ---- 알림(이벤트): 보유 수류탄 수가 바뀜 ----
    public static event Action<int> onGrenadeCntChanged;

    private void Start() {
        playerInput = GetComponent<PlayerInput>();
        onGrenadeCntChanged?.Invoke(startGrenadeCnt);
    }

    private void Update() {
        if (playerInput != null && playerInput.grenade && startGrenadeCnt > 0) {
            ThrowGrenade();
        }
    }

    // 수류탄 아이템을 먹었을 때 호출 — 최대 보유 개수를 넘지 않게 더한다
    public void AddGrenade(int amount) {
        startGrenadeCnt = Mathf.Min(startGrenadeCnt + amount, maxGrenadeCnt);
        onGrenadeCntChanged?.Invoke(startGrenadeCnt);
    }

    private void ThrowGrenade() {
        startGrenadeCnt--;
        onGrenadeCntChanged?.Invoke(startGrenadeCnt);

        GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();

        // throwPoint가 바라보는 방향(플레이어 앞)으로 던지고, 위로 살짝 띄워 포물선이 되게 한다
        Vector3 throwDirection = throwPoint.forward * throwForce + Vector3.up * upwardForce;
        grenadeRb.AddForce(throwDirection, ForceMode.Impulse);
    }
}
