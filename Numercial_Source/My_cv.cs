using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


/// <summary>
/// 模块名：My_cv
/// 功能：实现了图像处理的变形和插值功能
/// 作者：高子靖
/// </summary>

namespace Numerical_image1
{
    class My_cv
    {
        public My_cv() { }          //构造函数
        public My_cv(Bitmap orig)   //拷贝构造函数
        {
            Image_to_trans = (Bitmap)orig.Clone();
        }

        public My_cv(Bitmap ori1,Bitmap ori2)  //构造函数初始化私有变量
        {
            Image_face_1 = (Bitmap)ori1.Clone();
            Image_face_2 = (Bitmap)ori2.Clone();
        }
        private Bitmap Image_after_trans = null;//扭曲变换后的图片
        private Bitmap Image_to_trans = null;   //扭曲变换待转换图片


        private Bitmap Image_face_1 = null;     //TPS人脸变形待转换人脸
        private Bitmap Image_face_2 = null;     //TPS人脸变形目标人脸
        private Bitmap Image_face_result = null;//TPS人脸变形转换结果

        public Bitmap Get_Reult(Bitmap result)  //私有变量接口函数
        {
            return Image_after_trans;
        }
        public void Set_Orig(Bitmap orig)       //接口
        {
            Image_to_trans = (Bitmap)orig.Clone();
        }


