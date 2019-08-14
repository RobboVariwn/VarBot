using System;

namespace Varwin
{
    public static class VMath
    {
        
        //https://www.jetbrains.com/help/rider/CompareOfFloatsByEqualityOperator.html
        private const double Tolerance = 0.0000000001d;
        
        public static dynamic Sum(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA + resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Subtraction(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA - resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Multiply(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA * resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Division(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            if (Math.Abs(resultB) < Tolerance)
            {
                return double.PositiveInfinity;              
            }

            return resultA / resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Pow(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return Math.Pow(resultA, resultB);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Sqrt(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Sqrt(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Exp(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Exp(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Pow10(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Pow(10.0, resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Abs(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Abs(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Negative (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return -resultA;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Log (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Log(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Log10 (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Log10(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Sin (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Sin(resultA / 180 * Math.PI);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Cos (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Cos(resultA / 180 * Math.PI);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Tan (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Tan(resultA / 180 * Math.PI);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Asin (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Asin(resultA) / Math.PI * 180;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Acos (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Acos(resultA) / Math.PI * 180;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic Atan (dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Atan(resultA) / Math.PI * 180;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsEven(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Abs(resultA % 2) < Tolerance;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsOdd(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Abs(resultA % 2 - 1) < Tolerance;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool IsPrime(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            // http://en.wikipedia.org/wiki/Primality_test#Naive_methods
            if (Math.Abs(resultA - 2.0) < Tolerance || Math.Abs(resultA - 3.0) < Tolerance)
            {
                return true;
            }

            // False if n is NaN, negative, is 1, or not whole. And false if n is divisible by 2 or 3.
            if (
                double.IsNaN(resultA) || resultA <= 1 || Math.Abs(resultA % 1) > Tolerance || Math.Abs(resultA % 2) < Tolerance || Math.Abs(resultA % 3) < Tolerance)
            {
                return false;
            }

            // Check all the numbers of form 6k +/- 1, up to sqrt(n).
            for (var x = 6; x <= Math.Sqrt(resultA) + 1; x += 6)
            {
                if (Math.Abs(resultA % (x - 1)) < Tolerance || Math.Abs(resultA % (x + 1)) < Tolerance)
                {
                    return false;
                }
            }

            return true;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool IsWhole(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Abs(resultA % 1) < Tolerance;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool IsPositive(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return resultA > 0;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool IsNegative(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return resultA < 0;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool DivisionBy(dynamic a, dynamic divisionBy)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultDivisionBy = DynamicCast.ConvertToDouble(divisionBy);

            return Math.Abs(resultA % resultDivisionBy) < Tolerance;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic Round(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);

            return Math.Round(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic RoundUp(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            resultA += 0.5d;

            return Math.Round(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static dynamic RoundDown(dynamic a)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            resultA -= 0.5d;

            return Math.Round(resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic RandomInt(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            int resultA = DynamicCast.ConvertToInt(a);
            int resultB = DynamicCast.ConvertToInt(b);
            
            return resultA < resultB ? Utils.RandomInt(resultA, resultB) : Utils.RandomInt(resultB, resultA);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static dynamic RandomDouble() => Utils.RandomDouble();
    }
}