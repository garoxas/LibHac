using System;
using System.Runtime.CompilerServices;
using LibHac.Common;
using LibHac.Diag;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.Sf;

namespace LibHac.FsSrv.Impl
{
    internal class FileSystemInterfaceAdapter : IFileSystemSf, IEnableSharedFromThis<FileSystemInterfaceAdapter>
    {
        private ReferenceCountedDisposable<IFileSystem> BaseFileSystem { get; }
        private bool IsHostFs { get; }
        private ReferenceCountedDisposable<FileSystemInterfaceAdapter>.WeakReference? SelfReference { get; set; }

        public FileSystemInterfaceAdapter(ReferenceCountedDisposable<IFileSystem> fileSystem, bool isHostFs = false)
        {
            BaseFileSystem = fileSystem.AddReference();
            IsHostFs = isHostFs;
        }

        private static ReadOnlySpan<byte> RootDir => new[] { (byte)'/' };

        public Result CreateFile(in Path path, long size, int option)
        {
            if (size < 0)
                return ResultFs.InvalidSize.Log();

            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            return BaseFileSystem.Target.CreateFile(normalizer.Path, size, (CreateFileOptions)option);
        }

        public Result DeleteFile(in Path path)
        {
            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            return BaseFileSystem.Target.DeleteFile(normalizer.Path);
        }

        public Result CreateDirectory(in Path path)
        {
            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            if (StringUtils.Compare(RootDir, normalizer.Path) != 0)
                return ResultFs.PathAlreadyExists.Log();

            return BaseFileSystem.Target.CreateDirectory(normalizer.Path);
        }

        public Result DeleteDirectory(in Path path)
        {
            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            if (StringUtils.Compare(RootDir, normalizer.Path) != 0)
                return ResultFs.DirectoryNotDeletable.Log();

            return BaseFileSystem.Target.DeleteDirectory(normalizer.Path);
        }

        public Result DeleteDirectoryRecursively(in Path path)
        {
            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            if (StringUtils.Compare(RootDir, normalizer.Path) != 0)
                return ResultFs.DirectoryNotDeletable.Log();

            return BaseFileSystem.Target.DeleteDirectoryRecursively(normalizer.Path);
        }

        public Result RenameFile(in Path oldPath, in Path newPath)
        {
            var normalizerOldPath = new PathNormalizer(new U8Span(oldPath.Str), GetPathNormalizerOption());
            if (normalizerOldPath.Result.IsFailure()) return normalizerOldPath.Result;

            var normalizerNewPath = new PathNormalizer(new U8Span(newPath.Str), GetPathNormalizerOption());
            if (normalizerNewPath.Result.IsFailure()) return normalizerNewPath.Result;

            return BaseFileSystem.Target.RenameFile(new U8Span(normalizerOldPath.Path),
                new U8Span(normalizerNewPath.Path));
        }

        public Result RenameDirectory(in Path oldPath, in Path newPath)
        {
            var normalizerOldPath = new PathNormalizer(new U8Span(oldPath.Str), GetPathNormalizerOption());
            if (normalizerOldPath.Result.IsFailure()) return normalizerOldPath.Result;

            var normalizerNewPath = new PathNormalizer(new U8Span(newPath.Str), GetPathNormalizerOption());
            if (normalizerNewPath.Result.IsFailure()) return normalizerNewPath.Result;

            if (PathTool.IsSubpath(normalizerOldPath.Path, normalizerNewPath.Path))
                return ResultFs.DirectoryNotRenamable.Log();

            return BaseFileSystem.Target.RenameDirectory(normalizerOldPath.Path, normalizerNewPath.Path);
        }

        public Result GetEntryType(out uint entryType, in Path path)
        {
            entryType = default;

            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            ref DirectoryEntryType type = ref Unsafe.As<uint, DirectoryEntryType>(ref entryType);

            return BaseFileSystem.Target.GetEntryType(out type, new U8Span(normalizer.Path));
        }

