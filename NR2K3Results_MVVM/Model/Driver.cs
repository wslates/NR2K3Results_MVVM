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

            if (driver.lastName.Equals(lastName) && 
                driver.firstName[0]==firstName[0] && 
                driver.number.Equals(number))
            {
                return true;
            }

            return false;
        }

        public int GetFinish()
        {
            return result.finish;
        }

        public string GetSpeed()
        {
            return Decimal.Round(result.speed, 3).ToString();
        }

        public string GetTime()
        {
            return Decimal.Round(result.time, 3).ToString();
        }

        public string GetOffLeader()
        {
            if (GetFinish() == 1)
            {
                return "---.---";
            }  else if (result.timeOffLeader == .000001m)
            {
                return result.lapsDown;
            }
            return Decimal.Round(result.timeOffLeader, 3).ToString();
        }

        public string GetOffNext()
        {
            if (GetFinish() == 1)
            {
                return "---.---";
            } else if(result.timeOffNext == 0)
            {
                return "-" + Decimal.Round(result.timeOffNext, 3).ToString();
            }

            return Decimal.Round(result.timeOffNext, 3).ToString();
        }
        public override string ToString()
        {
            return  "#" + number + 
                    "; " + firstName + " " + lastName + 
                    "; " + sponsor + 
                    "; " + team +
                    "; " + result.finish + 
                    "; " + result.time + 
                    "; " + result.timeOffLeader + 
                    "; " + result.timeOffNext;
        }

        
    }
   
        
}
