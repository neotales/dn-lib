using Neotales.Results;

namespace Neotales.Tests
{

    /// <summary>
    /// Three defects in <c>Env.Expand</c>, asserted against shell behaviour rather
    /// than against the code's own output.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Each test here states what a shell does and asserts this does the same
    /// thing. That is the point: a test written against the implementation's
    /// current output passes by definition and proves nothing, whereas one written
    /// against <c>bash</c> is a specification that a fix has to satisfy.
    /// </para>
    /// <para>
    /// The library's behaviour is not changed here. These are recorded findings,
    /// and the tests are skipped with the reason rather than deleted, because the
    /// assertion is the specification and a deleted test is a defect nobody finds
    /// again. Every skip names what has to change.
    /// </para>
    /// <para>
    /// The last two are not defects but an open decision, and they say so. A skip
    /// that claims a defect where there is a choice teaches the next reader that
    /// this code is known broken, which is a different and wrong claim.
    /// </para>
    /// </remarks>
    public sealed class EnvExpandDefaultValueTests
    {

        /// <summary>
        /// Gets the options a test expands with, carrying one variable.
        /// </summary>
        /// <param name="variables">The variables the expansion may read.</param>
        /// <returns>Options carrying <paramref name="variables"/> and nothing else.</returns>
        private static EnvExpandOptions Options(IDictionary<string, string> variables) => new()
        {
            GetAllVariables = () => variables,
        };

        /// <summary>
        /// A variable used as the default for another is itself expanded, as bash does.
        /// </summary>
        /// <remarks>
        /// Skipped: <c>${A:-$USER}</c> returns empty. The branch guarded by
        /// <c>defaultValue.Contains('$')</c> calls <c>TryExpand</c> on the value,
        /// which is null or whitespace on that branch, so the default is never
        /// expanded at all. Passes when the default is expanded.
        /// </remarks>
        [Fact]
        public static void Expand_ExpandsAVariableUsedAsTheDefaultValue()
        {
            Assert.Skip("KNOWN DEFECT: ${A:-$USER} returns empty. The branch guarded by defaultValue.Contains('$') calls TryExpand on the value, which is null or whitespace on that branch, so the default is never expanded. Passes when the default is expanded, as bash does.");

            Dictionary<string, string> variables = new()
            {
                ["USER"] = "testuser"
            };

            string actual = Env.Expand("${A:-$USER}", Options(variables));

            Assert.Equal("testuser", actual);
        }

        /// <summary>
        /// The same, reached through the non-throwing entry point.
        /// </summary>
        /// <remarks>
        /// Skipped: the same wrong span, so <c>TryExpand</c> reports the same empty
        /// result as <see cref="Expand_ExpandsAVariableUsedAsTheDefaultValue"/>. It
        /// is a separate test because a fix that repaired only the throwing path
        /// would leave the other broken, and this repository's two entry points have
        /// diverged before.
        /// </remarks>
        [Fact]
        public static void Expand_ExpandsAVariableUsedAsTheDefaultValue_In_TryExpand()
        {
            Assert.Skip("KNOWN DEFECT: the same wrong span reached through TryExpand. ${A:-$USER} returns empty where bash returns the value of USER.");

            Dictionary<string, string> variables = new()
            {
                ["USER"] = "testuser"
            };

            Result<ReadOnlyMemory<char>> result = Env.TryExpand("${A:-$USER}", Options(variables));

            Assert.False(result.HasError);
            Assert.Equal("testuser", ((ReadOnlyMemory<char>)result.Value!).Span.ToString());
        }

        /// <summary>
        /// A variable anywhere in a default is expanded, not only at the start.
        /// </summary>
        /// <remarks>
        /// Skipped: <c>${A:-prefix$USER}</c> returns empty. A dollar anywhere in the
        /// default is enough to enter the branch, and the branch expands the empty
        /// value rather than the default, so the surrounding text is lost too.
        /// </remarks>
        [Fact]
        public static void Expand_ExpandsAVariableInsideALongerDefaultValue()
        {
            Assert.Skip("KNOWN DEFECT: ${A:-prefix$USER} returns empty. A dollar anywhere in the default enters the branch, and the branch expands the empty value instead of the default.");

            Dictionary<string, string> variables = new()
            {
                ["USER"] = "testuser"
            };

            string actual = Env.Expand("${A:-prefix$USER}", Options(variables));

            Assert.Equal("prefixtestuser", actual);
        }

