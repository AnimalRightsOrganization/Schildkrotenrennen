using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Data;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Excel;

public class ExcelUtility
{
	/// <summary>
	/// 表格数据集合
	/// </summary>
	private DataSet mResultSet;

	private string xmlPath = Application.dataPath + "/xmlName.xml";

	private string[] fName;
	/// <summary>
	/// 构造函数
	/// </summary>
	/// <param name="excelFile">Excel file.</param>
	public ExcelUtility(string excelFile)
	{
		FileStream mStream = File.Open(excelFile, FileMode.Open, FileAccess.Read);
		IExcelDataReader mExcelReader = ExcelReaderFactory.CreateOpenXmlReader(mStream);
		mResultSet = mExcelReader.AsDataSet();
	}

	/// <summary>
	/// 转换为实体类列表
	/// </summary>
	public List<T> ConvertToList<T>()
	{
		//判断Excel文件中是否存在数据表
		if (mResultSet.Tables.Count < 1)
			return null;
		//默认读取第一个数据表
		DataTable mSheet = mResultSet.Tables[0];

		//判断数据表内是否存在数据
		if (mSheet.Rows.Count < 1)
			return null;

		//读取数据表行数和列数
		int rowCount = mSheet.Rows.Count;
		int colCount = mSheet.Columns.Count;

		//准备一个列表以保存全部数据
		List<T> list = new List<T>();

		//读取数据
		for (int i = 1; i < rowCount; i++)
		{
			//创建实例
			Type t = typeof(T);
			ConstructorInfo ct = t.GetConstructor(System.Type.EmptyTypes);
			T target = (T)ct.Invoke(null);
			for (int j = 0; j < colCount; j++)
			{
				//读取第1行数据作为表头字段
				string field = mSheet.Rows[0][j].ToString();
				object value = mSheet.Rows[i][j];
				//设置属性值
				SetTargetProperty(target, field, value);
			}

			//添加至列表
			list.Add(target);
		}

		return list;
	}

	/// <summary>
	/// 转换为Json
	/// </summary>
	/// <param name="JsonPath">Json文件路径</param>
	/// <param name="Header">表头行数</param>
	public void ConvertToJson(string JsonPath, Encoding encoding)
	{
		//判断Excel文件中是否存在数据表
		if (mResultSet.Tables.Count < 1)
			return;

		//默认读取第一个数据表
		DataTable mSheet = mResultSet.Tables[0];

		//判断数据表内是否存在数据
		if (mSheet.Rows.Count < 1)
			return;

		//读取数据表行数和列数
		int rowCount = mSheet.Rows.Count;
		int colCount = mSheet.Columns.Count;

		//准备一个列表存储整个表的数据
		List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

		//读取数据
		for (int i = 1; i < rowCount; i++)
		{
			//准备一个字典存储每一行的数据
			Dictionary<string, object> row = new Dictionary<string, object>();
			for (int j = 0; j < colCount; j++)
			{
				//读取第1行数据作为表头字段
				string field = mSheet.Rows[0][j].ToString();
				//Key-Value对应
				row[field] = mSheet.Rows[i][j];
			}

			//添加到表数据中
			table.Add(row);
		}

		//生成Json字符串
		//string json = JsonConvert.SerializeObject(table, Newtonsoft.Json.Formatting.Indented);
		string json = JsonConvert.SerializeObject(table, new DecimalJsonConverter());
		Debug.Log(json);
		//写入文件
		using (FileStream fileStream = new FileStream(JsonPath, FileMode.Create, FileAccess.Write))
		{
			using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
			{
				textWriter.Write(json);
			}
		}
	}

