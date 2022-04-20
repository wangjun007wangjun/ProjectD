using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Engine.Schedule;
using Engine.Asset;
using Engine.Base;
using Engine.Cor;
using System;
using Data;
using Engine.Audio;

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

//游戏的主要控制管理器
//分数控制，难度控制，音乐控制，气泡产生算法
public class DanceMgr : IScheduleHandler
{
    private GameState gameState;
    //主界面窗口
    private UIGamingForm _mainForm;
    //产生气泡的安全区域
    private GameObject safeArea;

    //游戏难度
    private GameDifficulty _gameDiff = GameDifficulty.Easy;

    private Vector3[] _cors = new Vector3[4];   //安全生产区域(左下、左上、右上、右下 )
    private int _safeWidth = 0;   //安全生产区域宽
    private int _safeHeight = 0;   //安全生产区域高

    private float _audioLength = 120f;    //音乐时长,秒

    private uint _chanceTimer;
    private uint _showResultTimer;

    //配置
    //每个个数球出现的概率
    private float[,] numChance;
    //对称出现的概率
    private float[,] typeChance;
    //多个球产生的间隔时间
    private float[] mulNumBornInterval;
    //开始时间
    private double startTime = 0;
    //结束时间
    private double endTime = 0;


    //总分数
    private int totalScore = 0;
    //当前局加分配置
    private int[] scoreState;
    //场景相关
    private BaseAsset danceRoot;
    //背景管理
    private MusicEnvMgr musicEnvMgr = null;

    private uint createItemCorId;

    //暂停时时间
    private double pauseStartTime;
    private double pauseEndTime;
    PlayVideo playVideo = null;

    //初始化管理器
    public void Init(GameState state, UIGamingForm form, GameDifficulty gameDiff, object param, MusicData musicData)
    {
        gameState = state;
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

        _mainForm = form;
        safeArea = _mainForm.RootPLink.GetCacheGameObject(0);

        Hashtable table = (Hashtable)param;
        _cors = (Vector3[])table["corners"];
        _safeWidth = (int)table["width"];
        _safeHeight = (int)table["height"];
        //音乐时长
        _audioLength = musicData.audio.length;
        //开始时间，结束时间
        startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        endTime = startTime + _audioLength * 1000;
    // GLog.LogD("开始时间：" + startTime.ToString()+" 结束："+endTime.ToString()+ " 时长："+_audioLength.ToString());

        // Debug.Log("安全区高:" + _safeHeight + ",宽:" + _safeWidth + ",坐标：" + _cors.ToString());
        if (DataService.GetInstance().Model == 2)
        {
            danceRoot = AssetService.GetInstance().LoadInstantiateAsset("Gaming/DanceRoot", LifeType.Manual);
            danceRoot.RootGo.SetActive(true);
            musicEnvMgr = danceRoot.RootGo.GetComponent<MusicEnvMgr>();

            //播放背景音乐
            musicEnvMgr.PlaySound("Mp3/" + musicData.difficulty.ToString() + "/" + musicData.audio.name);
        }
        else
        {
            startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            playVideo = _mainForm.RootPLink.GetCacheComponent(1) as PlayVideo;
            string videoName = "mountain";
            if (_gameDiff == GameDifficulty.Easy)
            {
                videoName = "mountain";
            }
            else if (_gameDiff == GameDifficulty.Normal)
            {
                videoName = "ocean";
            }
            else
            {
                videoName = "starrysky";
            }
            playVideo.VideoPlay(videoName, _audioLength, null, null, _audioLength);
            AudioService.GetInstance().Play(AudioChannelType.MUSIC, "Mp3/" + musicData.difficulty.ToString() + "/" + musicData.audio.name, false);
        }
    }
    //再来一次
    public void ReTry(MusicData musicData)
    {
        //音乐时长
        _audioLength = musicData.audio.length;
        //开始时间，结束时间
        startTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        endTime = startTime + _audioLength * 1000;
        if (DataService.GetInstance().Model == 2)
        {
            //播放背景音乐
            musicEnvMgr.PlaySound("Mp3/" + musicData.difficulty.ToString() + "/" + musicData.audio.name);
        }
        else
        {
            playVideo = _mainForm.RootPLink.GetCacheComponent(1) as PlayVideo;
            string videoName = "mountain";
            if (_gameDiff == GameDifficulty.Easy)
            {
                videoName = "mountain";
            }
            else if (_gameDiff == GameDifficulty.Normal)
            {
                videoName = "ocean";
            }
            else
            {
                videoName = "starrysky";
            }
            endTime = startTime + _audioLength * 1000;
            playVideo.VideoPlay(videoName, _audioLength, null, null, _audioLength);
            AudioService.GetInstance().Play(AudioChannelType.MUSIC, "Mp3/" + musicData.difficulty.ToString() + "/" + musicData.audio.name, false);
        }
    }
    public void BeginDanceGame()
    {
        //时间间隔也是随机值
        int[] intervalArr = new int[2] { 500, 3000 };

        int next = UnityEngine.Random.Range(intervalArr[0], intervalArr[1]);
        // Debug.Log("下次时间："+ next);
        _chanceTimer = this.AddTimer(next, false);
    }

