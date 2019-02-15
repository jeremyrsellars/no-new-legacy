using System;
using System.Linq;

namespace Sheepish.CSharp
{
    public class Sheepish
    {
        public static bool IsSheepBleat(string text) =>
            text[0] == 'b' && text.Substring(1).All('a'.Equals);

        //public static bool IsSheepBleat(string text) =>
        //    text.Length >= 3  // This should fix it!
        //    && text[0] == 'b' && text.Substring(1).All('a'.Equals);

        //public static bool IsSheepBleat(string text)
        //{
        //    if (text.Length < 3 || text[0] != 'b')
        //        return false;
        //    for (var i = 1; i < text.Length; i++)
        //        if (text[i] != 'a')
        //            return false;
        //    return true;
        //}
    }
}
