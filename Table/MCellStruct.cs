using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using CYQ.Data.SQL;

namespace CYQ.Data.Table
{
    /// <summary>
    /// �нṹ��ѡ��
    /// </summary>
    [Flags]
    public enum AlterOp
    {
        /// <summary>
        /// Ĭ�ϲ��޸�״̬
        /// </summary>
        None = 0,
        /// <summary>
        /// ���ӻ��޸�״̬
        /// </summary>
        AddOrModify = 1,
        /// <summary>
        /// ɾ����״̬
        /// </summary>
        Drop = 2,
        /// <summary>
        /// ��������״̬
        /// </summary>
        Rename = 4
    }
    /// <summary>
    /// ��Ԫ�ṹ����
    /// </summary>
    public partial class MCellStruct
    {
        private MDataColumn _MDataColumn = null;
        /// <summary>
        /// �ṹ����
        /// </summary>
        public MDataColumn MDataColumn
        {
            get
            {
                return _MDataColumn;
            }
            internal set
            {
                _MDataColumn = value;
            }
        }
        /// <summary>
        /// �Ƿ��ֵ���и�ʽУ��
        /// </summary>
        //public bool IsCheckValue = true;
        /// <summary>
        /// �Ƿ�ؼ���
        /// </summary>
        public bool IsPrimaryKey = false;

        /// <summary>
        /// �Ƿ�Ψһ����
        /// </summary>
        public bool IsUniqueKey = false;

        /// <summary>
        /// �Ƿ����
        /// </summary>
        public bool IsForeignKey = false;
        /// <summary>
        /// �������
        /// </summary>
        public string FKTableName;

        /// <summary>
        /// �ֶ�����
        /// </summary>
        public string Description;
        /// <summary>
        /// Ĭ��ֵ
        /// </summary>
        public object DefaultValue;
        /// <summary>
        /// �Ƿ�����ΪNull
        /// </summary>
        public bool IsCanNull;
        /// <summary>
        /// �Ƿ�������
        /// </summary>
        public bool IsAutoIncrement;
        /// <summary>
        /// �ɵ�������AlterOpΪRenameʱ���ã�
        /// </summary>
        public string OldName;
        private string _ColumnName = string.Empty;
        /// <summary>
        /// ����
        /// </summary>
        public string ColumnName
        {
            get
            {
                return _ColumnName;
            }
            set
            {
                _ColumnName = value;
                if (_MDataColumn != null)
                {
                    _MDataColumn.IsColumnNameChanged = true;//�����ѱ�����洢����Ҳ��Ҫ���
                }
            }
        }
        /// <summary>
        /// ����
        /// </summary>
        public string TableName;
        private SqlDbType _SqlType;
        /// <summary>
        /// SqlDbType����
        /// </summary>
        public SqlDbType SqlType
        {
            get
            {
                return _SqlType;
            }
            set
            {
                _SqlType = value;
                ValueType = DataType.GetType(_SqlType, DalType);
            }
        }
        /// <summary>
        /// ����ֽ�
        /// </summary>
        public int MaxSize;

        /// <summary>
        /// ���ȣ�С��λ��
        /// </summary>
        public short Scale;
        /// <summary>
        /// ԭʼ�����ݿ��ֶ���������
        /// </summary>
        internal string SqlTypeName;
        internal Type ValueType;
        private DalType dalType = DalType.None;
        internal DalType DalType
        {
            get
            {
                if (_MDataColumn != null)
                {
                    return _MDataColumn.dalType;
                }
                return dalType;
            }
        }
        private AlterOp _AlterOp = AlterOp.None;
        /// <summary>
        /// �нṹ�ı�״̬
        /// </summary>
        public AlterOp AlterOp
        {
            get { return _AlterOp; }
            set { _AlterOp = value; }
        }
        //�ڲ�ʹ�õ����������ֶ���Ϊ��ʱʹ��
        internal int ReaderIndex = -1;

