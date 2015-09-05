using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	class Uncore
	{
		private InterconnectTerminal m_IOInterconnect;
		List<InterconnectTerminal> m_coreInterconencts;

		public Uncore(InterconnectTerminal IOInterconnect)
		{
			this.m_IOInterconnect = IOInterconnect;
			m_coreInterconencts = new List<InterconnectTerminal>();
		}

		public void AddCoreInterconnect(InterconnectTerminal coreInterconnect)
		{
			m_coreInterconencts.Add(coreInterconnect);
		}

		public void Tick()
		{
			foreach(InterconnectTerminal ic in m_coreInterconencts)
			{
				if(ic.HasPacket)
				{
					int[] packet = new int[ic.RecievedSize];
					ic.ReadRecievedPacket(packet);
					bool forwarded = m_IOInterconnect.SendPacket(packet, packet.Length);
					if(forwarded)
					{
						ic.ClearRecievedPacket();
					}
				}
			}
			if(m_IOInterconnect.HasPacket)
			{
				int[] packet = new int[m_IOInterconnect.RecievedSize];
				m_IOInterconnect.ReadRecievedPacket(packet);
				
				// assumes single core, fix when moving to multi-core system
				bool forwarded = m_coreInterconencts[0].SendPacket(packet, packet.Length);
				if(forwarded)
				{
					m_IOInterconnect.ClearRecievedPacket();
				}
			}
		}
	}
}
