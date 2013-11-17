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

            var visualizations = xml.Element("Visualizations");
            foreach (var vis in visualizations.Elements("Visualization")) {
                NodeFunctions.KnownTypes.Add(vis.Attribute("Type").Value, TypeSettings.FromXml(vis));
            }

            var controls = xml.Element("XamlControls");
            if (controls != null) {
                foreach (var con in controls.Elements("XamlControl")) {
                    NodeFunctions.AddXamlControl(XamlControl.FromXml(con));
                }
            }

            this.VisualizationRoot.Children.Add(this.Root.ToUIElement());

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
            var xml = XElement.Load(this.DataSource);
            this.Root = Node.FromXml(xml.Element("Node"));
            this.tree.ItemsSource = null;
            this.tree.ItemsSource = new List<Node>() { this.Root };

            foreach (var c in NodeFunctions.XamlControls.Where(i => !i.Valid)) {
                NodeFunctions.Execute(c);
            }
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
            xml.Save(this.DataSource);
        }

        private void Save_Click_2(object sender, RoutedEventArgs e) {
            saveVisualizations();
        }

        private void Compile_Click_2(object sender, RoutedEventArgs e) {
            ///TODO keep track if the xaml control is valid//dirty and 
            ///recompile automatically on update (when necessary);
            var c = ((sender as Button).Tag as XamlControl);
            NodeFunctions.Execute(c);
        }

        private void XamlControl_TextChanged_1(object sender, TextChangedEventArgs e) {
            var c = (sender as TextBox).Tag as XamlControl;
            c.Valid = false;
        }
    }
}
