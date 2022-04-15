using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(TEMP_Local))]
public class TEMP_LocalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); //显示默认所有参数

        TEMP_Local demo = (TEMP_Local)target;

        if (GUILayout.Button("开始游戏", GUILayout.Height(25)))
        {
            demo.GameStart();
        }
        if (GUILayout.Button("洗牌", GUILayout.Height(25)))
        {
            demo.Shuffle();
        }
    }
}
#endif
public class TEMP_Local : MonoBehaviour
{
    public GameLogic logic;

    public void GameStart()
    {
        // 模拟创建房间，模拟加入机器人，
        // 服务器选颜色、洗牌、发牌逻辑，
        logic = new GameLogic();
        logic.OnGameStart_Server();

        //S2C_GameStart buffer = new S2C_GameStart
        //{
        //    RoomID = 0,
        //    Seats = new S2C_SeatInfo[]
        //    {
        //        new S2C_SeatInfo { PlayerID = 0, Color = 1, Cards = new byte[]{ 1,2,3 } },
        //        new S2C_SeatInfo { },
        //    },
        //};
        //logic.OnGameStart(buffer);
    }
    public void Shuffle()
    {

    }
    public void Deal()
    {

    }
}