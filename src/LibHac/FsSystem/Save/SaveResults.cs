﻿using LibHac.Fs;

namespace LibHac.FsSystem.Save
{
    internal static class SaveResults
    {
        public static Result ConvertToExternalResult(Result result)
        {
            int description = (int)result.Description;

            if (result == Result.Success)
            {
                return Result.Success;
            }

            if (result.Module != ResultFs.ModuleFs)
            {
                return result;
            }

            if (ResultFs.UnsupportedVersion.Includes(result))
            {
                return ResultFs.UnsupportedSaveVersion.Value;
            }

            if (ResultFs.IntegrityVerificationStorageCorrupted.Includes(result))
            {
                if (ResultFs.InvalidIvfcMagic.Includes(result))
                {
                    return ResultFs.InvalidSaveDataIvfcMagic.Value;
                }

                if (ResultFs.InvalidIvfcHashValidationBit.Includes(result))
                {
                    return ResultFs.InvalidSaveDataIvfcHashValidationBit.Value;
                }

                if (ResultFs.InvalidIvfcHash.Includes(result))
                {
                    return ResultFs.InvalidSaveDataIvfcHash.Value;
                }

                if (ResultFs.EmptyIvfcHash.Includes(result))
                {
                    return ResultFs.EmptySaveDataIvfcHash.Value;
                }

                if (ResultFs.InvalidHashInIvfcTopLayer.Includes(result))
                {
                    return ResultFs.InvalidSaveDataHashInIvfcTopLayer.Value;
                }

                return result;
            }

            if (ResultFs.BuiltInStorageCorrupted.Includes(result))
            {
                if (ResultFs.InvalidGptPartitionSignature.Includes(result))
                {
                    return ResultFs.SaveDataInvalidGptPartitionSignature.Value;
                }

                return result;
            }

            if (ResultFs.HostFileSystemCorrupted.Includes(result))
            {
                if (description > 4701 && description < 4706)
                {
                    return new Result(ResultFs.ModuleFs, description - 260);
                }

                return result;
            }

            if (ResultFs.ZeroBitmapFileCorrupted.Includes(result))
            {
                if (ResultFs.IncompleteBlockInZeroBitmapHashStorageFile.Includes(result))
                {
                    return ResultFs.IncompleteBlockInZeroBitmapHashStorageFileSaveData.Value;
                }

                return result;
            }

            if (ResultFs.DatabaseCorrupted.Includes(result))
            {
                if (description > 4721 && description < 4729)
                {
                    return new Result(ResultFs.ModuleFs, description - 260);
                }

                return result;
            }

            if (ResultFs.FatFileSystemCorrupted.Includes(result))
            {
                return result;
            }

            if (ResultFs.EntryNotFound.Includes(result))
            {
                return ResultFs.PathNotFound.Value;
            }

            if (ResultFs.SaveDataPathAlreadyExists.Includes(result))
            {
                return ResultFs.PathAlreadyExists.Value;
            }

            if (ResultFs.PathNotFoundInSaveDataFileTable.Includes(result))
            {
                return ResultFs.PathNotFound.Value;
            }

            if (ResultFs.InvalidOffset.Includes(result))
            {
                return ResultFs.OutOfRange.Value;
            }

            if (ResultFs.AllocationTableInsufficientFreeBlocks.Includes(result))
            {
                return ResultFs.InsufficientFreeSpace.Value;
            }

            return result;
        }
    }
}
