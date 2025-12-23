using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Numerics;
using System;

namespace Core.Utils
{
    public static class BigIntegerExt
    {
        static char[] Unit = new char[] { ' ', 'K', 'M', 'G', 'T', 'P', 'E', 'Z', 'Y',
                         'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
                         'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'  };


        public static string ToAbbString(this BigInteger number)
        {
            const int div = 1000;
            int idx = 0;
            int belowDot = 0;
            BigInteger big = number;
            while (big > 1000)
            {
                belowDot = (int)(big % div);
                big = big / div;
                ++idx;    
            }

            idx = Mathf.Min(idx, Unit.Length - 1);

            if (idx > 0)
                return (big + "." + (belowDot / 10).ToString() + Unit[idx].ToString());
            else
                return big.ToString();

        }

        public static BigInteger SimpleRound(this BigInteger biValue)
        { 
            if(biValue < 10)        return biValue;
            if(biValue < 100)       // 10 ~ 99
            {
                return biValue - (biValue%10);
            }
            if(biValue < 1000)      // 100 ~ 999
            {
                return biValue - (biValue%100);
            }
            return biValue - (biValue%100);
        }
    }
}
