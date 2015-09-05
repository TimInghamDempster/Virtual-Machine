using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class CPU
    {
        public CPUCore[] m_cores;
        public Clock m_clock;
		const uint NumCores = 1;

        MemeoryController m_memoryController;

        InterconnectTerminal m_IOInterconnect;
		List<InterconnectTerminal> m_coreUncoreInterconnects;
		Uncore m_uncore;

        public CPU(InterconnectTerminal IOInterconnect)
        {
            m_clock = new Clock();

            m_IOInterconnect = IOInterconnect;
            m_memoryController = new MemeoryController();

			m_coreUncoreInterconnects = new List<InterconnectTerminal>();

			m_uncore = new Uncore(m_IOInterconnect);

            m_cores = new CPUCore[NumCores];

			for(uint i = 0; i < NumCores; i++)
			{
				InterconnectTerminal coreUncore = new InterconnectTerminal(1, 10);
				InterconnectTerminal uncoreCore = new InterconnectTerminal(1, 10);
				coreUncore.SetOtherEnd(uncoreCore);

				m_coreUncoreInterconnects.Add(coreUncore);
				m_coreUncoreInterconnects.Add(uncoreCore);

				m_uncore.AddCoreInterconnect(uncoreCore);
				m_cores[i] = new CPUCore(coreUncore, i);
			}
        }
        
        public void Tick()
        {
            m_cores[0].Tick();
            m_clock.Tick();
			m_uncore.Tick();

			foreach(InterconnectTerminal ic in m_coreUncoreInterconnects)
			{
				ic.Tick();
			}
        }
    }
}
