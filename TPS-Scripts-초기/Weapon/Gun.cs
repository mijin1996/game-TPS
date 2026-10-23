using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour {
    public enum State {
        Ready,
        Empty,
        Reloading
    }

    public State state { get; private set; }

    public Transform fireTransform;

    public ParticleSystem muzzleFx;
    public ParticleSystem shellFx;

    private LineRenderer bulletLine;

    private AudioSource gunSfx;

    public GunData gunData;

    private float fireDistance = 50f;

    public int ammoRemain = 100;
    public int magAmmo;

    private float lastFireTime;

    private void Awake() {
        gunSfx = GetComponent<AudioSource>();
        bulletLine = GetComponent<LineRenderer>();

        bulletLine.positionCount = 2;

        bulletLine.enabled = false;
    }

    private void OnEnable() {
        ammoRemain = gunData.startAmmoRemain;
        magAmmo = gunData.magCapacity;

        state = State.Ready;
        lastFireTime = 0;
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

        if(Physics.Raycast(fireTransform.position,fireTransform.forward,out hit, fireDistance)) {

            IDamageable target = hit.collider.GetComponent<IDamageable>();

            if(target != null) {
                target.OnDamage(gunData.damage, hit.point, hit.normal);
            }

            hitPosition = hit.point;
        } else {
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        StartCoroutine(ShotEffect(hitPosition));

        magAmmo--;
        if (magAmmo <= 0) {
            state = State.Empty;
        }
    }

    private IEnumerator ShotEffect(Vector3 hitPosition) {
        muzzleFx.Play();
        shellFx.Play();

        gunSfx.PlayOneShot(gunData.gunShotClip);

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
        gunSfx.PlayOneShot(gunData.gunReloadClip);
      
        yield return new WaitForSeconds(gunData.reloadTime);

        int ammoToFill = gunData.magCapacity - magAmmo;

        if(ammoRemain < ammoToFill) {
            ammoToFill = ammoRemain;
        }

        magAmmo += ammoToFill;
        ammoRemain -= ammoToFill;

        state = State.Ready;
    }
}