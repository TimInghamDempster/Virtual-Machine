using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class Bios
    {
        uint m_startAddress;
        int[] m_biosData;
        InterconnectTerminal m_systenInterconnect;

        bool m_sending;
        int[] m_sendData;

        public uint Size
        {
            get
            {
                return (uint)m_biosData.Count();
            }
        }

        public Bios(uint startAddress, InterconnectTerminal systemInterconnect)
        {
            m_systenInterconnect = systemInterconnect;
            m_startAddress = startAddress;
            m_sending = false;
            m_biosData = new int[] {    ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.SetLiteral << 8) | 0,                       (int)Program.displayStartAddress,    // Put display address into a register
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.AddLiteral << 8) | 0,                       1,                                   // Move past control byte of display
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.SetLiteral << 8) | 1,                       0x68656c6c,                          // Add the four characters "hell" to register
                                        ((int)ExecutionUnitCodes.Store << 16) | ((int)StoreOperations.StoreToRegisterLocation << 8) | 1,            0,                                   // Move characters from register to display (address stored in another register)
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.AddLiteral << 8) | 0,                       4,                                   // Move display address past four characters just added
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.SetLiteral << 8) | 1,                       0x6f000000,                          // Add the character "o" to a register
                                        ((int)ExecutionUnitCodes.Store << 16) |  ((int)StoreOperations.StoreToRegisterLocation << 8) | 1,           0};                                  // Move character to display
        }

        public void Tick()
        {
            if (m_systenInterconnect.HasPacket)
            {
                int[] packet = new int[2];
                m_systenInterconnect.ReadRecievedPacket(packet);
                m_systenInterconnect.ClearRecievedPacket();

                uint localAddress = (uint)packet[0] - m_startAddress;
                int readLength = packet[1];

                if (localAddress < m_biosData.Count() - (readLength - 1))
                {
                    m_sending = true;
                    m_sendData = new int[readLength];
                    for (int i = 0; i < readLength; i++)
                    {
                        m_sendData[i] = m_biosData[localAddress + i];
                    }
                }
            }

            if (m_sending)
            {
                bool sent = m_systenInterconnect.SendPacket(m_sendData, m_sendData.Count());
                if (sent)
                {
                    m_sending = false;
                }
            }
        }
    }
}
