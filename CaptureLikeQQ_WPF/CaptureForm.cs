using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaptureLikeQQ_WPF
{
    public partial class CaptureForm : Form
    {
        private bool isMouseDown = false;
        Point upperleft;
        Point downright;
        public CaptureForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;//设置本窗体
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true); // 禁止擦除背景.
            SetStyle(ControlStyles.DoubleBuffer, true); // 双缓冲
        }
        private void CaptureForm_Load(object sender, EventArgs e)
        {
        }
        private void CaptureForm_MouseMove(object sender, MouseEventArgs e)
        {
            
            Point currentPoint = new Point(0, 0);
            User32Helper.GetCursorPos(ref currentPoint);
            Color c = (this.BackgroundImage as Bitmap).GetPixel(currentPoint.X, currentPoint.Y);
            string msg = string.Format("R:{0} G:{1} B:{2}",c.R,c.G,c.B);
            if (!isMouseDown)
            {
                this.lbMsg.Text = msg;
                return;
            }
            msg=msg+"  "+ string.Format("选取的bmp大小{0}:{1}", Math.Abs(e.X - upperleft.X), Math.Abs(e.Y - upperleft.Y));
            this.lbMsg.Text = msg;
            int x_max;
            int x_min;
            int y_max;
            int y_min;
            if (e.X > upperleft.X)
            {
                x_max = e.X;
                x_min = upperleft.X;
            }
            else
            {
                x_min = e.X;
                x_max = upperleft.X;
            }
            if (e.Y > upperleft.Y)
            {
                y_max = e.Y;
                y_min = upperleft.Y;
            }
            else
            {
                y_min = e.Y;
                y_max = upperleft.Y;
            }
            int width = x_max - x_min;
            int height = y_max - y_min;
            this.Refresh();//清除上一次的痕迹
            using (var g = this.CreateGraphics())
            {
                g.DrawRectangle(new Pen(new SolidBrush(Color.Blue)), new Rectangle(x_min - 1, y_min - 1, width + 1, height + 1));
            }
            
        }
        private void CaptureForm_MouseDown(object sender, MouseEventArgs e)
        {
            this.Cursor = Cursors.Hand;
            User32Helper.GetCursorPos(ref upperleft);
            isMouseDown = true;
        }

        private void CaptureForm_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            
            this.lbMsg.Text = "";
            Application.DoEvents();
            User32Helper.GetCursorPos(ref downright);
            Size size = new System.Drawing.Size(Math.Abs(downright.X - upperleft.X), Math.Abs(downright.Y - upperleft.Y));
            this.Refresh();
            Bitmap bmp = null;
            if (upperleft.X < downright.X && upperleft.Y < downright.Y)//从左上往右下方拖动
                bmp = ImageHelper.GetImage(upperleft, size);
            else if (upperleft.X > downright.X && upperleft.Y > downright.Y) //从右下往左上方拖动
                bmp = ImageHelper.GetImage(downright, size);
            else if (upperleft.X < downright.X && upperleft.X > downright.Y)//从左下方往右上方拖动
                bmp = ImageHelper.GetImage(new Point(upperleft.X, downright.Y), size);
            else if (upperleft.X > downright.X && upperleft.Y < downright.Y)//从右上方往左下方拖动
                bmp = ImageHelper.GetImage(new Point(downright.X, upperleft.Y), size);
            SaveImage(bmp);
        }
        private void SaveImage(Bitmap bmp)
        {
            if (bmp == null)
                return;
            Clipboard.SetDataObject(bmp);
            SaveFileDialog s = new SaveFileDialog();
            s.Filter = "BMP|*.bmp;|PNG|*.png|GIF|*.gif|JPEG|*.jpeg";
            if (s.ShowDialog() == DialogResult.OK)
            {
                switch (s.FilterIndex)
                {
                    case 0: bmp.Save(s.FileName); break;
                    case 1: bmp.Save(s.FileName, System.Drawing.Imaging.ImageFormat.Png); break;
                    case 2: bmp.Save(s.FileName, System.Drawing.Imaging.ImageFormat.Gif); break;
                    case 3: bmp.Save(s.FileName, System.Drawing.Imaging.ImageFormat.Jpeg); break;
                }

                bmp.Save(s.FileName);
                this.Cursor = Cursors.Default;

                this.Close();
            }
            else
            {
                this.Refresh();
            }
        }
        private void CaptureForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                this.Refresh();
                Bitmap bmp = ImageHelper.GetImage(new Point(0,0),new Size(this.Width,this.Height));
                SaveImage(bmp);
            }
        }
    }
}
