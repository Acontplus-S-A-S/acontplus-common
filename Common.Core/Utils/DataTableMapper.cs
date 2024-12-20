using System.Reflection;

namespace Common.Core.Utils;

public static class DataTableMapper
{
    public static T MapDataRowToModel<T>(DataRow row) where T : new()
    {
        if (row == null)
        {
            throw new ArgumentException("DataRow is null");
        }

        var columns = row.Table.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName.ToLower()).ToList();
        var ob = new T();
        var type = typeof(T);

        foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
        {
            if (columns.Contains(member.Name.ToLower()))
            {
                var value = row[member.Name];
                switch (member.MemberType)
                {
                    case MemberTypes.Field:
                        var field = (FieldInfo)member;
                        field.SetValue(ob, value);
                        break;
                    case MemberTypes.Property:
                        var property = (PropertyInfo)member;
                        if (property.CanWrite)
                        {
                            property.SetValue(ob, value);
                        }

                        break;
                }
            }
        }

        return ob;
    }


    public static List<T> MapDataTableToList<T>(DataTable dt) where T : new()
    {
        if (dt == null || dt.Rows.Count == 0)
        {
            throw new ArgumentException("DataTable is null or empty");
        }

        var columns = dt.Columns.Cast<DataColumn>().Select(dc => dc.ColumnName.ToLower()).ToList();
        var lst = new List<T>();
        foreach (DataRow dr in dt.Rows)
        {
            var ob = new T();
            var type = typeof(T);
            foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                if (columns.Contains(member.Name.ToLower()))
                {
                    var value = dr[member.Name];
                    switch (member.MemberType)
                    {
                        case MemberTypes.Field:
                            var field = (FieldInfo)member;
                            field.SetValue(ob, value);
                            break;
                        case MemberTypes.Property:
                            var property = (PropertyInfo)member;
                            if (property.CanWrite)
                            {
                                property.SetValue(ob, value);
                            }

                            break;
                    }
                }
            }

            lst.Add(ob);
        }

        return lst;
    }
}
