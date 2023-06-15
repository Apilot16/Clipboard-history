using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Windows.Forms;
using Try_to_hook.Properties;


namespace Try_to_hook
{

    public partial class Form1 : Form
    {
        public List<string> Text_buffer = new List<string>();
        public List<System.Drawing.Image> Bitmap_buffer = new List<System.Drawing.Image>();
        public List<string> Deleted_buffer = new List<string>();
        public Size image_panel_size = new Size(50, 50);
        public Size image_scale_size = new Size(50, 50);
        private bool want_to_close = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!want_to_close)
            {
                e.Cancel = true;
                Hide();
                notifyIcon1.Visible = true;
                GC.Collect();
            }
            GC.Collect();
        }


        public void Generate_Tabs()
        {
            Clear_panel(tabPage1);
            Clear_panel(tabPage2);
            for (int i = 0; i < Text_buffer.Count; i++)
            {
                Create_panel(Text_buffer[i].GetHashCode().ToString(), Text_buffer[i], tabPage1);
            }
            for (int i = 0; i < Bitmap_buffer.Count; i++)
            {
                Create_panel(Bitmap_buffer[i].Tag.ToString(), Bitmap_buffer[i], image_panel_size, tabPage2);
            }
        }
        public void Scan_Clipboard_buffer()
        {
            if (Clipboard.ContainsText() && !Text_buffer.Contains(Clipboard.GetText()) && !Deleted_buffer.Contains(Clipboard.GetText()))
            {
                Text_buffer.Add(Clipboard.GetText());
                if (Settings.Default.File_logging) { Text_saver(Text_buffer.Last()); }
            }
            if (Clipboard.ContainsImage() && (Bitmap_buffer.Count == 0 || !Bitmap_buffer.Exists(elm => (int)elm.Tag == Hash_image(Clipboard.GetImage(), image_scale_size))) && !Deleted_buffer.Contains(Hash_image(Clipboard.GetImage(), image_scale_size).ToString()))
            {
                Bitmap_buffer.Add((Bitmap)Clipboard.GetImage());
                Bitmap_buffer.Last().Tag = Hash_image(Bitmap_buffer.Last(), image_scale_size);
                if (Settings.Default.Auto_save_image) { Image_saver(Bitmap_buffer.Last()); }
            }
        }

        private static int Hash_image(System.Drawing.Image image, Size new_size)
        {
            ImageConverter imgCon = new ImageConverter();
            using (MD5 mD = MD5.Create())
            {
                //Image.GetThumbnailImageAbort myCallback =  new Image.GetThumbnailImageAbort(ThumbnailCallback);
                using (System.Drawing.Image image_scaled = image.GetThumbnailImage(new_size.Width, new_size.Height, null, IntPtr.Zero))
                {

                    return BitConverter.ToInt32(mD.ComputeHash((byte[])imgCon.ConvertTo(image_scaled, typeof(byte[]))), 0);

                }
            }
        }
        public void Clear_buffer(List<string> buffer)
        {
            buffer.Clear();
        }
        public static void Clear_buffer(List<System.Drawing.Image> buffer)
        {
            foreach (Image item in buffer)
            {
                item.Dispose();
            }
            buffer.Clear();
        }
        public void Delete_element(List<string> buffer, int element)
        {
            buffer.RemoveAt(element);
        }
        public void Delete_element(List<System.Drawing.Image> buffer, int element)
        {
            buffer.ElementAt(element).Dispose();
            buffer.RemoveAt(element);
        }

        public IntPtr Hook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)Key_Global_hook.WM_KEYDOWN)//это проверка на именно нажатие, смотри keydown,keypress,keyup
            {
                int vkCode = Marshal.ReadInt32(lParam);//тут чтение данных из ячейки памяти по адресу lParam(адресс определяется автоматически при запуске приложений)
                if ((Keys)vkCode == Keys.C && Control.ModifierKeys == Keys.Control)
                {
                    Scan_Clipboard_buffer();
                }
                if ((Keys)vkCode == Keys.PrintScreen)
                {
                    Scan_Clipboard_buffer();
                    Generate_Tabs();
                }
                if ((Keys)vkCode == Keys.S && (Control.ModifierKeys == Keys.LWin || Control.ModifierKeys == Keys.RWin))
                {
                    Scan_Clipboard_buffer();
                }
                if ((Keys)vkCode == Keys.F2 && ModifierKeys == Keys.Control)
                {
                    this.Show();
                    notifyIcon1.Visible = false;
                }
            }
            return Key_Global_hook.CallNextHookEx(Key_Global_hook._hookID, nCode, wParam, lParam);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            сохранятьИзображенияToolStripMenuItem1.Checked = Settings.Default.Auto_save_image;
            записыватьВФайлToolStripMenuItem1.Checked = Settings.Default.File_logging;
            this.TopMost = Settings.Default.Always_on_display;
            всегдаСверхуToolStripMenuItem.Checked=Settings.Default.Always_on_display; 
            Key_Global_hook._proc = Hook;
            Key_Global_hook._hookID = Key_Global_hook.SetHook(Key_Global_hook._proc);
            notifyIcon1.ContextMenu = new ContextMenu();
            notifyIcon1.ContextMenu.MenuItems.Add(new MenuItem("Открыть", new EventHandler(show_window)));
            notifyIcon1.ContextMenu.MenuItems.Add(new MenuItem("Выйти", new EventHandler(exit)));

        }
        private void exit(object sender, EventArgs e)
        {
            want_to_close = true;
            if (Settings.Default.File_logging) { Text_saver(); }
            Key_Global_hook.UnhookWindowsHookEx(Key_Global_hook._hookID);
            Close();
            System.Windows.Forms.Application.Exit();
        }
        private void show_window(object sender, EventArgs e)
        {
            Show();
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            Scan_Clipboard_buffer();
            Generate_Tabs();
        }
        public void Create_panel(string name, string text, TabPage parent)
        {
            Panel Pan = new Panel
            {
                Name = name.ToString(),
                Dock = DockStyle.Top,
                BorderStyle = BorderStyle.FixedSingle,
                MinimumSize = new Size(50, 50),
                MaximumSize = new Size(99999, 70),
                BackColor = SystemColors.Window
            };
            System.Windows.Forms.Button Btn = new System.Windows.Forms.Button
            {
                Text = "",
                Name = "Button",
                BackgroundImage = Resources.seo_social_web_network_internet_322_icon_icons_com_61532.ToBitmap(),//System.Drawing.Image.FromFile(@"D:\Programming\C#\seo-social-web-network-internet_322_icon-icons.com_61532.ico"),
                BackgroundImageLayout = ImageLayout.Stretch,
                Size = new Size(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Btn.Location = new Point(Pan.Size.Width - Btn.Size.Width, 0);
            Btn.Click += Btn_Click;
            Pan.Controls.Add(Btn);
            //this.screen.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("screen.BackgroundImage")));
            // this.screen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            Label label = new System.Windows.Forms.Label
            {
                Padding = new Padding(10),
                Size = Pan.Size,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = text,
                Name = name.ToString()
            };
            label.Click += Label_Click;
            label.MouseEnter += Label_MouseEnter;
            label.MouseLeave += Label_MouseLeave;
            parent.Controls.Add(Pan);

            Pan.Controls.Add(label);

            object butn = Pan.Controls.Find("Button", true);

        }
        public void Create_panel(string name, System.Drawing.Image image, Size size, TabPage parent)
        {
            Panel Pan = new Panel
            {
                Name = name,
                Dock = DockStyle.Top,
                BorderStyle = BorderStyle.FixedSingle,
                MinimumSize = size,
                MaximumSize = new Size(99999, size.Height + 20),
                BackColor = SystemColors.Window
            };
            System.Windows.Forms.Button Btn = new System.Windows.Forms.Button
            {
                Text = "",
                BackgroundImage = Resources.seo_social_web_network_internet_322_icon_icons_com_61532.ToBitmap(),
                BackgroundImageLayout = ImageLayout.Stretch,
                Size = new Size(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            System.Windows.Forms.Button Sav = new System.Windows.Forms.Button
            {
                Text = "",
                BackgroundImage = Resources.icons8_save_100,
                BackgroundImageLayout = ImageLayout.Stretch,
                Size = new Size(25, 25),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            Btn.Location = new Point(Pan.Size.Width - Btn.Size.Width, 0);
            Sav.Location = new Point(Btn.Location.X - Sav.Size.Width, 0);
            Btn.Click += Btn_Click;
            Sav.Click += Sav_Click;
            Pan.Controls.Add(Btn);
            Pan.Controls.Add(Sav);
            PictureBox PicBox = new System.Windows.Forms.PictureBox
            {
                Padding = new Padding(10),
                Size = Pan.Size,
                Dock = DockStyle.Top,
                Image = image,
                SizeMode = PictureBoxSizeMode.StretchImage,
                Name = name
            };
            PicBox.Click += PicBox_Click;
            parent.Controls.Add(Pan);
            Pan.Controls.Add(PicBox);
        }
        public void Clear_panel(TabPage parent)
        {
            parent.Controls.Clear();
        }
        private void Sav_Click(object sender, EventArgs e)
        {
            using (Button obj = sender as System.Windows.Forms.Button)
            {

                using (SaveFileDialog save_FileDialog = new SaveFileDialog())
                {
                    save_FileDialog.InitialDirectory = Settings.Default.Image_filepath;
                    save_FileDialog.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|Png Image|*.png|All files (*.*)|*.*";
                    save_FileDialog.Title = "Save an Image File";
                    save_FileDialog.DefaultExt = "png";
                    save_FileDialog.FilterIndex = 4;
                    save_FileDialog.RestoreDirectory = true;

                    if (save_FileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(save_FileDialog.FileName))
                    {
                        switch (save_FileDialog.FilterIndex)
                        {
                            case 1:
                                Image_saver(Bitmap_buffer.ElementAt(Bitmap_buffer.FindIndex(im => im.Tag.ToString() == obj.Parent.Name)), save_FileDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                                break;

                            case 2:
                                Image_saver(Bitmap_buffer.ElementAt(Bitmap_buffer.FindIndex(im => im.Tag.ToString() == obj.Parent.Name)), save_FileDialog.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                                break;

                            case 3:
                                Image_saver(Bitmap_buffer.ElementAt(Bitmap_buffer.FindIndex(im => im.Tag.ToString() == obj.Parent.Name)), save_FileDialog.FileName, System.Drawing.Imaging.ImageFormat.Gif);
                                break;

                            case 4:
                                Image_saver(Bitmap_buffer.ElementAt(Bitmap_buffer.FindIndex(im => im.Tag.ToString() == obj.Parent.Name)), save_FileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                                break;
                        }


                    }
                }
                obj.Dispose();
            }
        }
        private void Btn_Click(object sender, EventArgs e)
        {
            using (Button obj = sender as System.Windows.Forms.Button)
            {
                if (obj.Parent.Parent.Name == "tabPage1")
                {
                    Text_buffer.RemoveAt(Text_buffer.FindIndex(elm => elm.GetHashCode().ToString() == obj.Parent.Name));
                }
                if (obj.Parent.Parent.Name == "tabPage2")
                {

                    Bitmap_buffer.RemoveAt(Bitmap_buffer.FindIndex(im => im.Tag.ToString() == obj.Parent.Name));
                }
                obj.Parent.Parent.Controls.Remove(obj.Parent);
                obj.Dispose();
            }
            GC.Collect();
        }
        private void Label_MouseLeave(object sender, EventArgs e)
        {
            Label obj = sender as System.Windows.Forms.Label;
            obj.BackColor = SystemColors.Window;

        }
        private void Label_MouseEnter(object sender, EventArgs e)
        {
            Label obj = sender as System.Windows.Forms.Label;
            obj.BackColor = SystemColors.GradientInactiveCaption;
        }
        private void PicBox_Click(object sender, EventArgs e)
        {
            PictureBox PicBox = sender as System.Windows.Forms.PictureBox;

            PicBox.BackColor = SystemColors.HotTrack;
            Clipboard.SetImage(Bitmap_buffer[Bitmap_buffer.FindIndex(im => im.Tag.ToString() == PicBox.Name)]);
        }

        private void list_add_Click(object sender, EventArgs e)
        {
            Generate_Tabs();
        }

        private void Label_Click(object sender, EventArgs e)
        {
            Label label = sender as System.Windows.Forms.Label;
            label.BackColor = SystemColors.HotTrack;
            Clipboard.SetText(label.Text);
        }

        /// <summary>
        /// </summary>

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Clear_panel(tabPage1);
            Clear_panel(tabPage2);
            Clear_buffer(Text_buffer);
            Clear_buffer(Bitmap_buffer);
            Clear_buffer(Deleted_buffer);
            Clipboard.Clear();
            GC.Collect();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Scan_Clipboard_buffer();
            Generate_Tabs();
        }

        private void задайтеФайлToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog save_FileDialog = new SaveFileDialog())
            {
                save_FileDialog.InitialDirectory = Settings.Default.Text_Folder;
                save_FileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                save_FileDialog.Title = "Select or Create File";
                save_FileDialog.DefaultExt = "txt";
                save_FileDialog.FilterIndex = 1;
                save_FileDialog.RestoreDirectory = true;

                if (save_FileDialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(save_FileDialog.FileName))
                {
                    DirectoryInfo info = new FileInfo(save_FileDialog.FileName).Directory;
                    Settings.Default.Text_Folder = info.FullName;
                    Settings.Default.Text_filepath = save_FileDialog.FileName;
                    Settings.Default.Save();
                }

            }
        }

        private void назначтеПапкуToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
            {
                DialogResult result = folderBrowser.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
                {
                    Settings.Default.Image_filepath = folderBrowser.SelectedPath;
                    Settings.Default.Save();
                }
            }
        }

        private void записатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Text_saver();
        }

        private void Text_saver()
        {
            Directory.CreateDirectory(Settings.Default.Text_Folder);
            using (StreamWriter streamWriter = File.AppendText(Settings.Default.Text_filepath))
            {
                Text_buffer.ForEach(s => streamWriter.WriteLine("![ " + s + " ]!\n"));
                streamWriter.Flush();
                streamWriter.Dispose();
                streamWriter.Close();
            }
            GC.Collect();
        }
        private void Text_saver(string text)
        {
            Directory.CreateDirectory(Settings.Default.Text_Folder);
            using (StreamWriter streamWriter = File.AppendText(Settings.Default.Text_filepath))
            {
                streamWriter.WriteLine("![ " + text + " ]!\n");
                streamWriter.Flush();
                streamWriter.Dispose();
                streamWriter.Close();
            }
            GC.Collect();
        }

        private void Image_saver(System.Drawing.Image image)
        {
            Directory.CreateDirectory(Settings.Default.Image_filepath);
            image.Save(Settings.Default.Image_filepath + @"\" + image.Tag + "." + Settings.Default.Image_extention.ToString(), Settings.Default.Image_extention);
            GC.Collect();
        }

        private void Image_saver(System.Drawing.Image image, string file_path, System.Drawing.Imaging.ImageFormat extention)
        {
            image.Save(file_path, extention);// + @"\" + image.Tag + "." + Settings.Default.Image_extention.ToString()
        }

        private void записыватьВФайлToolStripMenuItem1_CheckedChanged_1(object sender, EventArgs e)
        {
            задайтеФайлToolStripMenuItem1.Checked = !задайтеФайлToolStripMenuItem1.Checked;
            Settings.Default.File_logging = задайтеФайлToolStripMenuItem1.Checked;
            Settings.Default.Save();
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Middle) { exit(sender,e); }   
            notifyIcon1.Visible = false;
            Show();
        }

        private void сохранятьИзображенияToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            сохранятьИзображенияToolStripMenuItem1.Checked = !Settings.Default.Auto_save_image;
            Settings.Default.Auto_save_image = !Settings.Default.Auto_save_image;
            Settings.Default.Save();
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GC.Collect();
            exit(sender, e);
        }

        private void скрыватьВТрейToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            Hide();
        }

        private void опрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Это программа для сохранения истории буфера обмена\n открыть:ctrl+F2\nзакрыть: главная-> выход или СКМ на значок в трее \n реагирует на ctrl+c, Win+Shift+S, PrintScreen");
        }

        private void всегдаСверхуToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            Settings.Default.Always_on_display = всегдаСверхуToolStripMenuItem.Checked;
            this.TopMost = всегдаСверхуToolStripMenuItem.Checked;
            Settings.Default.Save();
        }
    }
}
