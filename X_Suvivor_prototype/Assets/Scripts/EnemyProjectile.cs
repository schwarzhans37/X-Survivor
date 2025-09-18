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
        if (!other.CompareTag("Player")) return;

        var p = other.GetComponent<Player>();
        if (p) p.TakeDamage(damage);

        StartExplode();
    }

    void StartExplode()
    {
        exploding = true;
        col.enabled = false;
        anim.SetTrigger("StartExplode");
    }

    // Explode 마지막 프레임 이벤트에서 호출
    public void Despawn() => gameObject.SetActive(false);
}
