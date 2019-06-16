﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using LibHac.IO;

namespace LibHac
{
    public class Keyset
    {
        public byte[][] KeyblobKeys { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x10);
        public byte[][] KeyblobMacKeys { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x10);
        public byte[][] EncryptedKeyblobs { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0xB0);
        public byte[][] Keyblobs { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x90);
        public byte[][] KeyblobKeySources { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x10);
        public byte[] KeyblobMacKeySource { get; } = new byte[0x10];
        public byte[] MasterKeySource { get; } = new byte[0x10];
        public byte[][] MasterKeys { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x10);
        public byte[][] Package1Keys { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x10);
        public byte[][] Package2Keys { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x10);
        public byte[] Package2KeySource { get; } = new byte[0x10];
        public byte[] AesKekGenerationSource { get; } = new byte[0x10];
        public byte[] AesKeyGenerationSource { get; } = new byte[0x10];
        public byte[] KeyAreaKeyApplicationSource { get; } = new byte[0x10];
        public byte[] KeyAreaKeyOceanSource { get; } = new byte[0x10];
        public byte[] KeyAreaKeySystemSource { get; } = new byte[0x10];
        public byte[] SaveMacKekSource { get; } = new byte[0x10];
        public byte[] SaveMacKeySource { get; } = new byte[0x10];
        public byte[] TitlekekSource { get; } = new byte[0x10];
        public byte[] HeaderKekSource { get; } = new byte[0x10];
        public byte[] SdCardKekSource { get; } = new byte[0x10];
        public byte[][] SdCardKeySources { get; } = Util.CreateJaggedArray<byte[][]>(2, 0x20);
        public byte[][] SdCardKeySourcesSpecific { get; } = Util.CreateJaggedArray<byte[][]>(2, 0x20);
        public byte[] HeaderKeySource { get; } = new byte[0x20];
        public byte[] HeaderKey { get; } = new byte[0x20];
        public byte[] XciHeaderKey { get; } = new byte[0x10];
        public byte[][] Titlekeks { get; } = Util.CreateJaggedArray<byte[][]>(0x20, 0x10);
        public byte[][][] KeyAreaKeys { get; } = Util.CreateJaggedArray<byte[][][]>(0x20, 3, 0x10);
        public byte[] SaveMacKey { get; } = new byte[0x10];
        public byte[][] SdCardKeys { get; } = Util.CreateJaggedArray<byte[][]>(2, 0x20);
        public byte[] EticketRsaKek { get; } = new byte[0x10];
        public byte[] RetailSpecificAesKeySource { get; } = new byte[0x10];
        public byte[] PerConsoleKeySource { get; } = new byte[0x10];
        public byte[] BisKekSource { get; } = new byte[0x10];
        public byte[][] BisKeySource { get; } = Util.CreateJaggedArray<byte[][]>(3, 0x20);

        public byte[] SecureBootKey { get; } = new byte[0x10];
        public byte[] TsecKey { get; } = new byte[0x10];
        public byte[] DeviceKey { get; } = new byte[0x10];
        public byte[][] BisKeys { get; } = Util.CreateJaggedArray<byte[][]>(4, 0x20);
        public byte[] SdSeed { get; } = new byte[0x10];

        public RSAParameters EticketExtKeyRsa { get; set; }

