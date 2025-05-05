using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;  // 무기 ID
    public int prefabId;    // 무기의 프리팹 ID
    public float damage;
    public int count;   // 무기 배치 개수
    public float attackSpeed; // 공격속도

    float timer;

    Player player;

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        switch (id) {
            case 0:
                transform.Rotate(Vector3.back * attackSpeed * Time.deltaTime);
                break;
            default:
                timer += Time.deltaTime;

                if (timer > attackSpeed) {
                    timer = 0f;
                    Fire();
                }

                attackSpeed = 0.5f;
                break;
        }

        // .. 테스트용 레벨업
        if (Input.GetButtonDown("Jump")) {
            LevelUp(20, 5);
        }
    }

    public void LevelUp(float damage, int count)
    {
        this.damage = damage;
        this.count  += count;

        if (id == 0)
            Batch();
    }

    public void Init()
    {
        switch (id) {
            case 0:
                attackSpeed = 300;
                Batch();
                break;
            default:
                attackSpeed = 0.3f;
                break;
        }
    }

    void Batch()
    {
        for (int index = 0; index < count; index++) {
            Transform bullet;

            if (index < transform.childCount) {
                bullet = transform.GetChild(index);
            }
            else {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;        //위치 초기화(플레이어 위치로)
            bullet.localRotation = Quaternion.identity;  //회전 초기화

            Vector3 rotVec = Vector3.forward * 360 * index / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);
            bullet.GetComponent<Bullet>().Init(damage, -1,Vector3.zero);     // -1은 여기서는 무한(-1)으로 관통하게끔 넣음
        }
    }

    void Fire() 
    {
        if (!player.scanner.nearestTarget)
            return;
        
        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.GetComponent<Bullet>().Init(damage, count, dir);
    }
}
