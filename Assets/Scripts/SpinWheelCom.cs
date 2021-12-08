using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpinWheelCom : MonoBehaviour
{
    public enum SpinWheelStateType
    {
        None, //待机状态
        Start, //加速阶段
        Prepared, //等待数据阶段
        End, //减速阶段
    }
    [Header("最大速度")]
    public int Velocity = 2500;
    [Header("转动转盘对象")]
    public Transform NodeTran;
    [Header("转动总时间")]
    public float TotalTime = 3f;
    [Header("减速前最短持续时间")]
    public float MinTime = 3.0f;

    [Header("转到该奖励时背景")]
    public Sprite SelectAwardAngleBg;
    [Header("正常静止时奖品文字颜色")]
    public Color awardTextNormalColor;
    [Header("转到该奖品时文字颜色")]
    public Color SelectAwardTextColor;
    [Header("所有奖品的奖品文字")]
    public TextMeshProUGUI[] AwardItemTexts;
    [Header("所有奖品背景")]
    public Image[] AwardItemBgs;
    [Header("角度修正")]
    public int ModifiedAngle = 280;
    
    //加速持续时间
    private readonly float AcceleateTime = 1f;
    private SpinWheelStateType _spinWheelStateType;

    private float _endAngle = 0f;
    public float EndAngle
    {
        get
        {
            return _endAngle;
        }
        set
        {
            _endAngle = Mathf.Abs(value);
            _endAngle = _endAngle % 360; //将角度限定在[0, 360]这个区间
            _endAngle = ModifiedAngle - _endAngle - 360 * 2; //多N圈并取反，圈数能使减速阶段变得更长，显示更自然，逼真
        }
    }
    //角度缓存
    private float _tmpAngle = 0f;
    // 时间统计
    private float _time;
    // 速度变化因子
    private float _factor;
    //被抽中的奖品位置
    private int _selectAwardIndex;

    public UnityAction EndSpinHandler;
    void Awake()
    {
        _spinWheelStateType = SpinWheelStateType.None;
    }

    void Update()
    {
        if (_spinWheelStateType == SpinWheelStateType.None)
        {
            return;
        }

        _time += Time.deltaTime;
        if (_spinWheelStateType == SpinWheelStateType.End) //减速
        {
            //通过差值运算实现精准地旋转到指定角度（球型插值无法实现大于360°的计算）
            float k = 2f; //如果嫌减速太慢，可以加个系数修正一下
            _tmpAngle = Mathf.Lerp(_tmpAngle, EndAngle, Time.deltaTime * k);

            //这里只存在一个方向的旋转，所以不存在欧拉角万向节的问题，所以使用欧拉角和四元数直接赋值都是可以的
            NodeTran.eulerAngles = new Vector3(0, 0, _tmpAngle);

            if (0.1f >= Mathf.Abs(_tmpAngle - EndAngle))
            {
                NodeTran.eulerAngles = new Vector3(0, 0, EndAngle);

                _spinWheelStateType = SpinWheelStateType.None;

                //设置对应奖励背景白色,然后弹出奖励弹窗
                StopSpin();
            }
        }
        else //加速
        {
            //利用一个速度因子实现变加速的过程
            _factor = _time / AcceleateTime;
            _factor = _factor > 1 ? 1 : _factor;
            NodeTran.Rotate(Vector3.back, _factor * Velocity * Time.deltaTime, Space.Self);
        }

        //当收到数据之后并且旋转了一定时间后开始减速
        if (_spinWheelStateType == SpinWheelStateType.Prepared && _time > MinTime)
        {
            //TODO播放声音
            _spinWheelStateType = SpinWheelStateType.End;
            _tmpAngle = GetCurClockwiseAngle();
        }
    }
    /// <summary>
    /// 将当前指针的欧拉角转换成顺时针统计角度
    /// </summary>
    /// <returns></returns>
    private float GetCurClockwiseAngle()
    {
        //由于读取到的值是[0, 180] U [-180, 0]，左边由0至180递增，右边由180转变成-180，然后递增至0，所以需要转相应的转换
        return (-1) * (360 - NodeTran.eulerAngles.z) % 360;
    }

    /// <summary>
    /// 开始转动 //传入奖品信息（带停止角度）
    /// </summary>
    public void OnStartSpin()
    {
        if(_spinWheelStateType == SpinWheelStateType.None)
        {
            //Test 随机停止
            int index = UnityEngine.Random.Range(0, AwardItemBgs.Length);
            float angle = index * AwardItemBgs.Length / 360f;
            //

            EndAngle = angle;
            _spinWheelStateType = SpinWheelStateType.Start;

            //Total后自动停止
            StartCoroutine(CountDown(TotalTime, () =>
            {
                _spinWheelStateType = SpinWheelStateType.Prepared;
            }));
        }
    }
    /// <summary>
    /// 停止转动
    /// </summary>
    private void StopSpin()
    {
        ChangeSelectedAwardBg(_selectAwardIndex);

        //TODO 处理中奖
    }

    public void ChangeSelectedAwardBg(int index)
    {
        if (AwardItemBgs.Length > 0 && index < AwardItemBgs.Length)
        {
            Sprite lastSprite = AwardItemBgs[index].sprite;
            AwardItemBgs[index].sprite = SelectAwardAngleBg;

            AwardItemTexts[index].color = SelectAwardTextColor;

            //1.5s后自动停止
            StartCoroutine(CountDown(1.5f, () =>
            {
                AwardItemBgs[index].sprite = lastSprite;
                AwardItemTexts[index].color = awardTextNormalColor;
            }));
        }
    }
    
    IEnumerator CountDown(float time,UnityAction action)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }
}