        public byte[] NcaHdrFixedKeyModulus { get; } =
        {
            0xBF, 0xBE, 0x40, 0x6C, 0xF4, 0xA7, 0x80, 0xE9, 0xF0, 0x7D, 0x0C, 0x99, 0x61, 0x1D, 0x77, 0x2F,
            0x96, 0xBC, 0x4B, 0x9E, 0x58, 0x38, 0x1B, 0x03, 0xAB, 0xB1, 0x75, 0x49, 0x9F, 0x2B, 0x4D, 0x58,
            0x34, 0xB0, 0x05, 0xA3, 0x75, 0x22, 0xBE, 0x1A, 0x3F, 0x03, 0x73, 0xAC, 0x70, 0x68, 0xD1, 0x16,
            0xB9, 0x04, 0x46, 0x5E, 0xB7, 0x07, 0x91, 0x2F, 0x07, 0x8B, 0x26, 0xDE, 0xF6, 0x00, 0x07, 0xB2,
            0xB4, 0x51, 0xF8, 0x0D, 0x0A, 0x5E, 0x58, 0xAD, 0xEB, 0xBC, 0x9A, 0xD6, 0x49, 0xB9, 0x64, 0xEF,
            0xA7, 0x82, 0xB5, 0xCF, 0x6D, 0x70, 0x13, 0xB0, 0x0F, 0x85, 0xF6, 0xA9, 0x08, 0xAA, 0x4D, 0x67,
            0x66, 0x87, 0xFA, 0x89, 0xFF, 0x75, 0x90, 0x18, 0x1E, 0x6B, 0x3D, 0xE9, 0x8A, 0x68, 0xC9, 0x26,
            0x04, 0xD9, 0x80, 0xCE, 0x3F, 0x5E, 0x92, 0xCE, 0x01, 0xFF, 0x06, 0x3B, 0xF2, 0xC1, 0xA9, 0x0C,
            0xCE, 0x02, 0x6F, 0x16, 0xBC, 0x92, 0x42, 0x0A, 0x41, 0x64, 0xCD, 0x52, 0xB6, 0x34, 0x4D, 0xAE,
            0xC0, 0x2E, 0xDE, 0xA4, 0xDF, 0x27, 0x68, 0x3C, 0xC1, 0xA0, 0x60, 0xAD, 0x43, 0xF3, 0xFC, 0x86,
            0xC1, 0x3E, 0x6C, 0x46, 0xF7, 0x7C, 0x29, 0x9F, 0xFA, 0xFD, 0xF0, 0xE3, 0xCE, 0x64, 0xE7, 0x35,
            0xF2, 0xF6, 0x56, 0x56, 0x6F, 0x6D, 0xF1, 0xE2, 0x42, 0xB0, 0x83, 0x40, 0xA5, 0xC3, 0x20, 0x2B,
            0xCC, 0x9A, 0xAE, 0xCA, 0xED, 0x4D, 0x70, 0x30, 0xA8, 0x70, 0x1C, 0x70, 0xFD, 0x13, 0x63, 0x29,
            0x02, 0x79, 0xEA, 0xD2, 0xA7, 0xAF, 0x35, 0x28, 0x32, 0x1C, 0x7B, 0xE6, 0x2F, 0x1A, 0xAA, 0x40,
            0x7E, 0x32, 0x8C, 0x27, 0x42, 0xFE, 0x82, 0x78, 0xEC, 0x0D, 0xEB, 0xE6, 0x83, 0x4B, 0x6D, 0x81,
            0x04, 0x40, 0x1A, 0x9E, 0x9A, 0x67, 0xF6, 0x72, 0x29, 0xFA, 0x04, 0xF0, 0x9D, 0xE4, 0xF4, 0x03
        };

        public byte[] AcidFixedKeyModulus { get; } =
        {
            0xDD, 0xC8, 0xDD, 0xF2, 0x4E, 0x6D, 0xF0, 0xCA, 0x9E, 0xC7, 0x5D, 0xC7, 0x7B, 0xAD, 0xFE, 0x7D,
            0x23, 0x89, 0x69, 0xB6, 0xF2, 0x06, 0xA2, 0x02, 0x88, 0xE1, 0x55, 0x91, 0xAB, 0xCB, 0x4D, 0x50,
            0x2E, 0xFC, 0x9D, 0x94, 0x76, 0xD6, 0x4C, 0xD8, 0xFF, 0x10, 0xFA, 0x5E, 0x93, 0x0A, 0xB4, 0x57,
            0xAC, 0x51, 0xC7, 0x16, 0x66, 0xF4, 0x1A, 0x54, 0xC2, 0xC5, 0x04, 0x3D, 0x1B, 0xFE, 0x30, 0x20,
            0x8A, 0xAC, 0x6F, 0x6F, 0xF5, 0xC7, 0xB6, 0x68, 0xB8, 0xC9, 0x40, 0x6B, 0x42, 0xAD, 0x11, 0x21,
            0xE7, 0x8B, 0xE9, 0x75, 0x01, 0x86, 0xE4, 0x48, 0x9B, 0x0A, 0x0A, 0xF8, 0x7F, 0xE8, 0x87, 0xF2,
            0x82, 0x01, 0xE6, 0xA3, 0x0F, 0xE4, 0x66, 0xAE, 0x83, 0x3F, 0x4E, 0x9F, 0x5E, 0x01, 0x30, 0xA4,
            0x00, 0xB9, 0x9A, 0xAE, 0x5F, 0x03, 0xCC, 0x18, 0x60, 0xE5, 0xEF, 0x3B, 0x5E, 0x15, 0x16, 0xFE,
            0x1C, 0x82, 0x78, 0xB5, 0x2F, 0x47, 0x7C, 0x06, 0x66, 0x88, 0x5D, 0x35, 0xA2, 0x67, 0x20, 0x10,
            0xE7, 0x6C, 0x43, 0x68, 0xD3, 0xE4, 0x5A, 0x68, 0x2A, 0x5A, 0xE2, 0x6D, 0x73, 0xB0, 0x31, 0x53,
            0x1C, 0x20, 0x09, 0x44, 0xF5, 0x1A, 0x9D, 0x22, 0xBE, 0x12, 0xA1, 0x77, 0x11, 0xE2, 0xA1, 0xCD,
            0x40, 0x9A, 0xA2, 0x8B, 0x60, 0x9B, 0xEF, 0xA0, 0xD3, 0x48, 0x63, 0xA2, 0xF8, 0xA3, 0x2C, 0x08,
            0x56, 0x52, 0x2E, 0x60, 0x19, 0x67, 0x5A, 0xA7, 0x9F, 0xDC, 0x3F, 0x3F, 0x69, 0x2B, 0x31, 0x6A,
            0xB7, 0x88, 0x4A, 0x14, 0x84, 0x80, 0x33, 0x3C, 0x9D, 0x44, 0xB7, 0x3F, 0x4C, 0xE1, 0x75, 0xEA,
            0x37, 0xEA, 0xE8, 0x1E, 0x7C, 0x77, 0xB7, 0xC6, 0x1A, 0xA2, 0xF0, 0x9F, 0x10, 0x61, 0xCD, 0x7B,
            0x5B, 0x32, 0x4C, 0x37, 0xEF, 0xB1, 0x71, 0x68, 0x53, 0x0A, 0xED, 0x51, 0x7D, 0x35, 0x22, 0xFD
        };

