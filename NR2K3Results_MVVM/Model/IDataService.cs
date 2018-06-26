using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NR2K3Results_MVVM.Model
{
    public interface IDataService
    {
        void GetData(Action<DataItem, Exception> callback);
    }
}
