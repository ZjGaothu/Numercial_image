using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// 模块名：矩阵类库
/// 功能：实现求解TPS所需的矩阵运算功能
/// 作者：高子靖
/// </summary>
/// 
namespace Numerical_image1
{
    class Matrix
    {
        //行数列数和数据
        public int row;
        public int col;
        public double[,] data_M;

        public Matrix()
        {
            row = 0;
            col = 0;
        }

        //构造函数
        public Matrix(int _row, int _col)
        {
            row = _row; col = _col;
            data_M = new double[_row, _col];
        }
        //拷贝构造函数
        public Matrix(Matrix a)
        {
            row = a.row;
            col = a.col;
            data_M = new double[row, col];
            for(int i = 0;i<row;i++)
            {
                for(int j = 0;j<col; j++)
                {
                    data_M[i, j] = a.data_M[i, j];
                }
            }
        }

        //对矩阵数据的赋值和读取
        public double this[int x, int y]
        {
            get
            {
                return data_M[x, y];
            }
            set
            {
                data_M[x, y] = value;
            }
        }

        //矩阵转置
        public Matrix transpose(Matrix Mat)
        {
            int row = Mat.row;
            int col = Mat.col;

            Matrix Result_Mat = new Matrix(col,row);

            for( int i=0;i < col; i++)
            {
                for(int j = 0; j < row; j++)
                {
                    Result_Mat[i, j] = Mat[j, i]; 
                }
            }
            return Result_Mat;
        }

        //矩阵求逆 前提为方阵
        //采用增广矩阵与高斯消元法
        public Matrix Ionverse_matr(Matrix Mat)
        {
            int n = Mat.row;
            Matrix temp = new Matrix(n, 2 * n);
            Matrix result = new Matrix(n, n);
            //初始化增广矩阵
            for(int i= 0; i< n;i++)
            {
                for(int j = 0; j< 2*n;j++)
                {
                    if(j<n)
                    {
                        temp[i, j] = Mat[i,j];
                    }
                    else
                    {
                        if(i == j-n)
                        {
                            temp[i, j] = 1;
                        }
                        else
                        {
                            temp[i, j] = 0;
                        }
                    }
                }
            }

            //进行增广矩阵的高斯消元
            //首先化为上三角矩阵
            for(int i = 0; i < n;i++)
            {
                //为防止出现double相减并不是等于0的情况 这里设置的最小数为double的精度
                if(temp[i,i] < 1e-16)
                {
                    int j;
                    for(j = i+1;j<n;j++)
                    {
                        if(temp[j,i] != 0)
                        { break; }
                    }
                    //在tps中不会出现不可逆
                    if(j == n)
                    {
                        return null;
                    }
                    //两行相加
                    for(int num = 0;num<2*n;num++)
                    {
                        temp[i, num] += temp[j, num];
                    }
                }
                //将这一行的对角线元素化为1
                double temp_ii_now = temp[i, i]; 
                for(int m = i;m < 2*n;m++)
                {
                    temp[i, m] = temp[i, m] / temp_ii_now;
                }
                //将该列全部置0
                for(int j = i+1;j<n;j++)
                {
                    //比例系数
                    double coefficient = temp[j, i];
                    for(int num = i; num<2*n;num++)
                    {
                        temp[j, num] = temp[j, num] - coefficient * temp[i, num];
                    }
                }
            }
            //化为对角矩阵
            for(int i = n-1;i>=0;i--)
            {
                for(int j = i-1;j>= 0; j--)
                {
                    double coefficient = temp[j, i];
                    for (int num = 0;num<2*n;num++)
                    {
                        temp[j,num] = temp[j,num] - coefficient * temp[i, num];
                    }
                }
            }
            //对结果赋值
            for(int i = 0;i<n;i++)
            {
                for(int j = 0;j<n;j++)
                {
                    result[i, j] = temp[i, j + n];
                }
            }
            return result;

        }

        //矩阵乘法
        public Matrix Matrix_Multi(Matrix mat1,Matrix mat2)
        {
            int row_1 = mat1.row;
            int col_1 = mat1.col;
            int row_2 = mat2.row;
            int col_2 = mat2.col;
            Matrix result = new Matrix(row_1, col_2);
            if(col_1 != row_2)
            {
                return null;
            }
            else
            {
                for(int i = 0;i<row_1;i++)
                {
                    for(int j = 0;j<col_2;j++)
                    {
                        result[i, j] = 0;
                        for(int k = 0;k<col_1;k++)
                        {
                            result[i, j] += mat1[i, k] * mat2[k, j];
                        }
                    }
                }
                return result;
            }
        }

        //横向合并矩阵
        public Matrix Horizontal_integra(Matrix mat_left, Matrix mat_right)
        {
            int width_1 = mat_left.col;
            int width_2 = mat_right.col;
            int row_1 = mat_left.row;
            int row_2 = mat_right.row;
            if(row_1 != row_2)
            {
                return null;
            }
            Matrix result = new Matrix(row_1, width_1 + width_2);
            for(int i = 0;i<row_1;i++)
            {
                for(int j = 0;j<width_1 + width_2; j++)
                {
                    result[i, j] = j < width_1 ? mat_left[i, j] : mat_right[i, j - width_1];
                }
            }
            return result;
        }
        //纵向合并矩阵
        public Matrix Vertical_integra(Matrix mat_up, Matrix mat_down)
        {
            int col_1 = mat_up.col;
            int col_2 = mat_down.col;
            int row_1 = mat_up.row;
            int row_2 = mat_down.row;
            if (col_1 != col_2)
            {
                return null;
            }
            Matrix result = new Matrix(row_1 + row_2, col_1);
            for(int i = 0;i<row_1+row_2;i++)
            {
                for(int j = 0;j<col_1;j++)
                {
                    result[i, j] = i < row_1 ? mat_up[i, j] : mat_down[i - row_1, j];
                }
            }
            return result;
        }

        //TPS使用径向基函数
        public double U_r(double x_1, double y_1, double x_2, double y_2)
        {
            if (x_1 == x_2 && y_1 == y_2)
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

    }
}
