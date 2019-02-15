using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using FsCheck;

namespace Sheepish.CSharp
{
    using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

    [TestFixture]
    public class G_Property_Tests_With_Oracle
    {
        [Property(Arbitrary = new[] { typeof(G_Property_Tests_With_Oracle) })] // *
        public bool SheepBleatExpression(SheepishTestCase nearlySheepish) =>
            Regex.IsMatch(nearlySheepish.Text, @"^baa+$")
            == Sheepish.IsSheepBleat(nearlySheepish.Text);

        [Property(Arbitrary = new[] { typeof(G_Property_Tests_With_Oracle) })] // * 
        public void SheepBleatAssertion(SheepishTestCase nearlySheepish) =>
            Assert.AreEqual(
                Regex.IsMatch(nearlySheepish.Text, @"^baa+$"),
                Sheepish.IsSheepBleat(nearlySheepish.Text));

        // Reproduce an test case that used to fail.
        //[Property(Arbitrary = new[] { typeof(G_Property_Tests_With_Oracle) },
        //Replay = "(3733660618497006796,9742222590813313245)")]
        //public bool SheepBleatExpressionThatFailedBefore(SheepishTestCase nearlySheepish) =>
        //    SheepBleatExpression(nearlySheepish);

        // * FsCheck uses reflection to find all the public Arbitrary functions that this class exposes.
        public static Arbitrary<SheepishTestCase> NearlySheepish() =>
            Gen.zip(
                Gen.zip3(UsuallyEmptyString,
                         StringOfB,
                         UsuallyEmptyString),
                Gen.zip3(StringOfA,
                         UsuallyEmptyString,
                         StringOfA))
            .Select(t => new SheepishTestCase
            {
                Text = t.Item1.Item1 + t.Item1.Item2 + t.Item1.Item3
                     + t.Item2.Item1 + t.Item2.Item2 + t.Item2.Item3,
            })
            .ToArbitrary();

        static Gen<string> UsuallyEmptyString =>
            Gen.Frequency(
                Tuple.Create(9, Gen.Constant("")),
                Tuple.Create(1, Arb.Default.NonEmptyString().Generator.Select(s => s.Item)));

        static Gen<string> StringOfA =>
            Gen.Choose(-3, 3).Select(n => new string('a', Math.Max(n, 0)));
        static Gen<string> StringOfB =>
            Gen.Choose(0, 3).Select(n => new string('b', Math.Max(n, 0)));

        public class SheepishTestCase
        {
            public string Text;
            public override string ToString() => Text;
        }
    }
}
