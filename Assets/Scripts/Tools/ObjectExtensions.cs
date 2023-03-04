using System.Linq;
using System.Reflection;

namespace Project.Scripts.Tools
{
    /// <summary>
    /// object型の拡張メソッドを管理するクラス
    /// </summary>
    public static class ObjectExtensions
    {
        private const string Separator = ",";       // 区切り記号として使用する文字列
        private const string Format = "{0}:{1}";    // 複合書式指定文字列
 
        /// <summary>
        /// すべての公開フィールドの情報を文字列にして返します
        /// </summary>
        private static string ToStringFields<T>(this T obj)
        {
            return string.Join(Separator, obj
                .GetType()
                .GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Select(c => string.Format(Format, c.Name, c.GetValue(obj))));
        }
    
        /// <summary>
        /// すべての公開プロパティの情報を文字列にして返します
        /// </summary>
        private static string ToStringProperties<T>(this T obj)
        {
            return string.Join(Separator, obj
                .GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(c => c.CanRead)
                .Select(c => string.Format(Format, c.Name, c.GetValue(obj, null))));
        }
    
        /// <summary>
        /// すべての公開フィールドと公開プロパティの情報を文字列にして返します
        /// </summary>
        public static string ToStringReflection<T>(this T obj)
        {
            return string.Join(Separator, 
                obj.ToStringFields(), 
                obj.ToStringProperties());
        }
    }
}