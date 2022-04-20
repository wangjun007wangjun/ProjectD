/********************************************************************
	created:	20:11:2017   15:27
	author:		OneJun
	purpose:	日期时间工具
*********************************************************************/
using System;
namespace Engine.TimeSync
{
    public static class TimeHelper
    {
        // 一天的秒数
        public const ulong SecsPerDay = 24 * 3600;
        public const ulong SecsPerHour = 3600;
        public const ulong SecsPerMin = 60;
        //UtC 启点
        private static readonly DateTime _localOrgTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
       
        /// <summary>
        /// Utc 时间戳秒转换成DateTime 格式
        /// </summary>
        /// <param name="unixTimeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            //起点开始
            DateTime dtDateTime = _localOrgTimeUtc;
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
            return dtDateTime;
        }

        /// <summary>
        /// DateTime格式转换成时间戳
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static double DateTimeToUnixTimeStamp(DateTime time)
        {
            DateTime dtDateTime = _localOrgTimeUtc;
            return (time.ToUniversalTime() - dtDateTime).TotalSeconds;
        }

        /// <summary>
        /// 根据时间差计算还剩多少时间
        /// </summary>
        /// <param name="leftTime">时间差</param>
        /// <param name="leftDay">天</param>
        /// <param name="leftHour">时</param>
        /// <param name="leftMinute">分</param>
        /// <param name="leftSec">秒</param>
        public static void CalculateLeftTime(ulong leftTime, ref int leftDay, ref int leftHour, ref int leftMinute, ref int leftSec)
        {
            leftDay = leftHour = leftMinute = leftSec = 0;
            if (leftTime > 0)
            {
                leftDay = (int)(leftTime / SecsPerDay);
                leftTime = leftTime % SecsPerDay;
                leftHour = (int)(leftTime / SecsPerHour);
                leftTime = leftTime % SecsPerHour;
                leftMinute = (int)(leftTime / SecsPerMin);
                leftSec = (int)(leftTime % SecsPerMin);
            }
        }

        /// <summary>
        /// 从时间戳获取对应时间
        /// </summary>
        /// <param name="timeTicks">时间戳(秒)</param>
        /// <returns></returns>
        public static DateTime GetDateTimeBySecondTicks(long timeTicks)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(_localOrgTimeUtc);
            DateTime dtRst = dtStart.AddSeconds(timeTicks);
            return dtRst;
        }

        public static string GetCurDateTimeStr()
        {
            return DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

