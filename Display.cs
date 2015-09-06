using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum DisplayCommands
    {
        WriteChar,
        Newline
    }

    class Display
    {
        InterconnectTerminal m_systemInterconnect;

        uint m_startAddress;
        const int m_lineLength = 64;
        char[] m_currentLine;
        bool m_newline = false;

        int m_cursorPos;
        char m_currentChar;

		int m_tickCount;

        public Display(uint startAddress, InterconnectTerminal systemInterconnect)
        {
            m_startAddress = startAddress;
            m_currentLine = new char[m_lineLength];

            m_systemInterconnect = systemInterconnect;
        }

        public void Tick()
        {
            if(m_systemInterconnect.HasPacket)
            {
                int[] packet = new int[m_systemInterconnect.RecievedSize];
                m_systemInterconnect.ReadRecievedPacket(packet);

                m_systemInterconnect.ClearRecievedPacket();

                if (packet[0] == (int)m_startAddress)
                {
                    switch ((DisplayCommands)packet[1])
                    {
                        case DisplayCommands.Newline:
                            {
                                m_newline = true;
                            } break;
                        case DisplayCommands.WriteChar:
                            {
                                m_currentLine[m_cursorPos] = m_currentChar;
                            } break;
                    }
                }
                else if (packet[0] == (int)m_startAddress + 1)
                {
                    if (packet[1] > 0 && packet[1] < m_currentLine.Count())
                    {
                        m_cursorPos = packet[1];
                    }
                }
                else if (packet[0] == (int)m_startAddress + 2)
                {
                    m_currentChar = (char)packet[1];
                }
            }

           

			m_tickCount++;

			if((m_tickCount % 100000) == 0)
			{
				Console.SetCursorPosition(0, Console.CursorTop);
				Console.Write(m_currentLine);
				Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

				if (m_newline)
				{
					Console.WriteLine();

					for (int i = 0; i < m_lineLength; i++)
					{
						m_currentLine[i] = ' ';
					}

					m_newline = false;
				}
			}
        }
    }
}
