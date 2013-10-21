namespace RestScratch
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization.Json;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Threading;

    using Microsoft.Win32;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// The is dirty
        /// </summary>
        private bool isDirty;
        
        /// <summary>
        /// The openFile
        /// </summary>
        private string filename;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();

            this.Closing += this.MainWindowClosing;
            var v = Assembly.GetExecutingAssembly().GetName().Version;
            sbiVersion.Content = string.Format("Version: {0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);

            var args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                this.OpenFile(args[1]);
            }
            else
            {
                this.MiNewClick(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [is dirty].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is dirty]; otherwise, <c>false</c>.
        /// </value>
        public bool IsDirty
        {
            get
            {
                return this.isDirty;
            }

            set
            {
                this.isDirty = value;
                this.SetTitle();
            }
        }

        /// <summary>
        /// Gets or sets the openFile.
        /// </summary>
        /// <value>
        /// The openFile.
        /// </value>
        public string Filename
        {
            get
            {
                return this.filename;
            }

            set
            {
                this.filename = value;
                this.sbiFile.Content = Path.GetFileNameWithoutExtension(this.filename);
                this.SetTitle();
            }
        }

        /// <summary>
        /// Gets or sets the request settings.
        /// </summary>
        /// <value>
        /// The request settings.
        /// </value>
        public RequestSettings RequestSettings { get; set; }

        /// <summary>
        /// Sets the title.
        /// </summary>
        private void SetTitle()
        {
            this.Title = "RestScratch" + (string.IsNullOrWhiteSpace(this.filename) ? string.Empty : ": " + Path.GetFileNameWithoutExtension(this.filename));
            if (this.IsDirty)
            {
                this.Title = string.Concat("*", this.Title);
            }
        }

        /// <summary>
        /// Handles the Closing event of the MainWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance containing the event data.</param>
        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!this.IsDirty)
            {
                return;
            }

            if (MessageBox.Show(
                this,
                "You have unsaved changes!\r\n Keep open?",
                "Closing RestScratch",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Sets the textbox binding.
        /// </summary>
        /// <param name="tb">The tb.</param>
        /// <param name="propertyPath">The property path.</param>
        private void SetTextboxBinding(FrameworkElement tb, string propertyPath)
        {
            var b = new Binding
                        {
                            Source = this.RequestSettings,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Path = new PropertyPath(propertyPath)
                        };

            tb.SetBinding(TextBox.TextProperty, b);
        }

        /// <summary>
        /// Sets the combo box binding.
        /// </summary>
        /// <param name="cb">The combo box.</param>
        /// <param name="propertyPath">The property path.</param>
        private void SetComboboxBinding(FrameworkElement cb, string propertyPath)
        {
            var b = new Binding
                        {
                            Source = this.RequestSettings,
                            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                            Path = new PropertyPath(propertyPath)
                        };

            cb.SetBinding(ComboBox.TextProperty, b);
        }

        /// <summary>
        /// Binds the request settings.
        /// </summary>
        private void BindRequestSettings()
        {
            this.SetComboboxBinding(cbMethod, "Method");
            this.SetComboboxBinding(cbContentType, "ContentType");
            this.SetTextboxBinding(tbAddress, "Address");
            this.SetTextboxBinding(tbFileName, "FileName");
            this.SetTextboxBinding(tbFilePath, "FilePath");
            this.SetTextboxBinding(tbEntityBody, "ContentBody");
            lvFormData.ItemsSource = null;
            lvFormData.ItemsSource = RequestSettings.Form;
            lvHeaders.ItemsSource = null;
            lvHeaders.ItemsSource = RequestSettings.Headers;

            tbResults.Text = string.Empty;

            RequestSettings.PropertyChanged += this.RequestSettingsPropertyChanged;

            this.IsDirty = false;
        }

        /// <summary>
        /// Handles the PropertyChanged event of the RequestSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void RequestSettingsPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.IsDirty = true;
        }

        private void RunOnMainThread(Action action)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, action);
        }
        /// <summary>
        /// Handles the Click event of the bRun control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BRunClick(object sender, RoutedEventArgs e)
        {
            tbResults.Text = "Running...";
            bRun.IsEnabled = false;
            Task.Factory.StartNew(
                () =>
                    {
                        var result = new StringBuilder();
                        try
                        {
                            var request = RequestSettings.MakeWebRequest();
                            this.RunOnMainThread(() => tbResults.Text += "Executing Request...");
                            
                            var response = request.GetResponse();
                            this.RunOnMainThread(() => tbResults.Text += "Got Response...");

                           this.RunOnMainThread(() => this.WriteResponse(result, response.ContentType, response));
                        }
                        catch (WebException ex)
                        {
                            result.AppendLine("Exception!");
                            result.AppendLine("HTTP Status: " + ex.Status);

                            if (ex.Response != null)
                            {
                                this.RunOnMainThread(() => this.WriteResponse(result, "text/html", ex.Response));
                            }
                            else
                            {
                                this.RunOnMainThread(
                                    () =>
                                    {
                                        wbResults.NavigateToString("Empty");
                                        tbiSource.IsSelected = true;
                                    });
                            }

                            result.AppendLine("Exception Detail");

                            result.AppendLine(ex.ToString());
                        }
                        finally
                        {
                            this.RunOnMainThread(() =>
                                { 
                                    tbResults.Text = result.ToString();
                                    bRun.IsEnabled = true;
                                });
                        }
                    });
        }

        /// <summary>
        /// Writes the response.
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="response">The response.</param>
        private void WriteResponse(StringBuilder result, string contentType, WebResponse response)
        {
            result.AppendLine("Content Type: " + response.ContentType);
            result.AppendLine(string.Format("HTTP Status: {0} ({1})", ((HttpWebResponse)response).StatusDescription, (int)((HttpWebResponse)response).StatusCode));
            result.AppendLine("------------------------------");
            if (contentType.ToLowerInvariant().StartsWith("text") || contentType.ToLowerInvariant().StartsWith("application/json"))
            {
                this.WriteResponse(result, response.GetResponseStream());
                this.SetForwardTab(response.ContentType);
            }
            else
            {
                result.AppendLine("non-textural response");
                if (contentType.StartsWith("image"))
                {
                    var path = Path.Combine(
                        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "C:\\",
                        "temp.png");
                    using (var stream = response.GetResponseStream())
                    {
                        var buffer = new byte[1024];
                        using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            int byteCount;
                            do
                            {
                                if (stream == null)
                                {
                                    break;
                                }

                                byteCount = stream.Read(buffer, 0, buffer.Length);
                                fs.Write(buffer, 0, byteCount);
                            }
                            while (byteCount > 0);
                        }

                        wbResults.Source = new Uri(path);
                    }

                    tbiHtml.IsSelected = true;
                }
                else
                {
                    var stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        wbResults.NavigateToStream(stream);
                    }

                    this.SetForwardTab(contentType);
                }
            }
        }

        /// <summary>
        /// Sets the forward tab.
        /// </summary>
        /// <param name="contentType">Type of the content.</param>
        private void SetForwardTab(string contentType)
        {
            if (contentType.ToLowerInvariant().Trim().StartsWith("text/html"))
            {
                tbiHtml.IsSelected = true;
            }
            else
            {
                tbiSource.IsSelected = true;
            }
        }

        /// <summary>
        /// Writes the response.
        /// </summary>
        /// <param name="output">The output.</param>
        /// <param name="stream">The stream.</param>
        private void WriteResponse(StringBuilder output, Stream stream)
        {
            string result;
            using (var sr = new StreamReader(stream))
            {
                result = sr.ReadToEnd();
            }

            output.AppendLine(result);

            this.wbResults.NavigateToString(string.IsNullOrWhiteSpace(result) ? "Empty" : result);
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the lvFormData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void LvFormDataMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HandleDoubleClick(lvFormData, RequestSettings.Form);
        }

        /// <summary>
        /// Handles the double click.
        /// </summary>
        /// <param name="lv">The lv.</param>
        /// <param name="dictionary">The dictionary.</param>
        private void HandleDoubleClick(Selector lv, IDictionary<string, string> dictionary)
        {
            if (lv.SelectedItem == null)
            {
                return;
            }

            var item = (KeyValuePair<string, string>)lv.SelectedItem;
            var modal = new EditItem(item.Key, item.Value) { Owner = this };

            if ((bool)(!modal.ShowDialog()))
            {
                return;
            }

            this.SetValueFromModal(modal, dictionary);

            if (item.Key != modal.Key && dictionary.ContainsKey(item.Key))
            {
                dictionary.Remove(item.Key);
            }

            lv.ItemsSource = null;
            lv.ItemsSource = dictionary;
        }

        /// <summary>
        /// Handles the Click event of the bAddFormData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BAddFormDataClick(object sender, RoutedEventArgs e)
        {
            this.HandleAdd(lvFormData, RequestSettings.Form);
        }

        /// <summary>
        /// Handles the add.
        /// </summary>
        /// <param name="lv">The lv.</param>
        /// <param name="dictionary">The dictionary.</param>
        private void HandleAdd(ItemsControl lv, IDictionary<string, string> dictionary)
        {
            var modal = new EditItem { Owner = this };

            if ((bool)(!modal.ShowDialog()))
            {
                return;
            }

            this.SetValueFromModal(modal, dictionary);

            lv.ItemsSource = null;
            lv.ItemsSource = dictionary;
        }

        /// <summary>
        /// Sets the value from modal.
        /// </summary>
        /// <param name="modal">The modal.</param>
        /// <param name="items">The items.</param>
        private void SetValueFromModal(EditItem modal, IDictionary<string, string> items)
        {
            this.IsDirty = true;
            if (items.ContainsKey(modal.Key))
            {
                items[modal.Key] = modal.Value;
            }
            else
            {
                items.Add(modal.Key, modal.Value);
            }
        }

        /// <summary>
        /// Handles the Click event of the bRemoveFormData control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BRemoveFormDataClick(object sender, RoutedEventArgs e)
        {
            this.HandleRemove(lvFormData, RequestSettings.Form);
        }

        /// <summary>
        /// Handles the remove.
        /// </summary>
        /// <param name="lv">The lv.</param>
        /// <param name="dictionary">The dictionary.</param>
        private void HandleRemove(Selector lv, IDictionary<string, string> dictionary)
        {
            if (lv.SelectedItem == null)
            {
                return;
            }

            var item = (KeyValuePair<string, string>)lv.SelectedItem;

            if (dictionary.ContainsKey(item.Key))
            {
                dictionary.Remove(item.Key);
            }

            lv.ItemsSource = null;
            lv.ItemsSource = dictionary;
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the lvHeaders control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void LvHeadersMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.HandleDoubleClick(lvHeaders, RequestSettings.Headers);
        }

        /// <summary>
        /// Handles the Click event of the bAddHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BAddHeaderClick(object sender, RoutedEventArgs e)
        {
            this.HandleAdd(lvHeaders, RequestSettings.Headers);
        }

        /// <summary>
        /// Handles the Click event of the bRemoveHeader control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BRemoveHeaderClick(object sender, RoutedEventArgs e)
        {
            this.HandleRemove(lvHeaders, RequestSettings.Headers);
        }

        /// <summary>
        /// Handles the Click event of the miOpen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MiOpenClick(object sender, RoutedEventArgs e)
        {
            if (this.CancelDirtyOperation())
            {
                return;
            }

            var diag = new OpenFileDialog { DefaultExt = ".rsr", Filter = "Rest Scratch Requets|*.rsr" };

            if ((bool)(!diag.ShowDialog()))
            {
                return;
            }

            this.OpenFile(diag.FileName);
        }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="openFile">The openFile.</param>
        private void OpenFile(string openFile)
        {
            this.Filename = openFile;
            var text = File.ReadAllText(openFile, Encoding.Default);
            var data = Encoding.Default.GetBytes(text);

            var dcjs = new DataContractJsonSerializer(typeof(RequestSettings));
            using (var ms = new MemoryStream(data))
            {
                RequestSettings = (RequestSettings)dcjs.ReadObject(ms);
            }

            this.BindRequestSettings();
        }

        /// <summary>
        /// Handles the Click event of the miSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MiSaveClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.Filename))
            {
                this.MiSaveAsClick(sender, e);
                return;
            }

            this.SaveRequest(this.Filename);
        }

        /// <summary>
        /// Handles the Click event of the miSaveAs control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MiSaveAsClick(object sender, RoutedEventArgs e)
        {
            var diag = new SaveFileDialog
                           {
                               FileName =
                                   string.IsNullOrWhiteSpace(this.Filename)
                                       ? "RestScratchRequest"
                                       : Path.GetFileNameWithoutExtension(this.Filename),
                               DefaultExt = ".rsr",
                               Filter = "Rest Scratch Requets|*.rsr"
                           };

            if ((bool)diag.ShowDialog())
            {
                this.SaveRequest(diag.FileName);
            }
        }

        /// <summary>
        /// Saves the request.
        /// </summary>
        /// <param name="saveFilename">The openFile.</param>
        private void SaveRequest(string saveFilename)
        {
            var dcjs = new DataContractJsonSerializer(typeof(RequestSettings));
            this.Filename = saveFilename;
            using (var ms = new MemoryStream())
            {
                dcjs.WriteObject(ms, RequestSettings);
                File.WriteAllBytes(saveFilename, ms.ToArray());
            }

            this.IsDirty = false;
        }

        /// <summary>
        /// Cancels the dirty operation.
        /// </summary>
        /// <returns>The result to cancel this operation</returns>
        private bool CancelDirtyOperation()
        {
            if (!this.IsDirty)
            {
                return false;
            }

            return MessageBox.Show("You have unsaved changes.\r\nDo you want to continue?", "Unsaved Changes", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes;
        }

        /// <summary>
        /// Handles the Click event of the miNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MiNewClick(object sender, RoutedEventArgs e)
        {
            if (this.CancelDirtyOperation())
            {
                return;
            }

            RequestSettings = new RequestSettings();
            this.Filename = null;
            this.BindRequestSettings();
        }

        /// <summary>
        /// Handles the Click event of the miExit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void MiExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the bBrowseFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BBrowseFileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Filter = "All Files (*.*)|*.*", Title = "Browse to file for upload" };

            if ((bool)dialog.ShowDialog())
            {
                tbFilePath.Text = dialog.FileName;
            }
        }

        /// <summary>
        /// Handles the Click event of the bClearFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void BClearFileClick(object sender, RoutedEventArgs e)
        {
            tbFilePath.Text = string.Empty;
        }
   }
}
