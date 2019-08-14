using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Text;
#if !UNITY_EDITOR
using NLog;
#endif
using UnityEngine;
using Random = System.Random;


namespace Varwin
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class VString
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static string ConvertToString(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a == null)
            {
                return "";
            }

            string result = "";

            try
            {
                result = a.ToString();
            }
            catch (Exception e)
            {
                Log($"Can not convert {a} to string");
            }
            return result;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static string Concat(params dynamic[] values)
        {
#if !NET_STANDARD_2_0
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            
            int totalLength = 0;
            
            string[] convertedValues = new string[values.Length];

            for (int i = 0; i < values.Length; ++i)
            {
                string str = ConvertToString(values[i]);

                convertedValues[i] = str;
                    
                totalLength += str.Length;

                if (totalLength < 0)
                {
                    throw new OutOfMemoryException();
                }
            }

            return string.Concat(convertedValues);
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static int Length(dynamic a)
        {
#if !NET_STANDARD_2_0
            if (a == null)
            {
                return 0;
            }
            string resultA = ConvertToString(a);

            return resultA.Length;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static int IndexOf(dynamic text, dynamic findText)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);
            string resultFindText = ConvertToString(findText);

            return resultText.IndexOf(resultFindText, StringComparison.Ordinal) + 1;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static int LastIndexOf(dynamic text, dynamic findText)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);
            string resultFindText = ConvertToString(findText);

            return resultText.LastIndexOf(resultFindText, StringComparison.Ordinal) + 1;
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static string Trim(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            return resultText.Trim();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static string TrimStart(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            return resultText.TrimStart();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static string TrimEnd(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            return resultText.TrimEnd();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static string ToUpper(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            return resultText.ToUpper();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static string ToLower(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            return resultText.ToLower();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static string ToTitleCase(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            StringBuilder buf = new StringBuilder(resultText.Length);
            bool toUpper = true;

            foreach (char ch in resultText)
            {
                buf.Append(toUpper ? char.ToUpper(ch) : ch);
                toUpper = char.IsWhiteSpace(ch);
            }

            return buf.ToString();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static char GetLetterFromStart(dynamic text, dynamic index)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);
            int resultIndex = DynamicCast.ConvertToInt(index);
            
            if (string.IsNullOrEmpty(resultText) || resultIndex == 0)
            {
                return '\0';
            }
            
            return resultText.Length > resultIndex - 1 ? resultText[resultIndex - 1] : '\0';
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static char GetLetterFromEnd(dynamic text, dynamic index)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);
            int resultIndex = DynamicCast.ConvertToInt(index);
            
            if (string.IsNullOrEmpty(resultText) || resultIndex == 0)
            {
                return '\0';
            }
            
            return resultText.Length - resultIndex > 0 ? resultText[resultText.Length - resultIndex] : '\0';
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static char GetLetterFirst(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            return resultText.Length == 0 ? '\0' : resultText.First();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static char GetLetterLast(dynamic text)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            return resultText.Length == 0 ? '\0' : resultText.Last();
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }
        
        public static char GetLetterRandom(dynamic text) 
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);

            if (resultText.Length == 0)
            {
                return '\0';
            }
            
            dynamic x = new Random().Next(resultText.Length);
            return resultText[x];
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

        public static string Substring(dynamic text, string where1, dynamic at1, string where2, dynamic at2)
        {
#if !NET_STANDARD_2_0
            string resultText = ConvertToString(text);
            int at1Result = DynamicCast.ConvertToInt(at1);
            int at2Result = DynamicCast.ConvertToInt(at2);

            var getAt = new Func<dynamic, int, int>((where, at) =>
            {
                switch (@where)
                {
                    case "FROM_START":
                        at--;

                        break;
                    case "FROM_END":
                        at = resultText.Length - at;

                        break;
                    case "FIRST":
                        at = 0;

                        break;
                    case "LAST":
                        at = resultText.Length - 1;

                        break;
                    default:

                        throw new ApplicationException("Unhandled option (GetSubstring).");
                }

                return at;
            });
            at1 = getAt(where1, at1Result);
            at2 = getAt(where2, at2Result) + 1;

            try
            {
                return resultText.Substring(at1, at2 - at1);
            }
            catch (Exception e)
            {
                Log($"Can not get Substring({at1}, {at2 - at1}); for {resultText}.");

                return "";
            }
#else
            throw new WrongApiCompatibilityLevelException();
#endif
        }

#if !NET_STANDARD_2_0
        public static bool NotEmpty(dynamic text) => VCompare.NotEmpty(text);
#else
        public static bool NotEmpty(dynamic text) => throw new WrongApiCompatibilityLevelException();
#endif

        private static void Log(string message)
        {
#if UNITY_EDITOR
            Debug.Log(message);
#else
            LogManager.GetCurrentClassLogger().Info(message);
#endif
        }
    }

}