using System;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Shim;
using LibHac.FsSystem;
using LibHac.FsSystem.Save;
using LibHac.FsService.Creators;
using LibHac.Ncm;
using LibHac.Spl;
using RightsId = LibHac.Fs.RightsId;

namespace LibHac.FsService
{
    public class FileSystemProxyCore
    {
        private FileSystemCreators FsCreators { get; }
        private ExternalKeySet ExternalKeys { get; }
        private IDeviceOperator DeviceOperator { get; }

        private byte[] SdEncryptionSeed { get; } = new byte[0x10];

        private const string NintendoDirectoryName = "Nintendo";
        private const string ContentDirectoryName = "Contents";

        private GlobalAccessLogMode LogMode { get; set; }

        public FileSystemProxyCore(FileSystemCreators fsCreators, ExternalKeySet externalKeys, IDeviceOperator deviceOperator)
        {
            FsCreators = fsCreators;
            ExternalKeys = externalKeys ?? new ExternalKeySet();
            DeviceOperator = deviceOperator;
        }

        public Result OpenFileSystem(out IFileSystem fileSystem, U8Span path, FileSystemProxyType type,
            bool canMountSystemDataPrivate, TitleId titleId)
        {
            fileSystem = default;

            U8Span path2 = path;

            Result rc = OpenFileSystemFromMountName(ref path2, out IFileSystem baseFileSystem, out bool successQQ,
                out MountNameInfo mountNameInfo);
            if (rc.IsFailure()) return rc;

            if (!successQQ)
                return ResultFs.InvalidArgument.Log();

            if (type == FileSystemProxyType.Logo && mountNameInfo.IsGameCard)
            {
                rc = OpenGameCardFileSystem(out fileSystem, new GameCardHandle(mountNameInfo.GcHandle),
                    GameCardPartition.Logo);

                if (rc.IsSuccess())
                    return Result.Success;

                if (rc != ResultFs.PartitionNotFound)
                    return rc;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Stores info obtained by parsing a common mount name.
        /// </summary>
        private struct MountNameInfo
        {
            public bool IsGameCard;
            public int GcHandle;
            public bool IsHostFs;
            public bool Field9;
        }

        private Result OpenFileSystemFromMountName(ref U8Span path, out IFileSystem fileSystem, out bool successQQ,
            out MountNameInfo info)
        {
            fileSystem = default;

            info = new MountNameInfo();
            successQQ = true;

            if (StringUtils.Compare(path, CommonMountNames.GameCardMountName) == 0)
            {
                path = path.Slice(CommonMountNames.SystemContentMountName.Length);

                info.IsGameCard = true;
                info.Field9 = true;

                throw new NotImplementedException();
            }

            else if (StringUtils.Compare(path, CommonMountNames.SystemContentMountName) == 0)
            {
                path = path.Slice(CommonMountNames.SystemContentMountName.Length);

                Result rc = OpenContentStorageFileSystem(out fileSystem, ContentStorageId.System);
                if (rc.IsFailure()) return rc;

                info.Field9 = true;
            }

            else if (StringUtils.Compare(path, CommonMountNames.UserContentMountName) == 0)
            {
                path = path.Slice(CommonMountNames.UserContentMountName.Length);

                Result rc = OpenContentStorageFileSystem(out fileSystem, ContentStorageId.User);
                if (rc.IsFailure()) return rc;

                info.Field9 = true;
            }

            else if (StringUtils.Compare(path, CommonMountNames.SdCardContentMountName) == 0)
            {
                path = path.Slice(CommonMountNames.SdCardContentMountName.Length);

                Result rc = OpenContentStorageFileSystem(out fileSystem, ContentStorageId.SdCard);
                if (rc.IsFailure()) return rc;

                info.Field9 = true;
            }

            else if (StringUtils.Compare(path, CommonMountNames.CalibrationPartitionMountName) == 0)
            {
                path = path.Slice(CommonMountNames.CalibrationPartitionMountName.Length);

                Result rc = OpenBisFileSystem(out fileSystem, string.Empty, BisPartitionId.CalibrationFile);
                if (rc.IsFailure()) return rc;
            }

            else if (StringUtils.Compare(path, CommonMountNames.SafePartitionMountName) == 0)
            {
                path = path.Slice(CommonMountNames.SafePartitionMountName.Length);

                Result rc = OpenBisFileSystem(out fileSystem, string.Empty, BisPartitionId.SafeMode);
                if (rc.IsFailure()) return rc;
            }

            else if (StringUtils.Compare(path, CommonMountNames.UserPartitionMountName) == 0)
            {
                path = path.Slice(CommonMountNames.UserPartitionMountName.Length);

                Result rc = OpenBisFileSystem(out fileSystem, string.Empty, BisPartitionId.User);
                if (rc.IsFailure()) return rc;
            }

            else if (StringUtils.Compare(path, CommonMountNames.SystemPartitionMountName) == 0)
            {
                path = path.Slice(CommonMountNames.SystemPartitionMountName.Length);

                Result rc = OpenBisFileSystem(out fileSystem, string.Empty, BisPartitionId.System);
                if (rc.IsFailure()) return rc;
            }

            else if (StringUtils.Compare(path, CommonMountNames.SdCardMountName) == 0)
            {
                path = path.Slice(CommonMountNames.SdCardMountName.Length);

                Result rc = OpenSdCardFileSystem(out fileSystem);
                if (rc.IsFailure()) return rc;
            }

            else if (StringUtils.Compare(path, CommonMountNames.HostMountName) == 0)
            {
                path = path.Slice(CommonMountNames.HostMountName.Length);

                info.IsHostFs = true;
                info.Field9 = true;

                throw new NotImplementedException();
            }

            else if (StringUtils.Compare(path, CommonMountNames.RegisteredUpdatePartitionMountName) == 0)
            {
                path = path.Slice(CommonMountNames.RegisteredUpdatePartitionMountName.Length);

                info.Field9 = true;

                throw new NotImplementedException();
            }

            else
            {
                return ResultFs.PathNotFound.Log();
            }

            if (StringUtils.GetLength(path, FsPath.MaxLength) == 0)
            {
                successQQ = false;
            }

            return Result.Success;
        }

        public Result OpenBisFileSystem(out IFileSystem fileSystem, string rootPath, BisPartitionId partitionId)
        {
            return FsCreators.BuiltInStorageFileSystemCreator.Create(out fileSystem, rootPath, partitionId);
        }

        public Result OpenSdCardFileSystem(out IFileSystem fileSystem)
        {
            return FsCreators.SdFileSystemCreator.Create(out fileSystem);
        }

        public Result OpenGameCardStorage(out IStorage storage, GameCardHandle handle, GameCardPartitionRaw partitionId)
        {
            switch (partitionId)
            {
                case GameCardPartitionRaw.Normal:
                    return FsCreators.GameCardStorageCreator.CreateNormal(handle, out storage);
                case GameCardPartitionRaw.Secure:
                    return FsCreators.GameCardStorageCreator.CreateSecure(handle, out storage);
                case GameCardPartitionRaw.Writable:
                    return FsCreators.GameCardStorageCreator.CreateWritable(handle, out storage);
                default:
                    throw new ArgumentOutOfRangeException(nameof(partitionId), partitionId, null);
            }
        }

        public Result OpenDeviceOperator(out IDeviceOperator deviceOperator)
        {
            deviceOperator = DeviceOperator;
            return Result.Success;
        }

        public Result OpenContentStorageFileSystem(out IFileSystem fileSystem, ContentStorageId storageId)
        {
            fileSystem = default;

            string contentDirPath = default;
            IFileSystem baseFileSystem = default;
            bool isEncrypted = false;
            Result rc;

            switch (storageId)
            {
                case ContentStorageId.System:
                    rc = OpenBisFileSystem(out baseFileSystem, string.Empty, BisPartitionId.System);
                    contentDirPath = $"/{ContentDirectoryName}";
                    break;
                case ContentStorageId.User:
                    rc = OpenBisFileSystem(out baseFileSystem, string.Empty, BisPartitionId.User);
                    contentDirPath = $"/{ContentDirectoryName}";
                    break;
                case ContentStorageId.SdCard:
                    rc = OpenSdCardFileSystem(out baseFileSystem);
                    contentDirPath = $"/{NintendoDirectoryName}/{ContentDirectoryName}";
                    isEncrypted = true;
                    break;
                default:
                    rc = ResultFs.InvalidArgument;
                    break;
            }

            if (rc.IsFailure()) return rc;

            baseFileSystem.EnsureDirectoryExists(contentDirPath);

            rc = FsCreators.SubDirectoryFileSystemCreator.Create(out IFileSystem subDirFileSystem,
                baseFileSystem, contentDirPath);
            if (rc.IsFailure()) return rc;

            if (!isEncrypted)
            {
                fileSystem = subDirFileSystem;
                return Result.Success;
            }

            return FsCreators.EncryptedFileSystemCreator.Create(out fileSystem, subDirFileSystem,
                EncryptedFsKeyId.Content, SdEncryptionSeed);
        }

        public Result OpenCustomStorageFileSystem(out IFileSystem fileSystem, CustomStorageId storageId)
        {
            fileSystem = default;

            switch (storageId)
            {
                case CustomStorageId.SdCard:
                    {
                        Result rc = FsCreators.SdFileSystemCreator.Create(out IFileSystem sdFs);
                        if (rc.IsFailure()) return rc;

                        string customStorageDir = CustomStorage.GetCustomStorageDirectoryName(CustomStorageId.SdCard);
                        string subDirName = $"/{NintendoDirectoryName}/{customStorageDir}";

                        rc = Util.CreateSubFileSystem(out IFileSystem subFs, sdFs, subDirName, true);
                        if (rc.IsFailure()) return rc;

                        rc = FsCreators.EncryptedFileSystemCreator.Create(out IFileSystem encryptedFs, subFs,
                            EncryptedFsKeyId.CustomStorage, SdEncryptionSeed);
                        if (rc.IsFailure()) return rc;

                        fileSystem = encryptedFs;
                        return Result.Success;
                    }
                case CustomStorageId.User:
                    {
                        Result rc = FsCreators.BuiltInStorageFileSystemCreator.Create(out IFileSystem userFs, string.Empty,
                            BisPartitionId.User);
                        if (rc.IsFailure()) return rc;

                        string customStorageDir = CustomStorage.GetCustomStorageDirectoryName(CustomStorageId.User);
                        string subDirName = $"/{customStorageDir}";

                        rc = Util.CreateSubFileSystem(out IFileSystem subFs, userFs, subDirName, true);
                        if (rc.IsFailure()) return rc;

                        fileSystem = subFs;
                        return Result.Success;
                    }
                default:
                    return ResultFs.InvalidArgument.Log();
            }
        }

        public Result OpenGameCardFileSystem(out IFileSystem fileSystem, GameCardHandle handle,
            GameCardPartition partitionId)
        {
            return FsCreators.GameCardFileSystemCreator.Create(out fileSystem, handle, partitionId);
        }

        public Result RegisterExternalKey(ref RightsId rightsId, ref AccessKey externalKey)
        {
            return ExternalKeys.Add(rightsId, externalKey);
        }

        public Result UnregisterExternalKey(ref RightsId rightsId)
        {
            ExternalKeys.Remove(rightsId);

            return Result.Success;
        }

        public Result UnregisterAllExternalKey()
        {
            ExternalKeys.Clear();

            return Result.Success;
        }

        public Result SetSdCardEncryptionSeed(ReadOnlySpan<byte> seed)
        {
            seed.CopyTo(SdEncryptionSeed);
            //FsCreators.SaveDataFileSystemCreator.SetSdCardEncryptionSeed(seed);

            return Result.Success;
        }

        public bool AllowDirectorySaveData(SaveDataSpaceId spaceId, string saveDataRootPath)
        {
            return spaceId == SaveDataSpaceId.User && !string.IsNullOrWhiteSpace(saveDataRootPath);
        }

        public Result DoesSaveDataExist(out bool exists, SaveDataSpaceId spaceId, ulong saveDataId)
        {
            exists = false;

            Result rc = OpenSaveDataDirectory(out IFileSystem fileSystem, spaceId, string.Empty, true);
            if (rc.IsFailure()) return rc;

            string saveDataPath = $"/{saveDataId:x16}";

            rc = fileSystem.GetEntryType(out _, saveDataPath);

            if (rc.IsFailure())
            {
                if (rc == ResultFs.PathNotFound)
                {
                    return Result.Success;
                }

                return rc;
            }

            exists = true;
            return Result.Success;
        }

        public Result OpenSaveDataFileSystem(out IFileSystem fileSystem, SaveDataSpaceId spaceId, ulong saveDataId,
            string saveDataRootPath, bool openReadOnly, SaveDataType type, bool cacheExtraData)
        {
            fileSystem = default;

            Result rc = OpenSaveDataDirectory(out IFileSystem saveDirFs, spaceId, saveDataRootPath, true);
            if (rc.IsFailure()) return rc;

            bool allowDirectorySaveData = AllowDirectorySaveData(spaceId, saveDataRootPath);
            bool useDeviceUniqueMac = Util.UseDeviceUniqueSaveMac(spaceId);

            if (allowDirectorySaveData)
            {
                rc = saveDirFs.EnsureDirectoryExists(GetSaveDataIdPath(saveDataId));
                if (rc.IsFailure()) return rc;
            }

            // Missing save FS cache lookup

            rc = FsCreators.SaveDataFileSystemCreator.Create(out IFileSystem saveFs, out _, saveDirFs, saveDataId,
                allowDirectorySaveData, useDeviceUniqueMac, type, null);

            if (rc.IsFailure()) return rc;

            if (cacheExtraData)
            {
                // Missing extra data caching
            }

            fileSystem = openReadOnly ? new ReadOnlyFileSystem(saveFs) : saveFs;

            return Result.Success;
        }

        public Result OpenSaveDataDirectory(out IFileSystem fileSystem, SaveDataSpaceId spaceId, string saveDataRootPath, bool openOnHostFs)
        {
            if (openOnHostFs && AllowDirectorySaveData(spaceId, saveDataRootPath))
            {
                Result rc = FsCreators.TargetManagerFileSystemCreator.Create(out IFileSystem hostFs, false);

                if (rc.IsFailure())
                {
                    fileSystem = default;
                    return rc;
                }

                return Util.CreateSubFileSystem(out fileSystem, hostFs, saveDataRootPath, true);
            }

            string dirName = spaceId == SaveDataSpaceId.TemporaryStorage ? "/temp" : "/save";

            return OpenSaveDataDirectoryImpl(out fileSystem, spaceId, dirName, true);
        }

        public Result OpenSaveDataDirectoryImpl(out IFileSystem fileSystem, SaveDataSpaceId spaceId, string saveDirName, bool createIfMissing)
        {
            fileSystem = default;
            Result rc;

            switch (spaceId)
            {
                case SaveDataSpaceId.System:
                    rc = OpenBisFileSystem(out IFileSystem sysFs, string.Empty, BisPartitionId.System);
                    if (rc.IsFailure()) return rc;

                    return Util.CreateSubFileSystem(out fileSystem, sysFs, saveDirName, createIfMissing);

                case SaveDataSpaceId.User:
                case SaveDataSpaceId.TemporaryStorage:
                    rc = OpenBisFileSystem(out IFileSystem userFs, string.Empty, BisPartitionId.User);
                    if (rc.IsFailure()) return rc;

                    return Util.CreateSubFileSystem(out fileSystem, userFs, saveDirName, createIfMissing);

                case SaveDataSpaceId.SdSystem:
                case SaveDataSpaceId.SdCache:
                    rc = OpenSdCardFileSystem(out IFileSystem sdFs);
                    if (rc.IsFailure()) return rc;

                    string sdSaveDirPath = $"/{NintendoDirectoryName}{saveDirName}";

                    rc = Util.CreateSubFileSystem(out IFileSystem sdSubFs, sdFs, sdSaveDirPath, createIfMissing);
                    if (rc.IsFailure()) return rc;

                    return FsCreators.EncryptedFileSystemCreator.Create(out fileSystem, sdSubFs,
                        EncryptedFsKeyId.Save, SdEncryptionSeed);

                case SaveDataSpaceId.ProperSystem:
                    rc = OpenBisFileSystem(out IFileSystem sysProperFs, string.Empty, BisPartitionId.SystemProperPartition);
                    if (rc.IsFailure()) return rc;

                    return Util.CreateSubFileSystem(out fileSystem, sysProperFs, saveDirName, createIfMissing);

                case SaveDataSpaceId.Safe:
                    rc = OpenBisFileSystem(out IFileSystem safeFs, string.Empty, BisPartitionId.SafeMode);
                    if (rc.IsFailure()) return rc;

                    return Util.CreateSubFileSystem(out fileSystem, safeFs, saveDirName, createIfMissing);

                default:
                    return ResultFs.InvalidArgument.Log();
            }
        }

        public Result OpenSaveDataMetaFile(out IFile file, ulong saveDataId, SaveDataSpaceId spaceId, SaveMetaType type)
        {
            file = default;

            string metaDirPath = $"/saveMeta/{saveDataId:x16}";

            Result rc = OpenSaveDataDirectoryImpl(out IFileSystem tmpMetaDirFs, spaceId, metaDirPath, true);
            using IFileSystem metaDirFs = tmpMetaDirFs;
            if (rc.IsFailure()) return rc;

            string metaFilePath = $"/{(int)type:x8}.meta";

            return metaDirFs.OpenFile(out file, metaFilePath, OpenMode.ReadWrite);
        }

        public Result DeleteSaveDataMetaFiles(ulong saveDataId, SaveDataSpaceId spaceId)
        {
            Result rc = OpenSaveDataDirectoryImpl(out IFileSystem metaDirFs, spaceId, "/saveMeta", false);

            using (metaDirFs)
            {
                if (rc.IsFailure()) return rc;

                rc = metaDirFs.DeleteDirectoryRecursively($"/{saveDataId:x16}");

                if (rc.IsFailure() && rc != ResultFs.PathNotFound)
                    return rc;

                return Result.Success;
            }
        }

        public Result CreateSaveDataMetaFile(ulong saveDataId, SaveDataSpaceId spaceId, SaveMetaType type, long size)
        {
            string metaDirPath = $"/saveMeta/{saveDataId:x16}";

            Result rc = OpenSaveDataDirectoryImpl(out IFileSystem tmpMetaDirFs, spaceId, metaDirPath, true);
            using IFileSystem metaDirFs = tmpMetaDirFs;
            if (rc.IsFailure()) return rc;

            string metaFilePath = $"/{(int)type:x8}.meta";

            if (size < 0) return ResultFs.ValueOutOfRange.Log();

            return metaDirFs.CreateFile(metaFilePath, size, CreateFileOptions.None);
        }

        public Result CreateSaveDataFileSystem(ulong saveDataId, ref SaveDataAttribute attribute,
            ref SaveDataCreateInfo createInfo, U8Span rootPath, OptionalHashSalt hashSalt, bool something)
        {
            // Use directory save data for now

            Result rc = OpenSaveDataDirectory(out IFileSystem fileSystem, createInfo.SpaceId, string.Empty, false);
            if (rc.IsFailure()) return rc;

            return fileSystem.EnsureDirectoryExists(GetSaveDataIdPath(saveDataId));
        }

        public Result DeleteSaveDataFileSystem(SaveDataSpaceId spaceId, ulong saveDataId, bool doSecureDelete)
        {
            Result rc = OpenSaveDataDirectory(out IFileSystem fileSystem, spaceId, string.Empty, false);

            using (fileSystem)
            {
                if (rc.IsFailure()) return rc;

                string saveDataPath = GetSaveDataIdPath(saveDataId);

                rc = fileSystem.GetEntryType(out DirectoryEntryType entryType, saveDataPath);
                if (rc.IsFailure()) return rc;

                if (entryType == DirectoryEntryType.Directory)
                {
                    rc = fileSystem.DeleteDirectoryRecursively(saveDataPath);
                }
                else
                {
                    if (doSecureDelete)
                    {
                        // Overwrite file with garbage before deleting
                        throw new NotImplementedException();
                    }

                    rc = fileSystem.DeleteFile(saveDataPath);
                }

                return rc;
            }
        }

        public Result SetGlobalAccessLogMode(GlobalAccessLogMode mode)
        {
            LogMode = mode;
            return Result.Success;
        }

        public Result GetGlobalAccessLogMode(out GlobalAccessLogMode mode)
        {
            mode = LogMode;
            return Result.Success;
        }

        private string GetSaveDataIdPath(ulong saveDataId)
        {
            return $"/{saveDataId:x16}";
        }
    }
}
