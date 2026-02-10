namespace TaskManager.API.Tests.Helpers
{
    public static class TestDataHelper
    {
        /// <summary>
        /// Creates a default row version byte array for testing.
        /// In real SQL Server, this is managed by the database, but for in-memory testing we need to provide it.
        /// </summary>
        public static byte[] DefaultRowVersion => new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };
    }
}
