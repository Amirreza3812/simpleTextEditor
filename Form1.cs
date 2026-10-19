namespace NotepadClone;

using System.IO;
using System.Text;
using System.Windows.Forms;

public partial class Form1 : Form
{
    // کادر ویرایش متن و مسیر فایل فعلی
    private readonly RichTextBox editor = new();
    private string? currentFilePath;

    public Form1()
    {
        InitializeComponent();

        Text = "Notepad Clone";
        Width = 800;
        Height = 600;

        editor.Dock = DockStyle.Fill;
        editor.BorderStyle = BorderStyle.None;
        editor.Font = new System.Drawing.Font("Consolas", 12);
        editor.AcceptsTab = true;
        editor.WordWrap = false;
        editor.ReadOnly = false;
        editor.Enabled = true;
        editor.TabStop = true;
        editor.ScrollBars = RichTextBoxScrollBars.Vertical;
        editor.ScrollBars = RichTextBoxScrollBars.Both;


        // منوی File و گزینه‌های آن
        var menu = new MenuStrip
        {
            Dock = DockStyle.Top
        };
        var fileMenu = new ToolStripMenuItem("File");

        var openItem = new ToolStripMenuItem("Open...");
        openItem.Click += (_, _) => OpenFile();

        var saveItem = new ToolStripMenuItem("Save");
        saveItem.ShortcutKeys = Keys.Control | Keys.S;
        saveItem.Click += (_, _) => SaveFile();

        var saveAsItem = new ToolStripMenuItem("Save As...");
        saveAsItem.Click += (_, _) => SaveFileAs();

        fileMenu.DropDownItems.Add(openItem);
        fileMenu.DropDownItems.Add(saveItem);
        fileMenu.DropDownItems.Add(saveAsItem);
        menu.Items.Add(fileMenu);

        MainMenuStrip = menu;
        // پنل فضای زیر منو را می‌گیرد و ادیتور داخل پنل قرار می‌گیرد
        var editorPanel = new Panel
        {
            Dock = DockStyle.Fill
        };

        // اول منو اضافه شود، بعد کادر متن تا کادر کل فضای باقی‌مانده را بگیرد
        editor.Dock = DockStyle.Fill;
        editor.ScrollBars = RichTextBoxScrollBars.Both;
        editor.WordWrap = false;

        editorPanel.Controls.Add(editor);
        Controls.Add(editorPanel);
        Controls.Add(menu);

        Shown += (_, _) => editor.Focus();
    }

    // بازکردن فایل موجود
    private void OpenFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            currentFilePath = dialog.FileName;
            editor.Text = File.ReadAllText(currentFilePath, Encoding.UTF8);
            editor.SelectionStart = 0;
            editor.SelectionLength = 0;
            editor.ScrollToCaret();
            UpdateWindowTitle();
        }
    }

    // ذخیره در فایل فعلی؛ اگر هنوز فایلی انتخاب نشده باشد، Save As را اجرا می‌کند
    private void SaveFile()
    {
        if (currentFilePath is null)
        {
            SaveFileAs();
            return;
        }

        File.WriteAllText(currentFilePath, editor.Text, Encoding.UTF8);
    }

    // انتخاب نام و مسیر برای ذخیره
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

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            currentFilePath = dialog.FileName;
            File.WriteAllText(currentFilePath, editor.Text, Encoding.UTF8);
            UpdateWindowTitle();
        }
    }

    // به‌روزرسانی عنوان پنجره با نام فایل
    private void UpdateWindowTitle()
    {
        var fileName = currentFilePath is null
            ? "Untitled"
            : Path.GetFileName(currentFilePath);

        Text = $"Notepad Clone - {fileName}";
    }
}
