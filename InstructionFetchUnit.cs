using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	enum InterruptPhase
	{
		RequestId,
		WaitForId,
		RequestInterruptPointer,
		WaitForInterruptPointer
	}

    class InstructionFetchUnit
    {
        CPUCore m_CPUCore;
        InstructionDispatchUnit m_dispatchUnit;
        InterconnectTerminal m_IOInterconnect;

        bool m_waitingForMemory;
		bool m_startInterrupt;

		InterruptPhase m_interruptPhase;
		int m_interruptId;

		Action EndInterrupt;

        public InstructionFetchUnit(CPUCore cPUCore, InterconnectTerminal IOInterconnect, InstructionDispatchUnit dispatchUnit, Action endInterrupt)
        {
            m_CPUCore = cPUCore;
            m_IOInterconnect = IOInterconnect;
            m_dispatchUnit = dispatchUnit;
            m_waitingForMemory = false;
			EndInterrupt = endInterrupt;
        }

        public void Tick()
        {
			if(!m_startInterrupt)
			{
				if (m_CPUCore.CurrentStage == PipelineStages.InstructionFetch)
				{                
					if (m_waitingForMemory == false)
					{
						int[] newPacket = new int[3];
						newPacket[0] = (int)MessageType.Read;
						newPacket[1] = (int)m_CPUCore.InstructionPointer;
						newPacket[2] = 2;
                    
						bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

						if(requestSent)
						{
							m_waitingForMemory = true;
						}
					}
					else
					{
						if(m_IOInterconnect.HasPacket)
						{
							int[] receivedPacket = new int[m_IOInterconnect.RecievedSize];
							m_IOInterconnect.ReadRecievedPacket(receivedPacket);

							m_IOInterconnect.ClearRecievedPacket();

							if ((receivedPacket[1] & 0xff000000) == (int)UnitCodes.Interrupt
								&& (receivedPacket[1] & 0x00ff0000) == (int)InterruptInstructions.InterruptReturn)
							{
								EndInterrupt();
								m_waitingForMemory = false;
							}
							else
							{
								int[] m_instruction = new int[] { receivedPacket[1], receivedPacket[2] };
								m_dispatchUnit.SetInstruction(m_instruction);
								m_waitingForMemory = false;
								m_CPUCore.NextStage = PipelineStages.InstructionDispatch;
							}
						}
						else
						{
							Program.Counters.FetchWaits++;
						}
					}
				}
			}
			else
			{
				switch(m_interruptPhase)
				{
					case InterruptPhase.RequestId:
					{
						int[] newPacket = new int[3];
						newPacket[0] = (int)MessageType.Read;
						newPacket[1] = (int)InterruptController.InterruptRegisterAddress;
						newPacket[2] = 1;
                    
						bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

						if(requestSent)
						{
							m_interruptPhase = InterruptPhase.WaitForId;
						}
						else
						{
							Program.Counters.InterruptWaits++;
						}
					}break;
					case InterruptPhase.WaitForId:
					{
						if (m_IOInterconnect.HasPacket)
						{
							int[] receivedPacket = new int[m_IOInterconnect.RecievedSize];
							m_IOInterconnect.ReadRecievedPacket(receivedPacket);

							m_IOInterconnect.ClearRecievedPacket();
							m_interruptId = receivedPacket[1];
							m_interruptPhase = InterruptPhase.RequestInterruptPointer;
						}
						else
						{
							Program.Counters.InterruptWaits++;
						}
					}break;
					case InterruptPhase.RequestInterruptPointer:
					{
						int[] newPacket = new int[3];
						newPacket[0] = (int)MessageType.Read;
						newPacket[1] = m_interruptId;
						newPacket[2] = 1;

						bool requestSent = m_IOInterconnect.SendPacket(newPacket, newPacket.Count());

						if (requestSent)
						{
							m_interruptPhase = InterruptPhase.WaitForInterruptPointer;
						}
						else
						{
							Program.Counters.InterruptWaits++;
						}
					}break;
					case InterruptPhase.WaitForInterruptPointer:
					{
						if (m_IOInterconnect.HasPacket)
						{
							int[] receivedPacket = new int[m_IOInterconnect.RecievedSize];
							m_IOInterconnect.ReadRecievedPacket(receivedPacket);

							m_IOInterconnect.ClearRecievedPacket();
							int handlerLocation = receivedPacket[1];

							int[] newInstruction = new int[2];
							newInstruction[0] = (int)UnitCodes.Branch | (int)BranchOperations.Jump;
							newInstruction[1] = handlerLocation;

							m_dispatchUnit.SetInstruction(newInstruction);
							m_CPUCore.NextStage = PipelineStages.InstructionDispatch;

							m_interruptPhase = InterruptPhase.RequestId;
							m_startInterrupt = false;
						}
						else
						{
							Program.Counters.InterruptWaits++;
						}
					}break;
				}
			}
        }

		internal void DoInterrupt()
		{
			m_interruptPhase = InterruptPhase.RequestId;
			m_startInterrupt = true;
		}
	}
}
