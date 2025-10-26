using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Model
{

    public class UpDateTableViewModel : ObservableObject
    {
 
        private string _node;
        private string _hardwareversion;
        private string _softwareVersion;
        


        public string Node
        {
            get => _node;
            set => SetProperty(ref _node, value);
        }
        public string Hardwareversion
        {
            get => _hardwareversion;
            set => SetProperty(ref _hardwareversion, value);
        }



        public string SoftwareVersion
        {
            get => _softwareVersion;
            set => SetProperty(ref _softwareVersion, value);
        }


    }
}
