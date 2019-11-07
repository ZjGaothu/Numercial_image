using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Drawing;
using Microsoft.Win32;
using System.Windows.Media;
using System.IO;
using System.Threading;

/// <summary>
/// 模块名：主窗体
/// 功能：图像变形主界面的UI设计
/// 作者：高子靖
/// </summary>

namespace Numerical_image1
{

    public partial class MainWindow : Window
    {
        //扭曲变换的原图和转换后图片
        private System.Drawing.Bitmap Image_after_trans = null;
        private System.Drawing.Bitmap Image_to_trans = null;

        //人脸变形Image
        private System.Drawing.Bitmap Image_face_1 = null;
        private System.Drawing.Bitmap Image_face_2 = null;

        private int flag_tochange = 0; //是否已经上传了图片
        private String[] my_txt;  //存储文件名


        private int flag_changeface = 0;    //是否完成人脸转换的标志
        private int flag_Distortion = 0;    //是否完成扭曲变换的标志

        
        private Matrix Source_point_1;      //控制关键点
        private Matrix Source_point_1_new;  //经平移和尺度变换后的关键点
        int flag_source_1 = 0;              //源图1是否上载图片
        private Matrix Source_point_2;      //目标关键点
        int flag_source_2 = 0;              //源图2是否上载图片
        int flag_keypoints = 0;             //关键点显示/消除

        public MainWindow()
        {
            InitializeComponent();

            //初始化显示图片 图框
            Uri Thu_path1 = new Uri("./UI_image/index_4.png", UriKind.Relative);
            Bitmap bi = new Bitmap("./UI_image/index_4.png");
            THUImage.Source = BitmaptoImage(bi);
            Bitmap bi_2 = new Bitmap("./UI_image/index_3.png");
            trans_THUImage.Source = BitmaptoImage(bi_2);
            //初始化私有变量
            Image_to_trans = (Bitmap)bi.Clone();
            center_x.Text = Convert.ToString(Image_to_trans.Width / 2);
            center_y.Text = Convert.ToString(Image_to_trans.Height / 2);
            //使用字典初始化combobox
            Dictionary<int, String> mydic_forchange = new Dictionary<int, String>()
            {
                {1,"旋转扭曲"},
                {2,"图像畸变"}
            };
            combo_change.ItemsSource = mydic_forchange;
            combo_change.SelectedValuePath = "Key";
            combo_change.DisplayMemberPath = "Value";
            Dictionary<int, String> mydic_formethod = new Dictionary<int, String>()
            {
                {1,"最近邻插值"},
                {2,"双线性插值"},
                {3,"双三次插值"}
            };
            combo_method.ItemsSource = mydic_formethod;
            combo_method.SelectedValuePath = "Key";
            combo_method.DisplayMemberPath = "Value";

            Dictionary<int, String> mydic_formethod_2 = new Dictionary<int, String>()
            {
                {1,"最近邻插值"},
                {2,"双线性插值"},
                {3,"双三次插值"}
            };
            My_combo3.ItemsSource = mydic_formethod;
            My_combo3.SelectedValuePath = "Key";
            My_combo3.DisplayMemberPath = "Value";


            //人脸变形page 初始化
            Image_face_ori1.Source = BitmaptoImage(bi);
            Image_face_resul.Source = BitmaptoImage(bi_2);
            Image_face_ori2.Source = BitmaptoImage(bi);
            Bitmap arrow = new Bitmap("./UI_image/arrow_1.png");
            Image_face_trans.Source = BitmaptoImage(arrow);

            //初始化txt的path列表
            initial_txt();
        }


        //将读入的txt转为字符串数组
        private Matrix TransStringto_matrxi(String[] str)
        {
            int n = str.Length;
            Matrix result = new Matrix(n, 3);
            for(int i = 0; i <n;i++)
            {
                result[i, 0] = 1;
                double[] temp = new double[2];
                temp = My_TDS(str[i]);
                result[i, 1] = temp[0];
                result[i, 2] = temp[1];
            }
            return result;
        }

