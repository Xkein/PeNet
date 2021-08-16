using System;
using PeNet.FileParser;
using PeNet.Header.Pe;

namespace PeNet.HeaderParser.Pe
{
    internal class ImageSectionHeadersParser : SafeParser<ImageSectionHeader[]>
    {
        private readonly ushort _numOfSections;
        private readonly ulong _imageBaseAddress;

        private readonly bool _inProcessMemory;

        internal ImageSectionHeadersParser(IRawFile peFile, uint offset, ushort numOfSections, ulong imageBaseAddress, bool inProcessMemory)
            : base(peFile, offset)
        {
            _numOfSections = numOfSections;
            _imageBaseAddress = imageBaseAddress;

            _inProcessMemory = inProcessMemory;
        }

        protected override ImageSectionHeader[] ParseTarget()
        {
            // Permanence and memory optimization for sorting the section headers
            static int Comparison(ImageSectionHeader x, ImageSectionHeader y)
            {
                if (x.VirtualAddress > y.VirtualAddress)
                    return 1;
                if (x.VirtualAddress < y.VirtualAddress)
                    return -1;

                return 0;
            }

            var sh = new ImageSectionHeader[_numOfSections];
            const uint secSize = 0x28; // Every section header is 40 bytes in size.
            for (uint i = 0; i < _numOfSections; i++)
            {
                sh[i] = new ImageSectionHeader(PeFile, Offset + i*secSize, _imageBaseAddress, _inProcessMemory);
            }

            Array.Sort(sh, Comparison);

            return sh;
        }
    }
}