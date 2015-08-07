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
			m_biosData = new int[] {
										// Instructions
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral				|	0,	0,										// Put desired cursor pos into register 0
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral				|	1,	(int)m_startAddress + 20,				// Put location of start of string into register 1
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral				|	2,	0,										// Put write character code into register 2
										(int)ExecutionUnitCodes.Load		|	(int)LoadOperations.LoadFromRegisterLocation|	3,	1,										// Load the value from the location specified in register 1 into register 3
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	0,	(int)Program.displayStartAddress + 1,	// Set cursor pos
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	3,	(int)Program.displayStartAddress + 2,	// Set character
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	2,	(int)Program.displayStartAddress,		// Write character
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.AddLiteral				|	1,	1,										// Increment string pointer
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.AddLiteral				|	0,	1,										// Increment cursor position register
										(int)ExecutionUnitCodes.Branch		|	(int)BranchOperations.Jump,							(int)m_startAddress + 6,				// Loop
										
										// Data section
										0x00000068,
										0x00000065,
										0x0000006c,
										0x0000006c,
										0x0000006f,
										0x00000020,
										0x00000077,
										0x0000006f,
										0x00000072,
										0x0000006c,
										0x00000064,
			};
		}

        public void Tick()
        {
            if (m_systenInterconnect.HasPacket)
            {
                int[] packet = new int[3];
                m_systenInterconnect.ReadRecievedPacket(packet);
                m_systenInterconnect.ClearRecievedPacket();

                uint localAddress = (uint)packet[0] - m_startAddress;
                int readLength = packet[1];

                if (localAddress < m_biosData.Count() - (readLength - 1))
                {
                    m_sending = true;
                    m_sendData = new int[readLength + 1];
                    for (int i = 0; i < readLength; i++)
                    {
                        m_sendData[i] = m_biosData[localAddress + i];
                    }
					m_sendData[m_sendData.Length - 1] = packet[2];
                }
            }

            if (m_sending)
            {
				if(m_sendData[m_sendData.Length - 1] == 0x20000)
				{
					int a = 0;
				}
                bool sent = m_systenInterconnect.SendPacket(m_sendData, m_sendData.Count());
                if (sent)
                {
                    m_sending = false;
                }
            }
        }
    }
}
