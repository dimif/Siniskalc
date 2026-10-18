namespace Siniskalc.Homm2.Tests.Data
{
    [TestFixture]
    internal sealed class TmpTurnDump
    {
        [Test]
        public void Dump()
        {
            var lines = new List<string>();
            foreach (var path in new[] { Saves.ThreedomSave(1), Saves.ThreedomSave(20), Saves.Path(Saves.ArraxTheJerk, "arrax_the_jerk_01_start") })
            {
                var g = TestServices.Load(path);
                var b = g.Bytes;
                var l = g.Layout;
                lines.Add($"{Path.GetFileName(path)} date@{l.DateStart:X} cur@{l.CurrentHumanStart:X} mask@{l.HumanMaskStart:X}: {Convert.ToHexString(b, l.DateStart - 16, 24)} turn={b[l.CurrentHumanStart]} current={g.CurrentPlayer}");
            }

            File.WriteAllLines(@"C:\Users\dimit\AppData\Local\Temp\turn_dump.txt", lines);
        }
    }
}
