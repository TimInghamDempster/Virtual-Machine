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
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)m_startAddress + 30,				// Jump to program start

										// Keyboard interrupt handler
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	15 << 8	|	0,	(int)Program.keyboardStartAddress,		// Copy last key pressed into register 9
										(int)UnitCodes.Interrupt	|	(int)InterruptInstructions.InterruptReturn		|	0 << 8	|	0,	0,										// Return to execution

										// Write string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	4 << 8	|	0,	0,										// Put loop pos into register 1
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromRegisterLocation	|	5 << 8	|	2,	0,										// Load next char into register 2
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	1 << 8	|	5,	(int)Program.displayStartAddress,		// Store char from register 2 to (display + register 1)
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	1,										// Increment cursor pos
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	1,										// Increment string pos
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	4 << 8	|	4,	1,										// Increment loop pos
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpLess					|	4 << 8	|	0,	(int)Program.biosStartAddress + 12,		// Loop if not written enough characters
										// Flush Screen
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)DisplayCommands.Refresh,			// Set the screen refresh command into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.displayCommandAddress,		// Write refresh command to display command buffer.
										// Jump back
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpRegister				|	0 << 8	|	3,	0,										// Jumpt to location passed in r3
										
										// Draw hello string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	24,										// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	0,										// Put desired cursor pos into register 1
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	(int)Program.biosStartAddress + 84,		// Put desired string pos into register
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 40,		// Set return pointer
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 10,		// Jumpt to string writing function

										// Handle keyboard input
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	0,										//  Setup Char counter
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	13,										// Add newline char to r2 for comparison
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpEqual					|	15 << 8	|	14,	(int)Program.biosStartAddress + 44,		// Break if enter pressed
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpEqual					|	15 << 8	|	2,	(int)Program.biosStartAddress + 64,		// Loop until key pressed
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	0 << 8	|	15,	(int)Program.displayStartAddress + 79,	// Store char to second line of display
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	(int)DisplayCommands.Refresh,			// Set the screen refresh command into register 1
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	1,	(int)Program.displayCommandAddress,		// Write refresh command to display command buffer.
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	0 << 8	|	0,	1,										// Increment char counter
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.RAMStartAddress,			// Store string length to memory
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	0 << 8	|	15,	(int)Program.RAMStartAddress,			// Store character at end of string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	15 << 8	|	0,	0,										// Re-set character register
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 0	|	0,	(int)Program.biosStartAddress + 44,		// Loop back and wait for next character										
										
										// Draw response string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	21,										// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	158,									// Put desired cursor pos into register 1
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	(int)Program.biosStartAddress + 108,	// Put desired string pos into register
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 74,		// Set return pointer
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 10,		// Jumpt to string writing function

										// Draw name string.  Dynamic string drawing based on user input, shiny!
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	0 << 8	|	0,	(int)Program.RAMStartAddress,			// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	180,									// Put desired cursor pos into register 1
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	(int)Program.RAMStartAddress + 1,		// Put desired string pos into register
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 40,		// Set return pointer
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 10,		// Jumpt to string writing function

										// Data section
										// "hello, what's your name?"
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

										// "It's nice to meet you "
										0x00000049,
										0x00000074,
										0x00000027,
										0x00000073,
										0x00000020,

										0x0000006e,
										0x00000069,
										0x00000063,
										0x00000065,
										0x00000020,
										
										0x00000074,
										0x0000006f,
										0x00000020,

										0x0000006d,
										0x00000065,
										0x00000065,
										0x00000074,
										0x00000020,

										0x00000079,
										0x0000006f,
										0x00000075,
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
