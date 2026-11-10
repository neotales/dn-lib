namespace Neotales.Results
{

    /// <summary>
    /// Represents exceptions raised when attempting to access values from an error-state <see cref="Result"/>.
    /// </summary>
    /// <remarks>
    /// <example>
    /// <code lang="csharp">
    /// Result result = Result.Fail(Error.From("failed"));
    /// Error error = result; // will throw <see cref="ResultException"/>
    /// </code>
    /// </example>
    /// </remarks>
    public class ResultException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class with a wrapped exception.
        /// </summary>
        /// <param name="exception">The source exception.</param>
        public ResultException(Exception exception)
            : base($"Result error: {exception?.Message ?? throw new ArgumentNullException(nameof(exception))}", exception)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class.
        /// </summary>
        public ResultException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class.
        ///
        /// Initializes a new instance with an error message.
        /// </summary>
        /// <param name="message">The user-facing error message.</param>
        /// <example>
        /// <code lang="csharp">
        /// throw new ResultException("Result is empty");
        /// </code>
        /// </example>
        public ResultException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Gets the result payload associated with this exception.
        /// </summary>
        /// <value>
        /// The <see cref="object"/> associated with the failed result, or <see langword="null"/>.
        /// </value>
        public object? Value
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class.
        ///
        /// Initializes a new instance with a message and payload.
        /// </summary>
        /// <param name="message">The user-facing error message.</param>
        /// <param name="value">Associated payload.</param>
        public ResultException(string message, object? value)
            : base(message)
        {
            this.Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class.
        ///
        /// Initializes a new instance with a message and inner exception.
        /// </summary>
        /// <param name="message">The user-facing error message.</param>
        /// <param name="innerException">The underlying cause.</param>
        public ResultException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResultException"/> class.
        ///
        /// Initializes a new instance with a message, inner exception, and payload.
        /// </summary>
        /// <param name="message">The user-facing error message.</param>
        /// <param name="innerException">The underlying cause.</param>
        /// <param name="value">Associated payload.</param>
        public ResultException(string message, Exception innerException, object? value)
            : base(message, innerException)
        {
            this.Value = value;
        }

        /// <summary>Gets or sets the caller member associated with this exception.</summary>
        public virtual string Target { get; protected set; } = string.Empty;

        /// <summary>Gets or sets the caller line associated with this exception.</summary>
        public virtual int LineNumber
        {
            get; protected set;
        }

        /// <summary>Gets or sets the caller file associated with this exception.</summary>
        public virtual string FilePath { get; protected set; } = string.Empty;

        /// <summary>Creates a result exception from an arbitrary error payload.</summary>
        /// <param name="error">The error payload.</param>
        /// <param name="message">Optional message prefix.</param>
        /// <returns>A new result exception.</returns>
        public static ResultException FromUnknown(object? error, string? message = null)
        {
            message ??= "Unknown error:";
            return error switch
            {
                Exception exception => FromException(exception),
                IError resultError => FromError(resultError),
                null => new ResultException(message),
                _ => new ResultException($"{message} {error}", error),
            };
        }

        /// <summary>Creates a result exception wrapping an exception error.</summary>
        /// <param name="exception">The source exception.</param>
        /// <returns>A new result exception.</returns>
        public static ResultException FromError(Exception exception) => FromException(exception);

        /// <summary>Creates a result exception wrapping the supplied exception.</summary>
        /// <param name="exception">The underlying exception.</param>
        /// <returns>A new result exception.</returns>
        public static ResultException FromException(Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);
            return new ResultException(exception);
        }

        /// <summary>Attaches compiler-captured caller information to this exception.</summary>
        /// <param name="lineNumber">Caller line number.</param>
        /// <param name="filePath">Caller file path.</param>
        /// <param name="target">Caller member name.</param>
        /// <returns>This exception.</returns>
        public ResultException TrackCallerInfo(
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerMemberName] string target = "")
        {
            this.LineNumber = lineNumber;
            this.FilePath = filePath;
            this.Target = target;
            return this;
        }

        private static ResultException FromError(IError error)
        {
            ArgumentNullException.ThrowIfNull(error);
            return error.Exception is { } exception
                ? new ResultException(error.Message, exception, error)
                : new ResultException(error.Message, (object)error);
        }
    }
}
