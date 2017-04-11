using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLibrary.Helpers;

namespace ClientLibrary.Controls
{
    public static class Constants
    {
        public static string FACE_SUBSCRIPTION_KEY = "d36061dd605f4bccbca339bacaff1cdb";
        public static string TMP_IMAGE_DIR = @"d:\_Data_\Bram\_Projecten_\MSFaceCognitiveServiceTest\_TMPpics_\";
        public static string PERSONS_IMAGE_DIR = @"d:\_Data_\Bram\_Projecten_\MSFaceCognitiveServiceTest\_PersonPics_\";
        public static string DB_FILE = @"d:\_Data_\Bram\_Projecten_\MSFaceCognitiveServiceTest\_DB_\MSCognitive.db";

        //DB stuff
        public static string DB_PERSON_TABLE_NAME= "Person";
        public static string DB_PERSON_PERSON_ID_FIELDNAME = "person_id";
        public static string DB_PERSON_PERSON_NAME_FIELDNAME = "name";
        public static string DB_PERSON_GROUP_ID_FIELDNAME = "group_id";

        public static string DB_GROUP_TABLE_NAME = "Group";
        public static string DB_GROUP_GROUP_ID_FIELDNAME = "group_id";
        public static string DB_GROUP_GROUP_NAME_FIELDNAME = "name";

        public static string DB_LIST_TABLE_NAME = "FaceList";
        public static string DB_LIST_ID_FIELDNAME= "list_id";
        public static string DB_LIST_NAME_FIELDNAME= "name";

        public static string DB_FACE_TABLE_NAME = "Face";
        public static string DB_FACE_ID_FIELDNAME= "persistedFaceId";
        public static string DB_FACE_FACE_LIST_ID_FIELDNAME= "facelist_id";
        public static string DB_FACE_IMAGENAME_FIELDNAME= "image_name";
        public static string DB_FACE_PERSON_ID_FIELDNAME= "person_id";

        public static void LoadSettings()
        {
            string settingsFile = Path.Combine(Directory.GetCurrentDirectory(), "settings.ini");
            IniFile iniSettingsFile = new IniFile(settingsFile);

            if (iniSettingsFile.KeyExists("FACE_SUBSCRIPTION_KEY")) { FACE_SUBSCRIPTION_KEY = iniSettingsFile.Read("FACE_SUBSCRIPTION_KEY"); }
            else { iniSettingsFile.Write("FACE_SUBSCRIPTION_KEY", FACE_SUBSCRIPTION_KEY); }

            if (iniSettingsFile.KeyExists("TMP_IMAGE_DIR")) { TMP_IMAGE_DIR = iniSettingsFile.Read("TMP_IMAGE_DIR"); }
            else { iniSettingsFile.Write("TMP_IMAGE_DIR", TMP_IMAGE_DIR); }

            if (iniSettingsFile.KeyExists("PERSONS_IMAGE_DIR")) { PERSONS_IMAGE_DIR = iniSettingsFile.Read("PERSONS_IMAGE_DIR"); }
            else { iniSettingsFile.Write("PERSONS_IMAGE_DIR", PERSONS_IMAGE_DIR); }

            if (iniSettingsFile.KeyExists("DB_FILE")) { DB_FILE = iniSettingsFile.Read("DB_FILE"); }
            else { iniSettingsFile.Write("DB_FILE", DB_FILE); }
        }
    }
}
