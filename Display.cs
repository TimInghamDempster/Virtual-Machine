﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class Display
    {
        InterconnectTerminal m_systemInterconnect;

        uint m_startAddress;
        const int m_lineLength = 64;
        char[] m_currentLine;
        bool m_newline = false;
        bool m_refreshed = false;

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
            }


            if (m_newline)
            {
                Console.WriteLine();
                m_newline = false;
            }

            if (m_newline || m_refreshed)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write(m_currentLine);
                m_refreshed = false;
            }
        }

        /*public int Read(uint address)
        {
            uint localAddress = address - m_startAddress;
            
            if (localAddress < m_currentLine.Count() && localAddress > 0)
            {
                localAddress--;
                int retval = 0;

                retval += m_currentLine[localAddress + 0];
                retval = retval << 8;
                retval += m_currentLine[localAddress + 1];
                retval = retval << 8;
                retval += m_currentLine[localAddress + 2];
                retval = retval << 8;
                retval += m_currentLine[localAddress + 3];

                return retval;
            }
            else
            {
                return -1;
            }
        }

        public void Write(int value, uint address)
        {
            m_refreshed = true;
            uint localAddress = address - m_startAddress;
            if (localAddress == 0)
            {
                if (value != 0)
                {
                    m_newline = true;
                }
            }
            else if (localAddress < m_currentLine.Count() && localAddress > 0)
            {
                localAddress--;
                m_currentLine[localAddress + 3] = (char)(value & 0xff);
                value = value >> 8;
                m_currentLine[localAddress + 2] = (char)(value & 0xff);
                value = value >> 8;
                m_currentLine[localAddress + 1] = (char)(value & 0xff);
                value = value >> 8;
                m_currentLine[localAddress + 0] = (char)(value & 0xff);
                value = value >> 8;
            }
        }*/
    }
}
