using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour {
    public enum State {
        Ready,
        Empty,
        Reloading
    }

    public State state { get; private set; }

    // ---- 알림(이벤트): 탄알 수가 바뀜 (탄창, 남은 탄알) ----
    public static event Action<int, int> onAmmoChanged;

    public Transform fireTransform;

    public ParticleSystem muzzleFx;
    public ParticleSystem shellFx;

    private LineRenderer bulletLine;

    private AudioSource gunSource;

    public GunData gunData;

    private float fireDistance = 50f;

    public int ammoRemain = 100;
    public int magAmmo;

    private float lastFireTime;

    public Camera scopeCam;   // 스코프를 켰을 때 조준 기준 카메라 (꺼져 있으면 메인 카메라 기준)

    private void Awake() {
        gunSource = GetComponent<AudioSource>();
        bulletLine = GetComponent<LineRenderer>();

        bulletLine.positionCount = 2;

        bulletLine.enabled = false;
    }

    private void OnEnable() {
        ammoRemain = gunData.startAmmoRemain;
        magAmmo = gunData.magCapacity;

        state = State.Ready;
        lastFireTime = 0;
        NotifyAmmo();
    }

    private void Start() {
        NotifyAmmo();   // 처음 탄알 수를 UI에 알림
    }

    private void NotifyAmmo() {
        onAmmoChanged?.Invoke(magAmmo, ammoRemain);
    }

    // 탄약 아이템을 먹었을 때
    public void AddAmmo(int amount) {
        ammoRemain += amount;
        NotifyAmmo();
    }

    public void Fire() {
        if (state == State.Ready && Time.time >= lastFireTime + gunData.timeBetFire) {
            lastFireTime = Time.time;
            Shot();
        }
    }

    private void Shot() {
        RaycastHit hit;
        Vector3 hitPosition = Vector3.zero;

        Vector3 fireDir = fireTransform.forward;   // 평소: 총구 방향

        // 화면 가운데(조준점)가 가리키는 곳으로 쏨: 스코프를 켰으면 스코프 카메라, 아니면 메인 카메라 기준
        Camera aimCam = (scopeCam != null && scopeCam.gameObject.activeInHierarchy) ? scopeCam : Camera.main;
        if (aimCam != null) {
            Ray aimRay = aimCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            int mask = ~LayerMask.GetMask("Player");   // ~ : Player 레이어만 빼고
            RaycastHit aimHit;
            Vector3 aimPoint = Physics.Raycast(aimRay, out aimHit, fireDistance, mask) ? aimHit.point : aimRay.GetPoint(fireDistance);
            fireDir = (aimPoint - fireTransform.position).normalized;
        }

        if (Physics.Raycast(fireTransform.position, fireDir, out hit, fireDistance)) {

            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if(target != null) {
                target.OnDamage(gunData.damage, hit.point, hit.normal);
            }

            hitPosition = hit.point;
        } else {
            hitPosition = fireTransform.position + fireDir * fireDistance;
        }

        StartCoroutine(ShotEffect(hitPosition));

        magAmmo--;
        if (magAmmo <= 0) {
            state = State.Empty;
        }
        NotifyAmmo();
    }

    private IEnumerator ShotEffect(Vector3 hitPosition) {
        muzzleFx.Play();
        shellFx.Play();

        gunSource.PlayOneShot(gunData.gunShotClip);

        bulletLine.SetPosition(0, fireTransform.position);
        bulletLine.SetPosition(1, hitPosition);
        bulletLine.enabled = true;

        yield return new WaitForSeconds(0.03f);

        bulletLine.enabled = false;
    }

    public bool Reload() {
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= gunData.magCapacity) {
            return false;
        }

        StartCoroutine(ReloadRoutine());
        return true;
    }

    private IEnumerator ReloadRoutine() {
        state = State.Reloading;
        gunSource.PlayOneShot(gunData.gunReloadClip);
      
        yield return new WaitForSeconds(gunData.reloadTime);

        int ammoToFill = gunData.magCapacity - magAmmo;

        if(ammoRemain < ammoToFill) {
            ammoToFill = ammoRemain;
        }

        magAmmo += ammoToFill;
        ammoRemain -= ammoToFill;

        state = State.Ready;
        NotifyAmmo();
    }
}