        public byte[] Package2FixedKeyModulus { get; } =
        {
            0x8D, 0x13, 0xA7, 0x77, 0x6A, 0xE5, 0xDC, 0xC0, 0x3B, 0x25, 0xD0, 0x58, 0xE4, 0x20, 0x69, 0x59,
            0x55, 0x4B, 0xAB, 0x70, 0x40, 0x08, 0x28, 0x07, 0xA8, 0xA7, 0xFD, 0x0F, 0x31, 0x2E, 0x11, 0xFE,
            0x47, 0xA0, 0xF9, 0x9D, 0xDF, 0x80, 0xDB, 0x86, 0x5A, 0x27, 0x89, 0xCD, 0x97, 0x6C, 0x85, 0xC5,
            0x6C, 0x39, 0x7F, 0x41, 0xF2, 0xFF, 0x24, 0x20, 0xC3, 0x95, 0xA6, 0xF7, 0x9D, 0x4A, 0x45, 0x74,
            0x8B, 0x5D, 0x28, 0x8A, 0xC6, 0x99, 0x35, 0x68, 0x85, 0xA5, 0x64, 0x32, 0x80, 0x9F, 0xD3, 0x48,
            0x39, 0xA2, 0x1D, 0x24, 0x67, 0x69, 0xDF, 0x75, 0xAC, 0x12, 0xB5, 0xBD, 0xC3, 0x29, 0x90, 0xBE,
            0x37, 0xE4, 0xA0, 0x80, 0x9A, 0xBE, 0x36, 0xBF, 0x1F, 0x2C, 0xAB, 0x2B, 0xAD, 0xF5, 0x97, 0x32,
            0x9A, 0x42, 0x9D, 0x09, 0x8B, 0x08, 0xF0, 0x63, 0x47, 0xA3, 0xE9, 0x1B, 0x36, 0xD8, 0x2D, 0x8A,
            0xD7, 0xE1, 0x54, 0x11, 0x95, 0xE4, 0x45, 0x88, 0x69, 0x8A, 0x2B, 0x35, 0xCE, 0xD0, 0xA5, 0x0B,
            0xD5, 0x5D, 0xAC, 0xDB, 0xAF, 0x11, 0x4D, 0xCA, 0xB8, 0x1E, 0xE7, 0x01, 0x9E, 0xF4, 0x46, 0xA3,
            0x8A, 0x94, 0x6D, 0x76, 0xBD, 0x8A, 0xC8, 0x3B, 0xD2, 0x31, 0x58, 0x0C, 0x79, 0xA8, 0x26, 0xE9,
            0xD1, 0x79, 0x9C, 0xCB, 0xD4, 0x2B, 0x6A, 0x4F, 0xC6, 0xCC, 0xCF, 0x90, 0xA7, 0xB9, 0x98, 0x47,
            0xFD, 0xFA, 0x4C, 0x6C, 0x6F, 0x81, 0x87, 0x3B, 0xCA, 0xB8, 0x50, 0xF6, 0x3E, 0x39, 0x5D, 0x4D,
            0x97, 0x3F, 0x0F, 0x35, 0x39, 0x53, 0xFB, 0xFA, 0xCD, 0xAB, 0xA8, 0x7A, 0x62, 0x9A, 0x3F, 0xF2,
            0x09, 0x27, 0x96, 0x3F, 0x07, 0x9A, 0x91, 0xF7, 0x16, 0xBF, 0xC6, 0x3A, 0x82, 0x5A, 0x4B, 0xCF,
            0x49, 0x50, 0x95, 0x8C, 0x55, 0x80, 0x7E, 0x39, 0xB1, 0x48, 0x05, 0x1E, 0x21, 0xC7, 0x24, 0x4F
        };

