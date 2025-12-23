using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Tween
{
    public class EaseFuncLib : MonoBehaviour
    {
        public enum DurationEase
        {
            Linear,
            SineEaseIn,
            SineEaseOut,
            SineEaseInOut,
            QuadraticEaseIn,
            QuadraticEaseOut,
            QuadraticEaseInOut,
            CubicEaseIn,
            CubicEaseOut,
            CubicEaseInOut,
            QuarticEaseIn,
            QuarticEaseOut,
            QuarticEaseInOut,
            QuinticEaseIn,
            QuinticEaseOut,
            QuinticEaseInOut,
            ExponentialEaseIn,
            ExponentialEaseOut,
            ExponentialEaseInOut,
            CircularEaseIn,
            CircularEaseOut,
            CircularEaseInOut,
            BackEaseIn,
            BackEaseOut,
            BackEaseInOut,
            ElasticEaseIn,
            ElasticEaseOut,
            ElasticEaseInOut,
            //        BounceEaseIn,
            BounceEaseOut
            //        BounceEaseInOut
        };

        [SerializeField] protected DurationEase EaseType = DurationEase.Linear;


        protected System.Func<float, float, float> durationEaseFunc;

        protected void UpdateEaseFunction()
        {
            switch (EaseType)
            {
                case DurationEase.Linear:
                    durationEaseFunc = Linear;
                    break;
                case DurationEase.SineEaseIn:
                    durationEaseFunc = SineEaseIn;
                    break;
                case DurationEase.SineEaseOut:
                    durationEaseFunc = SineEaseOut;
                    break;
                case DurationEase.SineEaseInOut:
                    durationEaseFunc = SineEaseInOut;
                    break;
                case DurationEase.QuadraticEaseIn:
                    durationEaseFunc = QuadraticEaseIn;
                    break;
                case DurationEase.QuadraticEaseOut:
                    durationEaseFunc = QuadraticEaseOut;
                    break;
                case DurationEase.QuadraticEaseInOut:
                    durationEaseFunc = QuadraticEaseInOut;
                    break;
                case DurationEase.CubicEaseIn:
                    durationEaseFunc = CubicEaseIn;
                    break;
                case DurationEase.CubicEaseOut:
                    durationEaseFunc = CubicEaseOut;
                    break;
                case DurationEase.CubicEaseInOut:
                    durationEaseFunc = CubicEaseInOut;
                    break;
                case DurationEase.QuarticEaseIn:
                    durationEaseFunc = QuarticEaseIn;
                    break;
                case DurationEase.QuarticEaseOut:
                    durationEaseFunc = QuarticEaseOut;
                    break;
                case DurationEase.QuarticEaseInOut:
                    durationEaseFunc = QuarticEaseInOut;
                    break;
                case DurationEase.QuinticEaseIn:
                    durationEaseFunc = QuinticEaseIn;
                    break;
                case DurationEase.QuinticEaseOut:
                    durationEaseFunc = QuinticEaseOut;
                    break;
                case DurationEase.QuinticEaseInOut:
                    durationEaseFunc = QuinticEaseInOut;
                    break;
                case DurationEase.ExponentialEaseIn:
                    durationEaseFunc = ExponentialEaseIn;
                    break;
                case DurationEase.ExponentialEaseOut:
                    durationEaseFunc = ExponentialEaseOut;
                    break;
                case DurationEase.ExponentialEaseInOut:
                    durationEaseFunc = ExponentialEaseInOut;
                    break;
                case DurationEase.CircularEaseIn:
                    durationEaseFunc = CircularEaseIn;
                    break;
                case DurationEase.CircularEaseOut:
                    durationEaseFunc = CircularEaseOut;
                    break;
                case DurationEase.CircularEaseInOut:
                    durationEaseFunc = CircularEaseInOut;
                    break;
                case DurationEase.BackEaseIn:
                    durationEaseFunc = BackEaseIn;
                    break;
                case DurationEase.BackEaseOut:
                    durationEaseFunc = BackEaseOut;
                    break;
                case DurationEase.BackEaseInOut:
                    durationEaseFunc = BackEaseInOut;
                    break;
                case DurationEase.ElasticEaseIn:
                    durationEaseFunc = ElasticEaseIn;
                    break;
                case DurationEase.ElasticEaseOut:
                    durationEaseFunc = ElasticEaseOut;
                    break;
                case DurationEase.ElasticEaseInOut:
                    durationEaseFunc = ElasticEaseInOut;
                    break;
                case DurationEase.BounceEaseOut:
                    durationEaseFunc = BounceEaseOut;
                    break;
                default:
                    durationEaseFunc = Linear;
                    break;
            }
        }



        private const float PI = Mathf.PI;
        private const float HALFPI = Mathf.PI / 2.0f;

        //////////////////// LINEAR FUNCTIONS ////////////////////
        public static float Linear(float timer, float duration)
        {
            return timer / duration;
        }


        //////////////////// SINUSOIDAL FUNCTIONS ////////////////////
        public static float SineEaseIn(float timer, float duration)
        {
            return Mathf.Sin(((timer / duration) - 1) * HALFPI) + 1;
        }

        public static float SineEaseOut(float timer, float duration)
        {
            return Mathf.Sin((timer / duration) * HALFPI);
        }

        public static float SineEaseInOut(float timer, float duration)
        {
            return 0.5f * (1 - Mathf.Cos((timer / duration) * PI));
        }


        //////////////////// QUADRATIC FUNCTIONS ////////////////////
        public static float QuadraticEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return t * t;
        }

        public static float QuadraticEaseOut(float timer, float duration)
        {
            float t = timer / duration;
            return -(t * (t - 2));
        }

        public static float QuadraticEaseInOut(float timer, float duration)
        {
            float t = timer / duration;

            if (t < 0.5f)
            {
                return 2 * t * t;
            }
            else
            {
                return (-2 * t * t) + (4 * t) - 1;
            }

        }


        //////////////////// CUBIC FUNCTIONS ////////////////////
        public static float CubicEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return t * t * t;
            //        return 1 * t * t * t + 0;
        }

        public static float CubicEaseOut(float timer, float duration)
        {
            float f = ((timer / duration) - 1);
            return f * f * f + 1;
        }

        public static float CubicEaseInOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t < 0.5f)
            {
                return 4 * t * t * t;
            }
            else
            {
                float f = ((2 * t) - 2);
                return 0.5f * f * f * f + 1;
            }

        }


        //////////////////// QUARTIC FUNCTIONS ////////////////////
        public static float QuarticEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return t * t * t * t;
        }

        public static float QuarticEaseOut(float timer, float duration)
        {
            float t = timer / duration;
            float f = (t - 1);
            return f * f * f * (1 - t) + 1;
        }

        public static float QuarticEaseInOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t < 0.5f)
            {
                return 8 * t * t * t * t;
            }
            else
            {
                float f = (t - 1);
                return -8 * f * f * f * f + 1;
            }

        }


        //////////////////// QUINTIC FUNCTIONS ////////////////////
        public static float QuinticEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return t * t * t * t * t;
        }

        public static float QuinticEaseOut(float timer, float duration)
        {
            float f = ((timer / duration) - 1);
            return f * f * f * f * f + 1;
        }

        public static float QuinticEaseInOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t < 0.5f)
            {
                return 16 * t * t * t * t * t;
            }
            else
            {
                float f = ((2 * t) - 2);
                return 0.5f * f * f * f * f * f + 1;
            }

        }


        //////////////////// EXPONENTIAL FUNCTIONS ////////////////////
        public static float ExponentialEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return (t == 0.0f) ? t : Mathf.Pow(2, 10 * (t - 1));
        }

        public static float ExponentialEaseOut(float timer, float duration)
        {
            float t = timer / duration;
            return (t == 1.0f) ? t : 1 - Mathf.Pow(2, -10 * t);
        }

        public static float ExponentialEaseInOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t == 0.0 || t == 1.0) return t;

            if (t < 0.5f)
            {
                return 0.5f * Mathf.Pow(2, (20 * t) - 10);
            }
            else
            {
                return -0.5f * Mathf.Pow(2, (-20 * t) + 10) + 1;
            }

        }


        //////////////////// CIRCULAR FUNCTIONS ////////////////////
        public static float CircularEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return 1 - Mathf.Sqrt(1 - (t * t));
        }

        public static float CircularEaseOut(float timer, float duration)
        {
            float t = timer / duration;
            return Mathf.Sqrt((2 - t) * t);
        }

        public static float CircularEaseInOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t < 0.5f)
            {
                return 0.5f * (1 - Mathf.Sqrt(1 - 4 * (t * t)));
            }
            else
            {
                return 0.5f * (Mathf.Sqrt(-((2 * t) - 3) * ((2 * t) - 1)) + 1);
            }

        }


        //////////////////// BACK FUNCTIONS ////////////////////
        public static float BackEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return t * t * t - t * Mathf.Sin(t * PI);
        }

        public static float BackEaseOut(float timer, float duration)
        {
            float f = (1 - (timer / duration));
            return 1 - (f * f * f - f * Mathf.Sin(f * PI));
        }

        public static float BackEaseInOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t < 0.5f)
            {
                float f = 2 * t;
                return 0.5f * (f * f * f - f * Mathf.Sin(f * PI));
            }
            else
            {
                float f = (1 - (2 * t - 1));
                return 0.5f * (1 - (f * f * f - f * Mathf.Sin(f * PI))) + 0.5f;
            }

        }


        //////////////////// ELASTIC FUNCTIONS ////////////////////
        public static float ElasticEaseIn(float timer, float duration)
        {
            float t = timer / duration;
            return Mathf.Sin(13 * HALFPI * t) * Mathf.Pow(2, 10 * (t - 1));
        }

        public static float ElasticEaseOut(float timer, float duration)
        {
            float t = timer / duration;
            return Mathf.Sin(-13 * HALFPI * (t + 1)) * Mathf.Pow(2, -10 * t) + 1;
        }

        public static float ElasticEaseInOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t < 0.5f)
            {
                return 0.5f * Mathf.Sin(13 * HALFPI * (2 * t)) * Mathf.Pow(2, 10 * ((2 * t) - 1));
            }
            else
            {
                return 0.5f * (Mathf.Sin(-13 * HALFPI * ((2 * t - 1) + 1)) * Mathf.Pow(2, -10 * (2 * t - 1)) + 2);
            }

        }


        //////////////////// BOUNCE FUNCTIONS ////////////////////
        //    public static float BounceEaseIn(float timer, float duration)
        //    {
        //        return 1 - BounceEaseOut(1 - (timer / duration), duration);
        //    }

        public static float BounceEaseOut(float timer, float duration)
        {
            float t = timer / duration;
            if (t < 4 / 11.0f)
            {
                return (121 * t * t) / 16.0f;
            }
            else if (t < 8 / 11.0f)
            {
                return (363 / 40.0f * t * t) - (99 / 10.0f * t) + 17 / 5.0f;
            }
            else if (t < 9 / 10.0f)
            {
                return (4356 / 361.0f * t * t) - (35442 / 1805.0f * t) + 16061 / 1805.0f;
            }
            else
            {
                return (54 / 5.0f * t * t) - (513 / 25.0f * t) + 268 / 25.0f;
            }

        }
    }
}
