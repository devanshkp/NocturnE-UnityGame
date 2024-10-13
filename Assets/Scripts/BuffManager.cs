using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuffType { SpeedBuff, DamageBuff, JumpBuff }

public class BuffManager : MonoBehaviour
{
    public PlayerController player;
    public GameObject speedBuff;
    public GameObject damageBuff;
    public GameObject jumpBuff;

    private List<GameObject> activeBuffs = new List<GameObject>(); // List to store active buffs
    // Start is called before the first frame update
    void Start()
    {
        if (player == null){
            player = GetComponentInParent<PlayerController>();
        }

        // Make sure all buff icons are initially disabled
        speedBuff.SetActive(false);
        damageBuff.SetActive(false);
        jumpBuff.SetActive(false);
    }

    public bool EnableBuff(BuffType buffType)
    {
        GameObject buffIcon = null;

        switch (buffType)
        {
            case BuffType.SpeedBuff:
                buffIcon = speedBuff;
                break;
            case BuffType.DamageBuff:
                buffIcon = damageBuff;
                break;
            case BuffType.JumpBuff:
                buffIcon = jumpBuff;
                break;
        }

        if (buffIcon != null && !activeBuffs.Contains(buffIcon))
        {
            activeBuffs.Add(buffIcon);
            buffIcon.SetActive(true);
            UpdateBuffIconPositions();
            return true;
        }
        return false;
    }

    // This function updates the position of all active buff icons based on how many are active
    private void UpdateBuffIconPositions()
    {
        float staticOffset = activeBuffs[0].transform.position.x;
        float xOffset = 90f;
        for (int i = 0; i < activeBuffs.Count; i++)
        {
            // Move each active buff to its correct position
            Vector3 newPosition = new Vector3(staticOffset + i * xOffset, activeBuffs[i].transform.position.y, activeBuffs[i].transform.position.z);
            activeBuffs[i].transform.position = newPosition;
        }
    }
}
