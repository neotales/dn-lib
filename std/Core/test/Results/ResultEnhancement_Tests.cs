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
            Assert.Equal("bad", valueFailure.MapError(e => e.Value));
            Assert.Equal(8, Assert.Throws<ResultException>(() => referenceSuccess.ExpectError()).Value);
            Assert.Equal(8, Assert.Throws<ResultException>(() => valueSuccess.ExpectError()).Value);
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
            Result untyped = success;
            Assert.True(untyped.HasValue);
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
