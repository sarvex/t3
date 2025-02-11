﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using ImGuiNET;
using T3.Core.Logging;
using T3.Core.Utils;
using T3.Editor.Gui.Graph;
using T3.Editor.Gui.Graph.Interaction;
using T3.Editor.Gui.Styling;
using T3.Editor.Gui.UiHelpers;

namespace T3.Editor.Gui.Windows
{
    /// <summary>
    /// Renders the <see cref="ConsoleLogWindow"/>
    /// </summary>
    public class ConsoleLogWindow : Window, ILogWriter
    {
        public ConsoleLogWindow()
        {
            Log.AddWriter(this);
            Config.Title = "Console";
            Config.Visible = true;
        }

        private readonly List<LogEntry> _filteredEntries = new(1000);

        protected override void DrawContent()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.One * 5);
            {
                CustomComponents.ToggleButton("Scroll", ref _shouldScrollToBottom, Vector2.Zero);
                ImGui.SameLine();

                ImGui.SetNextWindowSize(new Vector2(500, 400), ImGuiCond.FirstUseEver);
                if (ImGui.Button("Clear"))
                {
                    lock (_logEntries)
                    {
                        _logEntries.Clear();
                    }
                }

                ImGui.SameLine();

                if (ImGui.Button("Copy"))
                {
                    lock (_logEntries)
                    {
                        var sb = new StringBuilder();
                        foreach (var entry in _logEntries)
                        {
                            sb.Append($"{(entry.TimeStamp - _startTime).Ticks / 10000000f:  0.000}");
                            sb.Append('\t');
                            sb.Append(entry.Level);
                            sb.Append('\t');
                            sb.Append(entry.Message);
                            sb.Append('\n');
                        }

                        Clipboard.SetText(sb.ToString());
                    }
                }

                ImGui.SameLine();
                CustomComponents.DrawSearchField("Filter", ref _filterString);

                ImGui.Separator();
                ImGui.BeginChild("scrolling");
                {
                    if (_logEntries == null)
                        return;

                    lock (_logEntries)
                    {
                        var items = _logEntries;
                        if (FilterIsActive)
                        {
                            _filteredEntries.Clear();
                            foreach (var e in _logEntries)
                            {
                                if (!e.Message.Contains(_filterString))
                                    continue;

                                _filteredEntries.Add(e);
                            }

                            items = _filteredEntries;
                        }

                        if (ImGui.IsWindowHovered() && ImGui.GetIO().MouseWheel != 0)
                        {
                            _shouldScrollToBottom = false;
                        }

                        unsafe
                        {
                            var clipperData = new ImGuiListClipper();
                            var listClipperPtr = new ImGuiListClipperPtr(&clipperData);

                            listClipperPtr.Begin(items.Count, ImGui.GetTextLineHeightWithSpacing());
                            while (listClipperPtr.Step())
                            {
                                for (var i = listClipperPtr.DisplayStart; i < listClipperPtr.DisplayEnd; ++i)
                                {
                                    if (i < 0 || i >= items.Count)
                                        continue;

                                    DrawEntry(items[i]);
                                }
                            }

                            listClipperPtr.End();
                        }
                    }

                    ImGui.TextColored(Color.Gray, "---"); // Indicator for end
                    if (_shouldScrollToBottom)
                    {
                        ImGui.SetScrollY(ImGui.GetScrollMaxY() + ImGui.GetFrameHeight());
                        _isAtBottom = true;
                    }
                    else
                    {
                        _isAtBottom = ImGui.GetScrollY() >= ImGui.GetScrollMaxY() - ImGui.GetWindowHeight();
                    }
                }

                ImGui.EndChild();
            }

