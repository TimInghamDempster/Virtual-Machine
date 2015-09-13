﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class CPU
    {
        CPUCore[] m_cores;
        Clock m_clock;
		const uint NumCores = 1;
		
        InterconnectTerminal m_IOInterconnect;
		List<InterconnectTerminal> m_coreUncoreInterconnects;
		Uncore m_uncore;
		InterruptController m_interruptController;
		InterconnectTerminal m_PICUncoreInterconnect;
		InterconnectTerminal m_uncorePICInterconenct;

        public CPU(InterconnectTerminal IOInterconnect)
        {
            m_clock = new Clock();

            m_IOInterconnect = IOInterconnect;

			m_coreUncoreInterconnects = new List<InterconnectTerminal>();

			m_PICUncoreInterconnect = new InterconnectTerminal(1,10);
			m_uncorePICInterconenct = new InterconnectTerminal(1,10);
			m_PICUncoreInterconnect.SetOtherEnd(m_uncorePICInterconenct);

			m_uncore = new Uncore(m_IOInterconnect, m_uncorePICInterconenct);
			m_interruptController = new InterruptController(m_PICUncoreInterconnect);

            m_cores = new CPUCore[NumCores];

			for(uint i = 0; i < NumCores; i++)
			{
				InterconnectTerminal coreUncore = new InterconnectTerminal(1, 10);
				InterconnectTerminal uncoreCore = new InterconnectTerminal(1, 10);
				coreUncore.SetOtherEnd(uncoreCore);

				m_coreUncoreInterconnects.Add(coreUncore);
				m_coreUncoreInterconnects.Add(uncoreCore);

				m_uncore.AddCoreInterconnect(uncoreCore);
				m_cores[i] = new CPUCore(coreUncore, i, m_interruptController);
			}
        }
        
        public void Tick()
        {
            m_cores[0].Tick();
            m_clock.Tick();
			m_uncore.Tick();
			m_interruptController.Tick();

			m_uncorePICInterconenct.Tick();
			m_PICUncoreInterconnect.Tick();

			foreach(InterconnectTerminal ic in m_coreUncoreInterconnects)
			{
				ic.Tick();
			}
        }
    }
}