	/// <summary>
	/// 转换为lua
	/// </summary>
	/// <param name="luaPath">lua文件路径</param>
	public void ConvertToLua(string luaPath, Encoding encoding)
	{
		//判断Excel文件中是否存在数据表
		if (mResultSet.Tables.Count < 1)
			return;

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("local datas = {");
		stringBuilder.Append("\r\n");

		//读取数据表
		foreach (DataTable mSheet in mResultSet.Tables)
		{
			//判断数据表内是否存在数据
			if (mSheet.Rows.Count < 1)
				continue;

			//读取数据表行数和列数
			int rowCount = mSheet.Rows.Count;
			int colCount = mSheet.Columns.Count;

			//准备一个列表存储整个表的数据
			List<Dictionary<string, object>> table = new List<Dictionary<string, object>>();

			//读取数据
			for (int i = 1; i < rowCount; i++)
			{
				//准备一个字典存储每一行的数据
				Dictionary<string, object> row = new Dictionary<string, object>();
				for (int j = 0; j < colCount; j++)
				{
					//读取第1行数据作为表头字段
					string field = mSheet.Rows[0][j].ToString();
					//Key-Value对应
					row[field] = mSheet.Rows[i][j];
				}
				//添加到表数据中
				table.Add(row);
			}
			stringBuilder.Append(string.Format("\t\"{0}\" = ", mSheet.TableName));
			stringBuilder.Append("{\r\n");
			foreach (Dictionary<string, object> dic in table)
			{
				stringBuilder.Append("\t\t{\r\n");
				foreach (string key in dic.Keys)
				{
					if (dic[key].GetType().Name == "String")
						stringBuilder.Append(string.Format("\t\t\t\"{0}\" = \"{1}\",\r\n", key, dic[key]));
					else
						stringBuilder.Append(string.Format("\t\t\t\"{0}\" = {1},\r\n", key, dic[key]));
				}
				stringBuilder.Append("\t\t},\r\n");
			}
			stringBuilder.Append("\t}\r\n");
		}

		stringBuilder.Append("}\r\n");
		stringBuilder.Append("return datas");

		//写入文件
		using (FileStream fileStream = new FileStream(luaPath, FileMode.Create, FileAccess.Write))
		{
			using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
			{
				textWriter.Write(stringBuilder.ToString());
			}
		}
	}

	/// <summary>
	/// 转换为CSV
	/// </summary>
	public void ConvertToCSV(string CSVPath, Encoding encoding)
	{
		//判断Excel文件中是否存在数据表
		if (mResultSet.Tables.Count < 1)
			return;

		//默认读取第一个数据表
		DataTable mSheet = mResultSet.Tables[0];

		//判断数据表内是否存在数据
		if (mSheet.Rows.Count < 1)
			return;

		//读取数据表行数和列数
		int rowCount = mSheet.Rows.Count;
		int colCount = mSheet.Columns.Count;

		//创建一个StringBuilder存储数据
		StringBuilder stringBuilder = new StringBuilder();

		//读取数据
		for (int i = 0; i < rowCount; i++)
		{
			for (int j = 0; j < colCount; j++)
			{
				//使用","分割每一个数值
				stringBuilder.Append(mSheet.Rows[i][j] + ",");
			}
			//使用换行符分割每一行
			stringBuilder.Append("\r\n");
		}

		//写入文件
		using (FileStream fileStream = new FileStream(CSVPath, FileMode.Create, FileAccess.Write))
		{
			using (TextWriter textWriter = new StreamWriter(fileStream, encoding))
			{
				textWriter.Write(stringBuilder.ToString());
			}
		}
	}

	List<string> mList = new List<string>();
	List<string> sList = new List<string>();
	public void ReadXmlName()
	{
		XmlDocument xmlDoc = new XmlDocument();
		xmlDoc.Load(xmlPath);
		int rootlist = xmlDoc.GetElementsByTagName("root")[0].ChildNodes.Count;

		XmlNodeList nodeList = xmlDoc.SelectSingleNode("root").ChildNodes;

		for (int k = 0; k < rootlist; k++)
		{
			mList.Add(xmlDoc.GetElementsByTagName("root")[0].ChildNodes[k].Name);
		}

		foreach (XmlElement xe in nodeList)
		{

			foreach (XmlElement x1 in xe.ChildNodes)
			{
				sList.Add(x1.InnerText);
			}
		}
	}

