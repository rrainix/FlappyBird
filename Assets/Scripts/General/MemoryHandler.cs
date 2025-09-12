using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Buffers.Binary;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public enum MemoryUnit
{
    Bit = 1,
    Byte = 1,
    KiloByte = 1024,
    MegaByte = 1024 * 1024,
    GigaByte = 1024 * 1024 * 1024,
}
public enum Extension
{
    txt, json, dat, bin, png
}

public static class FileManager
{
    public static string DesktopFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    public static string GameFolder
    {
        get
        {
            string basePath = UnityEngine.Application.persistentDataPath;
            string folder = Path.Combine(basePath, "Rainix");
            
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            return folder;
        }
    }

    public static string FromGameFolder(params string[] strings) => Path.Combine(GameFolder, Path.Combine(strings));
    public static string FromGameFolder(Extension extension, params string[] strings) => SetExtensionPath(Path.Combine(GameFolder, Path.Combine(strings)), extension);

    public static string SetExtensionPath(string path, Extension dataType) => Path.ChangeExtension(path, dataType.ToString());
    public static float GetFileSize(string path, MemoryUnit memoryUnit)
    {
        if (File.Exists(path))
        {
            long fileSizeInBytes = new FileInfo(path).Length;
            return fileSizeInBytes / (float)((int)memoryUnit);
        }

        return -1;
    }

    public static void DeleteAllFromDirectory(string directoryPath, bool onlyFiles)
    {
        if (Directory.GetDirectories(directoryPath).Length > 0)
            DeleteAllDirectories(directoryPath, onlyFiles);

        string[] files = Directory.GetFiles(directoryPath);

        foreach (string file in files)
        {
            File.Delete(file);
        }
    }
    public static void DeleteAllDirectories(string path, bool onlyFiles)
    {
        try
        {
            string[] directories = Directory.GetDirectories(path);

            foreach (string directory in directories)
            {
                DeleteAllFromDirectory(directory, false);

                if (!onlyFiles)
                    Directory.Delete(directory);
            }
        }
        catch (Exception ex)
        {
            throw new ArgumentException(ex.ToString());
        }
    }

    public static string[] GetDirectoryFiles(string directoryPath) => Directory.GetFiles(directoryPath);
    public static string[] GetDirectories(string path) => Directory.GetDirectories(path);

    public static string[] GetDirectoryFilesRecursive(string path)
    {
        List<string> files = new List<string>();

        foreach (var dir in GetDirectories(path))
        {
            files.AddRange(GetDirectoryFiles(dir));
        }

        return files.ToArray();
    }

    public static string RemoveLastPathSegment(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;


        string trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string parent = Path.GetDirectoryName(trimmed) ?? string.Empty;
        if (string.IsNullOrEmpty(parent) && Path.IsPathRooted(trimmed))
            return Path.GetPathRoot(trimmed) ?? string.Empty;

        return parent;
    }
    public static void DirectoryDetection(string path)
    {
        path = RemoveLastPathSegment(path);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }
}

