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
        MoveToRegister,
        DummyForCount
    }

    class CPU
    {
        CPUCore[] m_cores;
        public Bus m_northbridge;

        public static uint[] CycleCountsPerInstruction;
        public static uint InstructionSize = 12;

        public CPU(Bus northbridge)
        {
            m_cores = new CPUCore[1];
            m_cores[0] = new CPUCore(this);
            m_northbridge = northbridge;

            SetupCycleCounts();
        }

        void SetupCycleCounts()
        {
            CycleCountsPerInstruction = new uint[(int)Operations.DummyForCount];
            CycleCountsPerInstruction[(int)Operations.MoveToRegister] = 5;
        }

        public void Tick()
        {
            m_cores[0].Tick();
        }
    }
}
