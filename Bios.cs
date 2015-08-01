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
			m_biosData = new int[] {	(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.SetLiteral				|	0,	0,										// Put desired cursor pos into register 0
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.SetLiteral				|	1,	0x68,									// Put character "h" into register 1
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.SetLiteral				|	2,	0,										// Put write character code into register 2
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	0,	(int)Program.displayStartAddress + 1,	// Set cursor pos
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	1,	(int)Program.displayStartAddress + 2,	// Set character
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	2,	(int)Program.displayStartAddress,		// Write character
										
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.AddLiteral				|	0,	1,										// Increment cursor pos register
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.SetLiteral				|	1,	0x65,									// Put character "e" into register 1
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	0,	(int)Program.displayStartAddress + 1,	// Set cursor pos
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	1,	(int)Program.displayStartAddress + 2,	// Set character
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	2,	(int)Program.displayStartAddress,		// Write character
										
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.AddLiteral				|	0,	1,										// Increment cursor pos register
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.SetLiteral				|	1,	0x6c,									// Put character "l" into register 1
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	0,	(int)Program.displayStartAddress + 1,	// Set cursor pos
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	1,	(int)Program.displayStartAddress + 2,	// Set character
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	2,	(int)Program.displayStartAddress,		// Write character
										
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.AddLiteral				|	0,	1,										// Increment cursor pos register
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.SetLiteral				|	1,	0x6c,									// Put character "l" into register 1
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	0,	(int)Program.displayStartAddress + 1,	// Set cursor pos
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	1,	(int)Program.displayStartAddress + 2,	// Set character
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	2,	(int)Program.displayStartAddress,		// Write character
										
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.AddLiteral				|	0,	1,										// Increment cursor pos register
										(int)ExecutionUnitCodes.SimpleALU	|	(int)ALUOperations.SetLiteral				|	1,	0x6f,									// Put character "o" into register 1
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	0,	(int)Program.displayStartAddress + 1,	// Set cursor pos
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	1,	(int)Program.displayStartAddress + 2,	// Set character
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation	|	2,	(int)Program.displayStartAddress,		// Write character
			};
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
