using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GamePlayEditor.UDBTool
{
    public class PropertyInfo
    {
        public string TypeName { get; private set; }
        public string PropertyName { get; private set; }
        public string Description { get; private set; }

        public void SetType ( string _typeName ) { this.TypeName = _typeName; }
        public void SetProperty ( string _propertyName ) { this.PropertyName = _propertyName; } 
        public void SetDescription ( string _description ) { this.Description = _description; }
    }

    public class ClassInfo
    {
        public string ClassName { get; private set; }
        public List<PropertyInfo> Propertys = new List<PropertyInfo>();
        public ClassInfo ( string _classInfo )
        {
            ClassName = _classInfo;
        }
    }

    public class CollectData
    {
        public List<ClassInfo> ClassInfos = new List<ClassInfo>();
        public Dictionary<string,string> TableContent = new Dictionary<string, string>();

        public CollectData ( List<ClassInfo> _classInfo, Dictionary<string, string> _tabContent )
        {
            ClassInfos = _classInfo;
            TableContent = _tabContent;
        }
    }

    class RuntimeWoker : Debug
    {
        /// <summary>
        /// 路径检查，支持这个路径返回true，否则返回false
        /// </summary>
        /// <param name="_url">绝对路径</param>
        /// <param name="_autoCreate">是否自动创建</param>
        /// <returns></returns>
        internal bool UrlChecker (string _url,bool _autoCreate)
        {
            if ( !Directory.Exists ( _url ) )
            {
                if ( _autoCreate )
                {
                    Warning ( "Auto create path：{0}", _url );
                    Directory.CreateDirectory ( _url );
                    return true;
                }
                else
                {
                    Error ( "Path does not exist：{0}", _url );
                    return false;
                }
            }
            else return true;
        }

        /// <summary>
        /// 获得所有_inurl路径下所有的xlsx文件的绝对路径
        /// </summary>
        /// <param name="_inUrl"></param>
        /// <param name="_containerFile"></param>
        /// <param name="_isIgnoreCaseFile"></param>
        /// <param name="_reuslt"></param>
        internal void GetUrlOfXlsxFiles ( string _inUrl, List<string> _containerFile, bool _isIgnoreCaseFile, ref List<string> _reuslt )
        {
            var subDirs = Directory.GetDirectories (_inUrl);

            var files = Directory.GetFiles (_inUrl);

            foreach ( var v in files )
            {
                var extension = Path.GetExtension ( v );
                if ( extension == ".xlsx" )
                {
                    bool isExport = true;
                    if ( _containerFile.Count > 0 )
                    {
                        var fileName = Path.GetFileNameWithoutExtension ( v );

                        if ( _isIgnoreCaseFile ) fileName = fileName.ToLower ();

                        if ( !_containerFile.Contains ( fileName ) )
                        {
                            isExport = false;
                        }
                    }
                    if ( isExport )
                    {
                        Msg ( "Export File: " + Path.GetFileName ( v ) );
                        _reuslt.Add ( v );
                    }
                }
            }

            foreach ( var v in subDirs )
            {
                GetUrlOfXlsxFiles ( v, _containerFile, _isIgnoreCaseFile, ref _reuslt );
            }
        }

        class ThreadTask 
        {
            public enum Status 
            {
                Waiting,
                Working,
                Done
            }
            public ISheet Sheet { private set; get; }
            public ClassInfo Info { private set; get; }
            public string Result = "";
            public int Begin { private set; get; }
            public int End { private set; get; }
            public int CompletedCount = 0;
            public Status State = Status.Waiting;
            public string Sign { private set; get; }
            public bool IsRemoveEmptyLine { private set; get; }
            public int StartRow { private set; get; }
            public int PropertyTypeRow { private set; get; }
            public int PropertyNameRow { private set; get; }
            public int DescriptionRow { private set; get; }
            public int MaxCell { private set; get; }

            public CancellationTokenSource CancelController { get; private set; }

            public ThreadTask(ISheet _sheet, ClassInfo _info, int _maxCell, int _begin, int _end, string _sign, int _startRow, int _propertyTypeRow, int _propertyNameRow, int _descriptionRow,CancellationTokenSource _cancelController) 
            {
                this.Sheet = _sheet;
                this.Info = _info;
                this.MaxCell = _maxCell;
                this.Begin = _begin;
                this.End = _end;
                this.Sign = _sign;
                this.StartRow = _startRow;
                this.PropertyTypeRow = _propertyTypeRow;
                this.PropertyNameRow = _propertyNameRow;
                this.DescriptionRow = _descriptionRow;
                this.CancelController = _cancelController;
            }
        }

        /// <summary>
        /// 线程任务，用来处理部分内容
        /// </summary>
        /// <param name="_info"></param>
        void ThreadTaskExportContentPart(object _info) 
        {
            ThreadTask info = _info as ThreadTask;
            CancellationToken token = info.CancelController.Token;
            bool isPropertyType = false,
                 isPropertyName = false,
                 isDescriptions = false;
            
            for (int r = info.Begin; r < info.End; r++)
            {
                IRow row = info.Sheet.GetRow(r);
                if (null == row) continue;
                if (token.IsCancellationRequested) break;

                if(r< info.PropertyTypeRow) isPropertyType = r + 1 == info.PropertyTypeRow;
                if(r< info.PropertyNameRow) isPropertyName = r + 1 == info.PropertyNameRow;
                if(r< info.DescriptionRow) isDescriptions = r + 1 == info.DescriptionRow;

                string line = ""; string cellValue = "";
                bool isEmptyLine = true; 

                for (int c = 0; c < info.MaxCell; c++)
                {
                    ICell cell = row.GetCell(c);

                    if (cell == null)
                    {
                        cellValue = "";
                    }
                    else
                    {
                        cellValue = cell.ToString();
                    }

                    //> 忽略空行
                    if (isEmptyLine && string.IsNullOrEmpty(cellValue) == false)
                    {
                        isEmptyLine = false;
                    }

                    if (c == 0)
                    {
                        line = cellValue;
                    }
                    else
                    {
                        line += info.Sign + cellValue;
                    }

                    if (null == info.Info)
                        continue; //> 跳过类型、属性名称写入

                    if (isPropertyType)
                    {
                        info.Info.Propertys[c].SetType(cellValue);
                    }
                    else if (isPropertyName)
                    {
                        info.Info.Propertys[c].SetProperty(cellValue);
                    }
                    else if (isDescriptions)
                    {
                        info.Info.Propertys[c].SetDescription(cellValue);
                    }
                }

                if ((r + 1) < info.StartRow || isDescriptions)
                {
                    continue;
                }

                if (!info.IsRemoveEmptyLine || !isEmptyLine)
                {
                    line += "\r\n";
                    info.Result += line; 
                }

                info.CompletedCount = r-info.Begin;
            }

            info.State = ThreadTask.Status.Done; 
        }

        /// <summary>
        /// 导出文件,这里只导出数据文件
        /// </summary>
        /// <param name="_url">导出文件的绝对路径</param>
        /// <param name="_outFolder">导出的目录路径</param>
        /// <param name="_isIgnoreCaseSheet">是否忽略表格名称的大小写</param>
        /// <param name="_isRemoveEmptyLine">是否移除空行数据</param>
        /// <param name="_containerList">包含指定导出单位的列表</param>
        /// <param name="_startRow">开始导出的数据行号</param>
        /// <param name="_splitSign">导出数据列使用的分隔符</param>
        /// <param name="_extensionName">导出文件的扩展名</param>
        /// <param name="_isOutputCode">是否导出代码</param>
        /// <param name="_propertyTypeRow">导出字段类型的行号</param>
        /// <param name="_propertyNameRow">导出字段名称的行号</param>
        /// <param name="_descriptionRow">输出的类数据集合</param>
        internal CollectData ExportFile 
            ( 
            string _url,string _outFolder, bool _isIgnoreCaseSheet, 
            bool _isRemoveEmptyLine,List<string> _containerList, 
            int _startRow , string _splitSign,string _extensionName,
            bool _isOutputCode,int _propertyTypeRow, 
            int _propertyNameRow,int _descriptionRow,CancellationTokenSource _cancelController = null,
            Action<int,int,string,int,int> _callback = null
            )
        {
            CancellationToken token = _cancelController.Token;
            var xlsName     = Path.GetFileName ( _url ); 
            FileStream fs   = new FileStream(_url,FileMode.Open); 
            IWorkbook wb    = WorkbookFactory.Create (fs); 
            fs.Dispose();
            fs.Close();
            fs = null;

            int sheetCount  = wb.NumberOfSheets;
            var outClassDataset = new List<ClassInfo> ();
            var outTableContent = new Dictionary<string,string>();
            ClassInfo classInfo = null;
             
            for ( int sheetIndex = 0 ; sheetIndex < sheetCount ; sheetIndex++ )
            {
                ISheet sheet  = wb.GetSheetAt(sheetIndex);
                string sheetName = sheet.SheetName; 
               
                if ( _isIgnoreCaseSheet )
                {
                    sheetName = sheetName.ToLower ();
                }

                if ( null == sheet || _containerList.Count > 0 && _containerList.Contains ( sheetName ) == false ) continue; 

                if ( sheet.PhysicalNumberOfRows <= _startRow )
                {
                    Error ( "{0}.{1} 数据格式不正确,读取失败!", xlsName, sheetName );
                }
                else
                {
                    Log ( "Begin Reading: {0}.{1}", xlsName, sheetName );
                }
                
                _startRow = _startRow <= 0 ? 1 : _startRow;
               
                string tabContent = "";
                string tabName = string.Format("{0}.{1}",sheetName, _extensionName);
                int maxCells = sheet.GetRow(0).LastCellNum;

                if ( _isOutputCode )
                {
                    classInfo = new ClassInfo ( sheet.SheetName );
                    for ( int i = 0 ; i < maxCells ; i++ )
                    {
                        classInfo.Propertys.Add ( new PropertyInfo () );
                    }
                }

                int lastRowNum = sheet.LastRowNum + 1;
                //> 每个线程最多负责20000行数据解析
                int rate = 20000;
                //> 最多5个线程同时执行
                int wokerCount = 5;
                int taskCount = lastRowNum / rate + ((lastRowNum % rate) > 0 ? 1 : 0);
                
                if (taskCount > 0)
                {
                    //> 采用多线程处理
                    List<ThreadTask> tasks = new List<ThreadTask>();
                    for (int t = 0; t < taskCount; t++) 
                    {
                        int end = t * rate + rate;
                        ThreadTask job = new ThreadTask(
                            sheet,
                            t == 0 ? classInfo : null,
                            maxCells,
                            t * rate, 
                            end,
                            _splitSign,
                            _startRow,
                            _propertyTypeRow,
                            _propertyNameRow,
                            _descriptionRow,
                            _cancelController);

                        tasks.Add(job);
                    }

                    bool doing = true;
                    bool completed = true;
                    int woker = 0;
                    int completedTotal = 0;
                    ThreadTask lastWating = null;
                    ThreadTask current = null;
                    while (doing) 
                    { 
                        //> 完成检查
                        completed = true;
                        completedTotal = 0;
                        lastWating = null;
                        woker = 0;
                        for (int i = 0; i < tasks.Count; i++) 
                        {
                            current = tasks[i];
                            if (current.State != ThreadTask.Status.Done) 
                            {
                                completed = false;
                                if (current.State == ThreadTask.Status.Working)
                                {
                                    woker++;
                                }
                                else if(lastWating == null)
                                {
                                    lastWating = tasks[i];
                                }
                            }
                            completedTotal += current.CompletedCount;
                        }

                        _callback(sheetIndex, sheetCount, sheetName, completedTotal, lastRowNum);

                        //> 同时工作量检查
                        if (woker < wokerCount && lastWating != null) 
                        { 
                            lastWating.State = ThreadTask.Status.Working;
                            Task wokerThread = new Task(ThreadTaskExportContentPart,lastWating);
                            wokerThread.Start();
                        }
                        doing = !completed;
                    }

                    foreach (var v in tasks) 
                    {
                        tabContent += v.Result;
                    }
                }
                else 
                {

                    bool isPropertyType = false,
                         isPropertyName = false,
                         isDescriptions = false;
                    for (int r = 0; r <= lastRowNum; r++)
                    {
                        IRow row = sheet.GetRow(r);
                        if (null == row) continue; 
                        if (r % 200 == 0 && _callback != null) _callback(sheetIndex, sheetCount, sheetName, r, lastRowNum);

                        isPropertyType = r + 1 == _propertyTypeRow;
                        isPropertyName = r + 1 == _propertyNameRow;
                        isDescriptions = r + 1 == _descriptionRow;
                        string line = ""; string cellValue = "";
                        bool isEmptyLine = true;

                        for (int c = 0; c < maxCells; c++)
                        {
                            if (token.IsCancellationRequested) return null;
                            ICell cell = row.GetCell(c);

                            if (cell == null)
                            {
                                cellValue = "";
                            }
                            else
                            {
                                cellValue = cell.ToString();
                            }

                            //> 忽略空行
                            if (isEmptyLine && string.IsNullOrEmpty(cellValue) == false)
                            {
                                isEmptyLine = false;
                            }

                            if (c == 0)
                            {
                                line = cellValue;
                            }
                            else
                            {
                                line += _splitSign + cellValue;
                            }

                            if (null == classInfo || !_isOutputCode)
                                continue; //> 跳过类型、属性名称写入

                            if (isPropertyType)
                            {
                                classInfo.Propertys[c].SetType(cellValue);
                            }
                            else if (isPropertyName)
                            {
                                classInfo.Propertys[c].SetProperty(cellValue);
                            }
                            else if (isDescriptions)
                            {
                                classInfo.Propertys[c].SetDescription(cellValue);
                            }
                        }

                        if ((r + 1) < _startRow || isDescriptions)
                        {
                            continue;
                        }

                        if (!_isRemoveEmptyLine || !isEmptyLine)
                        {
                            line += "\r\n";
                            tabContent += line;
                        }
                    }
                }


                if ( outTableContent.ContainsKey ( tabName ) )
                {
                    outTableContent[tabName] = tabContent;
                }
                else
                {
                    outTableContent.Add (tabName,tabContent);
                }

                if ( !_isOutputCode ) continue;

                //> 将代码结构加入数据集
                outClassDataset.Add ( classInfo );
            }
            
            return new CollectData(outClassDataset,outTableContent);
        }

        class TypeInfo
        {
            public Type Type { get; private set; }
            public string CodeID { get; private set; }
            public TypeInfo ( Type _type, string _codeID )
            {
                this.Type = _type;
                this.CodeID = _codeID;
            }
        }
        internal void ExportCodeFile ( string _outCodeUrl,string _outFileName, string _usings, bool _isDefineUDBDatasetPorperty,List<ClassInfo> dataset )
        {
            List<TypeInfo> baseType = new List<TypeInfo>
            {
                new TypeInfo(typeof(bool),"bool"),
                new TypeInfo(typeof(short),"short"),
                new TypeInfo(typeof(int),"int"),
                new TypeInfo(typeof(long),"long"),
                new TypeInfo(typeof(float),"float"),
                new TypeInfo(typeof(double),"double"),
                new TypeInfo(typeof(string),"string")
            };
            string content = GetFileHead(_outFileName);

            content += "\r\n";

            var usingline = _usings.Split (new string[] { "\r\n" },StringSplitOptions.None);

            foreach ( var v in usingline )
            {
                if ( string.IsNullOrEmpty ( v ) ) continue;

                content += "using " + v + "\r\n";
            }

            content += "\r\n\r\n";
            foreach ( var v in dataset )
            {
                content += FormatClassInfo ( baseType, v , _isDefineUDBDatasetPorperty );
            }
            string path = string.Format("{0}\\{1}", _outCodeUrl, _outFileName);
            File.WriteAllText(path,content);
        }

        string FormatClassInfo (List<TypeInfo> _baseTypes,ClassInfo _info,bool _isDefineUDBDatasetPorperty )
        {
            string content = "";

            content += string.Format("public class {0}\r\n",_info.ClassName);
            content += "{\r\n";

            foreach ( var v in _info.Propertys )
            {
                var type = _baseTypes.Find ( ( t ) => { return t.Type.Name.ToLower() == v.TypeName.ToLower(); } );
                var typeName = v.TypeName;

                if ( type != null )
                {
                    typeName = type.CodeID;
                }
                if ( !string.IsNullOrEmpty ( v.Description ) )
                {
                    content += GetDescriptionFormat (v.Description);
                }

                content += string.Format ( "\tpublic {0} {1};\r\n", typeName, v.PropertyName.Replace ( " ", "_" ) );
            }
            if ( _isDefineUDBDatasetPorperty )
            {
                content += GetDescriptionFormat ( "通过UDBTool解析的数据会存储在这里." );
                content += string.Format ("\tstatic public {0}[] UDBDataSet;\r\n",_info.ClassName);
            }
            content += "}\r\n\r\n";
            return content;
        }

        string GetFileHead (string _fileName)
        {
            string fileHead = "";
            fileHead += "/******************************************************************\r\n";
            fileHead += string.Format(" * FileName: {0} \r\n", _fileName );
            fileHead += string.Format(" * Time: {0} \r\n",System.DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
            fileHead += " ******************************************************************\r\n";
            fileHead += " * Warning: This file came from the build of UDBTool.\r\n";
            fileHead += " *          Don't modify it by yourself.\r\n";
            fileHead += " *          Use UDBTool to create a new file when you want it.\r\n";
            //            fileHead += " * BUG Submit：550001112@qq.com\r\n";
            fileHead += " ******************************************************************/\r\n\r\n";
            return fileHead;
        }

        string GetDescriptionFormat ( string _description )
        {
            return string.Format( "\t/// <summary>\r\n\t/// {0}\r\n\t/// </summary>\r\n", _description);
        }
    }
}


