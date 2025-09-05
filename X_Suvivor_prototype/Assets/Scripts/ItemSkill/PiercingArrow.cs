using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PiercingArrow : MonoBehaviour
{
    // PiercingArrow.cs 에 추가/수정
    [Header("비주얼")]
    public Transform graphic;          // 스프라이트가 달린 자식 트랜스폼(없으면 this)
    public float size = 0.2f;            // 비주얼 스케일
    public bool spriteFacesUp = true;
    public float facingOffsetDeg = 0f; // 스프라이트의 기본 각도 보정

    [Header("Bounce Randomness")]
    public float bounceAngleJitterDeg = 15f;   // 반사 직후 ±이 각도만큼 랜덤 회전
    public float bounceSpeedJitter = 0.15f;    // 속도 크기를 (1±이값) 랜덤 배수로
    public float bounceFriction = 0.0f;        // 반사 시 속도 감쇠(0~1, 0.1=10%감쇠)
    public float minSpeedAfterBounce = 3f;     // 너무 느려지는 것 방지

    [Header("옵션")]
    public bool faceVelocity = true;     // 진행 방향으로 스프라이트 회전
    public bool useTrigger = true;       // Collider2D.isTrigger로 관통 처리

    // 내부 상태
    Vector2 velocity;
    int damage;
    float lifeRemain;
    LayerMask enemyMask;

    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        if (col) col.isTrigger = useTrigger;   // 관통용
    }

    /// <summary>
    /// 스폰 직후 호출: 속도/데미지/수명/타겟 레이어 설정
    /// </summary>
    public void Init(Vector2 initialVelocity, int damage, float lifeTime, LayerMask enemyMask)
    {
        this.velocity = initialVelocity;
        this.damage = damage;
        this.lifeRemain = lifeTime;
        this.enemyMask = enemyMask;

        // 그래픽 스케일 적용
        var g = graphic != null ? graphic : transform;
        g.localScale = Vector3.one * Mathf.Max(0.01f, size);

        // 시작 방향 정렬
        SetVisualFacing(this.velocity);
    }

    void Update()
    {
        // 수명
        lifeRemain -= Time.deltaTime;
        if (lifeRemain <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        // 이동
        Vector3 pos = transform.position;
        pos += (Vector3)(velocity * Time.deltaTime);

        // 화면 경계에서 반사
        ReflectOnScreenBounds(ref pos, ref velocity);

        transform.position = pos;

        // 진행 방향으로 회전(옵션)
        if (faceVelocity)
            SetVisualFacing(velocity);
    }

    // 진행방향을 바라보게 설정
    void SetVisualFacing(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;

        if (spriteFacesUp) transform.up = dir;   // 스프라이트 기본이 '위쪽'일 때
        else transform.right = dir;   // 스프라이트 기본이 '오른쪽'일 때

        if (Mathf.Abs(facingOffsetDeg) > 0.001f)
            transform.Rotate(0f, 0f, facingOffsetDeg);
    }

    static Vector2 Rotate(Vector2 v, float deg)
    {
        float r = deg * Mathf.Deg2Rad;
        float cs = Mathf.Cos(r);
        float sn = Mathf.Sin(r);
        return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs);
    }

    // 카메라 뷰 경계에서 튕김
    void ReflectOnScreenBounds(ref Vector3 pos, ref Vector2 vel)
    {
        var cam = Camera.main;
        if (!cam) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        Vector3 c = cam.transform.position;

        float minX = c.x - halfW;
        float maxX = c.x + halfW;
        float minY = c.y - halfH;
        float maxY = c.y + halfH;

        bool hitX = false, hitY = false;

        if (pos.x < minX) { pos.x = minX; vel.x = -vel.x; hitX = true; }
        else if (pos.x > maxX) { pos.x = maxX; vel.x = -vel.x; hitX = true; }

        if (pos.y < minY) { pos.y = minY; vel.y = -vel.y; hitY = true; }
        else if (pos.y > maxY) { pos.y = maxY; vel.y = -vel.y; hitY = true; }

        if (hitX || hitY)
        {
            // 1) 각도 지터(±bounceAngleJitterDeg)
            float jitter = Random.Range(-bounceAngleJitterDeg, bounceAngleJitterDeg);
            vel = Rotate(vel, jitter);

            // 2) 속도 감쇠 + 크기 지터
            float mag = vel.magnitude;
            mag *= (1f - Mathf.Clamp01(bounceFriction));                 // 감쇠
            mag *= Random.Range(1f - bounceSpeedJitter, 1f + bounceSpeedJitter); // 크기 랜덤
            mag = Mathf.Max(minSpeedAfterBounce, mag);
            vel = vel.normalized * mag;

            // 진행 방향 회전 옵션이면 바로 비주얼도 맞춰주기(프레임 지연 방지)
            if (faceVelocity) SetVisualFacing(vel);
        }
    }

    // Enemy에 맞아도 파괴하지 않음(관통)
    void OnTriggerEnter2D(Collider2D other)
    {
        // 레이어 필터 (Enemy)
        if (((1 << other.gameObject.layer) & enemyMask) == 0) return;

        if (other.TryGetComponent<Enemy>(out var e) && e.IsAlive)
        {
            e.ApplyDamage(damage, transform.position, 0f); // 넉백 없음
        }
    }

    // (Trigger가 아닐 때 벽 등과의 충돌 처리용) 현재는 미사용
    void OnCollisionEnter2D(Collision2D collision) { }
}
