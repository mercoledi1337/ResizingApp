using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace test127
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files|*.jpg;*.jpeg";
            openFileDialog.FilterIndex = 1;
            if (openFileDialog.ShowDialog() == true)
            {
                Bitmap bit = new Bitmap(openFileDialog.FileName);
                JamesBond.Height = bit.Height;
                JamesBond.Width = bit.Width;
                JamesBond.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PixelColor
        {
            public byte Blue;
            public byte Green;
            public byte Red;
            public byte Alpha;
        }

        public PixelColor[,] GetPixels(BitmapSource source)
        {
            if (source.Format != PixelFormats.Bgra32)
                source = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            PixelColor[,] result = new PixelColor[width, height];

            source.CopyPixels(result, width * 4, 0);
            return result;
        }

        public void PutPixels(WriteableBitmap bitmap, PixelColor[,] pixels, int x, int y)
        {
            int width = pixels.GetLength(0);
            int height = pixels.GetLength(1);
            bitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * 4, x, y);
        }



        public struct Cord
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Energy { get; set; }
        }

        private async Task<int[,]> SeamMap(int[,] energyMap, Cord[] oldPath = null)
        {
            int[,] seamMap = new int[energyMap.GetLength(0), energyMap.GetLength(1)];

                for (int i = 0; i < energyMap.GetLength(0); i++)
                {
                    seamMap[i, 0] = energyMap[i, 0];
                }

                for (int i = 1; i < seamMap.GetLength(1); i++)
                {
                    for (int j = 0; j < seamMap.GetLength(0); j++)
                    {
                        if (j == 0)
                        {
                            if (seamMap[0, i - 1] < seamMap[1, i - 1])
                            {
                                seamMap[j, i] = seamMap[0, i - 1] + energyMap[j, i];
                            }
                            else
                            {
                                seamMap[j, i] = seamMap[1, i - 1] + energyMap[j, i];
                            }
                        }
                        else if (j == seamMap.GetLength(0) - 1)
                        {
                            if (seamMap[j, i - 1] < seamMap[j - 1, i - 1])
                            {
                                seamMap[j, i] = seamMap[j, i - 1] + energyMap[j, i];
                            }
                            else
                            {
                                seamMap[j, i] = seamMap[j - 1, i - 1] + energyMap[j, i];
                            }
                        }
                        else
                        {
                            if (seamMap[j - 1, i - 1] < seamMap[j, i - 1] && seamMap[j - 1, i - 1] < seamMap[j + 1, i - 1])
                            {
                                seamMap[j, i] = seamMap[j - 1, i - 1] + energyMap[j, i];
                            }
                            else if (seamMap[j, i - 1] < seamMap[j + 1, i - 1])
                            {
                                seamMap[j, i] = seamMap[j, i - 1] + energyMap[j, i];
                            }
                            else
                            {
                                seamMap[j, i] = seamMap[j + 1, i - 1] + energyMap[j, i];
                            }

                        }
                    }
                }

            return seamMap;
        }
        private async Task<Cord[]> GetPath(int[,] energyMap, Cord[] oldPath = null)
        {
            Cord[] res = new Cord[energyMap.GetLength(1)];

            int[,] seamMap = await SeamMap(energyMap, oldPath);

            Cord StartMin = new Cord();
            StartMin.Y = seamMap.GetLength(1) - 1;
            StartMin.Energy = seamMap[0, seamMap.GetLength(1) - 1]; // first pixel

            //get smallest energy in last line!!!!!
            for (int i = 1; i < seamMap.GetLength(0); i++)
            {
                if (StartMin.Energy > seamMap[i, seamMap.GetLength(1) - 1])
                {
                    StartMin.Energy = seamMap[i, seamMap.GetLength(1) - 1];
                    StartMin.X = i;
                }
            }
            res[seamMap.GetLength(1) - 1] = StartMin;

            //line above
            for (int y = seamMap.GetLength(1) - 2; y >= 0; y--)
            {
                int variableForSearching = res[y + 1].X; //x where we have smallest seam 
                Cord min = new Cord();
                min.Y = y;
                min.Energy = seamMap[variableForSearching, y];
                min.X = variableForSearching;

                //left
                if (variableForSearching == 0)
                {
                    if (min.Energy > seamMap[variableForSearching + 1, y])
                    {
                        min.Energy = seamMap[variableForSearching + 1, y];
                        min.X = variableForSearching + 1;
                    }

                }
                //right
                else if (variableForSearching == seamMap.GetLength(0) - 1)
                {
                    if (min.Energy > seamMap[variableForSearching - 1, y])
                    {
                        min.Energy = seamMap[variableForSearching - 1, y];
                        min.X = variableForSearching - 1;
                    }
                }
                //mid
                else
                {
                    if (min.Energy > seamMap[variableForSearching + 1, y] && seamMap[variableForSearching - 1, y] > seamMap[variableForSearching + 1, y])
                    {
                        min.Energy = seamMap[variableForSearching + 1, y];
                        min.X = variableForSearching + 1;
                    }
                    else if (min.Energy > seamMap[variableForSearching - 1, y])
                    {
                        min.Energy = seamMap[variableForSearching - 1, y];
                        min.X = variableForSearching - 1;
                    }
                }

                res[y] = min;
            }

            return res;
        }


        //make this dont locking and unlocking whole time

        private async Task<byte[]> DeletingPathLockBits(int stride, byte[] rgbValues, Cord[] pathToDelete)
        {


            //making array to store energy


            // Copy the RGB values into the array.


            int y = 0;
            //for moving in rgbArray
            for (int counter = 0; counter < rgbValues.Length; counter +=stride)
            {
                //change with smallest energy with next one
                int pixelToRemove = (pathToDelete[y].X * 4) + counter;

                //changing next pixels in row
                for (int counterX = pixelToRemove; counterX < counter + stride; counterX += 4)
                {
                    if (counterX + 4 != counter + stride)
                    {
                        rgbValues[counterX] = rgbValues[counterX + 4];
                        rgbValues[counterX + 1] = rgbValues[counterX + 5];
                        rgbValues[counterX + 2] = rgbValues[counterX + 6];
                        rgbValues[counterX + 3] = rgbValues[counterX + 7];


                    }
                    else
                    {
                        rgbValues[counterX] = 255;
                        rgbValues[counterX + 1] = 255;
                        rgbValues[counterX + 2] = 255;
                    }

                }

                y++;
               

            }

            return rgbValues;
        }

        private async Task<byte[]> DoublePathLockBits( int stride, int heigth, byte[] rgbValues1, Cord[] pathToDelete)
        {

            int bytes = rgbValues1.Length + (heigth * 4);
            byte[] rgbValues = new byte[bytes];


            //for(int i = 0; i < rgbValues1.Length; i++)
            //{
            //    rgbValues[i] = rgbValues1[i];
            //}
            int pam = 0;
            int pam1 = 0;

            //filling new one 
            for (int counter = 0; counter < rgbValues1.Length; counter += stride)
            {

                //changing next pixels in row
                for (int counterX = counter; counterX < counter + stride; counterX += 4)
                {


                    rgbValues[counterX + pam] = rgbValues1[counterX];
                    rgbValues[counterX + pam + 1] = rgbValues1[counterX + 1];
                    rgbValues[counterX + pam + 2] = rgbValues1[counterX + 2];
                    rgbValues[counterX + pam + 3] = rgbValues1[counterX + 3];

                }
                pam += 4;

            }
            for (int counter = stride; counter < rgbValues.Length; counter += stride + 4)
            
            {
                rgbValues[counter] = 255;
                rgbValues[counter + 1] = 255;
                rgbValues[counter + 2] = 255;
                rgbValues[counter + 3] = 125;
            }




                int y = 0;
           // for moving in rgbArray
            for (int counter = 0; counter < rgbValues.Length; counter += stride + 4)
                {
                //change with smallest energy with next one
                    int pixelToRemove = (pathToDelete[y].X * 4) + counter;

                //changing next pixels in row
                for (int counterX = counter + stride; counterX > pixelToRemove; counterX -= 4)
                {
                    rgbValues[counterX] = rgbValues[counterX - 4];
                    rgbValues[counterX + 1] = rgbValues[counterX - 3];
                    rgbValues[counterX + 2] = rgbValues[counterX - 2];
                    rgbValues[counterX + 3] = rgbValues[counterX - 1];

                }



                y++;


                }




            return rgbValues;
        }

        private byte[] DrawDeletingPathLockBits(int stride, byte[] rgbValues, Cord[] pathToDelete)
        {


            //making array to store energy
         



            int x = stride - 4;
            int y = 0;
            for (int counter = 0; counter < rgbValues.Length; counter += stride)
            {
                //change with smallest energy with next one
                int pixelToRemove = (pathToDelete[y].X * 4) + counter;

                rgbValues[pixelToRemove] = 0;
                rgbValues[pixelToRemove + 1] = 0;
                rgbValues[pixelToRemove + 2] = 0;

                y++;

            }

            return rgbValues;
        }


        private async Task<int> EnergyLockBits(int leftR, int leftG, int leftB, int midR, int midG, int midB, int rightR, int rightG, int rightB)
        {
            double lEnergy = 0;

            if (leftR != -1)
            {
                lEnergy = Math.Pow(leftR - midR, 2) + Math.Pow(leftG - midG, 2) + Math.Pow(leftB - midB, 2);
            }

            double rEnergy = 0;

            if (rightR != -1)
            {
                rEnergy = Math.Pow(rightR - midR, 2) + Math.Pow(rightG - midG, 2) + Math.Pow(rightB - midB, 2);
            }
            return (int)Math.Sqrt(lEnergy + rEnergy);
        }

        private async Task<int[,]> EnergyMapWithLockBits(int width, int height, int stride, byte[] rgbValues, int DeletedCount = 0, Cord[] cords = null, int[,] oldEnergyMap = null)
        {
            
            int[,] ress = new int[width - DeletedCount, height];

            int leftBorder = 0;
            int rightBorder = stride - 4 - (DeletedCount * 4);

            int x = 0;
            int y = 0;
            if (cords == null)
            {
                for (long counter = 0; counter < rgbValues.Length; counter += 4)
                {
                    if (leftBorder == counter)
                    {
                        ress[x, y] = await EnergyLockBits(
                            -1, -1, -1
                            , rgbValues[counter], rgbValues[counter + 1], rgbValues[counter + 2] // 4 bytes, alpha dont used
                            , rgbValues[counter + 4], rgbValues[counter + 5], rgbValues[counter + 6]);
                        leftBorder += stride - (DeletedCount * 4);
                        x++;
                        continue;
                    }
                    else if (rightBorder == counter)
                    {
                        ress[x, y] = await EnergyLockBits(
                           rgbValues[counter - 4], rgbValues[counter - 3], rgbValues[counter - 2]
                           , rgbValues[counter], rgbValues[counter + 1], rgbValues[counter + 2]
                           , -1, -1, -1);
                        rightBorder += stride - (DeletedCount * 4);
                        x = 0;
                        y++;
                        continue;
                    }
                    else
                    {
                        ress[x, y] = await EnergyLockBits(
                           rgbValues[counter - 4], rgbValues[counter - 3], rgbValues[counter - 2]
                           , rgbValues[counter], rgbValues[counter + 1], rgbValues[counter + 2]
                           , rgbValues[counter + 4], rgbValues[counter + 5], rgbValues[counter + 6]);
                        x++;
                    }
                }
            }
            else
            {
                int cordIterator = 0;
                int forMovingInCords = 0;
                for (int y1 = 0; y1 < ress.GetLength(1); y1++)
                {
                    for (int x1 = 0; x1 < ress.GetLength(0); x1++)
                    {
                        if (x1 < cords[cordIterator].X)
                        {
                            ress[x1, y1] = oldEnergyMap[x1, y1];
                        }
                        else
                        {
                            ress[x1, y1] = oldEnergyMap[x1 + 1, y1];
                        }
                    }
                    cordIterator++;
                }

                cordIterator = 0;
                int y2 = 0;
                for (int counterX = 0; counterX < rgbValues.Length; counterX += stride)
                {
                    //change with smallest energy with next one
                    int pixelToRemove = (cords[y2].X * 4) + counterX;

                    if (cords[y2].X == 0)
                    {
                        ress[cords[y2].X, cords[y2].Y] = await EnergyLockBits(
                        -1, -1, -1
                        , rgbValues[pixelToRemove], rgbValues[pixelToRemove + 1], rgbValues[pixelToRemove + 2] // 4 bytes, alpha dont used
                        , rgbValues[pixelToRemove + 4], rgbValues[pixelToRemove + 5], rgbValues[pixelToRemove + 6]);
                    }
                    else if (cords[y2].X == (stride - (DeletedCount * 4)) / 4)
                    {
                        if (pixelToRemove + 1 >= rgbValues.Length)
                        {
                            continue;
                        }
                        var rgb1 = await EnergyLockBits(
                        rgbValues[pixelToRemove - 4], rgbValues[pixelToRemove - 3], rgbValues[pixelToRemove - 2]
                           , rgbValues[pixelToRemove ], rgbValues[pixelToRemove + 1], rgbValues[pixelToRemove + 2]
                        , -1, -1, -1);
                        ress[cords[y2].X - 1, cords[y2].Y] = rgb1;
                    }
                    else
                    {//jakiś bag wychodzi za granice
                        if(pixelToRemove + 4 >= rgbValues.Length)
                        {
                            continue;
                        }
                        var a = rgbValues[pixelToRemove + 4];
                        var b = rgbValues[pixelToRemove + 5];
                        var c = rgbValues[pixelToRemove + 6];
                        var rgb = await EnergyLockBits(
                            rgbValues[pixelToRemove - 4], rgbValues[pixelToRemove - 3], rgbValues[pixelToRemove - 2]
                            , rgbValues[pixelToRemove], rgbValues[pixelToRemove + 1], rgbValues[pixelToRemove + 2]
                            , a,b,c);

                        ress[cords[y2].X, cords[y2].Y] = rgb;

                        if (cords[y2].X - 1 == 0)
                        {
                            ress[cords[y2].X, cords[y2].Y] = await EnergyLockBits(
                        -1, -1, -1
                        , rgbValues[pixelToRemove], rgbValues[pixelToRemove + 1], rgbValues[pixelToRemove + 2] // 4 bytes, alpha dont used
                        , rgbValues[pixelToRemove + 4], rgbValues[pixelToRemove + 5], rgbValues[pixelToRemove + 6]);
                        }
                        else
                        {
                            ress[cords[y2].X - 1, cords[y2].Y] = await EnergyLockBits(
                            rgbValues[pixelToRemove - 8], rgbValues[pixelToRemove - 7], rgbValues[pixelToRemove - 6]
                           , rgbValues[pixelToRemove - 4], rgbValues[pixelToRemove - 3], rgbValues[pixelToRemove - 2]
                           , rgbValues[pixelToRemove], rgbValues[pixelToRemove + 1], rgbValues[pixelToRemove + 2]);
                        }
                    }

                    y2++;

                }
            }

            return ress;
        }

        public static byte[] Array1DFromBitmap(Bitmap bmp)
        {
            if (bmp == null) throw new NullReferenceException("Bitmap is null");

            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData data = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = data.Scan0;

            //declare an array to hold the bytes of the bitmap
            int numBytes = data.Stride * bmp.Height;
            byte[] bytes = new byte[numBytes];

            //copy the RGB values into the array
            System.Runtime.InteropServices.Marshal.Copy(ptr, bytes, 0, numBytes);

            bmp.UnlockBits(data);

            return bytes;
        }

        public static Bitmap BitmapFromArray1D(byte[] bytes, int width, int height)
        {
            Bitmap grayBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Rectangle grayRect = new Rectangle(0, 0, grayBmp.Width, grayBmp.Height);
            BitmapData grayData = grayBmp.LockBits(grayRect, ImageLockMode.ReadWrite, grayBmp.PixelFormat);
            IntPtr grayPtr = grayData.Scan0;

            int grayBytes = grayData.Stride * grayBmp.Height;


            System.Runtime.InteropServices.Marshal.Copy(bytes, 0, grayPtr, grayBytes);

            grayBmp.UnlockBits(grayData);
            return grayBmp;
        }

        private int[,] doublePathInEnergyMap(Cord[] a, int[,] eMap)
        {
            int[,] res = new int[eMap.GetLength(0) + 1, eMap.GetLength(1)];

            for(int y = 0;y < res.GetLength(1); y++)
            {
                for (int x = a[y].X + 2; x < res.GetLength(0); x++)
                {
                    res[x,y] = eMap[x - 1,y];
                }
                for (int x = 0; x <= a[y].X; x++)
                {
                    res[x, y] = eMap[x, y];
                }
                res[a[y].X, y] = 100000;
                res[a[y].X + 1, y] = 100000;

            }
            return res;
        }

        private async Task<Bitmap> ScaleImageAsync(Bitmap bit, IProgress<int> progress,  int percentW = 0)
        {
            int widthBitmap = (int)bit.Width;
            int scale = 0;
            if (percentW > 100)
            {
                scale = (int)(widthBitmap * (((double)percentW - 100) / 100));
            }
            else
            {
                scale = (int)(widthBitmap * ((double)percentW / 100));
            }
            

            var arr = Array1DFromBitmap(bit);

            var a = await EnergyMapWithLockBits(bit.Width, bit.Height, bit.Width * 4, arr);
            Bitmap resized = BitmapFromArray1D(arr, bit.Width, bit.Height);
            Cord[] cords111 = await GetPath(a);

            if (percentW > 100)
            {
                for (int i = 0; i < scale; i++)
                {


                    arr = await DoublePathLockBits((bit.Width + i) * 4, bit.Height, arr, cords111);
                    a = doublePathInEnergyMap(cords111, a);
                    cords111 = await GetPath(a);
                    var per = (i * 100) / scale;
                    progress.Report(per);
                    Dispatcher.Invoke(new Action(() =>
                    {
                        resized = BitmapFromArray1D(arr, bit.Width + i + 1, bit.Height);
                        resized = resized.Clone(new System.Drawing.Rectangle(0, 0, (int)resized.Width, resized.Height), resized.PixelFormat);
                        JamesBond.Width = resized.Width;
                        JamesBond.Height = resized.Height;
                        JamesBond.Source = ToBitmapImage(resized);
                    }));

                }
                resized = BitmapFromArray1D(arr, bit.Width + scale, bit.Height);
                resized = resized.Clone(new System.Drawing.Rectangle(0, 0, (int)resized.Width, resized.Height), resized.PixelFormat);


            }



            else
            {

                for (int i = 1; i < scale; i++)
                {
                    a = await EnergyMapWithLockBits(bit.Width, bit.Height, bit.Width * 4, arr, i, cords111, a);
                    cords111 = await GetPath(a);
                    

                    var per = (i * 100) / scale;
                    bit = BitmapFromArray1D(DrawDeletingPathLockBits(bit.Width * 4, arr, cords111), bit.Width, bit.Height);
                    

                    progress.Report(per);

                    Dispatcher.Invoke(new Action(() =>
                    {
                        bit = BitmapFromArray1D(arr, bit.Width, bit.Height);
                        JamesBond.Source = ToBitmapImage(bit.Clone(new System.Drawing.Rectangle(0, 0, (int)bit.Width - i, bit.Height), bit.PixelFormat));
                    }));


                   arr = await DeletingPathLockBits(bit.Width * 4, arr, cords111);


                    Dispatcher.Invoke(new Action(() =>
                    {
                        bit = BitmapFromArray1D(arr, bit.Width, bit.Height);
                        JamesBond.Source = ToBitmapImage(bit.Clone(new System.Drawing.Rectangle(0, 0, (int)bit.Width - i, bit.Height), bit.PixelFormat));
                    }));

                }



                resized = bit.Clone(new System.Drawing.Rectangle(0, 0, (int)bit.Width - scale, bit.Height), bit.PixelFormat);
            }
           

            //Bitmap resized = bit.Clone(new System.Drawing.Rectangle(0, 0, (int)bit.Width - scale, bit.Height), bit.PixelFormat);

            return resized;
        }

        public Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap(
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            BitmapData data = bmp.LockBits(
              new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
              ImageLockMode.WriteOnly,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            source.CopyPixels(
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
    {
        using (var memory = new MemoryStream())
        {
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            return bitmapImage;
        }
    }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {

            Bitmap bit = GetBitmap((BitmapImage)JamesBond.Source);

            var progress = new Progress<int>(value =>
            {
                pbStatus.Value = value;
            });
            int h = 0;
            int w = 0;
            try
            {
                h = Convert.ToInt32(Height.Text);
                w = Convert.ToInt32(width.Text);
                
            }
            catch(FormatException ex)
            {

            }

            bit.RotateFlip(RotateFlipType.Rotate90FlipNone);
            bit = await Task.Run(() => ScaleImageAsync(bit, progress, h));
            bit.RotateFlip(RotateFlipType.Rotate270FlipNone);
            bit = await Task.Run(() => ScaleImageAsync(bit, progress, w));

            JamesBond.Width = bit.Width;
            JamesBond.Height = bit.Height;
            JamesBond.Source = ToBitmapImage(bit);

            pbStatus.Value = 100;
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void WindowStateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow.WindowState != WindowState.Maximized)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ImagePanel_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                Bitmap bit = new Bitmap(files[0]);
                JamesBond.Height = bit.Height;
                JamesBond.Width = bit.Width;
                JamesBond.Source = new BitmapImage(new Uri(files[0]));
            }
        }


    }
}
