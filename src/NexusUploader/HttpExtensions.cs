using System.Net.Http;

namespace NexusUploader
{
    public static class HttpExtensions
    {
        public static StringContent ToContent(this int i) {
            return new StringContent(i.ToString());
        }

        public static StringContent ToContent(this string s) {
            return new StringContent(string.IsNullOrWhiteSpace(s) ? string.Empty : s);
        }

        public static StringContent ToContent(this long l) {
            return new StringContent(l.ToString());
        }
    }
}