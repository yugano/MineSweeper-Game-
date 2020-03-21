using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mayınTarlası
{
    public class ElapsedTime
    {
        Stopwatch stopwatch;
        long elapsedTime;

        public ElapsedTime()
        {
            elapsedTime = 0;
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        // getting elapsed time in watch format like 01 : 57
        public string TimeInHourFormat()
        {          
            elapsedTime = stopwatch.ElapsedMilliseconds / 1000;
            int second = (int) elapsedTime % 60;
            int minute = (int) (elapsedTime % 3600) / 60;
            string result = string.Empty;

            if (second < 10)
            {
                result = $": 0{second}";
            }
            else  
            {
                result = $": {second}";
            }

            if (minute < 10)
            {                
                result = $"0{minute} {result}";
            }
            else  
            {
                result = $"{minute} {result}";
            }

            if (elapsedTime > 3600)
            {
                int hour = (int)elapsedTime / 3600;
                result = $"{hour} : {result}";
            }

            return result;
        }

        public void StopTimer()
        {
            stopwatch.Stop();
        }

    }
}
