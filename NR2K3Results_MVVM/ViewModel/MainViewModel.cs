using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NR2K3Results_MVVM.Model;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace NR2K3Results_MVVM.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// See http://www.mvvmlight.net
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private String selectedSeries;
        private readonly IDataService _dataService;
        public String SelectedSeries
        {
            get
            {
                return selectedSeries;
            }
            set
            {
                selectedSeries = value;
                RaisePropertyChanged();
            }
        }
        public RelayCommand NewSeriesCommand { get; private set; }
        public RelayCommand EditSeriesCommand { get; private set; }
        public RelayCommand DeleteSeriesCommand { get; private set; }
        public RelayCommand OutputCommand { get; private set; }
        public RelayCommand ResultFileCommand { get; private set; }
        public RelayCommand NR2k3Command { get; private set; }
        public ObservableCollection<String> Series { get; private set; }
        /// <summary>
        /// The <see cref="WelcomeTitle" /> property's name.
        /// </summary>
        public const string WelcomeTitlePropertyName = "WelcomeTitle";

        private string _welcomeTitle = string.Empty;

        /// <summary>
        /// Gets the WelcomeTitle property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string WelcomeTitle
        {
            get
            {
                return _welcomeTitle;
            }
            set
            {
                Set(ref _welcomeTitle, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel(IDataService dataService)
        {
            _dataService = dataService;
            _dataService.GetData(
                (item, error) =>
                {
                    if (error != null)
                    {
                        // Report error here
                        return;
                    }

                    WelcomeTitle = item.Title;
                });
            NewSeriesCommand = new RelayCommand(NewSeriesCommandAction);
            EditSeriesCommand = new RelayCommand(EditSeriesCommandAction);
            DeleteSeriesCommand = new RelayCommand(DeleteSeriesCommandAction);
            OutputCommand = new RelayCommand(OutputCommandAction);
            ResultFileCommand = new RelayCommand(ResultFileCommandAction);
            NR2k3Command = new RelayCommand(NR2k3CommandAction);
            Messenger.Default.Register<Model.AddDeleteOrModifySeriesMessage>(this, UpdateSeries);
            Series = new ObservableCollection<String>();
            UpdateSeries();
        }

        private void UpdateSeries(AddDeleteOrModifySeriesMessage obj)
        {
            UpdateSeries();

            if (Series.Contains(obj.newSeries))
            {
                SelectedSeries = obj.newSeries;
            }
            
        }

        public void NewSeriesCommandAction()
        {
            View.SeriesWindow window = new View.SeriesWindow();
            window.ShowDialog();
            
        }

        public void EditSeriesCommandAction()
        { 
            if (selectedSeries!=null)
            {
                View.SeriesWindow window = new View.SeriesWindow();
                Messenger.Default.Send(new SendDataToSeriesView(SelectedSeries));
                window.ShowDialog();
            }
         
        }

        public void DeleteSeriesCommandAction()
        {
            if (selectedSeries!=null)
            {
                string message = string.Concat("Are you sure you want to delete ", SelectedSeries, "?");
                string title = string.Concat("Deleting ", selectedSeries);
                if (MessageBox.Show(message, title, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var db = new NR2k3ResultsEntities())
                    {
                        Series s = db.Series.Where(d => d.SeriesName.Equals(SelectedSeries)).FirstOrDefault();
                        db.Series.Remove(s);
                        db.SaveChanges();
                    }
                    UpdateSeries();
                    SelectedSeries = (Series.Count>0) ? Series.ElementAt(0) : null;
                }
            }
            

           
        }

        public void ResultFileCommandAction()
        {
            System.Console.WriteLine("Open Result File");
        }
        public void OutputCommandAction()
        {
            System.Console.WriteLine("Output PDF!");
        }

        public void NR2k3CommandAction()
        {
            System.Console.WriteLine("NR2k3");
        }

        private void UpdateSeries()
        {
            using (var db = new NR2k3ResultsEntities())
            {
                Series.Clear();
                foreach (string s in db.Series.Select(d => d.SeriesName))
                {
                    Series.Add(s);
                }
            }
        }

        

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}
    }
}