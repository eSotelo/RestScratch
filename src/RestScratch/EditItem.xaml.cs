using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RestScratch
{
    /// <summary>
    /// Interaction logic for EditItem.xaml
    /// </summary>
    public partial class EditItem : Window, INotifyPropertyChanged
    {
        string key;
        public string Key
        {
            get { return key; }
            set
            {
                key = value;
                OnPropertyChanged("Key");
            }
        }
        string value;
        public string Value
        {
            get { return value; }
            set
            {
                this.value = value;
                OnPropertyChanged("Value");
            }
        }

        public EditItem(string key, string value) : this()
        {
            Key = key;
            Value = value;
        }
        public EditItem()
        {
            InitializeComponent();

            Binding b = new Binding();
            b.Source = this;
            b.Path = new PropertyPath("Key");
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            tbKey.SetBinding(TextBox.TextProperty, b);

            b = new Binding();
            b.Source = this;
            b.Path = new PropertyPath("Value");
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            tbValue.SetBinding(TextBox.TextProperty, b);
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        #region INotifyPropertyChanged Members

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged == null) return;

            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
