using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class StageSelectConsole : UdonSharpBehaviour
{
    [SerializeField]
    private Text _UI_Text;

    [SerializeField]
    private StageSelect _stageSelect;

    public void Plus()
    {
        _stageSelect.Plus();
    }

    public void Minus()
    {
        _stageSelect.Minus();
    }

    public void LoadStart()
    {
        _stageSelect.LoadStart();
    }

    public void StageIdUpdate(int stageId)
    {
        _UI_Text.text = "Select : " + stageId;
    }
}
