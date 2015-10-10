using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum DisplayCommands
    {
        Refresh,
        Clear
    }

    class Display
    {
        InterconnectTerminal m_systemInterconnect;

        uint m_startAddress;
        int m_lineLength = Console.WindowWidth - 1;
		int m_numlines = Console.WindowHeight - 1;
		uint m_commandAddress;
        char[] m_charData;

        public Display(uint startAddress, InterconnectTerminal systemInterconnect)
        {
            m_startAddress = startAddress;
			m_commandAddress = Program.displayCommandAddress;
            m_charData = new char[m_lineLength * m_numlines];

            m_systemInterconnect = systemInterconnect;
        }

        public void Tick()
        {
            if(m_systemInterconnect.HasPacket)
            {
                int[] packet = new int[m_systemInterconnect.RecievedSize];
                m_systemInterconnect.ReadRecievedPacket(packet);

                m_systemInterconnect.ClearRecievedPacket();

				if(packet[0] == (int)MessageType.Write)
				{
					if (packet[1] < (int)m_commandAddress)
					{
						m_charData[packet[1] - m_startAddress] = (char)packet[2];
					}
					else if (packet[1] == (int)m_commandAddress)
					{
						switch((DisplayCommands)packet[2])
						{
							case DisplayCommands.Clear:
							{
								Array.Clear(m_charData, 0, m_charData.Length);
								Refresh();
							}break;
							case DisplayCommands.Refresh:
							{
								Refresh();	
							}break;
						}
					}
				}
            }
        }

		public void Refresh()
		{
			Console.CursorTop = 0;
			Console.CursorLeft = 0;
			for(int y = 0; y < m_numlines; y++)
			{
				for(int x = 0; x < m_lineLength; x++)
				{
					Console.Write(m_charData[x + y * m_lineLength]);
				}
				Console.Write('\n');
			}
		}
    }
}