        #region ���캯��
        internal MCellStruct(DalType dalType)
        {
            this.dalType = dalType;
        }
        public MCellStruct(string columnName, SqlDbType sqlType)
        {
            Init(columnName, sqlType, false, true, false, -1, null);
        }
        public MCellStruct(string columnName, SqlDbType sqlType, bool isAutoIncrement, bool isCanNull, int maxSize)
        {
            Init(columnName, sqlType, isAutoIncrement, isCanNull, false, maxSize, null);
        }
        internal void Init(string columnName, SqlDbType sqlType, bool isAutoIncrement, bool isCanNull, bool isPrimaryKey, int maxSize, object defaultValue)
        {
            ColumnName = columnName.Trim();
            SqlType = sqlType;
            IsAutoIncrement = isAutoIncrement;
            IsCanNull = isCanNull;
            MaxSize = maxSize;
            IsPrimaryKey = isPrimaryKey;
            DefaultValue = defaultValue;
        }
        internal void Load(MCellStruct ms)
        {
            ColumnName = ms.ColumnName;
            SqlType = ms.SqlType;
            IsAutoIncrement = ms.IsAutoIncrement;
            IsCanNull = ms.IsCanNull;
            MaxSize = ms.MaxSize;
            Scale = ms.Scale;
            IsPrimaryKey = ms.IsPrimaryKey;
            IsUniqueKey = ms.IsUniqueKey;
            IsForeignKey = ms.IsForeignKey;
            FKTableName = ms.FKTableName;
            SqlTypeName = ms.SqlTypeName;
            AlterOp = ms.AlterOp;

            if (ms.DefaultValue != null)
            {
                DefaultValue = ms.DefaultValue;
            }
            if (!string.IsNullOrEmpty(ms.Description))
            {
                Description = ms.Description;
            }
        }
        /// <summary>
        /// ��¡һ������
        /// </summary>
        /// <returns></returns>
        public MCellStruct Clone()
        {
            MCellStruct ms = new MCellStruct(dalType);
            ms.ColumnName = ColumnName;
            ms.SqlType = SqlType;
            ms.IsAutoIncrement = IsAutoIncrement;
            ms.IsCanNull = IsCanNull;
            ms.MaxSize = MaxSize;
            ms.Scale = Scale;
            ms.IsPrimaryKey = IsPrimaryKey;
            ms.IsUniqueKey = IsUniqueKey;
            ms.IsForeignKey = IsForeignKey;
            ms.FKTableName = FKTableName;
            ms.SqlTypeName = SqlTypeName;
            ms.DefaultValue = DefaultValue;
            ms.Description = Description;
            ms.MDataColumn = MDataColumn;
            ms.AlterOp = AlterOp;
            ms.TableName = TableName;
            return ms;
        }
        #endregion
    }

    public partial class MCellStruct // ��չ�������÷���
    {
        /// <summary>
        /// Ϊ�е�����������ֵ
        /// </summary>
        public MCellStruct Set(object value)
        {
            Set(value, -1);
            return this;
        }
        public MCellStruct Set(object value, int state)
        {
            if (_MDataColumn != null)
            {
                MDataTable dt = _MDataColumn._Table;
                if (dt != null && dt.Rows.Count > 0)
                {
                    int index = _MDataColumn.GetIndex(ColumnName);
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        dt.Rows[i].Set(index, value, state);
                    }
                }
            }
            return this;
        }
        /// <summary>
        /// ���ظ��е�where In ���� : Name in("aa","bb")
        /// </summary>
        /// <returns></returns>
        public string GetWhereIn()
        {
            if (_MDataColumn != null)
            {
                MDataTable dt = _MDataColumn._Table;
                if (dt != null && dt.Rows.Count > 0)
                {
                    List<string> items = dt.GetColumnItems<string>(ColumnName, BreakOp.NullOrEmpty, true);
                    if (items != null && items.Count > 0)
                    {
                        return SqlCreate.GetWhereIn(this, items, dalType);
                    }
                }
            }
            return string.Empty;
        }
    }
}