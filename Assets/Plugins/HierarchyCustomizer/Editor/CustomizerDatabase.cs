using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace HierarchyCustomizer
{
    /// <summary>
    /// A single customization entry: a color and/or icon assigned to either
    /// a GameObject (keyed by a stable GlobalObjectId string) or a Project
    /// folder (keyed by its asset GUID).
    /// </summary>
    [Serializable]
    public class CustomizerEntry
    {
        public string key;

        public bool hasColor;
        public Color color = new Color(0.35f, 0.55f, 0.85f, 0.35f);

        public bool hasIcon;
        public string iconName;   // built-in icon name, OR a texture GUID when isCustomIcon is true
        public bool isCustomIcon;
    }

    /// <summary>
    /// Persistent ScriptableObject asset that stores every customization made
    /// in the Hierarchy and Project windows. The asset is created
    /// automatically in the same folder as this script, wherever you've
    /// placed the HierarchyCustomizer package in your project, so it travels
    /// with the tool instead of scattering a separate "Editor" folder at
    /// your Assets root.
    /// </summary>
    public class CustomizerDatabase : ScriptableObject
    {
        private const string AssetFileName = "CustomizerDatabase.asset";

        [SerializeField] private List<CustomizerEntry> entries = new List<CustomizerEntry>();

        private Dictionary<string, CustomizerEntry> lookup;
        private static CustomizerDatabase instance;
        private static string cachedFolder;

        public static CustomizerDatabase Instance
        {
            get
            {
                if (instance == null)
                    instance = LoadOrCreate();
                return instance;
            }
        }

        public IReadOnlyList<CustomizerEntry> Entries => entries;

        /// <summary>
        /// Resolves the folder this script itself lives in. CallerFilePath
        /// is filled in by the compiler with the path of the file that calls
        /// this method - since every call site is inside this same file,
        /// that's always CustomizerDatabase.cs's own location.
        /// </summary>
        private static string PluginFolder([CallerFilePath] string sourceFilePath = "")
        {
            if (cachedFolder != null) return cachedFolder;

            string sourceFullPath = sourceFilePath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/'); // ".../MyProject/Assets"
            string projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);

            string relative = sourceFullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase)
                ? sourceFullPath.Substring(projectRoot.Length)
                : "Assets/Editor/HierarchyCustomizer/CustomizerDatabase.cs"; // fallback if path resolution fails

            cachedFolder = Path.GetDirectoryName(relative)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(cachedFolder))
                cachedFolder = "Assets";

            return cachedFolder;
        }

        private static string AssetPath => $"{PluginFolder()}/{AssetFileName}";

        private static CustomizerDatabase LoadOrCreate()
        {
            var path = AssetPath;
            var db = AssetDatabase.LoadAssetAtPath<CustomizerDatabase>(path);
            if (db != null)
                return db;

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            db = CreateInstance<CustomizerDatabase>();
            AssetDatabase.CreateAsset(db, path);
            AssetDatabase.SaveAssets();
            return db;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, CustomizerEntry>();
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.key) && !lookup.ContainsKey(e.key))
                    lookup.Add(e.key, e);
            }
        }

        public CustomizerEntry Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (lookup == null) BuildLookup();
            lookup.TryGetValue(key, out var entry);
            return entry;
        }

        public CustomizerEntry GetOrCreate(string key)
        {
            var e = Get(key);
            if (e != null) return e;

            e = new CustomizerEntry { key = key };
            entries.Add(e);
            lookup[key] = e;
            return e;
        }

        public void Remove(string key)
        {
            if (lookup == null) BuildLookup();
            entries.RemoveAll(e => e.key == key);
            lookup.Remove(key);
            Save();
        }

        public void ClearAll()
        {
            entries.Clear();
            lookup?.Clear();
            Save();
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
