using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NR2K3Results_MVVM.Model
{
    class Driver: IComparable<Driver>
    {
        public string firstName;
        public string lastName;
        public string number;
        public string sponsor;
        public string team;
        public bool isPlayer;
        public DriverResult result;

        public int CompareTo(Driver other)
        {
            if (other.result.finish>result.finish)
            {
                return -1;
            } else
            {
                return 1;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Driver)) { return false; }

            Driver driver = obj as Driver;

            bool equal = driver.lastName.Equals(lastName) && driver.firstName[0] == firstName[0] && driver.number.Equals(number);

            return (equal && isPlayer) || equal;
        }

        public int GetFinish()
        {
            return result.finish;
        }

        public string GetSpeed()
        {
            return String.Format("{0:0.000}", result.speed);
        }
        public void SetTime(TimeSpan time)
        {
            result.time = time;
        }

        public void SetTimeOffLeader(TimeSpan time)
        {
            result.timeOffLeader = time;
        }

        public void SetTimeOffNext(TimeSpan time)
        {
            result.timeOffNext = time;
        }

        public string GetTime()
        {
            
            return String.Format("{0:0.000}", result.time.TotalSeconds);
            
        }

        public string GetOffLeader()
        {
            
            if (GetFinish() == 1)
            {
                return "---.---";
            }  else if (result.lapsDown!=0)
            {
                return result.lapsDown.ToString() + "L";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("-");
            builder.Append(String.Format("{0:0.000}", result.timeOffLeader.TotalSeconds));
           
            return builder.ToString();
        }

        public string GetOffNext()
        {
            if (GetFinish() == 1)
            {
                return "---.---";
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("-");
            
            builder.Append(String.Format("{0:0.000}", result.timeOffNext.TotalSeconds));

            return builder.ToString();

        }
        public String GetName()
        {
            return String.Format("{0} {1}", firstName, lastName);
        }
        public override string ToString()
        {
            return "#" + number +
                    "; " + firstName + " " + lastName +
                    "; " + sponsor +
                    "; " + team;
        }


        
    }
   
        
}
