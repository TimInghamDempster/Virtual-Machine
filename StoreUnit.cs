using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum StoreOperations
    {
        StoreToRegisterLocation,
        StoreToLiteralLocation
    }

    class StoreUnit
    {
        CPUCore m_CPUCore;
        CoreMemeoryController m_memoryController;

        int[] m_currentInstruction;
        bool m_hasInstruction;

        public StoreUnit(CPUCore cPUCore, CoreMemeoryController memoryController)
        {
            m_CPUCore = cPUCore;
            m_memoryController = memoryController;
        }
        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.Execution && m_hasInstruction == true)
            {
                StoreOperations operation = (StoreOperations)(m_currentInstruction[0] & 0xff);
                switch(operation)
                {
                    case StoreOperations.StoreToRegisterLocation:
                        if (m_memoryController.m_readyToStore)
                        {
                            m_memoryController.StoreValue(m_CPUCore.m_registers[m_currentInstruction[1]], (uint)m_CPUCore.m_registers[m_currentInstruction[2]]);
                            m_CPUCore.m_currentStage = PipelineStages.BranchPredict;
                        }
                        break;
                    case StoreOperations.StoreToLiteralLocation:
                        if (m_memoryController.m_readyToStore)
                        {
                            m_memoryController.StoreValue(m_currentInstruction[1], (uint)m_CPUCore.m_registers[m_currentInstruction[2]]);
                            m_CPUCore.m_currentStage = PipelineStages.BranchPredict;
                        }
                        break;
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
