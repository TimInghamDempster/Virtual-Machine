using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	class RAM
	{

		uint m_startAddress;
		int[] m_Data;
		InterconnectTerminal m_cpuInterconnect;

		bool m_sending;
		int[] m_sendData;
		int m_sendCountdown;

		const int CyclesPerAccess = 200; // assumes 0.1ms at 2.0GHz

		public RAM(InterconnectTerminal CPUInterconnect, int size)
		{
			m_Data = new int[size];
			m_cpuInterconnect = CPUInterconnect;
		}

		public void Tick()
		{
			if (m_cpuInterconnect.HasPacket)
			{
				int[] packet = new int[m_cpuInterconnect.RecievedSize];
				m_cpuInterconnect.ReadRecievedPacket(packet);
				m_cpuInterconnect.ClearRecievedPacket();

				if (packet[0] == (int)MessageType.Read && !m_sending)
				{
					uint localAddress = (uint)packet[1] - m_startAddress;
					int readLength = packet[2];

					m_sending = true;
					m_sendData = new int[readLength + 1];

					if (localAddress < m_Data.Count() - (readLength - 1))
					{
						for (int i = 0; i < readLength; i++)
						{
							m_sendData[i + 1] = m_Data[localAddress + i];
						}
					}
					m_sendData[0] = (int)MessageType.Response;

					m_sendCountdown = CyclesPerAccess;
				}
				else if(packet[0] == (int)MessageType.Write)
				{
					uint localAddress = (uint)packet[1] - m_startAddress;

					if (localAddress < m_Data.Count())
					{
						m_Data[localAddress] = packet[2];
					}
				}
			}

			if (m_sending)
			{
				if (m_sendCountdown > 0)
				{
					m_sendCountdown--;
				}
				else
				{
					bool sent = m_cpuInterconnect.SendPacket(m_sendData, m_sendData.Count());
					if (sent)
					{
						m_sending = false;
					}
				}
			}
		}
	}
}
