using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class InstructionFetchUnit
    {
        CPUCore m_CPUCore;
        //MemeoryController m_memoryController;
        InstructionDispatchUnit m_dispatchUnit;
        InterconnectTerminal m_IOInterconnect;

        bool m_waitingForMemory;

        public InstructionFetchUnit(CPUCore cPUCore, InterconnectTerminal IOInterconnect, InstructionDispatchUnit dispatchUnit)
        {
            m_CPUCore = cPUCore;
            m_IOInterconnect = IOInterconnect;
            m_dispatchUnit = dispatchUnit;
            m_waitingForMemory = false;
        }

        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.InstructionFetch)
            {                
                if (m_waitingForMemory == false)
                {
                    int[] newPacket = new int[2];
                    newPacket[0] = (int)m_CPUCore.m_instructionPointer;
                    newPacket[1] = 2;
                    bool requestSent = m_IOInterconnect.SendPacket(newPacket, 2);

                    if(requestSent)
                    {
                        m_waitingForMemory = true;
                    }
                }
                else
                {
                    if(m_IOInterconnect.HasPacket)
                    {
                        int[] recivedPacket = new int[2];
                        m_IOInterconnect.ReadRecievedPacket(recivedPacket);
                        m_IOInterconnect.ClearRecievedPacket();
                        m_dispatchUnit.SetInstruction(recivedPacket);
                        m_waitingForMemory = false;
                        m_CPUCore.m_nextStage = PipelineStages.InstructionDispatch;
                    }
                }
            }
        }
    }
}
