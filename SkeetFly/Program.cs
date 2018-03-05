using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkeetFly
{
    using System;
    using System.Reflection;
    using System.Text;

    public static class DictUtils
    {
        public static void AddEntryToList<T1, T2>(Dictionary<T1, List<T2>> d, T1 key, T2 listEntry)
        {
            if (!d.TryGetValue(key, out var list))
            {
                list = new List<T2>();
                d.Add(key, list);
            }

            list.Add(listEntry);
        }
    }

    public class X
    {
        public int I1 { get; set; } = 20;
    }
    public class Test
    {
        private static Dictionary<Type, List<Func<Type, object>>> dict = new Dictionary<Type, List<Func<Type, object>>>();
        static void Main()
        {
            var properties = typeof(X).GetProperties();
            foreach (var property in properties)
            {
                MethodInfo mi = property.GetGetMethod();

                Func<X, int> func1 = (Func<X, int>)Delegate.CreateDelegate(typeof(Func<X, int>), mi);

                MethodInfo genericHelper = typeof(Test).GetMethod("MagicMethod", BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo constructedHelper = genericHelper.MakeGenericMethod(typeof(X), mi.ReturnType);

                var func = (Func<X, object>)constructedHelper.Invoke(null, new object[] { mi });
                DictUtils.AddEntryToList(dict, typeof(X), func);
                var result = func(new X());
                Console.WriteLine();
            }

            //MethodInfo indexOf = typeof(string).GetMethod("IndexOf", new Type[] { typeof(char) });
            //MethodInfo getByteCount = typeof(Encoding).GetMethod("GetByteCount", new Type[] { typeof(string) });

            //Func<string, object> indexOfFunc = MagicMethod<string>(indexOf);
            //Func<Encoding, object> getByteCountFunc = MagicMethod<Encoding>(getByteCount);

            //Console.WriteLine(indexOfFunc("Hello", 'e'));
            //Console.WriteLine(getByteCountFunc(Encoding.UTF8, "Euro sign: u20ac"));
        }

        static Func<TargetType, object> MagicMethod<TargetType, PropertyType>(MethodInfo method) where TargetType : class
        {
            // First fetch the generic form
            MethodInfo genericHelper = typeof(Test).GetMethod("MagicMethodHelper",
            BindingFlags.Static | BindingFlags.NonPublic);

            // Now supply the type arguments
            MethodInfo constructedHelper = genericHelper.MakeGenericMethod(typeof(TargetType), typeof(PropertyType));

            // Now call it. The null argument is because it’s a static method.
            object ret = constructedHelper.Invoke(null, new object[] { method });

            // Cast the result to the right kind of delegate and return it
            return (Func<TargetType, object>)ret;
        }


        static Func<object, object> MagicMethodHelper<TTarget, TReturn>(MethodInfo method)
            where TTarget : class
        {
            // Convert the slow MethodInfo into a fast, strongly typed, open delegate
            Func<TTarget, TReturn> func = (Func<TTarget, TReturn>)Delegate.CreateDelegate(typeof(Func<TTarget, TReturn>), method);

            // Now create a more weakly typed delegate which will call the strongly typed one
            Func<object, object> ret = (object target) => func((TTarget)target);
            return ret;
        }
    }
}
