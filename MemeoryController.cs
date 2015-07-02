using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class MemeoryController
    {
        Bios m_bios;
        Display m_display;

        //public bool m_instructionReady;
        public bool m_readyToStore;

        /*bool m_instructionRequestPending;
        uint m_instructionFetchAddress;
        int[] lastInstruction;*/

        int[] m_data;
        uint[] m_addresses;
        int[] m_requestTimers;
        bool[] m_requested;
        bool[] m_ready;

        public bool[] Ready
        {
            get
            {
                return m_ready;
            }
        }

        public MemeoryController(Bios bios, Display display)
        {
            m_bios = bios;
            m_display = display;
            m_readyToStore = true;

            m_data = new int[8];
            m_addresses = new uint[4];
            m_requestTimers = new int[4];
            m_requested = new bool[4];
            m_ready = new bool[4];
        }

        public int Request(uint address)
        {
            for (int i = 0; i < m_requested.Count(); i++)
            {
                if (!m_requested[i])
                {
                    m_addresses[i] = address;
                    m_requested[i] = true;
                    m_ready[i] = false;
                    m_requestTimers[i] = 200;
                    return i;
                }
            }
            return -1;
        }

        public void Read(int channel, out int data1, out int data2)
        {
            m_requested[channel] = false;
            m_ready[channel] = false;
            data1 = m_data[channel * 2];
            data2 = m_data[channel * 2 + 1];
        }

        /*public void RequestInstruction(uint address)
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
        }*/

        public void StoreValue(int value, uint address)
        {
            m_display.Write(value, address);
        }

        public void Tick()
        {
            for (int i = 0; i < m_requestTimers.Count(); i++)
            {
                if (m_requested[i] == true && m_ready[i] == false)
                {
                    if (m_requestTimers[i] > 0)
                    {
                        m_requestTimers[i]--;
                    }
                    else
                    {
                        m_ready[i] = true;
                        m_data[i * 2] = m_bios.Read(m_addresses[i]);
                        m_data[i * 2 + 1] = m_bios.Read(m_addresses[i] + 1);
                    }
                }
            }
            /*
            if (m_instructionRequestPending)
            {
                lastInstruction[0] = m_nothbridge.Read(m_instructionFetchAddress);
                lastInstruction[1] = m_nothbridge.Read(m_instructionFetchAddress + 4);
                lastInstruction[2] = m_nothbridge.Read(m_instructionFetchAddress + 8);
                m_instructionReady = true;
            }*/
        }
    }
}
