namespace RestScratch
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;

    /// <summary>
    /// URL encoded form data writer
    /// </summary>
    public class UrlEncodedFormDataWriter : FormDataWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UrlEncodedFormDataWriter"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        public UrlEncodedFormDataWriter(WebRequest request)
            : base(request)
        {
        }

        /// <summary>
        /// Writes the form data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="settings">The settings.</param>
        protected override void WriteFormData(System.IO.Stream stream, RequestSettings settings)
        {
            if (settings.Form.Count == 0 && string.IsNullOrWhiteSpace(settings.ContentBody))
            {
                return;
            }

            var postData = string.IsNullOrWhiteSpace(settings.ContentBody) ? ReconstructQueryString(settings.Form) : settings.ContentBody;

            var data = Encoding.UTF8.GetBytes(postData);

            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Reconstructs the query string.
        /// </summary>
        /// <param name="queryComponents">The query components.</param>
        /// <returns>The reconstructed query string form the dictionary of components</returns>
        private static string ReconstructQueryString(IEnumerable<KeyValuePair<string, string>> queryComponents)
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
    }
}