        public Dictionary<byte[], byte[]> TitleKeys { get; } = new Dictionary<byte[], byte[]>(new ByteArray128BitComparer());
        public Dictionary<byte[], string> TitleNames { get; } = new Dictionary<byte[], string>(new ByteArray128BitComparer());
        public Dictionary<byte[], uint> TitleVersions { get; } = new Dictionary<byte[], uint>(new ByteArray128BitComparer());

        public void SetSdSeed(byte[] sdseed)
        {
            Array.Copy(sdseed, SdSeed, SdSeed.Length);
            DeriveSdCardKeys();
        }

        public void DeriveKeys(IProgressReport logger = null)
        {
            DeriveKeyblobKeys();
            DecryptKeyblobs(logger);
            ReadKeyblobs();

            DerivePerConsoleKeys();
            DerivePerFirmwareKeys();
            DeriveNcaHeaderKey();
            DeriveSdCardKeys();
        }

        private void DeriveKeyblobKeys()
        {
            if (SecureBootKey.IsEmpty() || TsecKey.IsEmpty()) return;

            bool haveKeyblobMacKeySource = !MasterKeySource.IsEmpty();
            var temp = new byte[0x10];

            for (int i = 0; i < 0x20; i++)
            {
                if (KeyblobKeySources[i].IsEmpty()) continue;

                Crypto.DecryptEcb(TsecKey, KeyblobKeySources[i], temp, 0x10);
                Crypto.DecryptEcb(SecureBootKey, temp, KeyblobKeys[i], 0x10);

                if (!haveKeyblobMacKeySource) continue;

                Crypto.DecryptEcb(KeyblobKeys[i], KeyblobMacKeySource, KeyblobMacKeys[i], 0x10);
            }
        }

        private void DecryptKeyblobs(IProgressReport logger = null)
        {
            var cmac = new byte[0x10];
            var expectedCmac = new byte[0x10];
            var counter = new byte[0x10];

            for (int i = 0; i < 0x20; i++)
            {
                if (KeyblobKeys[i].IsEmpty() || KeyblobMacKeys[i].IsEmpty() || EncryptedKeyblobs[i].IsEmpty())
                {
                    continue;
                }

                Array.Copy(EncryptedKeyblobs[i], expectedCmac, 0x10);
                Crypto.CalculateAesCmac(KeyblobMacKeys[i], EncryptedKeyblobs[i], 0x10, cmac, 0, 0xa0);

                if (!Util.ArraysEqual(cmac, expectedCmac))
                {
                    logger?.LogMessage($"Warning: Keyblob MAC {i:x2} is invalid. Are SBK/TSEC key correct?");
                }

                Array.Copy(EncryptedKeyblobs[i], 0x10, counter, 0, 0x10);

                using (var keyblobDec = new Aes128CtrStorage(
                    new MemoryStorage(EncryptedKeyblobs[i], 0x20, Keyblobs[i].Length), KeyblobKeys[i], counter, false))
                {
                    keyblobDec.Read(Keyblobs[i], 0);
                }
            }
        }

        private void ReadKeyblobs()
        {
            var masterKek = new byte[0x10];

            bool haveMasterKeySource = !MasterKeySource.IsEmpty();

            for (int i = 0; i < 0x20; i++)
            {
                if (Keyblobs[i].IsEmpty()) continue;

                Array.Copy(Keyblobs[i], 0x80, Package1Keys[i], 0, 0x10);

                if (!haveMasterKeySource) continue;

                Array.Copy(Keyblobs[i], masterKek, 0x10);

                Crypto.DecryptEcb(masterKek, MasterKeySource, MasterKeys[i], 0x10);
            }
        }

