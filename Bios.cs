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
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)m_startAddress + 6,				// Set register 0 to address 6 (keyboard ISR address)
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)VMKeyboard.InterruptNo,			// Write keyboard ISR address from register 0 to PIC
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)m_startAddress + 10,				// Jump to program start

										// Keyboard interrupt handler
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	15 << 8	|	0,	(int)Program.keyboardStartAddress,		// Copy last key pressed into register 9
										(int)UnitCodes.Interrupt	|	(int)InterruptInstructions.InterruptReturn		|	0 << 8	|	0,	0,										// Return to execution

										// Write "Hello world"
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	24,										// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	0,										// Put desired cursor pos into register 1
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromRegisterLocation	|	2 << 8	|	1,	(int)Program.biosStartAddress + 42,		// Load next char into register 2
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	1 << 8	|	2,	(int)Program.displayStartAddress,		// Store char from register 2 to (display + register 1)
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	1,										// Increment cursor/string pos
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpLess					|	1 << 8	|	0,	(int)Program.biosStartAddress + 14,		// Loop if not written enough characters
										// Flush Screen
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)DisplayCommands.Refresh,			// Set the screen refresh command into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.displayCommandAddress,		// Write refresh command to display command buffer.
										
										// Handle keyboard input
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 0	|	0,	0,										//  Setup Char counter
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpEqual					|	15 << 8	|	14, (int)Program.biosStartAddress + 28,		// Loop until key pressed
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	0 << 8	|	15,	(int)Program.displayStartAddress + 79,	// Store char to second line of display
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	(int)DisplayCommands.Refresh,			// Set the screen refresh command into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	1,	(int)Program.displayCommandAddress,		// Write refresh command to display command buffer.
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	0 << 8	|	0,	1,										// Increment char counter
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	15 << 8	|	0,	0,										// Re-set character register
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 0	|	0,	(int)Program.biosStartAddress + 28,		// Loop back and wait for next character

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
