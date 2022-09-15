#if UNITY_EDITOR

using UnityEngine;

public class GroupDataArraySetting : MonoBehaviour
{
    [ContextMenu("Setting start")]
    private void SettingStart()
    {
        GroupDataSetting[] array = gameObject.GetComponentsInChildren<GroupDataSetting>(true);

        foreach (GroupDataSetting groupDataSetting in array)
        {
            groupDataSetting.SettingStart();
        }
    }
}

#endif