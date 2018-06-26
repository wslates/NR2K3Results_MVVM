using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NR2K3Results_MVVM.Model
{
    class AddDeleteOrModifySeriesMessage:MessageBase
    {
        public String newSeries;
        public AddDeleteOrModifySeriesMessage(String series)
        {
            newSeries = series;
        }
    }

    class SendDataToSeriesView : MessageBase
    {
        public String series;
        public SendDataToSeriesView(String series)
        {
            this.series = series;
        }
    }
}
