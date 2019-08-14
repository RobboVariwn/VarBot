using System.Collections;
using System.Collections.Generic;

namespace Varwin
{
    public static class Utils
    {
        public static string ConvertToString(object o)
        {
            return o.ToString();
        }

        public static List<T> GetElements<T>(IList list, int from, int to)
        {
            int i = 0;
            List<T> result = new List<T>();
            
            foreach (object o in list)
            {
                if (i >= from)
                {
                    result.Add((T)o);
                }

                if (i > to)
                {
                    break;
                }
            }

            return result;
        }

        #region Random Numbers Generator

            private static readonly System.Random randomService = new System.Random();

            public static int RandomInt(int min, int max)
            {
                lock (randomService)
                {
                    return randomService.Next(min, max);
                }
            }

            public static double RandomDouble()
            {
                lock (randomService)
                {
                    return randomService.NextDouble();
                }
            }

        #endregion
    }
}
