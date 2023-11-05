using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUFramework
{
    public class bizObject
    {
        string _tablename = "";
        string _getstsproc = "";
        string _updatestsproc = "";
        string _deletestsproc = "";
        string _primarykeyname = "";
        string _primarykeyparamname = "";
        DataTable _datatable = new();

        public bizObject(string tablename)
        {
            _tablename = tablename;
            _getstsproc = tablename + "Get";
            _updatestsproc = tablename + "Update";
            _deletestsproc = tablename + "Delete";
            _primarykeyname = tablename + "Id";
            _primarykeyparamname = "@" + _primarykeyname;
        }

        public DataTable Load(int primarykeyvalue)
        {
            DataTable dt = new();

            SqlCommand cmd = SQLUtility.GetSqlCommand(_getstsproc);
            SQLUtility.SetParamValue(cmd, _primarykeyparamname, primarykeyvalue);
            dt = SQLUtility.GetDataTable(cmd);
            _datatable = dt;
            return dt;
        }

        public void Delete(DataTable datatable)
        {
            int id = (int)datatable.Rows[0][_primarykeyname];
            SqlCommand cmd = SQLUtility.GetSqlCommand(_deletestsproc);
            SQLUtility.SetParamValue(cmd, _primarykeyparamname, id);
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

    }
}
