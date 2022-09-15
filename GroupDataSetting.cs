#if UNITY_EDITOR

using System;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.Udon;
using VRC.Udon.Common;
using VRC.Udon.Common.Interfaces;

public class GroupDataSetting : MonoBehaviour
{
    private static IUdonVariable CreateUdonVariable(string symbolName, object value, Type type)
    {
        Type udonVariableType = typeof(UdonVariable<>).MakeGenericType(type);
        return (IUdonVariable)Activator.CreateInstance(udonVariableType, symbolName, value);
    }

    public static void SetUdonBehaviourVariable(UdonBehaviour udonBehaviour, string symbolName, object value)
    {
        IUdonVariableTable publicVariables = udonBehaviour?.publicVariables;
        if (publicVariables == null) return;

        if (!publicVariables.TrySetVariableValue(symbolName, value))
        {
            UdonSharpProgramAsset udonSharpProgramAsset = (UdonSharpProgramAsset)udonBehaviour.programSource;
            Type symbolType = udonSharpProgramAsset?.GetRealProgram()?.SymbolTable?.GetSymbolType(symbolName);

            if (symbolType == null || !publicVariables.TryAddVariable(CreateUdonVariable(symbolName, value, symbolType)))
            {
                Debug.LogError($"Failed to set public variable '{symbolName}' value.");
            }
        }

        if (PrefabUtility.IsPartOfPrefabInstance(udonBehaviour))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(udonBehaviour);
        }
    }

    public static object GetUdonBehaviourVariable(UdonBehaviour udonBehaviour, string symbolName)
    {
        IUdonVariableTable publicVariables = udonBehaviour?.publicVariables;
        if (publicVariables == null) return null;

        if (!publicVariables.TryGetVariableValue(symbolName, out object variableValue))
        {
            UdonSharpProgramAsset udonSharpProgramAsset = (UdonSharpProgramAsset)udonBehaviour.programSource;
            variableValue = udonSharpProgramAsset.GetPublicVariableDefaultValue(symbolName);
        }

        return variableValue;
    }



    [SerializeField]
    private byte _teamId;

    [ContextMenu("Setting start")]
    public void SettingStart()
    {
        UdonBehaviour targetUdonBehaviour = (UdonBehaviour)gameObject.GetComponent(typeof(UdonBehaviour));
        if (targetUdonBehaviour == null) return;

        UdonBehaviour[] temp_0 = GetComponentsInChildren<UdonBehaviour>(true);
        GameObject[] temp_1 = temp_0.Select(i => i.gameObject).Where(i => i != gameObject).ToArray();
        SetUdonBehaviourVariable(targetUdonBehaviour, "_resetArray", temp_1);
        Debug.Log("_resetArray : " + temp_1.Length);

        UdonBehaviour[] temp_2 = temp_0.Where(i => i.CompareTag("Damageable")).ToArray();

        foreach (UdonBehaviour ub in temp_2)
        {
            SetUdonBehaviourVariable(ub, "TeamId", _teamId);
        }

        GameObject[] temp_3 = temp_2.Select(i => i.gameObject).ToArray();
        SetUdonBehaviourVariable(targetUdonBehaviour, "_damageableObjectArray", temp_3);
        Debug.Log("_damageableObjectArray : " + temp_3.Length);
    }
}

#endif