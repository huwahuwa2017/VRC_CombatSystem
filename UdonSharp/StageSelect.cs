using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class StageSelect : UdonSharpBehaviour
{
    //C#ではconstはあまり使わないほうが良いのだが、U#ではstaticが使えないので仕方なくconstを使用している
    private const uint _conversion_0 = 0b11111111;
    private const int _conversion_1 = 8;



    [SerializeField]
    private DamageManagement _damageManagement;

    [SerializeField]
    private StageSelectConsole[] _stageSelectConsoleArray;

    [SerializeField]
    private GroupData[][] _groupDataArrayArray;



    [UdonSynced, FieldChangeCallback(nameof(SelectStageId))]
    private byte _selectStageId;

    public byte SelectStageId
    {
        set
        {
            Debug.Log("StageSelect.SetStageId");

            _selectStageId = value;

            foreach (StageSelectConsole stageSelectConsole in _stageSelectConsoleArray)
            {
                stageSelectConsole.StageIdUpdate(_selectStageId);
            }
        }
    }



    [UdonSynced, FieldChangeCallback(nameof(LoadStageId))]
    private uint _loadStageId;

    public uint LoadStageId
    {
        set
        {
            Debug.Log("StageSelect.SetLoadStageId : " + Time.frameCount);

            _loadStageId = value;
            byte stageId = (byte)(_loadStageId & _conversion_0);
            _damageManagement.Restart(_groupDataArrayArray[stageId]);
        }
    }



    private void Start()
    {
        Debug.Log("StageSelect.Start() : " + Time.frameCount);

        if (Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            LoadStart();
        }
    }

    public void Plus()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "Plus");
            return;
        }

        SelectStageId = (byte)Mathf.Min(_selectStageId + 1, _groupDataArrayArray.Length - 1);
        RequestSerialization();
    }

    public void Minus()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "Minus");
            return;
        }

        SelectStageId = (byte)Mathf.Max(_selectStageId - 1, 0);
        RequestSerialization();
    }

    public void LoadStart()
    {
        if (!Networking.IsOwner(Networking.LocalPlayer, gameObject))
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "LoadStart");
            return;
        }

        LoadStageId = ((uint)Time.frameCount << _conversion_1) | _selectStageId;
        RequestSerialization();
    }
}
