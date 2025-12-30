using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using UnityEngine;


namespace IGCore.PlatformService.Util
{
    public static class StringCompressor
    {
        // ----------------------------------------------------------------
        // 1. [압축] 원본 JSON 문자열 -> 압축된 Base64 문자열 (Cloud 저장용)
        // ----------------------------------------------------------------
        public static string CompressToEncodedString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return "";

            // A. 문자열 -> 바이트
            byte[] buffer = Encoding.UTF8.GetBytes(plainText);

            // B. GZip 압축
            using (var memoryStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                {
                    zipStream.Write(buffer, 0, buffer.Length);
                }
            
                // C. 압축된 바이트 -> Base64 문자열로 변환
                byte[] compressedBytes = memoryStream.ToArray();
                return Convert.ToBase64String(compressedBytes);
            }
        }

        // ----------------------------------------------------------------
        // 2. [해제] 압축된 Base64 문자열 -> 원본 JSON 문자열 (Cloud 로드용)
        // ----------------------------------------------------------------
        public static string DecompressFromEncodedString(string base64String)
        {
            if (string.IsNullOrEmpty(base64String)) return "";

            // A. Base64 문자열 -> 바이트 배열 (TryFromBase64String 사용)
            // Base64는 원본보다 약 33% 크므로 넉넉하게 버퍼 할당
            byte[] buffer = new byte[base64String.Length];
        
            if (Convert.TryFromBase64String(base64String, buffer, out int bytesWritten))
            {
                try
                {
                    // B. GZip 해제
                    // (bytesWritten만큼만 읽어서 해제)
                    using (var inputMs = new MemoryStream(buffer, 0, bytesWritten))
                    using (var outputMs = new MemoryStream())
                    using (var zipStream = new GZipStream(inputMs, CompressionMode.Decompress))
                    {
                        zipStream.CopyTo(outputMs);
                        // C. 바이트 -> 원본 문자열 복구
                        return Encoding.UTF8.GetString(outputMs.ToArray());
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[GZip] 압축 해제 실패: {ex.Message}");
                    return null;
                }
            }
            else
            {
                Debug.LogError("[Base64] 올바른 형식이 아닙니다.");
                return null;
            }
        }
    }
}