public static class Serializer
{
    private static readonly JsonSerializerSettings defaultJson = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        ContractResolver = new DefaultContractResolver
        {
            DefaultMembersSearchFlags = System.Reflection.BindingFlags.Public
                                     | System.Reflection.BindingFlags.NonPublic
                                     | System.Reflection.BindingFlags.Instance
        }
    };
    public static readonly JsonSerializerSettings formatedJson = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        ContractResolver = new DefaultContractResolver
        {
            DefaultMembersSearchFlags = System.Reflection.BindingFlags.Public
                             | System.Reflection.BindingFlags.NonPublic
                             | System.Reflection.BindingFlags.Instance
        }
    };

    public static class Binary
    {
        public static void SaveUnmanagedBlock<T>(string path, ReadOnlySpan<T> span, CompressionLevel level = CompressionLevel.Fastest) where T : unmanaged
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20, FileOptions.SequentialScan);
            using var gzip = new GZipStream(fs, level, leaveOpen: false);

            Span<byte> len = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(len, span.Length);
            gzip.Write(len);

            ReadOnlySpan<byte> raw = MemoryMarshal.AsBytes(span);
            gzip.Write(raw);
        }

        public static ReadOnlySpan<T> LoadUnmanagedBlock<T>(string path) where T : unmanaged
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Datei nicht gefunden: {path}");

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
            using var gzip = new GZipStream(fs, CompressionMode.Decompress, leaveOpen: false);

            Span<byte> lenSpan = stackalloc byte[4];
            if (gzip.Read(lenSpan) != lenSpan.Length)
                throw new EndOfStreamException("Konnte Länge nicht vollständig lesen.");

            int count = BinaryPrimitives.ReadInt32LittleEndian(lenSpan);

            int byteCount = count * Marshal.SizeOf<T>();
            byte[] buffer = new byte[byteCount];
            int offset = 0;
            while (offset < byteCount)
            {
                int n = gzip.Read(buffer, offset, byteCount - offset);
                if (n == 0)
                    throw new EndOfStreamException("Unerwartetes Dateiende beim Lesen der Daten.");
                offset += n;
            }

            return MemoryMarshal.Cast<byte, T>(buffer);
        }

        public static void SaveObject<T>(string path, T item, CompressionLevel compressionLevel = CompressionLevel.Fastest, JsonSerializerSettings options = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            options ??= defaultJson;

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20, FileOptions.SequentialScan);
            using var gzip = new GZipStream(fs, compressionLevel, leaveOpen: false);
            using var sw = new StreamWriter(gzip);
            var serializer = JsonSerializer.Create(options);
            serializer.Serialize(sw, item);
        }

        public static T LoadObject<T>(string path, JsonSerializerSettings options = null)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File nicht gefunden: {path}");

            options ??= defaultJson;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
            using var gzip = new GZipStream(fs, CompressionMode.Decompress, leaveOpen: false);
            using var sr = new StreamReader(gzip);
            using var jr = new JsonTextReader(sr);
            var serializer = JsonSerializer.Create(options);
            return serializer.Deserialize<T>(jr)
                   ?? throw new InvalidDataException("Deserialisierung ergab null");
        }

        public static void SaveArray<T>(string path, T[] array, CompressionLevel compressionLevel = CompressionLevel.Fastest, JsonSerializerSettings options = null)
        {
            options ??= defaultJson;
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20, FileOptions.SequentialScan);
            using var gzip = new GZipStream(fs, compressionLevel, leaveOpen: false);
            using var sw = new StreamWriter(gzip);
            var serializer = JsonSerializer.Create(options);
            serializer.Serialize(sw, array);
        }

        public static T[] LoadArray<T>(string path, JsonSerializerSettings options = null)
        {
            if (!File.Exists(path))
                return Array.Empty<T>();

            options ??= defaultJson;
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
            using var gzip = new GZipStream(fs, CompressionMode.Decompress, leaveOpen: false);
            using var sr = new StreamReader(gzip);
            using var jr = new JsonTextReader(sr);
            var serializer = JsonSerializer.Create(options);
            return serializer.Deserialize<T[]>(jr)
                   ?? throw new InvalidDataException("Deserialisierung ergab null");
        }
    }
    public static class Json
    {
        public static void SaveObject<T>(string path, T obj, JsonSerializerSettings options = null)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            options ??= defaultJson;

            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20, FileOptions.SequentialScan);
            using var sw = new StreamWriter(fs);
            var serializer = JsonSerializer.Create(options);
            serializer.Serialize(sw, obj);
        }

        public static T LoadObject<T>(string path, T defaultValue = default!, JsonSerializerSettings options = null)
        {
            if (!File.Exists(path))
                return defaultValue;

            options ??= defaultJson;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);
            using var sr = new StreamReader(fs);
            using var jr = new JsonTextReader(sr);
            var serializer = JsonSerializer.Create(options);
            try
            {
                return serializer.Deserialize<T>(jr)!;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static void SaveUnmanagedArray<T>(string path, T[] array, JsonSerializerSettings options = null) where T : unmanaged
        {
            options ??= defaultJson;

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 20, FileOptions.SequentialScan);

            Span<byte> len = stackalloc byte[4];
            BinaryPrimitives.WriteInt32LittleEndian(len, array.Length);
            fs.Write(len);

            ReadOnlySpan<byte> raw = MemoryMarshal.AsBytes(array.AsSpan());
            fs.Write(raw);
        }

        public static T[] LoadUnmanagedArray<T>(string path) where T : unmanaged
        {
            if (!File.Exists(path))
                return Array.Empty<T>();

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 20, FileOptions.SequentialScan);

            Span<byte> len = stackalloc byte[4];
            if (fs.Read(len) != 4)
                throw new EndOfStreamException("Konnte Länge nicht lesen.");
            int count = BinaryPrimitives.ReadInt32LittleEndian(len);

            int byteCount = count * Marshal.SizeOf<T>();
            byte[] buffer = new byte[byteCount];
            int offset = 0;
            while (offset < byteCount)
            {
                int n = fs.Read(buffer, offset, byteCount - offset);
                if (n == 0) throw new EndOfStreamException("Unerwartetes Ende.");
                offset += n;
            }

            T[] result = new T[count];
            MemoryMarshal.Cast<byte, T>(buffer).CopyTo(result);
            return result;
        }

        public static void SaveArray<T>(string path, T[] array, JsonSerializerSettings options)
        {
            SaveObject(path, array, options);
        }

        public static T[] LoadArray<T>(string path, JsonSerializerSettings options)
        {
            return LoadObject<T[]>(path, defaultValue: Array.Empty<T>(), options);
        }
    }
}

public static class SaveManager
{
    public static readonly string mainPath = FileManager.FromGameFolder("Data");

    public static string GetType<T>()
    {
        Type type = typeof(T);

        if (type.IsClass)
            return "Class";
        else if (type.IsValueType && !type.IsPrimitive)
            return "Nonprimtive Struct";
        else if (type.IsValueType)
            return "Primitive Struct";
        else
            return "Undefined Type";
    }

    private static string CreatePathFromType<T>(string key)
        => FileManager.SetExtensionPath(Path.Combine(mainPath, mainPath, GetType<T>().ToString(), typeof(T).Name, key), Extension.json);

    public static int FilesCount => FileManager.GetDirectoryFilesRecursive(mainPath).Length;

    public static void ClearAll(bool filesOnly = false) => FileManager.DeleteAllDirectories(mainPath, filesOnly);

    public static void Delete<T>(string key)
    {
        string path = CreatePathFromType<T>(key);

        if (!File.Exists(path))
            throw new FileNotFoundException("File doesn't Exist");

        File.Delete(path);
    }
    public static void Save<T>(string key, T obj, JsonSerializerSettings? options = null)
    {
        string path = CreatePathFromType<T>(key);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        Serializer.Json.SaveObject(path, obj, options);
    }
    public static T Load<T>(string key, T defaultValue = default!, JsonSerializerSettings? options = null)
    {
        string path = CreatePathFromType<T>(key);

        if (!File.Exists(path)) return defaultValue;
        return Serializer.Json.LoadObject<T>(path);
    }
}