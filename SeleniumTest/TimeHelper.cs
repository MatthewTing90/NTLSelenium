using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTest
{
    class TimeHelper
    {
        public static string FormatNum(int num)
        {
            return (num == 0) ? "00" : (num < 10) ? $"0{num}" : $"{num}";
        }
        public static string GetTime(int seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return $"{FormatNum(t.Hours)}:{FormatNum(t.Minutes)}:{FormatNum(t.Seconds)}";
        }
    }
}
