using System.IO;
using System.Text;
using LibHac.IO;

namespace LibHac
{
    public class Nro
    {
        public NroStart Start { get; }
        public NroHeader Header { get; }
        public int HeaderSize { get; }

        private IStorage BaseStorage { get; }
        private AssetHeader Assets { get; }

        public Nro(IStorage storage)
        {
            using (var reader = new BinaryReader(storage.AsStream(), Encoding.Default, true))
            {
                Start = new NroStart(reader);
                Header = new NroHeader(reader);
            }

            HeaderSize = Header.HeaderSize;
            BaseStorage = storage;

            using (var reader = new BinaryReader(storage.Slice(HeaderSize, 56).AsStream(), Encoding.Default, true))
            {
                Assets = new AssetHeader(reader);
            }
        }

        public IStorage OpenIcon()
        {
            return BaseStorage.Slice(HeaderSize + Assets.Sections[0].Offset, Assets.Sections[0].Size);
        }

        public IStorage OpenNacp()
        {
            return BaseStorage.Slice(HeaderSize + Assets.Sections[1].Offset, Assets.Sections[1].Size);
        }

        public IStorage OpenRomfs()
        {
            return BaseStorage.Slice(HeaderSize + Assets.Sections[2].Offset, Assets.Sections[2].Size);
        }
    }

    public class NroStart
    {
        public int Unused;
        public int ModOffset;
        public long Padding;

        public NroStart(BinaryReader reader)
        {
            Unused = reader.ReadInt32();
            ModOffset = reader.ReadInt32();
            Padding = reader.ReadInt64();
        }
    }

    public class NroHeader
    {
        private const string HeaderMagic = "NRO0";

        public string Magic;
        public int FormatVersion;
        public int HeaderSize;
        public int Unused;

        public NroHeader(BinaryReader reader)
        {
            Magic = reader.ReadAscii(4);
            if (Magic != HeaderMagic)
            {
                throw new InvalidDataException("Invalid NRO file: Header magic invalid.");
            }

            FormatVersion = reader.ReadInt32();
            HeaderSize = reader.ReadInt32();
            Unused = reader.ReadInt32();
        }
    }

    public class AssetSection
    {
        public long Offset;
        public long Size;

        public AssetSection(BinaryReader reader)
        {
            Offset = reader.ReadInt64();
            Size = reader.ReadInt64();
        }
    }

    public class AssetHeader
    {
        private const string HeaderMagic = "ASET";

        public string Magic;
        public int FormatVersion;

        public AssetSection[] Sections { get; } = new AssetSection[3];

        public AssetHeader(BinaryReader reader)
        {
            Magic = reader.ReadAscii(4);
            if (Magic != HeaderMagic)
            {
                throw new InvalidDataException("Invalid Asset file: Header magic invalid.");
            }

            FormatVersion = reader.ReadInt32();

            for (int i = 0; i < 3; i++)
            {
                Sections[i] = new AssetSection(reader);
            }
        }
    }
}