        private void DerivePerConsoleKeys()
        {
            var kek = new byte[0x10];

            // Derive the device key
            if (!PerConsoleKeySource.IsEmpty() && !KeyblobKeys[0].IsEmpty())
            {
                Crypto.DecryptEcb(KeyblobKeys[0], PerConsoleKeySource, DeviceKey, 0x10);
            }

            // Derive save key
            if (!SaveMacKekSource.IsEmpty() && !SaveMacKeySource.IsEmpty() && !DeviceKey.IsEmpty())
            {
                Crypto.GenerateKek(DeviceKey, SaveMacKekSource, kek, AesKekGenerationSource, null);
                Crypto.DecryptEcb(kek, SaveMacKeySource, SaveMacKey, 0x10);
            }

            // Derive BIS keys
            if (DeviceKey.IsEmpty()
                || BisKeySource[0].IsEmpty()
                || BisKeySource[1].IsEmpty()
                || BisKeySource[2].IsEmpty()
                || BisKekSource.IsEmpty()
                || AesKekGenerationSource.IsEmpty()
                || AesKeyGenerationSource.IsEmpty()
                || RetailSpecificAesKeySource.IsEmpty())
            {
                return;
            }

            Crypto.DecryptEcb(DeviceKey, RetailSpecificAesKeySource, kek, 0x10);
            Crypto.DecryptEcb(kek, BisKeySource[0], BisKeys[0], 0x20);

            Crypto.GenerateKek(DeviceKey, BisKekSource, kek, AesKekGenerationSource, AesKeyGenerationSource);

            Crypto.DecryptEcb(kek, BisKeySource[1], BisKeys[1], 0x20);
            Crypto.DecryptEcb(kek, BisKeySource[2], BisKeys[2], 0x20);

            // BIS keys 2 and 3 are the same
            Array.Copy(BisKeys[2], BisKeys[3], 0x20);
        }

        private void DerivePerFirmwareKeys()
        {
            bool haveKakSource0 = !KeyAreaKeyApplicationSource.IsEmpty();
            bool haveKakSource1 = !KeyAreaKeyOceanSource.IsEmpty();
            bool haveKakSource2 = !KeyAreaKeySystemSource.IsEmpty();
            bool haveTitleKekSource = !TitlekekSource.IsEmpty();
            bool havePackage2KeySource = !Package2KeySource.IsEmpty();

            for (int i = 0; i < 0x20; i++)
            {
                if (MasterKeys[i].IsEmpty())
                {
                    continue;
                }

                if (haveKakSource0)
                {
                    Crypto.GenerateKek(MasterKeys[i], KeyAreaKeyApplicationSource, KeyAreaKeys[i][0],
                        AesKekGenerationSource, AesKeyGenerationSource);
                }

                if (haveKakSource1)
                {
                    Crypto.GenerateKek(MasterKeys[i], KeyAreaKeyOceanSource, KeyAreaKeys[i][1],
                        AesKekGenerationSource, AesKeyGenerationSource);
                }

                if (haveKakSource2)
                {
                    Crypto.GenerateKek(MasterKeys[i], KeyAreaKeySystemSource, KeyAreaKeys[i][2],
                        AesKekGenerationSource, AesKeyGenerationSource);
                }

                if (haveTitleKekSource)
                {
                    Crypto.DecryptEcb(MasterKeys[i], TitlekekSource, Titlekeks[i], 0x10);
                }

                if (havePackage2KeySource)
                {
                    Crypto.DecryptEcb(MasterKeys[i], Package2KeySource, Package2Keys[i], 0x10);
                }
            }
        }

        private void DeriveNcaHeaderKey()
        {
            if (HeaderKekSource.IsEmpty() || HeaderKeySource.IsEmpty() || MasterKeys[0].IsEmpty()) return;

            var headerKek = new byte[0x10];

            Crypto.GenerateKek(MasterKeys[0], HeaderKekSource, headerKek, AesKekGenerationSource,
                AesKeyGenerationSource);
            Crypto.DecryptEcb(headerKek, HeaderKeySource, HeaderKey, 0x20);
        }

        public void DeriveSdCardKeys()
        {
            var sdKek = new byte[0x10];
            Crypto.GenerateKek(MasterKeys[0], SdCardKekSource, sdKek, AesKekGenerationSource, AesKeyGenerationSource);

            for (int k = 0; k < SdCardKeySources.Length; k++)
            {
                for (int i = 0; i < 0x20; i++)
                {
                    SdCardKeySourcesSpecific[k][i] = (byte)(SdCardKeySources[k][i] ^ SdSeed[i & 0xF]);
                }
            }

            for (int k = 0; k < SdCardKeySourcesSpecific.Length; k++)
            {
                Crypto.DecryptEcb(sdKek, SdCardKeySourcesSpecific[k], SdCardKeys[k], 0x20);
            }
        }

        internal static readonly string[] KakNames = {"application", "ocean", "system"};
    }

    public static class ExternalKeys
    {
        private const int TitleKeySize = 0x10;

        public static readonly Dictionary<string, KeyValue> CommonKeyDict;
        public static readonly Dictionary<string, KeyValue> UniqueKeyDict;
        public static readonly Dictionary<string, KeyValue> AllKeyDict;

