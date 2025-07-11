using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PetDatabase : MonoBehaviour
{
    public List<PetData> pets = new List<PetData>();

    void Awake()
    {
        LoadPetData();
    }

    public void LoadPetData()
    {
        TextAsset csv = Resources.Load<TextAsset>("PetData");

        pets.Clear();

        StringReader reader = new StringReader(csv.text);
        bool isFirstLine = true;

        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            if (isFirstLine) { isFirstLine = false; continue; }

            string[] values = line.Split(',');

            if (values.Length < 7) continue;

            PetData pet = new PetData
            {
                Id = int.Parse(values[0].Trim()),
                PetType = values[1].Trim(),
                Grade = values[2].Trim(),
                HasSkill = bool.Parse(values[3].Trim()),
                Buff = values[4].Trim(),
                Skill = values[5].Trim(),
                SkillCount = int.Parse(values[6].Trim())
            };

            pets.Add(pet);
        }

        Debug.Log($"[PetDatabase] {pets.Count}개의 펫 데이터를 불러왔습니다.");
    }
}
