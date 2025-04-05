using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicPlayer2
{
    /// <summary>
    /// Interaction logic for Testes.xaml
    /// </summary>
    public partial class WindowTests : MetroWindow
    {
        public WindowTests()
        {
            InitializeComponent();
        }

    }

    public class Animal
    {
        public string Name { get; set; }
    }

    public class AnimalsViewModel
    {
        public AnimalsViewModel() 
        {
            RemoveAllCommand = new RelayCommand(ClearAll);

            Animals.Add(new Animal { Name = "Lion" });
            Animals.Add(new Animal { Name = "Cat" });
            Animals.Add(new Animal { Name = "Dog" });
            Animals.Add(new Animal { Name = "Hippo" });
            Animals.Add(new Animal { Name = "Giraffe" });
        }

        public ICommand RemoveAllCommand { get; }

        public ObservableCollection<Animal> Animals { get; } = new ObservableCollection<Animal>();

        private void ClearAll()
        {
            Animals.Clear();
        }
    }
}
