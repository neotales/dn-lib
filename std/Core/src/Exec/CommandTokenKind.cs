namespace Neotales.Exec
{

    /// <summary>
    /// The kinds of token <see cref="CommandLexer"/> produces from a command line.
    /// </summary>
    /// <remarks>
    /// A token is either a piece of text to pass to a process, or a control token
    /// such as a pipe or a redirection boundary. <see cref="CommandLexer"/> keeps
    /// them apart so a caller can decide what to do with each.
    /// </remarks>
    public enum CommandTokenKind
    {
        /// <summary>
        /// The lexer could not classify the input, which is also the default value.
        /// </summary>
        Invalid = 0,

        /// <summary>An unquoted run of text to pass to a process.</summary>
        Arg,

        /// <summary>Text inside single quotes, which is passed through literally.</summary>
        SingleQuotedArg,

        /// <summary>Text inside double quotes, where an escape is still interpreted.</summary>
        DoubleQuotedArg,

        /// <summary>The start of a sub process, written as <c>(</c>.</summary>
        SubProcessStart,

        /// <summary>The end of a sub process, written as <c>)</c>.</summary>
        SubProcessEnd,

        /// <summary>A conditional run, written as <c>&amp;&amp;</c>.</summary>
        And,

        /// <summary>A conditional run, written as <c>||</c>.</summary>
        Or,

        /// <summary>A pipe between two processes, written as <c>|</c>.</summary>
        Pipe,

        /// <summary>The end of a statement, written as <c>;</c> or a newline.</summary>
        StatementEnd,

        /// <summary>The start of a command substitution, written as <c>$(</c>.</summary>
        ClosureStart,

        /// <summary>The end of a command substitution, written as <c>)</c>.</summary>
        ClosureEnd,
    }
}
