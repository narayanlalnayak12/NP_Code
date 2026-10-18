using System.Security.Cryptography;

namespace SelectPhotos.Desktop;

public partial class Form1 : Form
{
    private readonly HashSet<string> _selectedPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _managedCopies = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _photoPaths = new();
    private int _currentIndex;

    private readonly Label _sourceLabel = new();
    private readonly Label _destinationLabel = new();
    private readonly Label _statusLabel = new();
    private readonly Label _selectionLabel = new();
    private readonly Label _positionLabel = new();
    private readonly Label _emptyLabel = new();
    private readonly Button _btnSource = new();
    private readonly Button _btnDestination = new();
    private readonly Button _btnPrevious = new();
    private readonly Button _btnNext = new();
    private readonly Button _btnToggle = new();
    private readonly Button _btnExport = new();
    private readonly PictureBox _pictureBox = new();
    private readonly NumericUpDown _targetCount = new();

    public Form1()
    {
        InitializeComponent();
        InitializeDesktopUi();
    }

    private void InitializeDesktopUi()
    {
        this.Text = "SelectPhotos Desktop";
        this.WindowState = FormWindowState.Maximized;
        this.MinimumSize = new Size(1000, 700);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(239, 243, 241);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Color.FromArgb(239, 243, 241),
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var header = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            ColumnStyles = { new ColumnStyle(SizeType.Percent, 80F), new ColumnStyle(SizeType.Percent, 20F) },
        };

        var title = new Label
        {
            Text = "SelectPhotos",
            Font = new Font("Segoe UI", 22, FontStyle.Bold),
            ForeColor = Color.FromArgb(31, 49, 45),
            AutoSize = true,
        };
        header.Controls.Add(title, 0, 0);

