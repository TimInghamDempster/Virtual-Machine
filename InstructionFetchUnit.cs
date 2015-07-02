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
        MemeoryController m_memoryController;
        InstructionDispatchUnit m_dispatchUnit;
        int m_memoryControllerRequestChannel;

        public InstructionFetchUnit(CPUCore cPUCore, MemeoryController memoryController, InstructionDispatchUnit dispatchUnit)
        {
            m_CPUCore = cPUCore;
            m_memoryController = memoryController;
            m_dispatchUnit = dispatchUnit;
            m_memoryControllerRequestChannel = -1;
        }

        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.InstructionFetch)
            {
                if (m_memoryControllerRequestChannel == -1)
                {
                    m_memoryControllerRequestChannel = m_memoryController.Request(m_CPUCore.m_instructionPointer);
                }
                else
                {
                    if (m_memoryController.Ready[m_memoryControllerRequestChannel])
                    {
                        int[] instruction = new int[2];
                        m_memoryController.Read(m_memoryControllerRequestChannel, out instruction[0], out instruction[1]);
                        m_dispatchUnit.SetInstruction(instruction);
                        m_CPUCore.m_nextStage = PipelineStages.InstructionDispatch;
                        m_memoryControllerRequestChannel = -1;
                    }
                }
            }
        }
    }
}
