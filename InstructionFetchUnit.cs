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
        CoreMemeoryController m_memoryController;
        InstructionDispatchUnit m_dispatchUnit;
        bool m_requestMade;

        public InstructionFetchUnit(CPUCore cPUCore, CoreMemeoryController memoryController, InstructionDispatchUnit dispatchUnit)
        {
            m_CPUCore = cPUCore;
            m_memoryController = memoryController;
            m_dispatchUnit = dispatchUnit;
        }

        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.InstructionFetch)
            {
                if (!m_requestMade)
                {
                    m_memoryController.RequestInstruction(m_CPUCore.m_instructionPointer);
                    m_requestMade = true;
                }
                else
                {
                    if (m_memoryController.m_instructionReady)
                    {
                        m_requestMade = false;
                        m_dispatchUnit.SetInstruction(m_memoryController.GetInstruction());
                        m_CPUCore.m_currentStage = PipelineStages.InstructionDispatch;
                    }
                }
            }
        }
    }
}
