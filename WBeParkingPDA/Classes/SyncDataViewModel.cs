using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Forms;

namespace WBeParkingPDA
{
    public class SyncDataViewModel
    {
        static log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SyncDataViewModel));

        public SyncDataViewModel()
        {
            etcbinding = new List<ETCBinding>();
            carpurposetypes = new List<CarPurposeTypes>();
            _settings = new Dictionary<string, object>();
        }

        private IList<ETCBinding> etcbinding;

        public virtual IList<ETCBinding> ETCBinding { get { return etcbinding; } set { etcbinding = value; } }

        private IList<CarPurposeTypes> carpurposetypes;
        public virtual IList<CarPurposeTypes> CarPurposeTypes { get { return carpurposetypes; } set { carpurposetypes = value; } }

        private IDictionary<string, object> _settings;

        public IDictionary<string, object> AppSettings { get { return _settings; } set { _settings = value; } }

        public void GetCarPurposeTypesList(ComboBox ddl)
        {

            try
            {

                if (carpurposetypes != null &&
                    carpurposetypes.Count > 0)
                {
                    ddl.Items.Clear();
                    ddl.DisplayMember = "Name";
                    ddl.ValueMember = "Id";

                    foreach (CarPurposeTypes row in carpurposetypes.Where(w => w.Void == false).ToList())
                    {
                        ddl.Items.Add(row);
                    }

                    ddl.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }

        }

        public bool IsETCExists(string EPCID, out string OCarId, out int OPropseId)
        {

            try
            {
                OCarId = string.Empty;
                OPropseId = 0;

                if (etcbinding != null & etcbinding.Count > 0)
                {
                    ETCBinding data = etcbinding.FirstOrDefault(w => w.ETCID.Equals(EPCID, StringComparison.InvariantCultureIgnoreCase));

                    if (data != null)
                    {
                        OCarId = data.CarID;
                        OPropseId = data.CarPurposeTypeID??0;
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void SaveETCTagBinding(string EPCID, string CarId, int PropseId)
        {

            try
            {
                string tmpCarId;
                int carpid;
                if (!IsETCExists(EPCID, out tmpCarId, out carpid))
                {
                    etcbinding.Add(new ETCBinding() { CarID = CarId, CarPurposeTypeID = PropseId, ETCID = EPCID, CreateTime = DateTime.Now });
                }
                else
                {
                    ETCBinding existdata = etcbinding.First(w => w.ETCID.Equals(EPCID, StringComparison.InvariantCultureIgnoreCase));

                    existdata.LastUpdateTiem = DateTime.Now;
                    existdata.LastUploadTime = null;
                    existdata.CarID = CarId;
                    existdata.CarPurposeTypeID = PropseId;

                    etcbinding.Remove(existdata);
                    etcbinding.Add(existdata);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw;
            }
        }

        public SyncDataViewModel CloneToUploadSync()
        {
            SyncDataViewModel cloneobject = new SyncDataViewModel();

            foreach (ETCBinding item in etcbinding.Where(w=>w.LastUploadTime==null).ToList())
            {
                item.LastUploadTime = DateTime.Now;
                cloneobject.ETCBinding.Add(item);
            }

            cloneobject.CarPurposeTypes = carpurposetypes;

            return cloneobject;
        }

        public static SyncDataViewModel LoadFile(string path)
        {
            SyncDataViewModel model = null;

            try
            {

                    FileStream jsonFile = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    StreamReader jsonReader = new StreamReader(jsonFile, Encoding.UTF8);
                    string filecontent = jsonReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(filecontent))
                    {
                        model = Newtonsoft.Json.JsonConvert.DeserializeObject<SyncDataViewModel>(filecontent);
                    }
                    else
                    {
                        model = new SyncDataViewModel();
                        model.CarPurposeTypes.Add(new CarPurposeTypes() { Id = 1, Name = "自用車", Void = false });
                        model.CarPurposeTypes.Add(new CarPurposeTypes() { Id = 2, Name = "公務車", Void = false });
#if DEBUG
                        model.AppSettings.Add("RemoteHost", "http://61.216.6.217:5002");
#else
                        model.AppSettings.Add("RemoteHost", "http://10.6.10.116");
#endif
                        filecontent = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                        StreamWriter jsonwriter = new StreamWriter(jsonFile);
                        jsonwriter.WriteLine(filecontent);
                        jsonwriter.Close();
                       // jsonFile.Write(Encoding.UTF8.GetBytes(filecontent), 0, filecontent.Length);
                    }
                    jsonReader.Close();
                    jsonFile.Close();
                    return model;
       
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                return model;
            }
        }

        public static void SaveFile(string path, SyncDataViewModel database)
        {
            try
            {
                if (File.Exists(path))
                {
                    FileStream jsonFile = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    StreamWriter jsonWriter = new StreamWriter(jsonFile, Encoding.UTF8);
                    jsonWriter.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(database));
                    jsonWriter.Close();
                    jsonFile.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
        }
    }
}
