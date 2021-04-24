using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpBehavioralTest.Components
{
    [TestFixture]
    public class MixedTests
    {
        [Test]
        public void When_MixedSources_Expect_Reference()
        {

            double a0_entry = 0.1;
            double b0_entry = 0.01;
            double density = 850;
            double viscosity = 0.000006;
            double rout_entry = 0.9;
            double rin_entry = 0.8;
            double r1_entry1 = 0.8;
            double r2_entry1 = 0.81;
            double r1_entry2 = 0.89;
            double r2_entry2 = 0.9;
            double FlowRate = 0.025;

            /*
            Example 01

            .param a0_entry=0.1
            .param b0_entry=0.01
            .param density =850
            .param viscosity =0.000006
            .param rout_entry =0.9
            .param rin_entry =0.8
            .param r1_entry1 =0.8
            .param r2_entry1=0.81
            .param r1_entry2 =0.89
            .param r2_entry2=0.9
            .param FlowRate =0.025

            I_M 0 N1 {FlowRate}

            X_entry1   N1 0 N2 entry params: a0={a0_entry}, b0={b0_entry}, ro={density}, v={viscosity}, rout={rout_entry},
            +rin={rin_entry},r1={r1_entry1},r2={r2_entry1}

            X_entry2   N1 0 N3 entry params: a0={a0_entry}, b0={b0_entry}, ro={density}, v={viscosity}, rout={rout_entry},
            +rin={rin_entry},r1={r1_entry2},r2={r2_entry2}

            .subckt entry m_in m_out v_vel params: a0=1, b0=1, ro=1, v=1, rout=1, rin=1, r1=1, r2=1
            .param D0 = {2*a0*b0/(a0+b0)}
            .param F1 = {(rout-rin)*a0*(rout+rin)/(r2+r1)}
            .param F0 = {(r2-r1)*a0}
            .param fraction = {F0/F1}
            .func 	Q(m) {m/ro}
            .func  	vel(m) {Q(m)/(a0*b0)}
            .func 	R(x) {x*D0/v}
            * Changed function
            .func xi(x) {40*pow(R(x),-0.9) + 90*pow(fraction,-0.003)-80}
            Vmas m_in msx {0}
            Rmas msx msy 1e-6
            *The pressure drop:
            Exm msy m_out value={xi(V(v_vel))*ro*V(v_vel)*V(v_vel)/2}
            Hmss mss 0 Vmas 1
            *Velocity:
            Guv 0 v_vel value={vel(V(mss))}
            Ruv 0 v_vel 1
            .ends

            .OP
            .SAVE V(N1) V(N2) V(N3)
            .end
             */

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            var ckt = new Circuit(new CurrentSource("I_M", "0", "N1", FlowRate));
            Entry(ckt, "X_entry1", "N1", "0", "N2", a0_entry, b0_entry, density, viscosity, rout_entry, rin_entry, r1_entry1, r2_entry1);
            Entry(ckt, "X_entry2", "N1", "0", "N3", a0_entry, b0_entry, density, viscosity, rout_entry, rin_entry, r1_entry2, r2_entry2);

            OP op = new OP("op");
            op.Run(ckt);

        }

        private static void Entry(Circuit ckt, string name, string m_in, string m_out, string v_vel, double a0, double b0, double ro, double v, double rout, double rin, double r1, double r2)
        {
            double D0 = 2 * a0 * b0 / (a0 + b0);
            double F1 = (rout - rin) * a0 * (rout + rin) / (r2 + r1);
            double F0 = (r2 - r1) * a0;
            double fraction = F0 / F1;
            Func<string, string> Q = (m) => $"({m}/{ro})";
            Func<string, string> vel = (m) => $"{Q(m)}/{a0 * b0}";
            Func<string, string> R = (x) => $"{x}*{D0 / v}";
            Func<string, string> xi = (x) => $"40*pow({R(x)},-0.9) + 90*pow({fraction},-0.003)-80";

            string msx = $"{name}.msx", msy = $"{name}.msy", mss = $"{name}.mss";
            ckt.Add(new VoltageSource($"{name}.Vmas", m_in, msx, 0.0));
            ckt.Add(new Resistor($"{name}.Rmas", msx, msy, 1e-6));

            // The pressure drop
            ckt.Add(new BehavioralVoltageSource($"{name}.Exm", msy, m_out, $"{xi($"V({v_vel})")}*{ro}*V({v_vel})*V({v_vel})/2"));
            ckt.Add(new CurrentControlledVoltageSource($"{name}.Hmms", mss, "0", $"{name}.Vmas", 1.0));

            // Velocity
            ckt.Add(new BehavioralCurrentSource($"{name}.Guv", "0", v_vel, vel($"V({mss})")));
            ckt.Add(new Resistor($"{name}.Ruv", "0", v_vel, 1.0));
        }

        [Test]
        public void When_ResistorCurrent_Expect_ReferenceWarning()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", 1.0),
                new Resistor("R1", "in", "0", 1e3),
                new BehavioralVoltageSource("V2", "out", "0", "I(R1)"));

            // Catch the warning
            int warnings = 0;
            SpiceSharpWarning.WarningGenerated += (sender, args) =>
            {
                warnings++;
            };

            // Run the simulation
            var op = new OP("op");
            op.ExportSimulationData += (sender, args) =>
            {
                Assert.AreEqual(1.0 / 1e3, args.GetVoltage("out"), 1e-9);
            };
            op.Run(ckt);

            Assert.AreEqual(1, warnings);
        }
    }
}
