using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class CoreMemeoryController
    {
        IIODevice m_nothbridge;

        public bool m_instructionReady;
        public bool m_readyToStore;

        bool m_instructionRequestPending;
        uint m_instructionFetchAddress;
        int[] lastInstruction;
        

        public CoreMemeoryController(IIODevice northbridge)
        {
            m_nothbridge = northbridge;
            lastInstruction = new int[3];
            m_readyToStore = true;
        }

        public void RequestInstruction(uint address)
        {
            m_instructionReady = false;
            m_instructionRequestPending = true;
            m_instructionFetchAddress = address;
        }

        public int[] GetInstruction()
        {
            if (m_instructionReady == true)
            {
                m_instructionReady = false;
                m_instructionRequestPending = false;
                return lastInstruction;
            }
            return null;
        }

        public void StoreValue(int value, uint address)
        {
            m_nothbridge.Write(value, address);
        }

        public void Tick()
        {
            if (m_instructionRequestPending)
            {
                lastInstruction[0] = m_nothbridge.Read(m_instructionFetchAddress);
                lastInstruction[1] = m_nothbridge.Read(m_instructionFetchAddress + 4);
                lastInstruction[2] = m_nothbridge.Read(m_instructionFetchAddress + 8);
                m_instructionReady = true;
            }
        }
    }
}
