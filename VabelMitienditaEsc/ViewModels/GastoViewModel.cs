using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VabelMitienditaEsc.Core;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class GastoViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly MainViewModel _mainViewModel;

        public GastoViewModel(NavigationStore navigationStore, MainViewModel mainViewModel)
        {
            _navigationStore = navigationStore;
            _mainViewModel = mainViewModel;
        }
    }
}

