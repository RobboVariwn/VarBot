namespace Varwin
{
    public static class VCompare
    {
        public new static bool Equals(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            dynamic resultA = a == null ? 0 : a;
            dynamic resultB = b == null ? 0 : b;

            if (resultA.GetType() != resultB.GetType())
            {
                DynamicCast.CastValue(a, b, ref resultA, ref resultB);
            }

            return resultA == resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool NotEquals(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            dynamic resultA = a == null ? 0 : a;
            dynamic resultB = b == null ? 0 : b;

            if (resultA.GetType() != resultB.GetType())
            {
                DynamicCast.CastValue(a, b, ref resultA, ref resultB);
            }

            return resultA != resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool Less(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);
            
            return resultA < resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool LessOrEquals(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA <= resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool Greater(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA > resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool GreaterOrEquals(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            double resultA = DynamicCast.ConvertToDouble(a);
            double resultB = DynamicCast.ConvertToDouble(b);

            return resultA >= resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool And(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            bool resultA = DynamicCast.ConvertToBoolean(a);
            bool resultB = DynamicCast.ConvertToBoolean(b);

            return resultA && resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool Or(dynamic a, dynamic b)
        {
#if !NET_STANDARD_2_0
            bool resultA = DynamicCast.ConvertToBoolean(a);
            bool resultB = DynamicCast.ConvertToBoolean(b);

            return resultA || resultB;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool Not(dynamic a)
        {
#if !NET_STANDARD_2_0
            bool resultA = DynamicCast.ConvertToBoolean(a);

            return !resultA;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static bool NotEmpty(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a == null)
            {
                return false;
            }
            
            if (a is bool && a == false)
            {
                return false;
            }

            if (a is string)
            {
                return !string.IsNullOrEmpty(a);
            }

            if (((object) a).IsNumericType())
            {
                return a != 0;
            }

            return true;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
    }
}