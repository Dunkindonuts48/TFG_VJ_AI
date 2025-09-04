using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


namespace TFG.Memory
{
    [Serializable] class MemDTO { public MemoryRecord[] items; }

    public static class MemPersistence
    {
        static string FilePath(string fileName) =>
            Path.Combine(Application.persistentDataPath, fileName);

        public static void Save(string fileName, List<MemoryRecord> all)
        {
            try
            {
                var dto = new MemDTO { items = all.ToArray() };
                var json = JsonUtility.ToJson(dto, true);
                File.WriteAllText(FilePath(fileName), json);
                Debug.Log($"[MemPersistence] Guardado {all.Count} → {FilePath(fileName)}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemPersistence] Error al guardar: {e.Message}");
            }
        }

        public static List<MemoryRecord> Load(string fileName)
        {
            var path = FilePath(fileName);
            var list = new List<MemoryRecord>();
            try
            {
                if (!File.Exists(path)) return list;
                var json = File.ReadAllText(path);
                var dto = JsonUtility.FromJson<MemDTO>(json);
                if (dto?.items != null) list.AddRange(dto.items);
                Debug.Log($"[MemPersistence] Cargado {list.Count} ← {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[MemPersistence] Error al cargar: {e.Message}");
            }
            return list;
        }

        public static string GetAbsolutePath(string fileName) => FilePath(fileName);
    }
}
