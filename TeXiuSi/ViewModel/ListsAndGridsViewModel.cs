using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using TeXiuSi.Model;

namespace TeXiuSi.ViewModel
{

    public class ListsAndGridsViewModel : ObservableObject
    {
        public ListsAndGridsViewModel()
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




        private static ObservableCollection<SelectableViewModel> CreateData()
        {
            return new ObservableCollection<SelectableViewModel>
        {
            new SelectableViewModel
            {
                Name = "C",
                Description = "Material Design in XAML Toolkit",
                Path = "D://"
            },
            new SelectableViewModel
            {
                Name = "D",
                Description = "Dragablz Tab Control",
                Path = "D://"
            },
            new SelectableViewModel
            {
                Name = "E",
                Description = "C://",
                Path = "E://"
            }
        };
        }

        public ObservableCollection<SelectableViewModel> Items1 { get; }
        public ObservableCollection<SelectableViewModel> Items2 { get; }
        public ObservableCollection<SelectableViewModel> Items3 { get; }
        public ObservableCollection<SelectableViewModel> Items4 { get; }

        public IEnumerable<string> Foods => new[] { "Burger", "Fries", "Shake", "Lettuce" };

        public IList<string> Files { get; }

        public IEnumerable<DataGridSelectionUnit> SelectionUnits => new[] { DataGridSelectionUnit.FullRow, DataGridSelectionUnit.Cell, DataGridSelectionUnit.CellOrRowHeader };
    }
}