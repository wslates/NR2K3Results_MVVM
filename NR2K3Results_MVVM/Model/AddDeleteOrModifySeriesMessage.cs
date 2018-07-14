using GalaSoft.MvvmLight.Messaging;
using System;


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
