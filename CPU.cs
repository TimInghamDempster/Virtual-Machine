using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class CPU
    {
        CPUCore[] m_cores;
        public Bus m_northbridge;
        public Clock m_clock;

        public static uint InstructionSize = 12;

        public CPU(Bus northbridge)
        {
            m_northbridge = northbridge;
            m_clock = new Clock();

            m_cores = new CPUCore[1];
            m_cores[0] = new CPUCore(this);
        }
        
        public void Tick()
        {
            m_cores[0].Tick();
            m_clock.Tick();
        }
    }
}
