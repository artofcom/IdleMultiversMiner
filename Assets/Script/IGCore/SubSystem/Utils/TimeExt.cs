using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System;

namespace Core.Utils
{
    public static class TimeExt
    {
        public enum UnitOption { NO_USE, FULL_NAME, SHORT_NAME };
        public enum TimeOption { ALL, DAY, HOUR, MIN, SEC };
        
        
        // d:h:m:s
        public static string ToTimeStringWithUnit(long second, bool fullUnitName = false, TimeOption timeOption = TimeOption.DAY)
        {
            return ToTimeString(second, fullUnitName ? UnitOption.FULL_NAME : UnitOption.SHORT_NAME, timeOption);
        }

        // d:h:m:s
        public static string ToTimeString(long second, UnitOption option = UnitOption.NO_USE, TimeOption timeOption = TimeOption.DAY, bool useUpperCase = false)
        {
            string strDay, strHour, strMin, strSec;
            switch(option)
            {
                case UnitOption.FULL_NAME: strDay = "day"; strHour = "hour"; strMin = "min"; strSec = "sec"; break;
                case UnitOption.SHORT_NAME: strDay = "d"; strHour = "h"; strMin = "m"; strSec = "s"; break;
                case UnitOption.NO_USE:
                default: 
                    strDay = "";    strHour = "";       strMin = "";    strSec = "";    
                    break;
            }
            if(useUpperCase)
            {
                strDay = strDay.ToUpper();      strHour = strHour.ToUpper();    strMin = strMin.ToUpper();     strSec = strSec.ToUpper();
            }

            //string dayRet = timeOption==TimeOption.DAY ? "00" + strDay : string.Empty; 

            long second2 = 0;
            if (second < 60)    // sec - less than min
            {
                switch(timeOption)
                {
                case TimeOption.HOUR:   return string.Format("00{0}:00{1}:{2:D2}{3}", strHour, strMin, second, strSec);
                case TimeOption.MIN:    return string.Format("00{0}:{1:D2}{2}", strMin, second, strSec);
                case TimeOption.SEC:    return string.Format("{0:D2}{1}", second, strSec);
                
                default:
                case TimeOption.DAY:    return string.Format("00{0}:00{1}:00{2}:{3:D2}{4}", strDay, strHour, strMin, second, strSec);
                }
            }

            int m, s, h;
            if (second < 3600)  // min - less than hour
            {
                m = (int)second / 60;
                s = (int)second % 60;
                switch(timeOption)
                {
                case TimeOption.HOUR:   return string.Format("00{0}:{1:D2}{2}:{3:D2}{4}", strHour, m, strMin, s, strSec);
                case TimeOption.MIN:    return string.Format("{0:D2}{1}:{2:D2}{3}", m, strMin, s, strSec);
                case TimeOption.SEC:    return string.Format("{0:D2}{1}", second, strSec);
                
                default:
                case TimeOption.DAY:    return string.Format("00{0}:00{1}:{2:D2}{3}:{4:D2}{5}", strDay, strHour, m, strMin, s, strSec);
                }
            }

            if (second < 24 * 3600)// hours - less than a day
            {
                h = (int)second / 3600;
                second2 = second - (3600 * h);
                m = (int)second2 / 60;
                s = (int)second2 % 60;

                switch(timeOption)
                {
                case TimeOption.HOUR:   return string.Format("{0:D2}{1}:{2:D2}{3}:{4:D2}{5}", h, strHour, m, strMin, s, strSec);

                case TimeOption.MIN:    
                    m = (int)second / 60;
                    return string.Format("{0:D2}{1}:{2:D2}{3}", m, strMin, s, strSec);
                case TimeOption.SEC:    return string.Format("{0:D2}{1}", second, strSec);

                default:
                case TimeOption.DAY:    return string.Format("00{0}:{1:D2}{2}:{3:D2}{4}:{5:D2}{6}", strDay, h, strHour, m, strMin, s, strSec);
                }
            }

            // more than days.

            int d = (int)second / (24 * 3600);
            second2 = second - (24 * 3600 * d);
            h = (int)second2 / 3600;
            second2 = second2 - 3600 * h;
            m = (int)second2 / 60;
            s = (int)second2 % 60;

            switch(timeOption)
            {
            case TimeOption.HOUR:   
                h = (int)second / 3600;
                return string.Format("{0:D2}{1}:{2:D2}{3}:{4:D2}{5}", h, strHour, m, strMin, s, strSec);

            case TimeOption.MIN:    
                m = (int)second / 60;
                return string.Format("{0:D2}{1}:{2:D2}{3}", m, strMin, s, strSec);
            
            case TimeOption.SEC:    
                return string.Format("{0:D2}{1}", second, strSec);

            default:
            case TimeOption.DAY:    return string.Format("{0:D2}{1}:{2:D2}{3}:{4:D2}{5}:{6:D2}{7}", d, strDay, h, strHour, m, strMin, s, strSec);
            }

//            return string.Format("{0:D2} {1}:{2:D2} {3}:{4:D2} {5}:{6:D2} {7}", d, strDay, h, strHour, m, strMin, s, strSec);
            //return $"{d}{strDay}:{h}{strHour}:{m}{strMin}:{s}{strSec}";

        }
    }
}
