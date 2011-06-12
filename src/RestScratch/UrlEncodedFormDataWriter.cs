using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace RestScratch
{
    public class UrlEncodedFormDataWriter : FormDataWriter
    {
        public UrlEncodedFormDataWriter(WebRequest request) : base(request) { }

        protected override void WriteFormData(System.IO.Stream stream, RequestSettings settings)
        {
            if (settings.Form.Count == 0 && string.IsNullOrWhiteSpace(settings.ContentBody)) return;

            string postData ="";
            if (string.IsNullOrWhiteSpace(settings.ContentBody))
                postData = ReconstructQueryString(settings.Form);
            else
                postData = settings.ContentBody;

            byte[] data = System.Text.UTF8Encoding.UTF8.GetBytes(postData);

            stream.Write(data, 0, data.Length);
        }

        private static string ReconstructQueryString(IDictionary<string, string> queryComponents)
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

    }
}
