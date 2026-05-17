using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RT950toCSV.App;

public sealed class MainForm : Form
{
    private readonly TextBox    _exportInput    = CreatePathBox();
    private readonly TextBox    _exportOutput   = CreatePathBox();
    private readonly TextBox    _importCsv      = CreatePathBox();
    private readonly TextBox    _importOutput   = CreatePathBox();
    private readonly TextBox    _importTemplate = CreatePathBox();
    private readonly ToolTip    _toolTip        = new();
    private readonly RichTextBox _log;

    public MainForm()
    {
        Text        = "RT950 Pro ↔ CSV Converter";
        MinimumSize = new Size(700, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = SystemIcons.Application;

        var outer = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 1,
            RowCount    = 3,
            Padding     = new Padding(10),
            AutoSize    = true
        };
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        outer.Controls.Add(BuildExportPanel(), 0, 0);
        outer.Controls.Add(BuildImportPanel(), 0, 1);

        _log = new RichTextBox
        {
            Dock      = DockStyle.Fill,
            ReadOnly  = true,
            BackColor = Color.Black,
            ForeColor = Color.LightGreen,
            Font      = new Font("Consolas", 9f),
            Margin    = new Padding(0, 8, 0, 0)
        };
        outer.Controls.Add(_log, 0, 2);

        Controls.Add(outer);
    }

    // ── Export panel ────────────────────────────────────────────────────────