        static ExternalKeys()
        {
            List<KeyValue> commonKeys = CreateCommonKeyList();
            List<KeyValue> uniqueKeys = CreateUniqueKeyList();

            CommonKeyDict = commonKeys.ToDictionary(k => k.Name, k => k);
            UniqueKeyDict = uniqueKeys.ToDictionary(k => k.Name, k => k);
            AllKeyDict = uniqueKeys.Concat(commonKeys).ToDictionary(k => k.Name, k => k);
        }

        public static Keyset ReadKeyFile(string filename, string titleKeysFilename = null, string consoleKeysFilename = null, IProgressReport logger = null)
        {
            var keyset = new Keyset();

            try
            {
                if (filename != null) ReadMainKeys(keyset, filename, AllKeyDict, logger);
            }
            catch (FileNotFoundException) { }

            try
            {
                if (consoleKeysFilename != null) ReadMainKeys(keyset, consoleKeysFilename, AllKeyDict, logger);
            }
            catch (FileNotFoundException) { }

            try
            {
                if (titleKeysFilename != null) ReadTitleKeys(keyset, titleKeysFilename, logger);
            }
            catch (FileNotFoundException) { }

            keyset.DeriveKeys(logger);

            return keyset;
        }

        public static void LoadConsoleKeys(this Keyset keyset, string filename, IProgressReport logger = null)
        {
            foreach (KeyValue key in UniqueKeyDict.Values)
            {
                byte[] keyBytes = key.GetKey(keyset);
                Array.Clear(keyBytes, 0, keyBytes.Length);
            }

            ReadMainKeys(keyset, filename, UniqueKeyDict, logger);
            keyset.DeriveKeys();
        }

        private static void ReadMainKeys(Keyset keyset, string filename, Dictionary<string, KeyValue> keyDict, IProgressReport logger = null)
        {
            if (filename == null) return;

            using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] a = line.Split(',', '=');
                    if (a.Length != 2) continue;

                    string key = a[0].Trim();
                    string valueStr = a[1].Trim();

                    if (!keyDict.TryGetValue(key, out KeyValue kv))
                    {
                        logger?.LogMessage($"Failed to match key {key}");
                        continue;
                    }

                    byte[] value = valueStr.ToBytes();
                    if (value.Length != kv.Size)
                    {
                        logger?.LogMessage($"Key {key} had incorrect size {value.Length}. (Expected {kv.Size})");
                        continue;
                    }