        public Result OpenFile(out ReferenceCountedDisposable<IFileSf> file, in Path path, uint mode)
        {
            const int maxTryCount = 2;
            file = default;

            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            Result rc = Result.Success;
            IFile fileInterface = null;

            for (int tryNum = 0; tryNum < maxTryCount; tryNum++)
            {
                rc = BaseFileSystem.Target.OpenFile(out fileInterface, new U8Span(normalizer.Path), (OpenMode)mode);

                // Retry on ResultDataCorrupted
                if (!ResultFs.DataCorrupted.Includes(rc))
                    break;
            }

            if (rc.IsFailure()) return rc;

            var adapter = new FileInterfaceAdapter(fileInterface, GetSelfReference());
            file = new ReferenceCountedDisposable<IFileSf>(adapter);

            return Result.Success;
        }

        public Result OpenDirectory(out ReferenceCountedDisposable<IDirectorySf> directory, in Path path, uint mode)
        {
            const int maxTryCount = 2;
            directory = default;

            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            Result rc = Result.Success;
            IDirectory dirInterface = null;

            for (int tryNum = 0; tryNum < maxTryCount; tryNum++)
            {
                rc = BaseFileSystem.Target.OpenDirectory(out dirInterface, new U8Span(normalizer.Path), (OpenDirectoryMode)mode);

                // Retry on ResultDataCorrupted
                if (!ResultFs.DataCorrupted.Includes(rc))
                    break;
            }

            if (rc.IsFailure()) return rc;

            var adapter = new DirectoryInterfaceAdapter(dirInterface, GetSelfReference());
            directory = new ReferenceCountedDisposable<IDirectorySf>(adapter);

            return Result.Success;
        }

        public Result Commit()
        {
            return BaseFileSystem.Target.Commit();
        }

        public Result GetFreeSpaceSize(out long freeSpace, in Path path)
        {
            freeSpace = default;

            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            return BaseFileSystem.Target.GetFreeSpaceSize(out freeSpace, normalizer.Path);
        }

        public Result GetTotalSpaceSize(out long totalSpace, in Path path)
        {
            totalSpace = default;

            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            return BaseFileSystem.Target.GetTotalSpaceSize(out totalSpace, normalizer.Path);
        }

        public Result CleanDirectoryRecursively(in Path path)
        {
            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            return BaseFileSystem.Target.CleanDirectoryRecursively(normalizer.Path);
        }

        public Result GetFileTimeStampRaw(out FileTimeStampRaw timeStamp, in Path path)
        {
            timeStamp = default;

            var normalizer = new PathNormalizer(new U8Span(path.Str), GetPathNormalizerOption());
            if (normalizer.Result.IsFailure()) return normalizer.Result;

            return BaseFileSystem.Target.GetFileTimeStampRaw(out timeStamp, normalizer.Path);
        }

        public Result QueryEntry(Span<byte> outBuffer, ReadOnlySpan<byte> inBuffer, int queryId, in Path path)
        {
            return BaseFileSystem.Target.QueryEntry(outBuffer, inBuffer, (QueryId)queryId, new U8Span(path.Str));
        }

        public void Dispose()
        {
            BaseFileSystem?.Dispose();
        }

        void IEnableSharedFromThis<FileSystemInterfaceAdapter>.SetSelfReference(ReferenceCountedDisposable<FileSystemInterfaceAdapter> reference)
        {
            SelfReference = new ReferenceCountedDisposable<FileSystemInterfaceAdapter>.WeakReference(reference);
        }

        private ReferenceCountedDisposable<FileSystemInterfaceAdapter> GetSelfReference()
        {
            Assert.AssertTrue(SelfReference != null, "Must first set a self-reference using SetSelfReference");

            // ReSharper disable once PossibleInvalidOperationException
            return SelfReference.Value.TryAddReference();
        }

        private PathNormalizer.Option GetPathNormalizerOption()
        {
            return IsHostFs ? PathNormalizer.Option.PreserveUnc : PathNormalizer.Option.None;
        }
    }
}
