using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Goblin : LivingEntity {
    public LayerMask whatIsTarget;

    private LivingEntity targetEntity;
    private NavMeshAgent navAgent;

    public ParticleSystem hitFx;
    public AudioClip goblinDeathClip;
    public AudioClip goblinHitClip;
    public AudioClip goblinAttackClip;

    private Animator goblinAni;
    private AudioSource goblinSource;
    private Renderer goblinRenderer;

    public float damage = 20f;
    public float timeBetAttack = 0.5f;
    private float lastAttackTime;

    public GameObject auraFx;  // 바닥에서 발생하게 될 이펙트
    private GameObject aura;

    private bool isPlayerDead;   // 게임오버 알림을 받으면 true

    private bool hasTarget {
        get {
            if (targetEntity != null && !targetEntity.dead) {
                return true;
            }

            return false;
        }
    }
    private void Awake() {
        navAgent = GetComponent<NavMeshAgent>();
        goblinAni = GetComponent<Animator>();
        goblinSource = GetComponent<AudioSource>();

        goblinRenderer = GetComponentInChildren<Renderer>();
    }

    public void Setup(GoblinData goblinData) {
        startingHealth = goblinData.health;
        health = goblinData.health;
        damage = goblinData.damage;
        navAgent.speed = goblinData.speed;
        goblinRenderer.material.color = goblinData.skinColor;

        // 원본 프리팹에서 바로 생성해서 고블린을 따라다니도록 자식으로 붙임
        if (auraFx != null) {
            aura = Instantiate(auraFx, transform);
            foreach (ParticleSystem ps in aura.GetComponentsInChildren<ParticleSystem>()) {
                var main = ps.main;
                main.loop = true;
            }
        }
    }

    // ---- 듣기(구독): 게임오버가 되면 멈춤 ----
    protected override void OnEnable() {
        base.OnEnable();
        GM.onGameOver += OnGameOver;
    }

    private void OnDisable() {
        GM.onGameOver -= OnGameOver;
    }

    private void OnGameOver() {
        isPlayerDead = true;
        if (!dead && navAgent.enabled) {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
        }
    }

    private void Start() {
        StartCoroutine(UpdatePath());
    }

    private void Update() {
        goblinAni.SetBool("HasTarget", hasTarget && !isPlayerDead);   // 플레이어가 죽으면 대기 동작으로
    }

    private IEnumerator UpdatePath() {
        while (!dead) {
            if (isPlayerDead) yield break;   // 게임오버면 길 찾기 중단 (멈추는 건 OnGameOver에서)

            if(hasTarget) {
                navAgent.isStopped = false;
                navAgent.SetDestination(targetEntity.transform.position);
            } else {
                navAgent.isStopped = true;

                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);
                
                for (int i = 0; i < colliders.Length; i++) {
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    if (livingEntity != null && !livingEntity.dead) {
                        targetEntity = livingEntity;

                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.25f);
        }
    }

    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal) {
        if(!dead) {
            ParticleSystem blood = Instantiate(hitFx, hitPoint, Quaternion.LookRotation(hitNormal));
            blood.Play();
            Destroy(blood.gameObject, 1f);

            if (goblinHitClip != null) goblinSource.PlayOneShot(goblinHitClip);
        }
        
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    public override void Die() {
        base.Die();

        Collider[] goblinColliders = GetComponents<Collider>();
        for (int i = 0; i < goblinColliders.Length; i++) {
            goblinColliders[i].enabled = false;
        }

        navAgent.isStopped = true;
        navAgent.enabled = false;

        goblinAni.SetTrigger("Die");
        goblinSource.PlayOneShot(goblinDeathClip);
        Destroy(aura);
    }

    private void OnTriggerStay(Collider other) {
        if (isPlayerDead) return;   // 플레이어가 죽으면 공격도, 공격 소리도 없음
        if(!dead && Time.time >= lastAttackTime + timeBetAttack) {
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();

            if(attackTarget != null && attackTarget == targetEntity) {
                lastAttackTime = Time.time;
                goblinAni.SetTrigger("Attack");
                if (goblinAttackClip != null) goblinSource.PlayOneShot(goblinAttackClip);

                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }
            
        }
    }
}