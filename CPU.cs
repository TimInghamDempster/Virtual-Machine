using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum Operations
    {
        None,
        AddLiteral,
        MoveToRegister,
        MoveFromRegisterToRegisterLocation,
        DummyForCount
    }

    class CPU
    {
        CPUCore[] m_cores;
        public Bus m_northbridge;
        public Clock m_clock;

        public static uint[] CycleCountsPerInstruction;
        public static uint InstructionSize = 12;

        public CPU(Bus northbridge)
        {
            m_cores = new CPUCore[1];
            m_cores[0] = new CPUCore(this);
            m_northbridge = northbridge;
            m_clock = new Clock();

            SetupCycleCounts();
        }

        void SetupCycleCounts()
        {
            CycleCountsPerInstruction = new uint[(int)Operations.DummyForCount];
            CycleCountsPerInstruction[(int)Operations.MoveToRegister] = 3;
            CycleCountsPerInstruction[(int)Operations.MoveFromRegisterToRegisterLocation] = 3;
            CycleCountsPerInstruction[(int)Operations.AddLiteral] = 4;
        }

        public void Tick()
        {
            m_cores[0].Tick();
            m_clock.Tick();
        }
    }
}
