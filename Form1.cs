namespace NotepadClone;

using System.IO;
using System.Text;
using System.Windows.Forms;

public partial class Form1 : Form
{
    // کادر اصلی ویرایش متن
    private readonly RichTextBox editor = new();

    // درخت پوشه‌ها و فایل‌ها
    private readonly TreeView fileTree = new();

    // مسیر فایلی که در حال حاضر باز است؛ برای Save استفاده می‌شود
    private string? currentFilePath;

    // سازنده: این کد هنگام ساخته‌شدن پنجره اجرا می‌شود
    public Form1()
    {
        InitializeComponent();

        // تنظیمات پنجره
        Text = "Notepad Clone";
        Width = 900;
        Height = 600;

        // تنظیمات کادر ویرایش متن
        editor.Dock = DockStyle.Fill;
        editor.BorderStyle = BorderStyle.None;
        editor.Font = new System.Drawing.Font("Consolas", 12);
        editor.AcceptsTab = true;
        editor.WordWrap = false;
        editor.ScrollBars = RichTextBoxScrollBars.Both;

        // تنظیمات درخت فایل‌ها
        fileTree.Dock = DockStyle.Fill;

        // با بازشدن یک پوشه، محتویاتش بارگذاری می‌شود
        fileTree.BeforeExpand += (_, e) =>
            {
                if (e.Node is not null)
                {
                    LoadFolderContents(e.Node);
                }
            };


        // با دوبار کلیک روی فایل، آن را باز می‌کنیم
        fileTree.NodeMouseDoubleClick += (_, e) =>
            {
                if (e.Node is not null)
                {
                    OpenSelectedFile(e.Node);
                }
            };


        // ساخت منوی بالای پنجره
        var menu = new MenuStrip
        {
            Dock = DockStyle.Top
        };

        var fileMenu = new ToolStripMenuItem("File");

        var openItem = new ToolStripMenuItem("Open...");
        openItem.Click += (_, _) => OpenFile();

        var openFolderItem = new ToolStripMenuItem("Open Folder...");
        openFolderItem.Click += (_, _) => OpenFolder();

        var saveItem = new ToolStripMenuItem("Save");
        saveItem.ShortcutKeys = Keys.Control | Keys.S;
        saveItem.Click += (_, _) => SaveFile();

        var saveAsItem = new ToolStripMenuItem("Save As...");
        saveAsItem.Click += (_, _) => SaveFileAs();

        // اضافه‌کردن گزینه‌ها به منوی File
        fileMenu.DropDownItems.Add(openItem);
        fileMenu.DropDownItems.Add(openFolderItem);
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(saveItem);
        fileMenu.DropDownItems.Add(saveAsItem);
        menu.Items.Add(fileMenu);

        MainMenuStrip = menu;

        // SplitContainer پنجره را به دو بخش تقسیم می‌کند:
        // سمت چپ برای درخت فایل‌ها، سمت راست برای ویرایش متن
        var workspace = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            SplitterDistance = 220
        };

        workspace.Panel1.Controls.Add(fileTree);
        workspace.Panel2.Controls.Add(editor);

        // افزودن بخش اصلی و منو به پنجره
        Controls.Add(workspace);
        Controls.Add(menu);

