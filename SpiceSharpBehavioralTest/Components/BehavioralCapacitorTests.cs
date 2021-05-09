using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpBehavioralTest.Components
{
    [TestFixture]
    public class BehavioralCapacitorTests
    {
        [Test]
        public void When_SimpleCapacitorTransient_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Sine(0, 1, 1e3)),
                new Capacitor("C1", "in", "0", 1e-3),
                new BehavioralCapacitor("C2", "in", "0", "1m*x"));
            var tran = new Transient("tran", 1e-6, 1e-3);

            // Check currents
            var refCurrent = new RealPropertyExport(tran, "C1", "i");
            var actCurrent = new RealPropertyExport(tran, "C2", "i");

            // Check voltages
            var refVoltage = new RealPropertyExport(tran, "C1", "v");
            var actVoltage = new RealPropertyExport(tran, "C2", "v");

            // check powers
            var refPower = new RealPropertyExport(tran, "C1", "p");
            var actPower = new RealPropertyExport(tran, "C2", "p");

            tran.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(refCurrent.Value, actCurrent.Value, 1e-9);
                Assert.AreEqual(refVoltage.Value, actVoltage.Value, 1e-9);
                Assert.AreEqual(refPower.Value, actPower.Value, 1e-9);
            };
            tran.Run(ckt);
        }

        [Test]
        public void When_SimpleCapacitorFrequency_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 0).SetParameter("acmag", 1.0),
                new Capacitor("C1", "in", "0", 1e-3),
                new BehavioralCapacitor("C2", "in", "0", "1m*x"));
            var ac = new AC("ac", new DecadeSweep(1, 1e6, 5));

            var refExport = new ComplexPropertyExport(ac, "C1", "i");
            var actExport = new ComplexPropertyExport(ac, "C2", "i");
            ac.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(refExport.Value.Real, actExport.Value.Real, 1e-9);
                Assert.AreEqual(refExport.Value.Imaginary, actExport.Value.Imaginary, 1e-9);
            };
            ac.Run(ckt);
        }

        [Test]
        public void When_LowpassRCTransientTrapezoidal_Expect_Reference()
        {
            /*
             * A test for a lowpass RC circuit (DC voltage, resistor, capacitor)
             * The initial voltage on capacitor is 0V. The result should be an exponential converging to dcVoltage.
             */
            double dcVoltage = 10;
            var resistorResistance = 10e3; // 10000;
            var capacitance = 1e-6; // 0.000001;
            var tau = resistorResistance * capacitance;

            // Build circuit
            var ckt = new Circuit(
                new BehavioralCapacitor("C1", "OUT", "0", $"{capacitance}*x"),
                new Resistor("R1", "IN", "OUT", resistorResistance),
                new VoltageSource("V1", "IN", "0", dcVoltage)
                );

            // Create simulation, exports and references
            var tran = new Transient("tran", 1e-8, 10e-6);
            tran.TimeParameters.InitialConditions["OUT"] = 0.0;
            IExport<double> exports = new RealPropertyExport(tran, "C1", "v");
            Func<double, double> reference = t => dcVoltage * (1.0 - Math.Exp(-t / tau));

            // Run
            tran.ExportSimulationData += (sender, args) =>
            {
                double v_actual = args.GetVoltage("OUT");
                double v_reference = reference(args.Time);
                double tol = Math.Max(Math.Abs(v_actual), Math.Abs(v_reference)) * 1e-3 + 1e-12;
                Assert.AreEqual(v_reference, v_actual, tol);
            };
            tran.Run(ckt);
        }
    }
}
