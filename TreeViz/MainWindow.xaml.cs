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
            var xml = XElement.Load(this.DataSource);
            this.Root = Node.FromXml(xml.Element("Node"));
            this.tree.ItemsSource = new List<Node>() { this.Root };

            NodeFunctions.LoadLibraries(xml);
            this.VisualizationRoot.Children.Add(this.Root.ToUIElement());

        }

        public List<Function> Functions {
            get {
                return NodeFunctions.Functions;
            }
        }

        public List<XamlControl> XamlControls {
            get {
                return NodeFunctions.XamlControls;
            }
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

        private void Refresh_Click_1(object sender, RoutedEventArgs e) {
            ///TODO: check if the datasource has changed since we last loaded one...
            var xml = XElement.Load(this.DataSource);
            this.Root = Node.FromXml(xml.Element("Node"));
            this.tree.ItemsSource = null;
            this.tree.ItemsSource = new List<Node>() { this.Root };
            NodeFunctions.RefreshLibraries();

            this.VisualizationRoot.Children.Clear();
            this.VisualizationRoot.Children.Add(this.Root.ToUIElement());
        }

        private string _DataSource = @"..\..\DataSet2.xml";
        public string DataSource {
            get { return _DataSource; }
            set {
                if (_DataSource != value) {
                    _DataSource = value;
                    OnPropertyChanged("DataSource");
                }
            }
        }

        private void Window_Closing_1(object sender, CancelEventArgs e) {
        }

        private void saveVisualizations() {
            var xml = new XElement("Data");
            xml.Add(this.Root.ToXml());
            var visualizations = new XElement("Visualizations");
            foreach (var vis in NodeFunctions.KnownTypes.Values) {
                visualizations.Add(vis.ToXml());
            }
            xml.Add(visualizations);
            var controls = new XElement("XamlControls");
            foreach (var con in NodeFunctions.XamlControls) {
                controls.Add(con.ToXml());
            }
            xml.Add(controls);

            var functions = new XElement("Functions");
            foreach (var f in NodeFunctions.Functions) {
                functions.Add(f.ToXml());
            }
            xml.Add(functions);
            xml.Save(this.DataSource);
        }

        private void Save_Click_2(object sender, RoutedEventArgs e) {
            saveVisualizations();
        }

        private void XamlControl_TextChanged_1(object sender, TextChangedEventArgs e) {
            var c = (sender as TextBox).Tag as XamlControl;
            c.Valid = false;
        }

        private void Function_TextChanged_1(object sender, TextChangedEventArgs e) {
            var c = (sender as TextBox).Tag as Function;
            c.Valid = false;
        }
    }
}
