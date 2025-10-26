using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeXiuSi.Model
{
    public class SelectableViewModel : ObservableObject
    {
        private string _path;
        private string _name;
        private string _description;


        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }



        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }


    }

}
