using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Engine.Schedule;
using Engine.Asset;

///状态
public enum ItemDanceState
{
    None,
    Fail,
    Good,
    Cool,
    Perfect,
}

//产生点击球个数
public enum BornNum
{
    One, //单个
    Two,
    Three,
    Four,
    Five,
}

//产生点击球模式
public enum BornType
{
    None,//无
    Ver,//纵向对称
    Hor,//横向对称
    Fork1,// '/'方向对称
    Fork2,// '\'方向对称
}

public enum GameDifficulty
{
    Easy,
    Normal,
    Hard,
}
public class DanceMgr : IScheduleHandler
{
    private const string C_danceItemPath = "Gaming/DanceItem";
    private UIGamingForm _mainForm;
    private string _audioPaht = "";
    private GameDifficulty _gameDiff = GameDifficulty.Easy;

    private List<DanceItem> danceItems;
    private Vector3[] _cors = new Vector3[4];   //安全生产区域(左下、左上、右上、右下 )
    private int _safeWidth = 0;   //安全生产区域宽
    private int _safeHeight = 0;   //安全生产区域高

    private float _audioLength = 2f;    //音乐时长

    private int _curTime = 0;    //当前播放时间（毫秒）
    private uint _chanceTimer;

    //配置
    //每个个数球出现的概率
    private float[,] numChance;
    //对称出现的概率
    private float[,] typeChance;

    public void Init(UIGamingForm form)
    {
        _mainForm = form;
    }

    public void OnSetSafeInfo(object param)
    {
        Hashtable table = (Hashtable)param;
        _cors = (Vector3[])table["corners"];
        _safeWidth = (int)table["width"];
        _safeHeight = (int)table["height"];

        Debug.Log("安全区高:" + _safeHeight + ",宽:" + _safeWidth + ",坐标：" + _cors.ToString());
    }

    public void BeginDanceGame()
    {
        //产生多少个
        int clickCnt = 60;
        float minInterval = 0.5f;

        // List<float, float> numChance = new List<float>(){{0, 0.7f}, {0, 0.7f}, {0, 0.7f}, {0, 0.7f}, {0, 0.7f}};
        //每个个数球出现的概率
        numChance = new float[5, 2] { { 0, 0.7f }, { 0.7f, 1f }, { -1, -1 }, { -1, -1 }, { -1, -1 } };
        //对称出现的概率
        typeChance = new float[5, 2] { { 0, 0.7f }, { 0.7f, 0.85f }, { 0.85f, 1f }, { -1, -1 }, { -1, -1 } };
        if (_gameDiff == GameDifficulty.Easy)
        {
            //TODO
        }

        //时间间隔也是随机值
        int[] intervalArr = new int[2] { 500, 3000 };
        //得出哪些时间点产生点击
        // Random.Range(1, 34);
        _curTime += Random.Range(intervalArr[0], intervalArr[1]);
        _chanceTimer = this.AddTimer(_curTime, false);
    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {
        if (id == _chanceTimer)
        {
            Debug.Log("产生新的");
            BornNewItem();

            //时间间隔也是随机值
            int[] intervalArr = new int[2] { 500, 3000 };
            _curTime += Random.Range(intervalArr[0], intervalArr[1]);
            if (_curTime < _audioLength * 1000)
            {
                _chanceTimer = this.AddTimer(_curTime, false);
            }
            else
            {
                this.RemoveTimer(_chanceTimer);
                //TODO结束了
            }
        }
    }

    private void BornNewItem()
    {
        float numC = Random.Range(0f, 1f);
        float typeC = Random.Range(0f, 1f);

        BornNum numE = BornNum.One;
        BornType typeE = BornType.None;
        for (int i = 0; i < numChance.Length - 1; i++)
        {
            if (numC >= numChance[i, 0] && numC < numChance[i, 1])
            {
                numE = (BornNum)i;
                break;
            }
        }
        for (int i = 0; i < typeChance.Length - 1; i++)
        {
            if (typeC >= typeChance[i, 0] && typeC < typeChance[i, 1])
            {
                typeE = (BornType)i;
                break;
            }
        }
        //位置
        //将安全区宽，高处100，作为随机index
        int intWeigthMin = Mathf.CeilToInt(_cors[1].x / 100);
        int intWeigthMax = Mathf.CeilToInt(_cors[3].x / 100);
        int intHeightMin = Mathf.CeilToInt(_cors[1].y / 100);
        int intHeightMax = Mathf.CeilToInt(_cors[3].y / 100);
        Debug.Log("宽高随机范围：" + intWeigthMin + "," +intWeigthMax + "," + intHeightMin + "," +intWeigthMax);

        int randomX = Random.Range(intWeigthMin, intWeigthMax) * 100;
        int randomY = Random.Range(intHeightMin, intHeightMax) * 100;
        Debug.Log("随机点：" + randomX + "," +randomY);

        BaseAsset obj = AssetService.GetInstance().LoadInstantiateAsset(C_danceItemPath, (int)LifeType.Manual);
        _mainForm.SetItemInsert(obj.RootGo);
        //演示卸载TODO
        AssetService.GetInstance().Unload(obj);
        obj = null;
    }
}
