using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class BranchUnit
    {
        CPUCore m_CPUCore;

        public BranchUnit(CPUCore cPUCore)
        {
            m_CPUCore = cPUCore;
        }
        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.BranchPredict)
            {
                m_CPUCore.m_instructionPointer += 12;
                m_CPUCore.m_currentStage = PipelineStages.InstructionFetch;
            }
        }
    }
}
