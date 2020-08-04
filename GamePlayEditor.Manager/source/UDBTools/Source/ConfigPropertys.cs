using System;
using System.Collections.Generic;

namespace GamePlayEditor.UDBTool
{
    enum EExportFileType
    {
        UnicodeText,
        CSV,
    }

    interface IConfigPropertys 
    {
        Dictionary<string, string> GetPropertys();
    }

    struct ConfigPropertys : IConfigPropertys
    {

        static string ApplicationPath = UnityEngine.Application.dataPath;
        /// <summary>
        /// 输入的路径
        /// </summary>
        public string InputPath { get; set; }
        /// <summary>
        /// 输出数据的路径
        /// </summary>
        public string OutputDataPath { get; set; }
        /// <summary>
        /// 输出代码的路径
        /// </summary>
        public string OutputCodePath { get; set; }

        /// <summary>
        /// 在磁盘上没有被访问文件夹时是否自动创建
        /// </summary>
        public bool IsCreateFolder { get; set; }
        /// <summary>
        /// 写入时是否跳过空行
        /// </summary>
        public bool IsRemoveEmptyLine { get; set; }
        /// <summary>
        /// 指定写入的文件集合，如果没有则表示全部写入
        /// </summary>
        public string ContainerFile { get; set; }
        /// <summary>
        /// 指定写入的表格集合，如果没有则表示全部写入
        /// </summary>
        public string ContainerSheet { get; set; }
        /// <summary>
        /// 是否忽略文件名称大小写
        /// </summary>
        public bool IsIgnoreCaseFile { get; set; }
        /// <summary>
        /// 是否忽略表格名称大小写
        /// </summary>
        public bool IsIgnoreCaseSheet { get; set; }

        /// <summary>
        /// 是否导出代码
        /// </summary>
        public bool IsOutputCode { get; set; }
        
        /// <summary>
        /// 导出的代码文件全称
        /// </summary>
        public string OutputCodeFileName { get; set; }

        /// <summary>
        /// 代码应用命名空间
        /// </summary>
        public string ContainerNamespace { get; set; }

        /// <summary>
        /// 是否包含UDB公开访问数据集的变量
        /// </summary>
        public bool IsDefineUDBDatasetProperty { get; set; }

        /// <summary>
        /// 字段描述的行数
        /// </summary>
        public int DescriptionRow { get; set; }

        /// <summary>
        /// 字段类型行
        /// </summary>
        public int PropertyTypeRow { get; set; }
        
        /// <summary>
        /// 字段名称行
        /// </summary>
        public int PropertyNameRow { get; set; }

        /// <summary>
        /// 数据导出起始行
        /// </summary>
        public int StartExportRow { get; set; }

        public EExportFileType ExportFileType;

        ///// <summary>
        ///// 扩展名
        ///// </summary>
        //public string ExportFileStyle { get; set; }

        ///// <summary>
        ///// 分开的符号
        ///// </summary>
        //public string SplitSign { get; private set; }

        public ConfigPropertys ( Config _config)
        {
            InputPath = _config.GetStrConfig ( "InputPath", "GamePlay/UDB/Examples/In" );
            OutputDataPath = _config.GetStrConfig ( "OutputDataPath", "GamePlay/UDB/OutData" );
            IsOutputCode = _config.GetBoolConfig ( "IsOutputCode" );
            IsDefineUDBDatasetProperty = _config.GetBoolConfig ( "UDBDataSet" );
            OutputCodePath = _config.GetStrConfig ( "OutputCodePath", "GamePlay/UDB/Examples/OutCode");
            OutputCodeFileName = _config.GetStrConfig ( "OutputCodeFileName", "GameTable.cs" );
            ContainerNamespace = _config.GetStrConfig ("ContainerNamespace","System;");
            //ContainerNamespace = value.Replace ( ";", ";\r\n" );
            DescriptionRow = _config.GetIntConfig ( "DescriptionRow", -1 );
            PropertyTypeRow = _config.GetIntConfig ( "PropertyTypeRow", 1 );
            PropertyNameRow = _config.GetIntConfig ( "PropertyNameRow", 2 );
            StartExportRow = _config.GetIntConfig ( "StartExportRow", IsOutputCode ? 2 : 1 );
            IsCreateFolder = _config.GetBoolConfig ( "IsCreateFolder" );
            IsRemoveEmptyLine = _config.GetBoolConfig ( "IsRemEmptyLine" );
            ContainerFile = _config.GetStrConfig ( "ContainerFile","*" );
            ContainerSheet = _config.GetStrConfig ( "ContainerSheet","*" );
            IsIgnoreCaseFile = _config.GetBoolConfig ( "IsIgnoreCaseFile" );
            IsIgnoreCaseSheet = _config.GetBoolConfig ( "IsIgnoreCaseSheet" );
            
            var value = _config.GetStrConfig ( "ExportFileType", "UnicodeText" );

            try
            {
                ExportFileType = (EExportFileType) Enum.Parse ( typeof ( EExportFileType ), value );
            }
            catch
            {
                ExportFileType = EExportFileType.UnicodeText;
            }

            //if ( IsIgnoreCaseFile )
            //{
            //    for ( var i = 0 ; i < ContainerFile.Count ; i++ )
            //    {
            //        ContainerFile[i] = ContainerFile[i].ToLower ();
            //    }
            //}

            //if ( IsIgnoreCaseSheet )
            //{
            //    for ( var i = 0 ; i < ContainerSheet.Count ; i++ )
            //    {
            //        ContainerSheet[i] = ContainerSheet[i].ToLower ();
            //    }
            //}
        }

