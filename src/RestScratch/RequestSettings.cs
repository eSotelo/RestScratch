namespace RestScratch
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Runtime.Serialization;
    using System.Text;

    /// <summary>
    /// Request Settings
    /// </summary>
    [DataContract]
    public class RequestSettings : INotifyPropertyChanged
    {
        /// <summary>
        /// The content body
        /// </summary>
        private string contentBody;

        /// <summary>
        /// The file name
        /// </summary>
        private string fileName;

        /// <summary>
        /// The file path
        /// </summary>
        private string filePath;

        /// <summary>
        /// The address
        /// </summary>
        private string address;

        /// <summary>
        /// The method
        /// </summary>
        private string method;

        /// <summary>
        /// The content type
        /// </summary>
        private string contentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestSettings"/> class.
        /// </summary>
        public RequestSettings()
        {
            this.Method = "GET";
            this.Headers = new Dictionary<string, string>();
            this.Form = new Dictionary<string, string>();
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets the content body.
        /// </summary>
        /// <value>
        /// The content body.
        /// </value>
        [DataMember]
        public string ContentBody
        {
            get
            {
                return this.contentBody;
            }

            set
            {
                this.contentBody = value;
                this.OnPropertyChanged("ContentBody");
            }
        }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>
        /// The name of the file.
        /// </value>
        [DataMember]
        public string FileName
        {
            get
            {
                return this.fileName;
            }

            set
            {
                this.fileName = value;
                this.OnPropertyChanged("FileName");
            }
        }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        [DataMember]
        public string FilePath
        {
            get
            {
                return this.filePath;
            }

            set
            {
                this.filePath = value;
                this.OnPropertyChanged("FilePath");
            }
        }

        /// <summary>
        /// Gets or sets the address.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        [DataMember]
        public string Address
        {
            get
            {
                return this.address;
            }

            set
            {
                this.address = value;
                this.OnPropertyChanged("Address");
            }
        }

        /// <summary>
        /// Gets or sets the method.
        /// </summary>
        /// <value>
        /// The method.
        /// </value>
        [DataMember]
        public string Method
        {
            get
            {
                return this.method;
            }

            set
            {
                this.method = value;
                this.OnPropertyChanged("Method");
            }
        }

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>
        /// The type of the content.
        /// </value>
        [DataMember]
        public string ContentType
        {
            get
            {
                return this.contentType;
            }

            set
            {
                this.contentType = value;
                this.OnPropertyChanged("ContentType");
            }
        }

        /// <summary>
        /// Gets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        [DataMember]
        public IDictionary<string, string> Headers { get; private set; }

        /// <summary>
        /// Gets the form.
        /// </summary>
        /// <value>
        /// The form.
        /// </value>
        [DataMember]
        public IDictionary<string, string> Form { get; private set; }

        /// <summary>
        /// Adds the header.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddHeader(string key, string value)
        {
            SetDictionaryValue(this.Headers, key, value);
            this.OnPropertyChanged("Headers");
        }

        /// <summary>
        /// Adds the form component.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void AddFormComponent(string key, string value)
        {
            SetDictionaryValue(this.Form, key, value);
            this.OnPropertyChanged("Form");
        }

        /// <summary>
        /// Makes the web request.
        /// </summary>
        /// <returns>The <see cref="WebRequest"/></returns>
        public WebRequest MakeWebRequest()
        {
            var builder = new UriBuilder(this.Address);

            var request = WebRequest.Create(builder.Uri.AbsoluteUri);
            request.Method = this.Method;
            request.ContentType = this.ContentType;

            foreach (var item in this.Headers)
            {
                request.Headers.Add(item.Key, item.Value);
            }

            if (request.Method.ToUpperInvariant() != "GET")
            {
                FormDataWriter formWritter;

                if (request.ContentType == "multipart/form-data")
                {
                    formWritter = new MultiPartFormDataWriter(request);
                }
                else
                {
                    formWritter = new UrlEncodedFormDataWriter(request);
                }

                formWritter.WriteRequestStream(this);
            }

            return request;
        }

        /// <summary>
        /// Reconstructs the query string.
        /// </summary>
        /// <param name="queryComponents">The query components.</param>
        /// <returns>The reconstructed query string from the dictionary</returns>
        internal static string ReconstructQueryString(IDictionary<string, string> queryComponents)
        {
            var output = new StringBuilder();
            var writer = new StringWriter(output);
            var first = true;
            const string QueryComponentFormat = "{0}={1}";

            foreach (var item in queryComponents.Where(item => !string.IsNullOrWhiteSpace(item.Value)))
            {
                if (!first)
                {
                    writer.Write("&");
                }
                else
                {
                    first = false;
                }

                writer.Write(QueryComponentFormat, item.Key, Uri.EscapeDataString(item.Value));
            }

            return output.ToString();
        }

        /// <summary>
        /// Sets the dictionary value.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private static void SetDictionaryValue(IDictionary<string, string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null)
            {
                return;
            }

            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
