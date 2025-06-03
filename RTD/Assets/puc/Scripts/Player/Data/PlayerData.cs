// -------------------------------------------------------
// ���� ���: Assets/puc/Scripts/Player/PlayerData.cs
// -------------------------------------------------------

using System;
using System.Collections.Generic;

namespace GameData
{
    [Serializable]
    public class PlayerData
    {
        public string id;
        public string characterName;
        public string characterType;

        public int level;      // ���� ����
        public int exp;        // ���� �������� �ʿ� ����ġ

        public int maxHp;
        public float recovery;
        public float attackDamage;
        public float attackSpeed;
        public float attackRange;
        public float moveSpeed;
        public int defense;

        public float hitStunTime;
        public float furyGainPerHit;
        public float furyMax;
        public float comboCooldown;
    }

    [Serializable]
    public class PlayerDataRoot
    {
        public List<PlayerData> data;
    }
}
