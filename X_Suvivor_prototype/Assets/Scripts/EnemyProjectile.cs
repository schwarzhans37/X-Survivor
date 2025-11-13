using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class EnemyProjectile : MonoBehaviour
{
    public int damage = 1;
    public float speed = 2f;
    public float lifeTime = 20f;
    public float knockback = 1f;

    public enum OrientationMode { Upright, FaceDirection }

    [Header("Visual Orientation")]
    public OrientationMode orientation = OrientationMode.Upright; // 번개는 Upright

    [Header("SFX on Hit")]
    string _sfxOnHitKey = "";
    float _sfxOnHitVol = 1f;
    float _sfxOnHitPitch = 1f;
    public void SetHitSfx(string key, float vol, float pitch)
    {
        _sfxOnHitKey = key;
        _sfxOnHitVol = vol;
        _sfxOnHitPitch = pitch;
    }

    Animator anim;
    Collider2D col;
    Vector2 dir;
    float despawnAt;
    bool exploding;

    public void Fire(Vector2 direction, string animName)
    {
        anim ??= GetComponent<Animator>();
        col ??= GetComponent<Collider2D>();

        dir = direction.normalized;
        despawnAt = Time.time + lifeTime;
        exploding = false;
        col.enabled = true;

        if (orientation == OrientationMode.FaceDirection)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            transform.rotation = Quaternion.identity; // 항상 세로
        }

        anim.Play(animName, 0, 0f);
    }

    void OnEnable()
    {
        if (orientation == OrientationMode.Upright)
            transform.rotation = Quaternion.identity;
        exploding = false;
    }

    void Update()
    {
        if (!exploding)
            transform.position += (Vector3)(dir * speed * Time.deltaTime);

        if (Time.time >= despawnAt && !exploding)
            StartExplode();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploding) return;

        // 스캐너/무기 히트박스 등 '트리거'는 전부 무시
        if (other.isTrigger) return;

        // 플레이어 몸통(논-트리거)만 히트 판정
        var player = other.GetComponent<Player>() ?? other.GetComponentInParent<Player>();
        if (player == null) return;

        player.TakeDamage(damage);

        // 맞았을 때만 Magic 사운드(키 세팅한 경우)
        if (!string.IsNullOrEmpty(_sfxOnHitKey) && AudioManager.instance != null)
            AudioManager.instance.PlaySfx(_sfxOnHitKey, _sfxOnHitVol, _sfxOnHitPitch);

        StartExplode();
    }

    void StartExplode()
    {
        exploding = true;
        if (col) col.enabled = false;
        if (anim) anim.SetTrigger("StartExplode");
    }

    // Explode 마지막 프레임 이벤트에서 호출
    public void Despawn() => gameObject.SetActive(false);
}
