using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace RestScratch
{
    public class MultiPartFormDataWriter : FormDataWriter
    {
        private readonly string Boundary;
        private readonly byte[] BoundaryBytes;
        public MultiPartFormDataWriter(WebRequest request)
            : base(request)
        {

            Boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");
            BoundaryBytes = ASCIIEncoding.ASCII.GetBytes("\r\n--" + Boundary + "\r\n");
        }

        protected override void AlterRequestContentType(RequestSettings settings)
        {
            Request.ContentType += "; boundary=" + Boundary;
        }

        protected override void WriteFormData(System.IO.Stream stream, RequestSettings settings)
        {
            string formdataTemplate = "\r\n--" + Boundary + "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

            StringWriter sw = new StringWriter();

            foreach (KeyValuePair<string, string> item in settings.Form)
                sw.Write(formdataTemplate, item.Key, item.Value);

            byte[] data = ASCIIEncoding.ASCII.GetBytes(sw.ToString());

            stream.Write(data, 0, data.Length);
            stream.Write(BoundaryBytes, 0, BoundaryBytes.Length);
        }

        protected override void WriteFileData(Stream stream, RequestSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.FilePath) || string.IsNullOrWhiteSpace(settings.FileName)) return;

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n";
            string header = string.Format(headerTemplate, settings.FileName, Path.GetFileName(settings.FilePath));
            byte[] headerData = ASCIIEncoding.ASCII.GetBytes(header);

            stream.Write(headerData, 0, headerData.Length);

            using (FileStream fileStream = new FileStream(settings.FilePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[1024];

                int bytesRead = 0;

                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    stream.Write(buffer, 0, bytesRead);
            }

            stream.Write(BoundaryBytes, 0, BoundaryBytes.Length);
            
        }
    }
}
