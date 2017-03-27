using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary.Controls
{
    public static class Constants
    {
        public static string FACE_SUBSCRIPTION_KEY = "d36061dd605f4bccbca339bacaff1cdb";
        public static string TMP_IMAGE_DIR = @"d:\_Data_\Bram\_Projecten_\MSFaceCognitiveServiceTest\_TMPpics_\";

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

        public static string DB_PERSON_FACE_TABLE_NAME = "Face";
        public static string DB_PERSON_FACE_ID_FIELDNAME= "persistedFaceId";
        public static string DB_PERSON_FACE_FACE_LIST_ID_FIELDNAME= "facelist_id";
        public static string DB_PERSON_FACE_IMAGENAME_FIELDNAME= "image_name";
    }
}
