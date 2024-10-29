using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Path = System.IO.Path;
using Xceed.Words.NET;


namespace RSA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            rd_tudong.IsChecked = true;
            rd_tuychon.IsChecked = false;
        }
        BigInteger N, P, Q, phiN, E, D, tmpD, tmpN;

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void rd_tudongCheck(object sender, RoutedEventArgs e)
        {
            soP.Text = soQ.Text = soPhiN.Text = soN.Text = soE.Text = soD.Text = soN2.Text = string.Empty;
            soP.IsEnabled = soQ.IsEnabled = soPhiN.IsEnabled = soN.IsEnabled = soE.IsEnabled = soD.IsEnabled = soN2.IsEnabled = false;
        }

        private void rd_tuychonCheck(object sender, RoutedEventArgs e)
        {
            soP.Text = soQ.Text = soPhiN.Text = soN.Text = soE.Text = soD.Text = soN2.Text = string.Empty;
            soP.IsEnabled = soQ.IsEnabled = true;
            soPhiN.IsEnabled = soN.IsEnabled = soE.IsEnabled = soD.IsEnabled = soN2.IsEnabled = false;
        }

        private void btn_TaoKhoa_Click(object sender, RoutedEventArgs e)
        {
            if (rd_tudong.IsChecked == true)
            {
                taoKhoaTuDong();
            }
            taoKhoa();
            soP.Text = P.ToString();
            soQ.Text = Q.ToString();
            soD.IsEnabled = soN2.IsEnabled = true;
        }

        private void btn_MaHoa_Click(object sender, RoutedEventArgs e)
        {
            if (banRo.Text == "")
            {
                MessageBox.Show("Bạn chưa nhập bản rõ để mã hóa!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            else
            {
                if (soE.Text == "" || soN.Text == "" || soD.Text == "" || soN2.Text == "")
                {
                    MessageBox.Show("Bạn chưa tạo khóa!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    // thực hiện mã hóa
                    try
                    {
                        maHoa(banRo.Text);
                        tmpD = D;
                        tmpN = N;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                
            }
        }

        private void btn_GiaiMa_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                giaiMa(banMaGuiDen.Text);
                if(banRo.Text != banGiaiMa.Text && banRo.Text != "")
                {
                    if(tmpD != BigInteger.Parse(soD.Text) || tmpN != BigInteger.Parse(soN2.Text))
                    {
                        MessageBox.Show("Khóa bị thay đổi", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    if(banMaHoa.Text != banMaGuiDen.Text)
                    {
                        MessageBox.Show("Bản mã bị thay đổi", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Giải mã thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.None);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btn_MaHoaMoi_Click(object sender, RoutedEventArgs e)
        {
            banRo.Text = banMaHoa.Text = banMaGuiDen.Text = banGiaiMa.Text = string.Empty;
        }

        private void btn_reset_Click(object sender, RoutedEventArgs e)
        {
            soP.Text = soQ.Text = soPhiN.Text = soN.Text = soE.Text = soD.Text = soN2.Text = string.Empty;
            banRo.Text = banMaHoa.Text = banMaGuiDen.Text = banGiaiMa.Text = string.Empty;
        }

        private bool nguyenToCungNhau(BigInteger a, BigInteger b)
        {
            // Giải thuật Euclid
            BigInteger temp;
            while (b != 0)
            {
                temp = a % b;
                a = b;
                b = temp;
            }
            return a == 1;
        }

        public void taoKhoaTuDong()
        {
            Random rd = new Random();
            do
            {
                P = RandomBigInt(rd, 32);
                Q = RandomBigInt(rd, 32);
            }
            while (P == Q || !kiemTraNguyenTo(P) || !kiemTraNguyenTo(Q));
            soP.Text = P.ToString();
            soQ.Text = Q.ToString();
            taoKhoa();
        }

        private BigInteger RandomBigInt(Random rd, int bitLength)
        {
            byte[] bytes = new byte[bitLength / 8 + 1];
            BigInteger value;
            do
            {
                rd.NextBytes(bytes);
                bytes[bytes.Length - 1] &= 0x7F; // Ensure positive BigInteger
                value = new BigInteger(bytes);
            } while (value < BigInteger.Pow(2, bitLength - 1)); // Ensure the bit length is correct
            return value;
        }

        public BigInteger Mod(BigInteger a, BigInteger b, BigInteger n)
        {
            // a^b mod n
            if (b == 0)
                return BigInteger.One;
            if (b == 1)
                return a % n;

            BigInteger t = Mod(a, b / 2, n);
            t = (t * t) % n;
            if (b % 2 == 0)
                return t;
            else
                return (a % n * t) % n;
        }

        private void btn_NhapMa_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string fileName;
            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                try
                {
                    string str = "";
                    if (Path.GetExtension(fileName).ToLower() == ".txt")
                    {
                        // Đọc file text
                        str = File.ReadAllText(fileName);
                    }
                    else if (Path.GetExtension(fileName).ToLower() == ".docx")
                    {
                        // Đọc file Word (.docx)
                        using (DocX doc = DocX.Load(fileName))
                        {
                            foreach (var paragraph in doc.Paragraphs)
                            {
                                str += paragraph.Text + "\r\n";
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Định dạng tệp không được hỗ trợ.");
                        return;
                    }

                    banMaGuiDen.Text = str;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Lỗi đọc file: " + ex.Message);
                }
                btn_GiaiMa.IsEnabled = true;
            }
        }

        private void btn_NhapRo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string fileName;
            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                try
                {
                    string str = "";
                    // Kiểm tra định dạng của file
                    string extension = Path.GetExtension(fileName);
                    if (extension.ToLower() == ".txt")
                    {
                        // Đọc file văn bản (.txt)
                        str = File.ReadAllText(fileName);
                    }
                    else if (extension.ToLower() == ".docx")
                    {
                        // Đọc file Word (.docx)
                        using (DocX doc = DocX.Load(fileName))
                        {
                            foreach (var paragraph in doc.Paragraphs)
                            {
                                str += paragraph.Text + "\r\n";
                            }
                        }
                    }
                    else
                    {
                        throw new IOException("Định dạng file không được hỗ trợ.");
                    }

                    // Hiển thị nội dung đọc được lên giao diện
                    banRo.Text = str;
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Lỗi đọc file: " + ex.Message);
                    // Xử lý lỗi đọc file
                }
            }
        }

        private void btn_LuuKhoaPub_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Text files (*.txt)|*.txt|Word files (*.docx)|*.docx|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == true)
            {
                // Lấy đường dẫn và tên file mà người dùng đã chọn
                string filePath = saveFileDialog1.FileName;

                try
                {
                    if (Path.GetExtension(filePath).ToLower() == ".txt")
                    {
                        // Lưu file .txt
                        File.WriteAllText(filePath, $"{E}/n{N}");
                    }
                    else if (Path.GetExtension(filePath).ToLower() == ".docx")
                    {
                        // Lưu file .docx
                        using (DocX doc = DocX.Create(filePath))
                        {
                            doc.InsertParagraph($"{E}");
                            doc.InsertParagraph($"{N}");
                            doc.Save();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Định dạng tệp không được hỗ trợ.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu file: " + ex.Message);
                }

            }
        }

        private void btn_LuuKhoaS_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog2 = new SaveFileDialog();

            saveFileDialog2.Filter = "Text files (*.txt)|*.txt|Word files (*.docx)|*.docx|All files (*.*)|*.*";
            saveFileDialog2.FilterIndex = 1;
            saveFileDialog2.RestoreDirectory = true;

            if (saveFileDialog2.ShowDialog() == true)
            {
                string filePath = saveFileDialog2.FileName;

                try
                {
                    if (Path.GetExtension(filePath).ToLower() == ".txt")
                    {
                        // Lưu file .txt
                        File.WriteAllText(filePath, $"{D}/n{N}");
                    }
                    else if (Path.GetExtension(filePath).ToLower() == ".docx")
                    {
                        // Lưu file .docx
                        using (DocX doc = DocX.Create(filePath))
                        {
                            doc.InsertParagraph($"{D}");
                            doc.InsertParagraph($"{N}");
                            doc.Save();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Định dạng tệp không được hỗ trợ.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu file: " + ex.Message);
                }
            }
        }

        private void btn_NhapKhoaPub_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string fileName;
            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                try
                {
                    string str = "";
                    // Kiểm tra định dạng của file
                    string extension = Path.GetExtension(fileName);
                    if (extension.ToLower() == ".txt")
                    {
                        // Đọc file text (.txt)
                        str = File.ReadAllText(fileName);
                    }
                    else if (extension.ToLower() == ".docx")
                    {
                        // Đọc file word (.docx)
                        using (DocX doc = DocX.Load(fileName))
                        {
                            foreach (var paragraph in doc.Paragraphs)
                            {
                                str += paragraph.Text + " ";
                            }
                        }
                    }
                    else
                    {
                        throw new IOException("Định dạng file không được hỗ trợ.");
                    }

                    // Chia chuỗi thành mảng các từ
                    string[] str1 = str.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    BigInteger eValue, nValue;
                    if (BigInteger.TryParse(str1[0], out eValue) && BigInteger.TryParse(str1[1], out nValue))
                    {
                        soE.Text = eValue.ToString();
                        soN.Text = nValue.ToString();
                        E = eValue;
                        N = nValue;
                    }
                    else
                    {
                        MessageBox.Show("Không thể chuyển đổi dữ liệu vào kiểu BigInteger.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Lỗi đọc file: " + ex.Message);
                }
            }
        }

        private void btn_NhapKhoaS_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            string fileName;
            if (dlg.ShowDialog() == true)
            {
                fileName = dlg.FileName;
                try
                {
                    string str = "";
                    // Kiểm tra định dạng của file
                    string extension = Path.GetExtension(fileName);
                    if (extension.ToLower() == ".txt")
                    {
                        // Đọc file text (.txt)
                        str = File.ReadAllText(fileName);
                    }
                    else if (extension.ToLower() == ".docx")
                    {
                        // Đọc file word (.docx)
                        using (DocX doc = DocX.Load(fileName))
                        {
                            foreach (var paragraph in doc.Paragraphs)
                            {
                                str += paragraph.Text + " ";
                            }
                        }
                    }
                    else
                    {
                        throw new IOException("Định dạng file không được hỗ trợ.");
                    }

                    // Chia chuỗi thành mảng các từ
                    string[] str1 = str.Split(new char[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    BigInteger dValue, nValue;
                    if (BigInteger.TryParse(str1[0], out dValue) && BigInteger.TryParse(str1[1], out nValue))
                    {
                        soD.Text = dValue.ToString();
                        soN2.Text = nValue.ToString();
                        D = dValue;
                        N = nValue;
                    }
                    else
                    {
                        MessageBox.Show("Không thể chuyển đổi dữ liệu vào kiểu BigInteger.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine("Lỗi đọc file: " + ex.Message);
                }
            }
        }

        private void btn_LuuMa_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "Text files (*.txt)|*.txt|Word files (*.docx)|*.docx|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;

                try
                {
                    if (Path.GetExtension(filePath).ToLower() == ".txt")
                    {
                        // Lưu file .txt
                        File.WriteAllText(filePath, banMaHoa.Text);
                    }
                    else if (Path.GetExtension(filePath).ToLower() == ".docx")
                    {
                        // Lưu file .docx
                        using (DocX doc = DocX.Create(filePath))
                        {
                            doc.InsertParagraph(banMaHoa.Text);
                            doc.Save();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Định dạng tệp không được hỗ trợ.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi lưu file: " + ex.Message);
                }
            }
        }

        public BigInteger timSoNghichDao(BigInteger a, BigInteger n)
        {
            // tim nghich dao
            // a^-1 mod n
            BigInteger r, q, y = 0, y0 = 0, y1 = 1, tmp = n;
            while(a > 0)
            {
                r = n % a;
                q = n / a;
                if (r == 0)
                {
                    break;
                }
                y = y0 - q * y1;
                y0 = y1;
                y1 = y;
                n = a;
                a = r;
            }
            if (a > 1)
            {
                return -1;
            }
            if (y >= 0)
            {
                return y;
            }
            else
            {
                return y + tmp;
            }
        }

        public bool kiemTraNguyenTo(BigInteger a)
        {
            if (a < 2)
            {
                return false;
            }
            else
            {
                if (a == 2 || a == 3)
                {
                    return true;
                }
                else
                {
                    BigInteger sqrtA = Sqrt(a);
                    for (BigInteger i = 2; i <= sqrtA; i++)
                    {
                        if (a % i == 0)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        // Hàm tính căn bậc hai của BigInteger
        public static BigInteger Sqrt(BigInteger n)
        {
            if (n == 0) return 0;
            if (n <= 1) return n;

            BigInteger x = n;
            BigInteger y = (x + 1) / 2;
            while (y < x)
            {
                x = y;
                y = (x + n / x) / 2;
            }
            return x;
        }

        private void taoKhoa()
        {
            try
            {
                P = BigInteger.Parse(soP.Text);
                Q = BigInteger.Parse(soQ.Text);
                if (kiemTraNguyenTo(P) && kiemTraNguyenTo(Q))
                {
                    // Tính n = p * q
                    N = BigInteger.Multiply(P, Q);
                    // Tính Phi(n) = (p-1) * (q-1)
                    phiN = BigInteger.Multiply(BigInteger.Subtract(P, 1), BigInteger.Subtract(Q, 1));
                    soPhiN.Text = phiN.ToString();
                    // Tính e là một số ngẫu nhiên có giá trị 1 < e < phi(n) và gcd(e, Phi(n)) = 1
                    Random rd = new Random();
                    do
                    {
                        E = BigIntegerExtensions.RandomInRange(2, phiN - 1, rd);
                    }
                    while (!nguyenToCungNhau(E, phiN));
                    soE.Text = E.ToString();
                    // Tính d là nghịch đảo modular của e
                    D = timSoNghichDao(E, phiN) % phiN;
                    if (D < 0)
                    {
                        D += phiN;
                    }
                    soD.Text = D.ToString();
                    soN.Text = soN2.Text = N.ToString();
                }
                else
                {
                    if (!kiemTraNguyenTo(P))
                    {
                        MessageBox.Show("Số P bạn nhập không phải số nguyên tố!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    if (!kiemTraNguyenTo(Q))
                    {
                        MessageBox.Show("Số Q bạn nhập không phải số nguyên tố!", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Chưa nhập dữ liệu", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
        }

        public void maHoa(string str_i)
        {
            // chuyển chuỗi thành mảng kí tự
            char[] tmp1 = str_i.ToCharArray();
            BigInteger[] tmp2 = new BigInteger[tmp1.Length];
            for (int i = 0; i < tmp1.Length; i++)
            {
                tmp2[i] = new BigInteger(tmp1[i]);
            }
            // lưu kết quả mã hóa
            BigInteger[] tmp3 = new BigInteger[tmp2.Length];
            // mã hóa C = M^e mod n
            for (int i = 0; i < tmp2.Length; i++)
            {
                tmp3[i] = BigInteger.ModPow(tmp2[i], E, N);
            }
            // Chuyển mảng BigInteger thành chuỗi kết quả
            StringBuilder sb = new StringBuilder();
            foreach (var num in tmp3)
            {
                sb.Append(num);
                sb.Append(" ");
            }
            string str = sb.ToString().Trim();
            banMaHoa.Text = banMaGuiDen.Text = str;
        }

        public void giaiMa(string str_i)
        {
            BigInteger.TryParse(soD.Text, out D);
            BigInteger.TryParse(soN2.Text, out N);
            // Chuyển chuỗi số thành mảng
            string[] giaima = str_i.Split(' ');
            BigInteger[] temp1 = new BigInteger[giaima.Length];
            for (int i = 0; i < giaima.Length; i++)
            {
                temp1[i] = BigInteger.Parse(giaima[i]);
            }
            // lưu kết quả giải mã
            char[] temp2 = new char[temp1.Length];
            // giải mã M = C^d mod n
            for (int i = 0; i < temp1.Length; i++)
            {
                temp2[i] = (char)(BigInteger.ModPow(temp1[i], D, N).ToByteArray()[0]);
            }
            string str = new string(temp2);

            // Hiển thị kết quả giải mã
            banGiaiMa.Text = str;
        }


    }
}