        // با بازشدن پنجره، امکان تایپ در ادیتور فراهم باشد
        Shown += (_, _) => editor.Focus();
    }

    // بازکردن یک فایل از پنجرهٔ انتخاب فایل
    private void OpenFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            LoadFile(dialog.FileName);
        }
    }

    // انتخاب پوشه و نمایش محتویاتش در درخت سمت چپ
    private void OpenFolder()
    {
        using var dialog = new FolderBrowserDialog();

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        fileTree.Nodes.Clear();

        var folder = new DirectoryInfo(dialog.SelectedPath);
        var rootNode = new TreeNode(folder.Name)
        {
            // Tag مسیر کامل پوشه را برای استفادهٔ بعدی نگه می‌دارد
            Tag = folder.FullName
        };

        fileTree.Nodes.Add(rootNode);

        // بارگذاری محتویات پوشهٔ اصلی و بازکردن آن
        LoadFolderContents(rootNode);
        rootNode.Expand();
    }

    // بارگذاری پوشه‌ها و فایل‌های داخل یک گره از درخت
    private void LoadFolderContents(TreeNode node)
    {
        if (node.Tag is not string folderPath || !Directory.Exists(folderPath))
        {
            return;
        }

        node.Nodes.Clear();

        try
        {
            // افزودن زیرپوشه‌ها
            foreach (string directoryPath in Directory.GetDirectories(folderPath))
            {
                var directory = new DirectoryInfo(directoryPath);
                var directoryNode = new TreeNode(directory.Name)
                {
                    Tag = directory.FullName
                };

                // گرهٔ موقت باعث می‌شود پوشه علامت بازشدن داشته باشد.
                // محتویات واقعی‌اش هنگام بازکردن پوشه خوانده می‌شوند.
                directoryNode.Nodes.Add(new TreeNode());

                node.Nodes.Add(directoryNode);
            }

            // افزودن فایل‌ها
            foreach (string filePath in Directory.GetFiles(folderPath))
            {
                var file = new FileInfo(filePath);
                var fileNode = new TreeNode(file.Name)
                {
                    Tag = file.FullName
                };

                node.Nodes.Add(fileNode);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // اگر اجازهٔ دسترسی به پوشه‌ای نباشد، از آن رد می‌شویم.
        }
        catch (IOException)
        {
            // اگر پوشه در دسترس نباشد، برنامه متوقف نمی‌شود.
        }
    }

    // بازکردن فایل انتخاب‌شده از درخت
    private void OpenSelectedFile(TreeNode node)
    {
        if (node.Tag is not string path || !File.Exists(path))
        {
            return;
        }

        LoadFile(path);
    }

    // خواندن فایل در ادیتور و به‌روزرسانی عنوان پنجره
    private void LoadFile(string path)
    {
        try
        {
            currentFilePath = path;
            editor.Text = File.ReadAllText(path, Encoding.UTF8);

            // پس از بازشدن فایل، نمایش از ابتدای متن شروع شود
            editor.SelectionStart = 0;
            editor.SelectionLength = 0;
            editor.ScrollToCaret();

            UpdateWindowTitle();
            editor.Focus();
        }
        catch (IOException ex)
        {
            MessageBox.Show($"فایل باز نشد:\n{ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"اجازهٔ دسترسی به فایل وجود ندارد:\n{ex.Message}");
        }
    }

    // ذخیره در فایل فعلی؛ اگر فایل جدید باشد، پنجرهٔ Save As باز می‌شود
    private void SaveFile()
    {
        if (currentFilePath is null)
        {
            SaveFileAs();
            return;
        }

        try
        {
            File.WriteAllText(currentFilePath, editor.Text, Encoding.UTF8);
        }
        catch (IOException ex)
        {
            MessageBox.Show($"فایل ذخیره نشد:\n{ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"اجازهٔ ذخیره در این مسیر وجود ندارد:\n{ex.Message}");
        }
    }

    // انتخاب نام و مسیر برای ذخیرهٔ فایل
    private void SaveFileAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            DefaultExt = "txt",
            AddExtension = true,
            FileName = currentFilePath is null
                ? "Untitled.txt"
                : Path.GetFileName(currentFilePath)
        };

        if (dialog.ShowDialog() != DialogResult.OK)
        {
            return;
        }

        try
        {
            currentFilePath = dialog.FileName;
            File.WriteAllText(currentFilePath, editor.Text, Encoding.UTF8);
            UpdateWindowTitle();
        }
        catch (IOException ex)
        {
            MessageBox.Show($"فایل ذخیره نشد:\n{ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show($"اجازهٔ ذخیره در این مسیر وجود ندارد:\n{ex.Message}");
        }
    }

    // نمایش نام فایل فعلی در عنوان پنجره
    private void UpdateWindowTitle()
    {
        string fileName = currentFilePath is null
            ? "Untitled"
            : Path.GetFileName(currentFilePath);

        Text = $"Notepad Clone - {fileName}";
    }
}
