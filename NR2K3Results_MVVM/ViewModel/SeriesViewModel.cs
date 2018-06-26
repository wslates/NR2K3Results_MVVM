using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NR2K3Results_MVVM.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NR2K3Results_MVVM.ViewModel
{
    public class SeriesViewModel:ViewModelBase, ICleanup
    {
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

        public RelayCommand OpenRosterFileCommand { get; private set; }
        public RelayCommand LoadNewSeriesCommand { get; private set; }
        public RelayCommand LoadNewSancCommand { get; private set; }
        public RelayCommand<Window> SaveSeriesCommand { get; private set; }
        public RelayCommand<Window> CancelCommand { get; private set; }
        public RelayCommand OnCloseCommand { get; private set; }
        public RelayCommand NR2K3RootCommand { get; private set; }

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
                seriesLogo = value;
                RaisePropertyChanged();
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

        private void CancelCommandAction(Window obj)
        {
            obj.Close();
        }

        private void NR2K3RootCommandAction()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = "C:\\",
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !String.IsNullOrWhiteSpace(dialog.FileName))
            {
                NR2K3Dir = dialog.FileName;
                
            }
        }

        private void OnCloseCommandAction()
        {
            context.Dispose();
            Messenger.Default.Unregister(this);
        }

        private void ReceiveSeriesData(SendDataToSeriesView obj)
        {
            if (obj != null)
            {

                selectedSeries = context.Series.Where(d => d.SeriesName.Equals(obj.series)).FirstOrDefault();
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

        public void OpenRosterFileCommandAction()
        {
            Microsoft.Win32.OpenFileDialog openFile = new Microsoft.Win32.OpenFileDialog
            {
                Filter = ".lst Files (*.lst)|*.lst"
            };

            if (openFile.ShowDialog() == true)
            {
                RosterFull = openFile.FileName;
                string[] filePath = openFile.FileName.Split('\\');
                RosterFile = filePath[filePath.Length - 1];
            }
        }

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

        public void SaveSeriesCommandAction(Window window)
        {
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

                if (selectedSeries != null) { context.Series.Remove(selectedSeries); }
                context.Series.Add(series);
                context.SaveChanges();

                
                Messenger.Default.Send(new Model.AddDeleteOrModifySeriesMessage(Name));
                window?.Close();
            } 
            
        }


    }
}
