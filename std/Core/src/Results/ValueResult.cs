using System.Runtime.CompilerServices;

namespace Neotales.Results
{
    /// <summary>A readonly result for operations without a success payload.</summary>
    [Union]
    public readonly struct ValueResult : IResult
    {
        private readonly Error error;
        private readonly bool hasValue;

        /// <summary>Initializes a new instance of the <see cref="ValueResult"/> struct in a successful state.</summary>
        public ValueResult()
        {
            this.error = Error.Empty;
            this.hasValue = true;
        }

        /// <summary>Initializes a new instance of the <see cref="ValueResult"/> struct in an error state.</summary>
        /// <param name="error">The failure.</param>
        public ValueResult(Error error)
        {
            this.error = error;
            this.hasValue = false;
        }

        /// <summary>Gets a value indicating whether this result contains its success alternative.</summary>
        /// <value><see langword="true"/> when successful.</value>
        public bool HasValue => this.hasValue;

        /// <summary>Gets a value indicating whether this result contains its error alternative.</summary>
        /// <value><see langword="true"/> when in error state.</value>
        public bool HasError => !this.hasValue;

        /// <summary>Gets the active union alternative.</summary>
        public object Value => this.hasValue ? Never.Value : this.error;

        /// <inheritdoc/>
        object? IUnion.Value => this.Value;

        /// <summary>Creates a successful result.</summary>
        /// <returns>A successful <see cref="ValueResult"/>.</returns>
        public static ValueResult Ok() => new();

        /// <summary>Creates a successful value-bearing result.</summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="value">The success value.</param>
        /// <returns>A successful <see cref="ValueResult{T}"/>.</returns>
        public static ValueResult<T> Ok<T>(T value) => new(value);

        /// <summary>Creates an error result.</summary>
        /// <param name="error">The failure.</param>
        /// <returns>An error <see cref="ValueResult"/>.</returns>
        public static ValueResult Fail(Error error) => new(error);

        /// <summary>Creates an error result for the specified success type.</summary>
        /// <typeparam name="T">The success value type.</typeparam>
        /// <param name="error">The failure value.</param>
        /// <returns>An error <see cref="ValueResult{T}"/>.</returns>
        public static ValueResult<T> Fail<T>(Error error) => new(error);

        /// <summary>Converts an error to an error result.</summary>
        /// <param name="error">The failure value.</param>
        public static implicit operator ValueResult(Error error) => new(error);

        /// <summary>Matches the active result alternative.</summary>
        /// <param name="onValue">Called for success.</param>
        /// <param name="onError">Called with the error for failure.</param>
        /// <returns>The selected callback result.</returns>
        public bool Match(Func<bool> onValue, Func<Error, bool> onError)
            => this.hasValue ? onValue() : onError(this.error);

        /// <summary>Converts to the reference result representation.</summary>
        /// <param name="result">The source value result.</param>
        /// <returns>The equivalent untyped result.</returns>
        public static implicit operator Result(ValueResult result)
            => result.hasValue ? Result.Ok() : Result.Fail(result.error);

        /// <summary>Converts an untyped reference result to the value representation.</summary>
        /// <param name="result">The source result.</param>
        /// <returns>The equivalent value result.</returns>
        public static implicit operator ValueResult(Result result)
            => result.HasValue ? Ok() : Fail(new Error(result.ErrorOrDefault()));

        /// <summary>Returns the error or throws when successful.</summary>
        /// <param name="message">Exception message when no error is present.</param>
        /// <returns>The active error.</returns>
        public Error ExpectError(string message = "Result has a value.")
            => this.hasValue ? throw new ResultException(message, Never.Value) : this.error;

        /// <summary>Returns the current error, or the default error.</summary>
        /// <returns>The current error or default.</returns>
        public Error ErrorOrDefault() => this.hasValue ? default : this.error;

        /// <summary>Returns the current error, or invokes a fallback.</summary>
        /// <param name="defaultValueFactory">Fallback factory used for success.</param>
        /// <returns>The current result value.</returns>
        public Error ErrorOrDefault(Func<Error> defaultValueFactory)
            => this.hasValue ? defaultValueFactory() : this.error;

        /// <inheritdoc/>
        IError IResult.ErrorOrDefault() => this.ErrorOrDefault();

        /// <inheritdoc/>
        IError IResult.ErrorOrDefault(Func<IError> defaultValueFactory)
            => this.hasValue ? defaultValueFactory() : this.error;

        /// <summary>Attempts to retrieve the error.</summary>
        /// <param name="value">Receives the active error when present.</param>
        /// <returns><see langword="true"/> when an error exists.</returns>
        public bool TryGetError(out Error value)
        {
            value = this.error;
            return this.HasError;
        }
    }
}
