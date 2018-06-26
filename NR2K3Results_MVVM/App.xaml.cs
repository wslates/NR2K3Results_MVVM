using System.Windows;
using GalaSoft.MvvmLight.Threading;

namespace NR2K3Results_MVVM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            DispatcherHelper.Initialize();
        }
    }
}