        List<string> ValueToStrArray ( string _value )
        { 
            List<string> containerNames = new List<string>();
              
            if ( _value != "*" && string.IsNullOrEmpty(_value) == false)
            {
                var result = _value.Split (new char[] {','} );
                containerNames.AddRange ( result );
                containerNames.RemoveAll ( ( v ) => { return string.IsNullOrEmpty ( v ); } );
            } 
            return containerNames;
        }

        internal string GetCodeNamespace ()
        {
           return ContainerNamespace.Replace ( ";", ";\r\n" );
        }

        internal string GetAbsUrl ( string _checkValue )
        {  
            if ( _checkValue.Contains ( ":\\" ) || _checkValue.Contains(":/") )
            {
                return _checkValue;
            }
            else
            {
                return ApplicationPath + "/" + _checkValue;
            }
        }

        internal string GetInUrl ()
        {
            return GetAbsUrl (InputPath);
        }

        internal string GetOutputDataPath ()
        {
            return GetAbsUrl (OutputDataPath);
        }

        internal string GetOutputCodePath ()
        {
            return GetAbsUrl ( OutputCodePath );
        }

        internal List<string> GetContainerFiles ()
        {
            return ValueToStrArray (ContainerFile);
        }

        internal List<string> GetContainerSheets ()
        {
            return ValueToStrArray ( ContainerSheet );
        }

        internal string GetExportFileExtensionName ()
        {
            if ( ExportFileType == EExportFileType.CSV )
            {
                return "csv";
            }
            else
            {
                return "txt";
            }
        }

        internal string GetExportFileSplitSign ()
        {
            if ( ExportFileType == EExportFileType.CSV )
            {
                return ",";
            }
            else
            {
                return "\t";
            }
        }

        public  Dictionary<string, string> GetPropertys ()
        {
            Dictionary<string,string> map = new Dictionary<string, string>();
            map.Add ( "InputPath",      InputPath );
            map.Add ( "OutputDataPath", OutputDataPath );
            map.Add ( "IsRemEmptyLine", IsRemoveEmptyLine.ToString() );
            map.Add ( "ExportFileType", ExportFileType.ToString () );
            map.Add ( "IsIgnoreCaseFile", IsIgnoreCaseFile.ToString () );
            map.Add ( "IsIgnoreCaseSheet", IsIgnoreCaseSheet.ToString () );
            map.Add ( "IsCreateFolder",IsCreateFolder.ToString() );
            map.Add ( "ContainerFile", ContainerFile );
            map.Add ( "ContainerSheet", ContainerSheet );
            //> 代码输出选项
            map.Add ( "IsOutputCode", IsOutputCode.ToString () );
            map.Add ( "OutputCodePath", OutputCodePath );
            map.Add ( "DescriptionRow", DescriptionRow.ToString () );
            map.Add ( "PropertyTypeRow", PropertyTypeRow.ToString () );
            map.Add ( "PropertyNameRow", PropertyNameRow.ToString () );
            map.Add ( "StartExportRow", StartExportRow.ToString () );
            map.Add ( "ContainerNamespace", ContainerNamespace );
            map.Add ( "OutputCodeFileName", OutputCodeFileName );
            map.Add ( "UDBDataSet", IsDefineUDBDatasetProperty.ToString() );

            return map;
        } 
    }
}


