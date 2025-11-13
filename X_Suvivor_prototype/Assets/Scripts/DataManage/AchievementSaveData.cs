using System.Collections.Generic;

[System.Serializable]
public class AchievementSaveData
{
    // 1. 플레이어의 모든 누적 통계
    //    Key: "totalKills", "playerDeaths", "gachaSpent" 등
    //    Value: 누적값
    public Dictionary<string, long> statistics = new Dictionary<string, long>();

    // 2. 보상을 수령한 업적 ID 목록
    public List<string> claimedAchievementIds = new List<string>();

    // 3. (선택적) 완료는 했지만 아직 보상을 받지 않은 업적 ID 목록
    public List<string> completedAchievementIds = new List<string>();
}