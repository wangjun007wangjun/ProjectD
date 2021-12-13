using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Engine.Schedule;
using Engine.Asset;
using Engine.Base;
using Engine.Cor;
using System;

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
    Fork,// 对角线方向对称
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
    private GameObject safeArea;

    private string _audioPaht = "";
    private GameDifficulty _gameDiff = GameDifficulty.Easy;

    private List<DanceItem> danceItems;
    private Vector3[] _cors = new Vector3[4];   //安全生产区域(左下、左上、右上、右下 )
    private int _safeWidth = 0;   //安全生产区域宽
    private int _safeHeight = 0;   //安全生产区域高

    private float _audioLength = 120f;    //音乐时长,秒

    private uint _chanceTimer;

    //配置
    //每个个数球出现的概率
    private float[,] numChance;
    //对称出现的概率
    private float[,] typeChance;
    private float[] mulNumBornInterval;
    private double startTime = 0;
    private double endTime = 0;


    //总分数
    private int totalScore;
    private int[] scoreState;
    //场景相关
    private BaseAsset danceRoot;
    private MusicEnvMgr musicEnvMgr;

    private uint createItemCorId;
    public void Init(UIGamingForm form, GameDifficulty gameDiff, object param)
    {
        _gameDiff = gameDiff;
        if (_gameDiff == GameDifficulty.Easy)
        {
            numChance = GameCfgConst.NumChance1;
            typeChance = GameCfgConst.TypeChance1;
            mulNumBornInterval = GameCfgConst.MulNumBornInterval1;
            scoreState = GameCfgConst.AddScoreByState1;
        }
        else if (_gameDiff == GameDifficulty.Normal)
        {
            numChance = GameCfgConst.NumChance2;
            typeChance = GameCfgConst.TypeChance2;
            mulNumBornInterval = GameCfgConst.MulNumBornInterval2;
            scoreState = GameCfgConst.AddScoreByState2;
        }
        else if (_gameDiff == GameDifficulty.Hard)
        {
            numChance = GameCfgConst.NumChance3;
            typeChance = GameCfgConst.TypeChance3;
            mulNumBornInterval = GameCfgConst.MulNumBornInterval3;
            scoreState = GameCfgConst.AddScoreByState3;
        }
        //开始时间
        startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        endTime = startTime + _audioLength * 1000;
        
        _mainForm = form;
        safeArea = _mainForm.RootPLink.GetCacheGameObject(0);

        Hashtable table = (Hashtable)param;
        _cors = (Vector3[])table["corners"];
        _safeWidth = (int)table["width"];
        _safeHeight = (int)table["height"];

        // Debug.Log("安全区高:" + _safeHeight + ",宽:" + _safeWidth + ",坐标：" + _cors.ToString());

        danceRoot = AssetService.GetInstance().LoadInstantiateAsset("Gaming/DanceRoot", LifeType.Manual);
        danceRoot.RootGo.SetActive(true);
        musicEnvMgr = danceRoot.RootGo.GetComponent<MusicEnvMgr>();
    }

    public void OnInitMusicEnv(AudioClip clip)
    {
    }

    public void BeginDanceGame(AudioClip clip)
    {
        musicEnvMgr.PlaySound(clip);

        //产生多少个
        // int clickCnt = 60;
        // float minInterval = 0.5f;

        //时间间隔也是随机值
        int[] intervalArr = new int[2] { 500, 3000 };
        //得出哪些时间点产生点击
        // Random.Range(1, 34);

        int next = UnityEngine.Random.Range(intervalArr[0], intervalArr[1]);
        // Debug.Log("下次时间："+ next);
        _chanceTimer = this.AddTimer(next, false);
    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {
        if (id == _chanceTimer)
        {
            BornNewItem();

            //时间间隔也是随机值
            int[] intervalArr = new int[2] { 500, 3000 };
            int next = UnityEngine.Random.Range(intervalArr[0], intervalArr[1]);
            
            double curDataTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            double nextDataTime = curDataTime + next + 1;
            // Debug.Log("下次时间："+ next);

            if (nextDataTime < endTime)
            {
                _chanceTimer = this.AddTimer(next, false);
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
        float numC = UnityEngine.Random.Range(0f, 1f);
        float typeC = UnityEngine.Random.Range(0f, 1f);

        BornNum numE = BornNum.One;
        BornType typeE = BornType.None;
        for (int i = 0; i < 5; i++)
        {
            if (numC >= numChance[i, 0] && numC < numChance[i, 1])
            {
                numE = (BornNum)i;
                break;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (typeC >= typeChance[i, 0] && typeC < typeChance[i, 1])
            {
                typeE = (BornType)i;
                break;
            }
        }
        //位置
        //将安全区宽，高处100，作为随机index
        // int intWeigthMin = Mathf.CeilToInt(_cors[1].x / 100);
        // int intWeigthMax = Mathf.CeilToInt(_cors[3].x / 100);
        // int intHeightMin = Mathf.CeilToInt(_cors[1].y / 100);
        // int intHeightMax = Mathf.CeilToInt(_cors[3].y / 100);
        // Debug.Log("宽高随机范围：" + intWeigthMin + "," +intWeigthMax + "," + intHeightMin + "," +intWeigthMax);

        //如果小于等于2个，位置完全随机，否则3个及以上，位置需要为连续线段
        //2个及以上时，产生时间需要有间隔
        int bornNum = (int)(numE + 1);
        List<Vector4> posList = CreatePos(bornNum, typeE);
        createItemCorId = CorService.GetInstance().StartCoroutineSession(CreateDanceGrid(posList));
    }
    private IEnumerator CreateDanceGrid(List<Vector4> posList)
    {
        float[] mulNumBornInterval = new float[2]{0, 0.3f};
        for (int i = 0; i < posList.Count; i++)
        {
            // Debug.Log("坐标x:"+posList[i].x+",y:"+posList[i].y+",z:"+posList[i].z+",w:"+posList[i].z);
            Vector2 pos1 = new Vector2(posList[i].x, posList[i].y);
            Vector2 pos2 = new Vector2(posList[i].x, posList[i].y);
            DanceGrid grid1 = new DanceGrid();
            grid1.Born(new Vector2(posList[i].x, posList[i].y), safeArea.transform);
            grid1.unSpawnAction = AddScore;
            if (posList[i].z != -1 && posList[i].w != -1)
            {
                DanceGrid grid2 = new DanceGrid();
                grid2.Born(new Vector2(posList[i].z, posList[i].w), safeArea.transform);
                grid2.unSpawnAction = AddScore;
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(mulNumBornInterval[0], mulNumBornInterval[1]));
        }
        yield return null;
    }
    
    //产生位置，num个数,Vector4,前两个为半边屏幕，后2个为半边屏幕的
    private List<Vector4> CreatePos(int num, BornType bornType)
    {
        Debug.Log("个数："+num+" 类型："+bornType.ToString());
        List<Vector4> result = new List<Vector4>();

        int intWeigthMin = Mathf.CeilToInt(_cors[1].x / 100);
        int intWeigthMax = Mathf.CeilToInt(_cors[3].x / 100);
        int intHeightMin = Mathf.CeilToInt(_cors[1].y / 100);
        int intHeightMax = Mathf.CeilToInt(_cors[3].y / 100);

        List<Vector2> existPos = new List<Vector2>();
        Vector2 pos = new Vector2(-1, -1);
        //如果小于等于2个，位置完全随机，否则3个及以上，位置需要为连续线段
        //2个及以上时，产生时间需要有间隔
        // if (num <= 2)
        // {
            for (int i = 0; i < num; i++)
            {
                int randCnt = 0;
                do
                {
                    int randomX = UnityEngine.Random.Range(intWeigthMin, intWeigthMax) * 100;
                    int randomY = UnityEngine.Random.Range(intHeightMin, intHeightMax) * 100;
                    // Debug.Log("随机点：" + randomX + "," +randomY);
                    pos = new Vector2(randomX, randomY);
                    existPos.Add(pos);

                    result.Add(new Vector4(randomX, randomY, -1, -1));

                    randCnt += 1;
                } 
                while ((!existPos.Contains(pos)) || randCnt > 3);
            }
            if (bornType != BornType.None)
            {
                //得出对称点
                List<Vector2> other = GetSymmetricPos(existPos, bornType);
                for (int i = 0; i < other.Count; i++)
                {
                    if (other[i].x == result[i].x && other[i].y == result[i].y)
                    {
                    }
                    else
                    {
                        // Debug.Log("设置对称x:"+other[i].x +",y:"+other[i].y);
                        result[i] = new Vector4(result[i].x, result[i].y, other[i].x, other[i].y);
                    }
                }
            }
        // }
        return result;   
    }

    private IEnumerator CreateDanceGrid(int num)
    {
        float[] mulNumBornInterval = new float[2]{0, 0.3f};
        int intWeigthMin = Mathf.CeilToInt(_cors[1].x / 100);
        int intWeigthMax = Mathf.CeilToInt(_cors[3].x / 100);
        int intHeightMin = Mathf.CeilToInt(_cors[1].y / 100);
        int intHeightMax = Mathf.CeilToInt(_cors[3].y / 100);

        List<Vector2> existPos = new List<Vector2>();
        Vector2 pos = new Vector2(-1, -1);
        for (int i = 0; i < num; i++)
        {
            do
            {
                int randomX = UnityEngine.Random.Range(intWeigthMin, intWeigthMax) * 100;
                int randomY = UnityEngine.Random.Range(intHeightMin, intHeightMax) * 100;
                Debug.Log("随机点：" + randomX + "," +randomY);
                pos = new Vector2(randomX, randomY);

                DanceGrid grid = new DanceGrid();
            
                grid.Born(pos, safeArea.transform);
                grid.unSpawnAction = AddScore;
                
                existPos.Add(pos);
            } 
            while (!existPos.Contains(pos));
            if (num != 1)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(mulNumBornInterval[0], mulNumBornInterval[1]));
            }
            else
            {
                yield return null;
            }
        }
        // yield return null;
    }

    //获得对称点
    private List<Vector2> GetSymmetricPos(List<Vector2> originList, BornType bornType)
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < originList.Count; i++)
        {
            if (bornType == BornType.Hor)
            {
                Vector2 pos = new Vector2(originList[i].x * -1, originList[i].y);
                // if (!originList.Contains(pos))
                // {
                    result.Add(pos);
                // }
            }
            else if (bornType == BornType.Ver)
            {
                Vector2 pos = new Vector2(originList[i].x, originList[i].y * -1);

                // if (!originList.Contains(pos))
                // {
                    result.Add(pos);
                // }
            }
            else if (bornType == BornType.Fork)
            {
                Vector2 pos = new Vector2(originList[i].x * -1, originList[i].y * -1);

                // if (!originList.Contains(pos))
                // {
                    result.Add(pos);
                // }
            }
        }
        // result.AddRange(originList);
        // Debug.Log("对称个数："+result.Count);
        return result;
    }

    public void AddScore(ItemDanceState state)
    {
        totalScore += scoreState[(int)state];
        _mainForm.UpdateUI("UpdateScore", totalScore);
    }

    public void UnInit()
    {
        this.RemoveTimer(_chanceTimer);
        CorService.GetInstance().StopCoroutineSession(createItemCorId);
        if (danceRoot != null)
        {
            AssetService.GetInstance().Unload(danceRoot);
            danceRoot = null;
        }
    }
}
