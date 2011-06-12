using System.IO;
using System.Net;

namespace RestScratch
{
    public abstract class FormDataWriter
    {
        protected WebRequest Request { get; private set; }

        protected FormDataWriter(WebRequest request)
        {
            Request = request;
        }

        public void WriteRequestStream(RequestSettings settings)
        {
            AlterRequestContentType(settings);

            Stream requestStream = Request.GetRequestStream();
            WriteFormData(requestStream, settings);
            WriteFileData(requestStream, settings);
            requestStream.Close();
        }

        protected virtual void AlterRequestContentType(RequestSettings settings) { }
        protected virtual void WriteFormData(Stream stream, RequestSettings settings) { }
        protected virtual void WriteFileData(Stream stream, RequestSettings settings) { }

    }
}
