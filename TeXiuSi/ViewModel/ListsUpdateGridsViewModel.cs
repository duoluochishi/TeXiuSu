using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{
    public class ListsUpdateGridsViewModel : ObservableObject
    {
        public ListsUpdateGridsViewModel()
        {
            Items1 = CreateData();
            Items2 = CreateData();
            Items3 = CreateData();
            Items4 = CreateData();


            Files = new List<string>();

            for (int i = 0; i < 1000; i++)
            {
                Files.Add(Path.GetRandomFileName());
            }
        }




        private static ObservableCollection<UpDateTableViewModel> CreateData()
        {
            return new ObservableCollection<UpDateTableViewModel>
        {
            new UpDateTableViewModel
            {
                Node = "Node1",
                Hardwareversion = "Hard-01",
                SoftwareVersion = "V1.0"
            },
            new UpDateTableViewModel
            {
                Node = "Node2",
                Hardwareversion = "Hard-02",
                SoftwareVersion = "V2.0"
            },

        };
        }

        public ObservableCollection<UpDateTableViewModel> Items1 { get; }
        public ObservableCollection<UpDateTableViewModel> Items2 { get; }
        public ObservableCollection<UpDateTableViewModel> Items3 { get; }
        public ObservableCollection<UpDateTableViewModel> Items4 { get; }

        public IEnumerable<string> Foods => new[] { "Burger", "Fries", "Shake", "Lettuce" };

        public IList<string> Files { get; }

        public IEnumerable<DataGridSelectionUnit> SelectionUnits => new[] { DataGridSelectionUnit.FullRow, DataGridSelectionUnit.Cell, DataGridSelectionUnit.CellOrRowHeader };
    }
}
