namespace Siniskalc.Homm2.Tests.Data
{
    /// <summary>
    /// Lists the bytes that differ between two versions of a record, so that a test of a serializer can state exactly which bytes a
    /// change of the model writes and that no other byte moves.
    /// </summary>
    internal static class ByteDiff
    {
        public static (int Offset, byte Before, byte After)[] Of(byte[] before, byte[] after)
        {
            Assert.That(after, Has.Length.EqualTo(before.Length), "a serializer keeps the size of the record");
            return Enumerable.Range(0, before.Length)
                .Where(i => before[i] != after[i])
                .Select(i => (i, before[i], after[i]))
                .ToArray();
        }
    }
}
