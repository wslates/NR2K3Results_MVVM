using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.WindowsAPICodePack.Dialogs;
using NR2K3Results_MVVM.Model;
using System;
using System.Linq;
using System.Windows;
using Ookii.Dialogs;
using System.Data.SqlClient;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;

namespace NR2K3Results_MVVM.ViewModel
{
    public class SeriesViewModel:ViewModelBase
    {
        #region PRIVATE_PROPERTIES
        private Series selectedSeries;
        private String RosterFull;
        private String SeriesFull;
        private String SancFull;
        private NR2K3ResultsEntities context = new NR2K3ResultsEntities();

        private String name;
        private String shortName;
        private String rosterFile;
        private String seriesLogo;
        private String sancLogo;
        private String NR2k3Dir;
        #endregion

        #region COMMANDS
        public RelayCommand OpenRosterFileCommand { get; private set; }
        public RelayCommand LoadNewSeriesCommand { get; private set; }
        public RelayCommand LoadNewSancCommand { get; private set; }
        public RelayCommand<Window> SaveSeriesCommand { get; private set; }
        public RelayCommand<Window> CancelCommand { get; private set; }
        public RelayCommand OnCloseCommand { get; private set; }
        public RelayCommand NR2K3RootCommand { get; private set; }
        #endregion

        #region PUBLIC_PROPERTIES
        public String Name
        {
            get
            {
                return name;
            }
            set
            { 
                if (value.Length>64)
                {
                    shortName = value.Substring(0, 64);
                } else
                    name = value;
                RaisePropertyChanged();
            }
        }

        public String ShortName
        {
            get
            {
                return shortName;
            }
            set
            {
                if (value.Length>16)
                {
                    shortName = value.Substring(0, 16);
                } else
                    shortName = value;
                RaisePropertyChanged();
            }
        }

        public String RosterFile
        {
            get
            {
                return rosterFile;
            }
            set
            {
                rosterFile = value;
                RaisePropertyChanged();
            }
        }

        public String SeriesLogo
        {
            get
            {
                return seriesLogo;
            }
            set
            {
                Set(ref seriesLogo, value);
            }
        }

        public String SancLogo
        {
            get
            {
                return sancLogo;
            }
            set
            {
                sancLogo = value;
                RaisePropertyChanged();
            }
        }

