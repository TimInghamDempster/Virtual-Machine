using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	class VMKeyboard
	{
		private InterruptController m_interruptController;
		InterconnectTerminal m_systemInterconnect;

		char m_currentKey;
		bool m_hasCurrentKey;

		bool m_isSendingKey;
		int m_returnAddress;

		const uint InterruptNo = 0;

		public VMKeyboard(InterruptController interruptController, InterconnectTerminal systemInterconnect)
		{
			m_interruptController = interruptController;
			m_systemInterconnect = systemInterconnect;
		}

		public void Tick()
		{
			if(Console.KeyAvailable)
			{
				m_currentKey = Console.ReadKey().KeyChar;
				if(m_hasCurrentKey == false)
				{
					m_hasCurrentKey = true;
					m_interruptController.Interrupt(InterruptNo);
				}
			}
			else
			{
				m_hasCurrentKey = false;
			}

			if(m_systemInterconnect.HasPacket)
			{
				int[] buffer = new int[m_systemInterconnect.RecievedSize];
				m_systemInterconnect.ReadRecievedPacket(buffer);
				m_systemInterconnect.ClearRecievedPacket();

				m_returnAddress = buffer[2];
				m_isSendingKey = true;
			}

			if(m_isSendingKey)
			{
				int[] buffer = new int[2];
				buffer[0] = m_returnAddress;
				buffer[1] = m_currentKey;

				bool sent = m_systemInterconnect.SendPacket(buffer, 2);

				if(sent)
				{
					m_isSendingKey = false;
				}
			}
		}
	}
}