    public void OnScheduleHandle(ScheduleType type, uint id)
    {
        if (id == _chanceTimer)
        {
            //产生点击气泡
            BornNewItem();

            //时间间隔也是随机值
            int[] intervalArr = new int[2] { 500, 3000 };
            int next = UnityEngine.Random.Range(intervalArr[0], intervalArr[1]);

            double curDataTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            double nextDataTime = curDataTime + next + 2;

            //游戏未结束，正常产生
            if (nextDataTime < endTime)
            {
                _chanceTimer = this.AddTimer(next, false);
            }
            else
            {
                //游戏音乐结束，准备结算
                this.RemoveTimer(_chanceTimer);
                _showResultTimer = this.AddTimer(1500, false);
            }
        }
        else if (id == _showResultTimer)
        {
            //结束了，进行结算
            if (musicEnvMgr != null)
            {
                musicEnvMgr.StopSound();
            }
            gameState.OnFinishDance(totalScore);
        }
    }

    //控制产生气泡，根据配置
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
        //如果小于等于2个，位置完全随机，否则3个及以上，位置需要为连续线段
        //2个及以上时，产生时间需要有间隔
        int bornNum = (int)(numE + 1);
        List<Vector4> posList = CreatePos(bornNum, typeE);
        createItemCorId = CorService.GetInstance().StartCoroutineSession(CreateDanceGrid(posList));
    }

    //协程创建气泡Item
    private IEnumerator CreateDanceGrid(List<Vector4> posList)
    {
        float[] mulNumBornInterval = new float[2] { 0, 0.3f };
        for (int i = 0; i < posList.Count; i++)
        {
            // Debug.Log("坐标x:"+posList[i].x+",y:"+posList[i].y+",z:"+posList[i].z+",w:"+posList[i].z);
            Vector2 pos1 = new Vector2(posList[i].x, posList[i].y);
            Vector2 pos2 = new Vector2(posList[i].x, posList[i].y);
            DanceGrid grid1 = new DanceGrid();
            grid1.AddScoreAction = AddScore;
            grid1.Born(new Vector2(posList[i].x, posList[i].y), safeArea.transform);
            if (posList[i].z != -1 && posList[i].w != -1)
            {
                DanceGrid grid2 = new DanceGrid();
                grid2.AddScoreAction = AddScore;
                grid2.Born(new Vector2(posList[i].z, posList[i].w), safeArea.transform);
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(mulNumBornInterval[0], mulNumBornInterval[1]));
        }
        yield return null;
    }

    //产生位置，num个数,Vector4,前两个为半边屏幕，后2个为半边屏幕的
    private List<Vector4> CreatePos(int num, BornType bornType)
    {
        // Debug.Log("个数："+num+" 类型："+bornType.ToString());
        List<Vector4> result = new List<Vector4>();

        int intWeigthMin = Mathf.CeilToInt(_cors[1].x / 150);
        int intWeigthMax = Mathf.CeilToInt(_cors[3].x / 150);
        int intHeightMin = Mathf.CeilToInt(_cors[1].y / 150);
        int intHeightMax = Mathf.CeilToInt(_cors[3].y / 150);

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
                int randomX = UnityEngine.Random.Range(intWeigthMin, intWeigthMax) * 150;
                int randomY = UnityEngine.Random.Range(intHeightMin, intHeightMax) * 150;
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

    //获得对称点
    private List<Vector2> GetSymmetricPos(List<Vector2> originList, BornType bornType)
    {
        List<Vector2> result = new List<Vector2>();
        for (int i = 0; i < originList.Count; i++)
        {
            if (bornType == BornType.Hor)
            {
                Vector2 pos = new Vector2(originList[i].x * -1, originList[i].y);
                result.Add(pos);
            }
            else if (bornType == BornType.Ver)
            {
                Vector2 pos = new Vector2(originList[i].x, originList[i].y * -1);

                result.Add(pos);
            }
            else if (bornType == BornType.Fork)
            {
                Vector2 pos = new Vector2(originList[i].x * -1, originList[i].y * -1);

                result.Add(pos);
            }
        }
        // Debug.Log("对称个数："+result.Count);
        return result;
    }

    //计分，ui显示更新
    public void AddScore(ItemDanceState state)
    {
        // Debug.Log("加分状态："+state.ToString()+ " 分数：" +(scoreState[(int)state]).ToString());
        totalScore += scoreState[(int)state];
        _mainForm.UpdateUI("UpdateScore", totalScore);
    }

    //卸载退出管理器
    public void UnInit()
    {
        this.RemoveTimer(_chanceTimer);
        CorService.GetInstance().StopCoroutineSession(createItemCorId);
        if (danceRoot != null)
        {
            AssetService.GetInstance().Unload(danceRoot);
            danceRoot = null;

            musicEnvMgr = null;
        }
        else
        {
            AudioService.GetInstance().StopChannel(1);
        }
    }

    //游戏暂停，结束时间边长
    private void OnGamePause()
    {
        OnSetMusicEnvPause(true);
    }

    //游戏继续
    private void OnGameContinue()
    {
        OnSetMusicEnvPause(false);
    }

    //暂停时，背景也暂停
    public void OnSetMusicEnvPause(bool pause)
    {
        if (pause)
        {
            pauseStartTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
        }
        else
        {
            pauseEndTime = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;
            endTime += (pauseEndTime - pauseStartTime);

            pauseEndTime = 0;
            pauseStartTime = 0;
        }
        if (DataService.GetInstance().Model == 2)
        {
            if (musicEnvMgr != null)
            {
                musicEnvMgr.OnSetGameEnablePause(pause);
            }
        }
        else
        {
            if (pause)
            {
                playVideo.PauseVideo();
            }
            else
            {
                playVideo.ContinueVideo();
            }
        }

    }
}
