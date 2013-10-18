namespace RestScratch
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    /// <summary>
    /// The multi part form data writer.
    /// </summary>
    public class MultiPartFormDataWriter : FormDataWriter
    {
        /// <summary>
        /// The boundary.
        /// </summary>
        private readonly string boundary;

        /// <summary>
        /// The boundary bytes.
        /// </summary>
        private readonly byte[] boundaryBytes;

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiPartFormDataWriter"/> class.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        public MultiPartFormDataWriter(WebRequest request)
            : base(request)
        {
            this.boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            this.boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + this.boundary + "\r\n");
        }

        /// <summary>
        /// The alter request content type.
        /// </summary>
        /// <param name="settings">
        /// The settings.
        /// </param>
        protected override void AlterRequestContentType(RequestSettings settings)
        {
            Request.ContentType += "; boundary=" + this.boundary;
        }

        /// <summary>
        /// The write form data.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        protected override void WriteFormData(System.IO.Stream stream, RequestSettings settings)
        {
            var formdataTemplate = "\r\n--" + this.boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

            var sw = new StringWriter();

            foreach (var item in settings.Form)
            {
                sw.Write(formdataTemplate, item.Key, item.Value);
            }

            var data = Encoding.ASCII.GetBytes(sw.ToString());

            stream.Write(data, 0, data.Length);
            stream.Write(this.boundaryBytes, 0, this.boundaryBytes.Length);
        }

        /// <summary>
        /// The write file data.
        /// </summary>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="settings">
        /// The settings.
        /// </param>
        protected override void WriteFileData(Stream stream, RequestSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.FilePath) || string.IsNullOrWhiteSpace(settings.FileName))
            {
                return;
            }

            const string HeaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n";
            var header = string.Format(HeaderTemplate, settings.FileName, Path.GetFileName(settings.FilePath));
            var headerData = Encoding.ASCII.GetBytes(header);

            stream.Write(headerData, 0, headerData.Length);

            using (var fileStream = new FileStream(settings.FilePath, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[1024];

                int bytesRead;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    stream.Write(buffer, 0, bytesRead);
                }
            }

            stream.Write(this.boundaryBytes, 0, this.boundaryBytes.Length);
        }
    }
}
