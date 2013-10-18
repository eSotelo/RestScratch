namespace RestScratch
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    /// <summary>
    /// Interaction logic for EditItem
    /// </summary>
    public partial class EditItem : INotifyPropertyChanged
    {
        /// <summary>
        /// The key
        /// </summary>
        private string key;

        /// <summary>
        /// The value
        /// </summary>
        private string value;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditItem"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public EditItem(string key, string value)
            : this()
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditItem"/> class.
        /// </summary>
        public EditItem()
        {
            this.InitializeComponent();

            var b = new Binding
            {
                Source = this,
                Path = new PropertyPath("Key"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            tbKey.SetBinding(TextBox.TextProperty, b);

            b = new Binding
            {
                Source = this,
                Path = new PropertyPath("Value"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            tbValue.SetBinding(TextBox.TextProperty, b);
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key
        {
            get
            {
                return this.key;
            }

            set
            {
                this.key = value;
                this.OnPropertyChanged("Key");
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
                this.OnPropertyChanged("Value");
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }

            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Handles the Click event of the bCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BCancelClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the bSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BSaveClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
