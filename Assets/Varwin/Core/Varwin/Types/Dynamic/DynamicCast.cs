using System;
using NLog;
using UnityEngine;

namespace Varwin
{
    public static class DynamicCast
    {
        
        public static dynamic DefaultNullValue(Type resultType)
        {
            dynamic result = 0;

            if (resultType == typeof(float))  
            {
                result = 0f;
            }
            
            if (resultType == typeof(decimal))  
            {
                result = 0m;
            }
            
            if (resultType == typeof(double))  
            {
                result = 0d;
            }
            
            if (resultType == typeof(int))  
            {
                result = 0;
            }

            if (resultType == typeof(string))
            {
                result = "";
            }

            if (resultType == typeof(bool))
            {
                result = false;
            }

            return result;
        }
        
        public static bool IsNumericType(this object o)
        {   
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static double ConvertToDouble(dynamic a)
        {
#if !NET_STANDARD_2_0
            dynamic resultA = a == null ? 0d : a;

            if (a is double || a is float || a is int)
            {
                return a;
            }

            if (a is bool)
            {
                return a ? 1d : 0d;
            }

            string resultStringA = resultA.ToString();
            resultStringA = resultStringA.Replace(".", ",");

            try
            {
                resultA = Convert.ToDouble(resultStringA);

                return resultA;
            }

            catch
            {
                Log($"Can not convert \"{resultStringA}\" to double!");

                return 0d;
            }

#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static void CastValue(dynamic a, dynamic b, ref dynamic resultA, ref dynamic resultB)
        {
#if !NET_STANDARD_2_0
            resultA = a == null ? 0 : a;
            resultB = b == null ? 0 : b;

            Type resultType = resultA.GetType();

            string stringB = "";

            if (resultB is bool)
            {
                stringB = (resultB == true) ? "1" : "";               
            }
            else
            {
                stringB = resultB.ToString().Replace(".", ",");    
            }
            
            if (resultA is bool)
            {
                try
                {                  
                    if (resultB is string)
                    {
                        if (resultA == false && resultB == "")
                        {
                            resultB = false;
                        }
                        else if (ConvertToDouble(stringB) == 1.0)
                        {
                            resultB = true;
                        }
                        else
                        {
                            resultB = !resultA;
                        }
                    }
                    else
                    {
                        resultB = (ConvertToDouble(stringB) == 1.0);
                    }

                }
                catch
                {
                    Log($"Can not convert {b} to boolean");
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }
            
            if (resultA is float)
            {
                try
                {
                    resultB = Convert.ToSingle(stringB);
                }
                catch
                {
                    Log($"Can not convert {b} to float");
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is int)
            {
                try
                {
                    resultB = Convert.ToInt32(stringB);
                }
                catch
                {
                    Log($"Can not convert {b} to int");
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is decimal)
            {
                try
                {
                    resultB = Convert.ToDecimal(stringB);
                }
                catch
                {
                    Log($"Can not convert {b} to decimal");
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is double)
            {
                try
                {
                    resultB = Convert.ToDouble(stringB);
                }
                catch
                {
                    Log($"Can not convert {b} to double");
                    resultB = DefaultNullValue(resultType);
                }

                return;
            }

            if (resultA is string)
            {
                resultB = stringB;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif            
        }

        private static void Log(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#else
            LogManager.GetCurrentClassLogger().Info(message);
#endif
        }

        public static int ConvertToInt(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                return 0;
            }

            if (a is int)
            {
                return a;
            }
            
            if (a is bool)
            {
                return a ? 1 : 0;
            }

            string stringInt = VString.ConvertToString(a);
            stringInt = stringInt.Replace(".", ",");

            try
            {
                double f = Convert.ToDouble(stringInt);
                int result = (int) Math.Floor(f + 0.5f);

                return result;

            }
            catch (Exception e)
            {
                Log($"Cannot convert {a} to int");

                return 0;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static bool ConvertToBoolean(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a is null)
            {
                return false;
            }

            if (a is bool)
            {
                return a;
            }

            string stringBool = VString.ConvertToString(a);

            try
            {
                bool result = Convert.ToBoolean(stringBool);

                return result;

            }
            catch (Exception e)
            {
                Log($"Cannot convert {a} to bool");

                return false;
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

       
    }

}