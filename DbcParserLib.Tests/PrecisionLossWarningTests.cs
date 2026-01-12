using NUnit.Framework;
using DbcParserLib;
using DbcParserLib.Observers;
using System.Linq;

namespace DbcParserLib.Tests
{
    [TestFixture]
    public class PrecisionLossWarningTests
    {
        [Test]
        public void IntegerPropertyWithFloatValue_WarnsAboutPrecisionLoss()
        {
            var dbcString = @"
VERSION """"

NS_ :

BS_:

BO_ 100 TestMessage: 8 Vector__XXX
 SG_ TestSignal : 0|8@1+ (1,0) [0|0] """"  Vector__XXX

BA_DEF_ BO_ ""GenMsgCycleTime"" INT 0 10000;
BA_ ""GenMsgCycleTime"" BO_ 100 100.5;
";
            var observer = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(observer);
            var dbc = Parser.Parse(dbcString);
            
            var errors = observer.GetErrorList();
            Assert.That(errors.Any(e => e.Contains("Precision loss") && e.Contains("GenMsgCycleTime") && e.Contains("100.5") && e.Contains("100")), Is.True, 
                $"Expected precision loss warning but got: {string.Join(", ", errors)}");
            
            // Verify the value was still parsed and rounded
            Assert.That(dbc.Messages.First().CycleTime(out var cycleTime), Is.True);
            Assert.That(cycleTime, Is.EqualTo(100));
        }

        [Test]
        public void IntegerPropertyWithWholeNumberFloatValue_DoesNotWarn()
        {
            var dbcString = @"
VERSION """"

NS_ :

BS_:

BO_ 100 TestMessage: 8 Vector__XXX
 SG_ TestSignal : 0|8@1+ (1,0) [0|0] """"  Vector__XXX

BA_DEF_ BO_ ""GenMsgCycleTime"" INT 0 10000;
BA_ ""GenMsgCycleTime"" BO_ 100 100.0;
";
            var observer = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(observer);
            var dbc = Parser.Parse(dbcString);
            
            var errors = observer.GetErrorList();
            Assert.That(errors.Any(e => e.Contains("Precision loss")), Is.False, 
                $"Did not expect precision loss warning but got: {string.Join(", ", errors)}");
            
            // Verify the value was parsed correctly
            Assert.That(dbc.Messages.First().CycleTime(out var cycleTime), Is.True);
            Assert.That(cycleTime, Is.EqualTo(100));
        }

        [Test]
        public void FloatPropertyDefinedAsFloat_NoWarning()
        {
            var dbcString = @"
VERSION """"

NS_ :

BS_:

BO_ 100 TestMessage: 8 Vector__XXX
 SG_ TestSignal : 0|8@1+ (1,0) [0|0] """"  Vector__XXX

BA_DEF_ BO_ ""GenMsgCycleTime"" FLOAT 0 10000;
BA_ ""GenMsgCycleTime"" BO_ 100 100.5;
";
            var observer = new SimpleFailureObserver();
            Parser.SetParsingFailuresObserver(observer);
            var dbc = Parser.Parse(dbcString);
            
            var errors = observer.GetErrorList();
            Assert.That(errors.Any(e => e.Contains("Precision loss")), Is.False, 
                $"Did not expect precision loss warning for float property but got: {string.Join(", ", errors)}");
            
            // Verify the value was parsed correctly as float
            var message = dbc.Messages.First();
            Assert.That(message.CustomProperties.TryGetValue("GenMsgCycleTime", out var property), Is.True);
            Assert.That(property.FloatCustomProperty, Is.Not.Null);
            Assert.That(property.FloatCustomProperty.Value, Is.EqualTo(100.5).Within(0.001));
        }
    }
}