        public String NR2K3Dir
        {
            get
            {
                return NR2k3Dir;
            }
            set
            {
                NR2k3Dir = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public SeriesViewModel()
        {
            OpenRosterFileCommand = new RelayCommand(OpenRosterFileCommandAction);
            LoadNewSeriesCommand = new RelayCommand(LoadNewSeriesCommandAction);
            LoadNewSancCommand = new RelayCommand(LoadNewSancCommandAction);
            SaveSeriesCommand = new RelayCommand<Window>(SaveSeriesCommandAction);
            OnCloseCommand = new RelayCommand(OnCloseCommandAction);
            NR2K3RootCommand = new RelayCommand(NR2K3RootCommandAction);
            CancelCommand = new RelayCommand<Window>(CancelCommandAction);
            Messenger.Default.Register<Model.SendDataToSeriesView>(this, ReceiveSeriesData);

        }
        
        /// <summary>
        /// Method to execute when the user cancels editing or creating a series.
        /// </summary>
        /// <param name="obj">Reference to this window.</param>
        private void CancelCommandAction(Window obj)
        {
            obj.Close();
        }

        /// <summary>
        /// Method to execute when the user opens their NR2K3 root folder.
        /// </summary>
        private void NR2K3RootCommandAction()
        {
            Ookii.Dialogs.Wpf.VistaFolderBrowserDialog diag = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (diag.ShowDialog() == true)
            {
                NR2K3Dir = diag.SelectedPath;               
            }
        }

        /// <summary>
        /// Method that executes when the window is closed.
        /// </summary>
        private void OnCloseCommandAction()
        {
            try
            {
                //dispose of the context object
                context.Dispose();
            }
            catch (EntityCommandExecutionException e)
            {
                MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (SqlException e)
            {
                MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            }

            //unregister messaging
            Messenger.Default.Unregister(this);
        }

        /// <summary>
        /// Method to execute when view model receives message from main view model with all data.
        /// </summary>
        /// <param name="obj">Data from message.</param>
        private void ReceiveSeriesData(SendDataToSeriesView obj)
        {
            if (obj != null)
            {
                try
                {
                    selectedSeries = context.Series.Where(d => d.SeriesName.Equals(obj.series)).FirstOrDefault();
                }
                catch (EntityCommandExecutionException e)
                {
                    MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);

                }
                NR2K3Dir = selectedSeries?.NR2K3Dir;
                Name = selectedSeries?.SeriesName;
                ShortName = selectedSeries?.SeriesShort;
                RosterFull = selectedSeries?.RosterFile;
                RosterFile = selectedSeries?.RosterFile.Split('\\').Last();
                SeriesFull = selectedSeries?.SeriesLogo;
                SancFull = selectedSeries?.SancLogo;
                SeriesLogo = selectedSeries?.SeriesLogo?.Split('\\').Last();
                SancLogo = selectedSeries?.SancLogo?.Split('\\').Last();

            }

        }

        /// <summary>
        /// Method to execute when user opens the roster file.
        /// </summary>
        public void OpenRosterFileCommandAction()
        {
            if (NR2k3Dir !=null)
            {
                Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = ".lst Files (*.lst)|*.lst",
                    InitialDirectory = NR2k3Dir,
                    
                };

                if (openFile.ShowDialog() == true)
                {
                    RosterFull = openFile.FileName;
                    string[] filePath = openFile.FileName.Split('\\');
                    RosterFile = filePath[filePath.Length - 1];
                }
            } else
            {
                MessageBox.Show("Please choose your NR2K3 root.", "No NR2k3 Root!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }

        /// <summary>
        /// Method to execute when user opens a new series image.
        /// </summary>
        public void LoadNewSeriesCommandAction()
        {
            String path = LoadImage();

            if (null != path)
            {
                SeriesFull = path;
                string[] filePath = path.Split('\\');
                SeriesLogo = filePath[filePath.Length - 1];
            } 
        }

        /// <summary>
        /// Action to execute when user opens a new sanctioning body image.
        /// </summary>
        public void LoadNewSancCommandAction()
        {
            String path = LoadImage();

            if (null != path)
            {
                SancFull = path;
                string[] filePath = path.Split('\\');
                SancLogo = filePath[filePath.Length - 1];
            }
        }

        /// <summary>
        /// Prompts user with open file dialog to open an image.
        /// </summary>
        /// <returns>Returns the path to the image the user opened.</returns>
        private String LoadImage()
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog();

            var codecs = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders();
            var codecFilter = "Image Files|";
            foreach (var codec in codecs)
            {
                codecFilter += codec.FilenameExtension + ";";
            }
            openFile.Filter = codecFilter;

            if (openFile.ShowDialog() == true)
            {
                return openFile.FileName;
            } else
            {
                return null;
            }

            
        }

        /// <summary>
        /// Action to execute when the user saves the series.
        /// </summary>
        /// <param name="window">Reference to this window.</param>
        public void SaveSeriesCommandAction(Window window)
        {
            //make sure all data is filled
            if (String.IsNullOrEmpty(Name) || String.IsNullOrEmpty(ShortName) || String.IsNullOrEmpty(RosterFull) || String.IsNullOrEmpty(NR2k3Dir))
            {
                MessageBox.Show("Please enter all required data!", "Error Saving Series!", MessageBoxButton.OK, MessageBoxImage.Error);
            } else
            {
                Series series = new Series
                {
                    SeriesName = Name,
                    SeriesShort = ShortName,
                    RosterFile = RosterFull,
                    SeriesLogo = SeriesFull,
                    SancLogo = SancFull,
                    NR2K3Dir = NR2k3Dir
                };
                try
                {
                    if (selectedSeries != null) { context.Series.Remove(selectedSeries); }
                    context.Series.Add(series);
                    context.SaveChanges();

                }
                catch (DbUpdateException e)
                {
                    MessageBox.Show("Error with database. Check if database file exists or is opened in another program.", "Database Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                
                //send the data back to the main view model.
                Messenger.Default.Send(new Model.AddDeleteOrModifySeriesMessage(Name));
                window?.Close();
            } 
            
        }


    }
}
