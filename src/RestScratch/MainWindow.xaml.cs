using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;

namespace RestScratch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RequestSettings RequestSettings { get; set; }
        bool isDirty;
        public bool IsDirty
        {
            get { return isDirty; }
            set
            {
                isDirty = value;
                SetTitle();
            }
        }
        string filename;
        public string Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                sbiFile.Content = Path.GetFileNameWithoutExtension(filename);
                SetTitle();
            }
        }

        private void SetTitle()
        {
            this.Title = "RestScratch" + (string.IsNullOrWhiteSpace(filename) ? "" : ": " + Path.GetFileNameWithoutExtension(filename));
            if (IsDirty)
                this.Title = string.Concat("*", this.Title);
        }
        
        public MainWindow()
        {
            InitializeComponent();


            Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            sbiVersion.Content = string.Format("Version: {0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
                OpenFile(args[1]);
            else
                miNew_Click(this, new RoutedEventArgs());
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsDirty) return;

            if (MessageBox.Show(this, "You have unsaved changes!\r\n Keep open?", "Closing RestScratch",MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                e.Cancel = true;

        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }
        private void SetTextboxBinding(TextBox tb, string propertyPath)
        {
            Binding b = new Binding();
            b.Source = RequestSettings;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Path = new PropertyPath(propertyPath);
            tb.SetBinding(TextBox.TextProperty, b);
        }
        private void SetComboboxBinding(ComboBox cb, string propertyPath)
        {
            Binding b = new Binding();
            b.Source = RequestSettings;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Path = new PropertyPath(propertyPath);
            cb.SetBinding(ComboBox.TextProperty, b);
        }

        private void BindRequestSettings()
        {
            SetComboboxBinding(cbMethod, "Method");
            SetComboboxBinding(cbContentType, "ContentType");
            SetTextboxBinding(tbAddress, "Address");
            SetTextboxBinding(tbFileName, "FileName");
            SetTextboxBinding(tbFilePath, "FilePath");

            lvFormData.ItemsSource = null;
            lvFormData.ItemsSource = RequestSettings.Form;
            lvHeaders.ItemsSource = null;
            lvHeaders.ItemsSource = RequestSettings.Headers;

            tbResults.Text = "";

            RequestSettings.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(RequestSettings_PropertyChanged);
            IsDirty = false;
        }

        void RequestSettings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            IsDirty = true;
        }
        private void bRun_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder result = new StringBuilder();
            try
            {
                WebRequest request = RequestSettings.MakeWebRequest();
                WebResponse response = request.GetResponse();

                WriteResponse(result, response);
            }
            catch (WebException ex)
            {
                

                result.AppendLine("Exception!");
                result.AppendLine("HTTP Status: " + ex.Status);

                if (ex.Response != null)
                    WriteResponse(result, ex.Response);
                else
                {
                    wbResults.NavigateToString("Empty");
                    tbiSource.IsSelected = true;
                }
                result.AppendLine("Exception Detail");

                result.AppendLine(ex.ToString());
            }
            finally
            {
                tbResults.Text = result.ToString();
            }
        }

        private void WriteResponse(StringBuilder result, WebResponse response)
        {
            result.AppendLine("Content Type: " + response.ContentType);
            result.AppendLine(string.Format("HTTP Status: {0} ({1})", ((HttpWebResponse)response).StatusDescription, (int)((HttpWebResponse)response).StatusCode));
            result.AppendLine("------------------------------");
            WriteResponse(result, response.GetResponseStream());
            SetForwardTab(response.ContentType);
        }

        private void SetForwardTab(string contentType)
        {
            if (contentType.ToLowerInvariant().Trim().StartsWith("text/html"))
                tbiHtml.IsSelected = true;
            else
                tbiSource.IsSelected = true;
        }

        private void WriteResponse(StringBuilder output, Stream stream)
        {
            string result = string.Empty;
            using (StreamReader sr = new StreamReader(stream))
                result = sr.ReadToEnd();

            output.AppendLine(result);
            if (string.IsNullOrWhiteSpace(result))
                wbResults.NavigateToString("Empty");
            else
                wbResults.NavigateToString(result);
        }

        private void lvFormData_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HandleDoubleClick(lvFormData, RequestSettings.Form);
        }

        private void HandleDoubleClick(ListView lv, IDictionary<string,string> dictionary)
        {
            if (lv.SelectedItem == null) return;

            KeyValuePair<string, string> item = (KeyValuePair<string, string>)lv.SelectedItem;
            EditItem modal = new EditItem(item.Key, item.Value);
            modal.Owner = this;
            if (!modal.ShowDialog() ?? false) return;

            SetValueFromModal(modal, dictionary);

            if (item.Key != modal.Key && dictionary.ContainsKey(item.Key))
                dictionary.Remove(item.Key);

            lv.ItemsSource = null;
            lv.ItemsSource = dictionary;
        }

        private void bAddFormData_Click(object sender, RoutedEventArgs e)
        {
            HandleAdd(lvFormData, RequestSettings.Form);
        }

        private void HandleAdd(ListView lv, IDictionary<string, string> dictionary)
        {
            EditItem modal = new EditItem();
            modal.Owner = this;
            if (!modal.ShowDialog() ?? false) return;

            SetValueFromModal(modal, dictionary);

            lv.ItemsSource = null;
            lv.ItemsSource = dictionary;
        }

        private void SetValueFromModal(EditItem modal, IDictionary<string, string> items)
        {
            IsDirty = true;
            if (items.ContainsKey(modal.Key))
                items[modal.Key] = modal.Value;
            else
                items.Add(modal.Key, modal.Value);
        }
        

        private void bRemoveFormData_Click(object sender, RoutedEventArgs e)
        {
            HandleRemove(lvFormData, RequestSettings.Form);
        }

        private void HandleRemove(ListView lv, IDictionary<string, string> dictionary)
        {
            if (lv.SelectedItem == null) return;

            KeyValuePair<string, string> item = (KeyValuePair<string, string>)lv.SelectedItem;

            if (dictionary.ContainsKey(item.Key))
                dictionary.Remove(item.Key);

            lv.ItemsSource = null;
            lv.ItemsSource = dictionary;
        }
        private void lvHeaders_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            HandleDoubleClick(lvHeaders, RequestSettings.Headers);
        }

        private void bAddHeader_Click(object sender, RoutedEventArgs e)
        {
            HandleAdd(lvHeaders, RequestSettings.Headers);
        }

        private void bRemoveHeader_Click(object sender, RoutedEventArgs e)
        {
            HandleRemove(lvHeaders, RequestSettings.Headers);
        }

        private void miOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.DefaultExt = ".rsr";
            diag.Filter = "Rest Scratch Requets|*.rsr";

            if (!diag.ShowDialog() ?? false) return;

            OpenFile(diag.FileName);
            
        }

        private void OpenFile(string filename)
        {
            Filename = filename;
            string text = File.ReadAllText(filename, Encoding.Default);
            byte[] data = Encoding.Default.GetBytes(text);

            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(RequestSettings));
            using (MemoryStream ms = new MemoryStream(data))
                RequestSettings = (RequestSettings)dcjs.ReadObject(ms);

            BindRequestSettings();
        }
        private void miSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Filename))
            {
                miSaveAs_Click(sender, e);
                return;
            }

            SaveRequest(Filename);
        }

        private void miSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog diag = new SaveFileDialog();
            diag.FileName = string.IsNullOrWhiteSpace(Filename) ? "RestScratchRequest" : Path.GetFileNameWithoutExtension(Filename);
            diag.DefaultExt = ".rsr";
            diag.Filter = "Rest Scratch Requets|*.rsr";

            if (diag.ShowDialog() ?? false)
                SaveRequest(diag.FileName);
        }

        private void SaveRequest(string filename)
        {
            DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(RequestSettings));
            Filename = filename;
            using (MemoryStream ms = new MemoryStream())
            {
                dcjs.WriteObject(ms, RequestSettings);
                File.WriteAllBytes(filename, ms.ToArray());
            }
            IsDirty = false;
        }

        private void miNew_Click(object sender, RoutedEventArgs e)
        {
            RequestSettings = new RequestSettings();
            Filename = null;
            BindRequestSettings();
        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Files (*.*)|*.*";
            dialog.Title = "Browse to file for upload";

            if( dialog.ShowDialog() ?? false)
                tbFilePath.Text = dialog.FileName;
        }

        private void bClearFile_Click(object sender, RoutedEventArgs e)
        {
            tbFilePath.Text = string.Empty;
        }
   }
}