        var targetWrap = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            WrapContents = false,
        };
        targetWrap.Controls.Add(new Label { Text = "Selection target", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Padding = new Padding(0, 6, 8, 0) });
        _targetCount.Minimum = 1;
        _targetCount.Maximum = 10000;
        _targetCount.Value = 500;
        _targetCount.Width = 100;
        _targetCount.Font = new Font("Segoe UI", 10);
        _targetCount.ValueChanged += (_, _) => UpdateStatus();
        targetWrap.Controls.Add(_targetCount);
        header.Controls.Add(targetWrap, 1, 0);
        root.Controls.Add(header, 0, 0);

        var folderPanel = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 2,
            Dock = DockStyle.Top,
            Padding = new Padding(12),
            Margin = new Padding(0, 8, 0, 12),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
        };
        folderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
        folderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        folderPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        folderPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        folderPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var sourceTitle = new Label { Text = "Source", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Left, Margin = new Padding(4, 8, 8, 8) };
        _sourceLabel.Text = "No source folder selected";
        _sourceLabel.AutoEllipsis = true;
        _sourceLabel.Dock = DockStyle.Fill;
        _sourceLabel.TextAlign = ContentAlignment.MiddleLeft;

        var destinationTitle = new Label { Text = "Destination", AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold), Anchor = AnchorStyles.Left, Margin = new Padding(4, 8, 8, 8) };
        _destinationLabel.Text = "No destination folder selected";
        _destinationLabel.AutoEllipsis = true;
        _destinationLabel.Dock = DockStyle.Fill;
        _destinationLabel.TextAlign = ContentAlignment.MiddleLeft;

        _btnSource.Text = "Browse...";
        _btnSource.Click += (_, _) => PickFolder(true);
        _btnDestination.Text = "Browse...";
        _btnDestination.Click += (_, _) => PickFolder(false);
        StyleButton(_btnSource, Color.FromArgb(231, 237, 234), Color.FromArgb(31, 49, 45));
        StyleButton(_btnDestination, Color.FromArgb(231, 237, 234), Color.FromArgb(31, 49, 45));

        folderPanel.Controls.Add(sourceTitle, 0, 0);
        folderPanel.Controls.Add(_btnSource, 1, 0);
        folderPanel.Controls.Add(_sourceLabel, 2, 0);
        folderPanel.Controls.Add(destinationTitle, 0, 1);
        folderPanel.Controls.Add(_btnDestination, 1, 1);
        folderPanel.Controls.Add(_destinationLabel, 2, 1);

        root.Controls.Add(folderPanel, 0, 1);

        var viewerPanel = new Panel
        {
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Black,
            Dock = DockStyle.Fill,
        };

        _pictureBox.Dock = DockStyle.Fill;
        _pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
        _pictureBox.BackColor = Color.Black;
        viewerPanel.Controls.Add(_pictureBox);
        _emptyLabel.Text = "Choose a source folder to begin";
        _emptyLabel.Dock = DockStyle.Fill;
        _emptyLabel.TextAlign = ContentAlignment.MiddleCenter;
        _emptyLabel.Font = new Font("Segoe UI", 16, FontStyle.Regular);
        _emptyLabel.ForeColor = Color.FromArgb(211, 220, 216);
        _emptyLabel.BackColor = Color.Transparent;
        viewerPanel.Controls.Add(_emptyLabel);
        _emptyLabel.BringToFront();
        root.Controls.Add(viewerPanel, 0, 2);

        var controlsRow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(4, 10, 4, 6), WrapContents = false };
        _btnPrevious.Text = "Previous";
        _btnPrevious.Click += (_, _) => MovePhoto(-1);
        _btnNext.Text = "Next";
        _btnNext.Click += (_, _) => MovePhoto(1);
        _btnToggle.Text = "Select Photo";
        _btnToggle.Click += (_, _) => ToggleSelected();
        _btnExport.Text = "Export Selected";
        _btnExport.Click += (_, _) => ExportSelected();
        StyleButton(_btnPrevious, Color.FromArgb(231, 237, 234), Color.FromArgb(31, 49, 45));
        StyleButton(_btnNext, Color.FromArgb(231, 237, 234), Color.FromArgb(31, 49, 45));
        StyleButton(_btnToggle, Color.FromArgb(43, 105, 88), Color.White);
        StyleButton(_btnExport, Color.FromArgb(31, 49, 45), Color.White);

        controlsRow.Controls.Add(_btnPrevious);
        controlsRow.Controls.Add(_btnNext);
        controlsRow.Controls.Add(_btnToggle);
        controlsRow.Controls.Add(_btnExport);
        root.Controls.Add(controlsRow, 0, 3);

        var footer = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Dock = DockStyle.Top,
            Padding = new Padding(12),
            Margin = new Padding(0),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle,
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

        _selectionLabel.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        _selectionLabel.ForeColor = Color.FromArgb(31, 49, 45);
        _selectionLabel.AutoSize = true;
        _selectionLabel.Text = "Selected: 0 / 0";

        _positionLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        _positionLabel.ForeColor = Color.FromArgb(75, 91, 85);
        _positionLabel.AutoSize = true;
        _positionLabel.Text = "Photo 0 of 0";

        _statusLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
        _statusLabel.ForeColor = Color.FromArgb(75, 91, 85);
        _statusLabel.AutoSize = true;
        _statusLabel.Text = "Ready";

        footer.Controls.Add(_selectionLabel, 0, 0);
        footer.Controls.Add(_positionLabel, 1, 0);
        footer.Controls.Add(_statusLabel, 0, 1);
        root.Controls.Add(footer, 0, 4);

        this.Controls.Add(root);
        UpdateButtons();
    }

    private void PickFolder(bool isSource)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = isSource ? "Select source folder" : "Select destination folder",
            RootFolder = Environment.SpecialFolder.MyComputer,
            ShowNewFolderButton = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var path = dialog.SelectedPath;
            if (isSource)
            {
                _sourceLabel.Text = path;
                LoadPhotos();
            }
            else
            {
                _destinationLabel.Text = path;
                if (!string.IsNullOrWhiteSpace(_sourceLabel.Text) && !string.Equals(_sourceLabel.Text, "No source folder selected", StringComparison.OrdinalIgnoreCase))
                {
                    LoadPhotos();
                }
            }

            UpdateButtons();
        }
    }

    private void LoadPhotos()
    {
        var source = _sourceLabel.Text;
        if (string.IsNullOrWhiteSpace(source) || source == "No source folder selected" || !Directory.Exists(source))
        {
            _photoPaths.Clear();
            _currentIndex = 0;
            _pictureBox.Image = null;
            _selectionLabel.Text = "Selected: 0 / 0";
            _positionLabel.Text = "Photo 0 of 0";
            return;
        }

        var destination = _destinationLabel.Text;
        var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".bmp", ".tif", ".tiff"
        };

        var files = Directory.EnumerateFiles(source, "*.*", SearchOption.AllDirectories)
            .Where(path => supported.Contains(Path.GetExtension(path)))
            .Where(path => string.IsNullOrWhiteSpace(destination) || destination == "No destination folder selected" || !IsChildOf(path, destination))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        _photoPaths.Clear();
        _photoPaths.AddRange(files);
        _currentIndex = Math.Min(_currentIndex, Math.Max(_photoPaths.Count - 1, 0));
        RenderCurrentPhoto();
        UpdateStatus();
    }

    private void RenderCurrentPhoto()
    {
        if (_photoPaths.Count == 0)
        {
            _pictureBox.Image = null;
            _emptyLabel.Visible = true;
            _positionLabel.Text = "Photo 0 of 0";
            _selectionLabel.Text = "Selected: 0 / 0";
            _btnToggle.Text = "Select Photo";
            return;
        }

        var path = _photoPaths[_currentIndex];
        try
        {
            _pictureBox.ImageLocation = path;
            _pictureBox.Load();
            _emptyLabel.Visible = false;
        }
        catch
        {
            _pictureBox.Image = null;
            _emptyLabel.Text = "This image could not be previewed";
            _emptyLabel.Visible = true;
        }

        var selected = _selectedPaths.Contains(path);
        _btnToggle.Text = selected ? "Selected ✓" : "Select Photo";
        _positionLabel.Text = $"Photo {_currentIndex + 1} of {_photoPaths.Count} - {Path.GetFileName(path)}";
        _selectionLabel.Text = $"Selected: {_selectedPaths.Count} / {_photoPaths.Count}";
        UpdateButtons();
    }

    private void MovePhoto(int delta)
    {
        if (_photoPaths.Count == 0)
        {
            return;
        }

        _currentIndex = Math.Clamp(_currentIndex + delta, 0, _photoPaths.Count - 1);
        RenderCurrentPhoto();
    }

    private void ToggleSelected()
    {
        if (_photoPaths.Count == 0)
        {
            return;
        }

        var current = _photoPaths[_currentIndex];
        if (_selectedPaths.Contains(current))
        {
            DeselectPhoto(current);
        }
        else
        {
            SelectPhoto(current);
        }
    }

    private void SelectPhoto(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(_destinationLabel.Text) || _destinationLabel.Text == "No destination folder selected")
        {
            MessageBox.Show(this, "Please choose a destination folder first.", "Select destination folder", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var destinationPath = EnsureDestinationFile(sourcePath, _destinationLabel.Text);
        if (destinationPath is null)
        {
            _statusLabel.Text = "Selection failed: destination not available.";
            return;
        }

        _selectedPaths.Add(sourcePath);
        _managedCopies[sourcePath] = destinationPath;
        RenderCurrentPhoto();
        _statusLabel.Text = $"Selected: {Path.GetFileName(sourcePath)}";
    }

    private void DeselectPhoto(string sourcePath)
    {
        if (!_managedCopies.TryGetValue(sourcePath, out var managedPath) || string.IsNullOrWhiteSpace(managedPath) || !File.Exists(managedPath))
        {
            _selectedPaths.Remove(sourcePath);
            _managedCopies.Remove(sourcePath);
            RenderCurrentPhoto();
            return;
        }

        if (!FilesMatch(sourcePath, managedPath))
        {
            MessageBox.Show(this, "The managed destination copy was changed externally. It was not deleted automatically.", "Safety warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _statusLabel.Text = "Managed copy changed externally; no automatic deletion.";
            return;
        }

        File.Delete(managedPath);
        _selectedPaths.Remove(sourcePath);
        _managedCopies.Remove(sourcePath);
        RenderCurrentPhoto();
        _statusLabel.Text = $"Deselected: {Path.GetFileName(sourcePath)}";
    }

    private string? EnsureDestinationFile(string sourcePath, string destinationRoot)
    {
        if (!Directory.Exists(destinationRoot))
        {
            return null;
        }

        var extension = Path.GetExtension(sourcePath);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(sourcePath);
        var candidate = Path.Combine(destinationRoot, Path.GetFileName(sourcePath));

        if (!File.Exists(candidate) || FilesMatch(sourcePath, candidate))
        {
            if (!File.Exists(candidate))
            {
                CopyFileSafely(sourcePath, candidate);
            }
            _managedCopies[sourcePath] = candidate;
            return candidate;
        }

        var unique = Path.Combine(destinationRoot, $"{nameWithoutExtension}_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}{extension}");
        CopyFileSafely(sourcePath, unique);
        _managedCopies[sourcePath] = unique;
        return unique;
    }

    private void ExportSelected()
    {
        if (_selectedPaths.Count == 0)
        {
            _statusLabel.Text = "No selected photos to export.";
            return;
        }

        foreach (var selected in _selectedPaths.ToList())
        {
            if (!File.Exists(selected))
            {
                _statusLabel.Text = "One or more selected source files are missing.";
                continue;
            }

            if (!_managedCopies.TryGetValue(selected, out var target) || string.IsNullOrWhiteSpace(target) || !File.Exists(target))
            {
                EnsureDestinationFile(selected, _destinationLabel.Text);
            }
        }

        _statusLabel.Text = $"Export complete: {_selectedPaths.Count} selected.";
        RenderCurrentPhoto();
    }

    private static void CopyFileSafely(string sourceFile, string destinationFile)
    {
        var temp = destinationFile + ".tmp" + Guid.NewGuid().ToString("N");
        File.Copy(sourceFile, temp, overwrite: false);
        if (!FilesMatch(sourceFile, temp))
        {
            File.Delete(temp);
            throw new IOException($"Copy verification failed for {sourceFile}");
        }

        if (File.Exists(destinationFile))
        {
            var hash = ComputeHash(destinationFile);
            var sourceHash = ComputeHash(sourceFile);
            if (!string.Equals(hash, sourceHash, StringComparison.OrdinalIgnoreCase))
            {
                File.Move(temp, destinationFile, overwrite: false);
                return;
            }
        }

        File.Move(temp, destinationFile, overwrite: false);
    }

    private static bool FilesMatch(string sourcePath, string targetPath)
    {
        if (!File.Exists(sourcePath) || !File.Exists(targetPath))
        {
            return false;
        }

        return FileInfoMatches(sourcePath, targetPath) && string.Equals(ComputeHash(sourcePath), ComputeHash(targetPath), StringComparison.OrdinalIgnoreCase);
    }

    private static bool FileInfoMatches(string sourcePath, string targetPath)
    {
        var sourceInfo = new FileInfo(sourcePath);
        var targetInfo = new FileInfo(targetPath);
        return sourceInfo.Length == targetInfo.Length;
    }

    private static string ComputeHash(string path)
    {
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    private static bool IsChildOf(string path, string root)
    {
        try
        {
            var fullPath = Path.GetFullPath(path);
            var fullRoot = Path.GetFullPath(root);
            return fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private void UpdateButtons()
    {
        _btnPrevious.Enabled = _photoPaths.Count > 0 && _currentIndex > 0;
        _btnNext.Enabled = _photoPaths.Count > 1 && _currentIndex < _photoPaths.Count - 1;
        _btnToggle.Enabled = _photoPaths.Count > 0 && !string.IsNullOrWhiteSpace(_destinationLabel.Text) && _destinationLabel.Text != "No destination folder selected";
        _btnExport.Enabled = _selectedPaths.Count > 0;
    }

    private void UpdateStatus()
    {
        var target = (int)_targetCount.Value;
        var selectedCount = _selectedPaths.Count;
        _selectionLabel.Text = $"Selected: {selectedCount} / {_photoPaths.Count}";
        var progressText = selectedCount >= target ? "Target reached" : $"{selectedCount} / {target} selected";
        _statusLabel.Text = progressText;
    }

    private static void StyleButton(Button button, Color backColor, Color foreColor)
    {
        button.AutoSize = true;
        button.MinimumSize = new Size(110, 38);
        button.Padding = new Padding(10, 5, 10, 5);
        button.Margin = new Padding(4);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = backColor;
        button.ForeColor = foreColor;
        button.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        button.Cursor = Cursors.Hand;
    }
}
