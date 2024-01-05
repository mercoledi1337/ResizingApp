using Microsoft.Win32;
using System.Windows;
using test127.Core;

namespace test127.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public RelayCommand HomeViewCommand { get; set; }
        public RelayCommand DiscoveryViewCommand { get; set; }


        public HomeViewModel HomeVm { get; set; }

        public DiscoveryViewModel DiscoveryVm { get; set; }

        private object _currentView;

        public object CurrentView
        {
            get { return _currentView; }
            set 
            { 
                _currentView = value;
                OnPropertyChanged();
            }
        }

        

        public MainViewModel()
        {

            

            HomeVm = new HomeViewModel();
            DiscoveryVm = new DiscoveryViewModel();

            CurrentView = HomeVm;

            HomeViewCommand = new RelayCommand(p =>
            {
                CurrentView = HomeVm;
            });

            DiscoveryViewCommand = new RelayCommand(p =>
            {
                CurrentView = DiscoveryVm;
            });
        }

        
    }
}