        //旋转操作
        //功能：将图像进行顺时针或逆时针旋转
        //参数列表： Angel:旋转角 radius:最大旋转半径 method:旋转插值函数0-最近邻 1-双线性 2-双三次 direction:旋转方向
        //           x_center,y_center:旋转中心点
        public System.Drawing.Bitmap Rotate_Image(double Angel, double radius, int method, int direction,double x_center ,double y_center)
        {
            int pict_wid = Image_to_trans.Width;
            int pict_hei = Image_to_trans.Height;
            Image_after_trans = (System.Drawing.Bitmap)Image_to_trans.Clone();
            double distance = 0;
            //对结果图片的每一个点进行反向插值
            for (int i = 0; i < pict_wid; i++)
            {
                for (int j = 0; j < pict_hei; j++)
                {
                    distance = Math.Sqrt((i - x_center) * (i - x_center) + (j - y_center) * (j - y_center));
                    if (distance < radius)
                    {
                        //旋转变形函数实现 坐标映射
                        double angel_ij = (Angel * (radius - distance) / radius) * direction * Math.PI / 180;
                        double x = (i - x_center) * Math.Cos(angel_ij) - (j - y_center) * Math.Sin(angel_ij) + x_center;
                        double y = (i - x_center) * Math.Sin(angel_ij) + (j - y_center) * Math.Cos(angel_ij) + y_center;
                        int[] temp = new int[3];
                        //边界处理 防止溢出
                        if (x > 1 && x < pict_wid -2 && y > 1 && y < pict_hei -2)
                        {
                            switch (method)
                            {
                                //最近邻插值
                                case 0:
                                    temp = Nearest_interpolation(x, y, Image_to_trans);
                                    Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                    break;
                                //双线性插值
                                case 1:
                                    //边界处理
                                    if(x > 1 && x < pict_wid -2 && y > 1 && y < pict_hei -2 )
                                    {
                                        temp = Bilinear_interpolation(x, y, Image_to_trans);
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                    }
                                    else
                                    {
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                                    }
                                    break;
                                //双三次插值
                                case 2:
                                    //边界处理
                                    if (x > 1 && x < pict_wid -3 && y > 1 && y < pict_hei -3)
                                    {
                                        temp = Bicubic_interpolation(x, y, Image_to_trans);
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                    }
                                    else
                                    {
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                                    }
                                    break;
                                default:
                                    break;

                            }
                        }
                        else
                        {
                            //超出范围的映射点置为黑色
                            Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));

                        }

                    }
                    //旋转半径外的点保留其原有像素值
                    else
                    {
                        Image_after_trans.SetPixel(i, j, Image_to_trans.GetPixel(i, j));

                    }

                }
            }
            return Image_after_trans;
        }
        //畸变校正
        //功能：实现图像的桶形畸变或枕形畸变
        public Bitmap Distortion_Image(double radius, int method , int method_trans)
        {
            int pict_wid = Image_to_trans.Width;
            int pict_hei = Image_to_trans.Height;
            Image_after_trans = (System.Drawing.Bitmap)Image_to_trans.Clone();
            //获取图像中心点位置
            double x_center = (pict_wid + 1) / 2;
            double y_center = (pict_hei + 1) / 2;
            double distance = 0;
            //逐坐标进行插值
            for(int i = 0;i<pict_wid;i++)
            {
                for(int j = 0;j<pict_hei;j++)
                {
                    distance = Math.Sqrt((i - x_center) * (i - x_center) + (j - y_center) * (j - y_center));
                    if(distance < radius)
                    {
                        double x, y;
                        if (distance != 0)
                        {
                            //坐标映射关系 当前为桶形畸变
                            if (method_trans == 1)
                            {
                                x = (i - x_center) / (radius / (distance) * Math.Asin(distance / radius)) + x_center;
                                y = (j - y_center) / (radius / (distance)* Math.Asin(distance / radius)) + y_center;
                            }
                            //坐标映射为枕形畸变
                            else
                            {
                                x = (i - x_center) * (radius / (distance) * Math.Asin(distance / radius)) + x_center;
                                y = (j - y_center) * (radius / (distance) * Math.Asin(distance / radius)) + y_center;
                            }
                            
                        }
                        else //中心点不带入上述公式 直接映射
                        {
                            x = x_center;
                            y = y_center;
                        }
                        //边界处理
                        if(x>0&&x<pict_wid -1 &&y>0&&y<pict_hei -1)
                        {
                            int[] temp = new int[3];
                            switch (method)
                            {
                                case 0:
                                    temp = Nearest_interpolation(x, y, Image_to_trans);
                                    Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                    break;
                                case 1:
                                    if (x > 1 && x < pict_wid -2 && y > 1 && y < pict_hei -2)
                                    {
                                        temp = Bilinear_interpolation(x, y, Image_to_trans);
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                    }
                                    else
                                    {
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                                    }
                                    break;
                                case 2:
                                    if (x > 1 && x < pict_wid-3 && y > 1 && y < pict_hei -3)
                                    {
                                        temp = Bicubic_interpolation(x, y, Image_to_trans);
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                    }
                                    else
                                    {
                                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                                    }
                                    break;
                                default:
                                    break;

                            }
                        }

                        else
                        {
                            Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                        }
                    }
                    else
                    {
                        Image_after_trans.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0,0));
                    }
                    
                }
            }
            return Image_after_trans;
        }


        //最近邻插值函数
        //输出：int []型矩阵，对应RGB三通道的像素值
        public int[] Nearest_interpolation(double x, double y, Bitmap bt)
        {
            int[] result = new int[3];
            //分别去最近邻整数点
            int near_x = (int)Math.Round(x);
            int near_y = (int)Math.Round(y);
            result[0] = bt.GetPixel(near_x, near_y).R;
            result[1] = bt.GetPixel(near_x, near_y).G;
            result[2] = bt.GetPixel(near_x, near_y).B;
            return result;
        }

        //双线性插值函数
        public int[] Bilinear_interpolation(double x, double y, Bitmap bt)
        {
            int[] result= new int[3];
            int near_x_min = (int)Math.Floor(x);
            int near_y_min = (int)Math.Floor(y);
            int near_x_max = near_x_min + 1;
            int near_y_max = near_y_min + 1;
            //转换为矩阵
            int[] f_1_1 = { bt.GetPixel(near_x_min, near_y_min).R, bt.GetPixel(near_x_min, near_y_min).G, bt.GetPixel(near_x_min, near_y_min).B};
            int[] f_1_2 = { bt.GetPixel(near_x_min, near_y_max).R, bt.GetPixel(near_x_min, near_y_max).G, bt.GetPixel(near_x_min, near_y_max).B };
            int[] f_2_1 = { bt.GetPixel(near_x_max, near_y_min).R, bt.GetPixel(near_x_max, near_y_min).G, bt.GetPixel(near_x_max, near_y_min).B };
            int[] f_2_2 = { bt.GetPixel(near_x_max, near_y_max).R, bt.GetPixel(near_x_max, near_y_max).G, bt.GetPixel(near_x_max, near_y_max).B };
            //对其二次型展开后的解析式累加
            for(int i =0;i<3;i++)
            {
                double temp =(f_1_1[i] * (near_x_max - x) * (near_y_max - y) + f_2_1[i]*(x - near_x_min)*(near_y_max - y));
                result[i] = (int)(temp + f_1_2[i]* (near_x_max - x)*(y-near_y_min) + f_2_2[i]*(x-near_x_min)*(y-near_y_min));
            }
            return result;

        }
        //双三次插值函数
        public int[] Bicubic_interpolation(double x, double y, Bitmap bt)
        {
            //向下取整
            int i = (int)Math.Floor(x);
            int j = (int)Math.Floor(y);
            //取小数部分
            double u = x - Math.Floor(x);
            double v = y - Math.Floor(y);
            int[] result = new int[3];
            double[] temp_result = { 0 , 0, 0 };
            double[] A = { Sx_For_Bicubic(u + 1), Sx_For_Bicubic(u), Sx_For_Bicubic(u - 1), Sx_For_Bicubic(u - 2) };
            double[] C = { Sx_For_Bicubic(v + 1), Sx_For_Bicubic(v), Sx_For_Bicubic(v - 1), Sx_For_Bicubic(v - 2) };
            //结果转换为二次型累加
            for(int m = 0;m<4;m++)
            {
                for(int n = 0;n<4;n++)
                {
                    temp_result[0] += A[m] * bt.GetPixel(i - 1 + m, j - 1 + n).R * C[n];
                    temp_result[1] += A[m] * bt.GetPixel(i - 1 + m, j - 1 + n).G * C[n];
                    temp_result[2] += A[m] * bt.GetPixel(i - 1 + m, j - 1 + n).B * C[n];
                }
            }
            //对于越界部分的处理
            for(int k=0;k<3;k++)
            {
                result[k] = (int)Math.Floor(temp_result[k]);
                if(result[k]> 255)
                {
                    result[k] = 255;
                }
                else if(result[k]<0)
                {
                    result[k] = 0;
                }
            }
            return result;
        }
        //双三次插值使用的中间函数
        public double Sx_For_Bicubic(double x)
        {
            double absx = Math.Abs(x);
            if (absx <= 1)
            {
                return (1 - 2 * absx * absx + absx * absx * absx);
            }
            else if (absx < 2)
            {
                return (4 - 8 * absx + 5 * absx * absx - absx * absx * absx);
            }
            else return 0;
        }

        //TPS人脸变形操作
        //参数：tps：TPS求解系数 method:选用的插值函数 P：控制点矩阵
        //功能：实现待转换人脸向目标人脸的转换操作
        public Bitmap TPS_face(Matrix tps,int method,Matrix P)
        {
            //获取原图的尺寸
            int pict_wid = Image_face_1.Width;
            int pict_hei = Image_face_1.Height;
            Image_face_result = (System.Drawing.Bitmap)Image_face_1.Clone();
            for (int i = 0;i<pict_wid;i++)
            {
                for(int j = 0;j<pict_hei;j++)
                {
                    //TPS变形函数
                    double x = TPS_xy(P, tps, i, j)[0];
                    double y = TPS_xy(P, tps, i, j)[1];
                    int[] temp = new int[3];
                    if (x > 1 && x < pict_wid - 2 && y > 1 && y < pict_hei - 2)
                    {
                        switch (method)
                        {
                            case 0:
                                temp = Nearest_interpolation(x, y, Image_face_1);
                                Image_face_result.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                break;
                            case 1:
                                if (x > 1 && x < pict_wid - 2 && y > 1 && y < pict_hei - 2)
                                {
                                    temp = Bilinear_interpolation(x, y, Image_face_1);
                                    Image_face_result.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                }
                                else
                                {
                                    Image_face_result.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                                }
                                break;
                            case 2:
                                if (x > 1 && x < pict_wid - 3 && y > 1 && y < pict_hei - 3)
                                {
                                    temp = Bicubic_interpolation(x, y, Image_face_1);
                                    Image_face_result.SetPixel(i, j, System.Drawing.Color.FromArgb(temp[0], temp[1], temp[2]));
                                }
                                else
                                {
                                    Image_face_result.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                                }
                                break;
                            default:
                                break;

                        }
                    }
                    else
                    {
                        Image_face_result.SetPixel(i, j, System.Drawing.Color.FromArgb(0, 0, 0));
                    }
                }
            }
            return Image_face_result;
        }

        //TPS变形函数
        //计算TPS映射到的坐标
        public double[] TPS_xy(Matrix P,Matrix TPS,int x,int y)
        {
            double[] result = new double[2];
            result[0] = result[1] = 0;
            //按公式进行累加
            for(int i = 0;i<P.row;i++)
            {
                result[0] += TPS[i, 0] * TPS.U_r(P[i, 1], P[i, 2], (double)x, (double)y);
                result[1] += TPS[i, 1] * TPS.U_r(P[i, 1], P[i, 2], (double)x, (double)y);
            }
            int num = P.row;
            result[0] += TPS[num, 0] + TPS[num + 1, 0] * x + TPS[num + 2, 0] * y;
            result[1] += TPS[num, 1] + TPS[num + 1, 1] * x + TPS[num + 2, 1] * y;
            return result;
        }

    }
}
