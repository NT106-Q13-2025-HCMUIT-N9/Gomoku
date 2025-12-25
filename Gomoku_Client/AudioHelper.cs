using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Resources;

namespace Gomoku_Client.Helpers
{
    public static class AudioHelper
    {
        /// <summary>
        /// Trích xuất file âm thanh từ Resource (trong file exe) ra thư mục Temp của Windows
        /// để MediaPlayer có thể đọc được.
        /// </summary>
        /// <param name="resourcePath">Đường dẫn tương đối (VD: "Assets/Sounds/click.wav")</param>
        /// <returns>Đường dẫn vật lý đến file tạm, hoặc null nếu lỗi.</returns>
        public static string ExtractResourceToTemp(string resourcePath)
        {
            try
            {
                string fileName = Path.GetFileName(resourcePath);

                string tempFolder = Path.Combine(Path.GetTempPath(), "Gomoku_TempAssets");
                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                string destinationPath = Path.Combine(tempFolder, fileName);

                if (File.Exists(destinationPath))
                {
                    return destinationPath;
                }

                Uri uri = new Uri($"pack://application:,,,/{resourcePath}", UriKind.Absolute);
                StreamResourceInfo info = Application.GetResourceStream(uri);

                if (info == null)
                {
                    Debug.WriteLine($"[AudioHelper] Không tìm thấy resource: {resourcePath}");
                    return null;
                }

                using (var fileStream = File.Create(destinationPath))
                {
                    info.Stream.CopyTo(fileStream);
                }

                return destinationPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioHelper] Lỗi giải nén âm thanh: {ex.Message}");
                return null;
            }
        }
    }
}