        //将txt每一行的字符串转换成double型的坐标
        public double[] My_TDS(String str)
        {
            double[] result = new double[2];
            String[] ss = str.Split(' ');
            if(ss.Length != 2)
            {
                return null;
            }
            else
            {
                result[0] = Convert.ToDouble(ss[0]);
                result[1] = Convert.ToDouble(ss[1]);
            }
            return result;
        }


        //初始化txt名列表
        private void initial_txt()
        {
            my_txt = new string[9];
            for(int i = 1;i <= 9;i++)
            {
                String temp = Convert.ToString(i);
                String name = "./My_txt/" + temp + ".txt";
                my_txt[i - 1] = name;
            }
        }


        //转动变形的方向
        public int Check_method_clock()
        {
            if (radio_clock.IsChecked == true)
            {
                return -1;
            }
            else if (radio_anticlock.IsChecked == true)
            {
                return 1;
            }
            else return 0;
        }

        //检查畸变的方式 桶形畸变或枕形畸变
        public int Check_method_Distortion()
        {
            if (Barrel.IsChecked == true)
            {
                return -1;
            }
            else if (Pincushion.IsChecked == true)
            {
                return 1;
            }
            else return 0;
        }


        //获取转动参数
        public double[] GetParameter_forrotate()
        {
            double[] para = new double[6];
            para[0] = double.Parse(angel.Text);
            para[1] = double.Parse(radius.Text);
            int method = Check_method_clock();
            para[2] = Getpola_function();
            para[3] = Convert.ToDouble(method);
            para[4] = double.Parse(center_x.Text);
            para[5] = double.Parse(center_y.Text);
            return para;
        }

        //获取扭曲变换插值函数的选择
        public int Getpola_function()
        {
            return combo_method.SelectedIndex;
        }

        //获取TPS插值函数的选择
        public int Get_TPS_polafunction()
        {
            return My_combo3.SelectedIndex;
        }

        //将Bitmap转换为ImageSource类型 即可以将Bitmap在容器显示
        public BitmapSource BitmaptoImage(Bitmap bit)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bit.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        //进行扭曲变形
        private void Trans_Btn_Click(object sender, RoutedEventArgs e)
        {
            //未传图片时
            if(flag_tochange == 0)
            {
                MessageBox.Show("请先点击原图上传图片", "提醒");
                return;
            }
            //实例化My_cv的对象
            My_cv my_trans = new My_cv(Image_to_trans);
            int result_method = combo_change.SelectedIndex;
            if (result_method == 0)
            {
                if (Check_method_clock() == 0)
                {
                    MessageBox.Show("请选择转动方向", "提醒");
                    return;
                }
                if (!Check_legal_rotate())
                {
                    MessageBox.Show("您有非数字的输入", "提醒");
                    return;
                }
                double[] parameter = new double[6];
                parameter = GetParameter_forrotate();
                if(Getpola_function() == -1)
                {
                    MessageBox.Show("请选择插值函数", "提示");
                    return;
                }
                if (parameter[4] < 1 || parameter[4] > Image_to_trans.Width || parameter[5] < 1 || parameter[5] > Image_to_trans.Height)
                {
                    MessageBox.Show("旋转中心超出范围自动帮您调为图片中心", "提醒");
                    parameter[4] = Image_to_trans.Width / 2;
                    parameter[5] = Image_to_trans.Height / 2;
                }
                //可进行旋转操作
                trans_THUImage.Source = BitmaptoImage(my_trans.Rotate_Image(parameter[0], parameter[1], (int)parameter[2], (int)parameter[3], parameter[4], parameter[5]));
                //完成扭曲变形标志
                flag_Distortion = 1;

            }
            else if (result_method == 1)
            {
                if (Check_method_Distortion() == 0)
                {
                    MessageBox.Show("请选择畸变方式", "提醒");
                    return;
                }
                if (!Check_legal_Distortion())
                {
                    MessageBox.Show("您有非数字的输入", "提醒");
                    return;
                }
                double para = double.Parse(radius_2.Text);
                //完成扭曲变形标志
                flag_Distortion = 1;
                trans_THUImage.Source = BitmaptoImage(my_trans.Distortion_Image(para, Check_method_clock(), Check_method_Distortion()));
            }
            else
            {
                MessageBox.Show("请选择图像变换的方式", "提示");
            }
        }