            ImGui.PopStyleVar();
        }

        private static double _lastLimeTime;

        public static void DrawEntry(LogEntry entry)
        {
            var entryLevel = entry.Level;

            var time = entry.SecondsSinceStart;
            var dt = time - _lastLimeTime;

            var fade = 1f;
            var frameFraction = (float)dt / (1.5f / 60f);
            if (frameFraction < 1)
            {
                fade = MathUtils.RemapAndClamp(frameFraction, 0, 1, 0.2f, 0.8f);
            }

            var timeColor = T3Style.Colors.Text.Fade(fade);
            var timeLabel = $" {time:0.000}";
            var timeLabelSize = ImGui.CalcTextSize(timeLabel);
            ImGui.SetCursorPosX(80 - timeLabelSize.X);
            ImGui.TextColored(timeColor, timeLabel);
            _lastLimeTime = time;
            ImGui.SameLine(90);

            var color = GetColorForLogLevel(entryLevel)
               .Fade(FrameStats.Last.HoveredIds.Contains(entry.SourceId) ? 1 : 0.6f);
            ImGui.TextColored(color, entry.Message);

            if (!IsLineHovered() || !(entry.SourceIdPath?.Length > 1))
                return;

            FrameStats.AddHoveredId(entry.SourceId);
            var childIdPath = entry.SourceIdPath.ToList();
            var hoveredSourceInstance = NodeOperations.GetInstanceFromIdPath(childIdPath);
            ImGui.BeginTooltip();
            {
                if (hoveredSourceInstance == null)
                {
                    ImGui.Text("Source Instance of message not longer valid");
                }
                else
                {
                    ImGui.TextColored(T3Style.Colors.TextMuted, "from ");

                    foreach (var p in NodeOperations.GetReadableInstancePath(childIdPath))
                    {
                        ImGui.SameLine();
                        ImGui.TextColored(T3Style.Colors.TextMuted, " / ");

                        ImGui.SameLine();
                        ImGui.Text(p);
                    }
                }
            }
            ImGui.EndTooltip();

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                GraphWindow.GetPrimaryGraphWindow().GraphCanvas.OpenAndFocusInstance(childIdPath);
            }
        }

        public static Color GetColorForLogLevel(LogEntry.EntryLevel entryLevel)
        {
            return _colorForLogLevel.TryGetValue(entryLevel, out var color)
                       ? color
                       : T3Style.Colors.TextMuted;
        }

        public override List<Window> GetInstances()
        {
            return new List<Window>();
        }

        private static bool IsLineHovered()
        {
            if (!ImGui.IsWindowHovered())
                return false;

            var min = new Vector2(ImGui.GetWindowPos().X, ImGui.GetItemRectMin().Y);
            var size = new Vector2(ImGui.GetWindowWidth() - 10, ImGui.GetItemRectSize().Y + LinePadding - 2);
            var lineRect = new ImRect(min, min + size);
            return lineRect.Contains(ImGui.GetMousePos());
        }

        private static readonly Dictionary<LogEntry.EntryLevel, Color> _colorForLogLevel = new Dictionary<LogEntry.EntryLevel, Color>()
                                                                                               {
                                                                                                   { LogEntry.EntryLevel.Debug, new Color(1, 1, 1, 0.6f) },
                                                                                                   { LogEntry.EntryLevel.Info, new Color(1, 1, 1, 0.6f) },
                                                                                                   {
                                                                                                       LogEntry.EntryLevel.Warning,
                                                                                                       new Color(1, 0.5f, 0.5f, 0.9f)
                                                                                                   },
                                                                                                   { LogEntry.EntryLevel.Error, new Color(1, 0.2f, 0.2f, 1f) },
                                                                                               };

        public void ProcessEntry(LogEntry entry)
        {
            lock (_logEntries)
            {
                _logEntries.Add(entry);
            }

            if (_isAtBottom)
            {
                _shouldScrollToBottom = true;
            }
        }

        public void Dispose()
        {
            _logEntries = null;
        }


        public LogEntry.EntryLevel Filter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private bool FilterIsActive => !string.IsNullOrEmpty(_filterString);
        private const float LinePadding = 3;
        private List<LogEntry> _logEntries = new();
        private bool _shouldScrollToBottom = true;
        private string _filterString = "";
        private bool _isAtBottom = true;
        private readonly DateTime _startTime = DateTime.Now;
    }
}