	/// <summary>
	/// 导出为Xml
	/// </summary>
	public void ConvertToXml(string XmlFile)
	{
		ReadXmlName();
		FileInfo info = new FileInfo(XmlFile);
		string xname = info.Name.Replace(".xlsx", "");

		//string Jname = XmlFile.Replace("C:/Users/dell/Desktop/New Unity Project/Assets/Excel/", "");
		//Debug.Log(Jname);
		//判断Excel文件中是否存在数据表
		if (mResultSet.Tables.Count < 1)
			return;

		//默认读取第一个数据表
		DataTable mSheet = mResultSet.Tables[0];
		string Zz = @"[\u4e00-\u9fa5]";
		//判断数据表内是否存在数据
		if (mSheet.Rows.Count < 1)
			return;

		//读取数据表行数和列数
		int rowCount = mSheet.Rows.Count;
		int colCount = mSheet.Columns.Count;

		//创建一个StringBuilder存储数据
		StringBuilder stringBuilder = new StringBuilder();
		//创建Xml文件头
		stringBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>");
		stringBuilder.Append("\r\n");
		//创建根节点
		stringBuilder.Append("<root xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">");
		stringBuilder.Append("\r\n");
		//读取数据
		for (int i = 1; i < rowCount; i++)
		{
			//创建子节点
			for (int a = 0; a < mList.Count; a++)
			{
				if (xname == mList[a])
				{
					stringBuilder.Append("\t<" + sList[a] + ">");
					stringBuilder.Append("\r\n");
				}
			}

			for (int j = 0; j < colCount; j++)
			{
				if (System.Text.RegularExpressions.Regex.IsMatch(mSheet.Rows[0][j].ToString(), Zz) || string.IsNullOrEmpty(mSheet.Rows[0][j].ToString()))
				{
					continue;
				}

				else
				{
					if (string.IsNullOrEmpty(mSheet.Rows[i][j].ToString()) && xname.Contains("Name"))
					{
						stringBuilder.Append("<" + mSheet.Rows[0][j].ToString() + "/>");
						stringBuilder.Append("\r\n");
						continue;
					}
					else if (string.IsNullOrEmpty(mSheet.Rows[i][j].ToString()))
					{
						continue;
					}
					else
					{
						stringBuilder.Append("\t\t<" + mSheet.Rows[0][j].ToString() + ">");
						stringBuilder.Append(mSheet.Rows[i][j].ToString());
						stringBuilder.Append("</" + mSheet.Rows[0][j].ToString() + ">");
						stringBuilder.Append("\r\n");
					}
				}
			}
			//使用换行符分割每一行
			for (int a = 0; a < mList.Count; a++)
			{
				if (xname == mList[a])
				{
					stringBuilder.Append("\t</" + sList[a] + ">");
					stringBuilder.Append("\r\n");
				}
			}
		}
		//闭合标签
		stringBuilder.Append("</root>");
		//写入文件
		using (FileStream fileStream = new FileStream(XmlFile, FileMode.Create, FileAccess.Write))
		{
			using (TextWriter textWriter = new StreamWriter(fileStream, Encoding.GetEncoding("utf-8")))
			{
				textWriter.Write(stringBuilder.ToString());
			}
		}
	}

	/// <summary>
	/// 设置目标实例的属性
	/// </summary>
	private void SetTargetProperty(object target, string propertyName, object propertyValue)
	{
		//获取类型
		Type mType = target.GetType();
		//获取属性集合
		PropertyInfo[] mPropertys = mType.GetProperties();
		foreach (PropertyInfo property in mPropertys)
		{
			if (property.Name == propertyName)
			{
				property.SetValue(target, Convert.ChangeType(propertyValue, property.PropertyType), null);
			}
		}
	}
}

// 序列化过程把默认的double变为Int
class DecimalJsonConverter : JsonConverter
{
	public DecimalJsonConverter() { }

	public override bool CanRead
	{
		get { return false; }
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
	}

	public override bool CanConvert(Type objectType)
	{
		return (objectType == typeof(decimal) || objectType == typeof(float) || objectType == typeof(double));
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (DecimalJsonConverter.IsWholeValue(value))
		{
			writer.WriteRawValue(JsonConvert.ToString(Convert.ToInt64(value)));
		}
		else
		{
			writer.WriteRawValue(JsonConvert.ToString(value));
		}
	}

	private static bool IsWholeValue(object value)
	{
		if (value is decimal decimalValue)
		{
			int precision = (Decimal.GetBits(decimalValue)[3] >> 16) & 0x000000FF;
			return precision == 0;
		}
		else if (value is float floatValue)
		{
			return floatValue == Math.Truncate(floatValue);
		}
		else if (value is double doubleValue)
		{
			return doubleValue == Math.Truncate(doubleValue);
		}

		return false;
	}
}