using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VabelMitienditaEsc.Core;
using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VabelMitienditaEsc.Models;
using VabelMitienditaEsc.Services;

namespace VabelMitienditaEsc.ViewModels
{
    public partial class ProveedorViewModel : ViewModelBase
    {
        public ProveedorViewModel(NavigationStore navigationStore, MainViewModel mainViewModel, ProveedorService proveedorService) 
        { 

        }
    }
}
