using FrameSync;
using LS;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LS.Editor
{
    public class LSWorldDebugWindow : EditorWindow
    {
        private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private readonly Dictionary<string, bool> m_foldouts = new();
        private Vector2 m_scroll;
        private bool m_autoRefresh = true;
        private int m_maxEntriesPerDictionary = 80;

        [MenuItem("LS/Debug/LSWorld Monitor")]
        public static void Open()
        {
            GetWindow<LSWorldDebugWindow>("LSWorld Monitor");
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (m_autoRefresh)
                Repaint();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                m_autoRefresh = GUILayout.Toggle(m_autoRefresh, "Auto Refresh", EditorStyles.toolbarButton, GUILayout.Width(100));

                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70)))
                    Repaint();

                GUILayout.FlexibleSpace();
                GUILayout.Label("Max Entries", GUILayout.Width(70));
                m_maxEntriesPerDictionary = Mathf.Max(1, EditorGUILayout.IntField(m_maxEntriesPerDictionary, GUILayout.Width(60)));
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to inspect the active LSWorld instance.", MessageType.Info);
                return;
            }

            GameEntry entry = FindObjectOfType<GameEntry>();
            if (entry == null || entry.world == null)
            {
                EditorGUILayout.HelpBox("No active GameEntry.world found.", MessageType.Warning);
                return;
            }

            LSWorld world = entry.world;

            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
            DrawScalarState(world);
            EditorGUILayout.Space(8);
            DrawDictionaryFields(world);
            EditorGUILayout.EndScrollView();
        }

        private void DrawScalarState(LSWorld world)
        {
            EditorGUILayout.LabelField("Frame State", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawFieldValue(world, "m_latestServerFrame", "Latest Server Frame");
                DrawFieldValue(world, "m_localExecutedFrame", "Local Executed Frame");
                DrawFieldValue(world, "m_authoritativeExecutedFrame", "Authoritative Executed Frame");
                DrawFieldValue(world, "m_inputTargetFrame", "Input Target Frame");
                DrawFieldValue(world, "m_hasReceivedServerFrame", "Has Received Server Frame");
                EditorGUILayout.LabelField("Public LatestServerFrame", world.LatestServerFrame.ToString());
                EditorGUILayout.LabelField("Public LocalExecutedFrame", world.LocalExecutedFrame.ToString());
            }
        }

        private void DrawDictionaryFields(LSWorld world)
        {
            EditorGUILayout.LabelField("Dictionaries", EditorStyles.boldLabel);

            FieldInfo[] fields = typeof(LSWorld)
                .GetFields(InstanceFields)
                .Where(field => typeof(IDictionary).IsAssignableFrom(field.FieldType))
                .OrderBy(field => field.Name)
                .ToArray();

            foreach (FieldInfo field in fields)
            {
                IDictionary dictionary = field.GetValue(world) as IDictionary;
                DrawDictionary(field.Name, dictionary);
            }
        }

        private void DrawDictionary(string name, IDictionary dictionary)
        {
            int count = dictionary?.Count ?? 0;
            string title = $"{name} ({count})";

            if (!m_foldouts.ContainsKey(name))
                m_foldouts[name] = true;

            m_foldouts[name] = EditorGUILayout.Foldout(m_foldouts[name], title, true);
            if (!m_foldouts[name])
                return;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (dictionary == null || dictionary.Count == 0)
                {
                    EditorGUILayout.LabelField("empty");
                    return;
                }

                int shown = 0;
                foreach (DictionaryEntry entry in OrderedEntries(dictionary))
                {
                    if (shown >= m_maxEntriesPerDictionary)
                    {
                        EditorGUILayout.LabelField($"... {dictionary.Count - shown} more");
                        break;
                    }

                    DrawEntry(entry);
                    shown++;
                }
            }
        }

        private IEnumerable<DictionaryEntry> OrderedEntries(IDictionary dictionary)
        {
            List<DictionaryEntry> entries = new();
            foreach (DictionaryEntry entry in dictionary)
                entries.Add(entry);

            return entries.OrderBy(entry => entry.Key is int intKey ? intKey : 0)
                .ThenBy(entry => entry.Key?.ToString());
        }

        private void DrawEntry(DictionaryEntry entry)
        {
            string key = entry.Key?.ToString() ?? "null";
            object value = entry.Value;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"[{key}]", EditorStyles.boldLabel);
                DrawValue(value);
            }
        }

        private void DrawValue(object value)
        {
            switch (value)
            {
                case null:
                    EditorGUILayout.LabelField("null");
                    break;

                case FrameInput frameInput:
                    DrawFrameInput(frameInput);
                    break;

                case PlayerInput playerInput:
                    EditorGUILayout.LabelField(FormatPlayerInput(playerInput));
                    break;

                case WorldSnapshot snapshot:
                    DrawWorldSnapshot(snapshot);
                    break;

                default:
                    EditorGUILayout.LabelField(value.ToString());
                    break;
            }
        }

        private void DrawFrameInput(FrameInput frameInput)
        {
            EditorGUILayout.LabelField($"Frame: {frameInput.FrameNumber}, Inputs: {frameInput.Inputs.Count}");

            foreach (PlayerInput input in frameInput.Inputs.OrderBy(input => input.PlayerId))
                EditorGUILayout.LabelField("  " + FormatPlayerInput(input));
        }

        private string FormatPlayerInput(PlayerInput input)
        {
            return $"Player={input.PlayerId}, TargetFrame={input.TargetFrame}, X={input.InputX}, Y={input.InputY}, Jump={input.Jump}";
        }

        private void DrawWorldSnapshot(WorldSnapshot snapshot)
        {
            EditorGUILayout.LabelField($"Frame: {snapshot.frame}, Objects: {snapshot.dic_ID_objectSnapshot.Count}");

            foreach (var kv in snapshot.dic_ID_objectSnapshot.OrderBy(kv => kv.Key))
            {
                ObjectSnapshot obj = kv.Value;
                EditorGUILayout.LabelField($"  Player={obj.PlayerID}, PosX={obj.PosX}, PosY={obj.PosY}");
            }
        }

        private void DrawFieldValue(LSWorld world, string fieldName, string label)
        {
            FieldInfo field = typeof(LSWorld).GetField(fieldName, InstanceFields);
            if (field == null)
            {
                EditorGUILayout.LabelField(label, $"Missing field: {fieldName}");
                return;
            }

            object value = field?.GetValue(world);
            EditorGUILayout.LabelField(label, value?.ToString() ?? "null");
        }
    }
}