    private GroupBox BuildExportPanel()
    {
        var grid = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 3,
            AutoSize    = true,
            Padding     = new Padding(6)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

        grid.Controls.Add(new Label { Text = "Radio.dat :", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 0);
        grid.Controls.Add(_exportInput, 1, 0);
        grid.Controls.Add(BrowseButton("Browse…", BrowseDat, _exportInput), 2, 0);

        grid.Controls.Add(new Label { Text = "Output CSV :", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 1);
        grid.Controls.Add(_exportOutput, 1, 1);
        grid.Controls.Add(BrowseButton("Save CSV…", SaveCsv, _exportOutput), 2, 1);


        var btn = new Button { Text = "Export  DAT → CSV", Dock = DockStyle.Fill, Height = 32 };
        btn.Click += OnExportClicked;
        grid.Controls.Add(btn, 2, 2);

        for (var i = 0; i < grid.RowCount; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        return new GroupBox { Text = "Export", Dock = DockStyle.Fill, AutoSize = true, Controls = { grid } };
    }

    // ── Import panel ────────────────────────────────────────────────────────

    private GroupBox BuildImportPanel()
    {
        var grid = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 3,
            AutoSize    = true,
            Padding     = new Padding(6),
            Margin      = new Padding(0, 8, 0, 0)
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));

        // Row 0: CSV input
        grid.Controls.Add(new Label { Text = "CHIRP CSV :", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 0);
        grid.Controls.Add(_importCsv, 1, 0);
        grid.Controls.Add(BrowseButton("Browse…", BrowseCsv, _importCsv), 2, 0);

        // Row 1: output
        grid.Controls.Add(new Label { Text = "Output .dat :", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 1);
        grid.Controls.Add(_importOutput, 1, 1);
        grid.Controls.Add(BrowseButton("Save .dat…", SaveDat, _importOutput), 2, 1);

        // Row 2: template
        var templateLabel = new Label { Text = "Template .dat :", AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top };
        _toolTip.SetToolTip(templateLabel, "Use your original Radio.dat from the radio.\n" +
            "It preserves VFO, DTMF, scan lists, and all settings\n" +
            "not stored in the CSV. The channel slots will be replaced\n" +
            "by the CSV contents; everything else is kept intact.");
        _importTemplate.PlaceholderText = "Original Radio.dat from your radio";
        grid.Controls.Add(templateLabel, 0, 2);
        grid.Controls.Add(_importTemplate, 1, 2);
        grid.Controls.Add(BrowseButton("Browse…", BrowseDat, _importTemplate), 2, 2);

        // Row 3: import button
        var btn = new Button { Text = "Import  CSV → DAT", Dock = DockStyle.Fill, Height = 32 };
        btn.Click += OnImportClicked;
        grid.Controls.Add(btn, 2, 3);

        for (var i = 0; i < 4; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        return new GroupBox { Text = "Import", Dock = DockStyle.Fill, AutoSize = true, Controls = { grid } };
    }

    // ── Event handlers ──────────────────────────────────────────────────────

    private void OnExportClicked(object? sender, EventArgs e)
    {
        if (!File.Exists(_exportInput.Text))
        {
            Warn("Select a valid Radio.dat file.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_exportOutput.Text))
        {
            Warn("Choose an output CSV path.");
            return;
        }

        RunWithProgress(() =>
        {
            var count = ConverterCore.ExportToChirpCsv(_exportInput.Text, _exportOutput.Text);
            Log($"\u2713 Exported {count} channels \u2192 {_exportOutput.Text}");
        }, "Export");
    }

    private void OnImportClicked(object? sender, EventArgs e)
    {
        if (!File.Exists(_importCsv.Text))      { Warn("Select a channels CSV file."); return; }
        if (!File.Exists(_importTemplate.Text)) { Warn("Select a template .dat file (your original Radio.dat)."); return; }
        if (string.IsNullOrWhiteSpace(_importOutput.Text)) { Warn("Choose an output .dat path."); return; }

        var csvPath      = _importCsv.Text;
        var outputPath   = _importOutput.Text;
        var templatePath = _importTemplate.Text;

        RunWithProgress(() =>
        {
            var (imported, skipped) = ConverterCore.ImportFromChirpCsv(csvPath, outputPath, templatePath);
            if (skipped > 0)
                Log($"  Note: {skipped} row(s) skipped (Location > {RT950toCSV.Data.Codeplug.RadioCodeplug.ChannelSlots})", error: true);
            Log($"\u2713 Imported {imported} channels \u2192 {outputPath}");
        }, "Import");
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private void RunWithProgress(Action action, string operationName)
    {
        try
        {
            Cursor = Cursors.WaitCursor;
            Log($"Starting {operationName}…");
            action();
        }
        catch (Exception ex)
        {
            Log($"✗ {operationName} failed: {ex.Message}", error: true);
            MessageBox.Show(ex.Message, $"{operationName} failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Cursor = Cursors.Default;
        }
    }

    private void Log(string message, bool error = false)
    {
        _log.SelectionStart  = _log.TextLength;
        _log.SelectionLength = 0;
        _log.SelectionColor  = error ? Color.Salmon : Color.LightGreen;
        _log.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        _log.ScrollToCaret();
    }

    private static void Warn(string message) =>
        MessageBox.Show(message, "Missing input", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static TextBox CreatePathBox() =>
        new() { Dock = DockStyle.Fill, PlaceholderText = "Select a file…" };

    private static Button BrowseButton(string label, Func<string?> picker, TextBox target)
    {
        var btn = new Button { Text = label, Dock = DockStyle.Fill };
        btn.Click += (_, _) =>
        {
            var path = picker();
            if (!string.IsNullOrEmpty(path))
                target.Text = path;
        };
        return btn;
    }

    // ── File dialogs ─────────────────────────────────────────────────────────

    private static string? BrowseDat()
    {
        using var dlg = new OpenFileDialog { Filter = "CPS data (*.dat)|*.dat|All files (*.*)|*.*", Title = "Select CPS data file" };
        return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
    }

    private static string? SaveDat()
    {
        using var dlg = new SaveFileDialog { Filter = "CPS data (*.dat)|*.dat|All files (*.*)|*.*", Title = "Save CPS data file", AddExtension = true, DefaultExt = ".dat" };
        return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
    }

    private static string? BrowseCsv()
    {
        using var dlg = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", Title = "Select channels CSV" };
        return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
    }

    private static string? SaveCsv()
    {
        using var dlg = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", Title = "Save channels CSV", AddExtension = true, DefaultExt = ".csv" };
        return dlg.ShowDialog() == DialogResult.OK ? dlg.FileName : null;
    }
}