        //判断String是否为小数或纯数字
        public static bool isNumberic(String _string)
        {
            if (string.IsNullOrEmpty(_string))
            {
                return false;
            }
            if (_string[0] == '.')
            {
                return false;
            }
            foreach (char c in _string)
            {
                if (!char.IsDigit(c) && c != '.')
                {
                    return false;
                }
            }
            return true;
        }

        //检查旋转输入的参数是否合法 是否是小数或整数
        public bool Check_legal_rotate()
        {
            if (isNumberic(center_x.Text) && isNumberic(center_y.Text) && isNumberic(angel.Text) && isNumberic(radius.Text))
            {
                return true;
            }
            return false;
        }

        //检查畸变的参数是否为合法数字
        public bool Check_legal_Distortion()
        {
            if (isNumberic(radius_2.Text))
            {
                return true;
            }
            return false;
        }


        //扭曲变换源图上载 使用对话框
        private void origin_mouse(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog my_image = new OpenFileDialog();
            my_image.Title = "选择文件";
            my_image.Filter = "image(*.jpg)|*.jpg|image(*.png)|*.png|image(*.bmp)|*.bmp|所有文件(*.*)|*.*"; ;
            my_image.FileName = "选择文件夹.";
            my_image.FilterIndex = 1;
            my_image.ValidateNames = false;
            my_image.CheckFileExists = false;
            my_image.CheckPathExists = true;
            bool? result = my_image.ShowDialog();
            if (result != true)
            {
                return;
            }
            else
            {
                //进行上载并重新初始化结果图片
                string[] files = my_image.FileNames;
                Bitmap temp = new Bitmap(files[0]);
                Image_to_trans = (Bitmap)temp.Clone();
                THUImage.Source = BitmaptoImage(temp);
                Bitmap bi_2 = new Bitmap("./UI_image/index_3.png");
                trans_THUImage.Source = BitmaptoImage(bi_2);
                flag_Distortion = 0;

            }
            //获取中心位置
            center_x.Text = Convert.ToString(Image_to_trans.Width / 2);
            center_y.Text = Convert.ToString(Image_to_trans.Height / 2);
            flag_tochange = 1;
        }


