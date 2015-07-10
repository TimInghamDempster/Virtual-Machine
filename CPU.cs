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
        public Clock m_clock;

        MemeoryController m_memoryController;

        InterconnectTerminal m_IOInterconnect;

        public CPU(InterconnectTerminal IOInterconnect)
        {
            m_clock = new Clock();

            m_IOInterconnect = IOInterconnect;
            m_memoryController = new MemeoryController();

            m_cores = new CPUCore[1];
            m_cores[0] = new CPUCore(m_IOInterconnect);
        }
        
        public void Tick()
        {
            m_cores[0].Tick();
            m_clock.Tick();
        }
    }
}
