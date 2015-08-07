using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	enum LoadOperations
	{
		Nop,
		LoadFromRegisterLocation = 1 << 8
	}

	class LoadUnit
	{
		CPUCore m_CPUCore;
		InterconnectTerminal m_IOInterconnect;

		int[] m_currentInstruction;
		bool m_hasInstruction;
		bool m_waitingForMemory;

		public LoadUnit(CPUCore cPUCore, InterconnectTerminal IOInterconnect)
		{
			m_CPUCore = cPUCore;
			m_IOInterconnect = IOInterconnect;
		}

		public void Tick()
		{
			if (m_CPUCore.m_currentStage == PipelineStages.Execution && m_hasInstruction == true)
			{
				switch((LoadOperations)(m_currentInstruction[0] & 0x0000ff00))
				{
					case LoadOperations.LoadFromRegisterLocation:
					{
						if (m_waitingForMemory == false)
						{
							int[] newPacket = new int[3];
							newPacket[0] = (int)m_CPUCore.m_registers[m_currentInstruction[1]];
							newPacket[1] = 1;
							newPacket[2] = (int)ExecutionUnitCodes.Load;

							bool requestSent = m_IOInterconnect.SendPacket(newPacket, 3);

							if (requestSent)
							{
								m_waitingForMemory = true;
							}
						}
						else
						{
							if (m_IOInterconnect.HasPacket)
							{
								int[] recivedPacket = new int[m_IOInterconnect.RecievedSize];
								m_IOInterconnect.ReadRecievedPacket(recivedPacket);

								if (recivedPacket[m_IOInterconnect.RecievedSize - 1]  == (int)ExecutionUnitCodes.Load)
								{
									m_IOInterconnect.ClearRecievedPacket();
									m_waitingForMemory = false;
									m_hasInstruction = false;

									m_CPUCore.m_registers[m_currentInstruction[0] & 0x000000ff] = recivedPacket[0];

									m_CPUCore.m_nextStage = PipelineStages.BranchPredict;
								}
							}
						}
					}break;
				}
			}
		}

		public void SetInstruction(int[] instruction)
		{
			m_hasInstruction = true;
			m_currentInstruction = instruction;
		}
	}
}
