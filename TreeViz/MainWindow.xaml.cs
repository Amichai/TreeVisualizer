using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using TreeLib;

namespace TreeViz {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {
        public MainWindow() {
            InitializeComponent();
            this.DataContext = this;
            var xml = XElement.Load(@"..\..\DataSet.xml");
            this.Root = Node.FromXml(xml.Element("Node"));
            this.tree.ItemsSource = new List<Node>() { this.Root };
            this.VisualizationRoot.Children.Add(this.Root.ToUIElement());
        }

        public List<TypeSettings> KnownTypes {
            get {
                return NodeFunctions.KnownTypes.Values.ToList();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }

        private Node _Root;
        public Node Root {
            get { return _Root; }
            set {
                if (_Root != value) {
                    _Root = value;
                    OnPropertyChanged("Root");
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            this.tree.ItemsSource = null;
            this.tree.ItemsSource = new List<Node>() { this.Root }; ;
            this.VisualizationRoot.Children.Clear();
            this.VisualizationRoot.Children.Add(this.Root.ToUIElement());
        }
    }
}
