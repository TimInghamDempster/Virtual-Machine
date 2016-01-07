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
		InterconnectTerminal m_systemInterconnect;

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
			const int dataSectionStart = 176;
			m_systemInterconnect = systemInterconnect;
			m_startAddress = startAddress;
			m_sending = false;
			m_biosData = new int[] {
										// Instructions

										// Set up keyboard interrupt handler
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)m_startAddress + 10,				// Set register 0 to address 6 (keyboard ISR address)
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)VMKeyboard.InterruptNo,			// Write keyboard ISR address from register 0 to PIC
										// Set up block device interrupt handler
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)m_startAddress + 14,				// Set register 0 to address 6 (keyboard ISR address)
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)BlockDevice.InterruptNo,			// Write block device ISR address from register 0 to PIC
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)m_startAddress + 38,				// Jump to program start

										// Keyboard interrupt handler
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	15 << 8	|	0,	(int)Program.keyboardStartAddress,		// Copy last key pressed into register 9
										(int)UnitCodes.Interrupt	|	(int)InterruptInstructions.InterruptReturn		|	0 << 8	|	0,	0,										// Return to execution

										// Block device interrupt handler
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	15 << 8	|	0,	(int)Program.SSDInterruptAcknowledgeAddress,// Acknowledge so device stops skwaking
										(int)UnitCodes.Interrupt	|	(int)InterruptInstructions.InterruptReturn		|	0 << 8	|	0,	0,										// Return to execution


										// Write string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	4 << 8	|	0,	0,										// Put loop pos into register 1
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromRegisterLocation	|	5 << 8	|	2,	0,										// Load next char into register 2
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	1 << 8	|	5,	(int)Program.displayStartAddress,		// Store char from register 2 to (display + register 1)
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	1,										// Increment cursor pos
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	1,										// Increment string pos
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	4 << 8	|	4,	1,										// Increment loop pos
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpLess					|	4 << 8	|	0,	(int)Program.biosStartAddress + 20,		// Loop if not written enough characters
										// Flush Screen
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)DisplayCommands.Refresh,			// Set the screen refresh command into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.displayCommandAddress,		// Write refresh command to display command buffer.
										// Jump back
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpRegister				|	0 << 8	|	3,	0,										// Jumpt to location passed in r3
										
										// Draw hello string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	24,										// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	0,										// Put desired cursor pos into register 1
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	(int)Program.biosStartAddress + dataSectionStart,// Put desired string pos into register
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 48,		// Set return pointer
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 18,		// Jumpt to string writing function

										// Handle keyboard input
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	0,										//  Setup Char counter
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	13,										// Add newline char to r2 for comparison
										(int)UnitCodes.ALU			|	(int)ALUOperations.Add							|	10 << 8	|	10,	0,										// NOp to stop caching effect on single instruction loop
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpEqual					|	15 << 8	|	14,	(int)Program.biosStartAddress + 52,		// Loop until key pressed
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpEqual					|	15 << 8	|	2,	(int)Program.biosStartAddress + 74,		// Break if enter pressed
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	0 << 8	|	15,	(int)Program.displayStartAddress + 79,	// Store char to second line of display
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	(int)DisplayCommands.Refresh,			// Set the screen refresh command into register 1
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	1,	(int)Program.displayCommandAddress,		// Write refresh command to display command buffer.
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	0 << 8	|	0,	1,										// Increment char counter
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.RAMStartAddress,			// Store string length to memory
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	0 << 8	|	15,	(int)Program.RAMStartAddress,			// Store character at end of string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	15 << 8	|	0,	0,										// Re-set character register
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 0	|	0,	(int)Program.biosStartAddress + 52,		// Loop back and wait for next character										
										
										// Draw response string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	21,										// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	158,									// Put desired cursor pos into register 1
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	(int)Program.biosStartAddress + dataSectionStart + 24,	// Put desired string pos into register
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 84,		// Set return pointer
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 18,		// Jumpt to string writing function

										// Draw name string.  Dynamic string drawing based on user input, shiny!
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	0 << 8	|	0,	(int)Program.RAMStartAddress,			// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	180,									// Put desired cursor pos into register 1
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	(int)Program.RAMStartAddress + 1,		// Put desired string pos into register
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 94,		// Set return pointer
										//(int)UnitCodes.Branch|(int)BranchOperations.Break,0,
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 18,		// Jumpt to string writing function

										

										// Draw second query string
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	5,										// Put string length into register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	237,									// Put desired cursor pos into register 1
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	(int)Program.biosStartAddress + dataSectionStart + 45,	// Put desired string pos into register
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 104,	// Set return pointer
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 18,		// Jumpt to string writing function
										
										//(int)UnitCodes.Branch		|	(int)BranchOperations.Break						|	0 << 8	|	0,	0,										// Break to gather stats

										// Load name from ssd
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	0,										// Set the block we want into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.SSDSeekAddress,			// Set storage to address in register 0
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	1 << 8	|	0,	(int)Program.RAMStartAddress,			// Load existing space used in memory
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	1,										// Move to next free slot in memory
										
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	2,	0,										// Initialise register 2 as length counter
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	3 << 8	|	0,	(int)Program.SSDFIFOAddress,			// Load a value from SSD into register 3
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	1 << 8	|	3,	(int)Program.RAMStartAddress,			// Store value to location in RAM
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	1,										// Increment RAM pointer
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	1,										// Increment string length
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpNotEqual				|	0 << 8	|	3,	(int)Program.biosStartAddress + 114,	// Jump back for the next character if current one not null

										// Write second name to display
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	0 << 8	|	2,	0,										// Copy string length from register 2 to register 0
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	1 << 8	|	0,	243,									// Set desired cursor pos
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	2 << 8	|	0,	(int)Program.RAMStartAddress,			// Set string pos to end of first string in ram
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	1,										// Move string pos to start of second string in ram
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	(int)Program.RAMStartAddress,			// Add location of RAM to string start address
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	3 << 8	|	0,	(int)Program.biosStartAddress + 138,	// Set return pointer
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress + 18,		// Jumpt to string writing function

										// Write question mark
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	0x0000003f,								// Set register 1 to "?"
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	1 << 8	|	1,	-1,										// Backspace the null character we accidentally wrote
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToRegisterLocation	|	1 << 8	|	2, (int)Program.displayStartAddress,		// Write "?" to screen
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	(int)DisplayCommands.Refresh,			// Set the screen refresh command into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.displayCommandAddress,		// Write refresh command to display command buffer.
										
										// Write current name to ssd
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	0 << 8	|	0,	0,										// Set the block we want into register 0
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.SSDSeekAddress,			// Flush the fifo
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	4 << 8	|	0,	0,										// Initialise char counter
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromLiteralLocation		|	1 << 8	|	0,	(int)Program.RAMStartAddress,			// Find how many chars to write
										(int)UnitCodes.ALU			|	(int)ALUOperations.SetLiteral					|	2 << 8	|	0,	1,										// Set start pointer into register 2
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	(int)Program.RAMStartAddress,			// Add RAM address to char pointer
										(int)UnitCodes.Load			|	(int)LoadOperations.LoadFromRegisterLocation	|	3 << 8	|	2,	0,										// Load char from RAM
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	3,	(int)Program.SSDFIFOAddress,			// Store char to fifo
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	2 << 8	|	2,	1,										// Increment char pointer
										(int)UnitCodes.ALU			|	(int)ALUOperations.AddLiteral					|	4 << 8	|	4,	1,										// Increment char count
										(int)UnitCodes.Branch		|	(int)BranchOperations.JumpLess					|	4 << 8	|	1,	(int)Program.biosStartAddress + 160,	// Loop back for next char
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.SSDFIFOAddress,			// Send a terminating null
										(int)UnitCodes.Store		|	(int)StoreOperations.StoreToLiteralLocation		|	0 << 8	|	0,	(int)Program.SSDSeekAddress,			// Flush the block
										(int)UnitCodes.Branch		|	(int)BranchOperations.Jump						|	0 << 8	|	0,	(int)Program.biosStartAddress +174,		// Self-loop
										
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

										// "how's"
										0x00000068,
										0x0000006f,
										0x00000077,
										0x00000027,
										0x00000073,
			};
		}

		public void Tick()
		{
			if (m_systemInterconnect.HasPacket)
			{
				int[] packet = new int[3];
				m_systemInterconnect.ReadRecievedPacket(packet);

				if(packet[0] == (int)MessageType.Read)
				{

					if(!m_sending)
					{
						m_systemInterconnect.ClearRecievedPacket();
						uint localAddress = (uint)packet[1] - m_startAddress;
						int readLength = packet[2];

						m_sending = true;
						m_sendData = new int[readLength + 2];

						if (localAddress < m_biosData.Count() - (readLength - 1))
						{
							for (int i = 0; i < readLength; i++)
							{
								m_sendData[i + 2] = m_biosData[localAddress + i];
							}
						}
						m_sendData[0] = (int)MessageType.Response;
						m_sendData[1] = packet[1];

						m_sendCountdown = CyclesPerAccess;
					}
				}
				else
				{
					m_systemInterconnect.ClearRecievedPacket();
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
					bool sent = m_systemInterconnect.SendPacket(m_sendData, m_sendData.Count());
					if (sent)
					{
						m_sending = false;
					}
				}
			}
		}
	}
}
