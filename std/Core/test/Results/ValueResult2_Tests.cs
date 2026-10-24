using Xunit;

namespace Neotales.Results.Tests
{

    public class ValueResult2Tests
    {
        [Fact]
        public void Constructor_WithValue_SetsHasValue()
        {
            var result = new ValueResult<string, int>("test");

            Assert.True(result.HasValue);
            Assert.False(result.HasError);
            Assert.Equal("test", result.Value);
        }

        [Fact]
        public void Constructor_WithError_SetsHasError()
        {
            var result = new ValueResult<string, int>(Error<int>.From(404));

            Assert.False(result.HasValue);
            Assert.True(result.HasError);
            Assert.Equal(404, result.ErrorOrDefault().Value);
        }

        [Fact]
        public void Map_OnOk_TransformsValue()
        {
            var result = new ValueResult<int, string>(5);
            var mapped = result.Map(x => x * 2);

            Assert.Equal(10, mapped);
        }

        [Fact]
        public void Map_OnError_UsesDefaultValuePath()
        {
            var result = new ValueResult<int, string>(Error<string>.From("error"));
            var mapped = result.Map(x => x * 2);

            Assert.Equal(0, mapped);
        }

        [Fact]
        public void TryGetError_OnError_ReturnsTrue()
        {
            var result = new ValueResult<string, int>(Error<int>.From(404));

            Assert.True(result.TryGetError(out Error<int> error));
            Assert.Equal(404, error.Value);
        }

        [Fact]
        public void TryGetError_WhenOk_ReturnsFalse()
        {
            var result = new ValueResult<string, int>("ok");

            Assert.False(result.TryGetError(out Error<int> error));
            Assert.Equal(default, error);
        }

        [Fact]
        public void TryGetValue_OnError_ReturnsFalse()
        {
            var result = new ValueResult<string, int>(Error<int>.From(500));

            Assert.False(result.TryGetValue(out string value));
            Assert.Equal(default, value);
        }

        [Fact]
        public void ConversionToResult_OnValue_PreservesValue()
        {
            ValueResult<int, string> source = 42;
            Result<int, string> result = source;

            Assert.True(result.HasValue);
            Assert.Equal(42, result.ValueOrDefault());
        }

        [Fact]
        public void ConversionToResult_OnError_PreservesErrorMetadata()
        {
            var exception = new InvalidOperationException("network failure");
            var cause = Error.From(exception, "transport failure", "SSH_TRANSPORT");
            ValueResult<int, string> source = new(new Error<string>("disconnected", (IError)cause, "connection lost", "SSH_DISCONNECTED"));
            Result<int, string> result = source;

            var error = result.ErrorOrDefault();

            Assert.True(result.HasError);
            Assert.Equal("disconnected", error.Value);
            Assert.Equal("connection lost", error.Message);
            Assert.Equal("SSH_DISCONNECTED", error.Code);
            Assert.Same(cause.Exception, error.Exception);
            Assert.Equal(cause.Message, error.Cause?.Message);
        }
    }
}
