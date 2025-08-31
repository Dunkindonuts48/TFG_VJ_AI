using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MemoryEditorUI : MonoBehaviour
{
    public MemoryRepository repo;
    public KeyCode toggleKey = KeyCode.F2;
    public bool visible = true;

    // Estado UI
    Vector2 _scroll;
    int _selected = -1;
    MemoryRecord _edit;

    void Awake()
    {
        if (!repo) repo = GetComponent<MemoryRepository>();
        if (repo) repo.OnChanged += () => { if (_selected >= repo.AllReadOnly.Count) Select(-1); };
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) visible = !visible;
    }

    void Select(int idx)
    {
        _selected = idx;
        if (_selected >= 0 && _selected < repo.AllReadOnly.Count)
        {
            var src = repo.AllReadOnly[_selected];
            _edit = new MemoryRecord
            {
                id = src.id,
                type = src.type,
                content = src.content,
                tags = src.tags != null ? src.tags.ToArray() : Array.Empty<string>(),
                timestamp = src.timestamp,
                importance = src.importance,
                occurrences = src.occurrences,
                embedding = src.embedding
            };
        }
        else _edit = null;
    }

    void OnGUI()
    {
        if (!visible || repo == null) return;

        GUI.depth = 0;
        GUI.enabled = true;

        var w = 640f;
        var h = Screen.height - 40f;
        GUILayout.BeginArea(new Rect(10, 10, w, h), GUI.skin.window);
        GUILayout.Label($"Memory Editor — {repo.AllReadOnly.Count} recuerdos  |  file: {repo.fileName}");

        // Toolbar
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Guardar ahora (JSON)")) repo.SaveNow();
        if (GUILayout.Button("Recargar"))
        {
            var loaded = MemPersistence.Load(repo.fileName);
            repo.ClearAll();
            foreach (var r in loaded) repo.Remember(r);
            Select(-1);
        }
        if (GUILayout.Button("Añadir nuevo"))
        {
            var r = new MemoryRecord
            {
                id = Guid.NewGuid().ToString(),
                type = MemoryType.Semantic,
                content = "nuevo recuerdo",
                tags = Array.Empty<string>(),
                timestamp = DateTime.UtcNow,
                importance = 0.5f,
                occurrences = 1
            };
            repo.Remember(r);
            Select(repo.AllReadOnly.Count - 1);
        }
        using (new GUILayout.HorizontalScope())
        {
            GUI.enabled = (_selected >= 0);
            if (GUILayout.Button("Borrar seleccionado"))
            {
                repo.RemoveAt(_selected);
                Select(-1);
            }
            GUI.enabled = true;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(6);

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(230));
        _scroll = GUILayout.BeginScrollView(_scroll, "box", GUILayout.Height(h - 120));
        for (int i = 0; i < repo.AllReadOnly.Count; i++)
        {
            var r = repo.AllReadOnly[i];
            var label = $"{i:00} | {r.type} | {(r.content.Length > 24 ? r.content.Substring(0, 24) + "…" : r.content)}";
            var prev = GUI.color;
            if (i == _selected) GUI.color = Color.cyan;
            if (GUILayout.Button(label, GUILayout.ExpandWidth(true))) Select(i);
            GUI.color = prev;
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.Height(h - 120));
        if (_edit != null)
        {
            GUILayout.Label($"ID: {_edit.id}");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Tipo:", GUILayout.Width(60));
            var types = Enum.GetNames(typeof(MemoryType));
            int idx = Array.IndexOf(types, _edit.type.ToString());
            idx = PopupRow(idx, types);
            _edit.type = (MemoryType)Enum.Parse(typeof(MemoryType), types[Mathf.Clamp(idx, 0, types.Length - 1)]);
            GUILayout.EndHorizontal();

            GUILayout.Label("Contenido:");
            GUI.SetNextControlName("CONTENT_TEXTAREA");
            _edit.content = GUILayout.TextArea(_edit.content, GUILayout.MinHeight(80));
            if (Event.current.type == EventType.MouseDown) FocusControlSafe();

            GUILayout.Label("Tags (coma-separados):");
            var tagsStr = string.Join(",", _edit.tags ?? Array.Empty<string>());
            tagsStr = GUILayout.TextField(tagsStr);
            _edit.tags = string.IsNullOrWhiteSpace(tagsStr)
                ? Array.Empty<string>()
                : tagsStr.Split(',').Select(t => t.Trim()).Where(t => t.Length > 0).ToArray();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Fecha (UTC ISO):", GUILayout.Width(110));
            var tsIso = _edit.timestamp.ToUniversalTime().ToString("o");
            var newIso = GUILayout.TextField(tsIso);
            if (newIso != tsIso && DateTime.TryParse(newIso, null, System.Globalization.DateTimeStyles.AdjustToUniversal, out var ts))
                _edit.timestamp = ts;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Importance: {_edit.importance:0.00}", GUILayout.Width(120));
            _edit.importance = GUILayout.HorizontalSlider(_edit.importance, 0f, 1f);
            GUILayout.Label($"Occur:", GUILayout.Width(52));
            var occStr = GUILayout.TextField(_edit.occurrences.ToString(), GUILayout.Width(40));
            if (int.TryParse(occStr, out var occ)) _edit.occurrences = Mathf.Max(0, occ);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            if (GUILayout.Button("Aplicar cambios"))
                repo.UpdateRecord(_selected, _edit);
        }
        else
        {
            GUILayout.FlexibleSpace();
            GUILayout.Label("Selecciona un recuerdo para editar.");
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    int PopupRow(int current, string[] options)
    {
        int chosen = current;
        GUILayout.BeginHorizontal();
        for (int i = 0; i < options.Length; i++)
        {
            var prev = GUI.color;
            if (i == current) GUI.color = Color.green;
            if (GUILayout.Button(options[i], GUILayout.Height(24))) chosen = i;
            GUI.color = prev;
        }
        GUILayout.EndHorizontal();
        return chosen;
    }

    void FocusControlSafe()
    {
#if UNITY_2019_1_OR_NEWER
        if (UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
#endif
        GUI.FocusControl("CONTENT_TEXTAREA");
    }
}
