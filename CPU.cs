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

        public CPU(Bios bios, Display display)
        {
            m_clock = new Clock();

            m_memoryController = new MemeoryController(bios, display);

            m_cores = new CPUCore[1];
            m_cores[0] = new CPUCore(m_memoryController);
        }
        
        public void Tick()
        {
            m_cores[0].Tick();
            m_clock.Tick();
        }
    }
}
