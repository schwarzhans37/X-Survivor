using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    // 재화 정보
    public int gold;
    public int gems;

    // 보유 목록 (ID만 저장하여 가볍게 관리)
    public List<int> unlockedCharacterIDs;
    public List<int> ownedPetIDs;

    // 업적 및 도감 정보 (미리 틀만 마련)
    public List<int> completedAchievementIDs;
    public List<int> unlockedMonsterIDs;
    public List<int> seenPetIDs;
    
    // 기본 데이터 생성자
    public PlayerData()
    {
        gold = 1000;
        gems = 100;
        unlockedCharacterIDs = new List<int>() { 1, 2 }; // 기본 캐릭터(1번, 2번)는 처음부터 해금
        ownedPetIDs = new List<int>() { 100 }; // 기본 펫(100번)은 처음부터 보유
        completedAchievementIDs = new List<int>();
        unlockedMonsterIDs = new List<int>();
        seenPetIDs = new List<int>();
    }
}