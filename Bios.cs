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
		int m_sendCountdown;

		const int CyclesPerAccess = 2000; // assumes 1ms at 2.0GHz

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

										// Set up keyboard interrupt handler
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)m_startAddress + 6,				// Set register 0 to address 6 (keyboard ISR address)
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)VMKeyboard.InterruptNo,			// Write keyboard ISR address from register 0 to PIC
										(int)ExecutionUnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)m_startAddress + 10,				// Jump to program start

										// Keyboard interrupt handler
										(int)ExecutionUnitCodes.Load		|	(int)LoadOperations.LoadFromLiteralLocation		|	9 << 8	|	0,	(int)Program.keyboardStartAddress,		// Copy last key pressed into register 9
										(int)ExecutionUnitCodes.Interrupt	|	(int)InterruptInstructions.InterruptReturn		|	0 << 8	|	0,	0,										// Return to execution

										// Write "Hello world"
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	4 << 8	|	0,	26,										// Put string length into register 4
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	0,										// Put desired cursor pos into register 0
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	(int)m_startAddress + 36,				// Put location of start of string into register 1
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	0,										// Put write character code into register 2
										(int)ExecutionUnitCodes.Load		|	(int)LoadOperations.LoadFromRegisterLocation	|	3 << 8	|	1,	0,										// Load the value from the location specified in register 1 into register 3
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.displayStartAddress + 1,	// Set cursor pos
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	3,	(int)Program.displayStartAddress + 2,	// Set character
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	2,	(int)Program.displayStartAddress,		// Write character
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	1,										// Increment string pointer
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	0 << 8	|	0,	1,										// Increment cursor position register
										(int)ExecutionUnitCodes.Branch		|	(int)BranchOperations.JumpNotEqual				|	0 << 8	|	4,	(int)m_startAddress + 18,				// Loop if not at string end
										(int)ExecutionUnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)DisplayCommands.Newline,			// Set register 0 to "newline" command
										(int)ExecutionUnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.displayStartAddress,		// Send newline command to display
										
										// Data section
										// "hello what's your name?"
										0x00000068,
										0x00000065,
										0x0000006c,
										0x0000006c,
										0x0000006f,
										0x0000002c,
										0x00000020,
										0x00000077,
										0x00000068,
										0x00000061,
										0x00000074,
										0x00000027,
										0x00000073,
										0x00000020,
										0x00000079,
										0x0000006f,
										0x00000075,
										0x00000072,
										0x00000020,
										0x0000006e,
										0x00000061,
										0x0000006d,
										0x00000065,
										0x0000003f,
			};
		}

		public void Tick()
		{
			if (m_systenInterconnect.HasPacket)
			{
				int[] packet = new int[3];
				m_systenInterconnect.ReadRecievedPacket(packet);
				m_systenInterconnect.ClearRecievedPacket();

				if(packet[0] == (int)MessageType.Read && !m_sending)
				{
					uint localAddress = (uint)packet[1] - m_startAddress;
					int readLength = packet[2];

					m_sending = true;
					m_sendData = new int[readLength + 1];

					if (localAddress < m_biosData.Count() - (readLength - 1))
					{
						for (int i = 0; i < readLength; i++)
						{
							m_sendData[i + 1] = m_biosData[localAddress + i];
						}
					}
					m_sendData[0] = (int)MessageType.Response;

					m_sendCountdown = CyclesPerAccess;
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
					bool sent = m_systenInterconnect.SendPacket(m_sendData, m_sendData.Count());
					if (sent)
					{
						m_sending = false;
					}
				}
			}
		}
	}
}
