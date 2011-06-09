using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace RestScratch
{
    [DataContract]
    public class RequestSettings : INotifyPropertyChanged
    {
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

        string contentBody;
        [DataMember]
        public string ContentBody
        {
            get { return contentBody; }
            set
            {
                contentBody= value;
                OnPropertyChanged("QueryString");
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
            string postData = null;
            if (Form.Count > 0)
                postData = ReconstructQueryString(Form);
            else if( !string.IsNullOrWhiteSpace(ContentBody))
                postData = ContentBody;

            foreach (KeyValuePair<string, string> item in this.Headers)
                request.Headers.Add(item.Key, item.Value);

            if (!string.IsNullOrWhiteSpace(postData) && request.Method.ToUpperInvariant() != "GET" )
            {
                byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(postData);
                request.ContentLength = data.Length;
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
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
