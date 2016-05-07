﻿/***********************************************************************
Copyright 2016 Stefan Hausotte

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

*************************************************************************/

using System.Text;

namespace PeNet.Structures
{
    /// <summary>
    ///     The runtime function struct is represents
    ///     a function in the exception header for x64
    ///     applications.
    /// </summary>
    public class RUNTIME_FUNCTION
    {
        private readonly byte[] _buff;
        private readonly uint _offset;

        /// <summary>
        ///     Create a new RUNTIME_FUNCTION object.
        /// </summary>
        /// <param name="buff">A PE file as a byte array.</param>
        /// <param name="offset">Raw offset of the runtime function struct.</param>
        /// <param name="sh">Section Headers of the PE file.</param>
        public RUNTIME_FUNCTION(byte[] buff, uint offset, IMAGE_SECTION_HEADER[] sh)
        {
            _buff = buff;
            _offset = offset;

            ResolvedUnwindInfo = GetUnwindInfo(sh);
        }

        /// <summary>
        ///     Get the UNWIND_INFO from a runtime function form the
        ///     Exception header in x64 applications.
        /// </summary>
        /// <param name="sh">Sectuion Headers of the PE file.</param>
        /// <returns>UNWIND_INFO for the runtime function.</returns>
        private UNWIND_INFO GetUnwindInfo(IMAGE_SECTION_HEADER[] sh)
        {
            // Check if the last bit is set in the UnwindInfo. If so, it is a chained 
            // information.
            var uwAddress = (UnwindInfo & 0x1) == 0x1
                ? UnwindInfo & 0xFFFE
                : UnwindInfo;

            var uw = new UNWIND_INFO(_buff, Utility.RVAtoFileMapping(uwAddress, sh));
            return uw;
        }

        /// <summary>
        ///     RVA Start of the function in code.
        /// </summary>
        public uint FunctionStart
        {
            get { return Utility.BytesToUInt32(_buff, _offset); }
            set { Utility.SetUInt32(value, _offset, _buff); }
        }

        /// <summary>
        ///     RVA End of the function in code.
        /// </summary>
        public uint FunctionEnd
        {
            get { return Utility.BytesToUInt32(_buff, _offset + 0x4); }
            set { Utility.SetUInt32(value, _offset + 0x4, _buff); }
        }

        /// <summary>
        ///     Pointer to the unwind information.
        /// </summary>
        public uint UnwindInfo
        {
            get { return Utility.BytesToUInt32(_buff, _offset + 0x8); }
            set { Utility.SetUInt32(value, _offset + 0x8, _buff); }
        }

        /// <summary>
        /// Unwind Info object belonging to this Runtime Function.
        /// </summary>
        public UNWIND_INFO ResolvedUnwindInfo { get; private set; }

        /// <summary>
        ///     Creates a string representation of the objects
        ///     properties.
        /// </summary>
        /// <returns>The runtime function properties as a string.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("RUNTIME_FUNCTION\n");
            sb.Append(Utility.PropertiesToString(this, "{0,-20}:\t{1,10:X}\n"));
            return sb.ToString();
        }
    }
}