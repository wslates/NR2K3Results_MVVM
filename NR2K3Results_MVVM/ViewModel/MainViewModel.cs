using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using NR2K3Results_MVVM.Model;
using NR2K3Results_MVVM.Parsers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Ookii.Dialogs;
using System.IO;
using System.Data.SqlClient;
using System.Data.Entity.Core;

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
        private String selectedSession;
        private String resultFilePath;
        private String raceName;
        private readonly IDataService _dataService;
        private Race track;
        private Series series;
        private string resultFile;
        private List<Driver> drivers;

        public String SelectedSeries
        {
            get
            {
                return selectedSeries;
            }
            set
            {
                using (var db = new NR2K3ResultsEntities())
                {
                    series = db.Series.Where(d => d.SeriesName.Equals(value)).FirstOrDefault();
                }
                Set(ref selectedSeries, value);
               
            }
        }

        public String SelectedSession
        {
            get
            {
                return selectedSession;
            } set
            {
                Set(ref selectedSession, value);
            }
        }

        public String ResultFile
        {
            get
            {
                return resultFile;
            }
            set
            {
                Set(ref resultFile, value);
            }
        }

        public String RaceName
        {
            get
            {
                return raceName;
            }
            set
            {
                Set(ref raceName, value);
            }
        }
        public RelayCommand NewSeriesCommand { get; private set; }
        public RelayCommand EditSeriesCommand { get; private set; }
        public RelayCommand DeleteSeriesCommand { get; private set; }
        public RelayCommand OutputCommand { get; private set; }
        public RelayCommand ResultFileCommand { get; private set; }
        public ObservableCollection<String> Series { get; private set; }
        public ObservableCollection<String> Sessions { get; private set; }
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
            Messenger.Default.Register<Model.AddDeleteOrModifySeriesMessage>(this, UpdateSeries);
            Series = new ObservableCollection<String>();
            Sessions = new ObservableCollection<String>();
            try
            {
                UpdateSeries();
            }
            catch (EntityCommandExecutionException e)
            {
                MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            catch (SqlException e)
            {
                MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();

            }


        }

        private void UpdateSeries(AddDeleteOrModifySeriesMessage obj)
        {
            try
            {
                UpdateSeries();
            }
            catch (EntityCommandExecutionException e)
            {
                MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SqlException e)
            {
                MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            }


            if (Series.Contains(obj.newSeries))
            {
               
                SelectedSeries = obj.newSeries;
            }
            
        }

        public void NewSeriesCommandAction()
        {
            View.SeriesWindow window = new View.SeriesWindow();
            window.ShowDialog();
            window = null;
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
                    try
                    {
                        using (var db = new NR2K3ResultsEntities())
                        {
                            Series s = db.Series.Where(d => d.SeriesName.Equals(SelectedSeries)).FirstOrDefault();
                            db.Series.Remove(s);
                            db.SaveChanges();
                        }
                        UpdateSeries();
                    }
                    catch (EntityCommandExecutionException e)
                    {
                        MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (SqlException e)
                    {
                        MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                    }

                    
                    SelectedSeries = (Series.Count>0) ? Series.ElementAt(0) : null;
                }
            }
            

           
        }

        public void ResultFileCommandAction()
        {
            if (selectedSeries!=null)
            {
                
                
                Microsoft.Win32.OpenFileDialog resultFile = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "HTML Files (*.html)|*.html",
                    InitialDirectory = (System.IO.Directory.Exists((series.NR2K3Dir +"\\exports_imports"))) ? series.NR2K3Dir + "\\exports_imports" : "C:\\"
                };

                if (resultFile.ShowDialog() == true)
                {
                    track = TrackParser.Parse(series.NR2K3Dir, resultFile.FileName);
                    resultFilePath = resultFile.FileName;
                    ResultFile = resultFile.FileName.Split('\\').Last();
                    Sessions.Clear();
                    ResultParser.GetSessions(resultFile.FileName, Sessions);
                    SelectedSession = (Sessions.Count > 0) ? Sessions[0] : null;
                }
            }
            
        }
        public void OutputCommandAction()
        {
            if (SelectedSeries != null && resultFilePath != null)
            {
                try
                {
                    drivers = CarFileParser.GetRosterDrivers(series.NR2K3Dir, series.RosterFile);
                    ResultParser.Parse(ref drivers, resultFilePath, SelectedSession, ref track);                  
                    drivers.Sort();
                } catch (FileNotFoundException e)
                {
                    MessageBox.Show("Error saving file. " + e.Message, "Error Saving file!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                

                Microsoft.Win32.SaveFileDialog dialog = new Microsoft.Win32.SaveFileDialog()
                {
                    Filter = "PDF Files (*.pdf)|*.pdf"
                };

                try
                {
                    if (dialog.ShowDialog() == true)
                    {
                        if (selectedSession.Equals("Race"))
                        {
                            PDFGeneration.RacePDFGenerator.OutputPDF(drivers, series, RaceName, track, dialog.FileName);
                        }
                        else
                        {
                            PDFGeneration.PracticePDFGenerators.OutputPDF(drivers, series, selectedSession, RaceName, track, dialog.FileName);
                        }
                    }
                    else { }
                } catch (IOException e)
                {
                    MessageBox.Show("Error saving file. Check if the file is opened in another window.", "Error Saving file!", MessageBoxButton.OK, MessageBoxImage.Error);
                } catch (System.Net.WebException e)
                {
                    MessageBox.Show("Error saving file. " + e.Message, "Error Saving file!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }
        }


        private void UpdateSeries()
        {
         
                using (var db = new NR2K3ResultsEntities())
                {
                    Series.Clear();
                    foreach (string s in db.Series.Select(d => d.SeriesName))
                    {
                        Series.Add(s);
                    }
                }
            

        }

    }
}