namespace GameMeanMachine.Unity.WindRose
{
    namespace Types
    {
        /// <summary>
        ///   Base class for WindRose exceptions.
        /// </summary>
        public class Exception : AlephVault.Unity.Support.Types.Exception
        {
            public Exception() {}
            public Exception(string message) : base(message) {}
            public Exception(string message, System.Exception inner) : base(message, inner) {}
        }
    }
}