        /// <summary>
        /// With assignment switched off, <c>${NAME:=value}</c> does not assign.
        /// </summary>
        /// <remarks>
        /// Skipped: <see cref="EnvExpandOptions.UnixAssignment"/> is declared,
        /// defaulted to true, documented as governing <c>${VAR:=value}</c>, and read
        /// nowhere in the repository. <c>${VAR:=x}</c> calls <c>SetVariable</c>
        /// unconditionally, and <c>SetVariable</c> defaults to
        /// <c>Environment.SetEnvironmentVariable</c>, so expanding a string writes
        /// the process environment even with the option set to false. That is the
        /// worst of the three, because it is a side effect on global state from an
        /// operation documented as a pure function of its input.
        /// </remarks>
        [Fact]
        public static void UnixAssignment_False_LeavesTheVariableUnassigned()
        {
            Assert.Skip("KNOWN DEFECT: EnvExpandOptions.UnixAssignment is declared, defaulted true, documented as governing ${VAR:=value}, and read nowhere. ${VAR:=x} calls SetVariable unconditionally and SetVariable defaults to Environment.SetEnvironmentVariable, so a string expansion writes the process environment even with the option set to false. Passes when the option is honoured.");

            Dictionary<string, string> variables = new()
            {
                ["NAME"] = "before"
            };

            EnvExpandOptions options = Options(variables);
            options.UnixAssignment = false;

            _ = Env.Expand("${NAME:=written}", options);

            Assert.DoesNotContain("written", variables.Values);
        }

        /// <summary>
        /// An unset variable in braces is diagnosed.
        /// </summary>
        /// <remarks>
        /// An open decision rather than a known defect, and this test is one side of
        /// it. <c>$NAME</c> throws; <c>${NAME}</c> returns empty. bash renders
        /// <c>${NAME}</c> as empty too and errors only for <c>${NAME:?}</c>, so the
        /// current behaviour is defensible and the option that looks as though it
        /// was meant to govern this, <see cref="EnvExpandOptions.UnixCustomErrorMessage"/>,
        /// is read nowhere, so both of its values behave identically.
        /// <para>
        /// This stays skipped until the behaviour is chosen. It is not evidence for
        /// either answer, and it is not a claim that the current behaviour is wrong.
        /// </para>
        /// </remarks>
        [Fact]
        public static void Expand_DiagnosesAMissingBracedVariable()
        {
            Assert.Skip("OPEN DECISION rather than a known defect: this asserts that ${NAME} throws when NAME is unset. $NAME throws and EnvExpandOptions.UnixCustomErrorMessage exists to govern this and is read nowhere, so both flag values behave identically. bash renders ${NAME} as empty and errors only for ${NAME:?}, so the current behaviour is defensible and this test encodes the other choice. It stays skipped until the behaviour is decided, and it is not evidence for either answer.");

            Dictionary<string, string> variables = [];

            Assert.Throws<EnvironmentException>(() => Env.Expand("${NAME}", Options(variables)));
        }

        /// <summary>
        /// The same, through the non-throwing entry point, which reports the error
        /// rather than raising it.
        /// </summary>
        /// <remarks>
        /// The same open decision as
        /// <see cref="Expand_DiagnosesAMissingBracedVariable"/>. It is a separate
        /// test because the two entry points report the same condition differently —
        /// one raises, one returns a result — and a change to one need not reach
        /// the other.
        /// </remarks>
        [Fact]
        public static void Expand_ReportsAMissingBracedVariable_FromTryExpand()
        {
            Assert.Skip("OPEN DECISION, the same one as Expand_DiagnosesAMissingBracedVariable, reached through TryExpand. Stays skipped until the behaviour of ${NAME} is decided.");

            Dictionary<string, string> variables = [];

            Result<ReadOnlyMemory<char>> result = Env.TryExpand("${NAME}", Options(variables));

            Assert.True(result.HasError);
        }
    }
}
