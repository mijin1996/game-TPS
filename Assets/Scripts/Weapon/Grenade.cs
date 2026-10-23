using UnityEngine;

// 던져진 수류탄 — 일정 시간 뒤 폭발해서 범위 안의 고블린을 모두 죽인다
public class Grenade : MonoBehaviour {
    public float fuseTime = 2.5f;         // 던진 뒤 폭발까지 걸리는 시간
    public float explosionRadius = 6f;    // 폭발 범위
    public float explosionDamage = 9999f; // 고블린 체력보다 크게 → 한 번에 사망
    public LayerMask whatIsGoblin;        // 폭발 피해를 받을 고블린 레이어
    public ParticleSystem explosionEffect;
    public AudioClip grenadeExplosionClip;
    [Range(0f, 1f)] public float explosionVolume = 1f; // 폭발음 크기

    private bool hasExploded = false; // 충돌 폭발과 시간 폭발이 겹쳐도 한 번만 터지게 한다

    private void Start() {
        Invoke(nameof(Explode), fuseTime);
    }

    // 바닥(메시 콜라이더 포함)이나 고블린에 부딪히면 즉시 폭발 (플레이어 자신과의 충돌은 무시)
    private void OnCollisionEnter(Collision collision) {
        if (collision.collider.GetComponentInParent<PlayerHealth>() != null) return;
        Explode();
    }

    private void Explode() {
        if (hasExploded) return;
        hasExploded = true;

        // 폭발 범위 안의 콜라이더를 전부 찾는다
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, whatIsGoblin);

        for (int i = 0; i < hitColliders.Length; i++) {
            LivingEntity goblin = hitColliders[i].GetComponentInParent<LivingEntity>();
            // 콜라이더가 여러 개인 고블린도 한 번만 피해를 받도록 dead 검사
            if (goblin != null && !goblin.dead) {
                Vector3 hitNormal = (goblin.transform.position - transform.position).normalized;
                if (hitNormal == Vector3.zero) hitNormal = Vector3.up; // LookRotation(0) 경고 방지
                goblin.OnDamage(explosionDamage, goblin.transform.position, hitNormal);
            }
        }

        if (explosionEffect != null) {
            // 이펙트 프리팹을 폭발 위치에 새로 생성해서 재생 (수류탄이 사라져도 이펙트는 남음)
            ParticleSystem effect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }
        if (grenadeExplosionClip != null)
            AudioSource.PlayClipAtPoint(grenadeExplosionClip, transform.position, explosionVolume);

        Destroy(gameObject);
    }
}
