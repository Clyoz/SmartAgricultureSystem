using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SmartAgricultureSystem.Helpers
{
    /// <summary>
    /// 验证码生成工具类
    /// 模拟生成图形验证码（4位随机字母数字）
    /// </summary>
    public static class CaptchaHelper
    {
        // 验证码字符集（去除易混淆字符 0/O/1/l/I）
        private static readonly string CAPTCHA_CHARS =
            "23456789ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz";

        // 随机数生成器
        private static readonly Random sRandom = new Random();

        /// <summary>
        /// 生成随机验证码字符串
        /// </summary>
        /// <param name="length">验证码长度，默认4位</param>
        /// <returns>验证码字符串</returns>
        public static string GenerateCaptchaCode(int length = 4)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(CAPTCHA_CHARS[sRandom.Next(CAPTCHA_CHARS.Length)]);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 生成验证码图片（BitmapImage，可直接绑定到WPF Image控件）
        /// </summary>
        /// <param name="captchaCode">验证码字符串</param>
        /// <returns>WPF可用的BitmapImage</returns>
        public static BitmapImage GenerateCaptchaImage(string captchaCode)
        {
            // 图片尺寸
            const int WIDTH = 120;
            const int HEIGHT = 40;

            using (var bitmap = new Bitmap(WIDTH, HEIGHT))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // 绘制背景
                graphics.Clear(System.Drawing.Color.WhiteSmoke);

                // 绘制干扰线（5条随机线）
                for (int i = 0; i < 5; i++)
                {
                    var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(
                        sRandom.Next(100, 200),
                        sRandom.Next(100, 200),
                        sRandom.Next(100, 200)));
                    graphics.DrawLine(pen,
                        sRandom.Next(WIDTH), sRandom.Next(HEIGHT),
                        sRandom.Next(WIDTH), sRandom.Next(HEIGHT));
                }

                // 绘制验证码文字
                var font = new Font("Arial", 20, System.Drawing.FontStyle.Bold);
                for (int i = 0; i < captchaCode.Length; i++)
                {
                    // 每个字符使用不同颜色和轻微旋转
                    var color = System.Drawing.Color.FromArgb(
                        sRandom.Next(0, 100),
                        sRandom.Next(0, 150),
                        sRandom.Next(100, 255));
                    var brush = new SolidBrush(color);
                    graphics.DrawString(
                        captchaCode[i].ToString(), font, brush,
                        5 + i * 28, sRandom.Next(2, 8));
                }

                // 绘制干扰点（50个）
                for (int i = 0; i < 50; i++)
                {
                    bitmap.SetPixel(
                        sRandom.Next(WIDTH), sRandom.Next(HEIGHT),
                        System.Drawing.Color.FromArgb(
                            sRandom.Next(0, 255),
                            sRandom.Next(0, 255),
                            sRandom.Next(0, 255)));
                }

                // 转换为WPF可用的BitmapImage
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    ms.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = ms;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze(); // 使其可跨线程访问
                    return bitmapImage;
                }
            }
        }
    }
}
