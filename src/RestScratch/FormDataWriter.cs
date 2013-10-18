namespace RestScratch
{
    using System.IO;
    using System.Net;

    /// <summary>
    /// The Form Data Writer
    /// </summary>
    public abstract class FormDataWriter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormDataWriter"/> class.
        /// </summary>
        /// <param name="request">The request.</param>
        protected FormDataWriter(WebRequest request)
        {
            this.Request = request;
        }

        /// <summary>
        /// Gets the request.
        /// </summary>
        /// <value>
        /// The request.
        /// </value>
        protected WebRequest Request { get; private set; }
        
        /// <summary>
        /// Writes the request stream.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void WriteRequestStream(RequestSettings settings)
        {
            this.AlterRequestContentType(settings);

            var requestStream = this.Request.GetRequestStream();
            this.WriteFormData(requestStream, settings);
            this.WriteFileData(requestStream, settings);
            requestStream.Close();
        }

        /// <summary>
        /// Alters the type of the request content.
        /// </summary>
        /// <param name="settings">The settings.</param>
        protected virtual void AlterRequestContentType(RequestSettings settings)
        {
        }

        /// <summary>
        /// Writes the form data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="settings">The settings.</param>
        protected virtual void WriteFormData(Stream stream, RequestSettings settings)
        {
        }

        /// <summary>
        /// Writes the file data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="settings">The settings.</param>
        protected virtual void WriteFileData(Stream stream, RequestSettings settings)
        {
        }
    }
}
