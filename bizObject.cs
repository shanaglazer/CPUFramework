using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CPUFramework
{
    public class bizObject : INotifyPropertyChanged
    {
        string _tablename = "";
        string _getstsproc = "";
        string _updatestsproc = "";
        string _deletestsproc = "";
        string _primarykeyname = "";
        string _primarykeyparamname = "";
        DataTable _datatable = new();
        List<PropertyInfo> _properties = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public bizObject()
        {
            Type t = this.GetType();
            _tablename = t.Name;
            if(_tablename.ToLower().StartsWith("biz"))
            {
                _tablename = _tablename.Substring(3);
            }
            _getstsproc = _tablename + "Get";
            _updatestsproc = _tablename + "Update";
            _deletestsproc = _tablename + "Delete";
            _primarykeyname = _tablename + "Id";
            _primarykeyparamname = "@" + _primarykeyname;
            _properties = t.GetProperties().ToList<PropertyInfo>();
        }

        public DataTable Load(int primarykeyvalue)
        {
            DataTable dt = new();

            SqlCommand cmd = SQLUtility.GetSqlCommand(_getstsproc);
            SQLUtility.SetParamValue(cmd, _primarykeyparamname, primarykeyvalue);
            dt = SQLUtility.GetDataTable(cmd);
            if(dt.Rows.Count > 0)
            {
                LoadProps(dt.Rows[0]);
            }
            _datatable = dt;
            return dt;
        }

        private void LoadProps(DataRow dr)
        {
            foreach(DataColumn col in dr.Table.Columns)
            {
                string colname = col.ColumnName.ToLower();
                PropertyInfo? prop = _properties.FirstOrDefault(p => p.Name.ToLower() == colname && p.CanWrite == true);
                if(prop != null)
                {
                    prop.SetValue(this, dr[colname]);
                }
            }
        }

        public void Delete(DataTable datatable)
        {
            int id = (int)datatable.Rows[0][_primarykeyname];
            SqlCommand cmd = SQLUtility.GetSqlCommand(_deletestsproc);
            SQLUtility.SetParamValue(cmd, _primarykeyparamname, id);
            SQLUtility.ExecuteSQL(cmd);
        }

        public void Save()
        {
            SqlCommand cmd = SQLUtility.GetSqlCommand(_updatestsproc);
            foreach(SqlParameter param in cmd.Parameters)
            {
                string colname = param.ParameterName.ToLower().Substring(1);
                PropertyInfo? prop = _properties.FirstOrDefault(p => p.Name.ToLower() == colname && p.CanRead == true);
                if(prop != null)
                {
                    param.Value = prop.GetValue(this);
                }
            }
            SQLUtility.ExecuteSQL(cmd);
        }

        public void Save(DataTable datatable)
        {
            if (datatable.Rows.Count == 0)
            {
                throw new Exception($"Cannot call {_tablename} Save method becuase there are no rows in the tabe.");
            }
            DataRow r = datatable.Rows[0];
            //var dateborn = ((DateTime)r["dateborn"]).ToString("yyyy-MM-dd h:mm");
            SQLUtility.SaveDataRow(r, _updatestsproc);
        }

        protected void InvokePropertyChanged([CallerMemberName] string propertyname = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
