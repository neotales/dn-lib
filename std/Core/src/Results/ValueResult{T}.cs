using System.Runtime.CompilerServices;

namespace Neotales.Results
{
    /// <summary>A readonly result containing either a value of <typeparamref name="T"/> or an error.</summary>
    /// <typeparam name="T">Success value type.</typeparam>
    [Union]
    public readonly struct ValueResult<T> : IResult<T>
    {
        private readonly T? value;
        private readonly Error error;
        private readonly bool hasValue;

        /// <summary>Initializes a new instance of the <see cref="ValueResult{T}"/> struct in a successful state.</summary>
        /// <param name="value">Success value.</param>
        public ValueResult(T value)
        {
            this.value = value;
            this.error = Error.Empty;
            this.hasValue = true;
        }

        /// <summary>Initializes a new instance of the <see cref="ValueResult{T}"/> struct in an error state.</summary>
        /// <param name="error">Failure.</param>
        public ValueResult(Error error)
        {
            this.value = default;
            this.error = error;
            this.hasValue = false;
        }

        /// <summary>Gets a value indicating whether this result contains a success value.</summary>
        public bool HasValue => this.hasValue;

        /// <summary>Gets a value indicating whether this result contains an error.</summary>
        public bool HasError => !this.hasValue;

        /// <summary>Gets the active union alternative.</summary>
        public object? Value => this.hasValue ? this.value : this.error;

        /// <inheritdoc/>
        object? IUnion.Value => this.Value;

        /// <summary>Creates a successful result.</summary>
        /// <param name="value">The success value.</param>
        /// <returns>The result value or error described by this member.</returns>
        public static ValueResult<T> Ok(T value) => new(value);

        /// <summary>Creates an error result.</summary>
        /// <param name="error">Failure.</param>
        /// <returns>The result value or error described by this member.</returns>
        public static ValueResult<T> Fail(Error error) => new(error);

        /// <summary>Returns the value or throws an exception with a custom message.</summary>
        /// <param name="message">Message used if the result is an error.</param>
        /// <returns>The result value or error described by this member.</returns>
        public T Expect(string message)
            => this.hasValue ? this.value! : throw new ResultException(message, (object)this.error);

        /// <summary>Returns the value or throws the error.</summary>
        /// <returns>The result value or error described by this member.</returns>
        public T ValueOrThrow() => this.hasValue ? this.value! : throw this.error.ToException();

        /// <summary>Returns the error or throws if the result has a value.</summary>
        /// <param name="message">Message used when no error is present.</param>
        /// <returns>The result value or error described by this member.</returns>
        public Error ExpectError(string message = "Result has a value.")
            => this.hasValue ? throw new ResultException(message, (object?)this.value) : this.error;

        /// <summary>Returns the value or default.</summary>
        /// <returns>The result value or error described by this member.</returns>
        public T ValueOrDefault() => this.hasValue ? this.value! : default!;

        /// <summary>Returns the value or a fallback.</summary>
        /// <param name="defaultValue">Fallback value.</param>
        /// <returns>The result value or error described by this member.</returns>
        public T ValueOrDefault(T defaultValue) => this.hasValue ? this.value! : defaultValue;

        /// <summary>Returns the value or invokes a fallback for an error.</summary>
        /// <param name="defaultValueFactory">Factory invoked only when in error state.</param>
        /// <returns>The success value or fallback.</returns>
        public T ValueOrDefault(Func<T> defaultValueFactory)
            => this.hasValue ? this.value! : defaultValueFactory();

        /// <summary>Maps the success value.</summary>
        /// <typeparam name="U">Mapped value type.</typeparam>
        /// <param name="map">Mapping function.</param>
        /// <returns>The mapped value, or default in error state.</returns>
        public U Map<U>(Func<T, U> map)
            => this.hasValue ? map(this.value!) : default!;

        /// <summary>Maps the success value or maps the error.</summary>
        /// <typeparam name="U">Mapped value type.</typeparam>
        /// <param name="map">Success mapping function.</param>
        /// <param name="mapError">Error mapping function.</param>
        /// <returns>The selected mapping result.</returns>
        public U MapEither<U>(Func<T, U> map, Func<Error, U> mapError)
            => this.hasValue ? map(this.value!) : mapError(this.error);

        /// <summary>Returns this result or a fallback success value.</summary>
        /// <param name="value">Fallback value.</param>
        /// <returns>This result when successful; otherwise the fallback result.</returns>
        public ValueResult<T> Or(T value)
            => this.hasValue ? this : new(value);

        /// <summary>Invokes an inspector when the result is successful.</summary>
        /// <param name="inspector">Action receiving the success value.</param>
        public void Inspect(Action<T> inspector)
        {
            if (this.hasValue)
            {
                inspector(this.value!);
            }
        }

        /// <summary>Attempts to get the value.</summary>
        /// <param name="value">Receives the success value when present.</param>
        /// <returns>The result value or error described by this member.</returns>
        public bool TryGetValue(out T value)
        {
            value = this.ValueOrDefault();
            return this.hasValue;
        }

        /// <summary>Attempts to get the error.</summary>
        /// <param name="value">Receives the error when present.</param>
        /// <returns>The result value or error described by this member.</returns>
        public bool TryGetError(out Error value)
        {
            value = this.error;
            return this.hasValue is false;
        }

        /// <summary>Returns the error or default.</summary>
        /// <returns>The result value or error described by this member.</returns>
        public Error ErrorOrDefault() => this.hasValue ? default : this.error;

        /// <inheritdoc/>
        IError IResult.ErrorOrDefault() => this.ErrorOrDefault();

        /// <inheritdoc/>
        IError IResult.ErrorOrDefault(Func<IError> defaultValueFactory)
            => this.hasValue ? defaultValueFactory() : this.error;

        /// <inheritdoc/>
        bool IResult<T>.TryGetValue(out T value) => this.TryGetValue(out value);

        /// <inheritdoc/>
        bool IResult<T>.TryGetError(out IError error)
        {
            if (this.hasValue)
            {
                error = default!;
                return false;
            }

            error = this.error;
            return true;
        }

        /// <inheritdoc/>
        T IResult<T>.ValueOrDefault() => this.ValueOrDefault();

        /// <inheritdoc/>
        T IResult<T>.ValueOrDefault(Func<T> defaultValueFactory)
            => this.ValueOrDefault(defaultValueFactory);

        /// <summary>Matches the active result alternative.</summary>
        /// <param name="onValue">Called with the success value.</param>
        /// <returns>The selected callback result.</returns>
        public bool Match(Func<T, bool> onValue)
            => this.hasValue && onValue(this.value!);

        /// <summary>Matches the active result alternative.</summary>
        /// <param name="onValue">Called with the success value.</param>
        /// <param name="onError">Called with the error.</param>
        /// <returns>The selected callback result.</returns>
        public bool Match(Func<T, bool> onValue, Func<Error, bool> onError)
            => this.hasValue ? onValue(this.value!) : onError(this.error);

        /// <summary>Converts to the reference result representation.</summary>
        /// <param name="result">The source value result.</param>
        /// <returns>The equivalent reference result.</returns>
        public static implicit operator Result<T>(ValueResult<T> result)
            => result.hasValue ? new(result.value!) : new(result.error);

        /// <summary>Converts a reference result to the value representation.</summary>
        /// <param name="result">The source result.</param>
        /// <returns>The equivalent value result.</returns>
        public static implicit operator ValueResult<T>(Result<T> result)
            => result.HasValue ? new(result.ValueOrDefault()) : new(result.ErrorOrDefault());

        /// <summary>Converts to the typed-error value result.</summary>
        /// <param name="result">The source value result.</param>
        /// <returns>The equivalent typed-error value result.</returns>
        public static implicit operator ValueResult<T, Exception>(ValueResult<T> result)
            => result.hasValue
                ? new(result.value!)
                : new(Error<Exception>.From(
                    result.error.Exception ?? result.error.ToException(),
                    (IError)result.error,
                    result.error.Message,
                    result.error.Code));

        /// <summary>Converts a success value to a value result.</summary>
        /// <param name="value">The success value.</param>
        public static implicit operator ValueResult<T>(T value) => new(value);

        /// <summary>Converts an error to a value result.</summary>
        /// <param name="error">The failure value.</param>
        public static implicit operator ValueResult<T>(Error error) => new(error);

        /// <summary>Converts this result to its success value, throwing in error state.</summary>
        /// <param name="result">The source result.</param>
        public static implicit operator T(ValueResult<T> result) => result.Expect("Result is in error state.");

        /// <summary>Converts this result to its error, throwing in success state.</summary>
        /// <param name="result">The source result.</param>
        public static implicit operator Error(ValueResult<T> result) => result.ExpectError();
    }
}
