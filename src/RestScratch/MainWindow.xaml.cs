using System.Windows;
using System.Net;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Collections.Generic;
using System.Collections;

namespace RestScratch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public RequestSettings RequestSettings { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            RequestSettings = new RequestSettings();

            RequestSettings.ContentType = "application/x-www-form-urlencoded";
            RequestSettings.Method = "POST";
            RequestSettings.Address = "http://localhost:8080/OAuth/Token";
            RequestSettings.Form["client_id"] = "9e880bc88e799ed";
            RequestSettings.Form["client_secret"] = "dWe@XDN7KW";
            RequestSettings.Form["grant_type"] = "refresh_token";
            RequestSettings.Form["refresh_token"] = "29e3f69922ec32bd";
            RequestSettings.Headers["User-Agent"] = "RestScratch";
            RequestSettings.ContentBody = "test";

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

            lvFormData.ItemsSource = RequestSettings.Form;
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

   }
}
