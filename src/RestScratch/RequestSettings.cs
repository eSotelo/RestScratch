using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace RestScratch
{
    [DataContract]
    public class RequestSettings : INotifyPropertyChanged
    {
        string fileName;
        [DataMember]
        public string FileName
        {
            get { return fileName; }
            set
            {
                this.fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        string filePath;
        [DataMember]
        public string FilePath
        {
            get { return filePath; }
            set
            {
                this.filePath = value;
                OnPropertyChanged("FilePath");
            }
        }

        string address;
        [DataMember]
        public string Address
        {
            get { return this.address; }
            set
            {
                this.address = value;
                OnPropertyChanged("Address");
            }
        }
        string method;
        [DataMember]
        public string Method { get { return method; }
            set
            {
                method = value;
                OnPropertyChanged("Method");
            }
        }

        string contentType;
        [DataMember]
        public string ContentType { get { return contentType; }
            set
            {
                contentType = value;
                OnPropertyChanged("ContentType");
            }
        }

        [DataMember]
        public IDictionary<string,string> Headers { get; private set; }
        [DataMember]
        public IDictionary<string, string> Form { get; private set; }


        public void AddHeader(string key, string value)
        {
            SetDictionaryValue(Headers, key, value);
            OnPropertyChanged("Headers");
        }
        public void AddFormComponent(string key, string value)
        {
            SetDictionaryValue(Form, key, value);
            OnPropertyChanged("Form");
        }

        private void SetDictionaryValue(IDictionary<string,string> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
            
        }
        public RequestSettings()
        {
            Method = "GET";
            Headers = new Dictionary<string, string>();
            Form = new Dictionary<string, string>();
        }

        public WebRequest MakeWebRequest()
        {
            UriBuilder builder = new UriBuilder(Address);

            WebRequest request = WebRequest.Create(builder.Uri.AbsoluteUri);
            request.Method = Method;
            request.ContentType = ContentType;

            foreach (KeyValuePair<string, string> item in this.Headers)
                request.Headers.Add(item.Key, item.Value);

            if (request.Method.ToUpperInvariant() != "GET")
            {
                FormDataWriter formWritter = null;
                if (request.ContentType == "application/x-www-form-urlencoded")
                    formWritter = new UrlEncodedFormDataWriter(request);
                else if (request.ContentType == "multipart/form-data")
                    formWritter = new MultiPartFormDataWriter(request);

                if (formWritter != null)
                    formWritter.WriteRequestStream(this);
            }

            return request;
        }


        internal static string ReconstructQueryString(IDictionary<string, string> queryComponents)
        {
            StringBuilder output = new StringBuilder();
            TextWriter writer = new StringWriter(output);
            bool first = true;
            string queryComponentFormat = "{0}={1}";
            foreach (var item in queryComponents)
            {
                if (string.IsNullOrWhiteSpace(item.Value))
                    continue;

                if (!first)
                    writer.Write("&");
                else
                    first = false;

                writer.Write(queryComponentFormat, item.Key, Uri.EscapeDataString(item.Value));

            }

            return output.ToString();
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged == null) return;

            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
