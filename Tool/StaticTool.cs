using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using CYQ.Data.Table;
using CYQ.Data.SQL;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace CYQ.Data.Tool
{
    /// <summary>
    /// ����ת����֧��jsonתʵ�壩
    /// </summary>
    public static class ConvertTool
    {
        /// <summary>
        /// ����ת��(��׼ǿ��)
        /// </summary>
        /// <param name="value">ֵ����</param>
        /// <param name="t">����</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type t)
        {
            return StaticTool.ChangeType(value, t);
        }
        /// <summary>
        /// ����ת��(��׼ǿ��)
        /// </summary>
        public static T ChangeType<T>(object value)
        {
            return (T)ChangeType(value, typeof(T));
        }
    }

    /// <summary>
    /// ��̬����������
    /// </summary>
    internal static class StaticTool
    {
        /// <summary>
        /// ��PropertyInfo[] �ĳ�PropertyInfo List������Ϊ.NET��CLR�������ڴ��д�쳣������IntelliTraceʱ��
        /// </summary>
        static MDictionary<string, List<PropertyInfo>> propCache = new MDictionary<string, List<PropertyInfo>>();
        static MDictionary<string, List<FieldInfo>> fieldCache = new MDictionary<string, List<FieldInfo>>();
        static MDictionary<string, object[]> attrCache = new MDictionary<string, object[]>();
        /// <summary>
        /// ��ȡ�����б�
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetPropertyInfo(Type t)
        {
            bool isAnonymousType = t.Name.Contains("f__AnonymousType");//������������
            string key = t.GUID.ToString();
            if (!isAnonymousType && propCache.ContainsKey(key))
            {
                return propCache[key];
            }
            else
            {
                bool isInheritOrm = t.BaseType.Name == "OrmBase" || t.BaseType.Name == "SimpleOrmBase";
                PropertyInfo[] pInfo = isInheritOrm ? t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : t.GetProperties();
                List<PropertyInfo> list = new List<PropertyInfo>(pInfo.Length);
                try
                {

                    list.AddRange(pInfo);
                    if (!isAnonymousType)
                    {
                        propCache.Set(key, list);
                    }
                }
                catch (Exception err)
                {
                    Log.Write(err, LogType.Error);
                }
                return list;
            }
        }
        /// <summary>
        /// ��ȡField�б�
        /// </summary>
        public static List<FieldInfo> GetFieldInfo(Type t)
        {
            string key = t.GUID.ToString();
            if (fieldCache.ContainsKey(key))
            {
                return fieldCache[key];
            }
            else
            {
                bool isInheritOrm = t.BaseType.Name == "OrmBase" || t.BaseType.Name == "SimpleOrmBase";
                FieldInfo[] pInfo = isInheritOrm ? t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) : t.GetFields();
                List<FieldInfo> list = new List<FieldInfo>(pInfo.Length);
                try
                {

                    list.AddRange(pInfo);
                    fieldCache.Set(key, list);
                }
                catch (Exception err)
                {
                    Log.Write(err, LogType.Error);
                }
                return list;
            }
        }
        static Dictionary<string, Type[]> argumentCache = new Dictionary<string, Type[]>();
        /// <summary>
        ///  ��ȡ���͵Ĳ������ȣ��Ƿ��Ͱ�Ĭ�Ϸ������㣩
        /// </summary>
        public static int GetArgumentLength(ref Type t)
        {
            Type[] argTypes;
            return GetArgumentLength(ref t, out argTypes);
        }
        /// <summary>
        /// ��ȡ���͵Ĳ������ȣ�ͬʱ�����޸�Ϊ��ͨ���ͣ��Ƿ��Ͱ�Ĭ�Ϸ������㣩
        /// </summary>
        public static int GetArgumentLength(ref Type t, out Type[] argTypes)
        {
            if (argumentCache.ContainsKey(t.FullName))
            {
                argTypes = argumentCache[t.FullName];
                return argTypes.Length;
            }
            else
            {
                int len = 0;
                if (t.IsGenericType)
                {
                    argTypes = t.GetGenericArguments();
                    len = argTypes.Length;
                    for (int i = 0; i < argTypes.Length; i++)
                    {
                        if (argTypes[i].IsGenericType && argTypes[i].Name.StartsWith("Nullable"))
                        {
                            argTypes[i] = Nullable.GetUnderlyingType(argTypes[i]);
                        }
                    }
                    if (t.Name.StartsWith("Nullable"))
                    {
                        t = Nullable.GetUnderlyingType(t);
                    }
                }
                else
                {
                    if (t.Name.EndsWith("[]") || t.Name == "MDataRowCollection")
                    {
                        len = 1;
                    }
                    else if (t.Name == "NameValueCollection" || (t.BaseType != null && t.BaseType.Name == "NameValueCollection"))
                    {
                        len = 2;
                    }
                    else
                    {
                        System.Reflection.MethodInfo mi = t.GetMethod("Add");
                        if (mi != null)
                        {
                            len = mi.GetParameters().Length;
                        }
                    }
                    argTypes = new Type[len];
                    for (int i = 0; i < argTypes.Length; i++)
                    {
                        argTypes[i] = typeof(object);
                    }
                }
                try
                {
                    argumentCache.Add(t.FullName, argTypes);
                }
                catch
                {

                }
                return len;
            }
        }

        /// <summary>
        /// ��ȡ�����б�
        /// </summary>
        public static object[] GetAttributes(Type t, Type searchType)
        {
            string key = t.GUID.ToString() + (searchType == null ? "" : searchType.Name);
            if (attrCache.ContainsKey(key))
            {
                return attrCache[key];
            }
            else
            {
                try
                {
                    object[] items = searchType == null ? t.GetCustomAttributes(false) : t.GetCustomAttributes(searchType, true);
                    attrCache.Add(key, items);
                    return items;
                }
                catch (Exception err)
                {
                    Log.Write(err, LogType.Error);
                }
                return null;
            }
        }

        /// <summary>
        /// ��ȡϵͳ���ͣ�����Nullable���ͣ���תΪ�������͡�
        ///  </summary>
        public static SysType GetSystemType(ref Type t)
        {
            if (t.IsEnum)
            {
                return SysType.Enum;
            }
            if (t.FullName.EndsWith("[]"))
            {
                return SysType.Array;
            }
            if (t.FullName.StartsWith("System.")) // ϵͳ����
            {
                if (t.IsGenericType)
                {
                    if (t.Name.StartsWith("Nullable"))//int? id
                    {
                        t = Nullable.GetUnderlyingType(t);
                        return SysType.Base;
                    }
                    return SysType.Generic;
                }
                else if (t.FullName.StartsWith("System.Collections."))
                {
                    return SysType.Collection;
                }
                else if (t.Name.EndsWith("[]"))
                {
                    return SysType.Array;
                }
                if (t.FullName.Split('.').Length > 2)
                {
                    return SysType.Custom;
                }
                return SysType.Base;
            }
            else
            {
                return SysType.Custom;
            }
        }


        /// <summary>
        /// ��GUIDת��16�ֽ��ַ���
        /// </summary>
        /// <returns></returns>
        internal static string ToGuidByteString(string guid)
        {
            return BitConverter.ToString(new Guid(guid).ToByteArray()).Replace("-", "");
        }



        /// <summary>
        /// ����ת��(��׼ǿ��)
        /// </summary>
        /// <param name="value">ֵ����</param>
        /// <param name="t">����</param>
        /// <returns></returns>
        public static object ChangeType(object value, Type t)
        {
            if (t == null)
            {
                return null;
            }
            if (t.FullName == "System.Type")
            {
                return (Type)value;
            }
            string strValue = Convert.ToString(value);
            if (t.IsGenericType && t.Name.StartsWith("Nullable"))
            {
                t = Nullable.GetUnderlyingType(t);
                if (strValue == "")
                {
                    return null;
                }
            }
            if (t.Name == "String")
            {
                return strValue;
            }
            if (t.FullName == "System.Text.StringBuilder")
            {
                return value as StringBuilder;
            }
            if (t.FullName == "System.Text.Encoding")
            {
                return value as Encoding;
            }
            if (strValue == "")
            {
                return Activator.CreateInstance(t);
            }
            else if (t.IsValueType)
            {
                if (t.Name == "DateTime")
                {
                    return Convert.ChangeType(value, t);//������value�����ⶪʧ����
                }
                if (t.Name == "Guid")
                {
                    return new Guid(strValue);
                }
                else if (t.Name.StartsWith("Int"))
                {
                    if (strValue.IndexOf('.') > -1)
                    {
                        strValue = strValue.Split('.')[0];
                    }
                    else if (value.GetType().IsEnum)
                    {
                        return (int)value;
                    }
                }
                else if (t.Name == "Boolean")
                {
                    switch (strValue.ToLower())
                    {
                        case "yes":
                        case "true":
                        case "1":
                        case "on":
                        case "��":
                            return true;
                        case "no":
                        case "false":
                        case "0":
                        case "":
                        case "��":
                        default:
                            return false;
                    }
                }
                else if (t.IsEnum)
                {

                    return Enum.Parse(t, strValue, true);
                }
                return Convert.ChangeType(strValue, t);
            }
            else
            {
                //Type valueType = value.GetType();
                //if(valueType.IsEnum && t.is)
                if (value.GetType().FullName != t.FullName)
                {
                    switch (GetSystemType(ref t))
                    {
                        case SysType.Custom:

                            return MDataRow.CreateFrom(strValue).ToEntity(t);
                        case SysType.Generic:
                            if (t.Name.StartsWith("List"))
                            {
                                return MDataTable.CreateFrom(strValue).ToList(t);
                            }
                            break;
                        case SysType.Array:
                            if (t.Name == "Byte[]" && value.GetType().Name != t.Name)
                            {
                                using (MemoryStream ms = new MemoryStream())
                                {
                                    new BinaryFormatter().Serialize(ms, value);
                                    return ms.ToArray();
                                }
                            }
                            break;
                    }
                }
                return Convert.ChangeType(value, t);
            }
        }

        #region ���ַ�����HashKey
        static MDictionary<string, string> hashKeyCache = new MDictionary<string, string>(32);
        internal static string GetHashKey(string sourceString)
        {
            try
            {
                if (hashKeyCache.ContainsKey(sourceString))
                {
                    return hashKeyCache[sourceString];
                }
                else
                {
                    if (hashKeyCache.Count > 512)
                    {
                        hashKeyCache.Clear();
                        hashKeyCache = new MDictionary<string, string>(64);
                    }
                    string value = "K" + Math.Abs(sourceString.GetHashCode()) + sourceString.Length;
                    hashKeyCache.Add(sourceString, value);
                    return value;
                }
            }
            catch
            {
                return sourceString;
            }
        }
        #endregion

    }
}
