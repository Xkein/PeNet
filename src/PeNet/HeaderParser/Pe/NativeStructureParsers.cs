using PeNet.FileParser;
using PeNet.Header.Pe;

namespace PeNet.HeaderParser.Pe
{
    internal class NativeStructureParsers
    {
        private readonly IRawFile _peFile;
        private readonly ImageDosHeaderParser _imageDosHeaderParser;
        private readonly ImageNtHeadersParser? _imageNtHeadersParser;
        private ImageSectionHeadersParser? _imageSectionHeadersParser;

        private readonly bool _inProcessMemory;

        internal NativeStructureParsers(IRawFile peFile, bool inProcessMemory)
        {
            _peFile = peFile;
            _inProcessMemory = inProcessMemory;

            // Init all parsers
            _imageDosHeaderParser = InitImageDosHeaderParser();
            _imageNtHeadersParser = InitNtHeadersParser();
            _imageSectionHeadersParser = InitImageSectionHeadersParser();
        }

        public ImageDosHeader? ImageDosHeader => _imageDosHeaderParser.GetParserTarget();
        public ImageNtHeaders? ImageNtHeaders => _imageNtHeadersParser?.GetParserTarget();
        public ImageSectionHeader[]? ImageSectionHeaders => _imageSectionHeadersParser?.GetParserTarget();

        private ImageSectionHeadersParser? InitImageSectionHeadersParser()
        {
            uint GetSecHeaderOffset()
            {
                var x = (uint)ImageNtHeaders!.FileHeader.SizeOfOptionalHeader + 0x18;
                return ImageDosHeader!.E_lfanew + x;
            }

            if (ImageNtHeaders is null || ImageDosHeader is null)
                return null;

            return new ImageSectionHeadersParser(
                _peFile, GetSecHeaderOffset(), 
                ImageNtHeaders.FileHeader.NumberOfSections, 
                ImageNtHeaders.OptionalHeader.ImageBase,
                _inProcessMemory
                );
        }

        internal void ReparseSectionHeaders()
        {
            _imageSectionHeadersParser = InitImageSectionHeadersParser();
        }

        private ImageNtHeadersParser? InitNtHeadersParser()
        {
            if (ImageDosHeader is null)
                return null;

            return new ImageNtHeadersParser(_peFile, ImageDosHeader.E_lfanew);
        }

        private ImageDosHeaderParser InitImageDosHeaderParser()
        {
            return new ImageDosHeaderParser(_peFile, 0);
        }
    }
}