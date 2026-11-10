namespace Neotales.Results.Tests
{
    public class ResultEnhancementTests
    {
        [Fact]
        public void ExpectAndExpectError_PreserveTheSelectedAlternative()
        {
            Result<int> success = new(42);
            var error = Error.From("failure", "E_FAIL");
            Result<int> failure = new(error);

            Assert.Equal(42, success.Expect("not used"));
            Assert.Equal(error, failure.ExpectError());
            Assert.Equal("custom", Assert.Throws<ResultException>(() => failure.Expect("custom")).Message);
            Assert.Equal(42, Assert.Throws<ResultException>(() => success.ExpectError("custom")).Value);
        }

        [Fact]
        public void TypedExpectHelpers_AgreeForReferenceAndValueResults()
        {
            var error = Error<string>.From("bad", "E_BAD");
            Result<int, string> referenceSuccess = Result.Ok<int, string>(8);
            Result<int, string> referenceFailure = Result.Fail<int, string>(error);
            ValueResult<int, string> valueSuccess = 8;
            ValueResult<int, string> valueFailure = error;

            Assert.Equal(8, referenceSuccess.Expect("unused"));
            Assert.Equal(8, valueSuccess.Expect("unused"));
            Assert.Equal(error, referenceFailure.ExpectError());
            Assert.Equal(error, valueFailure.ExpectError());
            var referenceThrown = Assert.Throws<ResultException>(() => referenceFailure.Expect("reference failure"));
            var valueThrown = Assert.Throws<ResultException>(() => valueFailure.Expect("value failure"));
            Assert.Equal(error, referenceThrown.Value);
            Assert.Equal(error, valueThrown.Value);
            Assert.Equal(16, valueSuccess.MapResult(x => x * 2).ValueOrDefault());
            Assert.Equal(1, valueFailure.MapError(_ => 1));
            Func<Error<string>, int> wrappedErrorMap = e => e.Value.Length;
            Func<string, int> rawErrorMap = value => value.Length;
            Assert.Equal(3, valueFailure.MapError(wrappedErrorMap));
            Assert.Equal(3, valueFailure.MapErrorValue(rawErrorMap));
            Assert.Equal(0, valueSuccess.MapError(wrappedErrorMap));
            Assert.Equal(0, valueSuccess.MapErrorValue(rawErrorMap));
            Assert.Equal(8, Assert.Throws<ResultException>(() => referenceSuccess.ExpectError()).Value);
            Assert.Equal(8, Assert.Throws<ResultException>(() => valueSuccess.ExpectError()).Value);
        }

        [Fact]
        public void ValueResultExpect_PreservesErrorPayloadAndMatchesReferenceResult()
        {
            var plainError = Error.From("message-only", "E_MESSAGE");
            var plainReferenceThrown = Assert.Throws<ResultException>(
                () => new Result<int>(plainError).Expect("custom message"));
            var plainValueThrown = Assert.Throws<ResultException>(
                () => new ValueResult<int>(plainError).Expect("custom message"));
            Assert.Equal(plainError, plainReferenceThrown.Value);
            Assert.Equal(plainError, plainValueThrown.Value);
            Assert.Null(plainValueThrown.InnerException);

            var cause = new InvalidOperationException("root cause");
            var originalError = Error.From(cause, "source message", "E_SOURCE");
            Result<int> reference = new(originalError);
            ValueResult<int> value = new(originalError);

            var referenceException = Assert.Throws<ResultException>(() => reference.Expect("custom message"));
            var valueException = Assert.Throws<ResultException>(() => value.Expect("custom message"));

            Assert.Equal("custom message", valueException.Message);
            Assert.Equal(referenceException.Message, valueException.Message);
            var preservedError = Assert.IsType<Error>(valueException.Value);
            Assert.Equal(originalError, preservedError);
            Assert.Same(cause, preservedError.Exception);
            Assert.Null(valueException.InnerException);
        }

        [Fact]
        public void ExpectError_PreservesExceptionTypedSuccessAsPayload()
        {
            var successValue = new InvalidOperationException("value alternative");
            Result<Exception> result = Result.Try<Exception>(() => successValue);

            var thrown = Assert.Throws<ResultException>(() => result.ExpectError());

            Assert.Same(successValue, thrown.Value);
            Assert.Null(thrown.InnerException);
        }

        [Fact]
        public void ValueResults_AreReadonlyUnionsAndDefaultMeansError()
        {
            ValueResult success = ValueResult.Ok();
            ValueResult failure = default;
            ValueResult<int> typedSuccess = ValueResult<int>.Ok(13);
            ValueResult<int> typedFailure = default;
            ValueResult<int, string> genericDefault = default;
            ValueResult<int> typedError = ValueResult<int>.Fail(Error.From("typed"));

            Assert.True(success.HasValue);
            Assert.True(failure.HasError);
            Assert.Equal(Never.Value, success.Value);
            Assert.IsType<Error>(failure.Value);
            Assert.True(typedSuccess.Match(v => v == 13, _ => false));
            Assert.True(typedFailure.HasError);
            Assert.False(typedFailure.TryGetValue(out _));
            Assert.True(genericDefault.HasError);
            Assert.IsType<Error<string>>(genericDefault.Value);
            Assert.Equal("typed", typedError.ExpectError().Message);
            Result<int> converted = typedSuccess;
            Assert.Equal(13, converted.Expect("unused"));
            IResult<int> contract = typedSuccess;
            Assert.True(contract.TryGetValue(out var contractValue));
            Assert.Equal(13, contractValue);
            Assert.False(contract.TryGetError(out var successError));
            Assert.Null(successError);
            var interfaceFailure = (IResult<int>)typedError;
            Assert.True(interfaceFailure.TryGetError(out var actualError));
            Assert.Equal(typedError.ErrorOrDefault(), actualError);
            Result untyped = success;
            Assert.True(untyped.HasValue);
        }

        [Fact]
        public void ValueResultErrorConversion_PreservesExceptionAndCauseMetadata()
        {
            var exception = new InvalidOperationException("underlying");
            var nestedCause = Error.From(exception, "nested cause", "E_NESTED");
            var sourceError = Error.From((IError)nestedCause, "outer cause", "E_OUTER");
            ValueResult<int> source = new(sourceError);

            ValueResult<int, Exception> converted = source;
            var convertedError = converted.ErrorOrDefault();

            Assert.True(converted.HasError);
            Assert.Same(exception, convertedError.Value);
            Assert.Same(exception, convertedError.Exception);
            Assert.Equal("outer cause", convertedError.Message);
            Assert.Equal("E_OUTER", convertedError.Code);
            Assert.Equal("outer cause", convertedError.Cause?.Message);
            Assert.Equal("nested cause", convertedError.Cause?.Cause?.Message);
            Assert.Same(exception, Assert.Throws<InvalidOperationException>(() => converted.ValueOrThrow()));
        }

        [Fact]
        public void ValueResultFactoriesAndReferenceConversionsRetainStates()
        {
            var error = Error.From("failed", "E_FAIL");
            ValueResult successfulAction = ValueResult.Ok();
            ValueResult failedAction = ValueResult.Fail(error);
            Result successfulReference = successfulAction;
            ValueResult successfulValue = successfulReference;
            Result failedReference = failedAction;
            ValueResult failedValue = failedReference;
            ValueResult<int> genericSuccess = ValueResult.Ok(21);
            ValueResult<int> genericFailure = ValueResult.Fail<int>(error);
            ValueResult<int> fromReference = new Result<int>(21);

            Assert.True(successfulValue.HasValue);
            Assert.True(failedValue.HasError);
            Assert.Equal("failed", failedValue.ErrorOrDefault().Message);
            Assert.Equal("E_FAIL", failedValue.ErrorOrDefault().Code);
            Assert.Equal(21, genericSuccess.Expect("unused"));
            Assert.Equal(error, genericFailure.ExpectError());
            Assert.Equal(21, fromReference.Expect("unused"));
            Assert.Equal(error, ((Result<int>)genericFailure).ErrorOrDefault());
            Assert.True(failedValue.Match(() => false, e => e.Code == "E_FAIL"));
        }

        [Fact]
        public void ValueResultValueHelpersUseOnlyTheActiveAlternative()
        {
            ValueResult<int> success = ValueResult<int>.Ok(5);
            ValueResult<int> failure = ValueResult<int>.Fail(Error.From("missing", "E_MISSING"));
            var inspected = 0;

            Assert.Equal(5, success.ValueOrDefault());
            Assert.Equal(5, success.ValueOrDefault(() => throw new InvalidOperationException()));
            Assert.Equal(0, failure.ValueOrDefault());
            Assert.Equal(9, failure.ValueOrDefault(9));
            Assert.Equal(9, failure.ValueOrDefault(() => 9));
            Assert.Equal(10, success.Map(value => value * 2));
            Assert.Equal(0, failure.Map(value => value * 2));
            Assert.True(success.Match(value => value == 5));
            Assert.True(failure.Match(_ => false, error => error.Code == "E_MISSING"));

            success.Inspect(value => inspected = value);
            failure.Inspect(_ => inspected = -1);
            Assert.Equal(5, inspected);
            Assert.True(failure.TryGetError(out var error));
            Assert.Equal("missing", error.Message);
            Assert.False(success.TryGetError(out _));
            Assert.Equal(7, failure.Or(7).Expect("fallback"));
            Assert.Equal(5, success.Or(7).Expect("unused"));
        }

        [Fact]
        public void ResultExceptionFactoriesRetainCauseAndCallerCoordinates()
        {
            var cause = new InvalidOperationException("root");
            var wrapped = ResultException.FromException(cause);
            var error = Error.From(cause, "context", "E_CONTEXT");
            var fromError = ResultException.FromUnknown(error);
            var unknown = ResultException.FromUnknown("raw", "prefix");

            Assert.Same(cause, wrapped.InnerException);
            Assert.Same(cause, fromError.InnerException);
            Assert.Equal(error, fromError.Value);
            Assert.Equal("prefix raw", unknown.Message);
            Assert.Equal("context", Error.From("context").ToException().Message);
            Assert.Equal("typed", Error<int>.From(7, "typed").ToException().Message);
            Assert.Same(cause, ResultException.FromError(cause).InnerException);
            Assert.Equal("Unknown error:", ResultException.FromUnknown(null).Message);
            Assert.Throws<ArgumentNullException>(() => ResultException.FromException(null!));
            var tracked = new ResultException("tracked").TrackCallerInfo();
            Assert.Equal(nameof(ResultExceptionFactoriesRetainCauseAndCallerCoordinates), tracked.Target);
            Assert.True(tracked.LineNumber > 0);
            Assert.EndsWith("ResultEnhancement_Tests.cs", tracked.FilePath);
        }
    }
}