        //扭曲 保存图片 点击变换后图片即可保存
        private void Save_Image(object sender, MouseButtonEventArgs e)
        {
            if(flag_Distortion == 0)
            {
                MessageBox.Show("请您先完成图片的变换", "提醒");
                return;
            }
            SaveFileDialog save_myimage = new SaveFileDialog();
            //保存格式
            save_myimage.Filter = "Image Files (*.bmp, *.png, *.jpg)|*.bmp;*.png;*.jpg | All Files | *.*";
            save_myimage.RestoreDirectory = true;

            if (save_myimage.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.trans_THUImage.Source));
                using (FileStream stream = new FileStream(save_myimage.FileName, FileMode.Create))
                    encoder.Save(stream);
            }

        }

        //将jpg名转换为txt名
        private String Transjpgto_txtname(String str)
        {
            String index = Char.ToString(str[0]);
            return index + ".txt";
        }
        //人脸变形 待转换人脸上载
        private void Source_1_upload(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog my_image = new OpenFileDialog();
            my_image.Title = "选择文件";
            my_image.Filter = "所有文件(*.*)|*.*";
            my_image.FileName = "选择文件夹.";
            my_image.FilterIndex = 1;
            my_image.ValidateNames = false;
            my_image.CheckFileExists = false;
            my_image.CheckPathExists = true;
            bool? result = my_image.ShowDialog();
            if (result != true)
            {
                return;
            }
            else
            {
                string[] files = my_image.FileNames;
                String path = files[0];
                int length = path.Length;
                //若未选择规定的图片
                if (path[length - 5] - '0' > 9 || path[length - 5] - '0' < 1)
                {
                    MessageBox.Show("请选择上传具有关键点坐标的图片", "提醒");
                    return;
                }
                Bitmap temp = new Bitmap(files[0]);
                Image_face_1 = (Bitmap)temp.Clone();
                Image_face_ori1.Source = BitmaptoImage(temp);
                //转换为相应的txt文件
                int number = int.Parse(Char.ToString(path[length - 5]));
                String[] strs;
                strs = File.ReadAllLines(my_txt[number-1]);
                Source_point_2 = new Matrix(TransStringto_matrxi(strs));
                Bitmap bi_3 = new Bitmap("./UI_image/index_3.png");
                Image_face_resul.Source = BitmaptoImage(bi_3);
                flag_source_1 = 1;
                flag_changeface = 0;
                if (flag_source_1 != 0 && flag_source_2 != 0)
                {
                    Key_point_registration();
                }
            }
        }

        //人脸变形 目标人脸上载
        private void Source_2_upload(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog my_image = new OpenFileDialog();
            my_image.Title = "选择文件";
            my_image.Filter = "所有文件(*.*)|*.*";
            my_image.FileName = "选择文件夹.";
            my_image.FilterIndex = 1;
            my_image.ValidateNames = false;
            my_image.CheckFileExists = false;
            my_image.CheckPathExists = true;
            bool? result = my_image.ShowDialog();
            if (result != true)
            {
                return;
            }
            else
            {
                string[] files = my_image.FileNames;
                String path = files[0];
                int length = path.Length;
                if(path[length - 5]-'0'>9 || path[length - 5] - '0' < 1)
                {
                    MessageBox.Show("请选择上传具有关键点坐标的图片", "提醒");
                    return;
                }
                Bitmap temp = new Bitmap(files[0]);
                Image_face_2 = (Bitmap)temp.Clone();
                int number = int.Parse(Char.ToString(path[length - 5]));
                String[] strs;
                strs = File.ReadAllLines(my_txt[number-1]);
                Source_point_1 = new Matrix(TransStringto_matrxi(strs));
                int num = Source_point_1.row;
                Image_face_ori2.Source = BitmaptoImage(Image_face_2);
                Bitmap bi_3 = new Bitmap("./UI_image/index_3.png");
                Image_face_resul.Source = BitmaptoImage(bi_3);
                flag_source_2 = 1;
                flag_changeface = 0;
                if(flag_source_1 != 0 && flag_source_2 != 0)
                {
                    Key_point_registration();
                }
            }
        }

        //TPS使用径向基函数
        private double U_r(double x_1,double y_1, double x_2,double y_2)
        {
            if(x_1 == x_2 && y_1 == y_2)
            {
                return 0;
            }
            else
            {
                double distance = (x_1 - x_2) * (x_1 - x_2) + (y_1 - y_2) * (y_1 - y_2);
                double result = distance * Math.Log(distance);
                return result;
            }
        }

        //求解TPS
        private Matrix initial_TPS_parameter()
        {
            //当前待修改人脸和目标人脸均有图片时
            if(Source_point_1 != null && Source_point_2 != null)
            {
                int lenth = Source_point_1.row;
                Matrix result = new Matrix(lenth + 3, 2);
                Matrix K = new Matrix(lenth,lenth); //TPS中间矩阵K
                Matrix O = new Matrix(3, 3);
                Matrix V = new Matrix(lenth, 2);
                for (int i =0;i<3;i++)
                {
                    for(int j = 0;j<3;j++)
                    {
                        O[i, j] = 0;
                    }
                }
                for(int i=0;i<lenth;i++)
                {
                    V[i, 0] = Source_point_2[i, 1];
                    V[i, 1] = Source_point_2[i, 2];
                    for(int j = 0;j<lenth;j++)
                    {
                        K[i, j] = U_r(Source_point_1_new[i, 1], Source_point_1_new[i, 2], Source_point_1_new[j, 1], Source_point_1_new[j, 2]);
                    }
                }
                //矩阵合并
                Matrix L_up = result.Horizontal_integra(K, Source_point_1_new);
                Matrix L_down = result.Horizontal_integra(K.transpose(Source_point_1_new), O);
                Matrix L = result.Vertical_integra(L_up, L_down);

                Matrix temp = new Matrix(3, 2);
                for(int i = 0;i<3;i++)
                {
                    for(int j =0;j<2;j++)
                    {
                        temp[i, j] = 0;
                    }
                }
                Matrix Y = new Matrix(result.Vertical_integra(V,temp));
                //求解系数 直接通过矩阵求逆和矩阵乘法
                result = result.Matrix_Multi(result.Ionverse_matr(L), Y);
                return result;
            }
            return null;
        }


        //人脸变换按钮
        private void Trans_face_btn(object sender, RoutedEventArgs e)
        {
            if(flag_source_1 == 0 || flag_source_2 == 0)
            {
                MessageBox.Show("请您先选择图片", "提醒");
                return;
            }
            int a = Get_TPS_polafunction();
            if(Get_TPS_polafunction() == -1)
            {
                MessageBox.Show("请先选择插值方法", "提醒");
                return;
            }
            Matrix TPS = new Matrix(initial_TPS_parameter());
            My_cv my_trans = new My_cv(Image_face_1,Image_face_2);
            DateTime beforDT = System.DateTime.Now;
            Image_face_resul.Source = BitmaptoImage(my_trans.TPS_face(TPS,Get_TPS_polafunction(),Source_point_1_new));
            flag_changeface = 1;

            //记录变换用时
            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforDT);
            double TPS_time =  ts.TotalMilliseconds/1000;
            Functiontime.Content = TPS_time.ToString("0.###");
        }

        private void Save_newface(object sender, MouseButtonEventArgs e)
        {
            if(flag_changeface == 0)
            {
                MessageBox.Show("请您先完成人脸变换，之后可进行保存", "提醒");
                return;
            }
            SaveFileDialog save_myimage = new SaveFileDialog();
            save_myimage.Filter = "Image Files (*.bmp, *.png, *.jpg)|*.bmp;*.png;*.jpg | All Files | *.*";
            save_myimage.RestoreDirectory = true;

            if (save_myimage.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.Image_face_resul.Source));
                using (FileStream stream = new FileStream(save_myimage.FileName, FileMode.Create))
                    encoder.Save(stream);
            }
        }

        //人脸关键点配准
        private void Key_point_registration()
        {

            //将控制点目标平移与尺度变换
            double x_mean_dif = 0;
            double y_mean_dif = 0;
            //记录中心位置
            double x_center_1 = 0, y_center_1 = 0;
            double x_center_2 = 0, y_center_2 = 0;
            int n = Source_point_1.row;
            for (int i = 0; i < n; i++)
            {
                x_mean_dif += Source_point_1[i, 1] - Source_point_2[i, 1];
                y_mean_dif += Source_point_1[i, 2] - Source_point_2[i, 2];
                x_center_1 += Source_point_1[i, 1];
                y_center_1 += Source_point_1[i, 2];
                x_center_2 += Source_point_2[i, 1];
                y_center_2 += Source_point_2[i, 2];
            }
            x_mean_dif = x_mean_dif / n;
            y_mean_dif = y_mean_dif / n;
            x_center_1 = x_center_1 / n;
            x_center_2 = x_center_2 / n;
            y_center_1 = y_center_1 / n;
            y_center_2 = y_center_2 / n;
            //计算xy方向上的方差
            double sum_x_1 = 0, sum_x_2 = 0;
            double sum_y_1 = 0, sum_y_2 = 0;
            for (int i = 0; i < n; i++)
            {
                sum_x_1 += (Source_point_1[i, 1] - x_center_1) * (Source_point_1[i, 1] - x_center_1);
                sum_x_2 += (Source_point_2[i, 1] - x_center_2) * (Source_point_2[i, 1] - x_center_2);
                sum_y_1 += (Source_point_1[i, 2] - y_center_1) * (Source_point_1[i, 2] - y_center_1);
                sum_y_2 += (Source_point_2[i, 2] - y_center_2) * (Source_point_2[i, 2] - y_center_2);
            }
            double scale_1 = sum_x_2 / sum_x_1;
            double scale_2 = sum_y_2 / sum_y_1;
            Source_point_1_new = new Matrix(Source_point_1);
            //按照标准差进行配准
            for (int i = 0; i < n; i++)
            {
                Source_point_1_new[i, 1] = (Source_point_1[i, 1] - x_center_1)*Math.Sqrt(scale_1) + x_center_1 - x_mean_dif;
                Source_point_1_new[i, 2] = (Source_point_1[i, 2] - y_center_1)*Math.Sqrt(scale_2) + y_center_1 - y_mean_dif;
            }
        }

        //显示关键点
        private void Point_show_btn(object sender, RoutedEventArgs e)
        {
            if(flag_source_1 == 0 || flag_source_2 == 0)
            {
                MessageBox.Show("请首先将两张图片上传", "提示");
                return;
            }
            if(flag_keypoints == 0)
            {
                int num = Source_point_1.row;
                //使用蓝点标出图像关键点
                Bitmap Image_face_1_clone = (Bitmap)Image_face_1.Clone();
                Bitmap Image_face_2_clone = (Bitmap)Image_face_2.Clone();

                for (int i = 0; i < num; i++)
                {
                    for (int j = -2; j < 3; j++)
                    {
                        for (int k = -2; k < 3; k++)
                        {
                            Image_face_1_clone.SetPixel((int)Source_point_2[i, 1] + j, (int)Source_point_2[i, 2] + k, System.Drawing.Color.FromArgb(0, 0, 255));
                            Image_face_2_clone.SetPixel((int)Source_point_1[i, 1] + j, (int)Source_point_1[i, 2] + k, System.Drawing.Color.FromArgb(0, 0, 255));
                        }
                    }
                }
                Image_face_ori2.Source = BitmaptoImage(Image_face_2_clone);
                Image_face_ori1.Source = BitmaptoImage(Image_face_1_clone);
                flag_keypoints = 1;
            }
            else if(flag_keypoints == 1)
            {
                Image_face_ori2.Source = BitmaptoImage(Image_face_2);
                Image_face_ori1.Source = BitmaptoImage(Image_face_1);
                flag_keypoints = 0;
            }
        }

        //保存图片按钮
        private void Save_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (flag_Distortion == 0)
            {
                MessageBox.Show("请您先完成图片的变换", "提醒");
                return;
            }
            SaveFileDialog save_myimage = new SaveFileDialog();
            save_myimage.Filter = "Image Files (*.bmp, *.png, *.jpg)|*.bmp;*.png;*.jpg | All Files | *.*";
            save_myimage.RestoreDirectory = true;

            if (save_myimage.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.trans_THUImage.Source));
                using (FileStream stream = new FileStream(save_myimage.FileName, FileMode.Create))
                    encoder.Save(stream);
            }
        }

        //点击结果图片保存人脸变形图片
        private void Save_face_btn(object sender, RoutedEventArgs e)
        {
            if (flag_changeface == 0)
            {
                MessageBox.Show("请您先完成人脸变换，之后可进行保存", "提醒");
                return;
            }
            SaveFileDialog save_myimage = new SaveFileDialog();
            save_myimage.Filter = "Image Files (*.bmp, *.png, *.jpg)|*.bmp;*.png;*.jpg | All Files | *.*";
            save_myimage.RestoreDirectory = true;

            if (save_myimage.ShowDialog() == true)
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create((BitmapSource)this.Image_face_resul.Source));
                using (FileStream stream = new FileStream(save_myimage.FileName, FileMode.Create))
                    encoder.Save(stream);
            }
        }
    }




}
