using System.Collections.Generic;
using UnityEngine;

public class MonsterDatabase : MonoBehaviour
{
    public List<MonsterData> monsters;
    public RuntimeAnimatorController[] animCon;

    void Awake()
    {
        LoadMonsterData();
        Debug.Log("[MonsterDatabase] Awake���� ������ �ε� �Ϸ�");
    }

    void LoadMonsterData()
    {
        TextAsset csv = Resources.Load<TextAsset>("MonsterData");
        string[] lines = csv.text.Split('\n');
        monsters = new List<MonsterData>();

        for (int i = 1; i < lines.Length; i++) // ù ���� ����ϱ� �ǳʶ�
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] fields = lines[i].Split(',');

            for (int f = 0; f < fields.Length; f++)
            {
                fields[f] = fields[f].Trim();
            }

            if (fields.Length < 10) continue;

            MonsterData data = new MonsterData
            {
                Id = int.Parse(fields[0]),
                Name = fields[1],
                Category = fields[2],
                Maxhealth = int.Parse(fields[3]),
                AttackPower = int.Parse(fields[4]),
                AttackRange = int.Parse(fields[5]),
                Speed = float.Parse(fields[6]),
                Size = float.Parse(fields[7]),
                isElite = bool.Parse(fields[8]),
                isBoss = bool.Parse(fields[9])
            };

            monsters.Add(data);
            Debug.Log($"[MonsterDatabase] ID {data.Id} �ε� �Ϸ�: {data.Name}");
        }

        Debug.Log($"[MonsterDatabase] ���� �� {monsters.Count}�� �ε� �Ϸ�");
    }


    public MonsterData GetByID(int id)
    {
        MonsterData result = monsters.Find(m => m.Id == id);
        if (result == null)
            Debug.LogError($"[MonsterDatabase] ID {id}�� �ش��ϴ� ���͸� ã�� �� �����ϴ�.");
        return result;
    }

    public List<MonsterData> GetByCategory(string category)
    {
        return monsters.FindAll(m => m.Category == category);
    }
}
