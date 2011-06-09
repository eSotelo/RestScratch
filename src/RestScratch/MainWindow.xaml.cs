using System.Windows;
using System.Net;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Win32;
using System.Runtime.Serialization.Json;
using System;
using System.Reflection;
using System.Diagnostics;

namespace RestScratch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RequestSettings RequestSettings { get; set; }
        string filename;
        public string Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                sbiFile.Content = Path.GetFileNameWithoutExtension(filename);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            sbiVersion.Content = string.Format("Version: {0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);

            string[] args = Environment.GetCommandLineArgs();
            if (args != null && args.Length > 1)
                OpenFile(args[1]);
            else
                miNew_Click(this, new RoutedEventArgs());
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        private void BindRequestSettings()
        {
            Binding b = new Binding();
            b.Source = RequestSettings;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Path = new PropertyPath("Method");
            this.cbMethod.SetBinding(ComboBox.TextProperty, b);

            b = new Binding();
            b.Source = RequestSettings;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Path = new PropertyPath("Address");
            tbAddress.SetBinding(TextBox.TextProperty, b);

            b = new Binding();
            b.Source = RequestSettings;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Path = new PropertyPath("ContentBody");
            tbEntityBody.SetBinding(TextBox.TextProperty, b);

            b = new Binding();
            b.Source = RequestSettings;
            b.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            b.Path = new PropertyPath("ContentType");
            tbContentType.SetBinding(TextBox.TextProperty, b);

            lvFormData.ItemsSource = null;
            lvFormData.ItemsSource = RequestSettings.Form;
            lvHeaders.ItemsSource = null;
            lvHeaders.ItemsSource = RequestSettings.Headers;

        }
        private void bRun_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                WebRequest request = RequestSettings.MakeWebRequest();
                WebResponse response = request.GetResponse();
                StringBuilder result = new StringBuilder();
                WriteResponse(result, response.GetResponseStream());
                tbResults.Text = result.ToString();
            }
            catch (WebException ex)
            {
                StringBuilder result =new StringBuilder();
                result.AppendLine("Exception!");
                result.AppendLine("HTTP Status: " + ex.Status);
                result.AppendLine();
                if (ex.Response != null)
                    WriteResponse(result, ex.Response.GetResponseStream());

                result.AppendLine();
                result.AppendLine("Exception Detail");

                result.AppendLine(ex.ToString());

                tbResults.Text = result.ToString();
            }
        }

        private void WriteResponse(StringBuilder output, Stream stream)
        {
            string result = string.Empty;
            using (StreamReader sr = new StreamReader(stream))
                result = sr.ReadToEnd();

            output.AppendLine(result);
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

            if (!modal.ShowDialog() ?? false) return;

            SetValueFromModal(modal, dictionary);

            lv.ItemsSource = null;
            lv.ItemsSource = dictionary;
        }

        private void SetValueFromModal(EditItem modal, IDictionary<string, string> items)
        {
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
            diag.FileName = "RestScratchRequest";
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
        }

        private void miNew_Click(object sender, RoutedEventArgs e)
        {
            RequestSettings = new RequestSettings();
            Filename = null;
            BindRequestSettings();
        }
   }
}
