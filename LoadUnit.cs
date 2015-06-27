using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum LoadOperations
    {

    }

    class LoadUnit
    {
        CPUCore m_CPUCore;

        int[] m_currentInstruction;
        bool m_hasInstruction;
        CoreMemeoryController m_memoryController;

        public LoadUnit(CPUCore cPUCore)
        {
            m_CPUCore = cPUCore;
        }
        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.Execution && m_hasInstruction == true)
            {
            }
        }

        public void SetInstruction(int[] instruction)
        {
            m_hasInstruction = true;
            m_currentInstruction = instruction;
        }
    }
}