                    byte[] dest = kv.GetKey(keyset);
                    Array.Copy(value, dest, value.Length);
                }
            }
        }

        private static void ReadTitleKeys(Keyset keyset, string filename, IProgressReport progress = null)
        {
            if (filename == null) return;

            int titleIdIdx = 0, rightsIdIdx = 0, titleKeyIdx = 1, titleNameIdx = 2, titleVersionIdx = 8;

            using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] splitLine;

                    // Some people use pipes as delimiters
                    if (line.Contains('|'))
                    {
                        splitLine = line.Split(new char[] { '|' }, StringSplitOptions.None);
                    }
                    else
                    {
                        splitLine = line.Split(new char[] { ',', '=' }, StringSplitOptions.None);
                    }

                    if (splitLine.Length < 2) continue;

                    if (!splitLine[0].Trim().TryToBytes(out byte[] _))
                    {
                        if (splitLine.Contains("id", StringComparer.OrdinalIgnoreCase))
                        {
                            titleIdIdx = Array.FindIndex(splitLine, x => x.Equals("id", StringComparison.OrdinalIgnoreCase));
                        }
                        if (splitLine.Contains("rightsId", StringComparer.OrdinalIgnoreCase))
                        {
                            rightsIdIdx = Array.FindIndex(splitLine, x => x.Equals("rightsId", StringComparison.OrdinalIgnoreCase));
                        }
                        else if (splitLine.Contains("rights_id", StringComparer.OrdinalIgnoreCase))
                        {
                            rightsIdIdx = Array.FindIndex(splitLine, x => x.Equals("rights_id", StringComparison.OrdinalIgnoreCase));
                        }
                        if (splitLine.Contains("key", StringComparer.OrdinalIgnoreCase))
                        {
                            titleKeyIdx = Array.FindIndex(splitLine, x => x.Equals("key", StringComparison.OrdinalIgnoreCase));
                        }
                        else if (splitLine.Contains("titleKey", StringComparer.OrdinalIgnoreCase))
                        {
                            titleKeyIdx = Array.FindIndex(splitLine, x => x.Equals("titleKey", StringComparison.OrdinalIgnoreCase));
                        }
                        else if (splitLine.Contains("hexadecimal_key_value", StringComparer.OrdinalIgnoreCase))
                        {
                            titleKeyIdx = Array.FindIndex(splitLine, x => x.Equals("hexadecimal_key_value", StringComparison.OrdinalIgnoreCase));
                        }
                        if (splitLine.Contains("name", StringComparer.OrdinalIgnoreCase))
                        {
                            titleNameIdx = Array.FindIndex(splitLine, x => x.Equals("name", StringComparison.OrdinalIgnoreCase));
                        }
                        else if (splitLine.Contains("title_name", StringComparer.OrdinalIgnoreCase))
                        {
                            titleNameIdx = Array.FindIndex(splitLine, x => x.Equals("title_name", StringComparison.OrdinalIgnoreCase));
                        }
                        if (splitLine.Contains("version", StringComparer.OrdinalIgnoreCase))
                        {
                            titleVersionIdx = Array.FindIndex(splitLine, x => x.Equals("version", StringComparison.OrdinalIgnoreCase));
                        }
                    }

                    try
                    {
                        splitLine[titleIdIdx].Trim().TryToBytes(out byte[] titleId);

                        if (!splitLine[rightsIdIdx].Trim().TryToBytes(out byte[] rightsId))
                        {
                            progress?.LogMessage($"Invalid rights ID \"{splitLine[rightsIdIdx].Trim()}\" in title key file");
                            continue;
                        }

                        if (!splitLine[titleKeyIdx].Trim().TryToBytes(out byte[] titleKey))
                        {
                            progress?.LogMessage($"Invalid title key \"{splitLine[titleKeyIdx].Trim()}\" in title key file");
                            continue;
                        }

                        if (rightsId.Length != TitleKeySize || rightsId.All(b => b == 0))
                        {
                            progress?.LogMessage($"Rights ID {rightsId.ToHexString()} had incorrect size {rightsId.Length}. (Expected {TitleKeySize}) or all zeros");

                            if (titleId.Length != TitleKeySize / 2)
                            {
                                continue;
                            }
                            else
                            {
                                rightsId = new byte[TitleKeySize];
                                Buffer.BlockCopy(titleId, 0, rightsId, 0, titleId.Length);
                            }
                        }

                        if (titleKey.Length != TitleKeySize)
                        {
                            progress?.LogMessage($"Title key {titleKey.ToHexString()} had incorrect size {titleKey.Length}. (Expected {TitleKeySize})");

                            titleKey = new byte[TitleKeySize];
                        }

                        keyset.TitleKeys[rightsId] = titleKey;

                        try
                        {
                            string titleName = splitLine[titleNameIdx].Trim();
                            if (!String.IsNullOrEmpty(titleName))
                            {
                                keyset.TitleNames[rightsId] = titleName;
                            }
                        }
                        catch (IndexOutOfRangeException) { }

                        try
                        {
                            if (UInt32.TryParse(splitLine[titleVersionIdx].Trim(), out uint titleVersion))
                            {
                                keyset.TitleVersions[rightsId] = titleVersion;
                            }
                        }
                        catch (IndexOutOfRangeException) { }
                    }
                    catch (IndexOutOfRangeException) { }
                }
            }
        }

        public static string PrintKeys(Keyset keyset, Dictionary<string, KeyValue> dict)
        {
            if (dict.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            int maxNameLength = dict.Values.Max(x => x.Name.Length);

            foreach (KeyValue keySlot in dict.Values.OrderBy(x => x.Name))
            {
                byte[] key = keySlot.GetKey(keyset);
                if (key.IsEmpty()) continue;

                string line = $"{keySlot.Name.PadRight(maxNameLength)} = {key.ToHexString()}";
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        public static string PrintCommonKeys(Keyset keyset)
        {
            return PrintKeys(keyset, CommonKeyDict);
        }

        public static string PrintUniqueKeys(Keyset keyset)
        {
            return PrintKeys(keyset, UniqueKeyDict);
        }

        public static string PrintAllKeys(Keyset keyset)
        {
            return PrintKeys(keyset, AllKeyDict);
        }

        public static string PrintTitleKeys(Keyset keyset)
        {
            var sb = new StringBuilder();

            foreach (KeyValuePair<byte[], byte[]> kv in keyset.TitleKeys)
            {
                string line = $"{kv.Key.ToHexString()} = {kv.Value.ToHexString()}";
                sb.AppendLine(line);
            }

            return sb.ToString();
        }

        private static List<KeyValue> CreateCommonKeyList()
        {
            var keys = new List<KeyValue>
            {
                new KeyValue("aes_kek_generation_source", 0x10, set => set.AesKekGenerationSource),
                new KeyValue("aes_key_generation_source", 0x10, set => set.AesKeyGenerationSource),
                new KeyValue("key_area_key_application_source", 0x10, set => set.KeyAreaKeyApplicationSource),
                new KeyValue("key_area_key_ocean_source", 0x10, set => set.KeyAreaKeyOceanSource),
                new KeyValue("key_area_key_system_source", 0x10, set => set.KeyAreaKeySystemSource),
                new KeyValue("titlekek_source", 0x10, set => set.TitlekekSource),
                new KeyValue("header_kek_source", 0x10, set => set.HeaderKekSource),
                new KeyValue("header_key_source", 0x20, set => set.HeaderKeySource),
                new KeyValue("header_key", 0x20, set => set.HeaderKey),
                new KeyValue("xci_header_key", 0x10, set => set.XciHeaderKey),
                new KeyValue("package2_key_source", 0x10, set => set.Package2KeySource),
                new KeyValue("sd_card_kek_source", 0x10, set => set.SdCardKekSource),
                new KeyValue("sd_card_nca_key_source", 0x20, set => set.SdCardKeySources[1]),
                new KeyValue("sd_card_save_key_source", 0x20, set => set.SdCardKeySources[0]),
                new KeyValue("master_key_source", 0x10, set => set.MasterKeySource),
                new KeyValue("keyblob_mac_key_source", 0x10, set => set.KeyblobMacKeySource),
                new KeyValue("eticket_rsa_kek", 0x10, set => set.EticketRsaKek),
                new KeyValue("retail_specific_aes_key_source", 0x10, set => set.RetailSpecificAesKeySource),
                new KeyValue("per_console_key_source", 0x10, set => set.PerConsoleKeySource),
                new KeyValue("bis_kek_source", 0x10, set => set.BisKekSource),
                new KeyValue("save_mac_kek_source", 0x10, set => set.SaveMacKekSource),
                new KeyValue("save_mac_key_source", 0x10, set => set.SaveMacKeySource),
                new KeyValue("save_mac_key", 0x10, set => set.SaveMacKey)
            };

            for (int slot = 0; slot < 0x20; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"keyblob_key_source_{i:x2}", 0x10, set => set.KeyblobKeySources[i]));
                keys.Add(new KeyValue($"keyblob_{i:x2}", 0x90, set => set.Keyblobs[i]));
                keys.Add(new KeyValue($"master_key_{i:x2}", 0x10, set => set.MasterKeys[i]));
                keys.Add(new KeyValue($"package1_key_{i:x2}", 0x10, set => set.Package1Keys[i]));
                keys.Add(new KeyValue($"package2_key_{i:x2}", 0x10, set => set.Package2Keys[i]));
                keys.Add(new KeyValue($"titlekek_{i:x2}", 0x10, set => set.Titlekeks[i]));
                keys.Add(new KeyValue($"key_area_key_application_{i:x2}", 0x10, set => set.KeyAreaKeys[i][0]));
                keys.Add(new KeyValue($"key_area_key_ocean_{i:x2}", 0x10, set => set.KeyAreaKeys[i][1]));
                keys.Add(new KeyValue($"key_area_key_system_{i:x2}", 0x10, set => set.KeyAreaKeys[i][2]));
            }

            for (int slot = 0; slot < 3; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"bis_key_source_{i:x2}", 0x20, set => set.BisKeySource[i]));
            }

            return keys;
        }

        private static List<KeyValue> CreateUniqueKeyList()
        {
            var keys = new List<KeyValue>
            {
                new KeyValue("secure_boot_key", 0x10, set => set.SecureBootKey),
                new KeyValue("tsec_key", 0x10, set => set.TsecKey),
                new KeyValue("device_key", 0x10, set => set.DeviceKey),
                new KeyValue("sd_seed", 0x10, set => set.SdSeed)
            };

            for (int slot = 0; slot < 0x20; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"keyblob_key_{i:x2}", 0x10, set => set.KeyblobKeys[i]));
                keys.Add(new KeyValue($"keyblob_mac_key_{i:x2}", 0x10, set => set.KeyblobMacKeys[i]));
                keys.Add(new KeyValue($"encrypted_keyblob_{i:x2}", 0xB0, set => set.EncryptedKeyblobs[i]));
            }

            for (int slot = 0; slot < 4; slot++)
            {
                int i = slot;
                keys.Add(new KeyValue($"bis_key_{i:x2}", 0x20, set => set.BisKeys[i]));
            }

            return keys;
        }

        public class KeyValue
        {
            public readonly string Name;
            public readonly int Size;
            public readonly Func<Keyset, byte[]> GetKey;

            public KeyValue(string name, int size, Func<Keyset, byte[]> retrieveFunc)
            {
                Name = name;
                Size = size;
                GetKey = retrieveFunc;
            }
        }
    }

    public enum KeyType
    {
        None,
        Common,
        Unique,
        Title
    }
}
