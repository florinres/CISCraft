namespace Assembler.Business
{
    /// <summary>
    /// Information about an assembly error, including location
    /// </summary>
    public class AssemblyError
    {
        /// <summary>
        /// Line where the error occurred (1-based)
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Column where the error occurred (1-based)
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Length of the error text
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

}