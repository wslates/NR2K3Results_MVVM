using System;

namespace NR2K3Results_MVVM.Model
{
    class DriverResult
    {
        public int start;
        public int finish;
        public TimeSpan time;
        public TimeSpan timeOffLeader;
        public TimeSpan timeOffNext;
        public decimal speed;
        public int lapsLed;
        public int laps;
        public string status;
        public int lapsDown;
    }
}
