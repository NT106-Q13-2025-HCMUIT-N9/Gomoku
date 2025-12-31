using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Gomoku_Client.Helpers
{
    public static class AudioHelper
    {
        /// <summary>
        /// Trích xuất file từ Embedded Resource ra thư mục Temp.
        /// </summary>
        /// <param name="relativeResourcePath">Đường dẫn tương đối dùng dấu gạch chéo. 
        /// VD: "Assets/Sounds/bgm.mp3"</param>
        /// <returns>Đường dẫn vật lý đến file tạm</returns>
        public static string ExtractResourceToTemp(string relativeResourcePath)
        {
            try
            {
                // 1. Lấy Assembly hiện tại
                Assembly assembly = Assembly.GetExecutingAssembly();

                // 2. Chuyển đổi đường dẫn file thành định dạng của Embedded Resource (dùng dấu chấm)
                // Giả sử Namespace gốc của bạn là "Gomoku_Client"
                string rootNamespace = "Gomoku_Client";
                string resourceName = $"{rootNamespace}.{relativeResourcePath.Replace('/', '.').Replace('\\', '.')}";

                // 3. Thiết lập thư mục tạm
                string fileName = Path.GetFileName(relativeResourcePath);
                string tempFolder = Path.Combine(Path.GetTempPath(), "Gomoku_TempAssets");

                if (!Directory.Exists(tempFolder))
                {
                    Directory.CreateDirectory(tempFolder);
                }

                string destinationPath = Path.Combine(tempFolder, fileName);

                // 4. Nếu file đã tồn tại rồi thì không cần giải nén lại (tối ưu hiệu năng)
                if (File.Exists(destinationPath))
                {
                    return destinationPath;
                }

                // 5. Đọc Stream từ Embedded Resource
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.WriteLine($"[AudioHelper] LỖI: Không tìm thấy Resource '{resourceName}'.");
                        // Mẹo: Nếu lỗi, hãy chạy dòng code dưới đây để xem danh sách tên tất cả Resource đang có:
                        // foreach (var name in assembly.GetManifestResourceNames()) Debug.WriteLine(name);
                        return null;
                    }

                    using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fileStream);
                    }
                }

                Debug.WriteLine($"[AudioHelper] Đã giải nén thành công: {destinationPath}");
                return destinationPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AudioHelper] Lỗi hệ thống: {ex.Message}");
                return null;
            }
        }
    }
}